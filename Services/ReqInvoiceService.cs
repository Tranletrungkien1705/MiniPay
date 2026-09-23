using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Đề nghị Xuất hóa đơn / Giao hồ sơ xe ô tô (RD_ReqInvoice).
/// Tương ứng bảng RD_ReqInvoice, RD_ReqInvoiceDtl và các nghiệp vụ trong hệ nguồn 2010.HTC:
///   - RD_ReqInvoiceCreate        (lập đề nghị giao hồ sơ)
///   - RD_ReqInvoiceSearch        (tra cứu đề nghị theo số / ngày / VIN / đại lý / trạng thái)
///   - RD_ReqInvoiceDtlApprove    (duyệt 1 dòng chi tiết xe)
///   - RD_ReqInvoiceDtlDelete     (xóa 1 dòng chi tiết xe)
///   - RD_ReqInvoiceDelete        (xóa toàn bộ đề nghị)
///   - GetReqIVNo                 (sinh số đề nghị)
///   - màn hình FrmMngRDInvoice, FrmNewRDInvoice, FrmPrintRDInvoice (Views/Sales/Redeem).
///
/// Nghiệp vụ: HTC lập đề nghị giao hồ sơ (hóa đơn GTGT, chứng từ gốc) cho đại lý hoặc ngân hàng
/// (bảo lãnh BL / thư tín dụng LC) theo từng xe (VIN). Mỗi dòng chi tiết xe được duyệt riêng khi
/// đủ điều kiện; khi tất cả dòng đã duyệt thì đề nghị chuyển trạng thái Approved.
/// </summary>
public sealed class ReqInvoiceService(AppDbContext db)
{
    /// <summary>
    /// Tra cứu danh sách đề nghị giao hồ sơ kèm bộ lọc (RD_ReqInvoiceSearch).
    /// </summary>
    public async Task<List<ReqInvoice>> GetListAsync(
        Guid orgId,
        string? reqIVNo = null,
        string? vin = null,
        string? carId = null,
        string? dealerCode = null,
        string? typeRDReqIv = null,
        string? status = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null)
    {
        var q = db.ReqInvoices
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(reqIVNo))
        {
            string s = reqIVNo.Trim().ToUpperInvariant();
            q = q.Where(x => x.ReqIVNo.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(vin))
        {
            string s = vin.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.VIN.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(carId))
        {
            string s = carId.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.CarId != null && d.CarId.ToUpper().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            string s = dealerCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.DealerCode != null && d.DealerCode.ToUpper().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(typeRDReqIv) && Enum.TryParse<RDInvoiceType>(typeRDReqIv, true, out var t))
        {
            q = q.Where(x => x.Details.Any(d => d.TypeRDReqIv == t));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReqIVStatus>(status, true, out var st))
        {
            q = q.Where(x => x.ReqIVStatus == st);
        }

        if (createdFrom.HasValue)
            q = q.Where(x => x.CreatedAt >= createdFrom.Value);

        if (createdTo.HasValue)
            q = q.Where(x => x.CreatedAt <= createdTo.Value);

        return await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    /// <summary>Lấy chi tiết đề nghị theo ID.</summary>
    public async Task<ReqInvoice?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.ReqInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết đề nghị theo số đề nghị (ReqIVNo).</summary>
    public async Task<ReqInvoice?> GetByNoAsync(Guid orgId, string reqIVNo)
    {
        string no = (reqIVNo ?? "").Trim().ToUpperInvariant();
        return await db.ReqInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.ReqIVNo == no);
    }

