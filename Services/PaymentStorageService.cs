using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Thanh toán Chi phí Lưu kho Xe (Vehicle Warehouse Storage Payment Management).
/// Tương ứng khối Pmt_PaymentStorage, Pmt_PaymentStorageDetail và FrmQuanLyThanhToanLuuKho, FrmSuaThanhToanLuuKho trong BizHTC.Payment / 0.34.Contract.
/// </summary>
public sealed class PaymentStorageService(AppDbContext db)
{
    /// <summary>
    /// Lập bảng kê thanh toán chi phí lưu kho xe mới (Job_Pmt_PaymentStorage_Create / FrmQuanLyThanhToanLuuKho).
    /// </summary>
    public async Task<PaymentStorage> CreatePaymentAsync(
        Guid orgId,
        string? paymentStorageNo,
        string pmtMonth,
        string? storageOperatorCode,
        string? storageOperatorName,
        decimal vatRate,
        string? remark,
        string? createdBy,
        List<PaymentStorageItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(pmtMonth))
            throw new ArgumentException("Kỳ / tháng thanh toán lưu kho (PmtMonth, ví dụ 2025-05) không được để trống.");
        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng kê chi phí lưu kho cần ít nhất 1 dòng xe.");

        var finalPmtMonth = pmtMonth.Trim();
        var finalNo = string.IsNullOrWhiteSpace(paymentStorageNo)
            ? $"LK-{finalPmtMonth.Replace("-", "")}-{Random.Shared.Next(100, 999)}"
            : paymentStorageNo.Trim().ToUpper();

        var exists = await db.PaymentStorages.AnyAsync(p => p.OrgId == orgId && p.PaymentStorageNo == finalNo);
        if (exists)
            throw new InvalidOperationException($"Số bảng kê thanh toán lưu kho '{finalNo}' đã tồn tại trên hệ thống.");

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
        var details = new List<PaymentStorageDetail>();

