using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ quản lý Thế chấp & Giải chấp tài sản ngân hàng (Bank Collateral Mortgage & Redemption Management).
/// Tương ứng khối RM_ReqMortgage, RM_ReqMortgageDtl, RD_ReqRedeem, RD_ReqRedeemDtl trong BizHTC.GiaiChap
/// và các màn hình FrmMngRM_ReqMortgage, FrmNewRM_ReqMortgage, FrmMngRedeem, FrmNewRedeem.
/// </summary>
public sealed class MortgageRedeemService(AppDbContext db)
{
    // ==========================================
    // 1. NGHIỆP VỤ THẾ CHẤP NGÂN HÀNG (MORTGAGE)
    // ==========================================

    /// <summary>
    /// Lập hồ sơ đề nghị thế chấp tài sản / kho xe vay ngân hàng (RM_ReqMortgage_Create trong BizHTC.GiaiChap).
    /// </summary>
    public async Task<MortgageRequest> CreateMortgageRequestAsync(
        Guid orgId,
        string? reqRMNo,
        string bankCode,
        string? bankName,
        string partnerCode,
        string? partnerName,
        string? creditContractNo,
        DateTime? mortgageDate,
        decimal? interestRate,
        int? loanPeriodDays,
        string? remark,
        List<MortgageItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Mã ngân hàng nhận thế chấp (BankCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đơn vị / đại lý thế chấp (PartnerCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Hồ sơ thế chấp phải có ít nhất 1 tài sản / xe bảo đảm.");

        var finalReqNo = string.IsNullOrWhiteSpace(reqRMNo)
            ? $"RM-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
            : reqRMNo.Trim().ToUpper();

        var exists = await db.MortgageRequests.AnyAsync(r => r.OrgId == orgId && r.ReqRMNo == finalReqNo);
        if (exists)
            throw new InvalidOperationException($"Số đề nghị thế chấp '{finalReqNo}' đã tồn tại.");

        var bankNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CTG"] = "VietinBank — Ngân hàng TMCP Công Thương Việt Nam",
            ["MBB"] = "MBBank — Ngân hàng TMCP Quân Đội",
            ["TCB"] = "Techcombank — Ngân hàng TMCP Kỹ Thương Việt Nam",
            ["VCB"] = "Vietcombank — Ngân hàng TMCP Ngoại Thương Việt Nam",
            ["BIDV"] = "BIDV — Ngân hàng TMCP Đầu tư và Phát triển Việt Nam",
            ["VPB"] = "VPBank — Ngân hàng TMCP Việt Nam Thịnh Vượng"
        };
        var normalizedBankCode = bankCode.Trim().ToUpper();
        var finalBankName = !string.IsNullOrWhiteSpace(bankName)
            ? bankName.Trim()
            : bankNames.TryGetValue(normalizedBankCode, out var bName) ? bName : $"Ngân hàng {normalizedBankCode}";

        // Kiểm tra trùng lặp VIN trong cùng đề nghị
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.ItemRefNo))
                throw new ArgumentException("Mã tài sản / Số khung (VIN) không được để trống.");
            var vin = it.ItemRefNo.Trim().ToUpper();
            if (!vinSet.Add(vin))
                throw new ArgumentException($"Số khung VIN '{vin}' bị trùng lặp trong danh sách nộp.");
        }

        // Kiểm tra xe đã đang thế chấp ở hồ sơ khác chưa giải chấp (như BizHTC.GiaiChap Check 1 VIN không có thế chấp ở trạng thái A, P)
        var activeExistingVins = await db.MortgageDetails
            .Where(d => d.OrgId == orgId && (d.Status == MortgageDetailStatus.Pending || d.Status == MortgageDetailStatus.Approved))
            .Select(d => d.ItemRefNo)
            .ToListAsync();
        var activeSet = new HashSet<string>(activeExistingVins, StringComparer.OrdinalIgnoreCase);

        foreach (var it in items)
        {
            var vin = it.ItemRefNo.Trim().ToUpper();
            if (activeSet.Contains(vin))
                throw new InvalidOperationException($"Số khung VIN '{vin}' đang được thế chấp ở một hồ sơ khác chưa giải chấp.");
        }

        long totalCollateral = 0;
        long totalLoan = 0;
        var details = new List<MortgageDetail>();

        int idx = 1;
        foreach (var it in items)
        {
            if (it.CollateralValue <= 0)
                throw new ArgumentException($"Giá trị định giá dòng #{idx} phải > 0.");

            var loanVal = it.LoanAmount > 0 ? it.LoanAmount : (long)Math.Round(it.CollateralValue * 0.70); // Mặc định 70% định giá
            totalCollateral += it.CollateralValue;
            totalLoan += loanVal;

            details.Add(new MortgageDetail
            {
                OrgId = orgId,
                ItemRefNo = it.ItemRefNo.Trim().ToUpper(),
                ModelCode = string.IsNullOrWhiteSpace(it.ModelCode) ? "MODEL-AUTO" : it.ModelCode.Trim().ToUpper(),
                EngineNo = it.EngineNo?.Trim().ToUpper(),
                CQNo = it.CQNo?.Trim(),
                CONo = it.CONo?.Trim(),
                DeclarationNo = it.DeclarationNo?.Trim(),
                CODate = it.CODate ?? DateTime.Today.AddDays(-15),
                CollateralValue = it.CollateralValue,
                LoanAmount = loanVal,
                Status = MortgageDetailStatus.Pending,
                Note = it.Note
            });
            idx++;
        }

        var req = new MortgageRequest
        {
            OrgId = orgId,
            ReqRMNo = finalReqNo,
            BankCode = normalizedBankCode,
            BankName = finalBankName,
            PartnerCode = partnerCode.Trim().ToUpper(),
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? partnerCode.Trim().ToUpper() : partnerName.Trim(),
            CreditContractNo = creditContractNo?.Trim(),
            MortgageDate = mortgageDate ?? DateTime.Today,
            TotalItems = details.Count,
            ActiveItems = 0,
            RedeemedItems = 0,
            TotalCollateralValue = totalCollateral,
            TotalLoanAmount = totalLoan,
            RemainingLoanAmount = totalLoan,
            InterestRate = interestRate.GetValueOrDefault(8.5m),
            LoanPeriodDays = loanPeriodDays.GetValueOrDefault(90),
            Status = MortgageStatus.PendingApproval,
            Remark = remark?.Trim(),
            CreatedBy = "Accountant",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.MortgageRequests.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Phê duyệt hồ sơ thế chấp và phong tỏa tài sản bảo đảm (RM_ReqMortgageDtl_Approve trong BizHTC.GiaiChap).
    /// </summary>
    public async Task<MortgageRequest?> ApproveMortgageRequestAsync(long id, Guid orgId, string? approverName)
    {
        var req = await db.MortgageRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status != MortgageStatus.PendingApproval && req.Status != MortgageStatus.Draft)
            throw new InvalidOperationException($"Chỉ hồ sơ ở trạng thái Chờ duyệt hoặc Nháp mới được phê duyệt (hiện tại: {req.Status}).");

        var approver = string.IsNullOrWhiteSpace(approverName) ? "CreditRiskManager" : approverName.Trim();
        var now = DateTime.Now;

        req.Status = MortgageStatus.Approved;
        req.ApprovedBy = approver;
        req.ApprovedAt = now;
        req.ActiveItems = req.Details.Count;
        req.RedeemedItems = 0;
        req.RemainingLoanAmount = req.TotalLoanAmount;

        foreach (var dtl in req.Details)
        {
            dtl.Status = MortgageDetailStatus.Approved;
            dtl.ApprovedBy = approver;
            dtl.ApprovedAt = now;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Từ chối đề nghị thế chấp.
    /// </summary>
    public async Task<MortgageRequest?> RejectMortgageRequestAsync(long id, Guid orgId, string? reason, string? rejecterName)
    {
        var req = await db.MortgageRequests.FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);
        if (req == null) return null;

        if (req.Status != MortgageStatus.PendingApproval && req.Status != MortgageStatus.Draft)
            throw new InvalidOperationException($"Chỉ hồ sơ ở trạng thái Chờ duyệt hoặc Nháp mới có thể từ chối.");

        req.Status = MortgageStatus.Rejected;
        req.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Không đáp ứng tiêu chuẩn định giá tài sản" : reason.Trim();
        req.ApprovedBy = string.IsNullOrWhiteSpace(rejecterName) ? "BankApprover" : rejecterName.Trim();
        req.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Hủy hồ sơ thế chấp khi chưa phê duyệt.
    /// </summary>
    public async Task<MortgageRequest?> CancelMortgageRequestAsync(long id, Guid orgId, string? reason)
    {
        var req = await db.MortgageRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status == MortgageStatus.Approved || req.Status == MortgageStatus.Finished)
            throw new InvalidOperationException($"Không thể hủy hồ sơ thế chấp đã được ngân hàng duyệt hoặc đã giải chấp.");

        req.Status = MortgageStatus.Cancelled;
        req.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason)) req.Remark = $"{req.Remark} | Lý do hủy: {reason.Trim()}";

        foreach (var dtl in req.Details)
        {
            dtl.Status = MortgageDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return req;
    }

    // ==========================================
    // 2. NGHIỆP VỤ GIẢI CHẤP TÀI SẢN (REDEMPTION)
    // ==========================================

    /// <summary>
    /// Lập đề nghị giải chấp tài sản ngân hàng (RD_ReqRedeem_Create trong BizHTC.GiaiChap).
    /// </summary>
    public async Task<RedeemRequest> CreateRedeemRequestAsync(
        Guid orgId,
        string? reqDMNo,
        string bankCode,
        string? bankName,
        string partnerCode,
        string? partnerName,
        DateTime? redeemDate,
        long? totalSettlementAmount,
        string? paymentProofNo,
        string? remark,
        List<RedeemItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Mã ngân hàng (BankCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đơn vị / đại lý (PartnerCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Đề nghị giải chấp phải có ít nhất 1 tài sản / xe cần giải phóng.");

        var finalReqDMNo = string.IsNullOrWhiteSpace(reqDMNo)
            ? $"DM-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
            : reqDMNo.Trim().ToUpper();

        var exists = await db.RedeemRequests.AnyAsync(r => r.OrgId == orgId && r.ReqDMNo == finalReqDMNo);
        if (exists)
            throw new InvalidOperationException($"Số đề nghị giải chấp '{finalReqDMNo}' đã tồn tại.");

        var normalizedBankCode = bankCode.Trim().ToUpper();

        // Kiểm tra tính hợp lệ của từng tài sản thế chấp cần giải chấp
        var details = new List<RedeemDetail>();
        long calculatedSettlement = 0;

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.ItemRefNo))
                throw new ArgumentException("Mã tài sản / Số khung (VIN) không được để trống.");

            var vin = it.ItemRefNo.Trim().ToUpper();

            // Tìm chi tiết thế chấp gốc đang ở trạng thái Approved (đang thế chấp)
            var mgDetail = await db.MortgageDetails
                .FirstOrDefaultAsync(d => d.OrgId == orgId
                    && d.ItemRefNo == vin
                    && d.Status == MortgageDetailStatus.Approved);

            if (mgDetail == null)
            {
                // Kiểm tra xem đã giải chấp chưa hay không tìm thấy
                var isAlreadyRedeemed = await db.MortgageDetails.AnyAsync(d => d.OrgId == orgId && d.ItemRefNo == vin && d.Status == MortgageDetailStatus.Redeemed);
                if (isAlreadyRedeemed)
                    throw new InvalidOperationException($"Số khung VIN '{vin}' đã được giải chấp trước đó.");
                else
                    throw new InvalidOperationException($"Số khung VIN '{vin}' không tìm thấy trong danh mục tài sản đang thế chấp của ngân hàng.");
            }

            var mgReq = await db.MortgageRequests.FirstOrDefaultAsync(m => m.Id == mgDetail.MortgageRequestId && m.OrgId == orgId);
            var reqRMNo = mgReq?.ReqRMNo ?? it.ReqRMNo ?? "RM-UNKNOWN";
            var settlement = it.SettlementAmount > 0 ? it.SettlementAmount : mgDetail.LoanAmount;
            calculatedSettlement += settlement;

            details.Add(new RedeemDetail
            {
                OrgId = orgId,
                ItemRefNo = vin,
                ReqRMNo = reqRMNo,
                MortgageDetailId = mgDetail.Id,
                ModelCode = mgDetail.ModelCode,
                DealerCode = it.DealerCode?.Trim().ToUpper() ?? partnerCode.Trim().ToUpper(),
                SettlementAmount = settlement,
                Status = RedeemDetailStatus.Pending,
                Note = it.Note
            });
        }

        var finalSettlement = totalSettlementAmount.GetValueOrDefault(calculatedSettlement);

        var req = new RedeemRequest
        {
            OrgId = orgId,
            ReqDMNo = finalReqDMNo,
            BankCode = normalizedBankCode,
            BankName = string.IsNullOrWhiteSpace(bankName) ? $"Ngân hàng {normalizedBankCode}" : bankName.Trim(),
            PartnerCode = partnerCode.Trim().ToUpper(),
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? partnerCode.Trim().ToUpper() : partnerName.Trim(),
            RedeemDate = redeemDate ?? DateTime.Today,
            TotalItems = details.Count,
            ApprovedItems = 0,
            TotalSettlementAmount = finalSettlement,
            PaymentProofNo = paymentProofNo?.Trim(),
            Status = RedeemStatus.PendingApproval,
            Remark = remark?.Trim(),
            CreatedBy = "Accountant",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.RedeemRequests.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Phê duyệt đề nghị giải chấp, cập nhật trạng thái dòng thế chấp gốc và tự động đóng hồ sơ thế chấp nếu tất toán toàn bộ.
    /// (Tương ứng RD_ReqRedeemDtl_Approve & Check Finish RM_ReqMortgage trong BizHTC.GiaiChap).
    /// </summary>
    public async Task<RedeemRequest?> ApproveRedeemRequestAsync(long id, Guid orgId, string? approverName)
    {
        var req = await db.RedeemRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status != RedeemStatus.PendingApproval && req.Status != RedeemStatus.Draft)
            throw new InvalidOperationException($"Chỉ đề nghị ở trạng thái Chờ duyệt mới được phê duyệt (hiện tại: {req.Status}).");

        var approver = string.IsNullOrWhiteSpace(approverName) ? "BankBranchOfficer" : approverName.Trim();
        var now = DateTime.Now;

        // Tập hợp các ID hồ sơ thế chấp gốc bị ảnh hưởng để cập nhật
        var affectedMortgageRequestIds = new HashSet<long>();

        foreach (var rDtl in req.Details)
        {
            rDtl.Status = RedeemDetailStatus.Approved;
            rDtl.ApprovedBy = approver;
            rDtl.ApprovedAt = now;

            // Cập nhật dòng thế chấp gốc
            MortgageDetail? mDtl = null;
            if (rDtl.MortgageDetailId.HasValue)
            {
                mDtl = await db.MortgageDetails.FirstOrDefaultAsync(d => d.Id == rDtl.MortgageDetailId.Value && d.OrgId == orgId);
            }
            if (mDtl == null)
            {
                mDtl = await db.MortgageDetails.FirstOrDefaultAsync(d => d.OrgId == orgId && d.ItemRefNo == rDtl.ItemRefNo && d.Status == MortgageDetailStatus.Approved);
            }

            if (mDtl != null)
            {
                mDtl.Status = MortgageDetailStatus.Redeemed;
                mDtl.ReqDMNo = req.ReqDMNo;
                mDtl.RedeemedAt = now;
                affectedMortgageRequestIds.Add(mDtl.MortgageRequestId);
            }
        }

        req.Status = RedeemStatus.Completed;
        req.ApprovedBy = approver;
        req.ApprovedAt = now;
        req.ApprovedItems = req.Details.Count;

        // Cập nhật lại các MortgageRequest liên quan: số xe đang thế chấp, số xe đã giải chấp, dư nợ vay
        // Và kiểm tra hoàn thành hồ sơ (Check Finish RM_ReqMortgage)
        foreach (var mId in affectedMortgageRequestIds)
        {
            var mReq = await db.MortgageRequests
                .Include(m => m.Details)
                .FirstOrDefaultAsync(m => m.Id == mId && m.OrgId == orgId);

            if (mReq != null)
            {
                mReq.ActiveItems = mReq.Details.Count(d => d.Status == MortgageDetailStatus.Approved);
                mReq.RedeemedItems = mReq.Details.Count(d => d.Status == MortgageDetailStatus.Redeemed);
                mReq.RemainingLoanAmount = mReq.Details
                    .Where(d => d.Status == MortgageDetailStatus.Approved)
                    .Sum(d => d.LoanAmount);

                // Nếu tất cả xe trong hồ sơ thế chấp đã được giải chấp thành công -> Finish RM_ReqMortgage!
                var hasActiveRemaining = mReq.Details.Any(d => d.Status == MortgageDetailStatus.Pending || d.Status == MortgageDetailStatus.Approved);
                if (!hasActiveRemaining && mReq.RedeemedItems > 0)
                {
                    mReq.Status = MortgageStatus.Finished;
                    mReq.FinishedBy = approver;
                    mReq.FinishedAt = now;
                }
            }
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Từ chối đề nghị giải chấp tài sản.
    /// </summary>
    public async Task<RedeemRequest?> RejectRedeemRequestAsync(long id, Guid orgId, string? reason, string? rejecterName)
    {
        var req = await db.RedeemRequests.FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);
        if (req == null) return null;

        if (req.Status != RedeemStatus.PendingApproval && req.Status != RedeemStatus.Draft)
            throw new InvalidOperationException($"Chỉ đề nghị ở trạng thái Chờ duyệt mới có thể từ chối.");

        req.Status = RedeemStatus.Rejected;
        req.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Chưa nhận đủ chứng từ tất toán nợ vay ngân hàng" : reason.Trim();
        req.ApprovedBy = string.IsNullOrWhiteSpace(rejecterName) ? "BankBranchOfficer" : rejecterName.Trim();
        req.ApprovedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Hủy hồ sơ đề nghị giải chấp khi chưa duyệt.
    /// </summary>
    public async Task<RedeemRequest?> CancelRedeemRequestAsync(long id, Guid orgId, string? reason)
    {
        var req = await db.RedeemRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);

        if (req == null) return null;

        if (req.Status == RedeemStatus.Completed)
            throw new InvalidOperationException($"Không thể hủy đề nghị giải chấp đã hoàn tất phê duyệt.");

        req.Status = RedeemStatus.Cancelled;
        req.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason)) req.Remark = $"{req.Remark} | Lý do hủy: {reason.Trim()}";

        foreach (var dtl in req.Details)
        {
            dtl.Status = RedeemDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return req;
    }

    // ==========================================
    // 3. TỔNG HỢP & BÁO CÁO (SUMMARY & STATS)
    // ==========================================

    /// <summary>
    /// Lấy danh sách các tài sản đang ở trạng thái thế chấp (Approved) để chuẩn bị tạo đề nghị giải chấp.
    /// </summary>
    public async Task<List<ActiveMortgagedItemDto>> GetActiveMortgagedItemsAsync(Guid orgId, string? bankCode)
    {
        var q = db.MortgageDetails
            .Where(d => d.OrgId == orgId && d.Status == MortgageDetailStatus.Approved);

        var list = await (
            from d in q
            join r in db.MortgageRequests on d.MortgageRequestId equals r.Id
            where string.IsNullOrWhiteSpace(bankCode) || r.BankCode == bankCode.ToUpper()
            orderby d.Id descending
            select new ActiveMortgagedItemDto(
                d.Id,
                r.ReqRMNo,
                r.BankCode,
                r.BankName,
                r.PartnerCode,
                r.CreditContractNo,
                d.ItemRefNo,
                d.ModelCode,
                d.EngineNo,
                d.CQNo,
                d.CONo,
                d.DeclarationNo,
                d.CollateralValue,
                d.LoanAmount,
                d.ApprovedAt
            )
        ).ToListAsync();

        return list;
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu thế chấp & giải chấp tài sản ngân hàng.
    /// </summary>
    public async Task<MortgageRedeemSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var mRequests = await db.MortgageRequests
            .Where(r => r.OrgId == orgId)
            .ToListAsync();

        var rRequests = await db.RedeemRequests
            .Where(r => r.OrgId == orgId)
            .ToListAsync();

        var mDetails = await db.MortgageDetails
            .Where(d => d.OrgId == orgId)
            .ToListAsync();

        var totalMortgageRequests = mRequests.Count;
        var activeMortgageRequests = mRequests.Count(r => r.Status == MortgageStatus.Approved);
        var finishedMortgageRequests = mRequests.Count(r => r.Status == MortgageStatus.Finished);

        var totalCollateralItems = mDetails.Count;
        var activeCollateralItems = mDetails.Count(d => d.Status == MortgageDetailStatus.Approved);
        var redeemedCollateralItems = mDetails.Count(d => d.Status == MortgageDetailStatus.Redeemed);

        var totalCollateralValue = mRequests.Sum(r => r.TotalCollateralValue);
        var totalLoanAmount = mRequests.Sum(r => r.TotalLoanAmount);
        var remainingLoanAmount = mRequests.Where(r => r.Status == MortgageStatus.Approved).Sum(r => r.RemainingLoanAmount);

        var totalRedeemRequests = rRequests.Count;
        var completedRedeemRequests = rRequests.Count(r => r.Status == RedeemStatus.Completed);
        var totalSettlementAmount = rRequests.Where(r => r.Status == RedeemStatus.Completed).Sum(r => r.TotalSettlementAmount);

        // Phân bổ theo từng ngân hàng
        var bankGroups = mRequests
            .GroupBy(r => new { r.BankCode, r.BankName })
            .Select(g => new BankMortgageStatDto(
                g.Key.BankCode,
                g.Key.BankName,
                g.Count(),
                g.Sum(r => r.TotalItems),
                g.Sum(r => r.ActiveItems),
                g.Sum(r => r.RedeemedItems),
                g.Sum(r => r.TotalLoanAmount),
                g.Sum(r => r.RemainingLoanAmount)
            ))
            .OrderByDescending(b => b.TotalLoanAmount)
            .ToList();

        return new MortgageRedeemSummaryDto(
            totalMortgageRequests,
            activeMortgageRequests,
            finishedMortgageRequests,
            totalCollateralItems,
            activeCollateralItems,
            redeemedCollateralItems,
            totalCollateralValue,
            totalLoanAmount,
            remainingLoanAmount,
            totalRedeemRequests,
            completedRedeemRequests,
            totalSettlementAmount,
            bankGroups
        );
    }
}

