using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Thanh toán Chi phí Kiểm tra Xe PDI (Pre-Delivery Inspection Payment Management).
/// Tương ứng khối Pmt_PaymentPDI, Pmt_PaymentPDIDetail và FrmQuanLyThanhToanPDI, FrmSuaThanhToanPDI trong BizHTC.Payment.
/// </summary>
public sealed class PaymentPDIService(AppDbContext db)
{
    /// <summary>
    /// Lập bảng kê thanh toán chi phí PDI xe mới (Job_Pmt_PaymentPDI_Create / FrmQuanLyThanhToanPDI).
    /// </summary>
    public async Task<PaymentPDI> CreatePaymentPDIAsync(
        Guid orgId,
        string? pmtPDINo,
        string pmtMonth,
        string? serviceUnitCode,
        string? serviceUnitName,
        decimal vatRate,
        string? remark,
        string? createdBy,
        List<PaymentPDIItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(pmtMonth))
            throw new ArgumentException("Kỳ / tháng thanh toán (PmtMonth, ví dụ 2025-05) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng kê chi phí PDI cần ít nhất 1 dòng xe kiểm tra.");

        var finalPmtMonth = pmtMonth.Trim();
        var finalPDINo = string.IsNullOrWhiteSpace(pmtPDINo)
            ? $"PDI-{finalPmtMonth.Replace("-", "")}-{Random.Shared.Next(100, 999)}"
            : pmtPDINo.Trim();

        var exists = await db.PaymentPDIs.AnyAsync(p => p.OrgId == orgId && p.PmtPDINo == finalPDINo);
        if (exists)
            throw new InvalidOperationException($"Số bảng kê thanh toán PDI '{finalPDINo}' đã tồn tại trên hệ thống.");

