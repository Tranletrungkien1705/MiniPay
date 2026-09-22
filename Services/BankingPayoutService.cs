using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ xử lý lệnh chi chuyển tiền ngân hàng tự động (Banking Payout / Bulk Payment).
/// Tương ứng khối RQ_BankingTransactions và RQ_BankingTransPmt trong BizHTC.VietinBank & BizHTC.MBBank.
/// </summary>
public sealed class BankingPayoutService(AppDbContext db)
{
    /// <summary>
    /// Tạo lô lệnh chi chuyển khoản ngân hàng (RQ_BankingTransactions_Save).
    /// </summary>
    public async Task<BankingPayoutBatch> CreateBatchAsync(
        Guid orgId,
        string? batchNo,
        string bankCode,
        string sourceAccount,
        string? sourceAccountName,
        string? bizResNumber,
        string? remark,
        List<PayoutItemInputDto> items,
        string? createdBy = "System")
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Lô lệnh chi phải có ít nhất 1 dòng chuyển tiền.");

        var finalBatchNo = string.IsNullOrWhiteSpace(batchNo)
            ? $"BTX-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
            : batchNo.Trim();

        // Kiểm tra trùng BatchNo
        var exists = await db.PayoutBatches.AnyAsync(b => b.OrgId == orgId && b.BatchNo == finalBatchNo);
        if (exists)
            throw new InvalidOperationException($"Mã lệnh chi '{finalBatchNo}' đã tồn tại trong hệ thống.");

        long totalAmount = 0;
        var details = new List<BankingPayoutDetail>();
        int idx = 1;

        foreach (var item in items)
        {
            if (item.TransferAmount <= 0)
                throw new ArgumentException($"Số tiền chuyển của người nhận '{item.ReceivingUnit}' phải > 0.");
            if (string.IsNullOrWhiteSpace(item.ReceivingUnit))
                throw new ArgumentException("Tên người thụ hưởng (ReceivingUnit) không được để trống.");
            if (string.IsNullOrWhiteSpace(item.BankAccountReceive))
                throw new ArgumentException($"Số tài khoản thụ hưởng của '{item.ReceivingUnit}' không được để trống.");
            if (string.IsNullOrWhiteSpace(item.BankNameReceive))
                throw new ArgumentException($"Tên ngân hàng thụ hưởng của '{item.ReceivingUnit}' không được để trống.");

            var transType = Enum.TryParse<PayoutTransType>(item.TransType, true, out var tt) ? tt : PayoutTransType.Napas247;
            var disbType = Enum.TryParse<DisbursementType>(item.DisbursementType, true, out var dt) ? dt : DisbursementType.Supplier;

            var detail = new BankingPayoutDetail
            {
                OrgId = orgId,
                TransNo = $"{finalBatchNo}-{idx:D3}",
                TransType = transType,
                DisbursementType = disbType,
                RefNo = item.RefNo?.Trim(),
                ReceivingUnit = item.ReceivingUnit.Trim(),
                BankAccountReceive = item.BankAccountReceive.Trim(),
                BankNameReceive = item.BankNameReceive.Trim().ToUpperInvariant(),
                ProvinceName = item.ProvinceName?.Trim(),
                TransferAmount = item.TransferAmount,
                TransferRemark = string.IsNullOrWhiteSpace(item.TransferRemark)
                    ? $"Thanh toan {item.RefNo ?? finalBatchNo}"
                    : item.TransferRemark.Trim(),
                Status = PayoutItemStatus.Pending
            };

            totalAmount += item.TransferAmount;
            details.Add(detail);
            idx++;
        }

