using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ tính toán và quản lý hồ sơ Chiết khấu thanh toán sớm (Payment Discount Request).
/// Tương ứng khối Req_PaymentDiscount & Req_PaymentDiscountDtl trong BizHTC.PaymentDiscount.
/// </summary>
public sealed class PaymentDiscountService(AppDbContext db)
{
    /// <summary>
    /// Tính toán chiết khấu thanh toán sớm theo chuẩn tài chính 360 ngày (BizHTC / Ngân hàng):
    /// DiscountAmount = OriginalAmount * (AnnualRate / 100) * EarlyDays / 360
    /// </summary>
    public static (int earlyDays, long discountAmount, long netPayAmount) CalculateDiscount(
        long originalAmount,
        decimal annualRate,
        DateTime dueDate,
        DateTime actualPaymentDate)
    {
        var diff = (dueDate.Date - actualPaymentDate.Date).Days;
        int earlyDays = diff > 0 ? diff : 0;

        long discount = 0;
        if (earlyDays > 0 && originalAmount > 0 && annualRate > 0)
        {
            var calculated = (double)originalAmount * ((double)annualRate / 100.0) * ((double)earlyDays / 360.0);
            discount = (long)Math.Round(calculated, MidpointRounding.AwayFromZero);
            if (discount > originalAmount) discount = originalAmount;
        }

        long netPay = originalAmount - discount;
        return (earlyDays, discount, netPay);
    }

