using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hợp đồng Mua bán Xe Đại lý (Dealer Sales Contract).
/// Tương ứng DMS40_CT_DealerContract, DMS40_CT_DealerContractDetail, DlrCtr_PaymentType trong
/// TERP.BizHTC/DMS40/0.34.Contract.cs (DMS40_CT_DealerContract_Get / _Save / _DlrApprove /
/// _HTCApprove1 / _HTCApprove2 / _HTCReject / _DlrCancel) và màn hình FrmMngDC, FrmNewDC
/// trong TERP.HTCClient/Views/Sales/Contract hệ nguồn HTC 2010.
///
/// Nghiệp vụ: lập hợp đồng bán buôn xe cho đại lý kèm chỉ định ngân hàng bảo lãnh (BankCodeMD)
/// và loại thanh toán (DCPType), quy trình ký số 2 cấp:
///   Draft (DlrSignStatus=Pending, HTCSignStatus=Pending, DlrCtrStatus=NotSign)
///   → Đại lý ký (DlrSignStatus=Approved)
///   → HTC duyệt cấp 1 (HTCSignStatus=Approved1)
///   → HTC duyệt cấp 2 (HTCSignStatus=Approved2, DlrCtrStatus=Signed)
///   → Từ chối (HTCSignStatus=Rejected) / Hủy (DlrSignStatus=Cancelled, DlrCtrStatus=Cancelled).
/// </summary>
public sealed class DealerContractService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách hợp đồng đại lý kèm bộ lọc (số HĐ, đại lý, ngân hàng bảo lãnh, loại thanh toán,
    /// trạng thái ký đại lý/HTC, trạng thái hợp đồng, khoảng ngày, từ khóa).
    /// Tương ứng DMS40_CT_DealerContract_Get.
    /// </summary>
    public async Task<List<DealerContract>> GetListAsync(
        Guid orgId,
        string? dlrCtrNo = null,
        string? dealerCode = null,
        string? bankCodeMD = null,
        string? dcpType = null,
        string? dlrSignStatus = null,
        string? htcSignStatus = null,
        string? status = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.DealerContracts
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(dlrCtrNo))
        {
            string s = dlrCtrNo.Trim().ToUpperInvariant();
            q = q.Where(x => x.DlrCtrNo.ToUpper().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            string d = dealerCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.DealerCode.ToUpper().Contains(d));
        }

        if (!string.IsNullOrWhiteSpace(bankCodeMD))
        {
            string b = bankCodeMD.Trim().ToUpperInvariant();
            q = q.Where(x => x.BankCodeMD != null && x.BankCodeMD.ToUpper().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(dcpType) && Enum.TryParse<DlrCtrPaymentType>(dcpType, true, out var pt))
            q = q.Where(x => x.DCPType == pt);

        if (!string.IsNullOrWhiteSpace(dlrSignStatus) && Enum.TryParse<DlrSignStatus>(dlrSignStatus, true, out var ds))
            q = q.Where(x => x.DlrSignStatus == ds);

        if (!string.IsNullOrWhiteSpace(htcSignStatus) && Enum.TryParse<HTCSignStatus>(htcSignStatus, true, out var hs))
            q = q.Where(x => x.HTCSignStatus == hs);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DlrCtrStatus>(status, true, out var st))
            q = q.Where(x => x.DlrCtrStatus == st);

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.DlrCtrNo.ToUpper().Contains(s)
                || x.DealerCode.ToUpper().Contains(s)
                || (x.DealerName != null && x.DealerName.ToUpper().Contains(s))
                || (x.BankCodeMD != null && x.BankCodeMD.ToUpper().Contains(s))
                || (x.BankNameMD != null && x.BankNameMD.ToUpper().Contains(s)));
        }

        if (fromDate.HasValue)
        {
            var fd = fromDate.Value.Date;
            q = q.Where(x => x.ContractDate >= fd);
        }

        if (toDate.HasValue)
        {
            var td = toDate.Value.Date;
            q = q.Where(x => x.ContractDate <= td);
        }

        return await q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.DlrCtrNo).ToListAsync();
    }

    /// <summary>Lấy chi tiết 1 hợp đồng đại lý theo ID (kèm danh sách xe).</summary>
    public async Task<DealerContract?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết theo số hợp đồng (DlrCtrNo).</summary>
    public async Task<DealerContract?> GetByNoAsync(Guid orgId, string dlrCtrNo)
    {
        string no = dlrCtrNo.Trim();
        return await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.DlrCtrNo == no);
    }

    /// <summary>
    /// Lập hợp đồng đại lý mới (tương ứng DMS40_CT_DealerContract_Save).
    /// Tự sinh số hợp đồng nếu bỏ trống, kiểm tra trùng số, bắt buộc có ít nhất 1 dòng xe.
    /// Khởi tạo trạng thái Draft: DlrSignStatus=Pending, HTCSignStatus=Pending, DlrCtrStatus=NotSign.
    /// </summary>
    public async Task<DealerContract> CreateAsync(Guid orgId, CreateDealerContractDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.");
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Hợp đồng phải có ít nhất 1 dòng xe (DMS40_CT_DealerContractDetail).");

        string dlrCtrNo = string.IsNullOrWhiteSpace(dto.DlrCtrNo)
            ? $"PLHD-{DateTime.Now:yyyyMM}-{Random.Shared.Next(100, 999)}"
            : dto.DlrCtrNo.Trim();

        bool exists = await db.DealerContracts.AnyAsync(x => x.OrgId == orgId && x.DlrCtrNo == dlrCtrNo);
        if (exists)
            throw new InvalidOperationException($"Số hợp đồng đại lý {dlrCtrNo} đã tồn tại trong hệ thống.");

        var now = DateTime.Now;
        var entity = new DealerContract
        {
            OrgId = orgId,
            DlrCtrNo = dlrCtrNo,
            DCPType = dto.DCPType ?? DlrCtrPaymentType.BankGuarantee,
            DealerCode = dto.DealerCode.Trim().ToUpperInvariant(),
            DealerName = dto.DealerName?.Trim(),
            BankCodeMD = string.IsNullOrWhiteSpace(dto.BankCodeMD) ? null : dto.BankCodeMD.Trim().ToUpperInvariant(),
            BankNameMD = dto.BankNameMD?.Trim(),
            ContractDate = dto.ContractDate?.Date ?? DateTime.Today,
            FilePath = dto.FilePath?.Trim(),
            FlagDlrCtrAdjust = false,
            DlrSignStatus = DlrSignStatus.Pending,
            HTCSignStatus = HTCSignStatus.Pending,
            DlrCtrStatus = DlrCtrStatus.NotSign,
            Remark = dto.Remark?.Trim(),
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "Admin_HTC",
            CreatedAt = now
        };

        foreach (var item in dto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.CarId))
                throw new ArgumentException("Mỗi dòng xe phải có mã xe (CarId).");
            entity.Details.Add(new DealerContractDetail
            {
                OrgId = orgId,
                DlrCtrNo = dlrCtrNo,
                CarId = item.CarId.Trim(),
                VIN = item.VIN?.Trim() ?? "",
                ModelCode = item.ModelCode?.Trim(),
                ModelName = item.ModelName?.Trim(),
                SpecCode = item.SpecCode?.Trim(),
                ColorCode = item.ColorCode?.Trim(),
                ColorName = item.ColorName?.Trim(),
                OriginNo = item.OriginNo?.Trim(),
                ProductionYear = item.ProductionYear ?? 0,
                UnitPrice = item.UnitPrice,
                DlrCtrStatusDtl = DlrCtrDetailStatus.NotSign,
                Remark = item.Remark?.Trim()
            });
        }

        entity.TotalVehicles = entity.Details.Count;
        entity.TotalAmount = entity.Details.Sum(d => d.UnitPrice);

        db.DealerContracts.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Đại lý ký hợp đồng (tương ứng DMS40_CT_DealerContract_DlrApprove).
    /// Chỉ cho phép khi DlrSignStatus=Pending và DlrCtrStatus=NotSign.
    /// </summary>
    public async Task<DealerContract> DlrApproveAsync(Guid orgId, long id, ApproveDealerContractDto dto)
    {
        var entity = await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng đại lý #{id}.");

        if (entity.DlrSignStatus != DlrSignStatus.Pending || entity.DlrCtrStatus != DlrCtrStatus.NotSign)
            throw new InvalidOperationException(
                $"Hợp đồng {entity.DlrCtrNo} không ở trạng thái chờ đại lý ký (hiện tại: DlrSignStatus={entity.DlrSignStatus}, DlrCtrStatus={entity.DlrCtrStatus}).");

        var now = DateTime.Now;
        entity.DlrSignStatus = DlrSignStatus.Approved;
        entity.DlrApprovedBy = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "DaiLy";
        entity.DlrApprovedAt = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark.Trim();
        entity.UpdatedBy = entity.DlrApprovedBy;
        entity.UpdatedAt = now;

        foreach (var d in entity.Details)
            d.DlrCtrStatusDtl = DlrCtrDetailStatus.Signed;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// HTC duyệt cấp 1 (tương ứng DMS40_CT_DealerContract_HTCApprove1).
    /// Yêu cầu DlrSignStatus=Approved, HTCSignStatus=Pending, DlrCtrStatus=NotSign.
    /// </summary>
    public async Task<DealerContract> HTCApprove1Async(Guid orgId, long id, ApproveDealerContractDto dto)
    {
        var entity = await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng đại lý #{id}.");

        if (entity.DlrSignStatus != DlrSignStatus.Approved
            || entity.HTCSignStatus != HTCSignStatus.Pending
            || entity.DlrCtrStatus != DlrCtrStatus.NotSign)
            throw new InvalidOperationException(
                $"Hợp đồng {entity.DlrCtrNo} không ở trạng thái chờ HTC duyệt cấp 1 (hiện tại: DlrSignStatus={entity.DlrSignStatus}, HTCSignStatus={entity.HTCSignStatus}, DlrCtrStatus={entity.DlrCtrStatus}).");

        var now = DateTime.Now;
        entity.HTCSignStatus = HTCSignStatus.Approved1;
        entity.HTCApproved1By = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "HTC_Approver1";
        entity.HTCApproved1At = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark.Trim();
        entity.UpdatedBy = entity.HTCApproved1By;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// HTC duyệt cấp 2 — hoàn tất ký hợp đồng (tương ứng DMS40_CT_DealerContract_HTCApprove2).
    /// Yêu cầu DlrSignStatus=Approved, HTCSignStatus=Approved1, DlrCtrStatus=NotSign.
    /// Kết quả: HTCSignStatus=Approved2, DlrCtrStatus=Signed.
    /// </summary>
    public async Task<DealerContract> HTCApprove2Async(Guid orgId, long id, ApproveDealerContractDto dto)
    {
        var entity = await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng đại lý #{id}.");

        if (entity.DlrSignStatus != DlrSignStatus.Approved
            || entity.HTCSignStatus != HTCSignStatus.Approved1
            || entity.DlrCtrStatus != DlrCtrStatus.NotSign)
            throw new InvalidOperationException(
                $"Hợp đồng {entity.DlrCtrNo} không ở trạng thái chờ HTC duyệt cấp 2 (hiện tại: DlrSignStatus={entity.DlrSignStatus}, HTCSignStatus={entity.HTCSignStatus}, DlrCtrStatus={entity.DlrCtrStatus}).");

        var now = DateTime.Now;
        entity.HTCSignStatus = HTCSignStatus.Approved2;
        entity.DlrCtrStatus = DlrCtrStatus.Signed;
        entity.HTCApproved2By = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "HTC_Approver2";
        entity.HTCApproved2At = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) entity.Remark = dto.Remark.Trim();
        entity.UpdatedBy = entity.HTCApproved2By;
        entity.UpdatedAt = now;

        foreach (var d in entity.Details)
            d.DlrCtrStatusDtl = DlrCtrDetailStatus.Signed;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// HTC từ chối hợp đồng (tương ứng DMS40_CT_DealerContract_HTCReject).
    /// Yêu cầu DlrSignStatus=Approved và HTCSignStatus=Pending hoặc Approved1.
    /// </summary>
    public async Task<DealerContract> HTCRejectAsync(Guid orgId, long id, RejectDealerContractDto dto)
    {
        var entity = await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng đại lý #{id}.");

        if (entity.DlrSignStatus != DlrSignStatus.Approved
            || (entity.HTCSignStatus != HTCSignStatus.Pending && entity.HTCSignStatus != HTCSignStatus.Approved1))
            throw new InvalidOperationException(
                $"Hợp đồng {entity.DlrCtrNo} không ở trạng thái có thể từ chối (hiện tại: DlrSignStatus={entity.DlrSignStatus}, HTCSignStatus={entity.HTCSignStatus}).");

        var now = DateTime.Now;
        entity.HTCSignStatus = HTCSignStatus.Rejected;
        entity.RejectedBy = !string.IsNullOrWhiteSpace(dto.RejecterName) ? dto.RejecterName.Trim() : "HTC_Approver";
        entity.RejectedAt = now;
        entity.Remark = string.IsNullOrWhiteSpace(dto.Reason) ? entity.Remark : dto.Reason.Trim();
        entity.UpdatedBy = entity.RejectedBy;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Đại lý hủy hợp đồng (tương ứng DMS40_CT_DealerContract_DlrCancel).
    /// Chỉ cho phép khi hợp đồng chưa hoàn tất ký (DlrCtrStatus != Signed).
    /// </summary>
    public async Task<DealerContract> DlrCancelAsync(Guid orgId, long id, CancelDealerContractDto dto)
    {
        var entity = await db.DealerContracts
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng đại lý #{id}.");

        if (entity.DlrCtrStatus == DlrCtrStatus.Signed)
            throw new InvalidOperationException($"Hợp đồng {entity.DlrCtrNo} đã ký hoàn tất, không thể hủy.");

        var now = DateTime.Now;
        entity.DlrSignStatus = DlrSignStatus.Cancelled;
        entity.DlrCtrStatus = DlrCtrStatus.Cancelled;
        entity.CancelledBy = !string.IsNullOrWhiteSpace(dto.CancellerName) ? dto.CancellerName.Trim() : "DaiLy";
        entity.CancelledAt = now;
        if (!string.IsNullOrWhiteSpace(dto.Reason)) entity.Remark = dto.Reason.Trim();
        entity.UpdatedBy = entity.CancelledBy;
        entity.UpdatedAt = now;

        foreach (var d in entity.Details)
            d.DlrCtrStatusDtl = DlrCtrDetailStatus.Cancelled;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>Báo cáo tổng hợp hợp đồng đại lý theo trạng thái và theo ngân hàng bảo lãnh.</summary>
    public async Task<DealerContractSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.DealerContracts
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new DealerContractSummaryDto
        {
            TotalContracts = list.Count,
            DraftCount = list.Count(x => x.DlrSignStatus == DlrSignStatus.Pending && x.DlrCtrStatus == DlrCtrStatus.NotSign),
            DlrSignedCount = list.Count(x => x.DlrSignStatus == DlrSignStatus.Approved && x.HTCSignStatus == HTCSignStatus.Pending),
            HTCApproved1Count = list.Count(x => x.HTCSignStatus == HTCSignStatus.Approved1),
            SignedCount = list.Count(x => x.DlrCtrStatus == DlrCtrStatus.Signed),
            RejectedCount = list.Count(x => x.HTCSignStatus == HTCSignStatus.Rejected),
            CancelledCount = list.Count(x => x.DlrCtrStatus == DlrCtrStatus.Cancelled),
            TotalVehicles = list.Sum(x => x.TotalVehicles),
            TotalAmount = list.Sum(x => x.TotalAmount)
        };

        summary.ByBank = list
            .GroupBy(x => string.IsNullOrWhiteSpace(x.BankCodeMD) ? "(Chưa chỉ định)" : x.BankCodeMD!)
            .Select(g => new DealerContractBankStatDto
            {
                BankCode = g.Key,
                BankName = g.Select(x => x.BankNameMD).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                ContractCount = g.Count(),
                VehicleCount = g.Sum(x => x.TotalVehicles),
                TotalAmount = g.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.ContractCount)
            .ThenBy(x => x.BankCode)
            .ToList();

        return summary;
    }
}