    /// <summary>
    /// Lập đề nghị giao hồ sơ mới (RD_ReqInvoiceCreate / FrmNewRDInvoice).
    /// Tự sinh số đề nghị RDIV-yyyyMM-xxx nếu không truyền vào.
    /// </summary>
    public async Task<ReqInvoice> CreateAsync(Guid orgId, CreateReqInvoiceDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Đề nghị giao hồ sơ phải có ít nhất 1 dòng xe.", nameof(dto.Items));

        // Kiểm tra trùng VIN trong cùng đề nghị.
        var dupVin = dto.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.VIN))
            .GroupBy(i => i.VIN.Trim().ToUpperInvariant())
            .FirstOrDefault(g => g.Count() > 1);
        if (dupVin != null)
            throw new InvalidOperationException($"Số khung (VIN) '{dupVin.Key}' bị lặp trong đề nghị.");

        // Sinh số đề nghị (GetReqIVNo).
        string no;
        if (!string.IsNullOrWhiteSpace(dto.ReqIVNo))
        {
            no = dto.ReqIVNo.Trim().ToUpperInvariant();
            bool exists = await db.ReqInvoices.AnyAsync(x => x.OrgId == orgId && x.ReqIVNo == no);
            if (exists)
                throw new InvalidOperationException($"Số đề nghị giao hồ sơ '{no}' đã tồn tại trong hệ thống.");
        }
        else
        {
            string monthPrefix = $"RDIV-{DateTime.Now:yyyyMM}-";
            int count = await db.ReqInvoices.CountAsync(x => x.OrgId == orgId && x.ReqIVNo.StartsWith(monthPrefix));
            no = $"{monthPrefix}{(count + 1):D3}";
        }

        var req = new ReqInvoice
        {
            OrgId = orgId,
            ReqIVNo = no,
            ReqIVStatus = ReqIVStatus.Pending,
            Remark = dto.Remark,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "Admin_HTC" : dto.CreatedBy,
            CreatedAt = DateTime.Now
        };

        foreach (var item in dto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN))
                throw new ArgumentException("Mỗi dòng xe phải có số khung (VIN).", nameof(dto.Items));

            req.Details.Add(new ReqInvoiceDetail
            {
                OrgId = orgId,
                ReqIVNo = no,
                VIN = item.VIN.Trim().ToUpperInvariant(),
                CarId = item.CarId,
                ModelCode = item.ModelCode,
                ModelName = item.ModelName,
                ColorCode = item.ColorCode,
                ColorName = item.ColorName,
                EngineNo = item.EngineNo,
                TypeRDReqIv = item.TypeRDReqIv,
                DealerCode = item.DealerCode,
                DealerName = item.DealerName,
                MortageBankCode = item.MortageBankCode,
                GuaranteeNo = item.GuaranteeNo,
                PGBankCode = item.PGBankCode,
                PGBankCodeMonitor = item.PGBankCodeMonitor,
                PGDateExpired = item.PGDateExpired,
                HTCInvoiceNo = item.HTCInvoiceNo,
                TCGInvoiceNo = item.TCGInvoiceNo,
                DlrCtrNo = item.DlrCtrNo,
                ProvinceName = item.ProvinceName,
                RDReqIvDtlStatus = RDReqIvDtlStatus.Pending,
                Remark = item.Remark,
                CreatedBy = req.CreatedBy,
                CreatedAt = DateTime.Now
            });
        }

        db.ReqInvoices.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Duyệt 1 dòng chi tiết đề nghị giao hồ sơ (RD_ReqInvoiceDtlApprove).
    /// Chỉ duyệt được dòng đang ở trạng thái Pending. Khi tất cả dòng đã duyệt,
    /// đề nghị tự động chuyển trạng thái Approved.
    /// </summary>
    public async Task<ReqInvoice?> ApproveDetailAsync(
        Guid orgId, long reqInvoiceId, string vin, string? carId, string? approvedBy)
    {
        var req = await db.ReqInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == reqInvoiceId);
        if (req == null) return null;

        string v = (vin ?? "").Trim().ToUpperInvariant();
        string? c = string.IsNullOrWhiteSpace(carId) ? null : carId.Trim().ToUpperInvariant();

        var detail = req.Details.FirstOrDefault(d =>
            d.VIN == v && (c == null || (d.CarId != null && d.CarId.ToUpperInvariant() == c)));
        if (detail == null)
            throw new InvalidOperationException($"Không tìm thấy dòng xe VIN '{v}' trong đề nghị {req.ReqIVNo}.");

        if (detail.RDReqIvDtlStatus != RDReqIvDtlStatus.Pending)
            throw new InvalidOperationException(
                $"Dòng xe VIN '{v}' đang ở trạng thái {detail.RDReqIvDtlStatus}, chỉ duyệt được dòng đang xử lý (Pending).");

        detail.RDReqIvDtlStatus = RDReqIvDtlStatus.Approved;
        detail.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "KeToan_HTC" : approvedBy;
        detail.ApprovedAt = DateTime.Now;

        // Nếu tất cả dòng đã duyệt → chuyển đề nghị sang Approved.
        if (req.Details.All(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Approved))
        {
            req.ReqIVStatus = ReqIVStatus.Approved;
            req.ApprovedBy = detail.ApprovedBy;
            req.ApprovedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Xóa 1 dòng chi tiết đề nghị giao hồ sơ (RD_ReqInvoiceDtlDelete).
    /// Không cho xóa dòng đã duyệt.
    /// </summary>
    public async Task<ReqInvoice?> DeleteDetailAsync(Guid orgId, long reqInvoiceId, string vin, string? carId)
    {
        var req = await db.ReqInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == reqInvoiceId);
        if (req == null) return null;

        string v = (vin ?? "").Trim().ToUpperInvariant();
        string? c = string.IsNullOrWhiteSpace(carId) ? null : carId.Trim().ToUpperInvariant();

        var detail = req.Details.FirstOrDefault(d =>
            d.VIN == v && (c == null || (d.CarId != null && d.CarId.ToUpperInvariant() == c)));
        if (detail == null)
            throw new InvalidOperationException($"Không tìm thấy dòng xe VIN '{v}' trong đề nghị {req.ReqIVNo}.");

        if (detail.RDReqIvDtlStatus == RDReqIvDtlStatus.Approved)
            throw new InvalidOperationException($"Dòng xe VIN '{v}' đã được duyệt, không thể xóa.");

        db.ReqInvoiceDetails.Remove(detail);
        req.Details.Remove(detail);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Xóa toàn bộ đề nghị giao hồ sơ (RD_ReqInvoiceDelete).
    /// Không cho xóa đề nghị đã duyệt.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid orgId, long reqInvoiceId)
    {
        var req = await db.ReqInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == reqInvoiceId);
        if (req == null) return false;

        if (req.ReqIVStatus == ReqIVStatus.Approved)
            throw new InvalidOperationException($"Đề nghị {req.ReqIVNo} đã được duyệt, không thể xóa.");

        db.ReqInvoices.Remove(req);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Báo cáo tổng hợp đề nghị giao hồ sơ theo loại và theo đại lý.</summary>
    public async Task<ReqInvoiceSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var reqs = await db.ReqInvoices
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var details = reqs.SelectMany(r => r.Details).ToList();

        var summary = new ReqInvoiceSummaryDto
        {
            TotalRequests = reqs.Count,
            TotalDetails = details.Count,
            PendingDetails = details.Count(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Pending),
            ApprovedDetails = details.Count(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Approved),
            CancelledDetails = details.Count(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Cancelled)
        };

        summary.ByType = details
            .GroupBy(d => d.TypeRDReqIv)
            .Select(g => new ReqInvoiceTypeStatDto
            {
                TypeRDReqIv = g.Key.ToString(),
                DetailCount = g.Count(),
                ApprovedCount = g.Count(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Approved)
            })
            .OrderByDescending(x => x.DetailCount)
            .ToList();

        summary.ByDealer = details
            .Where(d => !string.IsNullOrWhiteSpace(d.DealerCode))
            .GroupBy(d => d.DealerCode!)
            .Select(g => new ReqInvoiceDealerStatDto
            {
                DealerCode = g.Key,
                DealerName = g.Select(d => d.DealerName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "",
                DetailCount = g.Count(),
                ApprovedCount = g.Count(d => d.RDReqIvDtlStatus == RDReqIvDtlStatus.Approved)
            })
            .OrderByDescending(x => x.DetailCount)
            .ThenBy(x => x.DealerCode)
            .ToList();

        return summary;
    }
}
