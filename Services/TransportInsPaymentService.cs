using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Thanh toán Chi phí Vận tải & Bảo hiểm xe (Vehicle Transport & Freight Insurance Payment Management).
/// Tương ứng khối Pmt_TransportIns, Pmt_TransportInsDetail và FrmQuanLyThanhToanVanTaiBaoHiem, FrmTaoThanhToanVanTaiBaoHiem trong BizHTC.Payment / DMS40.Contract.
/// </summary>
public sealed class TransportInsPaymentService(AppDbContext db)
{
    /// <summary>
    /// Lập bảng kê thanh toán chi phí vận chuyển & bảo hiểm mới (Pmt_TransportIns_Save / FrmTaoThanhToanVanTaiBaoHiem).
    /// </summary>
    public async Task<TransportInsPayment> CreatePaymentAsync(
        Guid orgId,
        string? transportInsNo,
        string pmtMonth,
        string transporterCode,
        string? transporterName,
        string? insuranceCompanyCode,
        string? insuranceCompanyName,
        string? insuranceContractNo,
        decimal vatRate,
        string? remark,
        string? createdBy,
        List<TransportInsItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(pmtMonth))
            throw new ArgumentException("Kỳ / tháng thanh toán (PmtMonth, ví dụ 2025-05) không được để trống.");
        if (string.IsNullOrWhiteSpace(transporterCode))
            throw new ArgumentException("Mã đơn vị vận tải (TransporterCode) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng kê chi phí vận tải & bảo hiểm cần ít nhất 1 dòng xe vận chuyển.");

        var finalPmtMonth = pmtMonth.Trim();
        var finalTransCode = transporterCode.Trim().ToUpper();
        var finalInsNo = string.IsNullOrWhiteSpace(transportInsNo)
            ? $"VTBH-{finalPmtMonth.Replace("-", "")}-{Random.Shared.Next(100, 999)}"
            : transportInsNo.Trim().ToUpper();

        var exists = await db.TransportInsPayments.AnyAsync(p => p.OrgId == orgId && p.TransportInsNo == finalInsNo);
        if (exists)
            throw new InvalidOperationException($"Số bảng kê vận tải & bảo hiểm '{finalInsNo}' đã tồn tại trên hệ thống.");