        var batch = new BankingPayoutBatch
        {
            OrgId = orgId,
            BatchNo = finalBatchNo,
            BankCode = bankCode.Trim().ToUpperInvariant(),
            SourceAccount = sourceAccount.Trim(),
            SourceAccountName = sourceAccountName?.Trim(),
            BizResNumber = bizResNumber?.Trim(),
            Remark = remark?.Trim() ?? $"Lenh chi chuyen tien lo {finalBatchNo}",
            TotalTrans = details.Count,
            TotalAmount = totalAmount,
            Status = PayoutStatus.PendingApproval,
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PayoutBatches.Add(batch);
        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Kế toán trưởng duyệt lệnh chi (tương ứng RQ_BankingTransactions_Approve trong BizHTC).
    /// </summary>
    public async Task<BankingPayoutBatch?> ApproveBatchAsync(long batchId, Guid orgId, string? approvedBy = "AccountingManager")
    {
        var batch = await db.PayoutBatches
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.OrgId == orgId);

        if (batch == null) return null;
        if (batch.Status != PayoutStatus.PendingApproval && batch.Status != PayoutStatus.Draft)
            throw new InvalidOperationException($"Lệnh chi #{batch.BatchNo} đang ở trạng thái '{batch.Status}', không thể phê duyệt.");

        batch.Status = PayoutStatus.Approved;
        batch.ApprovedBy = approvedBy ?? "AccountingManager";
        batch.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Từ chối duyệt lệnh chi kèm lý do.
    /// </summary>
    public async Task<BankingPayoutBatch?> RejectBatchAsync(long batchId, Guid orgId, string? reason, string? rejectedBy = "AccountingManager")
    {
        var batch = await db.PayoutBatches
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.OrgId == orgId);

        if (batch == null) return null;
        if (batch.Status != PayoutStatus.PendingApproval)
            throw new InvalidOperationException($"Chỉ có thể từ chối lệnh chi ở trạng thái 'PendingApproval'.");

        batch.Status = PayoutStatus.Rejected;
        batch.RejectReason = reason ?? "Không đủ điều kiện thanh toán hoặc sai lệch chứng từ.";
        batch.ApprovedBy = rejectedBy;
        batch.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Hủy lệnh chi khi chưa đẩy ngân hàng.
    /// </summary>
    public async Task<BankingPayoutBatch?> CancelBatchAsync(long batchId, Guid orgId)
    {
        var batch = await db.PayoutBatches
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.OrgId == orgId);

        if (batch == null) return null;
        if (batch.Status == PayoutStatus.Processing || batch.Status == PayoutStatus.Completed)
            throw new InvalidOperationException($"Lệnh chi đã gửi sang ngân hàng xử lý, không thể hủy.");

        batch.Status = PayoutStatus.Cancelled;
        batch.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Đẩy lệnh chi sang cổng ngân hàng điện tử (Host-to-Host transfer).
    /// Tương ứng RQ_BankingTransactions_PushBank và MBBank_MakeBulkPayment_v2_1 trong BizHTC.
    /// Mô phỏng quy trình xử lý chuyển khoản ngân hàng và cập nhật kết quả từng dòng.
    /// </summary>
    public async Task<BankingPayoutBatch?> PushToBankAsync(long batchId, Guid orgId)
    {
        var batch = await db.PayoutBatches
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.OrgId == orgId);

        if (batch == null) return null;
        if (batch.Status != PayoutStatus.Approved)
            throw new InvalidOperationException($"Lệnh chi #{batch.BatchNo} cần được phê duyệt (Approved) trước khi đẩy sang ngân hàng.");

        batch.Status = PayoutStatus.Processing;
        var now = DateTime.Now;

        // Sinh mã tham chiếu giao dịch cổng ngân hàng (RefBankCode)
        var prefix = batch.BankCode.ToUpperInvariant() switch
        {
            "CTG" or "VIETINBANK" => "VTB-BULK",
            "MBB" or "MB" => "MBB-BULK",
            "VCB" => "VCB-BULK",
            "TCB" => "TCB-BULK",
            _ => "BANK-BULK"
        };
        batch.RefBankCode = $"{prefix}-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

        int successCount = 0;
        int failedCount = 0;
        long successAmount = 0;

        foreach (var d in batch.Details)
        {
            // Kiểm tra tính hợp lệ cơ bản của số tài khoản nhận
            if (d.BankAccountReceive.Length < 6)
            {
                d.Status = PayoutItemStatus.Failed;
                d.ErrorMessage = "Số tài khoản thụ hưởng không hợp lệ hoặc ngân hàng từ chối.";
                failedCount++;
            }
            else
            {
                d.Status = PayoutItemStatus.Success;
                d.BankTxnRef = $"{batch.BankCode}-{now:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
                d.ExecutedAt = now;
                successCount++;
                successAmount += d.TransferAmount;
            }
        }

        batch.SuccessTrans = successCount;
        batch.FailedTrans = failedCount;
        batch.SuccessAmount = successAmount;
        batch.CompletedAt = now;

        if (failedCount == 0)
        {
            batch.Status = PayoutStatus.Completed;
            batch.BankStatusCode = "00";
            batch.BankRemark = "Giao dịch chuyển tiền ngân hàng thành công trọn gói.";
        }
        else if (successCount > 0)
        {
            batch.Status = PayoutStatus.Completed; // Hoàn tất với sai sót một số dòng
            batch.BankStatusCode = "01";
            batch.BankRemark = $"Chuyển thành công {successCount}/{batch.TotalTrans} món, thất bại {failedCount} món.";
        }
        else
        {
            batch.Status = PayoutStatus.Rejected;
            batch.BankStatusCode = "99";
            batch.BankRemark = "Cổng ngân hàng từ chối toàn bộ danh sách giao dịch.";
        }

        await db.SaveChangesAsync();
        return batch;
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu Payout của merchant.
    /// </summary>
    public async Task<object> GetSummaryAsync(Guid orgId)
    {
        var batches = await db.PayoutBatches
            .Where(b => b.OrgId == orgId)
            .Include(b => b.Details)
            .ToListAsync();

        var totalBatches = batches.Count;
        var totalAmount = batches.Sum(b => b.TotalAmount);
        var successAmount = batches.Sum(b => b.SuccessAmount);
        var totalTrans = batches.Sum(b => b.TotalTrans);
        var successTrans = batches.Sum(b => b.SuccessTrans);
        var failedTrans = batches.Sum(b => b.FailedTrans);

        var successRate = totalTrans > 0 ? Math.Round((double)successTrans * 100.0 / totalTrans, 1) : 0;

        // Thống kê theo ngân hàng trích nợ
        var byBank = batches
            .GroupBy(b => b.BankCode)
            .Select(g => new
            {
                bankCode = g.Key,
                batches = g.Count(),
                totalAmount = g.Sum(x => x.TotalAmount),
                successAmount = g.Sum(x => x.SuccessAmount)
            })
            .OrderByDescending(x => x.totalAmount)
            .ToList();

        // Thống kê theo loại chi
        var allDetails = batches.SelectMany(b => b.Details).ToList();
        var byDisbursementType = allDetails
            .GroupBy(d => d.DisbursementType.ToString())
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                amount = g.Sum(x => x.TransferAmount)
            })
            .OrderByDescending(x => x.amount)
            .ToList();

        return new
        {
            totalBatches,
            totalAmount,
            successAmount,
            totalTrans,
            successTrans,
            failedTrans,
            successRatePercent = successRate,
            byBank,
            byDisbursementType
        };
    }
}

public sealed record PayoutItemInputDto(
    string? TransType,
    string? DisbursementType,
    string? RefNo,
    string ReceivingUnit,
    string BankAccountReceive,
    string BankNameReceive,
    string? ProvinceName,
    long TransferAmount,
    string? TransferRemark
);

