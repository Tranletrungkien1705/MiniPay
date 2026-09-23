using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Cập nhật Chứng từ Kế toán (Accounting Voucher / Accounting Record No bulk update).
/// Tương ứng nghiệp vụ FrmUpdateChungTuKT + SalesService.UpdateCTKT + Pmt_Payment_UpdateFinancial
/// trong hệ nguồn 2010.HTC (TERP.HTCClient/Views/Sales/Payment & TERP.BizHTC/TCFIntergration).
///
/// Nghiệp vụ: kế toán cập nhật HÀNG LOẠT số chứng từ kế toán (AccountingRecordNo) cho các phiếu
/// thanh toán (Pmt_Payment) đã hoàn tất (Stage.Finished), phục vụ ghi sổ / đối chiếu sổ quỹ.
/// Quy trình: lập lô cập nhật (Draft) -> áp dụng ghi sổ (Applied) hoặc hủy (Cancelled).
/// Khi áp dụng, hệ thống kiểm tra từng phiếu: phải tồn tại và ở trạng thái Finished mới cập nhật,
/// ngược lại đánh dấu Skipped kèm lý do (tương ứng Pmt_Payment_CheckDB trong nguồn).
/// </summary>
public sealed class AccountingVoucherService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách lô cập nhật chứng từ kế toán kèm bộ lọc (trạng thái, từ khóa, khoảng ngày).
    /// </summary>
    public async Task<List<AccountingVoucherUpdate>> GetListAsync(
        Guid orgId,
        string? status = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.AccountingVoucherUpdates
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AccountingVoucherUpdateStatus>(status, true, out var st))
            q = q.Where(x => x.Status == st);

        if (fromDate.HasValue)
            q = q.Where(x => x.CreatedAt >= fromDate.Value.Date);

        if (toDate.HasValue)
            q = q.Where(x => x.CreatedAt < toDate.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.BatchNo.ToUpper().Contains(s)
                || (x.Description != null && x.Description.ToUpper().Contains(s))
                || (x.Remark != null && x.Remark.ToUpper().Contains(s)));
        }

        return await q.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 lô cập nhật chứng từ kế toán theo ID (kèm danh sách dòng).</summary>
    public async Task<AccountingVoucherUpdate?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.AccountingVoucherUpdates
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>
    /// Lập lô cập nhật chứng từ kế toán mới (tương ứng FrmUpdateChungTuKT nhập danh sách PaymentNo + NewAccountingRecordNo).
    /// Sinh số lô tự động CTKT-yyyyMM-xxx nếu không truyền vào.
    /// </summary>
    public async Task<AccountingVoucherUpdate> CreateAsync(Guid orgId, CreateAccountingVoucherUpdateDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Danh sách phiếu thanh toán cần cập nhật chứng từ không được để trống.");

        var validItems = dto.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.PaymentNo))
            .ToList();
        if (validItems.Count == 0)
            throw new ArgumentException("Mỗi dòng cập nhật phải có Số phiếu thanh toán (PaymentNo).");

        string batchNo = dto.BatchNo?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(batchNo))
        {
            string monthPrefix = $"CTKT-{DateTime.Now:yyyyMM}-";
            int seq = await db.AccountingVoucherUpdates
                .Where(x => x.OrgId == orgId && x.BatchNo.StartsWith(monthPrefix))
                .CountAsync();
            batchNo = $"{monthPrefix}{seq + 1:D3}";
        }
        else
        {
            bool dup = await db.AccountingVoucherUpdates.AnyAsync(x => x.OrgId == orgId && x.BatchNo == batchNo);
            if (dup)
                throw new InvalidOperationException($"Số lô cập nhật chứng từ {batchNo} đã tồn tại.");
        }

        var now = DateTime.Now;
        var entity = new AccountingVoucherUpdate
        {
            OrgId = orgId,
            BatchNo = batchNo,
            Description = dto.Description?.Trim(),
            TotalItems = validItems.Count,
            UpdatedItems = 0,
            SkippedItems = 0,
            Status = AccountingVoucherUpdateStatus.Draft,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Accountant_HTC",
            CreatedAt = now,
            Remark = dto.Remark?.Trim()
        };

        foreach (var item in validItems)
        {
            entity.Details.Add(new AccountingVoucherUpdateDetail
            {
                OrgId = orgId,
                PaymentNo = item.PaymentNo.Trim(),
                NewAccountingRecordNo = item.NewAccountingRecordNo?.Trim(),
                Status = AccountingVoucherUpdateDetailStatus.Pending
            });
        }

        db.AccountingVoucherUpdates.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Áp dụng lô cập nhật chứng từ kế toán (tương ứng Pmt_Payment_UpdateFinancial).
    /// Với mỗi dòng: kiểm tra phiếu thanh toán tồn tại và ở trạng thái Finished mới cập nhật
    /// AccountingRecordNo; ngược lại đánh dấu Skipped kèm lý do.
    /// </summary>
    public async Task<AccountingVoucherUpdate> ApplyAsync(Guid orgId, long id, ApplyAccountingVoucherUpdateDto? dto)
    {
        var entity = await db.AccountingVoucherUpdates
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lô cập nhật chứng từ #{id}.");

        if (entity.Status != AccountingVoucherUpdateStatus.Draft)
            throw new InvalidOperationException($"Chỉ áp dụng được lô cập nhật ở trạng thái Draft (hiện tại: {entity.Status}).");

        int updated = 0;
        int skipped = 0;

        foreach (var detail in entity.Details)
        {
            var order = await db.PaymentOrders
                .FirstOrDefaultAsync(x => x.OrgId == orgId && x.PaymentNo == detail.PaymentNo);

            if (order == null)
            {
                detail.Status = AccountingVoucherUpdateDetailStatus.Skipped;
                detail.Note = "Không tìm thấy phiếu thanh toán (PaymentNoNotFound).";
                skipped++;
                continue;
            }

            if (order.Status != PaymentOrderStatus.Finished)
            {
                detail.Status = AccountingVoucherUpdateDetailStatus.Skipped;
                detail.Note = $"Phiếu chưa hoàn tất ghi sổ (trạng thái hiện tại: {order.Status}).";
                skipped++;
                continue;
            }

            detail.OldAccountingRecordNo = order.AccountingRecordNo;
            order.AccountingRecordNo = detail.NewAccountingRecordNo;
            detail.Status = AccountingVoucherUpdateDetailStatus.Updated;
            detail.Note = "Đã cập nhật số chứng từ kế toán.";
            updated++;
        }

        entity.UpdatedItems = updated;
        entity.SkippedItems = skipped;
        entity.Status = AccountingVoucherUpdateStatus.Applied;
        entity.AppliedBy = !string.IsNullOrWhiteSpace(dto?.AppliedBy) ? dto!.AppliedBy!.Trim() : "Accountant_HTC";
        entity.AppliedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>Hủy lô cập nhật chứng từ kế toán (chỉ khi còn Draft).</summary>
    public async Task<AccountingVoucherUpdate> CancelAsync(Guid orgId, long id)
    {
        var entity = await db.AccountingVoucherUpdates
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lô cập nhật chứng từ #{id}.");

        if (entity.Status == AccountingVoucherUpdateStatus.Applied)
            throw new InvalidOperationException("Không thể hủy lô cập nhật đã áp dụng ghi sổ.");

        entity.Status = AccountingVoucherUpdateStatus.Cancelled;
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>Báo cáo tổng hợp các lô cập nhật chứng từ kế toán.</summary>
    public async Task<AccountingVoucherUpdateSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.AccountingVoucherUpdates
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new AccountingVoucherUpdateSummaryDto
        {
            TotalBatches = list.Count,
            DraftCount = list.Count(x => x.Status == AccountingVoucherUpdateStatus.Draft),
            AppliedCount = list.Count(x => x.Status == AccountingVoucherUpdateStatus.Applied),
            CancelledCount = list.Count(x => x.Status == AccountingVoucherUpdateStatus.Cancelled),
            TotalItems = list.Sum(x => x.TotalItems),
            UpdatedItems = list.Sum(x => x.UpdatedItems),
            SkippedItems = list.Sum(x => x.SkippedItems)
        };

        summary.RecentBatches = list
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(10)
            .Select(x => new AccountingVoucherBatchStatDto
            {
                Id = x.Id,
                BatchNo = x.BatchNo,
                Status = x.Status.ToString(),
                TotalItems = x.TotalItems,
                UpdatedItems = x.UpdatedItems,
                SkippedItems = x.SkippedItems,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return summary;
    }
}