        // Phân tích tháng kỳ thanh toán để xác định ngày đầu tháng và cuối tháng
        DateTime dateFrom;
        if (DateTime.TryParse($"{finalPmtMonth}-01", out var parsedDate))
            dateFrom = parsedDate;
        else
            dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);

        foreach (var item in items)
        {
            var detail = BuildDetailItem(orgId, finalNo, item, dateFrom, dateTo);
            details.Add(detail);
        }

        long totalCoatCost = details.Sum(d => d.CostCoat);
        long totalStorageCost = details.Sum(d => d.CostStorage);
        long totalAmount = totalCoatCost + totalStorageCost; // Tổng tiền trước VAT
        long unitPriceVat = (long)Math.Round(totalAmount * (finalVatRate / 100m), MidpointRounding.AwayFromZero);
        long amountTotal = totalAmount + unitPriceVat;       // Tổng thanh toán sau VAT

        var payment = new PaymentStorage
        {
            OrgId = orgId,
            PaymentStorageNo = finalNo,
            PmtMonth = finalPmtMonth,
            StorageOperatorCode = string.IsNullOrWhiteSpace(storageOperatorCode) ? "KHO-NBD" : storageOperatorCode.Trim().ToUpper(),
            StorageOperatorName = string.IsNullOrWhiteSpace(storageOperatorName) ? "Tổng kho Phân phối Ô tô Hyundai Ninh Bình" : storageOperatorName.Trim(),
            TotalVehicles = details.Count,
            TotalCoatCost = totalCoatCost,
            TotalStorageCost = totalStorageCost,
            TotalAmount = totalAmount,
            VATRate = finalVatRate,
            UnitPriceVAT = unitPriceVat,
            AmountTotal = amountTotal,
            Status = PaymentStorageStatus.Draft,
            TCMSSignStatus = StorageSignCAStatus.Pending,
            HTVSignStatus = StorageSignCAStatus.Pending,
            Remark = remark?.Trim(),
            CreatedBy = createdBy ?? "ChuyenVienQuanLyKho",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PaymentStorages.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Tìm kiếm danh sách bảng kê thanh toán lưu kho (Pmt_PaymentStorage_Get).
    /// </summary>
    public async Task<List<PaymentStorage>> GetListAsync(
        Guid orgId,
        string? pmtMonth,
        string? status,
        string? operatorCode,
        string? vin)
    {
        var query = db.PaymentStorages
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pmtMonth))
            query = query.Where(p => p.PmtMonth == pmtMonth.Trim());

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStorageStatus>(status.Trim(), true, out var parsedStatus))
            query = query.Where(p => p.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(operatorCode))
        {
            var opCode = operatorCode.Trim().ToUpper();
            query = query.Where(p => p.StorageOperatorCode.Contains(opCode) || p.StorageOperatorName.Contains(operatorCode.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(vin))
        {
            var v = vin.Trim().ToUpper();
            query = query.Where(p => p.Details.Any(d => d.VIN.Contains(v)));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Chi tiết 1 bảng kê kèm danh sách xe lưu kho (Pmt_PaymentStorageDetail_Get).
    /// </summary>
    public async Task<PaymentStorage?> GetByIdAsync(long id, Guid orgId)
    {
        return await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// HTV Duyệt sơ bộ cấp 1 (Pmt_PaymentStorage_Approve1).
    /// Chuyển trạng thái từ Draft -> HTVApproved (A1).
    /// </summary>
    public async Task<PaymentStorage> Approve1HTVAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể duyệt cấp 1 (HTV) khi bảng kê ở trạng thái Mới tạo (Draft). Trạng thái hiện tại: {payment.Status}.");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Pending || payment.HTVSignStatus != StorageSignCAStatus.Pending)
            throw new InvalidOperationException("Bảng kê đã phát sinh chữ ký số, không thể duyệt lại cấp 1.");

        var now = DateTime.Now;
        var who = string.IsNullOrWhiteSpace(approverName) ? "LanhDaoPhongKinhDoanh_HTV" : approverName.Trim();

        payment.Status = PaymentStorageStatus.HTVApproved;
        payment.Appr1By = who;
        payment.Appr1DTime = now;

        foreach (var d in payment.Details)
        {
            if (d.Status == PaymentStorageDetailStatus.Pending)
                d.Status = PaymentStorageDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// TCMS Thẩm định duyệt cấp 2 (Pmt_PaymentStorage_Approve2).
    /// Chuyển trạng thái từ HTVApproved (A1) -> TCMSApproved (A2).
    /// </summary>
    public async Task<PaymentStorage> Approve2TCMSAsync(long id, Guid orgId, string? approverName)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.HTVApproved)
            throw new InvalidOperationException($"Chỉ có thể duyệt cấp 2 (TCMS) khi bảng kê đã được HTV duyệt cấp 1 (HTVApproved). Trạng thái hiện tại: {payment.Status}.");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Pending || payment.HTVSignStatus != StorageSignCAStatus.Pending)
            throw new InvalidOperationException("Bảng kê đã phát sinh chữ ký số, không thể duyệt lại cấp 2.");

        var now = DateTime.Now;
        var who = string.IsNullOrWhiteSpace(approverName) ? "GiamDocTaiChinh_TCMS" : approverName.Trim();

        payment.Status = PaymentStorageStatus.TCMSApproved;
        payment.Appr2By = who;
        payment.Appr2DTime = now;

        foreach (var d in payment.Details)
        {
            if (d.Status == PaymentStorageDetailStatus.Pending)
                d.Status = PaymentStorageDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện TCMS (Pmt_PaymentStorage_TCMSApproveAndSign).
    /// </summary>
    public async Task<PaymentStorage> SignTCMSAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentStorages.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.TCMSApproved)
            throw new InvalidOperationException($"Bảng kê cần được TCMS duyệt cấp 2 trước khi ký số. Trạng thái hiện tại: {payment.Status}.");

        if (payment.TCMSSignStatus == StorageSignCAStatus.Signed)
            throw new InvalidOperationException("Bảng kê đã được đại diện TCMS ký số trước đó.");

        payment.TCMSSignStatus = StorageSignCAStatus.Signed;
        payment.TCMSSignUser = string.IsNullOrWhiteSpace(signerName) ? "DaiDienTCMS_Kysodientu" : signerName.Trim();
        payment.TCMSSignDTime = DateTime.Now;
        payment.FilePath = filePath ?? $"/documents/storage/{payment.PaymentStorageNo}-TCMS-Signed.pdf";

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Ký số điện tử CA đại diện HTV (Pmt_PaymentStorage_HTVApproveAndSign).
    /// Khi cả TCMS và HTV đã ký, chuyển trạng thái hoàn tất sang Signed (F).
    /// </summary>
    public async Task<PaymentStorage> SignHTVAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.TCMSApproved)
            throw new InvalidOperationException($"Bảng kê phải ở trạng thái TCMSApproved trước khi ký số HTV. Trạng thái hiện tại: {payment.Status}.");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Signed)
            throw new InvalidOperationException("Đại diện TCMS chưa ký số, HTV chưa thể ký hoàn tất.");

        if (payment.HTVSignStatus == StorageSignCAStatus.Signed)
            throw new InvalidOperationException("Bảng kê đã được đại diện HTV ký số trước đó.");

        var now = DateTime.Now;
        payment.HTVSignStatus = StorageSignCAStatus.Signed;
        payment.HTVSignUser = string.IsNullOrWhiteSpace(signerName) ? "TongGiamDoc_HTV_Kysodientu" : signerName.Trim();
        payment.HTVSignDTime = now;
        payment.Status = PaymentStorageStatus.Signed; // Hoàn tất ký số 2 cấp (F)
        payment.FilePath = filePath ?? $"/documents/storage/{payment.PaymentStorageNo}-Final-Signed.pdf";

        foreach (var d in payment.Details)
        {
            d.Status = PaymentStorageDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Quyết toán chi trả chi phí lưu kho qua UNC ngân hàng (Settled / Paid).
    /// </summary>
    public async Task<PaymentStorage> SettlePaymentAsync(long id, Guid orgId, string? bankTxnRef, string? settlerName)
    {
        var payment = await db.PaymentStorages.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.Signed && payment.Status != PaymentStorageStatus.TCMSApproved)
            throw new InvalidOperationException($"Chỉ có thể quyết toán chi trả khi bảng kê đã ký số (Signed) hoặc đã được duyệt cấp 2. Trạng thái hiện tại: {payment.Status}.");

        var now = DateTime.Now;
        payment.Status = PaymentStorageStatus.Settled;
        payment.SettledBy = string.IsNullOrWhiteSpace(settlerName) ? "KeToanTruongHTV" : settlerName.Trim();
        payment.SettledAt = now;
        payment.BankTxnRef = bankTxnRef ?? $"UNC-CTG-STORAGE-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Từ chối bảng kê kèm lý do (Pmt_PaymentStorage_Cancel / btnDeny_Click).
    /// </summary>
    public async Task<PaymentStorage> RejectAsync(long id, Guid orgId, string reason, string? rejecterName)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Lý do từ chối bảng kê không được để trống.");

        var payment = await db.PaymentStorages.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status == PaymentStorageStatus.Settled)
            throw new InvalidOperationException("Bảng kê đã quyết toán chi trả, không thể từ chối.");

        if (payment.TCMSSignStatus == StorageSignCAStatus.Signed && payment.HTVSignStatus == StorageSignCAStatus.Signed)
            throw new InvalidOperationException("Bảng kê đã ký số CA cả 2 cấp, không thể từ chối.");

        payment.Status = PaymentStorageStatus.Rejected;
        payment.RejectReason = $"{reason.Trim()} (Người từ chối: {rejecterName ?? "CapThamDinh"})";
        payment.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy bảng kê thanh toán lưu kho (Pmt_PaymentStorage_Cancel).
    /// </summary>
    public async Task<PaymentStorage> CancelAsync(long id, Guid orgId, string? reason)
    {
        var payment = await db.PaymentStorages.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Pending || payment.HTVSignStatus != StorageSignCAStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể hủy bảng kê khi trạng thái ký của HTV và TCMS đều là Chưa ký (Pending).");

        payment.Status = PaymentStorageStatus.Cancelled;
        payment.RejectReason = reason?.Trim() ?? "Hủy bảng kê chi phí lưu kho theo yêu cầu nghiệp vụ";
        payment.CancelledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Cập nhật điều chỉnh chi phí bạt che phủ, đơn giá lưu kho và ghi chú xe hàng loạt (Pmt_PaymentStorage_UpdateMulti / FrmSuaThanhToanLuuKho).
    /// </summary>
    public async Task<PaymentStorage> UpdateDetailsAsync(long id, Guid orgId, List<UpdateStorageDetailItemDto> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Danh sách cập nhật chi phí xe không được để trống.");

        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.Draft && payment.Status != PaymentStorageStatus.HTVApproved)
            throw new InvalidOperationException("Chỉ có thể sửa đổi chi phí khi bảng kê ở trạng thái Mới tạo (Draft) hoặc Đã duyệt A1.");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Pending || payment.HTVSignStatus != StorageSignCAStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể sửa khi trạng thái ký của HTV và TCMS là Chưa ký.");

        foreach (var item in items)
        {
            var detail = payment.Details.FirstOrDefault(d => d.Id == item.DetailId || (!string.IsNullOrWhiteSpace(item.VIN) && d.VIN == item.VIN.Trim().ToUpper()));
            if (detail == null) continue;

            if (item.CostCoat.HasValue && item.CostCoat.Value >= 0)
                detail.CostCoat = item.CostCoat.Value;

            if (item.DailyStorageRate.HasValue && item.DailyStorageRate.Value > 0)
            {
                detail.DailyStorageRate = item.DailyStorageRate.Value;
                detail.CostStorage = detail.CostStorageMonth * detail.DailyStorageRate;
            }
            else if (item.CostStorage.HasValue && item.CostStorage.Value >= 0)
            {
                detail.CostStorage = item.CostStorage.Value;
            }

            detail.TotalAmount = detail.CostCoat + detail.CostStorage;
            detail.Status = PaymentStorageDetailStatus.Adjusted;

            if (!string.IsNullOrWhiteSpace(item.Remark))
                detail.Remark = item.Remark.Trim();
        }

        // Tái tính toán tổng số liệu bảng kê
        payment.TotalCoatCost = payment.Details.Sum(d => d.CostCoat);
        payment.TotalStorageCost = payment.Details.Sum(d => d.CostStorage);
        payment.TotalAmount = payment.TotalCoatCost + payment.TotalStorageCost;
        payment.UnitPriceVAT = (long)Math.Round(payment.TotalAmount * (payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.AmountTotal = payment.TotalAmount + payment.UnitPriceVAT;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Bổ sung xe vào bảng kê hiện có kèm tự động tính toán số ngày và chi phí.
    /// </summary>
    public async Task<PaymentStorage> ImportVehiclesAsync(long id, Guid orgId, List<PaymentStorageItemInputDto> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Cần danh sách xe để bổ sung vào bảng kê.");

        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê thanh toán lưu kho #{id}.");

        if (payment.Status != PaymentStorageStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể bổ sung xe vào bảng kê ở trạng thái Mới tạo (Draft).");

        DateTime dateFrom;
        if (DateTime.TryParse($"{payment.PmtMonth}-01", out var parsedDate))
            dateFrom = parsedDate;
        else
            dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);

        var existingVins = new HashSet<string>(payment.Details.Select(d => d.VIN), StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN)) continue;
            var vinUpper = item.VIN.Trim().ToUpper();
            if (existingVins.Contains(vinUpper)) continue; // Tránh trùng xe
            existingVins.Add(vinUpper);

            var detail = BuildDetailItem(orgId, payment.PaymentStorageNo, item, dateFrom, dateTo);
            payment.Details.Add(detail);
        }

        payment.TotalVehicles = payment.Details.Count;
        payment.TotalCoatCost = payment.Details.Sum(d => d.CostCoat);
        payment.TotalStorageCost = payment.Details.Sum(d => d.CostStorage);
        payment.TotalAmount = payment.TotalCoatCost + payment.TotalStorageCost;
        payment.UnitPriceVAT = (long)Math.Round(payment.TotalAmount * (payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.AmountTotal = payment.TotalAmount + payment.UnitPriceVAT;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Xóa 1 xe khỏi bảng kê chi phí lưu kho.
    /// </summary>
    public async Task<PaymentStorage> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê #{id}.");

        if (payment.Status != PaymentStorageStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể xóa xe khi bảng kê ở trạng thái Mới tạo (Draft).");

        var detail = payment.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong bảng kê.");

        payment.Details.Remove(detail);
        db.PaymentStorageDetails.Remove(detail);

        payment.TotalVehicles = payment.Details.Count;
        payment.TotalCoatCost = payment.Details.Sum(d => d.CostCoat);
        payment.TotalStorageCost = payment.Details.Sum(d => d.CostStorage);
        payment.TotalAmount = payment.TotalCoatCost + payment.TotalStorageCost;
        payment.UnitPriceVAT = (long)Math.Round(payment.TotalAmount * (payment.VATRate / 100m), MidpointRounding.AwayFromZero);
        payment.AmountTotal = payment.TotalAmount + payment.UnitPriceVAT;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Xóa hẳn bảng kê thanh toán lưu kho khi ở trạng thái Draft hoặc Cancelled và chưa ký số (Pmt_PaymentStorage_Delete).
    /// </summary>
    public async Task DeleteDraftAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng kê #{id}.");

        if (payment.Status != PaymentStorageStatus.Draft && payment.Status != PaymentStorageStatus.Cancelled)
            throw new InvalidOperationException("Chỉ có thể xóa khi bảng kê ở trạng thái Mới tạo (Draft) hoặc Đã hủy (Cancelled).");

        if (payment.TCMSSignStatus != StorageSignCAStatus.Pending || payment.HTVSignStatus != StorageSignCAStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể xóa khi trạng thái ký của HTV và TCMS đều là Chưa ký (Pending).");

        db.PaymentStorageDetails.RemoveRange(payment.Details);
        db.PaymentStorages.Remove(payment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Sinh mẫu in Bảng kê quyết toán chi phí lưu kho xe (CR_PAYMENT_STORAGE Advice) chuẩn quy định kế toán HTC kèm đọc tiền bằng chữ tiếng Việt.
    /// </summary>
    public async Task<StorageStatementAdviceDto?> GenerateStatementAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.PaymentStorages
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        var amountInWords = NumberToVietnameseText(payment.AmountTotal);

        return new StorageStatementAdviceDto(
            PaymentStorageNo: payment.PaymentStorageNo,
            PmtMonth: payment.PmtMonth,
            DateCreated: payment.CreatedAt.ToString("dd/MM/yyyy"),
            StorageOperatorName: payment.StorageOperatorName,
            TotalVehicles: payment.TotalVehicles,
            TotalCoatCost: payment.TotalCoatCost,
            TotalStorageCost: payment.TotalStorageCost,
            TotalAmountBeforeVAT: payment.TotalAmount,
            VATRate: payment.VATRate,
            UnitPriceVAT: payment.UnitPriceVAT,
            AmountTotalAfterVAT: payment.AmountTotal,
            AmountInWords: amountInWords,
            Status: payment.Status.ToString(),
            TCMSSignStatus: payment.TCMSSignStatus.ToString(),
            TCMSSignUser: payment.TCMSSignUser,
            TCMSSignDate: payment.TCMSSignDTime?.ToString("dd/MM/yyyy HH:mm"),
            HTVSignStatus: payment.HTVSignStatus.ToString(),
            HTVSignUser: payment.HTVSignUser,
            HTVSignDate: payment.HTVSignDTime?.ToString("dd/MM/yyyy HH:mm"),
            BankTxnRef: payment.BankTxnRef,
            FilePath: payment.FilePath,
            Remark: payment.Remark,
            Vehicles: payment.Details.Select((d, idx) => new StorageVehicleLineDto(
                Index: idx + 1,
                VIN: d.VIN,
                ModelName: d.ModelName ?? d.ModelCode,
                SpecDescription: d.SpecDescription ?? d.SpecCode ?? "",
                ColorName: d.ColorExtNameVN ?? "Tiêu chuẩn",
                StorageCode: d.StorageCodeInit,
                StorageDate: d.StorageDate.ToString("dd/MM/yyyy"),
                ApprovedDate2: d.ApprovedDate2?.ToString("dd/MM/yyyy") ?? "-",
                DeliveryOutDate: d.DeliveryOutDate?.ToString("dd/MM/yyyy") ?? "Chưa xuất kho",
                DealerCode: d.DealerCode ?? "-",
                InCostStorageDate: d.InCostStorageDate.ToString("dd/MM/yyyy"),
                OutCostStorageDate: d.OutCostStorageDate.ToString("dd/MM/yyyy"),
                CostStorageMonth: d.CostStorageMonth,
                CostCoat: d.CostCoat,
                CostStorage: d.CostStorage,
                TotalAmount: d.TotalAmount,
                Remark: d.Remark ?? ""
            )).ToList()
        );
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu lưu kho cho Dashboard.
    /// </summary>
    public async Task<StorageSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.PaymentStorages.Where(p => p.OrgId == orgId).ToListAsync();

        return new StorageSummaryDto(
            TotalBatches: list.Count,
            TotalVehicles: list.Sum(p => p.TotalVehicles),
            TotalCoatCost: list.Sum(p => p.TotalCoatCost),
            TotalStorageCost: list.Sum(p => p.TotalStorageCost),
            TotalBeforeVAT: list.Sum(p => p.TotalAmount),
            TotalVAT: list.Sum(p => p.UnitPriceVAT),
            TotalSettledAmount: list.Where(p => p.Status == PaymentStorageStatus.Settled).Sum(p => p.AmountTotal),
            TotalPendingAmount: list.Where(p => p.Status != PaymentStorageStatus.Settled && p.Status != PaymentStorageStatus.Cancelled && p.Status != PaymentStorageStatus.Rejected).Sum(p => p.AmountTotal),
            DraftCount: list.Count(p => p.Status == PaymentStorageStatus.Draft),
            HTVApprovedCount: list.Count(p => p.Status == PaymentStorageStatus.HTVApproved),
            TCMSApprovedCount: list.Count(p => p.Status == PaymentStorageStatus.TCMSApproved),
            SignedCount: list.Count(p => p.Status == PaymentStorageStatus.Signed),
            SettledCount: list.Count(p => p.Status == PaymentStorageStatus.Settled),
            RejectedCount: list.Count(p => p.Status == PaymentStorageStatus.Rejected),
            CancelledCount: list.Count(p => p.Status == PaymentStorageStatus.Cancelled)
        );
    }

    // ==========================================
    // PRIVATE HELPER METHODS & FORMULAS
    // ==========================================

    private static PaymentStorageDetail BuildDetailItem(
        Guid orgId,
        string paymentStorageNo,
        PaymentStorageItemInputDto item,
        DateTime dateFrom,
        DateTime dateTo)
    {
        var storeDate = item.StorageDate;
        // InCostStorageDate: Nếu ngày nhập kho <= ngày đầu tháng thì tính từ đầu tháng, ngược lại tính từ ngày nhập kho
        var inCostDate = storeDate <= dateFrom ? dateFrom : storeDate;

        var levelStorage = item.LevelStorage > 0 ? item.LevelStorage : 15;
        var checkDate = (item.ApprovedDate2 ?? dateTo).AddDays(levelStorage);

        DateTime outCostDate;
        if (!item.ApprovedDate2.HasValue)
        {
            // Chưa duyệt lệnh xuất xe LXX A2 -> tính hết tháng
            outCostDate = dateTo;
        }
        else if (item.DeliveryOutDate.HasValue)
        {
            // Đã duyệt và đã xuất kho
            outCostDate = item.DeliveryOutDate.Value <= checkDate ? item.DeliveryOutDate.Value : checkDate;
        }
        else
        {
            // Đã duyệt nhưng chưa xuất kho
            outCostDate = checkDate < dateTo ? checkDate : dateTo;
        }

        if (outCostDate < inCostDate)
            outCostDate = inCostDate;

        // Số ngày lưu kho tính phí trong tháng
        int storageDaysInMonth = (outCostDate - inCostDate).Days + 1;
        if (storageDaysInMonth < 0) storageDaysInMonth = 0;

        var dailyRate = item.DailyStorageRate > 0 ? item.DailyStorageRate : 25_000;
        var costCoat = item.CostCoat >= 0 ? item.CostCoat : 50_000;
        var costStorage = storageDaysInMonth * dailyRate;
        var totalAmount = costCoat + costStorage;

        return new PaymentStorageDetail
        {
            OrgId = orgId,
            PaymentStorageNo = paymentStorageNo,
            VIN = item.VIN.Trim().ToUpper(),
            CarId = item.CarId?.Trim(),
            ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "HYUNDAI" : item.ModelCode.Trim().ToUpper(),
            ModelName = item.ModelName?.Trim(),
            SpecCode = item.SpecCode?.Trim(),
            SpecDescription = item.SpecDescription?.Trim(),
            ColorExtNameVN = item.ColorExtNameVN?.Trim(),
            StorageCodeInit = string.IsNullOrWhiteSpace(item.StorageCodeInit) ? "KHO-NBD" : item.StorageCodeInit.Trim().ToUpper(),
            StorageDate = storeDate,
            ApprovedDate2 = item.ApprovedDate2,
            DeliveryOutDate = item.DeliveryOutDate,
            DealerCode = item.DealerCode?.Trim().ToUpper(),
            DealerName = item.DealerName?.Trim(),
            InCostStorageDate = inCostDate,
            OutCostStorageDate = outCostDate,
            CostStorageMonth = storageDaysInMonth,
            LevelStorage = levelStorage,
            DailyStorageRate = dailyRate,
            CostCoat = costCoat,
            CostStorage = costStorage,
            TotalAmount = totalAmount,
            Status = PaymentStorageDetailStatus.Pending,
            Remark = item.Remark
        };
    }

    /// <summary>
    /// Thuật toán đọc số tiền thành chữ tiếng Việt tài chính (tương ứng Util.dichso trong BizHTC).
    /// </summary>
    public static string NumberToVietnameseText(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseText(Math.Abs(number));

        string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
        string result = "";
        int unitIndex = 0;

        while (number > 0)
        {
            long group = number % 1000;
            if (group > 0)
            {
                string groupText = ThreeDigitsToText((int)group, number >= 1000);
                result = $"{groupText} {units[unitIndex]} {result}".Trim();
            }
            number /= 1000;
            unitIndex++;
        }

        result = char.ToUpper(result[0]) + result[1..].Trim() + " đồng chẵn.";
        return result.Replace("  ", " ");
    }

    private static string ThreeDigitsToText(int number, bool hasHigherDigits)
    {
        string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        int hundreds = number / 100;
        int tens = (number % 100) / 10;
        int ones = number % 10;

        var sb = new System.Text.StringBuilder();

        if (hundreds > 0 || hasHigherDigits)
        {
            sb.Append($"{digits[hundreds]} trăm ");
        }

        if (tens > 1)
        {
            sb.Append($"{digits[tens]} mươi ");
            if (ones == 1) sb.Append("mốt ");
            else if (ones == 5) sb.Append("lăm ");
            else if (ones > 0) sb.Append($"{digits[ones]} ");
        }
        else if (tens == 1)
        {
            sb.Append("mười ");
            if (ones == 5) sb.Append("lăm ");
            else if (ones > 0) sb.Append($"{digits[ones]} ");
        }
        else if (tens == 0 && ones > 0)
        {
            if (hundreds > 0 || hasHigherDigits) sb.Append("lẻ ");
            sb.Append($"{digits[ones]} ");
        }

        return sb.ToString().Trim();
    }
}

// ==========================================
// DTOs CHO PAYMENT STORAGE
// ==========================================

public record PaymentStorageItemInputDto(
    string VIN,
    string? CarId,
    string ModelCode,
    string? ModelName,
    string? SpecCode,
    string? SpecDescription,
    string? ColorExtNameVN,
    string? StorageCodeInit,
    DateTime StorageDate,
    DateTime? ApprovedDate2,
    DateTime? DeliveryOutDate,
    string? DealerCode,
    string? DealerName,
    int LevelStorage,
    long DailyStorageRate,
    long CostCoat,
    string? Remark
);

public record UpdateStorageDetailItemDto(
    long DetailId,
    string? VIN,
    long? CostCoat,
    long? DailyStorageRate,
    long? CostStorage,
    string? Remark
);

public record StorageStatementAdviceDto(
    string PaymentStorageNo,
    string PmtMonth,
    string DateCreated,
    string StorageOperatorName,
    int TotalVehicles,
    long TotalCoatCost,
    long TotalStorageCost,
    long TotalAmountBeforeVAT,
    decimal VATRate,
    long UnitPriceVAT,
    long AmountTotalAfterVAT,
    string AmountInWords,
    string Status,
    string TCMSSignStatus,
    string? TCMSSignUser,
    string? TCMSSignDate,
    string HTVSignStatus,
    string? HTVSignUser,
    string? HTVSignDate,
    string? BankTxnRef,
    string? FilePath,
    string? Remark,
    List<StorageVehicleLineDto> Vehicles
);

public record StorageVehicleLineDto(
    int Index,
    string VIN,
    string ModelName,
    string SpecDescription,
    string ColorName,
    string StorageCode,
    string StorageDate,
    string ApprovedDate2,
    string DeliveryOutDate,
    string DealerCode,
    string InCostStorageDate,
    string OutCostStorageDate,
    int CostStorageMonth,
    long CostCoat,
    long CostStorage,
    long TotalAmount,
    string Remark
);

public record StorageSummaryDto(
    int TotalBatches,
    int TotalVehicles,
    long TotalCoatCost,
    long TotalStorageCost,
    long TotalBeforeVAT,
    long TotalVAT,
    long TotalSettledAmount,
    long TotalPendingAmount,
    int DraftCount,
    int HTVApprovedCount,
    int TCMSApprovedCount,
    int SignedCount,
    int SettledCount,
    int RejectedCount,
    int CancelledCount
);
