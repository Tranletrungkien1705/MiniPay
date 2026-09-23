using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hóa đơn GTGT Nhà máy sản xuất TCG (TCG Factory VAT Invoice).
/// Tương ứng VAT_TCGInvoice, VAT_TCGInvoiceDetail trong TERP.BizHTC/BizHTC.InvoiceHTC_TCG.cs
/// (VAT_TCGInvoiceGet / VAT_TCGInvoiceCreate / VAT_TCGInvoiceApprove / VAT_TCGInvoiceUpdate /
///  VAT_TCGInvoiceDelete / VAT_TCGInvoiceDetailDelete / VAT_TCGInvoice_GenTCGInvoiceNo)
/// và các màn hình FrmMngTCGInvoice, FrmNewTCGInvoice, FrmImportNewTCGInvoice, FrmThuHoiHDTCG
/// trong TERP.HTCClient/Views/Sales/PrintVAT hệ nguồn HTC 2010.
///
/// Nghiệp vụ: HTC nhận hóa đơn GTGT đầu vào do Nhà máy sản xuất (TCG) xuất cho từng xe (VIN).
/// Hóa đơn TCG là nguồn chứng từ để HTC xuất hóa đơn GTGT bán buôn cho đại lý (VAT_HTCInvoice).
/// Vòng đời: Pending ('P') → Finished ('F') khi duyệt / Cancelled ('C') khi hủy.
/// Số hóa đơn (TCGInvoiceNo) và ngày hóa đơn (TCGInvoiceDate) để NULL lúc tạo, chỉ điền khi cập nhật.
/// </summary>
public sealed class TCGInvoiceService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách hóa đơn TCG kèm bộ lọc (mã, số hóa đơn, trạng thái, VIN, nhãn hiệu, khoảng ngày, từ khóa).
    /// Tương ứng VAT_TCGInvoiceGet.
    /// </summary>
    public async Task<List<TCGInvoice>> GetListAsync(
        Guid orgId,
        string? code = null,
        string? invoiceNo = null,
        string? status = null,
        string? vin = null,
        string? brandName = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.TCGInvoices
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(code))
        {
            string s = code.Trim().ToUpperInvariant();
            q = q.Where(x => x.TCGInvoiceCode.ToUpper().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(invoiceNo))
        {
            string s = invoiceNo.Trim();
            q = q.Where(x => x.TCGInvoiceNo != null && x.TCGInvoiceNo.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TCGInvoiceStatus>(status, true, out var st))
            q = q.Where(x => x.VatTCGStatus == st);

        if (!string.IsNullOrWhiteSpace(vin))
        {
            string v = vin.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.VIN.ToUpper().Contains(v)));
        }

        if (!string.IsNullOrWhiteSpace(brandName))
        {
            string b = brandName.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.BrandName != null && d.BrandName.ToUpper().Contains(b)));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.TCGInvoiceCode.ToUpper().Contains(s)
                || (x.TCGInvoiceNo != null && x.TCGInvoiceNo.ToUpper().Contains(s))
                || (x.Remark != null && x.Remark.ToUpper().Contains(s))
                || x.Details.Any(d => d.VIN.ToUpper().Contains(s)));
        }

        if (fromDate.HasValue)
        {
            var fd = fromDate.Value.Date;
            q = q.Where(x => x.CreatedAt >= fd);
        }

        if (toDate.HasValue)
        {
            var td = toDate.Value.Date.AddDays(1);
            q = q.Where(x => x.CreatedAt < td);
        }

        return await q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.TCGInvoiceCode).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 hóa đơn TCG theo ID (kèm danh sách dòng xe).</summary>
    public async Task<TCGInvoice?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết theo mã hóa đơn TCG (TCGInvoiceCode).</summary>
    public async Task<TCGInvoice?> GetByCodeAsync(Guid orgId, string code)
    {
        string c = code.Trim();
        return await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.TCGInvoiceCode == c);
    }

    /// <summary>
    /// Lập hóa đơn TCG mới (tương ứng VAT_TCGInvoiceCreate).
    /// Tự sinh mã nếu bỏ trống, kiểm tra trùng mã, bắt buộc có ít nhất 1 dòng xe.
    /// Kiểm tra VIN không trùng trong hóa đơn TCG đang hoạt động (P/F/A).
    /// Xe CKD phải có số hóa đơn nhà máy (InvoiceNoFactory); xe CBU phải có ngày thông quan (CustomsClearanceDate).
    /// Số hóa đơn (TCGInvoiceNo) và ngày hóa đơn (TCGInvoiceDate) để NULL lúc tạo.
    /// </summary>
    public async Task<TCGInvoice> CreateAsync(Guid orgId, CreateTCGInvoiceDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Hóa đơn TCG phải có ít nhất 1 dòng xe (VAT_TCGInvoiceDetail).");

        string code = string.IsNullOrWhiteSpace(dto.TCGInvoiceCode)
            ? $"TCG-{DateTime.Now:yyyyMM}-{Random.Shared.Next(100, 999)}"
            : dto.TCGInvoiceCode.Trim();

        bool exists = await db.TCGInvoices.AnyAsync(x => x.OrgId == orgId && x.TCGInvoiceCode == code);
        if (exists)
            throw new InvalidOperationException($"Mã hóa đơn TCG {code} đã tồn tại trong hệ thống.");

        // Kiểm tra trùng VIN trong hóa đơn TCG đang hoạt động (P/F/A) — tương ứng VAT_TCGInvoiceCreate_ExistTCGInvoice.
        var vins = dto.Items.Select(i => i.VIN.Trim().ToUpperInvariant()).Where(v => v.Length > 0).ToList();
        if (vins.Count != dto.Items.Count)
            throw new ArgumentException("Mỗi dòng xe phải có số khung VIN.");
        if (vins.Distinct().Count() != vins.Count)
            throw new ArgumentException("Danh sách xe có VIN trùng lặp trong cùng hóa đơn.");

        var dupVin = await db.TCGInvoiceDetails
            .Where(d => d.OrgId == orgId && vins.Contains(d.VIN)
                && d.TCGStatusDetail != TCGInvoiceDetailStatus.Cancelled)
            .Select(d => d.VIN)
            .FirstOrDefaultAsync();
        if (dupVin != null)
            throw new InvalidOperationException($"Xe VIN {dupVin} đã có hóa đơn TCG đang hoạt động, không thể lập hóa đơn mới.");

        var now = DateTime.Now;
        var entity = new TCGInvoice
        {
            OrgId = orgId,
            TCGInvoiceCode = code,
            SourceInvoiceCode = TCGInvoiceSource.Invoice,
            InvoiceIDType = string.IsNullOrWhiteSpace(dto.InvoiceIDType) ? "TCG" : dto.InvoiceIDType.Trim(),
            InvoiceIDCode = dto.InvoiceIDCode?.Trim(),
            VatTCGStatus = TCGInvoiceStatus.Pending,
            TCGInvoiceNo = null,
            TCGInvoiceDate = null,
            VAT = dto.VAT?.Trim(),
            FlagisHTC = dto.FlagisHTC?.Trim(),
            Remark = dto.Remark?.Trim(),
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Admin_HTC",
            CreatedAt = now
        };

        foreach (var item in dto.Items)
        {
            string vin = item.VIN.Trim().ToUpperInvariant();
            decimal vat = item.TCGVAT ?? 0m;
            // Đơn giá trước thuế = Đơn giá gồm thuế / (1 + VAT%/100) — tương ứng DONGIA trong VAT_TCGInvoiceGet.
            long tInvoicePrice = vat > 0
                ? (long)Math.Round(item.TCGUnitPrice / (1 + vat / 100m), 0, MidpointRounding.AwayFromZero)
                : item.TCGUnitPrice;

            entity.Details.Add(new TCGInvoiceDetail
            {
                OrgId = orgId,
                TCGInvoiceCode = code,
                VIN = vin,
                TCGUnitPrice = item.TCGUnitPrice,
                TCGVAT = vat,
                TInvoicePrice = tInvoicePrice,
                BrandName = item.BrandName?.Trim(),
                CarType = item.CarType?.Trim(),
                CustomsClearanceDate = item.CustomsClearanceDate?.Date,
                InvoiceNoFactory = item.InvoiceNoFactory?.Trim(),
                ProductionMonth = item.ProductionMonth?.Trim(),
                TCGStatusDetail = TCGInvoiceDetailStatus.Pending,
                Remark = item.Remark?.Trim()
            });
        }

        db.TCGInvoices.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Duyệt / Hủy hóa đơn TCG (tương ứng VAT_TCGInvoiceApprove).
    /// Duyệt: chỉ hóa đơn Pending ('P') → Finished ('F').
    /// Hủy: chỉ hóa đơn Finished ('F') → Cancelled ('C'); đồng thời hủy toàn bộ dòng xe.
    /// </summary>
    public async Task<TCGInvoice> ApproveAsync(Guid orgId, long id, ApproveTCGInvoiceDto dto)
    {
        var entity = await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn TCG #{id}.");

        var now = DateTime.Now;
        string who = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "Admin_HTC";

        if (dto.Approve)
        {
            if (entity.VatTCGStatus != TCGInvoiceStatus.Pending)
                throw new InvalidOperationException(
                    $"Hóa đơn {entity.TCGInvoiceCode} không ở trạng thái chờ duyệt (hiện tại: {entity.VatTCGStatus}).");

            entity.VatTCGStatus = TCGInvoiceStatus.Finished;
            entity.ApprovedBy = who;
            entity.ApprovedAt = now;
            foreach (var d in entity.Details)
                d.TCGStatusDetail = TCGInvoiceDetailStatus.Finished;
        }
        else
        {
            if (entity.VatTCGStatus != TCGInvoiceStatus.Finished)
                throw new InvalidOperationException(
                    $"Hóa đơn {entity.TCGInvoiceCode} không ở trạng thái đã duyệt để hủy (hiện tại: {entity.VatTCGStatus}).");

            entity.VatTCGStatus = TCGInvoiceStatus.Cancelled;
            entity.CancelledBy = who;
            entity.CancelledAt = now;
            if (!string.IsNullOrWhiteSpace(dto.Reason)) entity.Remark = dto.Reason.Trim();
            foreach (var d in entity.Details)
                d.TCGStatusDetail = TCGInvoiceDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Cập nhật số hóa đơn & ngày hóa đơn TCG (tương ứng VAT_TCGInvoiceUpdate).
    /// Số hóa đơn phải đúng 7 ký tự số và không trùng trong cùng mẫu hóa đơn (InvoiceIDCode + InvoiceIDType='TCG').
    /// </summary>
    public async Task<TCGInvoice> UpdateAsync(Guid orgId, long id, UpdateTCGInvoiceDto dto)
    {
        var entity = await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn TCG #{id}.");

        string no = (dto.TCGInvoiceNo ?? "").Trim();
        if (no.Length != 7 || !long.TryParse(no, out _))
            throw new ArgumentException("Số hóa đơn TCG phải gồm đúng 7 ký tự số (ví dụ: 0000123).");

        bool dup = await db.TCGInvoices.AnyAsync(x => x.OrgId == orgId
            && x.Id != id
            && x.InvoiceIDType == "TCG"
            && x.InvoiceIDCode == entity.InvoiceIDCode
            && x.TCGInvoiceNo == no);
        if (dup)
            throw new InvalidOperationException($"Số hóa đơn TCG {no} đã tồn tại trong cùng mẫu hóa đơn.");

        entity.TCGInvoiceNo = no;
        entity.TCGInvoiceDate = dto.TCGInvoiceDate?.Date ?? DateTime.Today;
        if (!string.IsNullOrWhiteSpace(dto.OS_HDDT_InvoiceCode)) entity.OS_HDDT_InvoiceCode = dto.OS_HDDT_InvoiceCode.Trim();
        if (!string.IsNullOrWhiteSpace(dto.OS_HDDT_RefNo)) entity.OS_HDDT_RefNo = dto.OS_HDDT_RefNo.Trim();

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Xóa hóa đơn TCG (tương ứng VAT_TCGInvoiceDelete). Chỉ xóa được khi hóa đơn ở trạng thái Pending ('P').
    /// </summary>
    public async Task<bool> DeleteAsync(Guid orgId, long id)
    {
        var entity = await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
        if (entity == null) return false;

        if (entity.VatTCGStatus != TCGInvoiceStatus.Pending)
            throw new InvalidOperationException(
                $"Hóa đơn {entity.TCGInvoiceCode} đang ở trạng thái {entity.VatTCGStatus}, chỉ xóa được khi Pending.");

        db.TCGInvoices.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Xóa 1 dòng xe khỏi hóa đơn TCG (tương ứng VAT_TCGInvoiceDetailDelete). Chỉ khi hóa đơn Pending ('P').
    /// </summary>
    public async Task<TCGInvoice> DeleteDetailAsync(Guid orgId, long id, string vin)
    {
        var entity = await db.TCGInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn TCG #{id}.");

        if (entity.VatTCGStatus != TCGInvoiceStatus.Pending)
            throw new InvalidOperationException(
                $"Hóa đơn {entity.TCGInvoiceCode} đang ở trạng thái {entity.VatTCGStatus}, chỉ sửa được khi Pending.");

        string v = (vin ?? "").Trim().ToUpperInvariant();
        var row = entity.Details.FirstOrDefault(d => d.VIN == v)
            ?? throw new KeyNotFoundException($"Hóa đơn {entity.TCGInvoiceCode} không có VIN {v}.");

        db.TCGInvoiceDetails.Remove(row);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Sinh số hóa đơn TCG kế tiếp (tương ứng VAT_TCGInvoice_GenTCGInvoiceNo).
    /// Lấy max(TCGInvoiceNo) trong cùng mẫu hóa đơn (InvoiceIDCode + InvoiceIDType='TCG') rồi +1, đệm 7 ký tự.
    /// Lưu ý: nguồn KHÔNG khóa và KHÔNG ghi — số trả về chỉ mang tính gợi ý, chống trùng do bước lưu hóa đơn đảm nhiệm.
    /// </summary>
    public async Task<string> GenerateInvoiceNoAsync(Guid orgId, string? invoiceIdCode)
    {
        string? idCode = string.IsNullOrWhiteSpace(invoiceIdCode) ? null : invoiceIdCode.Trim();
        var nos = await db.TCGInvoices
            .Where(x => x.OrgId == orgId
                && x.InvoiceIDType == "TCG"
                && x.InvoiceIDCode == idCode
                && x.TCGInvoiceNo != null)
            .Select(x => x.TCGInvoiceNo!)
            .ToListAsync();

        long max = 0;
        foreach (var n in nos)
            if (long.TryParse(n, out var v) && v > max) max = v;

        return (max + 1).ToString("D7");
    }

    /// <summary>Báo cáo tổng hợp hóa đơn TCG theo trạng thái và theo nhãn hiệu xe.</summary>
    public async Task<TCGInvoiceSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.TCGInvoices
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new TCGInvoiceSummaryDto
        {
            TotalInvoices = list.Count,
            PendingCount = list.Count(x => x.VatTCGStatus == TCGInvoiceStatus.Pending),
            FinishedCount = list.Count(x => x.VatTCGStatus == TCGInvoiceStatus.Finished),
            CancelledCount = list.Count(x => x.VatTCGStatus == TCGInvoiceStatus.Cancelled),
            TotalVehicles = list.Sum(x => x.Details.Count),
            TotalAmount = list.Sum(x => x.Details.Sum(d => d.TCGUnitPrice)),
            TotalVATAmount = list.Sum(x => x.Details.Sum(d => d.TCGUnitPrice - d.TInvoicePrice))
        };

        summary.ByBrand = list
            .SelectMany(x => x.Details)
            .GroupBy(d => string.IsNullOrWhiteSpace(d.BrandName) ? "(Chưa xác định)" : d.BrandName!)
            .Select(g => new TCGInvoiceBrandStatDto
            {
                BrandName = g.Key,
                InvoiceCount = g.Select(d => d.TCGInvoiceCode).Distinct().Count(),
                VehicleCount = g.Count(),
                TotalAmount = g.Sum(d => d.TCGUnitPrice)
            })
            .OrderByDescending(x => x.VehicleCount)
            .ThenBy(x => x.BrandName)
            .ToList();

        return summary;
    }
}
