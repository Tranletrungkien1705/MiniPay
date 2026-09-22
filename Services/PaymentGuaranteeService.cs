using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ quản lý Thư Bảo lãnh thanh toán ngân hàng (Bank Payment Guarantee).
/// Tương ứng khối Pmt_Guarantee & Pmt_GuaranteeDetail và FrmMngGrt trong BizHTC.Payment.
/// </summary>
public sealed class PaymentGuaranteeService(AppDbContext db)
{
    /// <summary>
    /// Lập thư bảo lãnh thanh toán mới (Pmt_Guarantee_Save / FrmNewGrt.MODE_NEW).
    /// </summary>
    public async Task<PaymentGuarantee> CreateGuaranteeAsync(
        Guid orgId,
        string? guaranteeNo,
        string bankGuaranteeNo,
        string bankCode,
        string? bankName,
        string partnerCode,
        string? partnerName,
        string? contractNo,
        GuaranteeType guaranteeType,
        long? totalAmount,
        DateTime? dateOpen,
        DateTime? dateExpired,
        DateTime? dateEnd,
        decimal? feePercent,
        string? remark,
        List<GuaranteeItemInputDto>? items)
    {
        if (string.IsNullOrWhiteSpace(bankGuaranteeNo))
            throw new ArgumentException("Số thư bảo lãnh ngân hàng (BankGuaranteeNo) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Mã ngân hàng (BankCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đối tác / đại lý (PartnerCode) không được để trống.");

        var finalGrtNo = string.IsNullOrWhiteSpace(guaranteeNo)
            ? $"GRT-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
            : guaranteeNo.Trim();

        var exists = await db.Guarantees.AnyAsync(g => g.OrgId == orgId && g.GuaranteeNo == finalGrtNo);
        if (exists)
            throw new InvalidOperationException($"Số bảo lãnh hệ thống '{finalGrtNo}' đã tồn tại.");

        var openDate = dateOpen ?? DateTime.Today;
        var expiredDate = dateExpired ?? openDate.AddDays(90);
        var endDate = dateEnd ?? expiredDate;
        var termDays = (expiredDate.Date - openDate.Date).Days;
        if (termDays <= 0) termDays = 1;

        var fee = feePercent.GetValueOrDefault(1.2m);
        if (fee < 0 || fee > 10m) fee = 1.2m;

        long calculatedUtilized = 0;
        var details = new List<PaymentGuaranteeDetail>();

        if (items != null && items.Count > 0)
        {
            int idx = 1;
            foreach (var item in items)
            {
                if (item.GuaranteeValue <= 0)
                    throw new ArgumentException($"Giá trị bảo lãnh dòng #{idx} phải > 0.");

                calculatedUtilized += item.GuaranteeValue;
                var ordAmount = item.OrderAmount > 0 ? item.OrderAmount : item.GuaranteeValue;
                var grtPct = ordAmount > 0
                    ? Math.Round((decimal)item.GuaranteeValue * 100m / ordAmount, 2)
                    : 100m;

                details.Add(new PaymentGuaranteeDetail
                {
                    OrgId = orgId,
                    ItemRefNo = string.IsNullOrWhiteSpace(item.ItemRefNo) ? $"ITEM-{idx:D2}" : item.ItemRefNo.Trim(),
                    Description = string.IsNullOrWhiteSpace(item.Description)
                        ? $"Hàng hóa / Xe theo đơn #{idx}"
                        : item.Description.Trim(),
                    OrderAmount = ordAmount,
                    GuaranteeValue = item.GuaranteeValue,
                    GuaranteePercent = grtPct,
                    DateStart = item.DateStart ?? openDate,
                    DateEnd = item.DateEnd ?? endDate,
                    Status = GuaranteeDetailStatus.Active,
                    Note = item.Note
                });
                idx++;
            }
        }

        long finalTotalAmount = totalAmount.HasValue && totalAmount.Value > 0
            ? totalAmount.Value
            : calculatedUtilized;

        if (finalTotalAmount < calculatedUtilized)
            finalTotalAmount = calculatedUtilized;

        long remainingAmount = finalTotalAmount - calculatedUtilized;

        var grt = new PaymentGuarantee
        {
            OrgId = orgId,
            GuaranteeNo = finalGrtNo,
            BankGuaranteeNo = bankGuaranteeNo.Trim(),
            BankCode = bankCode.Trim().ToUpper(),
            BankName = string.IsNullOrWhiteSpace(bankName) ? ResolveBankName(bankCode) : bankName.Trim(),
            PartnerCode = partnerCode.Trim().ToUpper(),
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? $"Đại lý {partnerCode.Trim().ToUpper()}" : partnerName.Trim(),
            ContractNo = contractNo?.Trim(),
            GuaranteeType = guaranteeType,
            TotalAmount = finalTotalAmount,
            UtilizedAmount = calculatedUtilized,
            RemainingAmount = remainingAmount,
            DateOpen = openDate,
            DateExpired = expiredDate,
            DateEnd = endDate,
            TermDays = termDays,
            FeePercent = fee,
            Status = GuaranteeStatus.PendingApproval,
            Remark = remark,
            CreatedBy = "SalesSpecialist",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.Guarantees.Add(grt);
        await db.SaveChangesAsync();

        return grt;
    }

    /// <summary>
    /// Phê duyệt Thư bảo lãnh thanh toán (FrmNewGrt.btnApprove_Click).
    /// </summary>
    public async Task<PaymentGuarantee?> ApproveGuaranteeAsync(long id, Guid orgId, string? approvedBy)
    {
        var grt = await db.Guarantees
            .Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);

        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.PendingApproval && grt.Status != GuaranteeStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể phê duyệt bảo lãnh ở trạng thái Chờ duyệt hoặc Dự thảo. Hiện tại: {grt.Status}.");

        grt.Status = GuaranteeStatus.Active;
        grt.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "FinanceDirector" : approvedBy.Trim();
        grt.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Gia hạn thời hạn thư bảo lãnh (FrmEditGrtExpiredDate / FrmEditGrtEndDate).
    /// </summary>
    public async Task<PaymentGuarantee?> ExtendGuaranteeAsync(
        long id,
        Guid orgId,
        DateTime newExpiredDate,
        DateTime? newEndDate,
        string? remark)
    {
        var grt = await db.Guarantees.FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);
        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.Active && grt.Status != GuaranteeStatus.PendingApproval)
            throw new InvalidOperationException($"Không thể gia hạn bảo lãnh ở trạng thái {grt.Status}.");

