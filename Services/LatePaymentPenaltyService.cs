using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý & Tính phạt Chậm thanh toán Đơn hàng / Hợp đồng Xe (Late Payment Delay Penalty Settlement Management).
/// Tương ứng khối TblRptPenaltyPmtDelay, Rpt_PenaltyPmtDelay, Ord_SalesOrder_UpdatePenalizeActual và FrmRptPenaltyPmtDelay, FrmUpdatePenaltyPmtDelayReal trong BizHTC.Report / BizHTC.Payment.
/// </summary>
public sealed class LatePaymentPenaltyService(AppDbContext db)
{
    /// <summary>
    /// Lập hồ sơ tính phạt chậm thanh toán mới cho đơn hàng/hợp đồng xe (FrmRptPenaltyPmtDelay).
    /// </summary>
    public async Task<LatePaymentPenalty> CreatePenaltyRecordAsync(
        Guid orgId,
        string? penaltyRecordNo,
        string soCode,
        string dealerCode,
        string? dealerName,
        string? contractNo,
        DateTime? soApprovedDate,
        decimal? penaltyRateAnnual,
        string? remark,
        string? createdBy,
        List<PenaltyItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(soCode))
            throw new ArgumentException("Số đơn hàng (SOCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(dealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Hồ sơ tính phạt cần ít nhất 1 dòng xe kiểm tra nghĩa vụ thanh toán.");

        var finalSOCode = soCode.Trim().ToUpper();
        var finalDealerCode = dealerCode.Trim().ToUpper();
        var finalRecordNo = string.IsNullOrWhiteSpace(penaltyRecordNo)
            ? $"PEN-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}"
            : penaltyRecordNo.Trim().ToUpper();

        var exists = await db.LatePaymentPenalties.AnyAsync(p => p.OrgId == orgId && p.PenaltyRecordNo == finalRecordNo);
        if (exists)
            throw new InvalidOperationException($"Số hồ sơ tính phạt '{finalRecordNo}' đã tồn tại trên hệ thống.");