        // Kiểm tra trùng VIN trong cùng bảng kê
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung (VIN) không được để trống.");
            if (!vinSet.Add(it.VIN.Trim().ToUpper()))
                throw new ArgumentException($"Số khung VIN '{it.VIN}' bị trùng lặp trong cùng bảng kê.");
        }

        var finalVatRate = vatRate >= 0 ? vatRate : 10.0m;
        long totalCostIn = 0;
        long totalCostOut = 0;
        var details = new List<PaymentPDIDetail>();

        foreach (var item in items)
        {
            var costIn = item.CostInCheck >= 0 ? item.CostInCheck : 150_000;
            var costOut = item.CostOutCheck >= 0 ? item.CostOutCheck : 200_000;
            var totalCost = costIn + costOut;

            totalCostIn += costIn;
            totalCostOut += costOut;

            details.Add(new PaymentPDIDetail
            {
                OrgId = orgId,
                VIN = item.VIN.Trim().ToUpper(),
                CarId = item.CarId?.Trim(),
                ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
                ModelName = item.ModelName?.Trim(),
                SpecCode = item.SpecCode?.Trim(),
                SpecDescription = item.SpecDescription?.Trim(),
                ColorExtNameVN = item.ColorExtNameVN?.Trim(),
                StorageCodeInit = string.IsNullOrWhiteSpace(item.StorageCodeInit) ? "KHO-NINHBINH" : item.StorageCodeInit.Trim().ToUpper(),
                StoreDate = item.StoreDate,
                DeliveryOutDate = item.DeliveryOutDate,
                DlvMnNo = item.DlvMnNo?.Trim(),
                DealerCode = item.DealerCode?.Trim().ToUpper(),
                CostInCheck = costIn,
                CostOutCheck = costOut,
                TotalCostCheck = totalCost,
                Status = PaymentPDIDetailStatus.Pending,
                Remark = item.Remark
            });
        }

        long totalAmount = totalCostIn + totalCostOut;
        long amountVat = (long)Math.Round(totalAmount * (finalVatRate / 100m), MidpointRounding.AwayFromZero);
        long totalAfterVat = totalAmount + amountVat;

        var pdi = new PaymentPDI
        {
            OrgId = orgId,
            PmtPDINo = finalPDINo,
            PmtMonth = finalPmtMonth,
            ServiceUnitCode = string.IsNullOrWhiteSpace(serviceUnitCode) ? "TCMS" : serviceUnitCode.Trim().ToUpper(),
            ServiceUnitName = string.IsNullOrWhiteSpace(serviceUnitName) ? "Trung tâm Dịch vụ Kỹ thuật & PDI Ô tô TCMS" : serviceUnitName.Trim(),
            TotalVehicles = details.Count,
            TotalCostIn = totalCostIn,
            TotalCostOut = totalCostOut,
            TotalAmount = totalAmount,
            VATRate = finalVatRate,
            AmountVAT = amountVat,
            TotalAmountAfterVAT = totalAfterVat,
            Status = PaymentPDIStatus.Draft,
            TCMSSignStatus = PDISignStatus.Pending,
            HTVSignStatus = PDISignStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = createdBy ?? "ChuyenVienKiemDinh",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PaymentPDIs.Add(pdi);
        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Lấy danh sách bảng kê PDI có lọc theo trạng thái, tháng và đơn vị dịch vụ (FrmQuanLyThanhToanPDI.btnSearch_Click).
    /// </summary>
    public async Task<List<PaymentPDI>> GetPaymentPDIsAsync(
        Guid orgId,
        string? status = null,
        string? pmtMonth = null,
        string? serviceUnitCode = null)
    {
        var q = db.PaymentPDIs
            .AsNoTracking()
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentPDIStatus>(status, true, out var st))
            q = q.Where(p => p.Status == st);

        if (!string.IsNullOrWhiteSpace(pmtMonth))
            q = q.Where(p => p.PmtMonth == pmtMonth.Trim());

        if (!string.IsNullOrWhiteSpace(serviceUnitCode))
            q = q.Where(p => p.ServiceUnitCode == serviceUnitCode.Trim().ToUpper());

        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết 1 bảng kê thanh toán PDI kèm danh sách toàn bộ xe (FrmQuanLyThanhToanPDI.loadGridDetail).
    /// </summary>
    public async Task<PaymentPDI?> GetPaymentPDIByIdAsync(long id, Guid orgId)
    {
        return await db.PaymentPDIs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// TCMS Phê duyệt cấp 1 & Ký số điện tử (Pmt_PaymentPDI_TCMSApproveAndSign / FrmQuanLyThanhToanPDI.btnTCMSApprove_Click).
    /// </summary>
    public async Task<PaymentPDI?> ApproveTCMSAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var pdi = await db.PaymentPDIs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (pdi == null) return null;

        if (pdi.Status != PaymentPDIStatus.Draft)
            throw new InvalidOperationException($"Bảng kê đang ở trạng thái '{pdi.Status}', chỉ bảng kê 'Draft' mới được duyệt cấp 1 (TCMS).");

        pdi.Status = PaymentPDIStatus.TCMSApproved;
        pdi.TCMSSignStatus = PDISignStatus.Signed;
        pdi.TCMSSignUser = string.IsNullOrWhiteSpace(signerName) ? "GiamDocKyThuat_TCMS" : signerName.Trim();
        pdi.TCMSSignDTime = DateTime.Now;
        pdi.Appr1By = pdi.TCMSSignUser;
        pdi.Appr1DTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) pdi.FilePath = filePath.Trim();

        foreach (var dtl in pdi.Details)
        {
            if (dtl.Status == PaymentPDIDetailStatus.Pending)
                dtl.Status = PaymentPDIDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// HTV Phê duyệt cấp 2 & Ký số điện tử (Pmt_PaymentPDI_HTVApproveAndSign / FrmQuanLyThanhToanPDI.btnHTVApprove_Click).
    /// </summary>
    public async Task<PaymentPDI?> ApproveHTVAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var pdi = await db.PaymentPDIs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (pdi == null) return null;

        if (pdi.Status != PaymentPDIStatus.TCMSApproved)
            throw new InvalidOperationException($"Bảng kê đang ở trạng thái '{pdi.Status}', cần hoàn tất duyệt cấp 1 TCMS trước khi HTV duyệt cấp 2.");

        pdi.Status = PaymentPDIStatus.HTVApproved;
        pdi.HTVSignStatus = PDISignStatus.Signed;
        pdi.HTVSignUser = string.IsNullOrWhiteSpace(signerName) ? "TruongPhongKeToan_HTV" : signerName.Trim();
        pdi.HTVSignDTime = DateTime.Now;
        pdi.Appr2By = pdi.HTVSignUser;
        pdi.Appr2DTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) pdi.FilePath = filePath.Trim();

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Hoàn tất thanh toán / Quyết toán chi phí PDI xe (Paid / Settled).
    /// </summary>
    public async Task<PaymentPDI?> SettlePaymentPDIAsync(long id, Guid orgId, string? bankTxnRef, string? payerName)
    {
        var pdi = await db.PaymentPDIs.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (pdi == null) return null;

        if (pdi.Status != PaymentPDIStatus.HTVApproved)
            throw new InvalidOperationException($"Chỉ bảng kê đã duyệt 2 cấp ('HTVApproved') mới được quyết toán thanh toán.");

        pdi.Status = PaymentPDIStatus.Paid;
        pdi.PaidBy = string.IsNullOrWhiteSpace(payerName) ? "KeToanNganHang" : payerName.Trim();
        pdi.PaidAt = DateTime.Now;
        pdi.BankTxnRef = string.IsNullOrWhiteSpace(bankTxnRef) ? $"UNC-PDI-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" : bankTxnRef.Trim();

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Từ chối duyệt bảng kê PDI kèm lý do (FrmQuanLyThanhToanPDI.btnDeny_Click).
    /// </summary>
    public async Task<PaymentPDI?> RejectPaymentPDIAsync(long id, Guid orgId, string reason, string? rejecterName)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cần cung cấp lý do từ chối (RejectReason).");

        var pdi = await db.PaymentPDIs.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (pdi == null) return null;

        if (pdi.Status == PaymentPDIStatus.Paid || pdi.Status == PaymentPDIStatus.Cancelled)
            throw new InvalidOperationException($"Không thể từ chối bảng kê ở trạng thái '{pdi.Status}'.");

        pdi.Status = PaymentPDIStatus.Rejected;
        pdi.RejectReason = reason.Trim();
        pdi.Appr1By = rejecterName ?? "CapThamQuyen";
        pdi.Appr1DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Hủy bảng kê chi phí PDI (Pmt_PaymentPDI_Cancel / FrmQuanLyThanhToanPDI.btnDelete_Click).
    /// </summary>
    public async Task<PaymentPDI?> CancelPaymentPDIAsync(long id, Guid orgId, string? reason)
    {
        var pdi = await db.PaymentPDIs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (pdi == null) return null;

        if (pdi.Status == PaymentPDIStatus.Paid)
            throw new InvalidOperationException("Bảng kê PDI đã hoàn tất thanh toán (Paid), không thể hủy.");

        pdi.Status = PaymentPDIStatus.Cancelled;
        pdi.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
            pdi.Remark = string.IsNullOrWhiteSpace(pdi.Remark) ? $"Lý do hủy: {reason}" : $"{pdi.Remark} | Lý do hủy: {reason}";

        foreach (var dtl in pdi.Details)
        {
            dtl.Status = PaymentPDIDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Cập nhật chi phí kiểm tra PDI của từng dòng xe (Pmt_PaymentPDI_UpdateMulti / FrmSuaThanhToanPDI.btnSave_Click).
    /// Tự động cập nhật lại tổng tiền chi phí nhập kho, xuất kho, tổng tiền trước VAT, thuế VAT và tổng thanh toán.
    /// </summary>
    public async Task<PaymentPDI?> UpdateDetailsAsync(long id, Guid orgId, List<UpdatePDIDetailItemDto> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Danh sách cập nhật chi phí không được để trống.");

        var pdi = await db.PaymentPDIs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (pdi == null) return null;

        if (pdi.Status == PaymentPDIStatus.Paid || pdi.Status == PaymentPDIStatus.Cancelled)
            throw new InvalidOperationException($"Không thể điều chỉnh chi phí khi bảng kê ở trạng thái '{pdi.Status}'.");

        var detailDict = pdi.Details.ToDictionary(d => d.VIN, StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN)) continue;
            var vinKey = item.VIN.Trim().ToUpper();

            if (detailDict.TryGetValue(vinKey, out var dtl))
            {
                if (item.CostInCheck.HasValue && item.CostInCheck.Value >= 0)
                    dtl.CostInCheck = item.CostInCheck.Value;
                if (item.CostOutCheck.HasValue && item.CostOutCheck.Value >= 0)
                    dtl.CostOutCheck = item.CostOutCheck.Value;

                dtl.TotalCostCheck = dtl.CostInCheck + dtl.CostOutCheck;
                dtl.Status = PaymentPDIDetailStatus.Adjusted;
                if (!string.IsNullOrWhiteSpace(item.Remark)) dtl.Remark = item.Remark.Trim();
            }
        }

        // Tự động tính toán lại tổng tiền toàn bảng kê
        long newCostIn = pdi.Details.Where(d => d.Status != PaymentPDIDetailStatus.Cancelled).Sum(d => d.CostInCheck);
        long newCostOut = pdi.Details.Where(d => d.Status != PaymentPDIDetailStatus.Cancelled).Sum(d => d.CostOutCheck);
        long newTotalAmount = newCostIn + newCostOut;
        long newAmountVat = (long)Math.Round(newTotalAmount * (pdi.VATRate / 100m), MidpointRounding.AwayFromZero);

        pdi.TotalCostIn = newCostIn;
        pdi.TotalCostOut = newCostOut;
        pdi.TotalAmount = newTotalAmount;
        pdi.AmountVAT = newAmountVat;
        pdi.TotalAmountAfterVAT = newTotalAmount + newAmountVat;

        // Nếu bảng kê đã được ký, chuyển trạng thái cần duyệt lại sau khi chỉnh sửa
        if (pdi.Status == PaymentPDIStatus.TCMSApproved || pdi.Status == PaymentPDIStatus.HTVApproved)
        {
            pdi.Status = PaymentPDIStatus.Draft;
            pdi.TCMSSignStatus = PDISignStatus.Pending;
            pdi.HTVSignStatus = PDISignStatus.Pending;
            pdi.Remark = string.IsNullOrWhiteSpace(pdi.Remark) ? "Đã điều chỉnh chi phí PDI, yêu cầu phê duyệt lại." : $"{pdi.Remark} (Đã điều chỉnh chi phí)";
        }

        await db.SaveChangesAsync();
        return pdi;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí PDI (PDI Payment Statement Advice / FrmQuanLyThanhToanPDI.btnPrint_Click).
    /// Kèm thuật toán đọc số tiền thành chữ tiếng Việt kế toán tài chính chuẩn.
    /// </summary>
    public async Task<PDIStatementAdviceDto?> GenerateStatementAdviceAsync(long id, Guid orgId)
    {
        var pdi = await db.PaymentPDIs
            .AsNoTracking()
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (pdi == null) return null;

        var amountInWords = ConvertNumberToWords(pdi.TotalAmountAfterVAT);

        return new PDIStatementAdviceDto
        {
            PdiId = pdi.Id,
            PmtPDINo = pdi.PmtPDINo,
            PmtMonth = pdi.PmtMonth,
            ServiceUnitCode = pdi.ServiceUnitCode,
            ServiceUnitName = pdi.ServiceUnitName,
            TotalVehicles = pdi.TotalVehicles,
            TotalCostIn = pdi.TotalCostIn,
            TotalCostOut = pdi.TotalCostOut,
            TotalAmount = pdi.TotalAmount,
            VATRate = pdi.VATRate,
            AmountVAT = pdi.AmountVAT,
            TotalAmountAfterVAT = pdi.TotalAmountAfterVAT,
            AmountInWords = amountInWords,
            Status = pdi.Status.ToString(),
            TCMSSignStatus = pdi.TCMSSignStatus.ToString(),
            TCMSSignUser = pdi.TCMSSignUser ?? "Chưa ký",
            TCMSSignDTime = pdi.TCMSSignDTime?.ToString("dd/MM/yyyy HH:mm"),
            HTVSignStatus = pdi.HTVSignStatus.ToString(),
            HTVSignUser = pdi.HTVSignUser ?? "Chưa ký",
            HTVSignDTime = pdi.HTVSignDTime?.ToString("dd/MM/yyyy HH:mm"),
            BankTxnRef = pdi.BankTxnRef,
            PaidAt = pdi.PaidAt?.ToString("dd/MM/yyyy"),
            CreatedAt = pdi.CreatedAt.ToString("dd/MM/yyyy"),
            Remark = pdi.Remark,
            Vehicles = pdi.Details.Select((d, index) => new PDIVehicleLineDto
            {
                No = index + 1,
                VIN = d.VIN,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? d.ModelCode,
                SpecDescription = d.SpecDescription ?? "",
                ColorExt = d.ColorExtNameVN ?? "",
                StorageCode = d.StorageCodeInit,
                StoreDate = d.StoreDate?.ToString("dd/MM/yyyy") ?? "-",
                DeliveryOutDate = d.DeliveryOutDate?.ToString("dd/MM/yyyy") ?? "-",
                DealerCode = d.DealerCode ?? "-",
                CostInCheck = d.CostInCheck,
                CostOutCheck = d.CostOutCheck,
                TotalCostCheck = d.TotalCostCheck,
                Status = d.Status.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu PDI (PDI Payment Summary Report).
    /// </summary>
    public async Task<PDISummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.PaymentPDIs
            .AsNoTracking()
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        return new PDISummaryDto
        {
            TotalBatches = list.Count,
            TotalVehicles = list.Sum(p => p.TotalVehicles),
            TotalPDIExpense = list.Sum(p => p.TotalAmountAfterVAT),
            DraftBatches = list.Count(p => p.Status == PaymentPDIStatus.Draft),
            TCMSApprovedBatches = list.Count(p => p.Status == PaymentPDIStatus.TCMSApproved),
            HTVApprovedBatches = list.Count(p => p.Status == PaymentPDIStatus.HTVApproved),
            PaidBatches = list.Count(p => p.Status == PaymentPDIStatus.Paid),
            PaidExpense = list.Where(p => p.Status == PaymentPDIStatus.Paid).Sum(p => p.TotalAmountAfterVAT),
            PendingExpense = list.Where(p => p.Status == PaymentPDIStatus.Draft || p.Status == PaymentPDIStatus.TCMSApproved || p.Status == PaymentPDIStatus.HTVApproved).Sum(p => p.TotalAmountAfterVAT)
        };
    }

    /// <summary>
    /// Thuật toán chuyển đổi số tiền (VND) thành chữ tiếng Việt chuẩn quy chuẩn kế toán.
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
        return System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
    }
}

public sealed class PaymentPDIItemInputDto
{
    public string VIN { get; set; } = "";
    public string? CarId { get; set; }
    public string? ModelCode { get; set; }
    public string? ModelName { get; set; }
    public string? SpecCode { get; set; }
    public string? SpecDescription { get; set; }
    public string? ColorExtNameVN { get; set; }
    public string? StorageCodeInit { get; set; }
    public DateTime? StoreDate { get; set; }
    public DateTime? DeliveryOutDate { get; set; }
    public string? DlvMnNo { get; set; }
    public string? DealerCode { get; set; }
    public long CostInCheck { get; set; } = 150_000;
    public long CostOutCheck { get; set; } = 200_000;
    public string? Remark { get; set; }
}

public sealed class UpdatePDIDetailItemDto
{
    public string VIN { get; set; } = "";
    public long? CostInCheck { get; set; }
    public long? CostOutCheck { get; set; }
    public string? Remark { get; set; }
}

public sealed class PDIStatementAdviceDto
{
    public long PdiId { get; set; }
    public string PmtPDINo { get; set; } = "";
    public string PmtMonth { get; set; } = "";
    public string ServiceUnitCode { get; set; } = "";
    public string ServiceUnitName { get; set; } = "";
    public int TotalVehicles { get; set; }
    public long TotalCostIn { get; set; }
    public long TotalCostOut { get; set; }
    public long TotalAmount { get; set; }
    public decimal VATRate { get; set; }
    public long AmountVAT { get; set; }
    public long TotalAmountAfterVAT { get; set; }
    public string AmountInWords { get; set; } = "";
    public string Status { get; set; } = "";
    public string TCMSSignStatus { get; set; } = "";
    public string TCMSSignUser { get; set; } = "";
    public string? TCMSSignDTime { get; set; }
    public string HTVSignStatus { get; set; } = "";
    public string HTVSignUser { get; set; } = "";
    public string? HTVSignDTime { get; set; }
    public string? BankTxnRef { get; set; }
    public string? PaidAt { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? Remark { get; set; }
    public List<PDIVehicleLineDto> Vehicles { get; set; } = [];
}

public sealed class PDIVehicleLineDto
{
    public int No { get; set; }
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecDescription { get; set; } = "";
    public string ColorExt { get; set; } = "";
    public string StorageCode { get; set; } = "";
    public string StoreDate { get; set; } = "";
    public string DeliveryOutDate { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public long CostInCheck { get; set; }
    public long CostOutCheck { get; set; }
    public long TotalCostCheck { get; set; }
    public string Status { get; set; } = "";
}

public sealed class PDISummaryDto
{
    public int TotalBatches { get; set; }
    public int TotalVehicles { get; set; }
    public long TotalPDIExpense { get; set; }
    public int DraftBatches { get; set; }
    public int TCMSApprovedBatches { get; set; }
    public int HTVApprovedBatches { get; set; }
    public int PaidBatches { get; set; }
    public long PaidExpense { get; set; }
    public long PendingExpense { get; set; }
}