    /// <summary>
    /// Lập hồ sơ yêu cầu chiết khấu thanh toán sớm (Req_PaymentDiscount_Save).
    /// Tự động tính toán số ngày thanh toán sớm và tiền chiết khấu từng hạng mục.
    /// </summary>
    public async Task<PaymentDiscountRequest> CreateRequestAsync(
        Guid orgId,
        string? discountNo,
        string partnerCode,
        string? partnerName,
        string? contractNo,
        decimal? defaultAnnualRate,
        string? remark,
        List<DiscountItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đối tác / đại lý (PartnerCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Hồ sơ chiết khấu phải có ít nhất 1 dòng thanh toán.");

        var finalDiscountNo = string.IsNullOrWhiteSpace(discountNo)
            ? $"DIS-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
            : discountNo.Trim();

        var exists = await db.DiscountRequests.AnyAsync(r => r.OrgId == orgId && r.DiscountNo == finalDiscountNo);
        if (exists)
            throw new InvalidOperationException($"Mã hồ sơ chiết khấu '{finalDiscountNo}' đã tồn tại.");

        var rate = defaultAnnualRate.GetValueOrDefault(7.5m);
        if (rate <= 0) rate = 7.5m;

        long totalPaymentAmount = 0;
        long totalDiscountAmount = 0;
        var details = new List<PaymentDiscountDetail>();
        int idx = 1;

        foreach (var item in items)
        {
            if (item.OriginalAmount <= 0)
                throw new ArgumentException($"Số tiền thanh toán dòng #{idx} phải > 0.");

            var itemRate = (item.AnnualDiscountRate.HasValue && item.AnnualDiscountRate.Value > 0)
                ? item.AnnualDiscountRate.Value
                : rate;

            var (earlyDays, discountAmount, netPayAmount) = CalculateDiscount(
                item.OriginalAmount,
                itemRate,
                item.DueDate,
                item.ActualPaymentDate
            );

            var detail = new PaymentDiscountDetail
            {
                OrgId = orgId,
                ItemRefNo = string.IsNullOrWhiteSpace(item.ItemRefNo) ? $"PHASE-{idx:D2}" : item.ItemRefNo.Trim(),
                Description = string.IsNullOrWhiteSpace(item.Description)
                    ? $"Thanh toán hạn {item.DueDate:dd/MM/yyyy}"
                    : item.Description.Trim(),
                DueDate = item.DueDate,
                ActualPaymentDate = item.ActualPaymentDate,
                EarlyDays = earlyDays,
                OriginalAmount = item.OriginalAmount,
                AnnualDiscountRate = itemRate,
                DiscountAmount = discountAmount,
                NetPayAmount = netPayAmount,
                Status = DiscountItemStatus.Active,
                Note = item.Note?.Trim()
            };

            details.Add(detail);
            totalPaymentAmount += item.OriginalAmount;
            totalDiscountAmount += discountAmount;
            idx++;
        }

        var req = new PaymentDiscountRequest
        {
            OrgId = orgId,
            DiscountNo = finalDiscountNo,
            PartnerCode = partnerCode.Trim().ToUpperInvariant(),
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? partnerCode.Trim() : partnerName.Trim(),
            ContractNo = contractNo?.Trim(),
            DefaultAnnualRate = rate,
            TotalPaymentAmount = totalPaymentAmount,
            TotalDiscountAmount = totalDiscountAmount,
            NetPaymentAmount = totalPaymentAmount - totalDiscountAmount,
            Status = DiscountRequestStatus.Draft,
            PartnerSignStatus = DiscountSignStatus.Pending,
            ApproverSignStatus = DiscountSignStatus.Pending,
            Remark = remark?.Trim() ?? "",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.DiscountRequests.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Đối tác / Đại lý ký số xác nhận yêu cầu chiết khấu (Req_PaymentDiscount_DlrSign).
    /// </summary>
    public async Task<PaymentDiscountRequest?> PartnerSignAsync(long id, Guid orgId, string? signerName)
    {
        var req = await db.DiscountRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status != DiscountRequestStatus.Draft)
            throw new InvalidOperationException($"Chỉ hồ sơ ở trạng thái Draft mới có thể ký xác nhận đối tác. Hiện tại: {req.Status}.");

        req.PartnerSignStatus = DiscountSignStatus.Signed;
        req.PartnerSignedBy = string.IsNullOrWhiteSpace(signerName) ? "PartnerRepresentative" : signerName.Trim();
        req.PartnerSignedAt = DateTime.Now;
        req.Status = DiscountRequestStatus.PartnerSigned;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Kế toán trưởng / Phòng tài chính duyệt chiết khấu thanh toán (Req_PaymentDiscount_HTCApprove).
    /// </summary>
    public async Task<PaymentDiscountRequest?> ApproveRequestAsync(long id, Guid orgId, string? approverName)
    {
        var req = await db.DiscountRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status != DiscountRequestStatus.Draft && req.Status != DiscountRequestStatus.PartnerSigned)
            throw new InvalidOperationException($"Chỉ hồ sơ Draft hoặc PartnerSigned mới có thể phê duyệt. Hiện tại: {req.Status}.");

        req.Status = DiscountRequestStatus.Approved;
        req.ApprovedBy = string.IsNullOrWhiteSpace(approverName) ? "ChiefFinancialOfficer" : approverName.Trim();
        req.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Lãnh đạo ký số & hoàn tất quyết toán chiết khấu công nợ (Req_PaymentDiscount_HTCSign).
    /// </summary>
    public async Task<PaymentDiscountRequest?> SettleRequestAsync(long id, Guid orgId, string? settlerName)
    {
        var req = await db.DiscountRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status != DiscountRequestStatus.Approved)
            throw new InvalidOperationException($"Chỉ hồ sơ đã được duyệt (Approved) mới có thể quyết toán & ký số. Hiện tại: {req.Status}.");

        req.ApproverSignStatus = DiscountSignStatus.Signed;
        req.SettledBy = string.IsNullOrWhiteSpace(settlerName) ? "FinancialDirector" : settlerName.Trim();
        req.SettledAt = DateTime.Now;
        req.Status = DiscountRequestStatus.Settled;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Từ chối yêu cầu chiết khấu (Req_PaymentDiscount_HTCReject).
    /// </summary>
    public async Task<PaymentDiscountRequest?> RejectRequestAsync(long id, Guid orgId, string? reason, string? rejecterName)
    {
        var req = await db.DiscountRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status == DiscountRequestStatus.Settled)
            throw new InvalidOperationException("Hồ sơ đã quyết toán hoàn tất, không thể từ chối.");
        if (req.Status == DiscountRequestStatus.Cancelled)
            throw new InvalidOperationException("Hồ sơ đã hủy, không thể từ chối.");

        req.Status = DiscountRequestStatus.Rejected;
        req.RejectReason = string.IsNullOrWhiteSpace(reason)
            ? "Không thỏa điều kiện chiết khấu thanh toán sớm hoặc ngày chứng từ quá hạn."
            : reason.Trim();
        req.ApprovedBy = string.IsNullOrWhiteSpace(rejecterName) ? "FinanceAuditor" : rejecterName.Trim();
        req.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Hủy yêu cầu chiết khấu (Req_PaymentDiscount_HTCCancel).
    /// </summary>
    public async Task<PaymentDiscountRequest?> CancelRequestAsync(long id, Guid orgId)
    {
        var req = await db.DiscountRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status == DiscountRequestStatus.Settled)
            throw new InvalidOperationException("Hồ sơ đã quyết toán hoàn tất, không thể hủy.");

        req.Status = DiscountRequestStatus.Cancelled;
        req.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu chiết khấu thanh toán của tổ chức (Merchant).
    /// </summary>
    public async Task<object> GetSummaryAsync(Guid orgId)
    {
        var requests = await db.DiscountRequests
            .Where(r => r.OrgId == orgId)
            .ToListAsync();

        var totalRequests = requests.Count;
        var totalPaymentAmount = requests.Sum(r => r.TotalPaymentAmount);
        var totalDiscountAmount = requests.Sum(r => r.TotalDiscountAmount);
        var settledDiscountAmount = requests.Where(r => r.Status == DiscountRequestStatus.Settled).Sum(r => r.TotalDiscountAmount);

        var draftCount = requests.Count(r => r.Status == DiscountRequestStatus.Draft);
        var partnerSignedCount = requests.Count(r => r.Status == DiscountRequestStatus.PartnerSigned);
        var approvedCount = requests.Count(r => r.Status == DiscountRequestStatus.Approved);
        var settledCount = requests.Count(r => r.Status == DiscountRequestStatus.Settled);
        var rejectedCount = requests.Count(r => r.Status == DiscountRequestStatus.Rejected);
        var cancelledCount = requests.Count(r => r.Status == DiscountRequestStatus.Cancelled);

        var discountRatePercent = totalPaymentAmount > 0
            ? Math.Round((double)totalDiscountAmount * 100.0 / totalPaymentAmount, 2)
            : 0;

        return new
        {
            totalRequests,
            totalPaymentAmount,
            totalDiscountAmount,
            settledDiscountAmount,
            discountRatePercent,
            counts = new
            {
                draft = draftCount,
                partnerSigned = partnerSignedCount,
                approved = approvedCount,
                settled = settledCount,
                rejected = rejectedCount,
                cancelled = cancelledCount
            },
            recentRequests = requests.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new
            {
                r.Id,
                r.DiscountNo,
                r.PartnerCode,
                r.PartnerName,
                r.TotalPaymentAmount,
                r.TotalDiscountAmount,
                r.NetPaymentAmount,
                status = r.Status.ToString(),
                r.CreatedAt
            })
        };
    }
}

public record DiscountItemInputDto(
    string? ItemRefNo,
    string? Description,
    DateTime DueDate,
    DateTime ActualPaymentDate,
    long OriginalAmount,
    decimal? AnnualDiscountRate,
    string? Note
);
