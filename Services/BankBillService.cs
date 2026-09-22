using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Biên bản Bàn giao Chứng từ gốc xe theo Hối phiếu ngân hàng (Bank Acceptance Draft / Bill of Exchange Handover Minutes).
/// Tương ứng khối Car_BankBillMinutes, Car_BankBillMinutesDtl và FrmQuanLyBBBGTheoHoiPhieu, FrmTaoBBBGTheoHoiPhieu trong BizHTC.Payment.
/// </summary>
public sealed class BankBillService(AppDbContext db)
{
    /// <summary>
    /// Lập biên bản bàn giao xe & chứng từ theo hối phiếu mới (Car_BankBillMinutes_Add / FrmTaoBBBGTheoHoiPhieu).
    /// </summary>
    public async Task<BankBillMinutes> CreateMinutesAsync(
        Guid orgId,
        string? bankBillMnNo,
        string bankCode,
        string? bankName,
        string partnerCode,
        string? partnerName,
        DateTime? bankBillDate,
        DateTime? bankBillPrintDate,
        string? remark,
        string? createdBy,
        List<BankBillItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Mã ngân hàng (BankCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đại lý / đối tác (PartnerCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Biên bản bàn giao hối phiếu phải có ít nhất 1 xe / hồ sơ gốc.");

        // Kiểm tra trùng VIN trong cùng biên bản
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung (VIN) không được để trống.");
            if (!vinSet.Add(it.VIN.Trim().ToUpper()))
                throw new ArgumentException($"Số khung VIN '{it.VIN}' bị trùng lặp trong danh sách biên bản.");
        }

        var finalMnNo = string.IsNullOrWhiteSpace(bankBillMnNo)
            ? $"BBBG-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}"
            : bankBillMnNo.Trim();

        var exists = await db.BankBillMinutes.AnyAsync(b => b.OrgId == orgId && b.BankBillMnNo == finalMnNo);
        if (exists)
            throw new InvalidOperationException($"Số biên bản bàn giao hối phiếu '{finalMnNo}' đã tồn tại.");

        var billDate = bankBillDate ?? DateTime.Today;
        var printDate = bankBillPrintDate ?? billDate;
        var finalBankCode = bankCode.Trim().ToUpper();
        var finalPartnerCode = partnerCode.Trim().ToUpper();

        long totalAmount = 0;
        var details = new List<BankBillMinutesDetail>();

        int idx = 1;
        foreach (var item in items)
        {
            var claimAmt = item.ClaimAmount > 0 ? item.ClaimAmount : 0;
            totalAmount += claimAmt;

            details.Add(new BankBillMinutesDetail
            {
                OrgId = orgId,
                VIN = item.VIN.Trim().ToUpper(),
                ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
                SpecCode = item.SpecCode?.Trim(),
                SpecDescription = item.SpecDescription?.Trim(),
                EngineNo = item.EngineNo?.Trim(),
                CONo = item.CONo?.Trim(),
                CabinCONo = item.CabinCONo?.Trim(),
                DeclarationNo = item.DeclarationNo?.Trim(),
                BankGuaranteeNo = item.BankGuaranteeNo?.Trim(),
                HTCInvoiceNo = item.HTCInvoiceNo?.Trim(),
                TCGInvoiceNo = item.TCGInvoiceNo?.Trim(),
                TransportMinutesNo = item.TransportMinutesNo?.Trim(),
                ClaimAmount = claimAmt,
                GuaranteeDateStart = item.GuaranteeDateStart ?? billDate,
                GuaranteeDateOpen = item.GuaranteeDateOpen ?? billDate,
                NumberOfDaysDeferred = item.NumberOfDaysDeferred > 0 ? item.NumberOfDaysDeferred : 30,
                Status = BankBillDetailStatus.Pending,
                Note = item.Note
            });
            idx++;
        }

        var minutes = new BankBillMinutes
        {
            OrgId = orgId,
            BankBillMnNo = finalMnNo,
            BankCode = finalBankCode,
            BankName = string.IsNullOrWhiteSpace(bankName) ? ResolveBankName(finalBankCode) : bankName.Trim(),
            PartnerCode = finalPartnerCode,
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? $"Đại lý Hyundai {finalPartnerCode}" : partnerName.Trim(),
            BankBillDate = billDate,
            BankBillPrintDate = printDate,
            TotalVehicles = details.Count,
            TotalClaimAmount = totalAmount,
            Status = BankBillMinutesStatus.PendingHandover,
            Remark = remark,
            CreatedBy = createdBy ?? "ChuyenVienHopDong",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.BankBillMinutes.Add(minutes);
        await db.SaveChangesAsync();

        return minutes;
    }

    /// <summary>
    /// Xuất trình và bàn giao bộ chứng từ gốc sang Ngân hàng (FrmQuanLyBBBGTheoHoiPhieu.btnHandover_Click).
    /// </summary>
    public async Task<BankBillMinutes?> HandoverToBankAsync(long id, Guid orgId, string? handedOverBy, string? note)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status != BankBillMinutesStatus.PendingHandover && minutes.Status != BankBillMinutesStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xuất trình bàn giao biên bản ở trạng thái Chờ bàn giao hoặc Dự thảo (Hiện tại: {minutes.Status}).");

        minutes.Status = BankBillMinutesStatus.Delivered;
        minutes.HandedOverBy = string.IsNullOrWhiteSpace(handedOverBy) ? "ChuyenVienGiaoNhan" : handedOverBy.Trim();
        minutes.HandedOverAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(note))
        {
            minutes.Remark = string.IsNullOrEmpty(minutes.Remark) ? note : $"{minutes.Remark}; {note}";
        }

        foreach (var dtl in minutes.Details)
        {
            if (dtl.Status == BankBillDetailStatus.Pending)
                dtl.Status = BankBillDetailStatus.Delivered;
        }

        await db.SaveChangesAsync();
        return minutes;
    }

