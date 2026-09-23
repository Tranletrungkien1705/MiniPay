using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Danh mục Ngân hàng - Đại lý (Bank-Dealer authorization master data).
/// Tương ứng bảng Mst_BankDealer trong hệ nguồn 2010.HTC (Biz.HTC.WH.cs:
/// Mst_BankDealer_Get / Mst_BankDealer_Create / Mst_BankDealer_Update / Mst_BankDealer_Delete)
/// và màn hình FrmDealerBank trong TERP.HTCClient/Views/Admin/Product.
///
/// Nghiệp vụ: quản lý việc mỗi đại lý được phép dùng ngân hàng nào cho BẢO LÃNH (FlagBankGrt)
/// và THANH TOÁN (FlagBankPmt), kèm thông tin hợp đồng tín dụng (số HĐ, ngày HĐ, hạn mức)
/// và chi nhánh ngân hàng. Cặp (DealerCode, BankCode) là duy nhất.
/// </summary>
public sealed class BankDealerService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách Ngân hàng - Đại lý kèm bộ lọc (đại lý, ngân hàng, trạng thái, cờ bảo lãnh/thanh toán, từ khóa).
    /// Tương ứng Mst_BankDealer_Get.
    /// </summary>
    public async Task<List<BankDealer>> GetListAsync(
        Guid orgId,
        string? dealerCode = null,
        string? bankCode = null,
        string? status = null,
        bool? flagBankGrt = null,
        bool? flagBankPmt = null,
        string? query = null)
    {
        var q = db.BankDealers.Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            string d = dealerCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.DealerCode.ToUpper().Contains(d));
        }

        if (!string.IsNullOrWhiteSpace(bankCode))
        {
            string b = bankCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.BankCode.ToUpper().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BankDealerStatus>(status, true, out var st))
            q = q.Where(x => x.Status == st);

        if (flagBankGrt.HasValue)
            q = q.Where(x => x.FlagBankGrt == flagBankGrt.Value);

        if (flagBankPmt.HasValue)
            q = q.Where(x => x.FlagBankPmt == flagBankPmt.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.DealerCode.ToUpper().Contains(s)
                || (x.DealerName != null && x.DealerName.ToUpper().Contains(s))
                || x.BankCode.ToUpper().Contains(s)
                || (x.BankName != null && x.BankName.ToUpper().Contains(s))
                || (x.CreditContractNo != null && x.CreditContractNo.ToUpper().Contains(s))
                || (x.BankBranchName != null && x.BankBranchName.ToUpper().Contains(s)));
        }

        return await q.OrderBy(x => x.DealerCode).ThenBy(x => x.BankCode).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 dòng Ngân hàng - Đại lý theo ID.</summary>
    public async Task<BankDealer?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.BankDealers.FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết theo cặp (DealerCode, BankCode).</summary>
    public async Task<BankDealer?> GetByKeyAsync(Guid orgId, string dealerCode, string bankCode)
    {
        string d = dealerCode.Trim().ToUpperInvariant();
        string b = bankCode.Trim().ToUpperInvariant();
        return await db.BankDealers.FirstOrDefaultAsync(x => x.OrgId == orgId && x.DealerCode == d && x.BankCode == b);
    }

    /// <summary>
    /// Tạo mới 1 dòng Ngân hàng - Đại lý (tương ứng Mst_BankDealer_Create_20230922).
    /// Kiểm tra trùng cặp (DealerCode, BankCode) và bắt buộc có ít nhất 1 cờ bảo lãnh/thanh toán.
    /// </summary>
    public async Task<BankDealer> CreateAsync(Guid orgId, CreateBankDealerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.BankCode))
            throw new ArgumentException("Mã ngân hàng (BankCode) không được để trống.");
        if (!dto.FlagBankGrt && !dto.FlagBankPmt)
            throw new ArgumentException("Phải chọn ít nhất một quyền: Bảo lãnh (FlagBankGrt) hoặc Thanh toán (FlagBankPmt).");

        string dealerCode = dto.DealerCode.Trim().ToUpperInvariant();
        string bankCode = dto.BankCode.Trim().ToUpperInvariant();

        bool exists = await db.BankDealers.AnyAsync(x => x.OrgId == orgId && x.DealerCode == dealerCode && x.BankCode == bankCode);
        if (exists)
            throw new InvalidOperationException($"Đại lý {dealerCode} đã được gán ngân hàng {bankCode} trong hệ thống.");

        var now = DateTime.Now;
        var entity = new BankDealer
        {
            OrgId = orgId,
            DealerCode = dealerCode,
            DealerName = dto.DealerName?.Trim(),
            BankCode = bankCode,
            BankName = dto.BankName?.Trim(),
            CreditContractNo = dto.CreditContractNo?.Trim(),
            CreditContractDate = dto.CreditContractDate,
            CreditAmount = dto.CreditAmount,
            BankBranchCode = dto.BankBranchCode?.Trim(),
            BankBranchName = dto.BankBranchName?.Trim(),
            FlagBankGrt = dto.FlagBankGrt,
            FlagBankPmt = dto.FlagBankPmt,
            Status = BankDealerStatus.Active,
            Remark = dto.Remark?.Trim(),
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Admin_HTC",
            CreatedAt = now
        };

        db.BankDealers.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Cập nhật 1 dòng Ngân hàng - Đại lý (tương ứng Mst_BankDealer_Update).
    /// Chỉ cập nhật các trường được cung cấp (khác null).
    /// </summary>
    public async Task<BankDealer> UpdateAsync(Guid orgId, long id, UpdateBankDealerDto dto)
    {
        var entity = await db.BankDealers.FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy dòng Ngân hàng - Đại lý #{id}.");

        if (dto.DealerName != null) entity.DealerName = dto.DealerName.Trim();
        if (dto.BankName != null) entity.BankName = dto.BankName.Trim();
        if (dto.CreditContractNo != null) entity.CreditContractNo = dto.CreditContractNo.Trim();
        if (dto.CreditContractDate.HasValue) entity.CreditContractDate = dto.CreditContractDate;
        if (dto.CreditAmount.HasValue) entity.CreditAmount = dto.CreditAmount;
        if (dto.BankBranchCode != null) entity.BankBranchCode = dto.BankBranchCode.Trim();
        if (dto.BankBranchName != null) entity.BankBranchName = dto.BankBranchName.Trim();
        if (dto.FlagBankGrt.HasValue) entity.FlagBankGrt = dto.FlagBankGrt.Value;
        if (dto.FlagBankPmt.HasValue) entity.FlagBankPmt = dto.FlagBankPmt.Value;
        if (dto.Status.HasValue) entity.Status = dto.Status.Value;
        if (dto.Remark != null) entity.Remark = dto.Remark.Trim();

        if (!entity.FlagBankGrt && !entity.FlagBankPmt)
            throw new ArgumentException("Phải giữ ít nhất một quyền: Bảo lãnh (FlagBankGrt) hoặc Thanh toán (FlagBankPmt).");

        entity.UpdatedBy = !string.IsNullOrWhiteSpace(dto.UpdatedBy) ? dto.UpdatedBy.Trim() : "Admin_HTC";
        entity.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>Xóa 1 dòng Ngân hàng - Đại lý (tương ứng Mst_BankDealer_Delete).</summary>
    public async Task<bool> DeleteAsync(Guid orgId, long id)
    {
        var entity = await db.BankDealers.FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
        if (entity == null) return false;

        db.BankDealers.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Báo cáo tổng hợp danh mục Ngân hàng - Đại lý theo ngân hàng.</summary>
    public async Task<BankDealerSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.BankDealers.Where(x => x.OrgId == orgId).ToListAsync();

        var summary = new BankDealerSummaryDto
        {
            TotalRecords = list.Count,
            ActiveCount = list.Count(x => x.Status == BankDealerStatus.Active),
            InactiveCount = list.Count(x => x.Status == BankDealerStatus.Inactive),
            GrtEnabledCount = list.Count(x => x.FlagBankGrt),
            PmtEnabledCount = list.Count(x => x.FlagBankPmt),
            DealerCount = list.Select(x => x.DealerCode).Distinct().Count(),
            BankCount = list.Select(x => x.BankCode).Distinct().Count(),
            TotalCreditAmount = list.Sum(x => x.CreditAmount ?? 0)
        };

        summary.ByBank = list
            .GroupBy(x => x.BankCode)
            .Select(g => new BankDealerBankStatDto
            {
                BankCode = g.Key,
                BankName = g.Select(x => x.BankName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                DealerCount = g.Select(x => x.DealerCode).Distinct().Count(),
                GrtEnabledCount = g.Count(x => x.FlagBankGrt),
                PmtEnabledCount = g.Count(x => x.FlagBankPmt),
                TotalCreditAmount = g.Sum(x => x.CreditAmount ?? 0)
            })
            .OrderByDescending(x => x.DealerCount)
            .ThenBy(x => x.BankCode)
            .ToList();

        return summary;
    }
}