        // Kiểm tra trùng VIN trong cùng bảng kê
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung xe (VIN) không được để trống.");
            if (!vinSet.Add(it.VIN.Trim().ToUpper()))
                throw new ArgumentException($"Số khung VIN '{it.VIN}' bị trùng lặp trong cùng bảng kê.");
        }

        var finalVatRate = vatRate >= 0 ? vatRate : 10.0m;
        var details = new List<TransportInsPaymentDetail>();

        foreach (var item in items)
        {
            var detail = BuildDetailItem(orgId, item);
            details.Add(detail);
        }

        long totalTransportCost = details.Sum(d => d.TFValReal);
        long totalDelayPenalty = details.Sum(d => d.TPValReal);
        long totalInsuranceCost = details.Sum(d => d.InsuranceCost);
        long totalAmount = details.Sum(d => d.Val_Transport); // Val_Transport = TFValReal + InsuranceCost - TPValReal

        // Công thức tính thuế: TotalBeforeVAT = TotalAmount / (1 + VATRate/100)
        long totalBeforeVat = (long)Math.Round(totalAmount / (1.0m + finalVatRate / 100m), MidpointRounding.AwayFromZero);
        long amountVat = totalAmount - totalBeforeVat;

        var payment = new TransportInsPayment
        {
            OrgId = orgId,
            TransportInsNo = finalInsNo,
            PmtMonth = finalPmtMonth,
            TransporterCode = finalTransCode,
            TransporterName = string.IsNullOrWhiteSpace(transporterName) ? ResolveTransporterName(finalTransCode) : transporterName.Trim(),
            InsuranceCompanyCode = string.IsNullOrWhiteSpace(insuranceCompanyCode) ? "PVI" : insuranceCompanyCode.Trim().ToUpper(),
            InsuranceCompanyName = string.IsNullOrWhiteSpace(insuranceCompanyName) ? ResolveInsuranceCompanyName(insuranceCompanyCode ?? "PVI") : insuranceCompanyName.Trim(),
            InsuranceContractNo = insuranceContractNo?.Trim(),
            TotalVehicles = details.Count,
            TotalTransportCost = totalTransportCost,
            TotalDelayPenalty = totalDelayPenalty,
            TotalInsuranceCost = totalInsuranceCost,
            TotalAmount = totalAmount,
            VATRate = finalVatRate,
            TotalBeforeVAT = totalBeforeVat,
            AmountVAT = amountVat,
            Status = TransportInsStatus.Draft,
            TCMSSignStatus = TransportSignCAStatus.Pending,
            HTVSignStatus = TransportSignCAStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = createdBy ?? "ChuyenVienDieuVan",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.TransportInsPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// TCMS Thẩm định duyệt cấp 1 (Pmt_TransportIns_Approve1 / FrmQuanLyThanhToanVanTaiBaoHiem.btnTCMSApprove_Click).
    /// </summary>
    public async Task<TransportInsPayment?> ApproveStep1TCMSAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != TransportInsStatus.Draft)
            throw new InvalidOperationException($"Chỉ bảng kê ở trạng thái 'Draft' mới có thể duyệt cấp 1 TCMS. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = TransportInsStatus.TCMSApproved;
        payment.Appr1By = string.IsNullOrWhiteSpace(approverName) ? "TCMS_Supervisor" : approverName.Trim();
        payment.Appr1DTime = DateTime.Now;

        foreach (var d in payment.Details)
        {
            if (d.Status == TransportInsDetailStatus.Pending)
                d.Status = TransportInsDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// HTV Lãnh đạo duyệt cấp 2 (Pmt_TransportIns_Approve2 / FrmQuanLyThanhToanVanTaiBaoHiem.btnHTVApprove_Click).
    /// </summary>
    public async Task<TransportInsPayment?> ApproveStep2HTVAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != TransportInsStatus.TCMSApproved)
            throw new InvalidOperationException($"Bảng kê cần được TCMS duyệt cấp 1 trước khi HTV duyệt cấp 2. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = TransportInsStatus.HTVApproved;
        payment.Appr2By = string.IsNullOrWhiteSpace(approverName) ? "HTV_Director" : approverName.Trim();
        payment.Appr2DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện TCMS (Pmt_TransportIns_TCMSApproveAndSign / FrmQuanLyThanhToanVanTaiBaoHiem.btnTCMSSign_Click).
    /// </summary>
    public async Task<TransportInsPayment?> SignTCMSAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.TransportInsPayments
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Cancelled || payment.Status == TransportInsStatus.Rejected)
            throw new InvalidOperationException($"Không thể ký số bảng kê đã bị hủy hoặc từ chối.");

        payment.TCMSSignStatus = TransportSignCAStatus.Signed;
        payment.TCMSSignUser = string.IsNullOrWhiteSpace(signerName) ? "TCMS_AuthorizedSigner" : signerName.Trim();
        payment.TCMSSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) payment.FilePath = filePath.Trim();

        // Nếu cả TCMS và HTV đã ký số thì tự động hoàn tất ký duyệt (Signed / Finished)
        if (payment.HTVSignStatus == TransportSignCAStatus.Signed && payment.Status != TransportInsStatus.Settled)
        {
            payment.Status = TransportInsStatus.Signed;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện HTV (Pmt_TransportIns_HTVApproveAndSign / FrmQuanLyThanhToanVanTaiBaoHiem.btnHTVSign_Click).
    /// </summary>
    public async Task<TransportInsPayment?> SignHTVAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.TransportInsPayments
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Cancelled || payment.Status == TransportInsStatus.Rejected)
            throw new InvalidOperationException($"Không thể ký số bảng kê đã bị hủy hoặc từ chối.");

        payment.HTVSignStatus = TransportSignCAStatus.Signed;
        payment.HTVSignUser = string.IsNullOrWhiteSpace(signerName) ? "HTV_AuthorizedSigner" : signerName.Trim();
        payment.HTVSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) payment.FilePath = filePath.Trim();

        // Khi HTV ký số hoàn tất, chuyển trạng thái bảng kê sang Signed
        payment.Status = TransportInsStatus.Signed;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Quyết toán chi trả chi phí vận tải & bảo hiểm qua UNC ngân hàng (Settled / Paid).
    /// </summary>
    public async Task<TransportInsPayment?> SettlePaymentAsync(long id, Guid orgId, string? bankTxnRef, string? settledBy)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != TransportInsStatus.Signed && payment.Status != TransportInsStatus.HTVApproved)
            throw new InvalidOperationException($"Bảng kê cần được phê duyệt và ký số hoàn tất trước khi quyết toán thanh toán. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = TransportInsStatus.Settled;
        payment.BankTxnRef = string.IsNullOrWhiteSpace(bankTxnRef) ? $"UNC-VTBH-{DateTime.Now:yyyyMMddHHmm}" : bankTxnRef.Trim();
        payment.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "KeToanNganHang" : settledBy.Trim();
        payment.SettledAt = DateTime.Now;

        foreach (var d in payment.Details)
        {
            d.Status = TransportInsDetailStatus.Settled;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Từ chối bảng kê kèm lý do (Pmt_TransportIns_Cancel / btnDeny_Click).
    /// </summary>
    public async Task<TransportInsPayment?> RejectPaymentAsync(long id, Guid orgId, string? reason, string? rejecterName)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Settled)
            throw new InvalidOperationException("Không thể từ chối bảng kê đã quyết toán chuyển tiền thanh toán.");

        payment.Status = TransportInsStatus.Rejected;
        payment.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Từ chối do sai lệch chi phí / cung đường vận tải" : reason.Trim();
        payment.CancelledAt = DateTime.Now;

        foreach (var d in payment.Details)
        {
            d.Status = TransportInsDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy bảng kê vận tải & bảo hiểm (Pmt_TransportIns_Cancel / btnDelete_Click).
    /// </summary>
    public async Task<TransportInsPayment?> CancelPaymentAsync(long id, Guid orgId, string? reason)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Settled)
            throw new InvalidOperationException("Không thể hủy bảng kê đã quyết toán thanh toán.");

        payment.Status = TransportInsStatus.Cancelled;
        payment.RejectReason = reason?.Trim();
        payment.CancelledAt = DateTime.Now;

        foreach (var d in payment.Details)
        {
            d.Status = TransportInsDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Cập nhật điều chỉnh chi phí xe hàng loạt (Pmt_TransportIns_UpdateMulti / FrmTaoThanhToanVanTaiBaoHiem).
    /// Tự động tái tính toán tổng cước vận chuyển, phạt chậm, phí bảo hiểm và thuế VAT.
    /// </summary>
    public async Task<TransportInsPayment?> UpdateDetailsAsync(long id, Guid orgId, List<UpdateTransportDetailItemDto> items)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Settled)
            throw new InvalidOperationException("Không thể cập nhật chi phí cho bảng kê đã quyết toán thanh toán.");

        var dict = payment.Details.ToDictionary(d => d.Id);

        foreach (var it in items)
        {
            if (!dict.TryGetValue(it.DetailId, out var detail)) continue;

            if (it.TFValReal.HasValue && it.TFValReal.Value >= 0)
                detail.TFValReal = it.TFValReal.Value;

            if (it.TPValReal.HasValue && it.TPValReal.Value >= 0)
                detail.TPValReal = it.TPValReal.Value;

            if (it.PriceCar.HasValue && it.PriceCar.Value >= 0)
                detail.PriceCar = it.PriceCar.Value;

            if (it.InsurancePercent.HasValue && it.InsurancePercent.Value >= 0)
                detail.InsurancePercent = it.InsurancePercent.Value;

            detail.InsuranceCost = (long)Math.Round(detail.PriceCar * (detail.InsurancePercent / 100m), MidpointRounding.AwayFromZero);
            detail.Val_Transport = detail.TFValReal + detail.InsuranceCost - detail.TPValReal;

            if (it.StandardRemark != null) detail.StandardRemark = it.StandardRemark.Trim();
            if (it.FProvinceRemark != null) detail.FProvinceRemark = it.FProvinceRemark.Trim();
            if (it.Remark != null) detail.Remark = it.Remark.Trim();

            detail.Status = TransportInsDetailStatus.Adjusted;
        }

        // Tái tính toán Header
        RecalculateHeader(payment);

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Bổ sung danh sách xe vào bảng kê hiện có (FrmTaoThanhToanVanTaiBaoHiem_AddCar).
    /// </summary>
    public async Task<TransportInsPayment?> ImportVehiclesAsync(long id, Guid orgId, List<TransportInsItemInputDto> items)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Settled || payment.Status == TransportInsStatus.Cancelled)
            throw new InvalidOperationException($"Không thể bổ sung xe vào bảng kê ở trạng thái {payment.Status}.");

        var existingVins = new HashSet<string>(payment.Details.Select(d => d.VIN), StringComparer.OrdinalIgnoreCase);

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN)) continue;
            var vin = it.VIN.Trim().ToUpper();
            if (existingVins.Contains(vin)) continue;

            var detail = BuildDetailItem(orgId, it);
            detail.TransportInsPaymentId = payment.Id;
            payment.Details.Add(detail);
            existingVins.Add(vin);
        }

        RecalculateHeader(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Xóa 1 xe khỏi bảng kê vận tải & bảo hiểm.
    /// </summary>
    public async Task<TransportInsPayment?> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == TransportInsStatus.Settled || payment.Status == TransportInsStatus.Cancelled)
            throw new InvalidOperationException($"Không thể xóa xe khỏi bảng kê ở trạng thái {payment.Status}.");

        var detail = payment.Details.FirstOrDefault(d => d.Id == detailId);
        if (detail == null) return payment;

        payment.Details.Remove(detail);
        db.TransportInsPaymentDetails.Remove(detail);

        RecalculateHeader(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí vận chuyển & bảo hiểm xe (CR_Pmt_TransportIns Advice).
    /// Bao gồm bảng kê chi tiết từng xe, tổng cước, phạt trễ hạn, bảo hiểm, thuế VAT và đọc số tiền bằng chữ tiếng Việt chuẩn ngân hàng.
    /// </summary>
    public async Task<TransportInsAdviceDto?> GenerateStatementAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.TransportInsPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        var inWords = NumberToVietnameseWords(payment.TotalAmount);

        var lines = payment.Details
            .OrderBy(d => d.Id)
            .Select((d, idx) => new TransportInsAdviceLineDto(
                d.Id,
                idx + 1,
                d.VIN,
                d.ModelCode,
                d.ModelName,
                d.ColorName,
                d.FStorageCode,
                d.FProvinceName,
                d.TStorageCode,
                d.TProvinceName,
                d.TranspReqType,
                d.DlvMnNo,
                d.DlvStartDate,
                d.ExpectedDays,
                d.ExpectedDlvEndDate,
                d.DlvEndDate,
                d.DelayDays,
                d.TFValReal,
                d.TPValReal,
                d.PriceCar,
                d.InsurancePercent,
                d.InsuranceCost,
                d.Val_Transport,
                d.StandardRemark,
                d.FProvinceRemark,
                d.Status.ToString(),
                d.Remark
            ))
            .ToList();

        return new TransportInsAdviceDto(
            payment.Id,
            payment.TransportInsNo,
            payment.PmtMonth,
            payment.TransporterCode,
            payment.TransporterName,
            payment.InsuranceCompanyCode,
            payment.InsuranceCompanyName,
            payment.InsuranceContractNo,
            payment.TotalVehicles,
            payment.TotalTransportCost,
            payment.TotalDelayPenalty,
            payment.TotalInsuranceCost,
            payment.TotalBeforeVAT,
            payment.VATRate,
            payment.AmountVAT,
            payment.TotalAmount,
            inWords,
            payment.Status.ToString(),
            payment.TCMSSignStatus.ToString(),
            payment.TCMSSignUser,
            payment.TCMSSignDTime,
            payment.HTVSignStatus.ToString(),
            payment.HTVSignUser,
            payment.HTVSignDTime,
            payment.Appr1By,
            payment.Appr1DTime,
            payment.Appr2By,
            payment.Appr2DTime,
            payment.SettledBy,
            payment.SettledAt,
            payment.BankTxnRef,
            payment.Remark,
            payment.CreatedAt,
            lines
        );
    }

    /// <summary>
    /// Thống kê dashboard tổng hợp số liệu thanh toán vận tải & bảo hiểm xe.
    /// </summary>
    public async Task<TransportInsSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.TransportInsPayments
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        int total = list.Count;
        int draft = list.Count(p => p.Status == TransportInsStatus.Draft);
        int tcmsAppr = list.Count(p => p.Status == TransportInsStatus.TCMSApproved);
        int htvAppr = list.Count(p => p.Status == TransportInsStatus.HTVApproved);
        int signed = list.Count(p => p.Status == TransportInsStatus.Signed);
        int settled = list.Count(p => p.Status == TransportInsStatus.Settled);
        int cancelled = list.Count(p => p.Status == TransportInsStatus.Cancelled || p.Status == TransportInsStatus.Rejected);

        int totalVehicles = list.Sum(p => p.TotalVehicles);
        long totalTransCost = list.Sum(p => p.TotalTransportCost);
        long totalDelayPen = list.Sum(p => p.TotalDelayPenalty);
        long totalInsCost = list.Sum(p => p.TotalInsuranceCost);
        long settledAmount = list.Where(p => p.Status == TransportInsStatus.Settled).Sum(p => p.TotalAmount);
        long pendingAmount = list.Where(p => p.Status != TransportInsStatus.Settled && p.Status != TransportInsStatus.Cancelled && p.Status != TransportInsStatus.Rejected).Sum(p => p.TotalAmount);

        return new TransportInsSummaryDto(
            total,
            draft,
            tcmsAppr,
            htvAppr,
            signed,
            settled,
            cancelled,
            totalVehicles,
            totalTransCost,
            totalDelayPen,
            totalInsCost,
            settledAmount,
            pendingAmount
        );
    }

    // ==================== Helper methods ====================

    private static TransportInsPaymentDetail BuildDetailItem(Guid orgId, TransportInsItemInputDto item)
    {
        var vin = item.VIN.Trim().ToUpper();
        var modelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper();
        var fStorage = string.IsNullOrWhiteSpace(item.FStorageCode) ? "KHO-NINHBINH" : item.FStorageCode.Trim().ToUpper();
        var tStorage = string.IsNullOrWhiteSpace(item.TStorageCode) ? "KHO-HANOI" : item.TStorageCode.Trim().ToUpper();

        var dlvStartDate = item.DlvStartDate ?? DateTime.Today.AddDays(-3);
        var expectedDays = item.ExpectedDays.HasValue && item.ExpectedDays.Value > 0 ? item.ExpectedDays.Value : 3;
        var expectedDlvEndDate = dlvStartDate.AddDays(expectedDays);
        var dlvEndDate = item.DlvEndDate ?? DateTime.Today;

        int delayDays = 0;
        if (dlvEndDate > expectedDlvEndDate)
        {
            delayDays = (int)(dlvEndDate.Date - expectedDlvEndDate.Date).TotalDays;
        }

        long priceCar = item.PriceCar.HasValue && item.PriceCar.Value > 0 ? item.PriceCar.Value : 650_000_000;
        decimal insPercent = item.InsurancePercent.HasValue && item.InsurancePercent.Value >= 0 ? item.InsurancePercent.Value : 0.05m;
        long insuranceCost = (long)Math.Round(priceCar * (insPercent / 100m), MidpointRounding.AwayFromZero);

        long tfValReal = item.TFValReal.HasValue && item.TFValReal.Value >= 0
            ? item.TFValReal.Value
            : ResolveDefaultFreightRate(fStorage, tStorage);

        long tpValReal = item.TPValReal.HasValue && item.TPValReal.Value >= 0
            ? item.TPValReal.Value
            : (delayDays > 0 ? delayDays * 200_000 : 0);

        long valTransport = tfValReal + insuranceCost - tpValReal;

        return new TransportInsPaymentDetail
        {
            OrgId = orgId,
            VIN = vin,
            CarId = item.CarId?.Trim(),
            ModelCode = modelCode,
            ModelName = item.ModelName?.Trim(),
            SpecCode = item.SpecCode?.Trim(),
            SpecDescription = item.SpecDescription?.Trim(),
            ColorName = item.ColorName?.Trim(),
            FStorageCode = fStorage,
            FProvinceName = item.FProvinceName?.Trim() ?? ResolveProvinceName(fStorage),
            TStorageCode = tStorage,
            TProvinceName = item.TProvinceName?.Trim() ?? ResolveProvinceName(tStorage),
            TranspReqType = string.IsNullOrWhiteSpace(item.TranspReqType) ? "CARTRANSPORT" : item.TranspReqType.Trim().ToUpper(),
            DlvMnNo = item.DlvMnNo?.Trim(),
            DlvStartDate = dlvStartDate,
            ExpectedDays = expectedDays,
            ExpectedDlvEndDate = expectedDlvEndDate,
            DlvEndDate = dlvEndDate,
            DelayDays = delayDays,
            TFValReal = tfValReal,
            TPValReal = tpValReal,
            PriceCar = priceCar,
            InsurancePercent = insPercent,
            InsuranceCost = insuranceCost,
            Val_Transport = valTransport,
            StandardRemark = item.StandardRemark?.Trim(),
            FProvinceRemark = item.FProvinceRemark?.Trim(),
            Status = TransportInsDetailStatus.Pending,
            Remark = item.Remark?.Trim()
        };
    }

    private static void RecalculateHeader(TransportInsPayment payment)
    {
        payment.TotalVehicles = payment.Details.Count;
        payment.TotalTransportCost = payment.Details.Sum(d => d.TFValReal);
        payment.TotalDelayPenalty = payment.Details.Sum(d => d.TPValReal);
        payment.TotalInsuranceCost = payment.Details.Sum(d => d.InsuranceCost);
        payment.TotalAmount = payment.Details.Sum(d => d.Val_Transport);

        payment.TotalBeforeVAT = (long)Math.Round(payment.TotalAmount / (1.0m + payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.AmountVAT = payment.TotalAmount - payment.TotalBeforeVAT;
    }

    private static string ResolveTransporterName(string code) => code switch
    {
        "TRANS-NEWWAY" => "Công ty Cổ phần Vận tải NewWay",
        "TRANS-DATDUC" => "Công ty TNHH Vận tải Thương mại Đạt Đức",
        "TRANS-MIENBAC" => "Công ty Vận tải & Tiếp vận Miền Bắc",
        "TRANS-SAIGON" => "Công ty Dịch vụ Vận tải Ô tô Sài Gòn",
        _ => $"Đơn vị vận tải {code}"
    };

    private static string ResolveInsuranceCompanyName(string code) => code switch
    {
        "PVI" or "INS-PVI" => "Tổng Công ty Cổ phần Bảo hiểm Dầu khí Việt Nam (PVI)",
        "BAOVIET" or "INS-BAOVIET" => "Tổng Công ty Bảo hiểm Bảo Việt",
        "MIC" or "INS-MIC" => "Tổng Công ty Cổ phần Bảo hiểm Quân đội (MIC)",
        "BIC" or "INS-BIC" => "Tổng Công ty Bảo hiểm BIDV (BIC)",
        _ => $"Công ty Bảo hiểm {code}"
    };

    private static string ResolveProvinceName(string storageCode) => storageCode switch
    {
        "KHO-NINHBINH" => "Ninh Bình",
        "KHO-DONGANH" or "KHO-HANOI" or "KHO-THANHXUAN" or "KHO-HADONG" => "Hà Nội",
        "KHO-SAIGON" or "KHO-BINHDUONG" => "Hồ Chí Minh",
        "KHO-DANANG" => "Đà Nẵng",
        "KHO-CANTHO" => "Cần Thơ",
        "KHO-HAIPHONG" => "Hải Phòng",
        _ => "Việt Nam"
    };

    private static long ResolveDefaultFreightRate(string fStorage, string tStorage)
    {
        if (fStorage.Contains("NINHBINH", StringComparison.OrdinalIgnoreCase) &&
            (tStorage.Contains("HANOI", StringComparison.OrdinalIgnoreCase) || tStorage.Contains("THANHXUAN", StringComparison.OrdinalIgnoreCase) || tStorage.Contains("HADONG", StringComparison.OrdinalIgnoreCase)))
        {
            return 1_850_000;
        }
        if (fStorage.Contains("NINHBINH", StringComparison.OrdinalIgnoreCase) &&
            (tStorage.Contains("SAIGON", StringComparison.OrdinalIgnoreCase) || tStorage.Contains("BINHDUONG", StringComparison.OrdinalIgnoreCase)))
        {
            return 6_500_000;
        }
        if (fStorage.Contains("NINHBINH", StringComparison.OrdinalIgnoreCase) && tStorage.Contains("DANANG", StringComparison.OrdinalIgnoreCase))
        {
            return 3_800_000;
        }
        return 2_500_000;
    }

    /// <summary>
    /// Thuật toán chuyển đổi số tiền thành chữ tiếng Việt chuẩn hóa theo mẫu kế toán và hóa đơn tài chính.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(Math.Abs(number));

        string[] units = ["", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ"];
        string result = "";
        int unitIndex = 0;

        while (number > 0)
        {
            int group = (int)(number % 1000);
            if (group > 0)
            {
                string groupStr = ReadThreeDigits(group, number >= 1000);
                result = groupStr + units[unitIndex] + (string.IsNullOrWhiteSpace(result) ? "" : " " + result);
            }
            number /= 1000;
            unitIndex++;
        }

        result = result.Trim();
        if (string.IsNullOrEmpty(result)) return "Không đồng";

        return char.ToUpper(result[0]) + result[1..] + " đồng chẵn./.";
    }

    private static string ReadThreeDigits(int n, bool hasHigherGroups)
    {
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
        int hundreds = n / 100;
        int tens = (n % 100) / 10;
        int ones = n % 10;

        string res = "";
        if (hundreds > 0 || hasHigherGroups)
        {
            res += digits[hundreds] + " trăm";
        }

        if (tens > 1)
        {
            res += (string.IsNullOrEmpty(res) ? "" : " ") + digits[tens] + " mươi";
            if (ones == 1) res += " mốt";
            else if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else if (tens == 1)
        {
            res += (string.IsNullOrEmpty(res) ? "" : " ") + "mười";
            if (ones == 1) res += " một";
            else if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else // tens == 0
        {
            if (ones > 0)
            {
                if (!string.IsNullOrEmpty(res)) res += " lẻ " + digits[ones];
                else res += digits[ones];
            }
        }

        return res;
    }
}

public sealed record TransportInsItemInputDto(
    string VIN,
    string? CarId,
    string? ModelCode,
    string? ModelName,
    string? SpecCode,
    string? SpecDescription,
    string? ColorName,
    string? FStorageCode,
    string? FProvinceName,
    string? TStorageCode,
    string? TProvinceName,
    string? TranspReqType,
    string? DlvMnNo,
    DateTime? DlvStartDate,
    int? ExpectedDays,
    DateTime? DlvEndDate,
    long? TFValReal,
    long? TPValReal,
    long? PriceCar,
    decimal? InsurancePercent,
    string? StandardRemark,
    string? FProvinceRemark,
    string? Remark
);

public sealed record UpdateTransportDetailItemDto(
    long DetailId,
    long? TFValReal,
    long? TPValReal,
    long? PriceCar,
    decimal? InsurancePercent,
    string? StandardRemark,
    string? FProvinceRemark,
    string? Remark
);

public sealed record TransportInsAdviceDto(
    long Id,
    string TransportInsNo,
    string PmtMonth,
    string TransporterCode,
    string TransporterName,
    string InsuranceCompanyCode,
    string InsuranceCompanyName,
    string? InsuranceContractNo,
    int TotalVehicles,
    long TotalTransportCost,
    long TotalDelayPenalty,
    long TotalInsuranceCost,
    long TotalBeforeVAT,
    decimal VATRate,
    long AmountVAT,
    long TotalAmount,
    string TotalAmountInWords,
    string Status,
    string TCMSSignStatus,
    string? TCMSSignUser,
    DateTime? TCMSSignDTime,
    string HTVSignStatus,
    string? HTVSignUser,
    DateTime? HTVSignDTime,
    string? Appr1By,
    DateTime? Appr1DTime,
    string? Appr2By,
    DateTime? Appr2DTime,
    string? SettledBy,
    DateTime? SettledAt,
    string? BankTxnRef,
    string? Remark,
    DateTime CreatedAt,
    List<TransportInsAdviceLineDto> Lines
);

public sealed record TransportInsAdviceLineDto(
    long Id,
    int LineIndex,
    string VIN,
    string ModelCode,
    string? ModelName,
    string? ColorName,
    string FStorageCode,
    string? FProvinceName,
    string TStorageCode,
    string? TProvinceName,
    string TranspReqType,
    string? DlvMnNo,
    DateTime? DlvStartDate,
    int ExpectedDays,
    DateTime? ExpectedDlvEndDate,
    DateTime? DlvEndDate,
    int DelayDays,
    long TFValReal,
    long TPValReal,
    long PriceCar,
    decimal InsurancePercent,
    long InsuranceCost,
    long Val_Transport,
    string? StandardRemark,
    string? FProvinceRemark,
    string Status,
    string? Remark
);

public sealed record TransportInsSummaryDto(
    int TotalStatements,
    int DraftStatements,
    int TCMSApprovedStatements,
    int HTVApprovedStatements,
    int SignedStatements,
    int SettledStatements,
    int CancelledStatements,
    int TotalVehicles,
    long TotalTransportCost,
    long TotalDelayPenalty,
    long TotalInsuranceCost,
    long TotalSettledAmount,
    long TotalPendingAmount
);
