using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Phiên bản Cước phí Vận tải (Transport Fee Version Master Data).
/// Tương ứng Mst_TranspFeeVer / Mst_TranspFee / Mst_TranspFeeHist trong
/// TERP.BizHTC/BizHTC.MasterData.cs (Mst_TranspFeeVerGet / Mst_TranspFeeVerGet_Hist /
/// Mst_TranspFeeVerGetCreate / Mst_TranspFeeVerDel) và màn hình quản lý cước vận tải
/// trong TERP.HTCClient hệ nguồn HTC 2010.
///
/// Nghiệp vụ: quản lý bảng cước phí vận tải xe theo PHIÊN BẢN (TFVCode). Mỗi phiên bản gồm
/// nhiều dòng cước theo tuyến đường (tỉnh/huyện đi → tỉnh/huyện đến), nhà vận tải và dòng xe,
/// kèm giá trị cước (ValFee) và số ngày vận chuyển định mức (ExpectedDays).
///
/// Quy tắc nghiệp vụ (theo hệ nguồn):
///   - Mã phiên bản (TFVCode) bắt buộc, không trùng.
///   - Mỗi dòng cước: ModelCode và TransporterCode có thể chứa nhiều mã phân tách bằng dấu phẩy,
///     hệ thống tự tách và nhân bản thành tích Đề-các (cartesian) như Mst_TranspFeeVerGetCreate.
///   - Không cho phép tuyến đường "A = A" (điểm đi trùng điểm đến).
///   - Không cho phép trùng khóa nghiệp vụ (tuyến + nhà vận tải + dòng xe) trong cùng phiên bản.
///   - Chỉ phiên bản ở trạng thái Draft mới được sửa/xóa; phiên bản Active không được xóa.
/// </summary>
public sealed class TransportFeeVersionService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách phiên bản cước phí vận tải kèm bộ lọc (mã phiên bản, trạng thái, nhà vận tải, dòng xe, từ khóa).
    /// Tương ứng Mst_TranspFeeVerGet.
    /// </summary>
    public async Task<List<TransportFeeVersion>> GetListAsync(
        Guid orgId,
        string? tfvCode = null,
        string? status = null,
        string? transporterCode = null,
        string? modelCode = null,
        string? query = null)
    {
        var q = db.TransportFeeVersions
            .Include(x => x.Rates)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(tfvCode))
        {
            string s = tfvCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.TFVCode.ToUpper().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransportFeeVersionStatus>(status, true, out var st))
            q = q.Where(x => x.Status == st);

        if (!string.IsNullOrWhiteSpace(transporterCode))
        {
            string t = transporterCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.Rates.Any(r => r.TransporterCode.ToUpper().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(modelCode))
        {
            string m = modelCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.Rates.Any(r => r.ModelCode.ToUpper().Contains(m)));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.TFVCode.ToUpper().Contains(s)
                || (x.Description != null && x.Description.ToUpper().Contains(s))
                || (x.Remark != null && x.Remark.ToUpper().Contains(s))
                || x.Rates.Any(r => r.TransporterCode.ToUpper().Contains(s)
                    || r.ModelCode.ToUpper().Contains(s)
                    || r.ProvinceCodeFrom.ToUpper().Contains(s)
                    || r.ProvinceCodeTo.ToUpper().Contains(s)));
        }

        return await q.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.TFVCode).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 phiên bản cước phí theo ID (kèm danh sách dòng cước).</summary>
    public async Task<TransportFeeVersion?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết theo mã phiên bản (TFVCode).</summary>
    public async Task<TransportFeeVersion?> GetByCodeAsync(Guid orgId, string tfvCode)
    {
        string code = tfvCode.Trim();
        return await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.TFVCode == code);
    }

    /// <summary>
    /// Lập phiên bản cước phí vận tải mới (tương ứng Mst_TranspFeeVerGetCreate).
    /// Tự sinh mã phiên bản nếu bỏ trống, kiểm tra trùng mã, bắt buộc có ít nhất 1 dòng cước,
    /// tách danh sách ModelCode/TransporterCode phân tách bằng dấu phẩy thành tích Đề-các,
    /// kiểm tra tuyến A=A và trùng khóa nghiệp vụ. Khởi tạo trạng thái Draft.
    /// </summary>
    public async Task<TransportFeeVersion> CreateAsync(Guid orgId, CreateTransportFeeVersionDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Phiên bản cước phí phải có ít nhất 1 dòng cước (Mst_TranspFee).");

        string code = string.IsNullOrWhiteSpace(dto.TFVCode)
            ? $"TFV-{DateTime.Now:yyyyMM}-{Random.Shared.Next(100, 999)}"
            : dto.TFVCode.Trim().ToUpperInvariant();

        if (code.Length < 3)
            throw new ArgumentException("Mã phiên bản cước (TFVCode) phải có tối thiểu 3 ký tự.");

        bool exists = await db.TransportFeeVersions.AnyAsync(x => x.OrgId == orgId && x.TFVCode == code);
        if (exists)
            throw new InvalidOperationException($"Mã phiên bản cước {code} đã tồn tại trong hệ thống.");

        var now = DateTime.Now;
        var entity = new TransportFeeVersion
        {
            OrgId = orgId,
            TFVCode = code,
            Description = dto.Description?.Trim(),
            Status = TransportFeeVersionStatus.Draft,
            CreatedDate = dto.CreatedDate?.Date ?? DateTime.Today,
            Remark = dto.Remark?.Trim(),
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Admin_HTC",
            CreatedAt = now
        };

        // Tách danh sách ModelCode / TransporterCode (phân tách bằng dấu phẩy) thành tích Đề-các.
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in dto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProvinceCodeFrom) || string.IsNullOrWhiteSpace(item.DistrictCodeFrom))
                throw new ArgumentException("Mỗi dòng cước phải có tỉnh/huyện đi (ProvinceCodeFrom, DistrictCodeFrom).");
            if (string.IsNullOrWhiteSpace(item.ProvinceCodeTo) || string.IsNullOrWhiteSpace(item.DistrictCodeTo))
                throw new ArgumentException("Mỗi dòng cước phải có tỉnh/huyện đến (ProvinceCodeTo, DistrictCodeTo).");
            if (string.IsNullOrWhiteSpace(item.TransporterCode))
                throw new ArgumentException("Mỗi dòng cước phải có nhà vận tải (TransporterCode).");
            if (string.IsNullOrWhiteSpace(item.ModelCode))
                throw new ArgumentException("Mỗi dòng cước phải có dòng xe (ModelCode).");

            string pFrom = item.ProvinceCodeFrom.Trim().ToUpperInvariant();
            string dFrom = item.DistrictCodeFrom.Trim().ToUpperInvariant();
            string pTo = item.ProvinceCodeTo.Trim().ToUpperInvariant();
            string dTo = item.DistrictCodeTo.Trim().ToUpperInvariant();

            // Kiểm tra tuyến A = A (điểm đi trùng điểm đến).
            if (pFrom == pTo && dFrom == dTo)
                throw new InvalidOperationException(
                    $"Tuyến đường không hợp lệ: điểm đi và điểm đến trùng nhau ({pFrom}/{dFrom}).");

            var transporters = SplitCodes(item.TransporterCode);
            var models = SplitCodes(item.ModelCode);

            foreach (var transporter in transporters)
            {
                foreach (var model in models)
                {
                    string key = $"{pFrom}|{dFrom}|{pTo}|{dTo}|{transporter}|{model}";
                    if (!seenKeys.Add(key))
                        throw new InvalidOperationException(
                            $"Dòng cước bị trùng khóa nghiệp vụ: tuyến {pFrom}/{dFrom} → {pTo}/{dTo}, " +
                            $"nhà vận tải {transporter}, dòng xe {model}.");

                    entity.Rates.Add(new TransportFeeRate
                    {
                        OrgId = orgId,
                        TFVCode = code,
                        ProvinceCodeFrom = pFrom,
                        ProvinceNameFrom = item.ProvinceNameFrom?.Trim(),
                        DistrictCodeFrom = dFrom,
                        DistrictNameFrom = item.DistrictNameFrom?.Trim(),
                        ProvinceCodeTo = pTo,
                        ProvinceNameTo = item.ProvinceNameTo?.Trim(),
                        DistrictCodeTo = dTo,
                        DistrictNameTo = item.DistrictNameTo?.Trim(),
                        TransporterCode = transporter,
                        TransporterName = item.TransporterName?.Trim(),
                        ModelCode = model,
                        ModelName = item.ModelName?.Trim(),
                        ValFee = item.ValFee,
                        ExpectedDays = item.ExpectedDays,
                        Remark = item.Remark?.Trim()
                    });
                }
            }
        }

        db.TransportFeeVersions.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Cập nhật thông tin mô tả / ghi chú phiên bản cước (chỉ khi phiên bản ở trạng thái Draft).
    /// </summary>
    public async Task<TransportFeeVersion> UpdateAsync(Guid orgId, long id, UpdateTransportFeeVersionDto dto)
    {
        var entity = await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phiên bản cước phí #{id}.");

        if (entity.Status != TransportFeeVersionStatus.Draft)
            throw new InvalidOperationException(
                $"Phiên bản {entity.TFVCode} không ở trạng thái nháp, không thể sửa (hiện tại: {entity.Status}).");

        if (dto.Description != null) entity.Description = dto.Description.Trim();
        if (dto.Remark != null) entity.Remark = dto.Remark.Trim();
        entity.UpdatedBy = !string.IsNullOrWhiteSpace(dto.UpdatedBy) ? dto.UpdatedBy.Trim() : "Admin_HTC";
        entity.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Áp dụng phiên bản cước (chuyển Draft → Active). Chỉ cho phép khi phiên bản đang ở trạng thái Draft.
    /// </summary>
    public async Task<TransportFeeVersion> ApplyAsync(Guid orgId, long id, string? appliedBy)
    {
        var entity = await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phiên bản cước phí #{id}.");

        if (entity.Status != TransportFeeVersionStatus.Draft)
            throw new InvalidOperationException(
                $"Phiên bản {entity.TFVCode} không ở trạng thái nháp, không thể áp dụng (hiện tại: {entity.Status}).");

        entity.Status = TransportFeeVersionStatus.Active;
        entity.AppliedDate = DateTime.Now;
        entity.UpdatedBy = !string.IsNullOrWhiteSpace(appliedBy) ? appliedBy.Trim() : "Admin_HTC";
        entity.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Ngừng áp dụng phiên bản cước (Active → Inactive).
    /// </summary>
    public async Task<TransportFeeVersion> DeactivateAsync(Guid orgId, long id, string? updatedBy)
    {
        var entity = await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phiên bản cước phí #{id}.");

        if (entity.Status != TransportFeeVersionStatus.Active)
            throw new InvalidOperationException(
                $"Phiên bản {entity.TFVCode} không ở trạng thái đang áp dụng (hiện tại: {entity.Status}).");

        entity.Status = TransportFeeVersionStatus.Inactive;
        entity.UpdatedBy = !string.IsNullOrWhiteSpace(updatedBy) ? updatedBy.Trim() : "Admin_HTC";
        entity.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Xóa phiên bản cước phí (tương ứng Mst_TranspFeeVerDel).
    /// Chỉ cho phép xóa khi phiên bản ở trạng thái Draft hoặc Inactive (không xóa phiên bản đang áp dụng Active).
    /// </summary>
    public async Task<bool> DeleteAsync(Guid orgId, long id)
    {
        var entity = await db.TransportFeeVersions
            .Include(x => x.Rates)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
        if (entity == null) return false;

        if (entity.Status == TransportFeeVersionStatus.Active)
            throw new InvalidOperationException(
                $"Phiên bản {entity.TFVCode} đang áp dụng, không thể xóa. Hãy ngừng áp dụng trước.");

        db.TransportFeeVersions.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Báo cáo tổng hợp phiên bản cước phí vận tải theo trạng thái và theo nhà vận tải.</summary>
    public async Task<TransportFeeVersionSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.TransportFeeVersions
            .Include(x => x.Rates)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var allRates = list.SelectMany(x => x.Rates).ToList();

        var summary = new TransportFeeVersionSummaryDto
        {
            TotalVersions = list.Count,
            DraftCount = list.Count(x => x.Status == TransportFeeVersionStatus.Draft),
            ActiveCount = list.Count(x => x.Status == TransportFeeVersionStatus.Active),
            InactiveCount = list.Count(x => x.Status == TransportFeeVersionStatus.Inactive),
            CancelledCount = list.Count(x => x.Status == TransportFeeVersionStatus.Cancelled),
            TotalRates = allRates.Count,
            TotalFeeValue = allRates.Sum(r => r.ValFee)
        };

        summary.ByTransporter = allRates
            .GroupBy(r => string.IsNullOrWhiteSpace(r.TransporterCode) ? "(Chưa xác định)" : r.TransporterCode)
            .Select(g => new TransportFeeVersionStatDto
            {
                TransporterCode = g.Key,
                TransporterName = g.Select(r => r.TransporterName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                RateCount = g.Count(),
                TotalFeeValue = g.Sum(r => r.ValFee),
                AvgFeeValue = g.Count() > 0 ? Math.Round((decimal)g.Sum(r => r.ValFee) / g.Count(), 0) : 0
            })
            .OrderByDescending(x => x.RateCount)
            .ThenBy(x => x.TransporterCode)
            .ToList();

        return summary;
    }

    /// <summary>Tách chuỗi mã phân tách bằng dấu phẩy thành danh sách mã (chuẩn hóa, bỏ rỗng, loại trùng).</summary>
    private static List<string> SplitCodes(string raw)
    {
        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToUpperInvariant())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}