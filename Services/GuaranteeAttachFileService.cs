using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý File / Chứng từ Đính kèm Thư Bảo lãnh Thanh toán Ngân hàng
/// (Bank Payment Guarantee Attachment File Management).
///
/// Tương ứng hệ nguồn 2010.HTC (BizHTC.TCFIntergration):
///   - PaymentGuarantee_SaveFile            (lưu danh sách file đính kèm của 1 thư bảo lãnh)
///   - Pmt_GuaranteeAttachFileX_CheckFileExistServer (kiểm tra file tồn tại trên server)
///   - bảng Pmt_GuaranteeAttachFile / Pmt_GuaranteeAttachFileHis
///   - màn hình FrmMngGrt / FrmNewGrt (tab đính kèm chứng từ).
///
/// Nghiệp vụ chính:
///   1. Mỗi thư bảo lãnh có nhiều file đính kèm, đánh số thứ tự FileIndex (1,2,3...).
///   2. Khi lưu, hệ thống so khớp theo TÊN FILE để xác định file THÊM MỚI và file BỊ XÓA.
///   3. File thêm mới được đặt tên theo quy ước:
///        {DealerCode}-{BankCode}-{BankGuaranteeNo}-{FileIndex:00}{ext}
///   4. Ràng buộc quyền theo trạng thái thư bảo lãnh (tương ứng Port Check trong nguồn):
///        - Đại lý (FlagDirect = Inactive): được THÊM khi trạng thái Active/PendingApproval;
///          chỉ được XÓA khi trạng thái PendingApproval.
///        - HTC (FlagDirect = Active): được thêm/sửa/xóa ở mọi trạng thái Active/PendingApproval/Rejected/Cancelled.
///   5. Tổng dung lượng file đính kèm của 1 thư bảo lãnh không vượt quá 5MB (5*1024*1024 bytes).
///   6. Mỗi lần lưu, toàn bộ trạng thái file hiện hành được ghi vào bảng lịch sử (Pmt_GuaranteeAttachFileHis).
/// </summary>
public sealed class GuaranteeAttachFileService(AppDbContext db)
{
    /// <summary>Giới hạn tổng dung lượng file đính kèm cho 1 thư bảo lãnh (5MB).</summary>
    public const long MaxTotalSizeInBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Lấy danh sách file đính kèm của 1 thư bảo lãnh (theo GuaranteeNo), sắp xếp theo FileIndex.
    /// </summary>
    public async Task<List<GuaranteeAttachFile>> GetFilesAsync(Guid orgId, string guaranteeNo)
    {
        string no = (guaranteeNo ?? "").Trim();
        return await db.GuaranteeAttachFiles
            .Where(x => x.OrgId == orgId && x.GuaranteeNo == no)
            .OrderBy(x => x.FileIndex)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy lịch sử các lần lưu file đính kèm của 1 thư bảo lãnh (mới nhất trước).
    /// </summary>
    public async Task<List<GuaranteeAttachFileHis>> GetHistoryAsync(Guid orgId, string guaranteeNo)
    {
        string no = (guaranteeNo ?? "").Trim();
        return await db.GuaranteeAttachFileHis
            .Where(x => x.OrgId == orgId && x.GuaranteeNo == no)
            .OrderByDescending(x => x.LogLUDateTime)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Kiểm tra file đính kèm có tồn tại trên server hay không (tương ứng
    /// Pmt_GuaranteeAttachFileX_CheckFileExistServer). Trả về danh sách file bị thiếu.
    /// </summary>
    public async Task<List<GuaranteeAttachFile>> CheckFileExistAsync(Guid orgId, string guaranteeNo, string? rootPath = null)
    {
        var files = await GetFilesAsync(orgId, guaranteeNo);
        var missing = new List<GuaranteeAttachFile>();
        foreach (var f in files)
        {
            if (string.IsNullOrWhiteSpace(f.GrtFilePath)) continue;
            string full = string.IsNullOrWhiteSpace(rootPath)
                ? f.GrtFilePath
                : Path.Combine(rootPath, f.GrtFilePath.TrimStart('/', '\\'));
            if (!File.Exists(full)) missing.Add(f);
        }
        return missing;
    }

    /// <summary>
    /// Lưu danh sách file đính kèm của 1 thư bảo lãnh (tương ứng PaymentGuarantee_SaveFile).
    ///
    /// Thuật toán:
    ///   - Kiểm tra thư bảo lãnh tồn tại (theo GuaranteeNo).
    ///   - Validate từng file: GrtFilePath, GrtFileName không rỗng; FileSizeInBytes > 0.
    ///   - Gán FileIndex tuần tự 1..n theo thứ tự đầu vào.
    ///   - So khớp theo GrtFileName để xác định file THÊM (có trong input, chưa có trong DB)
    ///     và file XÓA (có trong DB, không còn trong input).
    ///   - Áp dụng ràng buộc quyền theo trạng thái thư bảo lãnh (isHtcUser).
    ///   - Kiểm tra tổng dung lượng <= 5MB.
    ///   - Đặt lại tên file thêm mới theo quy ước {DealerCode}-{BankCode}-{BankGuaranteeNo}-{FileIndex:00}{ext}.
    ///   - Ghi lại toàn bộ trạng thái file hiện hành vào bảng lịch sử.
    /// </summary>
    public async Task<GuaranteeAttachFileSaveResultDto> SaveFilesAsync(
        Guid orgId,
        string guaranteeNo,
        List<GuaranteeAttachFileInputDto>? items,
        bool isHtcUser = true,
        string? updatedBy = null)
    {
        string no = (guaranteeNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(no))
            throw new ArgumentException("Cần số bảo lãnh hệ thống (GuaranteeNo).");

        var grt = await db.Guarantees.FirstOrDefaultAsync(g => g.OrgId == orgId && g.GuaranteeNo == no)
            ?? throw new KeyNotFoundException($"Không tìm thấy Thư bảo lãnh {no}.");

        items ??= [];

        // Validate & gán FileIndex tuần tự.
        var normalized = new List<GuaranteeAttachFileInputDto>();
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (string.IsNullOrWhiteSpace(it.GrtFilePath))
                throw new ArgumentException($"File thứ {i + 1}: đường dẫn file (GrtFilePath) không được để trống.");
            if (string.IsNullOrWhiteSpace(it.GrtFileName))
                throw new ArgumentException($"File thứ {i + 1}: tên file (GrtFileName) không được để trống.");
            if (it.FileSizeInBytes <= 0)
                throw new ArgumentException($"File thứ {i + 1} ({it.GrtFileName}): kích thước file phải > 0.");
            normalized.Add(it);
        }

        var existing = await db.GuaranteeAttachFiles
            .Where(x => x.OrgId == orgId && x.GuaranteeNo == no)
            .ToListAsync();

        var existingNames = existing.Select(x => x.GrtFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var inputNames = normalized.Select(x => x.GrtFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = normalized.Where(x => !existingNames.Contains(x.GrtFileName)).ToList();
        var toDelete = existing.Where(x => !inputNames.Contains(x.GrtFileName)).ToList();

        // Ràng buộc quyền theo trạng thái (Port Check trong nguồn).
        if (!isHtcUser)
        {
            // Đại lý: thêm khi Active/PendingApproval.
            if (toAdd.Count > 0 && grt.Status != GuaranteeStatus.Active && grt.Status != GuaranteeStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Đại lý chỉ được thêm file đính kèm khi thư bảo lãnh ở trạng thái Chờ duyệt hoặc Đang hiệu lực (hiện tại: {grt.Status}).");
            // Đại lý: xóa chỉ khi PendingApproval.
            if (toDelete.Count > 0 && grt.Status != GuaranteeStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Đại lý chỉ được xóa file đính kèm khi thư bảo lãnh ở trạng thái Chờ duyệt (hiện tại: {grt.Status}).");
        }

        // Kiểm tra tổng dung lượng <= 5MB.
        long totalSize = normalized.Sum(x => x.FileSizeInBytes);
        if (totalSize > MaxTotalSizeInBytes)
            throw new InvalidOperationException(
                $"Tổng dung lượng file đính kèm ({totalSize:N0} bytes) vượt quá giới hạn cho phép ({MaxTotalSizeInBytes:N0} bytes = 5MB).");

        var now = DateTime.Now;
        string user = string.IsNullOrWhiteSpace(updatedBy) ? "Admin_HTC" : updatedBy!.Trim();

        // Xóa các file không còn trong danh sách.
        if (toDelete.Count > 0)
            db.GuaranteeAttachFiles.RemoveRange(toDelete);

        // Thêm file mới với tên chuẩn hóa.
        int maxIndex = existing.Count == 0 ? 0 : existing.Max(x => x.FileIndex);
        var addedNames = new List<string>();
        foreach (var it in toAdd)
        {
            maxIndex++;
            string ext = Path.GetExtension(it.GrtFileName).ToUpperInvariant();
            string newName = $"{grt.PartnerCode}-{grt.BankCode}-{grt.BankGuaranteeNo}-{maxIndex:00}{ext}";
            db.GuaranteeAttachFiles.Add(new GuaranteeAttachFile
            {
                OrgId = orgId,
                GuaranteeId = grt.Id,
                GuaranteeNo = no,
                FileIndex = maxIndex,
                GrtFilePath = it.GrtFilePath.Trim(),
                GrtFileName = newName,
                FileSizeInBytes = it.FileSizeInBytes,
                GrtFileRemark = it.GrtFileRemark?.Trim(),
                LogLUBy = user,
                LogLUDateTime = now
            });
            addedNames.Add(newName);
        }

        await db.SaveChangesAsync();

        // Ghi lịch sử toàn bộ trạng thái file hiện hành (tương ứng insert Pmt_GuaranteeAttachFileHis).
        var current = await db.GuaranteeAttachFiles
            .Where(x => x.OrgId == orgId && x.GuaranteeNo == no)
            .OrderBy(x => x.FileIndex)
            .ToListAsync();

        foreach (var f in current)
        {
            db.GuaranteeAttachFileHis.Add(new GuaranteeAttachFileHis
            {
                OrgId = orgId,
                GuaranteeNo = no,
                FileIndex = f.FileIndex,
                GrtFilePath = f.GrtFilePath,
                GrtFileName = f.GrtFileName,
                FileSizeInBytes = f.FileSizeInBytes,
                GrtFileRemark = f.GrtFileRemark,
                LogLUBy = user,
                LogLUDateTime = now
            });
        }
        await db.SaveChangesAsync();

        return new GuaranteeAttachFileSaveResultDto
        {
            GuaranteeId = grt.Id,
            GuaranteeNo = no,
            TotalFiles = current.Count,
            AddedCount = toAdd.Count,
            DeletedCount = toDelete.Count,
            KeptCount = current.Count - toAdd.Count,
            TotalSizeInBytes = current.Sum(x => x.FileSizeInBytes),
            AddedFileNames = addedNames,
            DeletedFileNames = toDelete.Select(x => x.GrtFileName).ToList(),
            Files = current
        };
    }

    /// <summary>Xóa toàn bộ file đính kèm của 1 thư bảo lãnh (dùng khi hủy thư bảo lãnh).</summary>
    public async Task<int> DeleteAllAsync(Guid orgId, string guaranteeNo)
    {
        string no = (guaranteeNo ?? "").Trim();
        var files = await db.GuaranteeAttachFiles
            .Where(x => x.OrgId == orgId && x.GuaranteeNo == no)
            .ToListAsync();
        if (files.Count == 0) return 0;
        db.GuaranteeAttachFiles.RemoveRange(files);
        await db.SaveChangesAsync();
        return files.Count;
    }

    /// <summary>Báo cáo tổng hợp file đính kèm thư bảo lãnh theo ngân hàng.</summary>
    public async Task<GuaranteeAttachFileSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var files = await db.GuaranteeAttachFiles.Where(x => x.OrgId == orgId).ToListAsync();
        var guarantees = await db.Guarantees.Where(g => g.OrgId == orgId).ToListAsync();
        var historyCount = await db.GuaranteeAttachFileHis.CountAsync(x => x.OrgId == orgId);

        var bankByNo = guarantees
            .GroupBy(g => g.GuaranteeNo)
            .ToDictionary(g => g.Key, g => g.First().BankCode);

        var summary = new GuaranteeAttachFileSummaryDto
        {
            TotalGuaranteesWithFiles = files.Select(x => x.GuaranteeNo).Distinct().Count(),
            TotalFiles = files.Count,
            TotalSizeInBytes = files.Sum(x => x.FileSizeInBytes),
            HistoryRecords = historyCount
        };

        summary.ByBank = files
            .GroupBy(x => bankByNo.TryGetValue(x.GuaranteeNo, out var b) ? b : "N/A")
            .Select(g => new GuaranteeAttachFileBankStatDto
            {
                BankCode = g.Key,
                GuaranteeCount = g.Select(x => x.GuaranteeNo).Distinct().Count(),
                FileCount = g.Count(),
                TotalSizeInBytes = g.Sum(x => x.FileSizeInBytes)
            })
            .OrderByDescending(x => x.FileCount)
            .ThenBy(x => x.BankCode)
            .ToList();

        return summary;
    }
}