        // Kiểm tra trùng VIN trong cùng hồ sơ
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung (VIN) không được để trống.");
            if (!vinSet.Add(it.VIN.Trim().ToUpper()))
                throw new ArgumentException($"Số khung VIN '{it.VIN}' bị trùng lặp trong cùng hồ sơ.");
            if (it.UnitPriceActual <= 0)
                throw new ArgumentException($"Đơn giá xe cho số khung '{it.VIN}' phải > 0.");
        }

        var rate = penaltyRateAnnual.HasValue && penaltyRateAnnual.Value > 0 ? penaltyRateAnnual.Value : 12.0m;
        var details = new List<LatePaymentPenaltyDetail>();
        long totalUnitPrice = 0;

        foreach (var item in items)
        {
            totalUnitPrice += item.UnitPriceActual;
            details.Add(new LatePaymentPenaltyDetail
            {
                OrgId = orgId,
                VIN = item.VIN.Trim().ToUpper(),
                CarId = item.CarId?.Trim(),
                ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
                ModelName = item.ModelName?.Trim(),
                ColorName = item.ColorName?.Trim(),
                UnitPriceActual = item.UnitPriceActual,
                DepositDueDate = item.DepositDueDate,
                ActualDepositDate = item.ActualDepositDate,
                GrtDueDate = item.GrtDueDate,
                ActualGrtDate = item.ActualGrtDate,
                GrtPayDueDate = item.GrtPayDueDate,
                ActualGrtPayDate = item.ActualGrtPayDate,
                Payment60DueDate = item.Payment60DueDate,
                Actual60PayDate = item.Actual60PayDate,
                PaymentRemainDueDate = item.PaymentRemainDueDate,
                ActualRemainPayDate = item.ActualRemainPayDate,
                Status = LatePaymentPenaltyDetailStatus.Pending,
                Note = item.Note
            });
        }

        var penalty = new LatePaymentPenalty
        {
            OrgId = orgId,
            PenaltyRecordNo = finalRecordNo,
            SOCode = finalSOCode,
            DealerCode = finalDealerCode,
            DealerName = string.IsNullOrWhiteSpace(dealerName) ? $"Đại lý {finalDealerCode}" : dealerName.Trim(),
            ContractNo = contractNo?.Trim().ToUpper(),
            SOApprovedDate = soApprovedDate ?? DateTime.Today.AddDays(-60),
            TotalApprovedQuantity = details.Count,
            TotalUnitPriceActual = totalUnitPrice,
            PenaltyRateAnnual = rate,
            Status = LatePaymentPenaltyStatus.Draft,
            Remark = remark?.Trim(),
            CreatedBy = createdBy ?? "ChuyenVienKeToanCongNo",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.LatePaymentPenalties.Add(penalty);
        await db.SaveChangesAsync();

        // Tự động tính toán số ngày và tiền phạt ban đầu
        await CalculatePenaltyInternalAsync(penalty);
        await db.SaveChangesAsync();

        return penalty;
    }

    /// <summary>
    /// Thuật toán tự động tính số ngày trễ của 5 mốc cam kết thanh toán xe và tính tiền phạt hệ thống (Rpt_PenaltyPmtDelay).
    /// </summary>
    public async Task<LatePaymentPenalty?> CalculatePenaltyAsync(long penaltyId, Guid orgId)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status == LatePaymentPenaltyStatus.Settled || penalty.Status == LatePaymentPenaltyStatus.Cancelled)
            throw new InvalidOperationException($"Không thể tính lại hồ sơ phạt ở trạng thái {penalty.Status}.");

        await CalculatePenaltyInternalAsync(penalty);
        await db.SaveChangesAsync();
        return penalty;
    }

    private Task CalculatePenaltyInternalAsync(LatePaymentPenalty penalty)
    {
        var rate = penalty.PenaltyRateAnnual > 0 ? penalty.PenaltyRateAnnual : 12.0m;
        long totalPenaltySystem = 0;

        foreach (var d in penalty.Details)
        {
            // 1. Chậm cọc
            d.DelayDaysDeposit = (d.ActualDepositDate.HasValue && d.DepositDueDate.HasValue && d.ActualDepositDate.Value > d.DepositDueDate.Value)
                ? (int)(d.ActualDepositDate.Value.Date - d.DepositDueDate.Value.Date).TotalDays : 0;

            // 2. Chậm mở bảo lãnh
            d.DelayDaysGrtOpen = (d.ActualGrtDate.HasValue && d.GrtDueDate.HasValue && d.ActualGrtDate.Value > d.GrtDueDate.Value)
                ? (int)(d.ActualGrtDate.Value.Date - d.GrtDueDate.Value.Date).TotalDays : 0;

            // 3. Chậm thanh toán bảo lãnh
            d.DelayDaysGrtPay = (d.ActualGrtPayDate.HasValue && d.GrtPayDueDate.HasValue && d.ActualGrtPayDate.Value > d.GrtPayDueDate.Value)
                ? (int)(d.ActualGrtPayDate.Value.Date - d.GrtPayDueDate.Value.Date).TotalDays : 0;

            // 4. Chậm thanh toán đợt 60%
            d.DelayDays60Pmt = (d.Actual60PayDate.HasValue && d.Payment60DueDate.HasValue && d.Actual60PayDate.Value > d.Payment60DueDate.Value)
                ? (int)(d.Actual60PayDate.Value.Date - d.Payment60DueDate.Value.Date).TotalDays : 0;

            // 5. Chậm thanh toán đợt 40% còn lại (100%)
            d.DelayDaysRemain = (d.ActualRemainPayDate.HasValue && d.PaymentRemainDueDate.HasValue && d.ActualRemainPayDate.Value > d.PaymentRemainDueDate.Value)
                ? (int)(d.ActualRemainPayDate.Value.Date - d.PaymentRemainDueDate.Value.Date).TotalDays : 0;

            // Quy tắc BizHTC: Lấy số ngày trễ lớn nhất của các mốc cho từng xe
            d.MaxDelayDays = Math.Max(d.DelayDaysDeposit,
                Math.Max(d.DelayDaysGrtOpen,
                Math.Max(d.DelayDaysGrtPay,
                Math.Max(d.DelayDays60Pmt, d.DelayDaysRemain))));

            // Tiền phạt xe = Đơn giá × (Tỷ lệ lãi phạt / 100) × Số ngày trễ / 365
            if (d.MaxDelayDays > 0)
            {
                decimal pmtVal = (decimal)d.UnitPriceActual * (rate / 100m) * ((decimal)d.MaxDelayDays / 365m);
                d.ItemPenaltyAmount = (long)Math.Round(pmtVal, MidpointRounding.AwayFromZero);
            }
            else
            {
                d.ItemPenaltyAmount = 0;
            }

            if (d.Status == LatePaymentPenaltyDetailStatus.Pending)
                d.Status = LatePaymentPenaltyDetailStatus.Calculated;

            totalPenaltySystem += d.ItemPenaltyAmount;
        }

        penalty.MaxDelayDaysDeposit = penalty.Details.Count > 0 ? penalty.Details.Max(d => d.DelayDaysDeposit) : 0;
        penalty.MaxDelayDaysGrtOpen = penalty.Details.Count > 0 ? penalty.Details.Max(d => d.DelayDaysGrtOpen) : 0;
        penalty.MaxDelayDaysGrtPay = penalty.Details.Count > 0 ? penalty.Details.Max(d => d.DelayDaysGrtPay) : 0;
        penalty.MaxDelayDays60Pmt = penalty.Details.Count > 0 ? penalty.Details.Max(d => d.DelayDays60Pmt) : 0;
        penalty.MaxDelayDaysRemain = penalty.Details.Count > 0 ? penalty.Details.Max(d => d.DelayDaysRemain) : 0;

        penalty.TotalDatePenalty = Math.Max(penalty.MaxDelayDaysDeposit,
            Math.Max(penalty.MaxDelayDaysGrtOpen,
            Math.Max(penalty.MaxDelayDaysGrtPay,
            Math.Max(penalty.MaxDelayDays60Pmt, penalty.MaxDelayDaysRemain))));

        penalty.AmountPenaltySystem = totalPenaltySystem;

        // Nếu mới tính toán lần đầu, mặc định mức phạt chốt thực tế = mức hệ thống tính
        if (penalty.PenalizeActual == 0 || penalty.Status == LatePaymentPenaltyStatus.Draft)
        {
            penalty.PenalizeActual = totalPenaltySystem;
            penalty.WaivedAmount = 0;
        }
        else
        {
            penalty.WaivedAmount = Math.Max(0, penalty.AmountPenaltySystem - penalty.PenalizeActual);
        }

        if (penalty.Status == LatePaymentPenaltyStatus.Draft)
            penalty.Status = LatePaymentPenaltyStatus.Calculated;

        penalty.CalculatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Kế toán thẩm định đề xuất mức phạt thực tế và số tiền miễn giảm (FrmUpdatePenaltyPmtDelayReal / Review).
    /// </summary>
    public async Task<LatePaymentPenalty?> ReviewPenaltyAsync(
        long penaltyId,
        Guid orgId,
        long proposedPenalizeActual,
        string? adjustmentReason,
        string? reviewerName)
    {
        if (proposedPenalizeActual < 0)
            throw new ArgumentException("Số tiền phạt thực tế đề xuất phải >= 0.");

        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status == LatePaymentPenaltyStatus.Settled || penalty.Status == LatePaymentPenaltyStatus.Cancelled)
            throw new InvalidOperationException($"Hồ sơ phạt ở trạng thái {penalty.Status} không được phép thẩm định.");

        penalty.PenalizeActual = proposedPenalizeActual;
        penalty.WaivedAmount = Math.Max(0, penalty.AmountPenaltySystem - proposedPenalizeActual);
        penalty.AdjustmentReason = adjustmentReason?.Trim();
        penalty.ReviewedBy = reviewerName ?? "KTT_KiemToanTaiChinh";
        penalty.ReviewedAt = DateTime.Now;
        penalty.Status = LatePaymentPenaltyStatus.Reviewed;

        // Phân bổ tỉ lệ phạt thực tế cho từng dòng xe
        DistributeActualPenaltyToDetails(penalty);

        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Ban Giám đốc phê duyệt chốt số tiền phạt thực tế (Ord_SalesOrder_UpdatePenalizeActual / Approve).
    /// </summary>
    public async Task<LatePaymentPenalty?> ApprovePenaltyAsync(
        long penaltyId,
        Guid orgId,
        string? approverName)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status != LatePaymentPenaltyStatus.Calculated && penalty.Status != LatePaymentPenaltyStatus.Reviewed)
            throw new InvalidOperationException($"Chỉ hồ sơ ở trạng thái Đã Tính (Calculated) hoặc Đã Thẩm Định (Reviewed) mới được phê duyệt. Hiện tại: {penalty.Status}.");

        penalty.ApprovedBy = approverName ?? "GiamDocTaiChinh_HTC";
        penalty.ApprovedAt = DateTime.Now;
        penalty.Status = LatePaymentPenaltyStatus.Approved;

        foreach (var d in penalty.Details)
        {
            if (d.Status != LatePaymentPenaltyDetailStatus.Cancelled)
                d.Status = LatePaymentPenaltyDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Quyết toán thu nộp phạt chậm thanh toán hoặc cấn trừ công nợ bán xe (Settled).
    /// </summary>
    public async Task<LatePaymentPenalty?> SettlePenaltyAsync(
        long penaltyId,
        Guid orgId,
        string? paymentProofRef,
        string? settlerName)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status != LatePaymentPenaltyStatus.Approved && penalty.Status != LatePaymentPenaltyStatus.Reviewed)
            throw new InvalidOperationException($"Chỉ hồ sơ đã được thẩm định hoặc phê duyệt mới được quyết toán. Hiện tại: {penalty.Status}.");

        penalty.PaymentProofRef = paymentProofRef?.Trim() ?? $"UNC-PHAT-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}";
        penalty.SettledBy = settlerName ?? "KeToanThanhToan_HTC";
        penalty.SettledAt = DateTime.Now;
        penalty.Status = LatePaymentPenaltyStatus.Settled;

        foreach (var d in penalty.Details)
        {
            if (d.Status != LatePaymentPenaltyDetailStatus.Cancelled)
                d.Status = LatePaymentPenaltyDetailStatus.Settled;
        }

        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Miễn phạt 100% khi có phê duyệt đặc biệt / lý do bất khả kháng (Waived).
    /// </summary>
    public async Task<LatePaymentPenalty?> WaivePenaltyAsync(
        long penaltyId,
        Guid orgId,
        string? waiveReason,
        string? approverName)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status == LatePaymentPenaltyStatus.Settled || penalty.Status == LatePaymentPenaltyStatus.Cancelled)
            throw new InvalidOperationException($"Không thể miễn phạt hồ sơ ở trạng thái {penalty.Status}.");

        penalty.PenalizeActual = 0;
        penalty.WaivedAmount = penalty.AmountPenaltySystem;
        penalty.AdjustmentReason = waiveReason?.Trim() ?? "Miễn phạt do sự kiện bất khả kháng được Ban Lãnh đạo phê duyệt";
        penalty.ApprovedBy = approverName ?? "TongGiamDoc_HTC";
        penalty.ApprovedAt = DateTime.Now;
        penalty.Status = LatePaymentPenaltyStatus.Waived;

        foreach (var d in penalty.Details)
        {
            d.ActualItemPenalty = 0;
            if (d.Status != LatePaymentPenaltyDetailStatus.Cancelled)
                d.Status = LatePaymentPenaltyDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Hủy hồ sơ tính phạt (Cancelled).
    /// </summary>
    public async Task<LatePaymentPenalty?> CancelPenaltyAsync(long penaltyId, Guid orgId, string? reason)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status == LatePaymentPenaltyStatus.Settled)
            throw new InvalidOperationException("Không thể hủy hồ sơ phạt đã được quyết toán.");

        penalty.Status = LatePaymentPenaltyStatus.Cancelled;
        penalty.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            penalty.Remark = string.IsNullOrWhiteSpace(penalty.Remark)
                ? $"Hủy: {reason.Trim()}"
                : $"{penalty.Remark} | Hủy: {reason.Trim()}";
        }

        foreach (var d in penalty.Details)
        {
            d.Status = LatePaymentPenaltyDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Cập nhật số tiền phạt chốt thực tế hàng loạt (tương ứng FrmUpdatePenaltyPmtDelayReal / Ord_SalesOrder_UpdatePenalizeActual).
    /// </summary>
    public async Task<List<LatePaymentPenalty>> UpdatePenalizeActualMultiAsync(
        Guid orgId,
        List<UpdateActualPenaltyItemDto> items,
        string? updatedBy)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Danh sách cập nhật không được để trống.");

        var results = new List<LatePaymentPenalty>();
        foreach (var it in items)
        {
            if (it.PenalizeActual < 0)
                throw new ArgumentException($"Số tiền phạt thực tế cho mã #{it.PenaltyId} phải >= 0.");

            var penalty = await db.LatePaymentPenalties
                .Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.Id == it.PenaltyId && p.OrgId == orgId);

            if (penalty == null || penalty.Status == LatePaymentPenaltyStatus.Settled || penalty.Status == LatePaymentPenaltyStatus.Cancelled)
                continue;

            penalty.PenalizeActual = it.PenalizeActual;
            penalty.WaivedAmount = Math.Max(0, penalty.AmountPenaltySystem - it.PenalizeActual);
            if (!string.IsNullOrWhiteSpace(it.Reason))
                penalty.AdjustmentReason = it.Reason.Trim();

            penalty.ReviewedBy = updatedBy ?? "KTT_KiemToanTaiChinh";
            penalty.ReviewedAt = DateTime.Now;
            if (penalty.Status == LatePaymentPenaltyStatus.Draft || penalty.Status == LatePaymentPenaltyStatus.Calculated)
                penalty.Status = LatePaymentPenaltyStatus.Reviewed;

            DistributeActualPenaltyToDetails(penalty);
            results.Add(penalty);
        }

        await db.SaveChangesAsync();
        return results;
    }

    /// <summary>
    /// Bổ sung danh sách xe vào hồ sơ phạt hiện có.
    /// </summary>
    public async Task<LatePaymentPenalty?> ImportVehiclesAsync(long penaltyId, Guid orgId, List<PenaltyItemInputDto> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Danh sách xe bổ sung không được để trống.");

        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;
        if (penalty.Status == LatePaymentPenaltyStatus.Settled || penalty.Status == LatePaymentPenaltyStatus.Cancelled)
            throw new InvalidOperationException($"Không thể bổ sung xe vào hồ sơ ở trạng thái {penalty.Status}.");

        var existingVins = new HashSet<string>(penalty.Details.Select(d => d.VIN), StringComparer.OrdinalIgnoreCase);

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN)) continue;
            var vinUpper = it.VIN.Trim().ToUpper();
            if (existingVins.Contains(vinUpper)) continue;

            existingVins.Add(vinUpper);
            penalty.Details.Add(new LatePaymentPenaltyDetail
            {
                OrgId = orgId,
                VIN = vinUpper,
                CarId = it.CarId?.Trim(),
                ModelCode = string.IsNullOrWhiteSpace(it.ModelCode) ? "HYUNDAI" : it.ModelCode.Trim().ToUpper(),
                ModelName = it.ModelName?.Trim(),
                ColorName = it.ColorName?.Trim(),
                UnitPriceActual = it.UnitPriceActual,
                DepositDueDate = it.DepositDueDate,
                ActualDepositDate = it.ActualDepositDate,
                GrtDueDate = it.GrtDueDate,
                ActualGrtDate = it.ActualGrtDate,
                GrtPayDueDate = it.GrtPayDueDate,
                ActualGrtPayDate = it.ActualGrtPayDate,
                Payment60DueDate = it.Payment60DueDate,
                Actual60PayDate = it.Actual60PayDate,
                PaymentRemainDueDate = it.PaymentRemainDueDate,
                ActualRemainPayDate = it.ActualRemainPayDate,
                Status = LatePaymentPenaltyDetailStatus.Pending,
                Note = it.Note
            });
        }

        penalty.TotalApprovedQuantity = penalty.Details.Count;
        penalty.TotalUnitPriceActual = penalty.Details.Sum(d => d.UnitPriceActual);

        await CalculatePenaltyInternalAsync(penalty);
        await db.SaveChangesAsync();
        return penalty;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Thông báo tính & quyết toán phạt chậm thanh toán xe (Penalty Settlement Advice).
    /// </summary>
    public async Task<PenaltyAdviceDto?> GeneratePenaltyAdviceAsync(long penaltyId, Guid orgId)
    {
        var penalty = await db.LatePaymentPenalties
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.OrgId == orgId);

        if (penalty == null) return null;

        var amountInWords = ConvertNumberToWords(penalty.PenalizeActual);
        var waivedInWords = ConvertNumberToWords(penalty.WaivedAmount);

        return new PenaltyAdviceDto
        {
            PenaltyRecordNo = penalty.PenaltyRecordNo,
            SOCode = penalty.SOCode,
            DealerCode = penalty.DealerCode,
            DealerName = penalty.DealerName ?? $"Đại lý {penalty.DealerCode}",
            ContractNo = penalty.ContractNo ?? "HĐ-BANBUON-XE",
            SOApprovedDate = penalty.SOApprovedDate?.ToString("dd/MM/yyyy") ?? "-",
            TotalQuantity = penalty.TotalApprovedQuantity,
            TotalOrderAmount = penalty.TotalUnitPriceActual,
            PenaltyRateAnnual = penalty.PenaltyRateAnnual,
            MaxDelayDaysDeposit = penalty.MaxDelayDaysDeposit,
            MaxDelayDaysGrtOpen = penalty.MaxDelayDaysGrtOpen,
            MaxDelayDaysGrtPay = penalty.MaxDelayDaysGrtPay,
            MaxDelayDays60Pmt = penalty.MaxDelayDays60Pmt,
            MaxDelayDaysRemain = penalty.MaxDelayDaysRemain,
            TotalDatePenalty = penalty.TotalDatePenalty,
            AmountPenaltySystem = penalty.AmountPenaltySystem,
            PenalizeActual = penalty.PenalizeActual,
            PenalizeActualInWords = amountInWords,
            WaivedAmount = penalty.WaivedAmount,
            WaivedAmountInWords = waivedInWords,
            AdjustmentReason = penalty.AdjustmentReason ?? "Theo biên bản đối chiếu tài chính và chính sách đại lý",
            PaymentProofRef = penalty.PaymentProofRef,
            Status = penalty.Status.ToString(),
            CreatedBy = penalty.CreatedBy ?? "HeThong",
            CreatedAt = penalty.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
            ReviewedBy = penalty.ReviewedBy ?? "Phòng Kế toán Công nợ",
            ReviewedAt = penalty.ReviewedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            ApprovedBy = penalty.ApprovedBy ?? "Ban Giám đốc Tài chính HTV",
            ApprovedAt = penalty.ApprovedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            SettledBy = penalty.SettledBy,
            SettledAt = penalty.SettledAt?.ToString("dd/MM/yyyy HH:mm"),
            Vehicles = penalty.Details.OrderBy(d => d.Id).Select(d => new PenaltyAdviceVehicleItemDto
            {
                VIN = d.VIN,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? d.ModelCode,
                ColorName = d.ColorName ?? "-",
                UnitPriceActual = d.UnitPriceActual,
                DelayDaysDeposit = d.DelayDaysDeposit,
                DelayDaysGrtOpen = d.DelayDaysGrtOpen,
                DelayDaysGrtPay = d.DelayDaysGrtPay,
                DelayDays60Pmt = d.DelayDays60Pmt,
                DelayDaysRemain = d.DelayDaysRemain,
                MaxDelayDays = d.MaxDelayDays,
                ItemPenaltyAmount = d.ItemPenaltyAmount,
                ActualItemPenalty = d.ActualItemPenalty
            }).ToList()
        };
    }

    /// <summary>
    /// Báo cáo dashboard tổng hợp số liệu tính phạt chậm thanh toán.
    /// </summary>
    public async Task<PenaltySummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.LatePaymentPenalties
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        var totalRecords = list.Count;
        var totalOrderAmount = list.Sum(p => p.TotalUnitPriceActual);
        var totalPenaltySystem = list.Sum(p => p.AmountPenaltySystem);
        var totalPenalizeActual = list.Sum(p => p.PenalizeActual);
        var totalWaivedAmount = list.Sum(p => p.WaivedAmount);
        var totalSettledAmount = list.Where(p => p.Status == LatePaymentPenaltyStatus.Settled).Sum(p => p.PenalizeActual);

        var settledCount = list.Count(p => p.Status == LatePaymentPenaltyStatus.Settled);
        var settledRate = totalRecords > 0 ? Math.Round((double)settledCount * 100.0 / totalRecords, 1) : 0;
        var avgDelayDays = totalRecords > 0 ? (int)Math.Round(list.Average(p => p.TotalDatePenalty)) : 0;

        return new PenaltySummaryDto
        {
            TotalRecords = totalRecords,
            TotalOrderAmount = totalOrderAmount,
            TotalPenaltySystem = totalPenaltySystem,
            TotalPenalizeActual = totalPenalizeActual,
            TotalWaivedAmount = totalWaivedAmount,
            TotalSettledAmount = totalSettledAmount,
            SettledRatePercent = settledRate,
            AverageDelayDays = avgDelayDays,
            DraftCount = list.Count(p => p.Status == LatePaymentPenaltyStatus.Draft || p.Status == LatePaymentPenaltyStatus.Calculated),
            ReviewedCount = list.Count(p => p.Status == LatePaymentPenaltyStatus.Reviewed),
            ApprovedCount = list.Count(p => p.Status == LatePaymentPenaltyStatus.Approved),
            SettledCount = settledCount,
            WaivedCount = list.Count(p => p.Status == LatePaymentPenaltyStatus.Waived)
        };
    }

    private static void DistributeActualPenaltyToDetails(LatePaymentPenalty penalty)
    {
        if (penalty.Details.Count == 0) return;
        if (penalty.AmountPenaltySystem == 0)
        {
            foreach (var d in penalty.Details) d.ActualItemPenalty = 0;
            return;
        }

        long runningActual = 0;
        for (int i = 0; i < penalty.Details.Count; i++)
        {
            var d = penalty.Details[i];
            if (i == penalty.Details.Count - 1)
            {
                d.ActualItemPenalty = Math.Max(0, penalty.PenalizeActual - runningActual);
            }
            else
            {
                decimal ratio = (decimal)d.ItemPenaltyAmount / penalty.AmountPenaltySystem;
                d.ActualItemPenalty = (long)Math.Round(penalty.PenalizeActual * ratio, MidpointRounding.AwayFromZero);
                runningActual += d.ActualItemPenalty;
            }
        }
    }

    /// <summary>
    /// Thuật toán chuyển đổi số tiền (VND) thành chữ tiếng Việt chuẩn quy chuẩn tài chính ngân hàng.
    /// </summary>
    public static string ConvertNumberToWords(long number)
    {
        if (number == 0) return "Không đồng chẵn.";
        if (number < 0) return "Âm " + ConvertNumberToWords(Math.Abs(number));

        string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
        string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

        List<string> groups = [];
        long temp = number;
        int groupIdx = 0;

        while (temp > 0)
        {
            int block = (int)(temp % 1000);
            temp /= 1000;

            if (block > 0)
            {
                int h = block / 100;
                int t = (block % 100) / 10;
                int u = block % 10;

                string blockText = "";
                if (h > 0 || temp > 0)
                {
                    blockText += digits[h] + " trăm ";
                }

                if (t > 1)
                {
                    blockText += digits[t] + " mươi ";
                    if (u == 1) blockText += "mốt ";
                    else if (u == 5) blockText += "lăm ";
                    else if (u > 0) blockText += digits[u] + " ";
                }
                else if (t == 1)
                {
                    blockText += "mười ";
                    if (u == 5) blockText += "lăm ";
                    else if (u > 0) blockText += digits[u] + " ";
                }
                else // t == 0
                {
                    if (u > 0)
                    {
                        if (h > 0 || temp > 0) blockText += "lẻ ";
                        blockText += digits[u] + " ";
                    }
                }

                blockText = blockText.Trim();
                if (!string.IsNullOrWhiteSpace(units[groupIdx]))
                    blockText += " " + units[groupIdx];

                groups.Insert(0, blockText);
            }
            groupIdx++;
        }

        string result = string.Join(" ", groups).Trim();
        if (string.IsNullOrWhiteSpace(result)) return "Không đồng chẵn.";

        result = char.ToUpper(result[0]) + result[1..] + " đồng chẵn.";
        return result;
    }
}

