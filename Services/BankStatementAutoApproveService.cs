using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Duyệt tự động Thanh toán theo Sổ phụ Ngân hàng (Bank Statement Auto-Approve / TCF).
/// Tương ứng nghiệp vụ FrmMngPM_ApproveAuto + FrmMngPM.btnAutoAppA/btnAutoAppF + LayTTSoPhu
/// + SalesService.OS_DMS_TCF_WA_Bank_BankStatementDtl_Get / PaymentPaymentApprove_Approve /
/// PaymentPaymentConfirm_MultiAndpushTCF + BizHTC.zTemp.PaymentPaymentApproveX_20210601 /
/// PaymentPaymentConfirmX_20210601 trong hệ nguồn 2010.HTC.
///
/// Nghiệp vụ: kế toán tải sổ phụ ngân hàng (Bank Statement) trong khoảng ngày tiền về, hệ thống tự động
/// so khớp từng dòng sổ phụ với phiếu thanh toán (Pmt_Payment) đang chờ duyệt (Pending) hoặc đã duyệt (Approved):
///   - Bước A (ApproveA): phiếu Pending khớp sổ phụ -> chuyển Approved, ghi nhận số chứng từ kế toán,
///     ngày tiền về, mã giao dịch TCF, cờ đối chiếu TCF.
///   - Bước F (ApproveF): phiếu Approved khớp sổ phụ -> chuyển Finished (hoàn tất), đẩy dữ liệu đối chiếu
///     sang hệ thống TCF.
/// Mỗi lần chạy tạo 1 lô (batch) kèm danh sách dòng đối chiếu để truy vết.
/// </summary>
public sealed class BankStatementAutoApproveService(AppDbContext db)
{
    /// <summary>Lấy danh sách lô duyệt tự động kèm bộ lọc (trạng thái, chế độ, ngân hàng, khoảng ngày).</summary>
    public async Task<List<BankStatementAutoApproveBatch>> GetListAsync(
        Guid orgId,
        string? status = null,
        string? mode = null,
        string? bankCode = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.AutoApproveBatches
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AutoApproveBatchStatus>(status, true, out var st))
            q = q.Where(x => x.Status == st);

        if (!string.IsNullOrWhiteSpace(mode) && Enum.TryParse<AutoApproveMode>(mode, true, out var md))
            q = q.Where(x => x.Mode == md);

        if (!string.IsNullOrWhiteSpace(bankCode))
            q = q.Where(x => x.BankCode == bankCode.ToUpper());

        if (fromDate.HasValue)
            q = q.Where(x => x.CreatedAt >= fromDate.Value.Date);

        if (toDate.HasValue)
            q = q.Where(x => x.CreatedAt < toDate.Value.Date.AddDays(1));