    /// <summary>
    /// Ngân hàng xác nhận tiếp nhận đủ hồ sơ gốc xe theo hối phiếu (Car_BankBillMinutes_Save / BankBillReciveDate).
    /// </summary>
    public async Task<BankBillMinutes?> BankConfirmReceiveAsync(long id, Guid orgId, DateTime? receiveDate, string? receivedBy, string? note)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status != BankBillMinutesStatus.Delivered && minutes.Status != BankBillMinutesStatus.PendingHandover)
            throw new InvalidOperationException($"Ngân hàng chỉ có thể xác nhận tiếp nhận hồ sơ khi biên bản đã bàn giao (Delivered). Hiện tại: {minutes.Status}.");

        var recDate = receiveDate ?? DateTime.Today;
        minutes.BankBillReciveDate = recDate;
        minutes.Status = BankBillMinutesStatus.BankReceived;
        minutes.BankReceivedBy = string.IsNullOrWhiteSpace(receivedBy) ? "GiaoDichVienNganHang" : receivedBy.Trim();
        minutes.BankReceivedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(note))
        {
            minutes.Remark = string.IsNullOrEmpty(minutes.Remark) ? note : $"{minutes.Remark}; {note}";
        }

        foreach (var dtl in minutes.Details)
        {
            if (dtl.Status == BankBillDetailStatus.Delivered || dtl.Status == BankBillDetailStatus.Pending)
                dtl.Status = BankBillDetailStatus.BankVerified;
        }

        await db.SaveChangesAsync();
        return minutes;
    }

    /// <summary>
    /// Quyết toán hoàn tất thanh toán hối phiếu khi ngân hàng hoặc đại lý chuyển tiền đầy đủ (FrmQuanLyBBBGTheoHoiPhieu / Settle).
    /// </summary>
    public async Task<BankBillMinutes?> SettleBankBillAsync(long id, Guid orgId, string? settledBy, string? note)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status != BankBillMinutesStatus.BankReceived && minutes.Status != BankBillMinutesStatus.Delivered)
            throw new InvalidOperationException($"Chỉ có thể quyết toán hối phiếu đã được ngân hàng tiếp nhận chứng từ (BankReceived) hoặc đã bàn giao. Hiện tại: {minutes.Status}.");

        minutes.Status = BankBillMinutesStatus.Settled;
        minutes.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "KeToanThanhToan" : settledBy.Trim();
        minutes.SettledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(note))
        {
            minutes.Remark = string.IsNullOrEmpty(minutes.Remark) ? note : $"{minutes.Remark}; {note}";
        }

        await db.SaveChangesAsync();
        return minutes;
    }

    /// <summary>
    /// Hủy biên bản bàn giao xe theo hối phiếu (FrmQuanLyBBBGTheoHoiPhieu.btnDelete_Click).
    /// </summary>
    public async Task<BankBillMinutes?> CancelMinutesAsync(long id, Guid orgId, string? reason)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status == BankBillMinutesStatus.Settled)
            throw new InvalidOperationException("Không thể hủy biên bản hối phiếu đã quyết toán hoàn tất (Settled).");

        minutes.Status = BankBillMinutesStatus.Cancelled;
        minutes.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            minutes.Remark = string.IsNullOrEmpty(minutes.Remark) ? reason : $"{minutes.Remark}; Lý do hủy: {reason}";
        }

        foreach (var dtl in minutes.Details)
        {
            dtl.Status = BankBillDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return minutes;
    }

    /// <summary>
    /// Bổ sung danh sách xe VIN vào biên bản hiện có (FrmTaoBBBGTheoHoiPhieu.btnImport_Click).
    /// </summary>
    public async Task<BankBillMinutes?> ImportVinsAsync(long id, Guid orgId, List<BankBillItemInputDto> newItems)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status != BankBillMinutesStatus.PendingHandover && minutes.Status != BankBillMinutesStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể bổ sung xe vào biên bản ở trạng thái Chờ bàn giao hoặc Dự thảo (Hiện tại: {minutes.Status}).");

        if (newItems == null || newItems.Count == 0)
            return minutes;

        var existingVins = new HashSet<string>(minutes.Details.Select(d => d.VIN), StringComparer.OrdinalIgnoreCase);

        foreach (var item in newItems)
        {
            if (string.IsNullOrWhiteSpace(item.VIN)) continue;
            var vinUpper = item.VIN.Trim().ToUpper();
            if (existingVins.Contains(vinUpper)) continue; // bỏ qua xe trùng

            existingVins.Add(vinUpper);
            var claimAmt = item.ClaimAmount > 0 ? item.ClaimAmount : 0;

            minutes.Details.Add(new BankBillMinutesDetail
            {
                OrgId = orgId,
                MinutesId = minutes.Id,
                VIN = vinUpper,
                ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
                SpecCode = item.SpecCode?.Trim(),
                SpecDescription = item.SpecDescription?.Trim(),
                EngineNo = item.EngineNo?.Trim(),
                CONo = item.CONo?.Trim(),
                CabinCONo = item.CabinCONo?.Trim(),
                DeclarationNo = item.DeclarationNo?.Trim(),
                BankGuaranteeNo = item.BankGuaranteeNo?.Trim(),
                HTCInvoiceNo = item.HTCInvoiceNo?.Trim(),
                TCGInvoiceNo = item.TCGInvoiceNo?.Trim(),
                TransportMinutesNo = item.TransportMinutesNo?.Trim(),
                ClaimAmount = claimAmt,
                GuaranteeDateStart = item.GuaranteeDateStart ?? minutes.BankBillDate,
                GuaranteeDateOpen = item.GuaranteeDateOpen ?? minutes.BankBillDate,
                NumberOfDaysDeferred = item.NumberOfDaysDeferred > 0 ? item.NumberOfDaysDeferred : 30,
                Status = BankBillDetailStatus.Pending,
                Note = item.Note
            });
        }

        minutes.TotalVehicles = minutes.Details.Count(d => d.Status != BankBillDetailStatus.Cancelled);
        minutes.TotalClaimAmount = minutes.Details.Where(d => d.Status != BankBillDetailStatus.Cancelled).Sum(d => d.ClaimAmount);

        await db.SaveChangesAsync();
        return minutes;
    }

    /// <summary>
    /// Xóa 1 xe khỏi biên bản bàn giao (FrmTaoBBBGTheoHoiPhieu.btnDelete_Click).
    /// </summary>
    public async Task<BankBillMinutes?> RemoveVinAsync(long id, long detailId, Guid orgId)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        if (minutes.Status != BankBillMinutesStatus.PendingHandover && minutes.Status != BankBillMinutesStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa xe khỏi biên bản khi chưa xuất trình bàn giao (Hiện tại: {minutes.Status}).");

        var dtl = minutes.Details.FirstOrDefault(d => d.Id == detailId);
        if (dtl != null)
        {
            db.BankBillMinutesDetails.Remove(dtl);
            minutes.Details.Remove(dtl);

            minutes.TotalVehicles = minutes.Details.Count;
            minutes.TotalClaimAmount = minutes.Details.Sum(d => d.ClaimAmount);

            await db.SaveChangesAsync();
        }

        return minutes;
    }

    /// <summary>
    /// Sinh nội dung mẫu in Hối phiếu thương mại ngân hàng (Bank Acceptance Draft / Bill of Exchange Advice - FrmPopupChonMauNHInHoiPhieu & btnExportHoiPhieu_Click).
    /// </summary>
    public async Task<BillOfExchangeAdviceDto?> GenerateBillOfExchangeAdviceAsync(long id, Guid orgId)
    {
        var minutes = await db.BankBillMinutes
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == orgId);

        if (minutes == null) return null;

        var amountWords = PaymentOrderService.NumberToVietnameseWords(minutes.TotalClaimAmount);
        var billNo = $"HP-{minutes.BankCode}-{minutes.BankBillMnNo}";

        // Tóm tắt danh sách số bảo lãnh / LC
        var grtList = minutes.Details
            .Where(d => !string.IsNullOrWhiteSpace(d.BankGuaranteeNo))
            .Select(d => d.BankGuaranteeNo!)
            .Distinct()
            .ToList();
        var grtSummary = grtList.Count > 0 ? string.Join(", ", grtList) : "Theo thỏa thuận tài trợ";

        // Tóm tắt danh sách VIN xe
        var vinSummary = string.Join(", ", minutes.Details.Take(5).Select(d => d.VIN))
            + (minutes.Details.Count > 5 ? $" và {minutes.Details.Count - 5} xe khác" : "");

        var tenorClause = $"Ngay khi nhìn thấy bản thứ nhất của Hối phiếu này (Bản thứ hai có cùng nội dung và ngày tháng không có giá trị thanh toán), Quý Ngân hàng vui lòng thanh toán vô điều kiện theo lệnh của CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM số tiền: {amountWords}";

        var vehicleDtos = minutes.Details.OrderBy(d => d.Id).Select(d => new BillVehicleItemDto(
            VIN: d.VIN,
            ModelCode: d.ModelCode,
            SpecCode: d.SpecCode ?? "",
            SpecDescription: d.SpecDescription ?? d.ModelCode,
            EngineNo: d.EngineNo ?? "",
            CONo: d.CONo ?? "",
            CabinCONo: d.CabinCONo ?? "",
            DeclarationNo: d.DeclarationNo ?? "",
            BankGuaranteeNo: d.BankGuaranteeNo ?? "",
            HTCInvoiceNo: d.HTCInvoiceNo ?? "",
            ClaimAmount: d.ClaimAmount,
            NumberOfDaysDeferred: d.NumberOfDaysDeferred
        )).ToList();

        return new BillOfExchangeAdviceDto(
            BillNo: billNo,
            MinutesNo: minutes.BankBillMnNo,
            IssueDate: minutes.BankBillDate,
            IssuePlace: "Hà Nội, Việt Nam",
            DrawerName: minutes.PartnerName,
            DrawerCode: minutes.PartnerCode,
            DrawerAddress: "Hệ thống Đại lý Ủy quyền Hyundai Thành Công",
            PayeeName: "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
            PayeeAddress: "KCN Gián Khẩu, Xã Gia Tân, Huyện Gia Viễn, Tỉnh Ninh Bình, Việt Nam",
            PayeeTaxCode: "0102607147",
            DraweeBankName: minutes.BankName,
            DraweeBankCode: minutes.BankCode,
            AmountFigures: minutes.TotalClaimAmount,
            AmountWords: amountWords,
            TenorClause: tenorClause,
            GuaranteeSummary: grtSummary,
            VinSummary: vinSummary,
            TotalVehicles: minutes.TotalVehicles,
            BankBillReciveDate: minutes.BankBillReciveDate,
            Status: minutes.Status.ToString(),
            Vehicles: vehicleDtos,
            GeneratedAt: DateTime.Now
        );
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu biên bản bàn giao xe theo hối phiếu ngân hàng.
    /// </summary>
    public async Task<BankBillSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.BankBillMinutes
            .Where(b => b.OrgId == orgId)
            .ToListAsync();

        var totalMinutes = list.Count;
        var pendingHandoverCount = list.Count(b => b.Status == BankBillMinutesStatus.PendingHandover);
        var deliveredCount = list.Count(b => b.Status == BankBillMinutesStatus.Delivered);
        var bankReceivedCount = list.Count(b => b.Status == BankBillMinutesStatus.BankReceived);
        var settledCount = list.Count(b => b.Status == BankBillMinutesStatus.Settled);
        var cancelledCount = list.Count(b => b.Status == BankBillMinutesStatus.Cancelled);

        var totalVehicles = list.Where(b => b.Status != BankBillMinutesStatus.Cancelled).Sum(b => b.TotalVehicles);
        var bankReceivedVehicles = list.Where(b => b.Status == BankBillMinutesStatus.BankReceived || b.Status == BankBillMinutesStatus.Settled).Sum(b => b.TotalVehicles);
        var totalClaimAmount = list.Where(b => b.Status != BankBillMinutesStatus.Cancelled).Sum(b => b.TotalClaimAmount);
        var settledAmount = list.Where(b => b.Status == BankBillMinutesStatus.Settled).Sum(b => b.TotalClaimAmount);

        var recentMinutes = list
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new BankBillRecentItemDto(
                b.Id,
                b.BankBillMnNo,
                b.BankCode,
                b.BankName,
                b.PartnerCode,
                b.PartnerName,
                b.TotalVehicles,
                b.TotalClaimAmount,
                b.BankBillDate,
                b.BankBillReciveDate,
                b.Status.ToString()
            ))
            .ToList();

        return new BankBillSummaryDto(
            TotalMinutes: totalMinutes,
            PendingHandoverCount: pendingHandoverCount,
            DeliveredCount: deliveredCount,
            BankReceivedCount: bankReceivedCount,
            SettledCount: settledCount,
            CancelledCount: cancelledCount,
            TotalVehicles: totalVehicles,
            BankReceivedVehicles: bankReceivedVehicles,
            TotalClaimAmount: totalClaimAmount,
            SettledAmount: settledAmount,
            RecentMinutes: recentMinutes
        );
    }

    public static string ResolveBankName(string code) => code.ToUpperInvariant() switch
    {
        "VPB" or "VPBANK" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
        "CTG" or "VIETINBANK" => "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
        "MBB" or "MB" => "Ngân hàng TMCP Quân Đội (MBBank)",
        "TCB" or "TECHCOMBANK" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
        "VCB" or "VIETCOMBANK" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
        "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
        "ACB" => "Ngân hàng TMCP Á Châu (ACB)",
        "TPB" or "TPBANK" => "Ngân hàng TMCP Tiên Phong (TPBank)",
        "VIB" => "Ngân hàng TMCP Quốc Tế (VIB)",
        _ => $"Ngân hàng {code.ToUpperInvariant()}"
    };
}