public sealed class PenaltyItemInputDto
{
    public string VIN { get; set; } = "";
    public string? CarId { get; set; }
    public string? ModelCode { get; set; }
    public string? ModelName { get; set; }
    public string? ColorName { get; set; }
    public long UnitPriceActual { get; set; }
    public DateTime? DepositDueDate { get; set; }
    public DateTime? ActualDepositDate { get; set; }
    public DateTime? GrtDueDate { get; set; }
    public DateTime? ActualGrtDate { get; set; }
    public DateTime? GrtPayDueDate { get; set; }
    public DateTime? ActualGrtPayDate { get; set; }
    public DateTime? Payment60DueDate { get; set; }
    public DateTime? Actual60PayDate { get; set; }
    public DateTime? PaymentRemainDueDate { get; set; }
    public DateTime? ActualRemainPayDate { get; set; }
    public string? Note { get; set; }
}

public sealed class UpdateActualPenaltyItemDto
{
    public long PenaltyId { get; set; }
    public long PenalizeActual { get; set; }
    public string? Reason { get; set; }
}

public sealed class PenaltyAdviceDto
{
    public string PenaltyRecordNo { get; set; } = "";
    public string SOCode { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string ContractNo { get; set; } = "";
    public string SOApprovedDate { get; set; } = "";
    public int TotalQuantity { get; set; }
    public long TotalOrderAmount { get; set; }
    public decimal PenaltyRateAnnual { get; set; }
    public int MaxDelayDaysDeposit { get; set; }
    public int MaxDelayDaysGrtOpen { get; set; }
    public int MaxDelayDaysGrtPay { get; set; }
    public int MaxDelayDays60Pmt { get; set; }
    public int MaxDelayDaysRemain { get; set; }
    public int TotalDatePenalty { get; set; }
    public long AmountPenaltySystem { get; set; }
    public long PenalizeActual { get; set; }
    public string PenalizeActualInWords { get; set; } = "";
    public long WaivedAmount { get; set; }
    public string WaivedAmountInWords { get; set; } = "";
    public string AdjustmentReason { get; set; } = "";
    public string? PaymentProofRef { get; set; }
    public string Status { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string ReviewedBy { get; set; } = "";
    public string ReviewedAt { get; set; } = "";
    public string ApprovedBy { get; set; } = "";
    public string ApprovedAt { get; set; } = "";
    public string? SettledBy { get; set; }
    public string? SettledAt { get; set; }
    public List<PenaltyAdviceVehicleItemDto> Vehicles { get; set; } = [];
}

public sealed class PenaltyAdviceVehicleItemDto
{
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string ColorName { get; set; } = "";
    public long UnitPriceActual { get; set; }
    public int DelayDaysDeposit { get; set; }
    public int DelayDaysGrtOpen { get; set; }
    public int DelayDaysGrtPay { get; set; }
    public int DelayDays60Pmt { get; set; }
    public int DelayDaysRemain { get; set; }
    public int MaxDelayDays { get; set; }
    public long ItemPenaltyAmount { get; set; }
    public long ActualItemPenalty { get; set; }
}

public sealed class PenaltySummaryDto
{
    public int TotalRecords { get; set; }
    public long TotalOrderAmount { get; set; }
    public long TotalPenaltySystem { get; set; }
    public long TotalPenalizeActual { get; set; }
    public long TotalWaivedAmount { get; set; }
    public long TotalSettledAmount { get; set; }
    public double SettledRatePercent { get; set; }
    public int AverageDelayDays { get; set; }
    public int DraftCount { get; set; }
    public int ReviewedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int SettledCount { get; set; }
    public int WaivedCount { get; set; }
}
