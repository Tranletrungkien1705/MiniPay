using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hồ sơ & Công văn Đòi Tiền Bảo Lãnh Thanh Toán Ngân Hàng (Bank Guarantee Default Claim Management).
/// Tương ứng khối Pmt_GrtClaim, Pmt_GrtClaimDetail và FrmMngGrtClaim, FrmNewGrtClaim, Pmt_GrtClaimCreate_Multi_New20190312, CR_ClaimPM trong BizHTC.Payment.
/// </summary>
public sealed class BankGuaranteeClaimService(AppDbContext db)
{
    /// <summary>
    /// Lập công văn đòi tiền bảo lãnh ngân hàng mới (Pmt_GrtClaimCreate_Multi_New20190312 / FrmNewGrtClaim).
    /// </summary>
    public async Task<BankGuaranteeClaim> CreateClaimAsync(
        Guid orgId,
        string? claimNo,
        string dealerCode,
        string? dealerName,
        string bankCode,
        string? bankName,
        string? bankCodeMonitor,
        string flagIsHTC,
        string? remark,
        string? createdBy,
        List<GuaranteeClaimItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(dealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Mã ngân hàng phát hành thư bảo lãnh (BankCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Hồ sơ đòi bảo lãnh cần ít nhất 1 dòng xe ô tô vi phạm cam kết thanh toán.");

        var finalNo = string.IsNullOrWhiteSpace(claimNo)
            ? $"CVDBL-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}"
            : claimNo.Trim().ToUpper();

        var exists = await db.GuaranteeClaims.AnyAsync(c => c.OrgId == orgId && c.ClaimNo == finalNo);
        if (exists)
            throw new InvalidOperationException($"Số công văn đòi bảo lãnh '{finalNo}' đã tồn tại trên hệ thống.");

        var cleanBankCode = bankCode.Trim().ToUpper();

        // Kiểm tra tính duy nhất của từng VIN trong cùng 1 công văn
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung xe (VIN) không được để trống.");

            var cleanVin = it.VIN.Trim().ToUpper();
            if (!vinSet.Add(cleanVin))
                throw new ArgumentException($"Số khung VIN '{cleanVin}' bị trùng lặp trong cùng công văn đòi bảo lãnh.");

            // Guard: Tất cả xe trong công văn phải thuộc cùng 1 ngân hàng bảo lãnh (Pmt_GrtClaimOtherBank)
            if (!string.IsNullOrWhiteSpace(it.BankCode) && !string.Equals(it.BankCode.Trim(), cleanBankCode, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Dòng xe VIN '{cleanVin}' có mã ngân hàng bảo lãnh '{it.BankCode}' không khớp với ngân hàng đòi nợ của công văn '{cleanBankCode}' (Pmt_GrtClaimOtherBank).");
        }

        // Guard: Kiểm tra xe (VIN) không thuộc một công văn đòi bảo lãnh khác đang có hiệu lực (VinSignStatus not in Cancelled/BankRejected)
        var activeClaims = await db.GuaranteeClaimDetails
            .Where(d => d.OrgId == orgId && vinSet.Contains(d.VIN) &&
                        d.Status != GuaranteeClaimDetailStatus.Cancelled &&
                        d.Status != GuaranteeClaimDetailStatus.BankRejected)
            .Select(d => new { d.VIN, d.ClaimId })
            .ToListAsync();

        if (activeClaims.Count > 0)
        {
            var conflict = activeClaims.First();
            var parent = await db.GuaranteeClaims.FirstOrDefaultAsync(c => c.Id == conflict.ClaimId);
            throw new InvalidOperationException($"Số khung xe VIN '{conflict.VIN}' đã nằm trong công văn đòi bảo lãnh '{parent?.ClaimNo ?? conflict.ClaimId.ToString()}' đang được xử lý.");
        }

        var details = new List<BankGuaranteeClaimDetail>();
        foreach (var item in items)
        {
            var detail = BuildDetailItem(orgId, item);
            details.Add(detail);
        }

        var claim = new BankGuaranteeClaim
        {
            OrgId = orgId,
            ClaimNo = finalNo,
            DealerCode = dealerCode.Trim().ToUpper(),
            DealerName = string.IsNullOrWhiteSpace(dealerName) ? dealerCode.Trim().ToUpper() : dealerName.Trim(),
            BankCode = cleanBankCode,
            BankName = string.IsNullOrWhiteSpace(bankName) ? ResolveBankName(cleanBankCode) : bankName.Trim(),
            BankCodeMonitor = bankCodeMonitor?.Trim().ToUpper(),
            FlagIsHTC = string.IsNullOrWhiteSpace(flagIsHTC) ? "1" : flagIsHTC.Trim(),
            TotalCarCount = details.Count,
            TotalClaimAmount = details.Sum(d => d.GrtValue),
            SettledAmount = 0,
            Status = GuaranteeClaimStatus.Draft,
            SignCAStatus = ClaimSignCAStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "ChuyenVienTinDungHTC" : createdBy.Trim(),
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.GuaranteeClaims.Add(claim);
        await db.SaveChangesAsync();

        return claim;
    }

    /// <summary>
    /// Trình hồ sơ công văn lên Ban Pháp chế &amp; Quản lý Rủi ro Tín dụng thẩm định (Draft -> Submitted).
    /// </summary>
    public async Task<BankGuaranteeClaim?> SubmitForReviewAsync(long id, Guid orgId, string? submittedBy)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status != GuaranteeClaimStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể trình thẩm định công văn ở trạng thái Mới tạo (Draft). Trạng thái hiện tại: {claim.Status}.");

        claim.Status = GuaranteeClaimStatus.Submitted;
        if (!string.IsNullOrWhiteSpace(submittedBy))
            claim.Remark = string.IsNullOrWhiteSpace(claim.Remark)
                ? $"Trình thẩm định bởi: {submittedBy.Trim()}"
                : $"{claim.Remark} | Trình duyệt: {submittedBy.Trim()}";

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Phê duyệt &amp; Ký số điện tử CA phát hành công văn chính thức (Pmt_GrtClaim_SignAndSendEmail / Submitted -> SignedCA).
    /// </summary>
    public async Task<BankGuaranteeClaim?> SignAndIssueCAAsync(
        long id,
        Guid orgId,
        string? signedBy,
        string? certThumbprint,
        string? filePath)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status != GuaranteeClaimStatus.Submitted && claim.Status != GuaranteeClaimStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể ký số công văn ở trạng thái Đã trình duyệt hoặc Dự thảo. Hiện tại: {claim.Status}.");

        claim.Status = GuaranteeClaimStatus.SignedCA;
        claim.SignCAStatus = ClaimSignCAStatus.Signed;
        claim.SignedBy = string.IsNullOrWhiteSpace(signedBy) ? "GiamDocTaiChinhHTC" : signedBy.Trim();
        claim.SignedAt = DateTime.Now;
        claim.CertThumbprint = string.IsNullOrWhiteSpace(certThumbprint) ? Guid.NewGuid().ToString("N")[..16].ToUpper() : certThumbprint.Trim();
        claim.FilePath = string.IsNullOrWhiteSpace(filePath) ? $"/reports/guarantee-claims/{claim.ClaimNo}.pdf" : filePath.Trim();

        foreach (var dtl in claim.Details)
        {
            dtl.Status = GuaranteeClaimDetailStatus.Claimed;
        }

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Gửi công văn đòi bảo lãnh tới Hội sở Ngân hàng bảo lãnh (SignedCA -> SentToBank).
    /// </summary>
    public async Task<BankGuaranteeClaim?> SendToBankAsync(
        long id,
        Guid orgId,
        string? sentBy,
        string? bankRefNo)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status != GuaranteeClaimStatus.SignedCA)
            throw new InvalidOperationException($"Chỉ có thể gửi ngân hàng khi công văn đã được ký số điện tử CA (SignedCA). Hiện tại: {claim.Status}.");

        claim.Status = GuaranteeClaimStatus.SentToBank;
        claim.SentToBankAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(bankRefNo))
            claim.BankRefNo = bankRefNo.Trim();

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Ngân hàng chi trả thanh toán bồi hoàn bảo lãnh thành công (SentToBank -> Settled).
    /// Ghi nhận số tiền thu hồi, mã bút toán ngân hàng và đồng bộ cập nhật Thư bảo lãnh gốc nếu có.
    /// </summary>
    public async Task<BankGuaranteeClaim?> SettleClaimAsync(
        long id,
        Guid orgId,
        long settledAmount,
        string? bankTxnRef,
        string? settledBy)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status != GuaranteeClaimStatus.SentToBank && claim.Status != GuaranteeClaimStatus.SignedCA)
            throw new InvalidOperationException($"Chỉ có thể quyết toán bồi hoàn khi công văn đã gửi ngân hàng hoặc đã ký CA. Hiện tại: {claim.Status}.");

        claim.Status = GuaranteeClaimStatus.Settled;
        claim.SettledAmount = settledAmount <= 0 ? claim.TotalClaimAmount : settledAmount;
        claim.SettledAt = DateTime.Now;
        claim.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "KeToanThanhToanHTC" : settledBy.Trim();
        claim.BankTxnRef = string.IsNullOrWhiteSpace(bankTxnRef) ? $"FT{DateTime.Now:yyyyMMdd}{Random.Shared.Next(10000, 99999)}" : bankTxnRef.Trim();

        foreach (var dtl in claim.Details)
        {
            dtl.Status = GuaranteeClaimDetailStatus.Settled;

            // Đồng bộ trạng thái Thư bảo lãnh gốc trên hệ thống
            if (!string.IsNullOrWhiteSpace(dtl.BankGuaranteeNo))
            {
                var relatedGrt = await db.Guarantees
                    .Include(g => g.Details)
                    .FirstOrDefaultAsync(g => g.OrgId == orgId && g.BankGuaranteeNo == dtl.BankGuaranteeNo);

                if (relatedGrt != null)
                {
                    relatedGrt.ClaimedAmount = (relatedGrt.ClaimedAmount ?? 0) + dtl.GrtValue;
                    var matchVin = relatedGrt.Details.FirstOrDefault(d => string.Equals(d.ItemRefNo, dtl.VIN, StringComparison.OrdinalIgnoreCase));
                    if (matchVin != null)
                    {
                        matchVin.Status = GuaranteeDetailStatus.Paid;
                    }
                }
            }
        }

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Ngân hàng từ chối chi trả bồi hoàn bảo lãnh kèm lý do giải trình (SentToBank -> BankRejected).
    /// </summary>
    public async Task<BankGuaranteeClaim?> BankRejectAsync(long id, Guid orgId, string reason, string? rejectedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Lý do ngân hàng từ chối chi trả bồi hoàn không được để trống.");

        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status != GuaranteeClaimStatus.SentToBank && claim.Status != GuaranteeClaimStatus.SignedCA)
            throw new InvalidOperationException($"Chỉ có thể ghi nhận ngân hàng từ chối khi công văn đã gửi hoặc ký số. Hiện tại: {claim.Status}.");

        claim.Status = GuaranteeClaimStatus.BankRejected;
        claim.BankRejectReason = reason.Trim();

        foreach (var dtl in claim.Details)
        {
            dtl.Status = GuaranteeClaimDetailStatus.BankRejected;
        }

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Hủy công văn đòi bảo lãnh (Pmt_GrtClaim_Cancel / FrmNewGrtClaim.btnCancel) khi đại lý đã nộp tiền trực tiếp.
    /// </summary>
    public async Task<BankGuaranteeClaim?> CancelClaimAsync(long id, Guid orgId, string? cancelReason, string? cancelledBy)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        if (claim.Status == GuaranteeClaimStatus.Settled)
            throw new InvalidOperationException("Không thể hủy công văn đòi nợ khi ngân hàng đã chính thức bồi hoàn tất toán.");

        claim.Status = GuaranteeClaimStatus.Cancelled;
        claim.CancelledAt = DateTime.Now;
        claim.CancelReason = cancelReason?.Trim();
        if (!string.IsNullOrWhiteSpace(cancelledBy))
            claim.Remark = string.IsNullOrWhiteSpace(claim.Remark)
                ? $"Hủy bởi: {cancelledBy.Trim()}"
                : $"{claim.Remark} | Hủy bởi: {cancelledBy.Trim()}";

        foreach (var dtl in claim.Details)
        {
            dtl.Status = GuaranteeClaimDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Bổ sung xe vào công văn đòi nợ khi đang ở trạng thái Draft hoặc Submitted (FrmNewGrtClaim.btnAddCar).
    /// </summary>
    public async Task<BankGuaranteeClaim> AddVehicleAsync(long id, Guid orgId, GuaranteeClaimItemInputDto item)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy công văn #{id}.");

        if (claim.Status != GuaranteeClaimStatus.Draft && claim.Status != GuaranteeClaimStatus.Submitted)
            throw new InvalidOperationException("Chỉ có thể thêm xe khi công văn ở trạng thái Mới tạo (Draft) hoặc Đang trình duyệt (Submitted).");

        if (string.IsNullOrWhiteSpace(item.VIN))
            throw new ArgumentException("Số khung xe (VIN) không được để trống.");

        var cleanVin = item.VIN.Trim().ToUpper();
        if (claim.Details.Any(d => string.Equals(d.VIN, cleanVin, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Số khung VIN '{cleanVin}' đã tồn tại trong công văn này.");

        // Guard: Kiểm tra trùng xe trong các công văn đang hiệu lực khác
        var existsOther = await db.GuaranteeClaimDetails.AnyAsync(d =>
            d.OrgId == orgId &&
            d.ClaimId != id &&
            d.VIN == cleanVin &&
            d.Status != GuaranteeClaimDetailStatus.Cancelled &&
            d.Status != GuaranteeClaimDetailStatus.BankRejected);

        if (existsOther)
            throw new InvalidOperationException($"Số khung VIN '{cleanVin}' đã nằm trong một công văn đòi nợ khác đang hiệu lực.");

        var detail = BuildDetailItem(orgId, item);
        claim.Details.Add(detail);

        claim.TotalCarCount = claim.Details.Count;
        claim.TotalClaimAmount = claim.Details.Sum(d => d.GrtValue);

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Xóa bớt xe khỏi công văn khi đang ở trạng thái Draft hoặc Submitted (FrmNewGrtClaim.btnRemoveCar).
    /// </summary>
    public async Task<BankGuaranteeClaim> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy công văn #{id}.");

        if (claim.Status != GuaranteeClaimStatus.Draft && claim.Status != GuaranteeClaimStatus.Submitted)
            throw new InvalidOperationException("Chỉ có thể bớt xe khi công văn ở trạng thái Mới tạo (Draft) hoặc Đang trình duyệt (Submitted).");

        var detail = claim.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong công văn.");

        claim.Details.Remove(detail);
        db.GuaranteeClaimDetails.Remove(detail);

        claim.TotalCarCount = claim.Details.Count;
        claim.TotalClaimAmount = claim.Details.Sum(d => d.GrtValue);

        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>
    /// Xóa hẳn hồ sơ công văn nháp chưa ký số (GrtClaimDelete_New20181115).
    /// </summary>
    public async Task DeleteDraftAsync(long id, Guid orgId)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy công văn #{id}.");

        if (claim.Status != GuaranteeClaimStatus.Draft && claim.Status != GuaranteeClaimStatus.Cancelled)
            throw new InvalidOperationException("Chỉ có thể xóa công văn ở trạng thái Mới tạo (Draft) hoặc Đã hủy (Cancelled).");

        if (claim.SignCAStatus == ClaimSignCAStatus.Signed)
            throw new InvalidOperationException("Không thể xóa công văn đã ký số điện tử CA.");

        db.GuaranteeClaimDetails.RemoveRange(claim.Details);
        db.GuaranteeClaims.Remove(claim);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Lấy danh sách công văn đòi bảo lãnh kèm bộ lọc tìm kiếm (Pmt_GrtClaimGet).
    /// </summary>
    public async Task<List<BankGuaranteeClaim>> GetClaimsAsync(
        Guid orgId,
        string? dealerCode = null,
        string? bankCode = null,
        string? status = null,
        string? flagIsHTC = null,
        string? vin = null)
    {
        var query = db.GuaranteeClaims
            .Include(c => c.Details)
            .Where(c => c.OrgId == orgId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dealerCode))
            query = query.Where(c => c.DealerCode == dealerCode.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(bankCode))
            query = query.Where(c => c.BankCode == bankCode.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GuaranteeClaimStatus>(status, true, out var st))
            query = query.Where(c => c.Status == st);

        if (!string.IsNullOrWhiteSpace(flagIsHTC))
            query = query.Where(c => c.FlagIsHTC == flagIsHTC.Trim());

        if (!string.IsNullOrWhiteSpace(vin))
        {
            var cleanVin = vin.Trim().ToUpper();
            query = query.Where(c => c.Details.Any(d => d.VIN.Contains(cleanVin)));
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết 1 hồ sơ công văn kèm danh mục xe đòi bảo lãnh.
    /// </summary>
    public async Task<BankGuaranteeClaim?> GetClaimByIdAsync(long id, Guid orgId)
    {
        return await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);
    }

    /// <summary>
    /// Tự động quét các xe bảo lãnh đã quá hạn thanh toán trong hệ thống, gom nhóm theo cặp (DealerCode, BankCode) để gợi ý lập công văn đòi tiền bảo lãnh.
    /// </summary>
    public async Task<List<AutoScanClaimCandidateDto>> AutoScanOverdueGuaranteesAsync(Guid orgId, int minOverdueDays = 1)
    {
        var today = DateTime.Today;

        // Lấy danh sách số khung đã nằm trong công văn đòi nợ đang hoạt động
        var activeClaimedVins = await db.GuaranteeClaimDetails
            .Where(d => d.OrgId == orgId &&
                        d.Status != GuaranteeClaimDetailStatus.Cancelled &&
                        d.Status != GuaranteeClaimDetailStatus.BankRejected)
            .Select(d => d.VIN)
            .Distinct()
            .ToListAsync();

        var claimedVinSet = new HashSet<string>(activeClaimedVins, StringComparer.OrdinalIgnoreCase);

        // Quét các Thư bảo lãnh có hạn hết hạn trước ngày hôm nay
        var overdueGuarantees = await db.Guarantees
            .Include(g => g.Details)
            .Where(g => g.OrgId == orgId &&
                        (g.Status == GuaranteeStatus.Active || g.Status == GuaranteeStatus.PendingApproval) &&
                        g.DateExpired < today)
            .ToListAsync();

        var candidates = new List<AutoScanClaimCandidateDto>();

        var grouped = overdueGuarantees
            .GroupBy(g => new { g.PartnerCode, g.BankCode });

        foreach (var grp in grouped)
        {
            var dealerCode = grp.Key.PartnerCode;
            var bankCode = grp.Key.BankCode;
            var dealerName = grp.First().PartnerName;
            var bankName = grp.First().BankName;

            var items = new List<GuaranteeClaimItemInputDto>();

            foreach (var grt in grp)
            {
                var overdueDays = (int)(today - grt.DateExpired).TotalDays;
                if (overdueDays < minOverdueDays) continue;

                foreach (var dtl in grt.Details.Where(d => d.Status == GuaranteeDetailStatus.Active))
                {
                    if (claimedVinSet.Contains(dtl.ItemRefNo)) continue;

                    var unitPrice = dtl.OrderAmount > 0 ? dtl.OrderAmount : dtl.GuaranteeValue;
                    var grtVal = dtl.GuaranteeValue > 0 ? dtl.GuaranteeValue : unitPrice;

                    items.Add(new GuaranteeClaimItemInputDto(
                        CarId: $"CAR-{dtl.Id}",
                        VIN: dtl.ItemRefNo,
                        ModelCode: string.IsNullOrWhiteSpace(dtl.Description) ? "HYUNDAI" : dtl.Description,
                        ModelName: dtl.Description,
                        SpecCode: "STD",
                        SpecDescription: "Phiên bản tiêu chuẩn",
                        ColorName: "Trắng/Đen",
                        SOCode: grt.ContractNo,
                        ContractNo: grt.ContractNo,
                        GuaranteeNo: grt.GuaranteeNo,
                        BankGuaranteeNo: grt.BankGuaranteeNo,
                        BankCode: grt.BankCode,
                        DateOpen: dtl.DateStart != default ? dtl.DateStart : grt.DateOpen,
                        DateExpired: dtl.DateEnd != default ? dtl.DateEnd : grt.DateExpired,
                        OverdueDays: overdueDays,
                        UnitPriceActual: unitPrice,
                        GrtValue: grtVal,
                        GrtPercent: (double)dtl.GuaranteePercent,
                        Remark: $"Quá hạn nợ bảo lãnh {overdueDays} ngày (Hết hạn: {grt.DateExpired:dd/MM/yyyy})"
                    ));
                }
            }

            if (items.Count > 0)
            {
                candidates.Add(new AutoScanClaimCandidateDto(
                    DealerCode: dealerCode,
                    DealerName: dealerName,
                    BankCode: bankCode,
                    BankName: bankName,
                    OverdueVehicleCount: items.Count,
                    TotalOverdueClaimAmount: items.Sum(i => i.GrtValue),
                    SuggestedClaimNo: $"CVDBL-{DateTime.Now:yyyyMMdd}-{dealerCode}-{bankCode}",
                    SuggestedVehicles: items
                ));
            }
        }

        return candidates;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Công văn Đòi Tiền Bảo Lãnh Ngân Hàng (CR_ClaimPM Advice) gửi ngân hàng tài trợ kèm đọc số tiền bằng chữ tiếng Việt chuẩn quy chuẩn tài chính ngân hàng.
    /// </summary>
    public async Task<BankClaimAdviceDto?> GenerateClaimAdviceAsync(long id, Guid orgId)
    {
        var claim = await db.GuaranteeClaims
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrgId == orgId);

        if (claim == null) return null;

        var amountInWords = NumberToVietnameseWords(claim.TotalClaimAmount);
        var phapNhan = claim.FlagIsHTC == "2"
            ? "CÔNG TY CỔ PHẦN HYUNDAI THÀNH CÔNG THƯƠNG MẠI (HTV)"
            : "CÔNG TY CỔ PHẦN HYUNDAI THÀNH CÔNG VIỆT NAM (HTC)";
        var subNo = claim.FlagIsHTC == "2" ? $"{claim.ClaimNo}/CV-HTCVN" : $"{claim.ClaimNo}/CV-HTC";

        return new BankClaimAdviceDto(
            ClaimNo: claim.ClaimNo,
            ClaimSubNo: subNo,
            CompanyLegalName: phapNhan,
            DateCreated: claim.CreatedAt.ToString("dd/MM/yyyy"),
            DealerCode: claim.DealerCode,
            DealerName: claim.DealerName,
            BankCode: claim.BankCode,
            BankName: claim.BankName,
            BankCodeMonitor: claim.BankCodeMonitor ?? "VPBANK_HO",
            TotalCarCount: claim.TotalCarCount,
            TotalClaimAmount: claim.TotalClaimAmount,
            AmountInWords: amountInWords,
            SettledAmount: claim.SettledAmount,
            Status: claim.Status.ToString(),
            SignCAStatus: claim.SignCAStatus.ToString(),
            SignedBy: claim.SignedBy,
            SignedDate: claim.SignedAt?.ToString("dd/MM/yyyy HH:mm"),
            CertThumbprint: claim.CertThumbprint,
            SentToBankDate: claim.SentToBankAt?.ToString("dd/MM/yyyy HH:mm"),
            BankRefNo: claim.BankRefNo,
            SettledDate: claim.SettledAt?.ToString("dd/MM/yyyy HH:mm"),
            SettledBy: claim.SettledBy,
            BankTxnRef: claim.BankTxnRef,
            BankRejectReason: claim.BankRejectReason,
            CancelReason: claim.CancelReason,
            Remark: claim.Remark,
            Vehicles: claim.Details.OrderByDescending(d => d.OverdueDays).Select(d => new BankClaimAdviceVehicleItemDto(
                VIN: d.VIN,
                ModelCode: d.ModelCode,
                ModelName: d.ModelName ?? d.ModelCode,
                SpecDescription: d.SpecDescription ?? "Bản tiêu chuẩn",
                ColorName: d.ColorName ?? "Tiêu chuẩn",
                ContractNo: d.ContractNo ?? "-",
                BankGuaranteeNo: d.BankGuaranteeNo,
                DateOpen: d.DateOpen.ToString("dd/MM/yyyy"),
                DateExpired: d.DateExpired.ToString("dd/MM/yyyy"),
                OverdueDays: d.OverdueDays,
                UnitPriceActual: d.UnitPriceActual,
                GrtValue: d.GrtValue,
                GrtPercent: d.GrtPercent,
                StatusText: d.Status switch
                {
                    GuaranteeClaimDetailStatus.Settled => "Đã thu hồi bồi hoàn",
                    GuaranteeClaimDetailStatus.Claimed => "Đang đòi ngân hàng",
                    GuaranteeClaimDetailStatus.BankRejected => "Ngân hàng từ chối",
                    GuaranteeClaimDetailStatus.Cancelled => "Đã hủy đòi nợ",
                    _ => "Chờ xử lý"
                }
            )).ToList()
        );
    }

    /// <summary>
    /// Báo cáo thống kê tổng hợp dashboard công văn đòi bảo lãnh ngân hàng.
    /// </summary>
    public async Task<BankGuaranteeClaimSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var claims = await db.GuaranteeClaims
            .Include(c => c.Details)
            .Where(c => c.OrgId == orgId)
            .ToListAsync();

        var totalClaims = claims.Count;
        var totalClaimAmount = claims.Sum(c => c.TotalClaimAmount);
        var totalSettledAmount = claims.Sum(c => c.SettledAmount);
        var totalCarCount = claims.Sum(c => c.TotalCarCount);

        var draftCount = claims.Count(c => c.Status == GuaranteeClaimStatus.Draft);
        var submittedCount = claims.Count(c => c.Status == GuaranteeClaimStatus.Submitted);
        var signedCount = claims.Count(c => c.Status == GuaranteeClaimStatus.SignedCA);
        var sentToBankCount = claims.Count(c => c.Status == GuaranteeClaimStatus.SentToBank);
        var settledCount = claims.Count(c => c.Status == GuaranteeClaimStatus.Settled);
        var rejectedCount = claims.Count(c => c.Status == GuaranteeClaimStatus.BankRejected);
        var cancelledCount = claims.Count(c => c.Status == GuaranteeClaimStatus.Cancelled);

        var activeDemandsAmount = claims
            .Where(c => c.Status == GuaranteeClaimStatus.SignedCA || c.Status == GuaranteeClaimStatus.SentToBank)
            .Sum(c => c.TotalClaimAmount);

        return new BankGuaranteeClaimSummaryDto(
            TotalClaims: totalClaims,
            TotalClaimAmount: totalClaimAmount,
            TotalSettledAmount: totalSettledAmount,
            ActiveDemandsAmount: activeDemandsAmount,
            TotalCarCount: totalCarCount,
            DraftCount: draftCount,
            SubmittedCount: submittedCount,
            SignedCount: signedCount,
            SentToBankCount: sentToBankCount,
            SettledCount: settledCount,
            RejectedCount: rejectedCount,
            CancelledCount: cancelledCount
        );
    }

    private static BankGuaranteeClaimDetail BuildDetailItem(Guid orgId, GuaranteeClaimItemInputDto item)
    {
        var cleanVin = item.VIN.Trim().ToUpper();
        var overdue = item.OverdueDays;
        if (overdue <= 0 && item.DateExpired != default)
        {
            overdue = Math.Max(0, (int)(DateTime.Today - item.DateExpired).TotalDays);
        }

        return new BankGuaranteeClaimDetail
        {
            OrgId = orgId,
            CarId = item.CarId?.Trim(),
            VIN = cleanVin,
            ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
            ModelName = item.ModelName?.Trim(),
            SpecCode = item.SpecCode?.Trim(),
            SpecDescription = item.SpecDescription?.Trim(),
            ColorName = item.ColorName?.Trim(),
            SOCode = item.SOCode?.Trim(),
            ContractNo = item.ContractNo?.Trim(),
            GuaranteeNo = item.GuaranteeNo?.Trim(),
            BankGuaranteeNo = string.IsNullOrWhiteSpace(item.BankGuaranteeNo) ? "BLNH-DEFAULT" : item.BankGuaranteeNo.Trim().ToUpper(),
            DateOpen = item.DateOpen == default ? DateTime.Today.AddDays(-60) : item.DateOpen,
            DateExpired = item.DateExpired == default ? DateTime.Today.AddDays(-15) : item.DateExpired,
            OverdueDays = overdue,
            UnitPriceActual = item.UnitPriceActual > 0 ? item.UnitPriceActual : item.GrtValue,
            GrtValue = item.GrtValue > 0 ? item.GrtValue : item.UnitPriceActual,
            GrtPercent = item.GrtPercent > 0 ? item.GrtPercent : 100.0,
            Status = GuaranteeClaimDetailStatus.Pending,
            Remark = item.Remark?.Trim()
        };
    }

    private static string ResolveBankName(string bankCode) => bankCode.ToUpper() switch
    {
        "VCB" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
        "CTG" => "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
        "TCB" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
        "MBB" => "Ngân hàng TMCP Quân Đội (MBBank)",
        "VPB" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
        "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
        "AGR" => "Ngân hàng Nông nghiệp và PTNT Việt Nam (Agribank)",
        "ACB" => "Ngân hàng TMCP Á Châu (ACB)",
        "STB" => "Ngân hàng TMCP Sài Gòn Thương Tín (Sacombank)",
        _ => $"Ngân hàng TMCP {bankCode}"
    };

    /// <summary>
    /// Thuật toán chuyển đổi số tiền thành chữ tiếng Việt chuẩn quy chuẩn tài chính ngân hàng Việt Nam.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(Math.Abs(number));

        string[] units = ["", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ"];
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

        var result = "";
        var groupIndex = 0;
        var temp = number;

        while (temp > 0)
        {
            var threeDigits = (int)(temp % 1000);
            if (threeDigits > 0)
            {
                var groupStr = ReadThreeDigits(threeDigits, digits, temp >= 1000);
                var unit = units[groupIndex];
                result = string.IsNullOrWhiteSpace(unit) ? $"{groupStr} {result}" : $"{groupStr} {unit} {result}";
            }
            temp /= 1000;
            groupIndex++;
        }

        result = result.Trim();
        if (result.Length > 0)
        {
            result = char.ToUpper(result[0]) + result[1..] + " đồng chẵn./.";
        }
        return result;
    }

    private static string ReadThreeDigits(int n, string[] digits, bool full)
    {
        var h = n / 100;
        var t = (n % 100) / 10;
        var u = n % 10;
        var s = "";

        if (h > 0 || full)
        {
            s += digits[h] + " trăm";
            if (t == 0 && u > 0) s += " lẻ";
        }

        if (t > 1)
        {
            s += " " + digits[t] + " mươi";
            if (u == 1) s += " mốt";
            else if (u == 5) s += " lăm";
            else if (u > 0) s += " " + digits[u];
        }
        else if (t == 1)
        {
            s += " mười";
            if (u == 1) s += " một";
            else if (u == 5) s += " lăm";
            else if (u > 0) s += " " + digits[u];
        }
        else if (u > 0)
        {
            s += " " + digits[u];
        }

        return s.Trim();
    }
}

// DTO records
public record GuaranteeClaimItemInputDto(
    string? CarId,
    string VIN,
    string ModelCode,
    string? ModelName,
    string? SpecCode,
    string? SpecDescription,
    string? ColorName,
    string? SOCode,
    string? ContractNo,
    string? GuaranteeNo,
    string BankGuaranteeNo,
    string? BankCode,
    DateTime DateOpen,
    DateTime DateExpired,
    int OverdueDays,
    long UnitPriceActual,
    long GrtValue,
    double GrtPercent,
    string? Remark
);

public record AutoScanClaimCandidateDto(
    string DealerCode,
    string DealerName,
    string BankCode,
    string BankName,
    int OverdueVehicleCount,
    long TotalOverdueClaimAmount,
    string SuggestedClaimNo,
    List<GuaranteeClaimItemInputDto> SuggestedVehicles
);

public record BankClaimAdviceDto(
    string ClaimNo,
    string ClaimSubNo,
    string CompanyLegalName,
    string DateCreated,
    string DealerCode,
    string DealerName,
    string BankCode,
    string BankName,
    string BankCodeMonitor,
    int TotalCarCount,
    long TotalClaimAmount,
    string AmountInWords,
    long SettledAmount,
    string Status,
    string SignCAStatus,
    string? SignedBy,
    string? SignedDate,
    string? CertThumbprint,
    string? SentToBankDate,
    string? BankRefNo,
    string? SettledDate,
    string? SettledBy,
    string? BankTxnRef,
    string? BankRejectReason,
    string? CancelReason,
    string? Remark,
    List<BankClaimAdviceVehicleItemDto> Vehicles
);

public record BankClaimAdviceVehicleItemDto(
    string VIN,
    string ModelCode,
    string ModelName,
    string SpecDescription,
    string ColorName,
    string ContractNo,
    string BankGuaranteeNo,
    string DateOpen,
    string DateExpired,
    int OverdueDays,
    long UnitPriceActual,
    long GrtValue,
    double GrtPercent,
    string StatusText
);

public record BankGuaranteeClaimSummaryDto(
    int TotalClaims,
    long TotalClaimAmount,
    long TotalSettledAmount,
    long ActiveDemandsAmount,
    int TotalCarCount,
    int DraftCount,
    int SubmittedCount,
    int SignedCount,
    int SentToBankCount,
    int SettledCount,
    int RejectedCount,
    int CancelledCount
);
