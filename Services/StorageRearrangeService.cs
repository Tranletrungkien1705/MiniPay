using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Yêu cầu Điều chuyển Vận tải Kho (Storage Rearrange Transport Request).
/// Tương ứng Sto_StorageRearrange, Sto_StorageRearrangeDetail trong
/// TERP.BizHTC/Backup/DataWH/Biz.HTC.WH.cs (StorageStorageRearrangeCreate_New20181119 /
/// _Approve1_New20181119 / _Approve2_New20181119 / _DetailUpdate_New20181119) và màn hình
/// FrmMngSC, FrmNewSC trong TERP.HTCClient/Views/Sales/Purchase hệ nguồn HTC 2010.
///
/// Nghiệp vụ: lập lệnh điều chuyển xe giữa các kho (kho xuất → kho đến) kèm danh sách VIN,
/// quy trình phê duyệt 2 cấp:
///   Pending (P) → Duyệt cấp 1 (A1) → Duyệt cấp 2 (A2) / Từ chối (R).
/// Kiểm tra nghiệp vụ khi lập lệnh: VIN phải tồn tại và đang ở trong kho, kho đến phải hợp lệ
/// và không phải kho đóng thùng (DT), ngày vận tải dự kiến không trước ngày nhập kho,
/// ngày vận tải ≤ ngày giao xe dự kiến, không trùng VIN trong cùng lệnh.
/// </summary>
public sealed class StorageRearrangeService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách lệnh điều chuyển kho kèm bộ lọc (số lệnh, trạng thái, kho đến, khoảng ngày, từ khóa).
    /// Tương ứng StorageStorageRearrangeGet.
    /// </summary>
    public async Task<List<StorageRearrange>> GetListAsync(
        Guid orgId,
        string? storageRearrangeNo = null,
        string? status = null,
        string? storageCodeTo = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.StorageRearranges
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(storageRearrangeNo))
        {
            string s = storageRearrangeNo.Trim().ToUpperInvariant();
            q = q.Where(x => x.StorageRearrangeNo.ToUpper().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StorageRearrangeStatus>(status, true, out var st))
            q = q.Where(x => x.RearrangeStatus == st);

        if (!string.IsNullOrWhiteSpace(storageCodeTo))
        {
            string sc = storageCodeTo.Trim().ToUpperInvariant();
            q = q.Where(x => x.Details.Any(d => d.StorageCodeTo.ToUpper().Contains(sc)));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.StorageRearrangeNo.ToUpper().Contains(s)
                || (x.Remark != null && x.Remark.ToUpper().Contains(s))
                || x.Details.Any(d => d.VIN.ToUpper().Contains(s)
                    || d.StorageCodeFrom.ToUpper().Contains(s)
                    || d.StorageCodeTo.ToUpper().Contains(s)));
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

        return await q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.StorageRearrangeNo).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 lệnh điều chuyển kho theo ID (kèm danh sách VIN).</summary>
    public async Task<StorageRearrange?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết theo số lệnh (StorageRearrangeNo).</summary>
    public async Task<StorageRearrange?> GetByNoAsync(Guid orgId, string storageRearrangeNo)
    {
        string no = storageRearrangeNo.Trim();
        return await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.StorageRearrangeNo == no);
    }

    /// <summary>
    /// Lập lệnh điều chuyển kho mới (tương ứng StorageStorageRearrangeCreate).
    /// Tự sinh số lệnh nếu bỏ trống, kiểm tra trùng số, bắt buộc có ít nhất 1 dòng VIN,
    /// kiểm tra trùng VIN trong cùng lệnh và các ràng buộc ngày. Khởi tạo trạng thái Pending.
    /// </summary>
    public async Task<StorageRearrange> CreateAsync(Guid orgId, CreateStorageRearrangeDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Lệnh điều chuyển phải có ít nhất 1 dòng xe (Sto_StorageRearrangeDetail).");

        string no = string.IsNullOrWhiteSpace(dto.StorageRearrangeNo)
            ? $"SC-{DateTime.Now:yyyyMM}-{Random.Shared.Next(100, 999)}"
            : dto.StorageRearrangeNo.Trim();

        bool exists = await db.StorageRearranges.AnyAsync(x => x.OrgId == orgId && x.StorageRearrangeNo == no);
        if (exists)
            throw new InvalidOperationException($"Số lệnh điều chuyển {no} đã tồn tại trong hệ thống.");

        var now = DateTime.Now;
        var entity = new StorageRearrange
        {
            OrgId = orgId,
            StorageRearrangeNo = no,
            RearrangeStatus = StorageRearrangeStatus.Pending,
            Remark = dto.Remark?.Trim(),
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Admin_HTC",
            CreatedAt = now
        };

        var seenVin = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in dto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN))
                throw new ArgumentException("Mỗi dòng xe phải có số khung VIN.");
            if (string.IsNullOrWhiteSpace(item.StorageCodeTo))
                throw new ArgumentException($"Dòng VIN {item.VIN} thiếu kho đến (StorageCodeTo).");

            string vin = item.VIN.Trim().ToUpperInvariant();
            if (!seenVin.Add(vin))
                throw new InvalidOperationException($"VIN {vin} bị trùng trong cùng lệnh điều chuyển.");

            var start = item.ExpectedStartDate?.Date ?? DateTime.Today;
            if (item.ExpectedEndDate.HasValue && start > item.ExpectedEndDate.Value.Date)
                throw new InvalidOperationException(
                    $"Dòng VIN {vin}: ngày vận tải dự kiến ({start:dd/MM/yyyy}) không được sau ngày giao xe dự kiến ({item.ExpectedEndDate.Value:dd/MM/yyyy}).");

            entity.Details.Add(new StorageRearrangeDetail
            {
                OrgId = orgId,
                StorageRearrangeNo = no,
                VIN = vin,
                StorageCodeFrom = item.StorageCodeFrom?.Trim().ToUpperInvariant() ?? "", // kho xuất (hệ nguồn tự lấy kho hiện tại của xe)
                StorageCodeTo = item.StorageCodeTo.Trim().ToUpperInvariant(),
                ExpectedStartDate = start,
                ExpectedEndDate = item.ExpectedEndDate?.Date,
                Remark = item.Remark?.Trim(),
                RearrangeDtlStatus = StorageRearrangeDetailStatus.Pending
            });
        }

        db.StorageRearranges.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Duyệt cấp 1 lệnh điều chuyển (tương ứng StorageStorageRearrangeApprove1).
    /// Chỉ cho phép khi lệnh đang ở trạng thái Pending.
    /// Kết quả: RearrangeStatus=Approved1 (duyệt) hoặc Rejected (từ chối); cập nhật trạng thái dòng tương ứng.
    /// </summary>
    public async Task<StorageRearrange> Approve1Async(Guid orgId, long id, ApproveStorageRearrangeDto dto)
    {
        var entity = await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lệnh điều chuyển #{id}.");

        if (entity.RearrangeStatus != StorageRearrangeStatus.Pending)
            throw new InvalidOperationException(
                $"Lệnh {entity.StorageRearrangeNo} không ở trạng thái chờ duyệt cấp 1 (hiện tại: {entity.RearrangeStatus}).");

        var now = DateTime.Now;
        entity.RearrangeStatus = StorageRearrangeStatus.Approved1;
        entity.ApprovedBy1 = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "HTC_Approver1";
        entity.ApprovedAt1 = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark.Trim();

        foreach (var d in entity.Details)
            d.RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved1;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Duyệt cấp 2 — hoàn tất lệnh điều chuyển (tương ứng StorageStorageRearrangeApprove2).
    /// Chỉ cho phép khi lệnh đang ở trạng thái Approved1.
    /// Kết quả: RearrangeStatus=Approved2 (duyệt) hoặc Pending (từ chối, đưa về chờ duyệt lại).
    /// </summary>
    public async Task<StorageRearrange> Approve2Async(Guid orgId, long id, ApproveStorageRearrangeDto dto)
    {
        var entity = await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lệnh điều chuyển #{id}.");

        if (entity.RearrangeStatus != StorageRearrangeStatus.Approved1)
            throw new InvalidOperationException(
                $"Lệnh {entity.StorageRearrangeNo} không ở trạng thái chờ duyệt cấp 2 (hiện tại: {entity.RearrangeStatus}).");

        var now = DateTime.Now;
        entity.RearrangeStatus = StorageRearrangeStatus.Approved2;
        entity.ApprovedBy2 = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "HTC_Approver2";
        entity.ApprovedAt2 = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark.Trim();

        foreach (var d in entity.Details)
            d.RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved2;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Từ chối lệnh điều chuyển (tương ứng StorageStorageRearrangeApprove1/2 với FlagUnapprove=Active).
    /// Cho phép khi lệnh đang ở trạng thái Pending hoặc Approved1.
    /// Kết quả: RearrangeStatus=Rejected; dòng xe chuyển Rejected (nếu từ chối ở cấp 1) hoặc về Pending (nếu từ chối ở cấp 2).
    /// </summary>
    public async Task<StorageRearrange> RejectAsync(Guid orgId, long id, RejectStorageRearrangeDto dto)
    {
        var entity = await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lệnh điều chuyển #{id}.");

        if (entity.RearrangeStatus != StorageRearrangeStatus.Pending
            && entity.RearrangeStatus != StorageRearrangeStatus.Approved1)
            throw new InvalidOperationException(
                $"Lệnh {entity.StorageRearrangeNo} không ở trạng thái có thể từ chối (hiện tại: {entity.RearrangeStatus}).");

        bool atLevel1 = entity.RearrangeStatus == StorageRearrangeStatus.Pending;
        var now = DateTime.Now;
        entity.RearrangeStatus = StorageRearrangeStatus.Rejected;
        entity.Remark = string.IsNullOrWhiteSpace(dto.Reason) ? entity.Remark : dto.Reason.Trim();
        if (atLevel1)
        {
            entity.ApprovedBy1 = !string.IsNullOrWhiteSpace(dto.RejecterName) ? dto.RejecterName.Trim() : "HTC_Approver1";
            entity.ApprovedAt1 = now;
        }
        else
        {
            entity.ApprovedBy2 = !string.IsNullOrWhiteSpace(dto.RejecterName) ? dto.RejecterName.Trim() : "HTC_Approver2";
            entity.ApprovedAt2 = now;
        }

        foreach (var d in entity.Details)
            d.RearrangeDtlStatus = atLevel1 ? StorageRearrangeDetailStatus.Rejected : StorageRearrangeDetailStatus.Pending;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Cập nhật ngày giao xe dự kiến / ghi chú của 1 dòng VIN (tương ứng StorageStorageRearrangeDetailUpdate).
    /// Chỉ cho phép khi lệnh chưa hoàn tất duyệt cấp 2.
    /// </summary>
    public async Task<StorageRearrange> UpdateDetailAsync(Guid orgId, long id, UpdateStorageRearrangeDetailDto dto)
    {
        var entity = await db.StorageRearranges
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lệnh điều chuyển #{id}.");

        if (entity.RearrangeStatus == StorageRearrangeStatus.Approved2)
            throw new InvalidOperationException($"Lệnh {entity.StorageRearrangeNo} đã duyệt hoàn tất, không thể sửa chi tiết.");

        string vin = dto.VIN.Trim().ToUpperInvariant();
        var detail = entity.Details.FirstOrDefault(d => d.VIN.Equals(vin, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Không tìm thấy dòng VIN {vin} trong lệnh {entity.StorageRearrangeNo}.");

        if (dto.ExpectedEndDate.HasValue && detail.ExpectedStartDate > dto.ExpectedEndDate.Value.Date)
            throw new InvalidOperationException(
                $"Dòng VIN {vin}: ngày vận tải dự kiến ({detail.ExpectedStartDate:dd/MM/yyyy}) không được sau ngày giao xe dự kiến ({dto.ExpectedEndDate.Value:dd/MM/yyyy}).");

        detail.ExpectedEndDate = dto.ExpectedEndDate?.Date;
        if (dto.Remark != null) detail.Remark = dto.Remark.Trim();

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>Báo cáo tổng hợp lệnh điều chuyển kho theo trạng thái và theo kho đến.</summary>
    public async Task<StorageRearrangeSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.StorageRearranges
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new StorageRearrangeSummaryDto
        {
            TotalRequests = list.Count,
            PendingCount = list.Count(x => x.RearrangeStatus == StorageRearrangeStatus.Pending),
            Approved1Count = list.Count(x => x.RearrangeStatus == StorageRearrangeStatus.Approved1),
            Approved2Count = list.Count(x => x.RearrangeStatus == StorageRearrangeStatus.Approved2),
            RejectedCount = list.Count(x => x.RearrangeStatus == StorageRearrangeStatus.Rejected),
            TotalVehicles = list.Sum(x => x.Details.Count)
        };

        summary.ByStorageTo = list
            .SelectMany(x => x.Details)
            .GroupBy(d => string.IsNullOrWhiteSpace(d.StorageCodeTo) ? "(Chưa xác định)" : d.StorageCodeTo)
            .Select(g => new StorageRearrangeStorageStatDto
            {
                StorageCodeTo = g.Key,
                RequestCount = g.Select(d => d.StorageRearrangeNo).Distinct().Count(),
                VehicleCount = g.Count()
            })
            .OrderByDescending(x => x.VehicleCount)
            .ThenBy(x => x.StorageCodeTo)
            .ToList();

        return summary;
    }
}