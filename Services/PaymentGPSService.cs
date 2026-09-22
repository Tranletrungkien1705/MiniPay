using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Thanh toán Chi phí Thiết bị & Dịch vụ Định vị Vệ tinh GPS trên Xe Ô tô (Vehicle GPS Tracking Payment Management).
/// Tương ứng khối Pmt_PaymentGPS, Pmt_PaymentGPSDetail, Mst_UnitPriceGPS, CR_Pmt_PaymentGPS và các form
/// FrmQuanLyThanhToanGPS, FrmTaoThanhToanGPS, FrmTaoThanhToanGPS_AddCar trong 2010.HTC (DMS40/0.34.Contract.cs & TERP.HTCClient).
/// </summary>
public sealed class PaymentGPSService(AppDbContext db)
{
    /// <summary>
    /// Bảng đơn giá định mức thuê bao dịch vụ GPS theo nhà cung cấp viễn thông (Mst_UnitPriceGPS).
    /// </summary>
    public static readonly Dictionary<string, (string ProviderName, string ContractNo, long DailyPrice, long MonthlyRate)> DefaultGPSProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VIETTEL"] = ("Tổng Công ty Viễn thông Viettel (Viettel Telecom)", "HD-GPS-VIETTEL-2024", 2_500, 75_000),
        ["VNPT"] = ("Tổng Công ty Dịch vụ Viễn thông VNPT (VNPT VinaPhone)", "HD-GPS-VNPT-TRACK", 2_400, 72_000),
        ["BAGPS"] = ("Công ty TNHH Phát triển Công nghệ Điện tử Bình Anh (BA GPS)", "HD-GPS-BINHANH", 2_300, 69_000),
        ["BKAV"] = ("Tập đoàn Công nghệ BKAV - Smart Vehicle IoT Solutions", "HD-GPS-BKAV-SMART", 2_600, 78_000),
        ["MOBIFONE"] = ("Tổng Công ty Viễn thông MobiFone - MobiGo Tracking", "HD-GPS-MOBIFONE", 2_450, 73_500)
    };

    /// <summary>
    /// Lập bảng kê thanh toán chi phí định vị GPS mới (Pmt_PaymentGPS_Save / FrmTaoThanhToanGPS).
    /// </summary>
    public async Task<PaymentGPS> CreatePaymentAsync(
        Guid orgId,
        string? paymentGPSNo,
        string pmtMonth,
        string? contractNo,
        string? providerCode,
        string? providerName,
        decimal? vatRate,
        string? remark,
        string? createdBy,
        List<PaymentGPSItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(pmtMonth))
            throw new ArgumentException("Kỳ / tháng thanh toán GPS (PmtMonth, ví dụ: 2025-05) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng kê thanh toán GPS cần ít nhất 1 dòng xe ô tô đã kích hoạt thiết bị định vị.");

        var cleanMonth = pmtMonth.Trim();
        var finalNo = string.IsNullOrWhiteSpace(paymentGPSNo)
            ? $"GPS-{cleanMonth.Replace("-", "")}-{Random.Shared.Next(100, 999)}"
            : paymentGPSNo.Trim().ToUpper();

        var exists = await db.PaymentGPSs.AnyAsync(p => p.OrgId == orgId && p.PaymentGPSNo == finalNo);
        if (exists)
            throw new InvalidOperationException($"Số bảng kê thanh toán GPS '{finalNo}' đã tồn tại trên hệ thống.");

        // Kiểm tra tính duy nhất của từng VIN trong cùng 1 bảng kê
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung xe (VIN) không được để trống.");

            var cleanVin = it.VIN.Trim().ToUpper();
            if (!vinSet.Add(cleanVin))
                throw new ArgumentException($"Số khung VIN '{cleanVin}' bị trùng lặp trong cùng bảng kê thanh toán GPS.");
        }

        // Kiểm tra xe (VIN) không thuộc bảng kê GPS khác đang có hiệu lực (Status != Cancelled)
        var activeGpsCars = await db.PaymentGPSDetails
            .Where(d => d.OrgId == orgId && vinSet.Contains(d.VIN) && d.Status != PaymentGPSDetailStatus.Cancelled)
            .Select(d => new { d.VIN, d.PaymentGPSNo })
            .ToListAsync();

        if (activeGpsCars.Count > 0)
        {
            var conflict = activeGpsCars.First();
            throw new InvalidOperationException($"Số khung xe VIN '{conflict.VIN}' đã nằm trong bảng kê thanh toán GPS '{conflict.PaymentGPSNo}' đang hoạt động.");
        }

        var cleanProvider = string.IsNullOrWhiteSpace(providerCode) ? "VIETTEL" : providerCode.Trim().ToUpper();
        string resolvedContract = contractNo?.Trim() ?? "";
        string resolvedProviderName = providerName?.Trim() ?? "";

        if (DefaultGPSProviders.TryGetValue(cleanProvider, out var provInfo))
        {
            if (string.IsNullOrWhiteSpace(resolvedContract)) resolvedContract = provInfo.ContractNo;
            if (string.IsNullOrWhiteSpace(resolvedProviderName)) resolvedProviderName = provInfo.ProviderName;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(resolvedContract)) resolvedContract = $"HD-GPS-{cleanProvider}";
            if (string.IsNullOrWhiteSpace(resolvedProviderName)) resolvedProviderName = $"Nhà cung cấp GPS {cleanProvider}";
        }

        var finalVatRate = vatRate is >= 0 ? vatRate.Value : 10.0m;
        var details = new List<PaymentGPSDetail>();

        foreach (var it in items)
        {
            var detail = BuildDetailItem(orgId, finalNo, it, resolvedContract, cleanProvider, cleanMonth);
            details.Add(detail);
        }

        long amountTotal = details.Sum(d => d.AmountGPS);
        long amountVat = (long)Math.Round(amountTotal * (finalVatRate / 100m), MidpointRounding.AwayFromZero);
        long totalAmountVat = amountTotal + amountVat;

        var payment = new PaymentGPS
        {
            OrgId = orgId,
            PaymentGPSNo = finalNo,
            PmtMonth = cleanMonth,
            ContractNo = resolvedContract,
            ProviderCode = cleanProvider,
            ProviderName = resolvedProviderName,
            TotalVehicles = details.Count,
            TotalPlanDays = details.Sum(d => d.PlanCostGPSDate),
            TotalDeductDays = details.Sum(d => d.DeductDate),
            TotalActualDays = details.Sum(d => d.ActualCostGPSDate),
            AmountTotal = amountTotal,
            VATRate = finalVatRate,
            UnitPriceVAT = amountVat,
            TotalAmountVAT = totalAmountVat,
            Status = PaymentGPSStatus.Draft,
            HTVSignStatus = GPSSignCAStatus.Pending,
            TCMSSignStatus = GPSSignCAStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "ChuyenVienGiamSatGPS" : createdBy.Trim(),
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PaymentGPSs.Add(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Lấy danh sách bảng kê thanh toán GPS có lọc đa tiêu chí (Pmt_PaymentGPS_Get / Pmt_PaymentGPS_GetAll).
    /// </summary>
    public async Task<List<PaymentGPS>> GetListAsync(
        Guid orgId,
        string? pmtMonth = null,
        string? status = null,
        string? providerCode = null,
        string? vin = null)
    {
        var q = db.PaymentGPSs
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(pmtMonth))
        {
            var m = pmtMonth.Trim();
            q = q.Where(p => p.PmtMonth == m);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentGPSStatus>(status.Trim(), true, out var stEnum))
        {
            q = q.Where(p => p.Status == stEnum);
        }

        if (!string.IsNullOrWhiteSpace(providerCode))
        {
            var pCode = providerCode.Trim().ToUpper();
            q = q.Where(p => p.ProviderCode == pCode);
        }

        if (!string.IsNullOrWhiteSpace(vin))
        {
            var cleanVin = vin.Trim().ToUpper();
            q = q.Where(p => p.Details.Any(d => d.VIN.Contains(cleanVin)));
        }

        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết bảng kê thanh toán GPS theo ID.
    /// </summary>
    public async Task<PaymentGPS?> GetByIdAsync(long id, Guid orgId)
    {
        return await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// Thẩm định duyệt cấp 1 HTV (Pmt_PaymentGPS_Approve1 trong BizHTC.Payment).
    /// Chuyển trạng thái Draft -> HTVApproved.
    /// </summary>
    public async Task<PaymentGPS?> HTVApproveAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể duyệt cấp 1 (HTV) khi bảng kê ở trạng thái Mới tạo (Draft). Trạng thái hiện tại: {payment.Status}.");

        payment.Status = PaymentGPSStatus.HTVApproved;
        payment.Appr1By = string.IsNullOrWhiteSpace(approverName) ? "TruongPhongQuanLyXe_HTV" : approverName.Trim();
        payment.Appr1DTime = DateTime.Now;

        foreach (var d in payment.Details)
        {
            if (d.Status == PaymentGPSDetailStatus.Pending)
                d.Status = PaymentGPSDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Thẩm định duyệt cấp 2 TCMS Ban Tài chính / Kế toán (Pmt_PaymentGPS_Approve2 trong BizHTC.Payment).
    /// Chuyển trạng thái HTVApproved -> TCMSApproved.
    /// </summary>
    public async Task<PaymentGPS?> TCMSApproveAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.HTVApproved)
            throw new InvalidOperationException($"Chỉ có thể duyệt cấp 2 (TCMS) khi bảng kê đã được HTV duyệt cấp 1. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = PaymentGPSStatus.TCMSApproved;
        payment.Appr2By = string.IsNullOrWhiteSpace(approverName) ? "GiamDocTaiChinh_TCMS" : approverName.Trim();
        payment.Appr2DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện HTV (Pmt_PaymentGPS_HTVApproveAndSign trong BizHTC.Payment).
    /// Nếu cả 2 bên TCMS và HTV đều đã ký số CA thì tự động chuyển trạng thái Signed.
    /// </summary>
    public async Task<PaymentGPS?> HTVSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.TCMSApproved && payment.Status != PaymentGPSStatus.HTVApproved && payment.Status != PaymentGPSStatus.Draft)
            throw new InvalidOperationException($"Bảng kê ở trạng thái '{payment.Status}' không hợp lệ để ký số điện tử HTV.");

        payment.HTVSignStatus = GPSSignCAStatus.Signed;
        payment.HTVSignUser = string.IsNullOrWhiteSpace(signerName) ? "DaiDienHTV_NguyenVanA" : signerName.Trim();
        payment.HTVSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) payment.FilePath = filePath.Trim();

        if (payment.TCMSSignStatus == GPSSignCAStatus.Signed)
        {
            payment.Status = PaymentGPSStatus.Signed;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện TCMS (Pmt_PaymentGPS_TCMSApproveAndSign trong BizHTC.Payment).
    /// Nếu cả 2 bên TCMS và HTV đều đã ký số CA thì tự động chuyển trạng thái Signed.
    /// </summary>
    public async Task<PaymentGPS?> TCMSSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.TCMSApproved && payment.Status != PaymentGPSStatus.HTVApproved && payment.Status != PaymentGPSStatus.Draft)
            throw new InvalidOperationException($"Bảng kê ở trạng thái '{payment.Status}' không hợp lệ để ký số điện tử TCMS.");

        payment.TCMSSignStatus = GPSSignCAStatus.Signed;
        payment.TCMSSignUser = string.IsNullOrWhiteSpace(signerName) ? "DaiDienTCMS_TranThiB" : signerName.Trim();
        payment.TCMSSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) payment.FilePath = filePath.Trim();

        if (payment.HTVSignStatus == GPSSignCAStatus.Signed)
        {
            payment.Status = PaymentGPSStatus.Signed;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Kế toán quyết toán thanh toán chuyển khoản qua ngân hàng (Settled) bằng Ủy nhiệm chi UNC.
    /// </summary>
    public async Task<PaymentGPS?> SettlePaymentAsync(long id, Guid orgId, string? bankTxnRef, string? settledBy)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.Signed)
            throw new InvalidOperationException($"Chỉ có thể quyết toán chi trả khi bảng kê đã được cả 2 bên ký số CA (Signed). Trạng thái hiện tại: {payment.Status}.");

        payment.Status = PaymentGPSStatus.Settled;
        payment.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "KeToanThanhToan_HTV" : settledBy.Trim();
        payment.SettledAt = DateTime.Now;
        payment.BankTxnRef = string.IsNullOrWhiteSpace(bankTxnRef) ? $"UNC-GPS-{DateTime.Today:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" : bankTxnRef.Trim().ToUpper();

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Từ chối phê duyệt bảng kê thanh toán GPS (Pmt_PaymentGPS_Cancel / btnDeny).
    /// </summary>
    public async Task<PaymentGPS?> RejectPaymentAsync(long id, Guid orgId, string reason, string? rejecterName)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Lý do từ chối bảng kê thanh toán GPS không được để trống.");

        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status == PaymentGPSStatus.Settled || payment.Status == PaymentGPSStatus.Cancelled)
            throw new InvalidOperationException($"Không thể từ chối bảng kê ở trạng thái {payment.Status}.");

        payment.Status = PaymentGPSStatus.Rejected;
        payment.RejectReason = reason.Trim();
        payment.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy bảng kê thanh toán GPS (Pmt_PaymentGPS_Cancel).
    /// </summary>
    public async Task<PaymentGPS?> CancelPaymentAsync(long id, Guid orgId, string? reason)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status == PaymentGPSStatus.Settled)
            throw new InvalidOperationException("Bảng kê đã quyết toán chi trả qua ngân hàng, không thể hủy.");

        payment.Status = PaymentGPSStatus.Cancelled;
        payment.RejectReason = reason?.Trim() ?? "Hủy bởi người lập bảng kê";
        payment.CancelledAt = DateTime.Now;

        foreach (var d in payment.Details)
        {
            d.Status = PaymentGPSDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Bổ sung xe vào bảng kê nháp (FrmTaoThanhToanGPS_AddCar / btnAddCar).
    /// </summary>
    public async Task<PaymentGPS?> AddVehiclesAsync(long id, Guid orgId, List<PaymentGPSItemInputDto> newItems)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể bổ sung danh sách xe khi bảng kê đang ở trạng thái Mới tạo (Draft).");

        if (newItems == null || newItems.Count == 0)
            throw new ArgumentException("Danh sách xe bổ sung rỗng.");

        var currentVins = payment.Details.Select(d => d.VIN).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in newItems)
        {
            if (string.IsNullOrWhiteSpace(item.VIN)) continue;
            var cleanVin = item.VIN.Trim().ToUpper();

            if (currentVins.Contains(cleanVin))
                throw new ArgumentException($"Số khung xe VIN '{cleanVin}' đã tồn tại trong bảng kê này.");

            var inOtherActive = await db.PaymentGPSDetails
                .AnyAsync(d => d.OrgId == orgId && d.VIN == cleanVin && d.PaymentGPSId != id && d.Status != PaymentGPSDetailStatus.Cancelled);
            if (inOtherActive)
                throw new InvalidOperationException($"Số khung xe VIN '{cleanVin}' đã nằm trong bảng kê GPS khác đang hoạt động.");

            var detail = BuildDetailItem(orgId, payment.PaymentGPSNo, item, payment.ContractNo, payment.ProviderCode, payment.PmtMonth);
            payment.Details.Add(detail);
            currentVins.Add(cleanVin);
        }

        RecalculateTotals(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Xóa 1 xe khỏi bảng kê nháp (btnDeleteCar).
    /// </summary>
    public async Task<PaymentGPS?> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể xóa xe khi bảng kê đang ở trạng thái Mới tạo (Draft).");

        var detail = payment.Details.FirstOrDefault(d => d.Id == detailId);
        if (detail == null)
            throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong bảng kê.");

        if (payment.Details.Count <= 1)
            throw new InvalidOperationException("Bảng kê cần duy trì ít nhất 1 dòng xe ô tô. Để xóa toàn bộ bảng kê xin chọn chức năng Xóa bảng kê.");

        payment.Details.Remove(detail);
        db.PaymentGPSDetails.Remove(detail);

        RecalculateTotals(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Cập nhật chi tiết các dòng xe trong bảng kê: ngày bắt đầu, ngày kết thúc, số ngày khấu trừ, đơn giá (FrmTaoThanhToanGPS CellValueChanged & ValidatingEditor).
    /// Tự động tái tính toán PlanCostGPSDate, ActualCostGPSDate, AmountGPS và tổng toàn bảng kê.
    /// </summary>
    public async Task<PaymentGPS?> UpdateDetailsAsync(long id, Guid orgId, List<UpdatePaymentGPSDetailItemDto> updateItems)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (payment.Status != PaymentGPSStatus.Draft && payment.Status != PaymentGPSStatus.HTVApproved)
            throw new InvalidOperationException($"Chỉ có thể điều chỉnh chi tiết xe khi bảng kê ở trạng thái Draft hoặc HTVApproved. Trạng thái hiện tại: {payment.Status}.");

        var detailMap = payment.Details.ToDictionary(d => d.Id);

        foreach (var u in updateItems)
        {
            if (!detailMap.TryGetValue(u.DetailId, out var d)) continue;

            if (u.CostGPSStartDate.HasValue) d.CostGPSStartDate = u.CostGPSStartDate.Value.Date;
            if (u.CostGPSEndDate.HasValue) d.CostGPSEndDate = u.CostGPSEndDate.Value.Date;

            if (d.CostGPSEndDate < d.CostGPSStartDate)
                throw new ArgumentException($"Số khung {d.VIN}: Ngày kết thúc tính phí ({d.CostGPSEndDate:dd/MM/yyyy}) phải lớn hơn hoặc bằng ngày bắt đầu ({d.CostGPSStartDate:dd/MM/yyyy}).");

            if (u.DeductDate.HasValue)
            {
                if (u.DeductDate.Value < 0)
                    throw new ArgumentException($"Số khung {d.VIN}: Số ngày khấu trừ phải >= 0.");
                d.DeductDate = u.DeductDate.Value;
            }

            if (u.PriceGPS.HasValue)
            {
                if (u.PriceGPS.Value < 0)
                    throw new ArgumentException($"Số khung {d.VIN}: Đơn giá dịch vụ GPS phải >= 0.");
                d.PriceGPS = u.PriceGPS.Value;
            }

            if (!string.IsNullOrWhiteSpace(u.Remark))
                d.Remark = u.Remark.Trim();

            // Tính toán lại theo đúng quy chuẩn nguồn
            d.PlanCostGPSDate = (int)(d.CostGPSEndDate.Date - d.CostGPSStartDate.Date).TotalDays + 1;
            d.ActualCostGPSDate = Math.Max(0, d.PlanCostGPSDate - d.DeductDate);
            d.AmountGPS = d.ActualCostGPSDate * d.PriceGPS;
            d.Status = PaymentGPSDetailStatus.Adjusted;
        }

        RecalculateTotals(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Xóa hoàn toàn bản ghi bảng kê nháp (Pmt_PaymentGPS_Save với FlagIsDelete = '1').
    /// </summary>
    public async Task DeleteDraftAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán GPS #{id}.");

        if (!((payment.Status == PaymentGPSStatus.Draft || payment.Status == PaymentGPSStatus.Cancelled)
            && payment.TCMSSignStatus == GPSSignCAStatus.Pending
            && payment.HTVSignStatus == GPSSignCAStatus.Pending))
        {
            throw new InvalidOperationException("Chỉ có thể xóa khi trạng thái thanh toán là Draft hoặc Cancelled và cả 2 bên chưa ký CA.");
        }

        db.PaymentGPSDetails.RemoveRange(payment.Details);
        db.PaymentGPSs.Remove(payment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí giám sát định vị GPS (CR_Pmt_PaymentGPS Advice)
    /// chuẩn mẫu Crystal Reports và thuật toán đọc số thành chữ tiếng Việt tài chính HTC.
    /// </summary>
    public async Task<PaymentGPSAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentGPSs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        string formattedMonth = payment.PmtMonth;
        if (DateTime.TryParse($"{payment.PmtMonth}-01", out var mDate))
        {
            formattedMonth = $"{mDate.Month:D2} - {mDate.Year}";
        }

        var tcmsSignInfo = payment.TCMSSignStatus == GPSSignCAStatus.Signed
            ? $"Đã ký số CA bởi {payment.TCMSSignUser} lúc {payment.TCMSSignDTime:dd/MM/yyyy HH:mm}"
            : "Chưa ký";

        var htvSignInfo = payment.HTVSignStatus == GPSSignCAStatus.Signed
            ? $"Đã ký số CA bởi {payment.HTVSignUser} lúc {payment.HTVSignDTime:dd/MM/yyyy HH:mm}"
            : "Chưa ký";

        var advice = new PaymentGPSAdviceDto
        {
            PaymentGPSNo = payment.PaymentGPSNo,
            PmtMonth = payment.PmtMonth,
            PmtMonthFormatted = formattedMonth,
            PrintDate = DateTime.Today.ToString("dd/MM/yyyy"),
            ContractNo = payment.ContractNo,
            ProviderName = payment.ProviderName,
            TotalVehicles = payment.TotalVehicles,
            TotalPlanDays = payment.TotalPlanDays,
            TotalDeductDays = payment.TotalDeductDays,
            TotalActualDays = payment.TotalActualDays,
            AmountTotal = payment.AmountTotal,
            VATRate = payment.VATRate,
            UnitPriceVAT = payment.UnitPriceVAT,
            TotalAmountVAT = payment.TotalAmountVAT,
            AmountInWords = NumberToVietnameseWords(payment.TotalAmountVAT),
            StatusText = ResolveStatusText(payment.Status),
            TCMSSignInfo = tcmsSignInfo,
            HTVSignInfo = htvSignInfo,
            BankTxnRef = payment.BankTxnRef,
            Items = payment.Details.Select((d, idx) => new PaymentGPSDetailAdviceDto
            {
                No = idx + 1,
                VIN = d.VIN,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? d.ModelCode,
                SpecCode = d.SpecCode ?? "",
                SpecDescription = d.SpecDescription ?? "",
                EngineNo = d.EngineNo ?? "",
                GPSID = d.GPSID,
                ContractGPS = d.ContractGPS,
                GPSStartDate = d.GPSStartDate?.ToString("dd/MM/yyyy"),
                RetailDate = d.RetailDate?.ToString("dd/MM/yyyy"),
                CostGPSStartDate = d.CostGPSStartDate.ToString("dd/MM/yyyy"),
                CostGPSEndDate = d.CostGPSEndDate.ToString("dd/MM/yyyy"),
                PlanCostGPSDate = d.PlanCostGPSDate,
                DeductDate = d.DeductDate,
                ActualCostGPSDate = d.ActualCostGPSDate,
                PriceGPS = d.PriceGPS,
                AmountGPS = d.AmountGPS,
                Status = d.Status.ToString()
            }).ToList()
        };

        return advice;
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu thanh toán GPS cho dashboard.
    /// </summary>
    public async Task<PaymentGPSSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.PaymentGPSs.Where(p => p.OrgId == orgId).ToListAsync();

        return new PaymentGPSSummaryDto
        {
            TotalStatements = list.Count,
            DraftCount = list.Count(p => p.Status == PaymentGPSStatus.Draft),
            ApprovedCount = list.Count(p => p.Status == PaymentGPSStatus.HTVApproved || p.Status == PaymentGPSStatus.TCMSApproved),
            SignedCount = list.Count(p => p.Status == PaymentGPSStatus.Signed),
            SettledCount = list.Count(p => p.Status == PaymentGPSStatus.Settled),
            CancelledCount = list.Count(p => p.Status == PaymentGPSStatus.Cancelled || p.Status == PaymentGPSStatus.Rejected),
            TotalVehiclesTracked = list.Sum(p => p.TotalVehicles),
            TotalTrackedDays = list.Sum(p => p.TotalActualDays),
            TotalAmountBeforeVAT = list.Sum(p => p.AmountTotal),
            TotalVATAmount = list.Sum(p => p.UnitPriceVAT),
            TotalSettledAmount = list.Where(p => p.Status == PaymentGPSStatus.Settled).Sum(p => p.TotalAmountVAT)
        };
    }

    /// <summary>
    /// Danh sách đơn giá GPS định mức cấu hình sẵn trong hệ thống (Mst_UnitPriceGPS).
    /// </summary>
    public async Task<List<UnitPriceGPS>> GetUnitPricesAsync(Guid orgId)
    {
        return await db.UnitPriceGPSs.Where(u => u.OrgId == orgId && u.IsActive).ToListAsync();
    }

    private static PaymentGPSDetail BuildDetailItem(
        Guid orgId,
        string pmtNo,
        PaymentGPSItemInputDto item,
        string contractNo,
        string providerCode,
        string pmtMonth)
    {
        var cleanModel = string.IsNullOrWhiteSpace(item.ModelCode) ? "SANTAFE" : item.ModelCode.Trim().ToUpper();

        // Xác định khoảng thời gian tính phí trong kỳ
        DateTime startDate;
        DateTime endDate;

        if (item.CostGPSStartDate.HasValue && item.CostGPSEndDate.HasValue)
        {
            startDate = item.CostGPSStartDate.Value.Date;
            endDate = item.CostGPSEndDate.Value.Date;
        }
        else if (DateTime.TryParse($"{pmtMonth}-01", out var mStart))
        {
            startDate = mStart;
            endDate = mStart.AddMonths(1).AddDays(-1);
        }
        else
        {
            startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate = startDate.AddMonths(1).AddDays(-1);
        }

        if (endDate < startDate)
            throw new ArgumentException($"Số khung {item.VIN}: Ngày kết thúc tính phí ({endDate:dd/MM/yyyy}) phải >= ngày bắt đầu ({startDate:dd/MM/yyyy}).");

        int deduct = Math.Max(0, item.DeductDate.GetValueOrDefault(0));
        int planDays = (int)(endDate - startDate).TotalDays + 1;
        int actualDays = Math.Max(0, planDays - deduct);

        long price = item.PriceGPS.GetValueOrDefault();
        if (price <= 0)
        {
            if (DefaultGPSProviders.TryGetValue(providerCode, out var pInfo))
                price = pInfo.DailyPrice;
            else
                price = 2_500;
        }

        long amount = actualDays * price;

        var gpsId = string.IsNullOrWhiteSpace(item.GPSID)
            ? $"GPS-{providerCode[..Math.Min(3, providerCode.Length)]}-{Random.Shared.Next(100000, 999999)}"
            : item.GPSID.Trim().ToUpper();

        return new PaymentGPSDetail
        {
            OrgId = orgId,
            PaymentGPSNo = pmtNo,
            VIN = item.VIN.Trim().ToUpper(),
            EngineNo = string.IsNullOrWhiteSpace(item.EngineNo) ? $"ENG-{Random.Shared.Next(100000, 999999)}" : item.EngineNo.Trim().ToUpper(),
            CarID = item.CarID?.Trim(),
            ModelCode = cleanModel,
            ModelName = string.IsNullOrWhiteSpace(item.ModelName) ? ResolveModelName(cleanModel) : item.ModelName.Trim(),
            SpecCode = item.SpecCode?.Trim().ToUpper(),
            SpecDescription = item.SpecDescription?.Trim(),
            GPSID = gpsId,
            ContractGPS = string.IsNullOrWhiteSpace(item.ContractGPS) ? contractNo : item.ContractGPS.Trim(),
            GPSStartDate = item.GPSStartDate ?? startDate.AddMonths(-1),
            RetailDate = item.RetailDate,
            CostGPSStartDate = startDate,
            CostGPSEndDate = endDate,
            PlanCostGPSDate = planDays,
            DeductDate = deduct,
            ActualCostGPSDate = actualDays,
            PriceGPS = price,
            AmountGPS = amount,
            Status = PaymentGPSDetailStatus.Pending,
            Remark = item.Remark?.Trim()
        };
    }

    private static void RecalculateTotals(PaymentGPS payment)
    {
        payment.TotalVehicles = payment.Details.Count;
        payment.TotalPlanDays = payment.Details.Sum(d => d.PlanCostGPSDate);
        payment.TotalDeductDays = payment.Details.Sum(d => d.DeductDate);
        payment.TotalActualDays = payment.Details.Sum(d => d.ActualCostGPSDate);
        payment.AmountTotal = payment.Details.Sum(d => d.AmountGPS);
        payment.UnitPriceVAT = (long)Math.Round(payment.AmountTotal * (payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.TotalAmountVAT = payment.AmountTotal + payment.UnitPriceVAT;
    }

    private static string ResolveModelName(string modelCode) => modelCode switch
    {
        "SANTAFE" or "SANTAFE-CAL" => "Hyundai Santa Fe All New",
        "TUCSON" or "TUCSON-TURBO" => "Hyundai Tucson Facelift",
        "CRETA" or "CRETA-PRE" => "Hyundai Creta Smart Stream",
        "ACCENT" or "ACCENT-1.5AT" => "Hyundai Accent Sedan",
        "ELANTRA" or "ELANTRA-NLINE" => "Hyundai Elantra Sport",
        "STARGAZER" => "Hyundai Stargazer MPV",
        "PALISADE" => "Hyundai Palisade Flagship SUV",
        "CUSTIN" => "Hyundai Custin Premium MPV",
        "IONIQ-5" => "Hyundai IONIQ 5 Electric Vehicle",
        _ => $"Hyundai {modelCode}"
    };

    private static string ResolveStatusText(PaymentGPSStatus status) => status switch
    {
        PaymentGPSStatus.Draft => "Mới tạo (Draft)",
        PaymentGPSStatus.HTVApproved => "HTV Thẩm định duyệt cấp 1 (A1)",
        PaymentGPSStatus.TCMSApproved => "TCMS Phê duyệt cấp 2 (A2)",
        PaymentGPSStatus.Signed => "Đã ký số điện tử CA hoàn tất (Signed)",
        PaymentGPSStatus.Settled => "Đã quyết toán chuyển khoản UNC (Settled)",
        PaymentGPSStatus.Rejected => "Từ chối duyệt (Rejected)",
        PaymentGPSStatus.Cancelled => "Đã hủy bảng kê (Cancelled)",
        _ => status.ToString()
    };

    /// <summary>
    /// Thuật toán đọc số tiền thành chữ tiếng Việt tài chính ngân hàng chuẩn mực HTC.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(-number);

        string[] units = ["", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ"];
        string result = "";
        int unitIndex = 0;

        while (number > 0)
        {
            long chunk = number % 1000;
            if (chunk > 0)
            {
                string chunkStr = ReadThreeDigits((int)chunk, number >= 1000);
                result = chunkStr + " " + units[unitIndex] + " " + result;
            }
            number /= 1000;
            unitIndex++;
        }

        result = result.Trim();
        if (string.IsNullOrWhiteSpace(result)) return "Không đồng";
        return char.ToUpper(result[0]) + result[1..] + " đồng chẵn./.";
    }

    private static string ReadThreeDigits(int n, bool hasHigherChunk)
    {
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
        int hundreds = n / 100;
        int tens = (n % 100) / 10;
        int ones = n % 10;

        string result = "";

        if (hundreds > 0 || hasHigherChunk)
        {
            result += digits[hundreds] + " trăm";
        }

        if (tens > 1)
        {
            result += (string.IsNullOrEmpty(result) ? "" : " ") + digits[tens] + " mươi";
            if (ones == 1) result += " mốt";
            else if (ones == 5) result += " lăm";
            else if (ones > 0) result += " " + digits[ones];
        }
        else if (tens == 1)
        {
            result += (string.IsNullOrEmpty(result) ? "" : " ") + "mười";
            if (ones == 5) result += " lăm";
            else if (ones > 0) result += " " + digits[ones];
        }
        else if (tens == 0 && ones > 0)
        {
            if (!string.IsNullOrEmpty(result)) result += " lẻ " + digits[ones];
            else result += digits[ones];
        }

        return result;
    }
}