        return await q.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 lô duyệt tự động theo ID (kèm danh sách dòng đối chiếu).</summary>
    public async Task<BankStatementAutoApproveBatch?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.AutoApproveBatches
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>
    /// Chạy duyệt tự động thanh toán theo sổ phụ ngân hàng (tương ứng btnAutoAppA / btnAutoAppF + LayTTSoPhu).
    /// Với mỗi dòng sổ phụ: tìm phiếu thanh toán khớp (theo PaymentNo nếu có, ngược lại theo số tiền + nội dung),
    /// kiểm tra trạng thái hợp lệ theo chế độ (A: Pending, F: Approved) rồi cập nhật trạng thái phiếu.
    /// </summary>
    public async Task<BankStatementAutoApproveBatch> RunAsync(Guid orgId, RunAutoApproveDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Cần ít nhất 1 dòng sổ phụ ngân hàng để đối chiếu.");
        if (string.IsNullOrWhiteSpace(dto.BankCode))
            throw new ArgumentException("Cần chỉ định mã ngân hàng của sổ phụ (BankCode: VCB, TCB, MBB, CTG...).");
        if (dto.StatementTo < dto.StatementFrom)
            throw new ArgumentException("Ngày tiền về đến phải lớn hơn hoặc bằng ngày tiền về từ.");

        string batchNo = dto.BatchNo?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(batchNo))
        {
            batchNo = $"AUTO-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        }
        else
        {
            bool dup = await db.AutoApproveBatches.AnyAsync(x => x.OrgId == orgId && x.BatchNo == batchNo);
            if (dup)
                throw new InvalidOperationException($"Số lô duyệt tự động {batchNo} đã tồn tại.");
        }

        var now = DateTime.Now;
        var batch = new BankStatementAutoApproveBatch
        {
            OrgId = orgId,
            BatchNo = batchNo,
            Mode = dto.Mode,
            Channel = dto.Channel,
            BankCode = dto.BankCode.Trim().ToUpper(),
            AccountNo = dto.AccountNo?.Trim(),
            StatementFrom = dto.StatementFrom.Date,
            StatementTo = dto.StatementTo.Date,
            TotalRecords = dto.Items.Count,
            Status = AutoApproveBatchStatus.Draft,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "KeToan_HTC",
            CreatedAt = now,
            Remark = dto.Remark?.Trim()
        };

        int matched = 0, approved = 0, skipped = 0;
        long totalAmount = 0, matchedAmount = 0;

        foreach (var line in dto.Items)
        {
            totalAmount += line.Amount;

            var detail = new BankStatementAutoApproveDetail
            {
                OrgId = orgId,
                BankTxnNo = line.BankTxnNo?.Trim() ?? "",
                TxnTime = line.TxnTime,
                Amount = line.Amount,
                SenderAccount = line.SenderAccount?.Trim(),
                ReceiverAccount = line.ReceiverAccount?.Trim(),
                Remark = line.Remark?.Trim(),
                DealerCode = line.DealerCode?.Trim(),
                AccountingRecordNo = line.AccountingRecordNo?.Trim(),
                TcfAutoId = line.TcfAutoId?.Trim(),
                TcfBsInputNo = line.TcfBsInputNo?.Trim(),
                TcfMaGiaoDich = line.TcfMaGiaoDich?.Trim()
            };

            // Tìm phiếu thanh toán khớp: ưu tiên theo PaymentNo, ngược lại theo số tiền + nội dung chuyển khoản.
            var order = await FindMatchingOrderAsync(orgId, line);

            if (order == null)
            {
                detail.MatchStatus = AutoApproveMatchStatus.Unmatched;
                detail.DiscrepancyReason = "Không tìm thấy phiếu thanh toán khớp (số tiền / nội dung chuyển khoản).";
                skipped++;
                batch.Details.Add(detail);
                continue;
            }

            detail.PaymentNo = order.PaymentNo;
            detail.DealerCode ??= order.PartnerCode;
            matched++;
            matchedAmount += line.Amount;

            // Kiểm tra trạng thái hợp lệ theo chế độ duyệt.
            var requiredStatus = dto.Mode == AutoApproveMode.ApproveA
                ? PaymentOrderStatus.PendingApproval
                : PaymentOrderStatus.Approved;

            if (order.Status != requiredStatus)
            {
                detail.MatchStatus = AutoApproveMatchStatus.Skipped;
                detail.DiscrepancyReason = dto.Mode == AutoApproveMode.ApproveA
                    ? $"Phiếu không ở trạng thái chờ duyệt (hiện tại: {order.Status})."
                    : $"Phiếu không ở trạng thái đã duyệt (hiện tại: {order.Status}).";
                skipped++;
                batch.Details.Add(detail);
                continue;
            }

            // Bước F yêu cầu có số chứng từ kế toán và ngày tiền về (tương ứng PaymentPaymentConfirm_MultiAndpushTCF).
            if (dto.Mode == AutoApproveMode.ApproveF)
            {
                if (string.IsNullOrWhiteSpace(detail.AccountingRecordNo) && string.IsNullOrWhiteSpace(order.AccountingRecordNo))
                {
                    detail.MatchStatus = AutoApproveMatchStatus.Skipped;
                    detail.DiscrepancyReason = "Thiếu số chứng từ kế toán (AccountingRecordNo) để hoàn tất.";
                    skipped++;
                    batch.Details.Add(detail);
                    continue;
                }
            }

            // Cập nhật phiếu thanh toán (tương ứng PaymentPaymentApproveX / PaymentPaymentConfirmX).
            if (!string.IsNullOrWhiteSpace(detail.AccountingRecordNo))
                order.AccountingRecordNo = detail.AccountingRecordNo;

            if (dto.Mode == AutoApproveMode.ApproveA)
            {
                order.Status = PaymentOrderStatus.Approved;
                order.ApprovedBy = "WSHTC"; // Duyệt tự động qua sổ phụ TCF
                order.ApprovedAt = now;
            }
            else
            {
                order.Status = PaymentOrderStatus.Finished;
                order.FinishedBy = "WSHTC";
                order.FinishedAt = now;
            }

            detail.MatchStatus = AutoApproveMatchStatus.Approved;
            detail.MatchedAt = now;
            approved++;
            batch.Details.Add(detail);
        }

        batch.MatchedCount = matched;
        batch.ApprovedCount = approved;
        batch.SkippedCount = skipped;
        batch.TotalAmount = totalAmount;
        batch.MatchedAmount = matchedAmount;
        batch.Status = AutoApproveBatchStatus.Completed;
        batch.CompletedAt = now;

        db.AutoApproveBatches.Add(batch);
        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Tìm phiếu thanh toán khớp với 1 dòng sổ phụ: ưu tiên theo PaymentNo, ngược lại theo số tiền + nội dung.
    /// </summary>
    private async Task<PaymentOrder?> FindMatchingOrderAsync(Guid orgId, BankStatementLineInputDto line)
    {
        if (!string.IsNullOrWhiteSpace(line.PaymentNo))
        {
            var byNo = await db.PaymentOrders
                .FirstOrDefaultAsync(x => x.OrgId == orgId && x.PaymentNo == line.PaymentNo.Trim());
            if (byNo != null) return byNo;
        }

        // Khớp theo số tiền + nội dung chuyển khoản (RemarkTranfer chứa số phiếu thanh toán).
        var candidates = await db.PaymentOrders
            .Where(x => x.OrgId == orgId && x.TotalAmount == line.Amount)
            .ToListAsync();

        if (candidates.Count == 0) return null;

        if (!string.IsNullOrWhiteSpace(line.Remark))
        {
            string remark = line.Remark.Trim().ToUpperInvariant();
            var byRemark = candidates.FirstOrDefault(c => remark.Contains(c.PaymentNo.ToUpperInvariant()));
            if (byRemark != null) return byRemark;
        }

        // Nếu chỉ có duy nhất 1 phiếu cùng số tiền thì coi như khớp.
        return candidates.Count == 1 ? candidates[0] : null;
    }

    /// <summary>Hủy lô duyệt tự động (chỉ khi còn Draft).</summary>
    public async Task<BankStatementAutoApproveBatch> CancelAsync(Guid orgId, long id)
    {
        var batch = await db.AutoApproveBatches
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lô duyệt tự động #{id}.");

        if (batch.Status == AutoApproveBatchStatus.Completed)
            throw new InvalidOperationException("Không thể hủy lô duyệt tự động đã hoàn tất.");

        batch.Status = AutoApproveBatchStatus.Cancelled;
        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>Báo cáo tổng hợp các lô duyệt tự động thanh toán theo sổ phụ.</summary>
    public async Task<AutoApproveSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.AutoApproveBatches
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new AutoApproveSummaryDto
        {
            TotalBatches = list.Count,
            CompletedCount = list.Count(x => x.Status == AutoApproveBatchStatus.Completed),
            DraftCount = list.Count(x => x.Status == AutoApproveBatchStatus.Draft),
            CancelledCount = list.Count(x => x.Status == AutoApproveBatchStatus.Cancelled),
            TotalRecords = list.Sum(x => x.TotalRecords),
            MatchedCount = list.Sum(x => x.MatchedCount),
            ApprovedCount = list.Sum(x => x.ApprovedCount),
            SkippedCount = list.Sum(x => x.SkippedCount),
            TotalAmount = list.Sum(x => x.TotalAmount),
            MatchedAmount = list.Sum(x => x.MatchedAmount)
        };

        summary.MatchRatePercent = summary.TotalRecords > 0
            ? Math.Round((double)summary.MatchedCount * 100.0 / summary.TotalRecords, 1)
            : 0;

        summary.RecentBatches = list
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(10)
            .Select(x => new AutoApproveBatchStatDto
            {
                Id = x.Id,
                BatchNo = x.BatchNo,
                Mode = x.Mode.ToString(),
                BankCode = x.BankCode,
                TotalRecords = x.TotalRecords,
                MatchedCount = x.MatchedCount,
                ApprovedCount = x.ApprovedCount,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return summary;
    }
}