public record BankBillItemInputDto(
    string VIN,
    string? ModelCode,
    string? SpecCode,
    string? SpecDescription,
    string? EngineNo,
    string? CONo,
    string? CabinCONo,
    string? DeclarationNo,
    string? BankGuaranteeNo,
    string? HTCInvoiceNo,
    string? TCGInvoiceNo,
    string? TransportMinutesNo,
    long ClaimAmount,
    DateTime? GuaranteeDateStart,
    DateTime? GuaranteeDateOpen,
    int NumberOfDaysDeferred,
    string? Note
);

public record BillVehicleItemDto(
    string VIN,
    string ModelCode,
    string SpecCode,
    string SpecDescription,
    string EngineNo,
    string CONo,
    string CabinCONo,
    string DeclarationNo,
    string BankGuaranteeNo,
    string HTCInvoiceNo,
    long ClaimAmount,
    int NumberOfDaysDeferred
);

public record BillOfExchangeAdviceDto(
    string BillNo,
    string MinutesNo,
    DateTime IssueDate,
    string IssuePlace,
    string DrawerName,
    string DrawerCode,
    string DrawerAddress,
    string PayeeName,
    string PayeeAddress,
    string PayeeTaxCode,
    string DraweeBankName,
    string DraweeBankCode,
    long AmountFigures,
    string AmountWords,
    string TenorClause,
    string GuaranteeSummary,
    string VinSummary,
    int TotalVehicles,
    DateTime? BankBillReciveDate,
    string Status,
    List<BillVehicleItemDto> Vehicles,
    DateTime GeneratedAt
);

public record BankBillRecentItemDto(
    long Id,
    string BankBillMnNo,
    string BankCode,
    string BankName,
    string PartnerCode,
    string PartnerName,
    int TotalVehicles,
    long TotalClaimAmount,
    DateTime BankBillDate,
    DateTime? BankBillReciveDate,
    string Status
);

public record BankBillSummaryDto(
    int TotalMinutes,
    int PendingHandoverCount,
    int DeliveredCount,
    int BankReceivedCount,
    int SettledCount,
    int CancelledCount,
    int TotalVehicles,
    int BankReceivedVehicles,
    long TotalClaimAmount,
    long SettledAmount,
    List<BankBillRecentItemDto> RecentMinutes
);