public record MortgageItemInputDto(
    string ItemRefNo,
    string? ModelCode,
    string? EngineNo,
    string? CQNo,
    string? CONo,
    string? DeclarationNo,
    DateTime? CODate,
    long CollateralValue,
    long LoanAmount,
    string? Note
);

public record RedeemItemInputDto(
    string ItemRefNo,
    string? ReqRMNo,
    string? ModelCode,
    string? DealerCode,
    long SettlementAmount,
    string? Note
);

public record ActiveMortgagedItemDto(
    long DetailId,
    string ReqRMNo,
    string BankCode,
    string BankName,
    string PartnerCode,
    string? CreditContractNo,
    string ItemRefNo,
    string ModelCode,
    string? EngineNo,
    string? CQNo,
    string? CONo,
    string? DeclarationNo,
    long CollateralValue,
    long LoanAmount,
    DateTime? ApprovedAt
);

public record BankMortgageStatDto(
    string BankCode,
    string BankName,
    int RequestCount,
    int TotalItems,
    int ActiveItems,
    int RedeemedItems,
    long TotalLoanAmount,
    long RemainingLoanAmount
);

public record MortgageRedeemSummaryDto(
    int TotalMortgageRequests,
    int ActiveMortgageRequests,
    int FinishedMortgageRequests,
    int TotalCollateralItems,
    int ActiveCollateralItems,
    int RedeemedCollateralItems,
    long TotalCollateralValue,
    long TotalLoanAmount,
    long RemainingLoanAmount,
    int TotalRedeemRequests,
    int CompletedRedeemRequests,
    long TotalSettlementAmount,
    List<BankMortgageStatDto> BankBreakdown
);
