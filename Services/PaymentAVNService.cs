using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Thanh toán Chi phí Thiết bị AVN trên Xe Ô tô (Vehicle Audio-Visual Navigation Payment Management).
/// Tương ứng khối Pmt_PaymentAVN, Pmt_PaymentAVNDetail, Tbl_Mst_UnitPriceAVN, CR_PAYMENT_AVN và các form FrmQuanLyThanhToanAVN, FrmTaoThanhToanAVN, FrmTaoThanhToanAVN_AddCar trong BizHTC.Payment.
/// </summary>
public sealed class PaymentAVNService(AppDbContext db)
{
    /// <summary>
    /// Bảng giá thiết bị AVN tiêu chuẩn theo dòng xe (Mst_UnitPriceAVN / FrmMst_AVNPrice).
    /// </summary>
    public static readonly Dictionary<string, (string AvnCode, long Price)> DefaultAVNPrices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SANTAFE"] = ("AVN-STF-GEN5-PREM", 12_500_000),
        ["SANTAFE-CAL"] = ("AVN-STF-CAL-12.3IN", 14_000_000),
        ["TUCSON"] = ("AVN-TUC-GEN5-1025", 11_000_000),
        ["TUCSON-TURBO"] = ("AVN-TUC-TURBO-1025", 11_500_000),
        ["CRETA"] = ("AVN-CRT-1025-NAV", 9_500_000),
        ["CRETA-PRE"] = ("AVN-CRT-PREM-1025", 9_800_000),
        ["ACCENT"] = ("AVN-ACC-GEN5-8IN", 8_500_000),
        ["ACCENT-1.5AT"] = ("AVN-ACC-PREM-8IN", 8_900_000),
        ["ELANTRA"] = ("AVN-ELN-1025-WIDESCREEN", 9_800_000),
        ["ELANTRA-NLINE"] = ("AVN-ELN-NLINE-SPORT", 10_500_000),
        ["STARGAZER"] = ("AVN-SGZ-1025-NAV", 8_800_000),
        ["PALISADE"] = ("AVN-PAL-DUAL-12.3IN", 16_500_000),
        ["CUSTIN"] = ("AVN-CST-PORTRAIT-104", 14_000_000),
        ["IONIQ-5"] = ("AVN-IQ5-EV-DUAL-12.3", 18_000_000)
    };

    /// <summary>
    /// Lập bảng kê thanh toán chi phí thiết bị AVN mới (Pmt_PaymentAVN_Save / FrmTaoThanhToanAVN).
    /// </summary>
    public async Task<PaymentAVN> CreatePaymentAsync(
        Guid orgId,
        string? paymentAVNNo,
        string pmtMonth,
        string? supplierCode,
        string? supplierName,
        decimal? vatRate,
        string? remark,
        string? createdBy,
        List<PaymentAVNItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(pmtMonth))
            throw new ArgumentException("Kỳ / tháng thanh toán AVN (PmtMonth, ví dụ: 2025-05) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng kê thanh toán AVN cần ít nhất 1 dòng xe ô tô đã lắp đặt thiết bị.");

        var cleanMonth = pmtMonth.Trim();
        var finalNo = string.IsNullOrWhiteSpace(paymentAVNNo)
            ? $"AVN-{cleanMonth.Replace("-", "")}-{Random.Shared.Next(100, 999)}"
            : paymentAVNNo.Trim().ToUpper();

        var exists = await db.PaymentAVNs.AnyAsync(p => p.OrgId == orgId && p.PaymentAVNNo == finalNo);
        if (exists)
            throw new InvalidOperationException($"Số bảng kê thanh toán AVN '{finalNo}' đã tồn tại trên hệ thống.");

        // Kiểm tra tính duy nhất của từng VIN trong cùng 1 bảng kê
        var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.VIN))
                throw new ArgumentException("Số khung xe (VIN) không được để trống.");

            var cleanVin = it.VIN.Trim().ToUpper();
            if (!vinSet.Add(cleanVin))
                throw new ArgumentException($"Số khung VIN '{cleanVin}' bị trùng lặp trong cùng bảng kê thanh toán AVN.");
        }

        // Kiểm tra xe (VIN) không thuộc bảng kê AVN khác đang có hiệu lực (Status != Cancelled)
        var activeAvnCars = await db.PaymentAVNDetails
            .Where(d => d.OrgId == orgId && vinSet.Contains(d.VIN) && d.Status != PaymentAVNDetailStatus.Cancelled)
            .Select(d => new { d.VIN, d.PaymentAVNNo })
            .ToListAsync();

        if (activeAvnCars.Count > 0)
        {
            var conflict = activeAvnCars.First();
            throw new InvalidOperationException($"Số khung xe VIN '{conflict.VIN}' đã nằm trong bảng kê thanh toán AVN '{conflict.PaymentAVNNo}' đang hoạt động.");
        }

        var finalVatRate = vatRate is >= 0 ? vatRate.Value : 10.0m;
        var details = new List<PaymentAVNDetail>();

        foreach (var it in items)
        {
            var detail = BuildDetailItem(orgId, finalNo, it);
            details.Add(detail);
        }

        long totalAmount = details.Sum(d => d.UnitPriceAVN);
        long amountVat = (long)Math.Round(totalAmount * (finalVatRate / 100m), MidpointRounding.AwayFromZero);
        long totalAmountAfterVat = totalAmount + amountVat;

        var payment = new PaymentAVN
        {
            OrgId = orgId,
            PaymentAVNNo = finalNo,
            PmtMonth = cleanMonth,
            SupplierCode = string.IsNullOrWhiteSpace(supplierCode) ? "MOBIS-VN" : supplierCode.Trim().ToUpper(),
            SupplierName = string.IsNullOrWhiteSpace(supplierName) ? "Công ty TNHH Mobis Auto Parts Việt Nam" : supplierName.Trim(),
            TotalVehicles = details.Count,
            TotalAmount = totalAmount,
            VATRate = finalVatRate,
            AmountVAT = amountVat,
            TotalAmountAfterVAT = totalAmountAfterVat,
            Status = PaymentAVNStatus.Draft,
            TCMSSignStatus = AVNSignCAStatus.Pending,
            HTVSignStatus = AVNSignCAStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "ChuyenVienLinhKienAVN" : createdBy.Trim(),
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PaymentAVNs.Add(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Tìm kiếm danh sách bảng kê thanh toán AVN (Pmt_PaymentAVN_Get / FrmQuanLyThanhToanAVN).
    /// </summary>
    public async Task<List<PaymentAVN>> GetListAsync(
        Guid orgId,
        string? pmtMonth,
        string? status,
        string? supplierCode,
        string? vin)
    {
        var query = db.PaymentAVNs
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pmtMonth))
            query = query.Where(p => p.PmtMonth == pmtMonth.Trim());

        if (!string.IsNullOrWhiteSpace(supplierCode))
            query = query.Where(p => p.SupplierCode == supplierCode.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentAVNStatus>(status, true, out var st))
            query = query.Where(p => p.Status == st);

        if (!string.IsNullOrWhiteSpace(vin))
        {
            var cleanVin = vin.Trim().ToUpper();
            query = query.Where(p => p.Details.Any(d => d.VIN.Contains(cleanVin)));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết 1 bảng kê thanh toán AVN kèm danh sách xe (FrmQuanLyThanhToanAVN.loadGridDetail).
    /// </summary>
    public async Task<PaymentAVN?> GetByIdAsync(long id, Guid orgId)
    {
        return await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// TCMS Duyệt thẩm định cấp 1 sơ bộ (Pmt_PaymentAVN_Approve1 / FrmQuanLyThanhToanAVN.btnTCMSApprove1_Click).
    /// Guard: Trạng thái thanh toán phải là Draft (P), trạng thái ký TCMS và HTV phải là Pending (P).
    /// </summary>
    public async Task<PaymentAVN?> TCMSApproveAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.Draft ||
            payment.TCMSSignStatus != AVNSignCAStatus.Pending ||
            payment.HTVSignStatus != AVNSignCAStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể duyệt cấp 1 TCMS khi trạng thái bảng kê là Draft (Mới tạo) và cả 2 bên chưa ký số CA.");
        }

        payment.Status = PaymentAVNStatus.TCMSApproved;
        payment.Appr1By = string.IsNullOrWhiteSpace(approverName) ? "GiamDocKyThuat_TCMS" : approverName.Trim();
        payment.Appr1DTime = DateTime.Now;

        foreach (var d in payment.Details)
        {
            if (d.Status == PaymentAVNDetailStatus.Pending)
                d.Status = PaymentAVNDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// HTV Duyệt thẩm định cấp 2 (Pmt_PaymentAVN_Approve2 / FrmQuanLyThanhToanAVN.btnHTVApprove2_Click).
    /// Guard: Trạng thái thanh toán phải là TCMSApproved (A1), trạng thái ký TCMS và HTV phải là Pending (P).
    /// </summary>
    public async Task<PaymentAVN?> HTVApproveAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.TCMSApproved ||
            payment.TCMSSignStatus != AVNSignCAStatus.Pending ||
            payment.HTVSignStatus != AVNSignCAStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể duyệt cấp 2 HTV khi trạng thái là TCMSApproved (Đã duyệt A1) và cả 2 bên chưa ký số CA.");
        }

        payment.Status = PaymentAVNStatus.HTVApproved;
        payment.Appr2By = string.IsNullOrWhiteSpace(approverName) ? "TruongPhongKeToan_HTV" : approverName.Trim();
        payment.Appr2DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// TCMS Ký số điện tử CA bảng kê (Pmt_PaymentAVN_TCMSApproveAndSign / FrmQuanLyThanhToanAVN.btnTCMSSign_Click).
    /// Guard: Trạng thái phải là HTVApproved (A2), TCMSSignStatus = Pending, HTVSignStatus = Pending.
    /// </summary>
    public async Task<PaymentAVN?> TCMSSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.HTVApproved ||
            payment.TCMSSignStatus != AVNSignCAStatus.Pending ||
            payment.HTVSignStatus != AVNSignCAStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể ký số TCMS khi trạng thái bảng kê là HTVApproved (A2) và TCMS chưa ký.");
        }

        payment.TCMSSignStatus = AVNSignCAStatus.Signed;
        payment.TCMSSignUser = string.IsNullOrWhiteSpace(signerName) ? "DaiDienKySo_TCMS" : signerName.Trim();
        payment.TCMSSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath))
            payment.FilePath = filePath.Trim();

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// HTV Ký số điện tử CA hoàn tất bảng kê (Pmt_PaymentAVN_HTVApproveAndSign / FrmQuanLyThanhToanAVN.btnHTVSign_Click).
    /// Guard: Trạng thái phải là HTVApproved (A2), TCMSSignStatus = Signed (A), HTVSignStatus = Pending (P).
    /// Sau khi HTV ký số thành công, trạng thái chuyển thành Signed (F - Đã ký).
    /// </summary>
    public async Task<PaymentAVN?> HTVSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.HTVApproved ||
            payment.TCMSSignStatus != AVNSignCAStatus.Signed ||
            payment.HTVSignStatus != AVNSignCAStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể ký số HTV khi trạng thái là HTVApproved (A2), TCMS đã ký và HTV chưa ký.");
        }

        payment.HTVSignStatus = AVNSignCAStatus.Signed;
        payment.HTVSignUser = string.IsNullOrWhiteSpace(signerName) ? "TongGiamDoc_HTV" : signerName.Trim();
        payment.HTVSignDTime = DateTime.Now;
        payment.Status = PaymentAVNStatus.Signed; // Trạng thái hoàn tất ký số CA (F)
        payment.FilePath = string.IsNullOrWhiteSpace(filePath)
            ? $"/documents/avn/{payment.PaymentAVNNo}-Signed.pdf"
            : filePath.Trim();

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Kế toán lập UNC ngân hàng quyết toán chi trả chuyển tiền thanh toán AVN.
    /// Guard: Trạng thái phải là Signed (F).
    /// </summary>
    public async Task<PaymentAVN?> SettlePaymentAsync(long id, Guid orgId, string? bankTxnRef, string? settledBy)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.Signed)
        {
            throw new InvalidOperationException("Chỉ có thể quyết toán chi trả sau khi bảng kê đã được cả 2 bên TCMS & HTV ký số điện tử hoàn tất (Signed).");
        }

        payment.Status = PaymentAVNStatus.Settled;
        payment.BankTxnRef = string.IsNullOrWhiteSpace(bankTxnRef)
            ? $"UNC-VCB-AVN-{payment.PaymentAVNNo}-{Random.Shared.Next(1000, 9999)}"
            : bankTxnRef.Trim();
        payment.SettledBy = string.IsNullOrWhiteSpace(settledBy) ? "KeToanThanhToan_HTV" : settledBy.Trim();
        payment.SettledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Từ chối bảng kê thanh toán AVN (Pmt_PaymentAVN_Cancel / FrmQuanLyThanhToanAVN.btnDeny_Click).
    /// Guard: Chỉ có thể từ chối khi trạng thái thanh toán là Draft (P) và cả 2 bên chưa ký CA.
    /// </summary>
    public async Task<PaymentAVN?> RejectPaymentAsync(long id, Guid orgId, string reason, string? rejecterName)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.Draft ||
            payment.TCMSSignStatus != AVNSignCAStatus.Pending ||
            payment.HTVSignStatus != AVNSignCAStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể từ chối khi trạng thái thanh toán là Draft (P) và cả HTV & TCMS đều chưa ký CA.");
        }

        payment.Status = PaymentAVNStatus.Rejected;
        payment.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Từ chối theo yêu cầu kiểm soát tài chính" : reason.Trim();
        payment.Appr1By = rejecterName?.Trim() ?? "KiemSoatTaiChinh";
        payment.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy bảng kê thanh toán AVN (FrmQuanLyThanhToanAVN.btnDelete_Click).
    /// </summary>
    public async Task<PaymentAVN?> CancelPaymentAsync(long id, Guid orgId, string? reason)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status == PaymentAVNStatus.Settled)
        {
            throw new InvalidOperationException("Bảng kê đã quyết toán chi trả chuyển tiền ngân hàng, không thể hủy.");
        }

        payment.Status = PaymentAVNStatus.Cancelled;
        payment.RejectReason = reason?.Trim() ?? "Hủy bảng kê theo yêu cầu đối chiếu lại thiết bị";
        payment.CancelledAt = DateTime.Now;

        foreach (var d in payment.Details)
        {
            d.Status = PaymentAVNDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Bổ sung xe vào bảng kê nháp (FrmTaoThanhToanAVN_AddCar).
    /// </summary>
    public async Task<PaymentAVN?> AddVehiclesAsync(long id, Guid orgId, List<PaymentAVNItemInputDto> newItems)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.Draft)
        {
            throw new InvalidOperationException("Chỉ có thể bổ sung xe khi bảng kê đang ở trạng thái Draft (Mới tạo).");
        }

        if (newItems == null || newItems.Count == 0)
            throw new ArgumentException("Danh sách xe bổ sung không được để trống.");

        var existingVins = new HashSet<string>(payment.Details.Select(d => d.VIN), StringComparer.OrdinalIgnoreCase);

        foreach (var item in newItems)
        {
            if (string.IsNullOrWhiteSpace(item.VIN))
                continue;

            var cleanVin = item.VIN.Trim().ToUpper();
            if (existingVins.Contains(cleanVin))
                throw new ArgumentException($"Số khung VIN '{cleanVin}' đã tồn tại trong bảng kê.");

            // Kiểm tra xe không nằm trong bảng kê AVN khác đang hoạt động
            var inOther = await db.PaymentAVNDetails.AnyAsync(d =>
                d.OrgId == orgId && d.VIN == cleanVin && d.PaymentAVNId != id && d.Status != PaymentAVNDetailStatus.Cancelled);
            if (inOther)
                throw new InvalidOperationException($"Số khung xe VIN '{cleanVin}' đã nằm trong một bảng kê AVN khác.");

            var detail = BuildDetailItem(orgId, payment.PaymentAVNNo, item);
            payment.Details.Add(detail);
            existingVins.Add(cleanVin);
        }

        RecalculateTotals(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Xóa 1 xe khỏi bảng kê nháp (FrmTaoThanhToanAVN.btnDeleteCar_Click).
    /// </summary>
    public async Task<PaymentAVN?> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        if (payment.Status != PaymentAVNStatus.Draft)
        {
            throw new InvalidOperationException("Chỉ có thể xóa xe khi bảng kê đang ở trạng thái Draft (Mới tạo).");
        }

        var detail = payment.Details.FirstOrDefault(d => d.Id == detailId);
        if (detail == null)
            throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong bảng kê.");

        if (payment.Details.Count <= 1)
            throw new InvalidOperationException("Bảng kê cần ít nhất 1 dòng xe, không thể xóa hết.");

        payment.Details.Remove(detail);
        db.PaymentAVNDetails.Remove(detail);

        RecalculateTotals(payment);
        await db.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Xóa hoàn toàn bản ghi bảng kê nháp (Pmt_PaymentAVN_Save với FlagIsDelete = '1').
    /// </summary>
    public async Task DeleteDraftAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán AVN #{id}.");

        if (!((payment.Status == PaymentAVNStatus.Draft || payment.Status == PaymentAVNStatus.Cancelled)
            && payment.TCMSSignStatus == AVNSignCAStatus.Pending
            && payment.HTVSignStatus == AVNSignCAStatus.Pending))
        {
            throw new InvalidOperationException("Chỉ có thể xóa khi trạng thái thanh toán là Draft hoặc Cancelled và cả 2 bên chưa ký CA.");
        }

        db.PaymentAVNDetails.RemoveRange(payment.Details);
        db.PaymentAVNs.Remove(payment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí thiết bị AVN (CR_PAYMENT_AVN Advice)
    /// chuẩn mẫu in Crystal Reports và thuật toán đọc số thành chữ tiếng Việt tài chính HTC.
    /// </summary>
    public async Task<PaymentAVNAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentAVNs
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        string formattedMonth = payment.PmtMonth;
        if (DateTime.TryParse($"{payment.PmtMonth}-01", out var mDate))
        {
            formattedMonth = $"{mDate.Month:D2} - {mDate.Year}";
        }

        var tcmsSignInfo = payment.TCMSSignStatus == AVNSignCAStatus.Signed
            ? $"Đã ký số CA bởi {payment.TCMSSignUser} lúc {payment.TCMSSignDTime:dd/MM/yyyy HH:mm}"
            : "Chưa ký";

        var htvSignInfo = payment.HTVSignStatus == AVNSignCAStatus.Signed
            ? $"Đã ký số CA bởi {payment.HTVSignUser} lúc {payment.HTVSignDTime:dd/MM/yyyy HH:mm}"
            : "Chưa ký";

        var advice = new PaymentAVNAdviceDto
        {
            PaymentAVNNo = payment.PaymentAVNNo,
            PmtMonth = payment.PmtMonth,
            PmtMonthFormatted = formattedMonth,
            PrintDate = DateTime.Today.ToString("dd/MM/yyyy"),
            SupplierName = payment.SupplierName,
            TotalVehicles = payment.TotalVehicles,
            TotalAmount = payment.TotalAmount,
            VATRate = payment.VATRate,
            AmountVAT = payment.AmountVAT,
            TotalAmountAfterVAT = payment.TotalAmountAfterVAT,
            AmountInWords = NumberToVietnameseWords(payment.TotalAmountAfterVAT),
            StatusText = ResolveStatusText(payment.Status),
            TCMSSignInfo = tcmsSignInfo,
            HTVSignInfo = htvSignInfo,
            BankTxnRef = payment.BankTxnRef,
            Items = payment.Details.Select((d, idx) => new PaymentAVNDetailAdviceDto
            {
                No = idx + 1,
                VIN = d.VIN,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? d.ModelCode,
                SpecCode = d.SpecCode ?? "",
                SpecDescription = d.SpecDescription ?? "",
                EngineNo = d.EngineNo ?? "",
                AVNCode = d.AVNCode,
                SerialNo = d.SerialNo,
                UnitPriceAVN = d.UnitPriceAVN,
                AVNDate = d.AVNDate?.ToString("dd/MM/yyyy") ?? "",
                InStorageDate = d.InStorageDate?.ToString("dd/MM/yyyy") ?? "",
                Status = d.Status.ToString()
            }).ToList()
        };

        return advice;
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu thanh toán AVN cho dashboard.
    /// </summary>
    public async Task<PaymentAVNSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.PaymentAVNs.Where(p => p.OrgId == orgId).ToListAsync();

        return new PaymentAVNSummaryDto
        {
            TotalStatements = list.Count,
            DraftCount = list.Count(p => p.Status == PaymentAVNStatus.Draft),
            ApprovedCount = list.Count(p => p.Status == PaymentAVNStatus.TCMSApproved || p.Status == PaymentAVNStatus.HTVApproved),
            SignedCount = list.Count(p => p.Status == PaymentAVNStatus.Signed),
            SettledCount = list.Count(p => p.Status == PaymentAVNStatus.Settled),
            CancelledCount = list.Count(p => p.Status == PaymentAVNStatus.Cancelled || p.Status == PaymentAVNStatus.Rejected),
            TotalVehiclesInstalled = list.Sum(p => p.TotalVehicles),
            TotalAmountBeforeVAT = list.Sum(p => p.TotalAmount),
            TotalVATAmount = list.Sum(p => p.AmountVAT),
            TotalSettledAmount = list.Where(p => p.Status == PaymentAVNStatus.Settled).Sum(p => p.TotalAmountAfterVAT)
        };
    }

    private static PaymentAVNDetail BuildDetailItem(Guid orgId, string pmtNo, PaymentAVNItemInputDto item)
    {
        var cleanModel = string.IsNullOrWhiteSpace(item.ModelCode) ? "SANTAFE" : item.ModelCode.Trim().ToUpper();

        // Tra cứu đơn giá và mã AVN mặc định nếu chưa truyền vào
        long unitPrice = item.UnitPriceAVN.GetValueOrDefault();
        string avnCode = item.AVNCode;

        if (DefaultAVNPrices.TryGetValue(cleanModel, out var defaultInfo))
        {
            if (unitPrice <= 0) unitPrice = defaultInfo.Price;
            if (string.IsNullOrWhiteSpace(avnCode)) avnCode = defaultInfo.AvnCode;
        }
        else
        {
            if (unitPrice <= 0) unitPrice = 10_000_000;
            if (string.IsNullOrWhiteSpace(avnCode)) avnCode = $"AVN-{cleanModel}-GEN5";
        }

        var serialNo = string.IsNullOrWhiteSpace(item.SerialNo)
            ? $"AVN-SN-{cleanModel[..Math.Min(3, cleanModel.Length)]}-{Random.Shared.Next(100000, 999999)}"
            : item.SerialNo.Trim().ToUpper();

        return new PaymentAVNDetail
        {
            OrgId = orgId,
            PaymentAVNNo = pmtNo,
            VIN = item.VIN.Trim().ToUpper(),
            EngineNo = string.IsNullOrWhiteSpace(item.EngineNo) ? $"ENG-{Random.Shared.Next(100000, 999999)}" : item.EngineNo.Trim().ToUpper(),
            ModelCode = cleanModel,
            ModelName = string.IsNullOrWhiteSpace(item.ModelName) ? ResolveModelName(cleanModel) : item.ModelName.Trim(),
            SpecCode = item.SpecCode?.Trim().ToUpper(),
            SpecDescription = item.SpecDescription?.Trim(),
            ColorName = item.ColorName?.Trim(),
            AVNCode = avnCode.Trim().ToUpper(),
            SerialNo = serialNo,
            UnitPriceAVN = unitPrice,
            AVNDate = item.AVNDate ?? DateTime.Today.AddDays(-Random.Shared.Next(5, 20)),
            InStorageDate = item.InStorageDate ?? DateTime.Today.AddDays(-Random.Shared.Next(1, 5)),
            Status = PaymentAVNDetailStatus.Pending,
            Remark = item.Remark?.Trim()
        };
    }

    private static void RecalculateTotals(PaymentAVN payment)
    {
        payment.TotalVehicles = payment.Details.Count;
        payment.TotalAmount = payment.Details.Sum(d => d.UnitPriceAVN);
        payment.AmountVAT = (long)Math.Round(payment.TotalAmount * (payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.TotalAmountAfterVAT = payment.TotalAmount + payment.AmountVAT;
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

    private static string ResolveStatusText(PaymentAVNStatus status) => status switch
    {
        PaymentAVNStatus.Draft => "Mới tạo (Draft)",
        PaymentAVNStatus.TCMSApproved => "TCMS Đã duyệt cấp 1 (A1)",
        PaymentAVNStatus.HTVApproved => "HTV Đã duyệt cấp 2 (A2)",
        PaymentAVNStatus.Signed => "Đã ký số điện tử CA hoàn tất (Signed)",
        PaymentAVNStatus.Settled => "Đã quyết toán chi trả chuyển khoản UNC (Settled)",
        PaymentAVNStatus.Rejected => "Từ chối duyệt (Rejected)",
        PaymentAVNStatus.Cancelled => "Đã hủy bảng kê (Cancelled)",
        _ => status.ToString()
    };

    /// <summary>
    /// Thuật toán đọc số tiền thành chữ tiếng Việt tài chính ngân hàng chuẩn mực.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(-number);

        string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
        string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

        var groups = new List<int>();
        long temp = number;
        while (temp > 0)
        {
            groups.Add((int)(temp % 1000));
            temp /= 1000;
        }

        var result = new List<string>();
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            int grp = groups[i];
            if (grp == 0) continue;

            int h = grp / 100;
            int t = (grp % 100) / 10;
            int u = grp % 10;

            if (h > 0 || i < groups.Count - 1)
            {
                result.Add(digits[h] + " trăm");
            }

            if (t > 1)
            {
                result.Add(digits[t] + " mươi");
                if (u == 1) result.Add("mốt");
                else if (u == 4) result.Add("tư");
                else if (u == 5) result.Add("lăm");
                else if (u > 0) result.Add(digits[u]);
            }
            else if (t == 1)
            {
                result.Add("mười");
                if (u == 1) result.Add("một");
                else if (u == 4) result.Add("bốn");
                else if (u == 5) result.Add("lăm");
                else if (u > 0) result.Add(digits[u]);
            }
            else if (t == 0 && (h > 0 || i < groups.Count - 1) && u > 0)
            {
                result.Add("lẻ");
                result.Add(digits[u]);
            }
            else if (t == 0 && u > 0)
            {
                result.Add(digits[u]);
            }

            if (i > 0 && !string.IsNullOrEmpty(units[i]))
            {
                result.Add(units[i]);
            }
        }

        result.Add("đồng chẵn./.");
        var text = string.Join(" ", result).Trim();
        return char.ToUpper(text[0]) + text[1..];
    }
}