        if (newExpiredDate.Date < grt.DateOpen.Date)
            throw new ArgumentException("Ngày hết hạn mới không được trước ngày mở bảo lãnh.");

        grt.DateExpired = newExpiredDate;
        grt.DateEnd = newEndDate ?? newExpiredDate;
        grt.TermDays = (newExpiredDate.Date - grt.DateOpen.Date).Days;
        if (!string.IsNullOrWhiteSpace(remark))
            grt.Remark = string.IsNullOrEmpty(grt.Remark) ? remark : $"{grt.Remark}; {remark}";

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Điều chỉnh giá trị/hạn mức bảo lãnh (FrmEditGrtValue).
    /// </summary>
    public async Task<PaymentGuarantee?> AdjustValueAsync(long id, Guid orgId, long newTotalAmount, string? remark)
    {
        var grt = await db.Guarantees.FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);
        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.Active && grt.Status != GuaranteeStatus.PendingApproval)
            throw new InvalidOperationException($"Không thể điều chỉnh giá trị bảo lãnh ở trạng thái {grt.Status}.");

        if (newTotalAmount < grt.UtilizedAmount)
            throw new ArgumentException($"Tổng giá trị bảo lãnh mới ({newTotalAmount:N0} đ) không thể nhỏ hơn số tiền đã phân bổ ({grt.UtilizedAmount:N0} đ).");

        grt.TotalAmount = newTotalAmount;
        grt.RemainingAmount = newTotalAmount - grt.UtilizedAmount;
        if (!string.IsNullOrWhiteSpace(remark))
            grt.Remark = string.IsNullOrEmpty(grt.Remark) ? remark : $"{grt.Remark}; {remark}";

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Cập nhật ngày nhận bản gốc thư bảo lãnh từ ngân hàng (FrmEditDateRecieveGrtRoot).
    /// </summary>
    public async Task<PaymentGuarantee?> UpdateReceiveRootDateAsync(long id, Guid orgId, DateTime receiveDate, string? note)
    {
        var grt = await db.Guarantees.FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);
        if (grt == null) return null;

        grt.DateRecieveGrtRoot = receiveDate;
        if (!string.IsNullOrWhiteSpace(note))
            grt.Remark = string.IsNullOrEmpty(grt.Remark) ? note : $"{grt.Remark}; {note}";

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Kích hoạt đòi bảo lãnh ngân hàng (Pmt_GrtClaim / FrmMngGrtClaim) khi đối tác vi phạm hoặc quá hạn thanh toán.
    /// </summary>
    public async Task<PaymentGuarantee?> ClaimGuaranteeAsync(
        long id,
        Guid orgId,
        long claimAmount,
        string claimReason,
        string? claimedBy)
    {
        var grt = await db.Guarantees
            .Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);

        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.Active)
            throw new InvalidOperationException($"Chỉ có thể kích hoạt đòi bảo lãnh khi đang có hiệu lực (Active). Hiện tại: {grt.Status}.");

        if (claimAmount <= 0 || claimAmount > grt.TotalAmount)
            throw new ArgumentException($"Số tiền đòi bảo lãnh phải > 0 và <= tổng giá trị bảo lãnh ({grt.TotalAmount:N0} đ).");

        if (string.IsNullOrWhiteSpace(claimReason))
            throw new ArgumentException("Vui lòng nhập lý do đòi bảo lãnh ngân hàng.");

        grt.Status = GuaranteeStatus.Claimed;
        grt.ClaimedAmount = claimAmount;
        grt.ClaimReason = claimReason.Trim();
        grt.ClaimedBy = string.IsNullOrWhiteSpace(claimedBy) ? "ChiefRiskOfficer" : claimedBy.Trim();
        grt.ClaimedAt = DateTime.Now;

        foreach (var dtl in grt.Details.Where(d => d.Status == GuaranteeDetailStatus.Active))
        {
            dtl.Status = GuaranteeDetailStatus.Claimed;
        }

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Hoàn tất tất toán / Giải tỏa toàn bộ nghĩa vụ bảo lãnh (FrmQuanLyBBBGTheoHoiPhieu / Settle Guarantee).
    /// </summary>
    public async Task<PaymentGuarantee?> SettleGuaranteeAsync(long id, Guid orgId, string? settledBy, string? remark)
    {
        var grt = await db.Guarantees
            .Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);

        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.Active && grt.Status != GuaranteeStatus.Claimed)
            throw new InvalidOperationException($"Chỉ có thể tất toán bảo lãnh đang có hiệu lực (Active) hoặc đã đòi bảo lãnh (Claimed). Hiện tại: {grt.Status}.");

        grt.Status = GuaranteeStatus.Settled;
        grt.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "ChiefAccountant" : settledBy.Trim();
        grt.SettledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(remark))
            grt.Remark = string.IsNullOrEmpty(grt.Remark) ? remark : $"{grt.Remark}; {remark}";

        foreach (var dtl in grt.Details.Where(d => d.Status == GuaranteeDetailStatus.Active || d.Status == GuaranteeDetailStatus.Claimed))
        {
            dtl.Status = GuaranteeDetailStatus.Released;
        }

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Từ chối thẩm định / phê duyệt thư bảo lãnh (FrmNewGrt.btnReject_Click).
    /// </summary>
    public async Task<PaymentGuarantee?> RejectGuaranteeAsync(long id, Guid orgId, string? reason, string? rejectedBy)
    {
        var grt = await db.Guarantees.FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);
        if (grt == null) return null;

        if (grt.Status != GuaranteeStatus.PendingApproval && grt.Status != GuaranteeStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể từ chối bảo lãnh ở trạng thái Chờ duyệt hoặc Dự thảo. Hiện tại: {grt.Status}.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Vui lòng nhập lý do từ chối thư bảo lãnh.");

        grt.Status = GuaranteeStatus.Rejected;
        grt.RemarkReject = reason.Trim();
        grt.ApprovedBy = string.IsNullOrWhiteSpace(rejectedBy) ? "FinanceDirector" : rejectedBy.Trim();
        grt.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Hủy thư bảo lãnh khi chưa sử dụng (FrmNewGrt.btnCancelGrt_Click).
    /// </summary>
    public async Task<PaymentGuarantee?> CancelGuaranteeAsync(long id, Guid orgId, string? reason)
    {
        var grt = await db.Guarantees
            .Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == id && g.OrgId == orgId);

        if (grt == null) return null;

        if (grt.Status == GuaranteeStatus.Settled || grt.Status == GuaranteeStatus.Cancelled)
            throw new InvalidOperationException($"Bảo lãnh đã ở trạng thái {grt.Status}, không thể hủy.");

        grt.Status = GuaranteeStatus.Cancelled;
        grt.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
            grt.Remark = string.IsNullOrEmpty(grt.Remark) ? reason : $"{grt.Remark}; Hủy: {reason}";

        foreach (var dtl in grt.Details)
        {
            dtl.Status = GuaranteeDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return grt;
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu bảo lãnh ngân hàng.
    /// </summary>
    public async Task<object> GetSummaryAsync(Guid orgId)
    {
        var list = await db.Guarantees
            .Where(g => g.OrgId == orgId)
            .ToListAsync();

        var today = DateTime.Today;
        var soonDate = today.AddDays(15);

        var totalGuarantees = list.Count;
        var activeCount = list.Count(g => g.Status == GuaranteeStatus.Active);
        var pendingCount = list.Count(g => g.Status == GuaranteeStatus.PendingApproval);
        var claimedCount = list.Count(g => g.Status == GuaranteeStatus.Claimed);
        var settledCount = list.Count(g => g.Status == GuaranteeStatus.Settled);
        var expiringSoonCount = list.Count(g => g.Status == GuaranteeStatus.Active && g.DateExpired <= soonDate && g.DateExpired >= today);

        var totalAmount = list.Sum(g => g.TotalAmount);
        var activeAmount = list.Where(g => g.Status == GuaranteeStatus.Active).Sum(g => g.TotalAmount);
        var claimedAmount = list.Where(g => g.Status == GuaranteeStatus.Claimed).Sum(g => g.ClaimedAmount ?? g.TotalAmount);
        var settledAmount = list.Where(g => g.Status == GuaranteeStatus.Settled).Sum(g => g.TotalAmount);

        var bankGroups = list.GroupBy(g => g.BankCode)
            .Select(grp => new
            {
                BankCode = grp.Key,
                Count = grp.Count(),
                TotalAmount = grp.Sum(g => g.TotalAmount),
                ActiveAmount = grp.Where(g => g.Status == GuaranteeStatus.Active).Sum(g => g.TotalAmount)
            })
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        return new
        {
            totalGuarantees,
            activeCount,
            pendingCount,
            claimedCount,
            settledCount,
            expiringSoonCount,
            totalAmount,
            activeAmount,
            claimedAmount,
            settledAmount,
            bankGroups
        };
    }

    private static string ResolveBankName(string code) => code.ToUpper() switch
    {
        "VCB" => "Ngân hàng TMCP Ngoại thương Việt Nam (Vietcombank)",
        "TCB" => "Ngân hàng TMCP Kỹ thương Việt Nam (Techcombank)",
        "MBB" => "Ngân hàng TMCP Quân đội (MBBank)",
        "CTG" => "Ngân hàng TMCP Công thương Việt Nam (VietinBank)",
        "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
        "VPB" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
        "ACB" => "Ngân hàng TMCP Á Châu (ACB)",
        "TPB" => "Ngân hàng TMCP Tiên Phong (TPBank)",
        _ => $"Ngân hàng {code.ToUpper()}"
    };
}

public record GuaranteeItemInputDto(
    string? ItemRefNo,
    string? Description,
    long OrderAmount,
    long GuaranteeValue,
    DateTime? DateStart,
    DateTime? DateEnd,
    string? Note
);
