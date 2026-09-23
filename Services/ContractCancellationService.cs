using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Biên Bản Thỏa Thuận Hủy Hợp Đồng Mua Bán Xe Ô Tô & Quyết Toán Nghĩa Vụ Tài Chính.
/// Tương ứng module Dlr_ContractCancel trong BizHTC.Contract / DMS40.Contract /
/// FrmDMS40_DlrCtr_CancelMinutes, FrmMngDMS40_DlrCtr_CancelMinutesHtc, FrmMngDMS40_DlrCtr_CancelMinutesDealer hệ nguồn HTC 2010.
/// </summary>
public sealed class ContractCancellationService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách biên bản thỏa thuận hủy hợp đồng & quyết toán tài chính.
    /// </summary>
    public async Task<List<ContractCancellation>> GetListAsync(
        Guid orgId,
        string? status = null,
        string? dealer = null,
        string? settlementType = null,
        string? query = null)
    {
        var q = db.ContractCancellations
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContractCancelStatus>(status, true, out var st))
        {
            q = q.Where(x => x.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(dealer))
        {
            string d = dealer.Trim().ToUpperInvariant();
            q = q.Where(x => x.DealerCode.Contains(d) || x.DealerName.ToUpper().Contains(d));
        }

        if (!string.IsNullOrWhiteSpace(settlementType) && Enum.TryParse<ContractCancelSettlementType>(settlementType, true, out var stype))
        {
            q = q.Where(x => x.SettlementType == stype);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.ContractCancelNo.Contains(s) || x.DlrContractNo.Contains(s) || x.CancelReason.ToUpper().Contains(s));
        }

        return await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết biên bản thỏa thuận hủy theo ID.
    /// </summary>
    public async Task<ContractCancellation?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>
    /// Lấy chi tiết biên bản thỏa thuận hủy theo Mã biên bản.
    /// </summary>
    public async Task<ContractCancellation?> GetByNoAsync(Guid orgId, string cancelNo)
    {
        string no = cancelNo.Trim().ToUpperInvariant();
        return await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.ContractCancelNo == no);
    }

    /// <summary>
    /// Lập biên bản thỏa thuận hủy hợp đồng & phương án quyết toán tài chính (tương ứng Dlr_ContractCancel_Save).
    /// </summary>
    public async Task<ContractCancellation> CreateAsync(Guid orgId, CreateContractCancelDto dto, string? createdBy = "SalesContractOfficer")
    {
        if (string.IsNullOrWhiteSpace(dto.DlrContractNo))
            throw new ArgumentException("Số phụ lục hợp đồng mua bán xe (DlrContractNo) không được để trống.", nameof(dto.DlrContractNo));

        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.", nameof(dto.DealerCode));

        if (string.IsNullOrWhiteSpace(dto.CancelReason))
            throw new ArgumentException("Lý do thỏa thuận hủy hợp đồng không được để trống.", nameof(dto.CancelReason));

        string dlrCtrNo = dto.DlrContractNo.Trim().ToUpperInvariant();
        string dlrCode = dto.DealerCode.Trim().ToUpperInvariant();

        // Kiểm tra hợp đồng đã có biên bản hủy nào đang chờ duyệt hay không
        bool hasActiveCancel = await db.ContractCancellations.AnyAsync(x =>
            x.OrgId == orgId &&
            x.DlrContractNo == dlrCtrNo &&
            (x.Status == ContractCancelStatus.Draft || x.Status == ContractCancelStatus.Submitted || x.Status == ContractCancelStatus.Approved));

        if (hasActiveCancel)
            throw new InvalidOperationException($"Hợp đồng '{dlrCtrNo}' đang có biên bản hủy chờ xử lý hoặc đã duyệt. Không thể tạo thêm.");

        // Sinh số biên bản: DCC-yyyyMM-xxx (tương ứng ContractCNo trong HTC)
        string finalCancelNo;
        if (!string.IsNullOrWhiteSpace(dto.ContractCancelNo))
        {
            finalCancelNo = dto.ContractCancelNo.Trim().ToUpperInvariant();
            if (await db.ContractCancellations.AnyAsync(x => x.OrgId == orgId && x.ContractCancelNo == finalCancelNo))
                throw new InvalidOperationException($"Số biên bản hủy '{finalCancelNo}' đã tồn tại trong hệ thống.");
        }
        else
        {
            string monthPrefix = $"DCC-{DateTime.Now:yyyyMM}-";
            int seq = await db.ContractCancellations
                .Where(x => x.OrgId == orgId && x.ContractCancelNo.StartsWith(monthPrefix))
                .CountAsync() + 1;
            finalCancelNo = $"{monthPrefix}{seq:D3}";
        }

        string? bankCode = string.IsNullOrWhiteSpace(dto.BankCode) ? null : dto.BankCode.Trim().ToUpperInvariant();
        string? bankName = string.IsNullOrWhiteSpace(dto.BankName) ? (bankCode != null ? GetBankNameByCode(bankCode) : null) : dto.BankName.Trim();

        var entity = new ContractCancellation
        {
            OrgId = orgId,
            ContractCancelNo = finalCancelNo,
            DlrContractNo = dlrCtrNo,
            DealerCode = dlrCode,
            DealerName = string.IsNullOrWhiteSpace(dto.DealerName) ? GetDealerNameByCode(dlrCode) : dto.DealerName.Trim(),
            CancelDate = dto.CancelDate ?? DateTime.Today,
            SettlementType = dto.SettlementType,
            BankCode = bankCode,
            BankName = bankName,
            BankGuaranteeNo = dto.BankGuaranteeNo?.Trim().ToUpperInvariant(),
            TransferContractNo = dto.TransferContractNo?.Trim().ToUpperInvariant(),
            CancelReason = dto.CancelReason.Trim(),
            Remark = dto.Remark?.Trim(),
            Status = ContractCancelStatus.Draft,
            CreatedBy = createdBy ?? "SalesAdmin",
            CreatedAt = DateTime.Now
        };

        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.VIN)) continue;

                long uPrice = item.UnitPrice > 0 ? item.UnitPrice : 820_000_000;
                long depPaid = item.DepositPaid ?? (long)(uPrice * 0.15); // mặc định cọc 15%
                long refAmt = item.RefundAmount ?? (dto.SettlementType == ContractCancelSettlementType.RefundDeposit ? depPaid : 0);
                long penAmt = item.PenaltyAmount ?? (dto.SettlementType == ContractCancelSettlementType.ForfeitDeposit ? depPaid : 0);
                long grtAmt = item.GuaranteeAmount ?? (dto.SettlementType == ContractCancelSettlementType.ReleaseGuarantee ? (long)(uPrice * 0.85) : 0);

                entity.Details.Add(new ContractCancelDetail
                {
                    OrgId = orgId,
                    ContractCancelNo = finalCancelNo,
                    VIN = item.VIN.Trim().ToUpperInvariant(),
                    CarId = item.CarId?.Trim(),
                    ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                    ModelName = string.IsNullOrWhiteSpace(item.ModelName) ? GetModelNameByCode(item.ModelCode) : item.ModelName.Trim(),
                    SpecCode = item.SpecCode?.Trim(),
                    ColorCode = item.ColorCode?.Trim() ?? "TRANG",
                    UnitPrice = uPrice,
                    DepositPaid = depPaid,
                    RefundAmount = refAmt,
                    PenaltyAmount = penAmt,
                    GuaranteeAmount = grtAmt,
                    TransferContractNo = item.TransferContractNo?.Trim().ToUpperInvariant() ?? entity.TransferContractNo,
                    Status = ContractCancelDetailStatus.Pending,
                    Remark = item.Remark?.Trim()
                });
            }
        }

        RecalculateTotals(entity);

        db.ContractCancellations.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// Cập nhật thông tin biên bản hủy khi còn ở trạng thái Draft.
    /// </summary>
    public async Task<ContractCancellation> UpdateAsync(Guid orgId, long id, UpdateContractCancelDto dto)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể sửa biên bản hủy khi ở trạng thái Draft (hiện tại: {entity.Status}).");

        if (dto.SettlementType.HasValue) entity.SettlementType = dto.SettlementType.Value;
        if (!string.IsNullOrWhiteSpace(dto.BankCode))
        {
            entity.BankCode = dto.BankCode.Trim().ToUpperInvariant();
            entity.BankName = string.IsNullOrWhiteSpace(dto.BankName) ? GetBankNameByCode(entity.BankCode) : dto.BankName.Trim();
        }
        if (dto.BankGuaranteeNo != null) entity.BankGuaranteeNo = dto.BankGuaranteeNo.Trim().ToUpperInvariant();
        if (dto.TransferContractNo != null) entity.TransferContractNo = dto.TransferContractNo.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.CancelReason)) entity.CancelReason = dto.CancelReason.Trim();
        if (dto.Remark != null) entity.Remark = dto.Remark.Trim();

        RecalculateTotals(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Thêm một xe ô tô vào biên bản hủy ở trạng thái Draft.
    /// </summary>
    public async Task<ContractCancellation> AddDetailAsync(Guid orgId, long id, ContractCancelItemInputDto item)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể thêm xe khi biên bản hủy ở trạng thái Draft.");

        string vin = item.VIN.Trim().ToUpperInvariant();
        if (entity.Details.Any(d => d.VIN == vin))
            throw new InvalidOperationException($"Số khung VIN '{vin}' đã có trong biên bản hủy này.");

        long uPrice = item.UnitPrice > 0 ? item.UnitPrice : 820_000_000;
        long depPaid = item.DepositPaid ?? (long)(uPrice * 0.15);
        long refAmt = item.RefundAmount ?? (entity.SettlementType == ContractCancelSettlementType.RefundDeposit ? depPaid : 0);
        long penAmt = item.PenaltyAmount ?? (entity.SettlementType == ContractCancelSettlementType.ForfeitDeposit ? depPaid : 0);
        long grtAmt = item.GuaranteeAmount ?? (entity.SettlementType == ContractCancelSettlementType.ReleaseGuarantee ? (long)(uPrice * 0.85) : 0);

        entity.Details.Add(new ContractCancelDetail
        {
            OrgId = orgId,
            ContractCancelNo = entity.ContractCancelNo,
            VIN = vin,
            CarId = item.CarId?.Trim(),
            ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
            ModelName = string.IsNullOrWhiteSpace(item.ModelName) ? GetModelNameByCode(item.ModelCode) : item.ModelName.Trim(),
            SpecCode = item.SpecCode?.Trim(),
            ColorCode = item.ColorCode?.Trim() ?? "TRANG",
            UnitPrice = uPrice,
            DepositPaid = depPaid,
            RefundAmount = refAmt,
            PenaltyAmount = penAmt,
            GuaranteeAmount = grtAmt,
            TransferContractNo = item.TransferContractNo?.Trim().ToUpperInvariant() ?? entity.TransferContractNo,
            Status = ContractCancelDetailStatus.Pending,
            Remark = item.Remark?.Trim()
        });

        RecalculateTotals(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Xóa một xe khỏi biên bản hủy ở trạng thái Draft.
    /// </summary>
    public async Task<ContractCancellation> RemoveDetailAsync(Guid orgId, long id, long detailId)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa xe khi biên bản hủy ở trạng thái Draft.");

        var detail = entity.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new InvalidOperationException($"Không tìm thấy dòng xe chi tiết ID {detailId}.");

        db.ContractCancelDetails.Remove(detail);
        entity.Details.Remove(detail);

        RecalculateTotals(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Xóa biên bản hủy ở trạng thái Draft.
    /// </summary>
    public async Task<bool> DeleteDraftAsync(Guid orgId, long id)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa biên bản khi ở trạng thái Draft (hiện tại: {entity.Status}).");

        db.ContractCancellations.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Trình Ban Quản lý Đại lý & Tài chính HTC thẩm định biên bản thỏa thuận hủy (Draft -> Submitted).
    /// </summary>
    public async Task<ContractCancellation> SubmitAsync(Guid orgId, long id, SubmitContractCancelDto dto)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Biên bản phải ở trạng thái Draft mới có thể trình duyệt (hiện tại: {entity.Status}).");

        if (entity.Details.Count == 0)
            throw new InvalidOperationException("Biên bản hủy chưa có danh sách xe ô tô. Vui lòng thêm ít nhất 1 xe.");

        entity.Status = ContractCancelStatus.Submitted;
        entity.Remark = string.IsNullOrWhiteSpace(entity.Remark)
            ? $"Trình thẩm định phương án tài chính bởi {dto.SubmitterName ?? "SalesOfficer"}."
            : $"{entity.Remark} | Trình thẩm định bởi {dto.SubmitterName ?? "SalesOfficer"} ({DateTime.Now:dd/MM/yyyy HH:mm}).";

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Lãnh đạo HTC phê duyệt chấp thuận biên bản hủy & phương án quyết toán tài chính (tương ứng Dlr_ContractCancel_ApproveMulti).
    /// </summary>
    public async Task<ContractCancellation> ApproveAsync(Guid orgId, long id, ApproveContractCancelDto dto)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Submitted && entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Biên bản không ở trạng thái hợp lệ để phê duyệt (hiện tại: {entity.Status}).");

        entity.Status = ContractCancelStatus.Approved;
        entity.ApprovedBy = dto.ApproverName ?? "BanGiamDoc_HTC";
        entity.ApprovedAt = DateTime.Now;

        foreach (var d in entity.Details)
        {
            d.Status = ContractCancelDetailStatus.Approved;
        }

        if (!string.IsNullOrWhiteSpace(dto.Remark))
        {
            entity.Remark = string.IsNullOrWhiteSpace(entity.Remark)
                ? dto.Remark.Trim()
                : $"{entity.Remark} | Phê duyệt: {dto.Remark.Trim()}";
        }

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Kế toán hoàn tất quyết toán tài chính: Chi trả UNC hoàn cọc, ghi nhận tịch thu phạt cọc, hoặc gửi giải tỏa bảo lãnh ngân hàng.
    /// </summary>
    public async Task<ContractCancellation> SettleAsync(Guid orgId, long id, SettleContractCancelDto dto)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Approved)
            throw new InvalidOperationException($"Biên bản phải được Lãnh đạo duyệt (Approved) trước khi quyết toán tài chính (hiện tại: {entity.Status}).");

        entity.Status = ContractCancelStatus.Settled;
        entity.SettledBy = dto.SettlerName ?? "KeToanThanhToan_HTC";
        entity.SettledAt = DateTime.Now;

        // Sinh mã giao dịch UNC hoặc mã giải tỏa ngân hàng nếu chưa có
        if (string.IsNullOrWhiteSpace(dto.BankTxnRef))
        {
            entity.BankTxnRef = entity.SettlementType switch
            {
                ContractCancelSettlementType.RefundDeposit => $"UNC-REF-{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}",
                ContractCancelSettlementType.ForfeitDeposit => $"PKT-FORFEIT-{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}",
                ContractCancelSettlementType.TransferDeposit => $"TRF-DEP-{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}",
                ContractCancelSettlementType.ReleaseGuarantee => $"REL-GRT-{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}",
                _ => $"SETTLE-{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}"
            };
        }
        else
        {
            entity.BankTxnRef = dto.BankTxnRef.Trim().ToUpperInvariant();
        }

        foreach (var d in entity.Details)
        {
            d.Status = ContractCancelDetailStatus.Settled;
        }

        if (!string.IsNullOrWhiteSpace(dto.Remark))
        {
            entity.Remark = string.IsNullOrWhiteSpace(entity.Remark)
                ? dto.Remark.Trim()
                : $"{entity.Remark} | Quyết toán: {dto.Remark.Trim()}";
        }

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Từ chối đề nghị thỏa thuận hủy hợp đồng của đại lý.
    /// </summary>
    public async Task<ContractCancellation> RejectAsync(Guid orgId, long id, RejectContractCancelDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do từ chối không được để trống.", nameof(dto.Reason));

        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status != ContractCancelStatus.Submitted && entity.Status != ContractCancelStatus.Draft)
            throw new InvalidOperationException($"Không thể từ chối biên bản ở trạng thái {entity.Status}.");

        entity.Status = ContractCancelStatus.Rejected;
        entity.RejectReason = dto.Reason.Trim();
        entity.Remark = string.IsNullOrWhiteSpace(entity.Remark)
            ? $"Từ chối bởi {dto.RejecterName ?? "CapThamDinh"}: {dto.Reason.Trim()}"
            : $"{entity.Remark} | Từ chối: {dto.Reason.Trim()}";

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Hủy biên bản thỏa thuận (tương ứng Dlr_ContractCancel_CancelMulti).
    /// </summary>
    public async Task<ContractCancellation> CancelAsync(Guid orgId, long id, CancelContractCancelDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy biên bản không được để trống.", nameof(dto.Reason));

        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        if (entity.Status == ContractCancelStatus.Settled)
            throw new InvalidOperationException("Biên bản đã hoàn tất quyết toán tài chính (Settled). Không thể hủy.");

        entity.Status = ContractCancelStatus.Cancelled;
        entity.CancelReasonText = dto.Reason.Trim();
        entity.Remark = string.IsNullOrWhiteSpace(entity.Remark)
            ? $"Hủy biên bản bởi {dto.CancellerName ?? "Admin"}: {dto.Reason.Trim()}"
            : $"{entity.Remark} | Hủy biên bản: {dto.Reason.Trim()}";

        foreach (var d in entity.Details)
        {
            d.Status = ContractCancelDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Báo cáo tổng hợp số liệu biên bản thỏa thuận hủy & quyết toán tài chính theo kỳ.
    /// </summary>
    public async Task<ContractCancelSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.ContractCancellations
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new ContractCancelSummaryDto
        {
            TotalAgreements = list.Count,
            DraftCount = list.Count(x => x.Status == ContractCancelStatus.Draft),
            SubmittedCount = list.Count(x => x.Status == ContractCancelStatus.Submitted),
            ApprovedCount = list.Count(x => x.Status == ContractCancelStatus.Approved),
            SettledCount = list.Count(x => x.Status == ContractCancelStatus.Settled),
            RejectedCount = list.Count(x => x.Status == ContractCancelStatus.Rejected),
            CancelledCount = list.Count(x => x.Status == ContractCancelStatus.Cancelled),
            TotalVehiclesCancelled = list.Sum(x => x.TotalVehicles),
            TotalContractAmount = list.Sum(x => x.TotalContractAmount),
            TotalDepositPaid = list.Sum(x => x.TotalDepositPaid),
            TotalRefundAmount = list.Sum(x => x.TotalRefundAmount),
            TotalPenaltyAmount = list.Sum(x => x.TotalPenaltyAmount),
            TotalGuaranteeRelease = list.Sum(x => x.TotalGuaranteeRelease),
        };

        summary.TopDealers = list
            .GroupBy(x => new { x.DealerCode, x.DealerName })
            .Select(g => new DealerCancelStatDto
            {
                DealerCode = g.Key.DealerCode,
                DealerName = g.Key.DealerName,
                AgreementCount = g.Count(),
                VehicleCount = g.Sum(x => x.TotalVehicles),
                TotalRefundAmount = g.Sum(x => x.TotalRefundAmount),
                TotalPenaltyAmount = g.Sum(x => x.TotalPenaltyAmount),
                TotalGuaranteeRelease = g.Sum(x => x.TotalGuaranteeRelease)
            })
            .OrderByDescending(x => x.AgreementCount)
            .Take(5)
            .ToList();

        return summary;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Biên Bản Thỏa Thuận Hủy Hợp Đồng & Thanh Quyết Toán Nghĩa Vụ Tài Chính.
    /// Tương ứng mẫu in văn bản FrmDMS40_DlrCtr_CancelMinutes trong hệ nguồn HTC.
    /// </summary>
    public async Task<ContractCancelAdviceDto> GenerateAdviceAsync(Guid orgId, long id)
    {
        var entity = await db.ContractCancellations
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy biên bản hủy ID {id}.");

        string settleText = entity.SettlementType switch
        {
            ContractCancelSettlementType.RefundDeposit => "Hoàn trả 100% tiền đặt cọc qua chuyển khoản UNC ngân hàng",
            ContractCancelSettlementType.ForfeitDeposit => "Tịch thu tiền đặt cọc nộp phạt vi phạm hợp đồng đại lý",
            ContractCancelSettlementType.TransferDeposit => $"Điều chuyển tiền đặt cọc sang phụ lục HĐ mới ({entity.TransferContractNo})",
            ContractCancelSettlementType.ReleaseGuarantee => $"Giải phóng nghĩa vụ Thư bảo lãnh thanh toán ngân hàng ({entity.BankGuaranteeNo ?? entity.BankCode})",
            ContractCancelSettlementType.MixedSettlement => "Thanh quyết toán tài chính hỗn hợp (Hoàn cọc + Phạt cọc + Giải tỏa bảo lãnh)",
            _ => "Thỏa thuận hủy hợp đồng"
        };

        string statusText = entity.Status switch
        {
            ContractCancelStatus.Draft => "Dự thảo biên bản (Draft)",
            ContractCancelStatus.Submitted => "Đã trình thẩm định (Submitted)",
            ContractCancelStatus.Approved => "Lãnh đạo HTC đã phê duyệt (Approved)",
            ContractCancelStatus.Settled => "Đã hoàn tất quyết toán tài chính (Settled)",
            ContractCancelStatus.Rejected => "Bị từ chối thỏa thuận (Rejected)",
            ContractCancelStatus.Cancelled => "Đã hủy biên bản (Cancelled)",
            _ => entity.Status.ToString()
        };

        var advice = new ContractCancelAdviceDto
        {
            ContractCancelNo = entity.ContractCancelNo,
            DlrContractNo = entity.DlrContractNo,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            CancelDate = entity.CancelDate.ToString("dd/MM/yyyy"),
            DealerCode = entity.DealerCode,
            DealerName = entity.DealerName,
            SettlementTypeText = settleText,
            BankCode = entity.BankCode,
            BankName = entity.BankName,
            BankGuaranteeNo = entity.BankGuaranteeNo,
            TransferContractNo = entity.TransferContractNo,
            TotalVehicles = entity.TotalVehicles,
            TotalContractAmount = entity.TotalContractAmount,
            TotalDepositPaid = entity.TotalDepositPaid,
            TotalRefundAmount = entity.TotalRefundAmount,
            TotalRefundAmountInWords = ReadVietnameseMoneyNumber(entity.TotalRefundAmount),
            TotalPenaltyAmount = entity.TotalPenaltyAmount,
            TotalPenaltyAmountInWords = ReadVietnameseMoneyNumber(entity.TotalPenaltyAmount),
            TotalGuaranteeRelease = entity.TotalGuaranteeRelease,
            TotalGuaranteeReleaseInWords = ReadVietnameseMoneyNumber(entity.TotalGuaranteeRelease),
            StatusText = statusText,
            CancelReason = entity.CancelReason,
            BankTxnRef = entity.BankTxnRef,
            CreatedBy = entity.CreatedBy,
            ApprovedBy = entity.ApprovedBy,
            ApprovedAt = entity.ApprovedAt?.ToString("dd/MM/yyyy HH:mm"),
            SettledBy = entity.SettledBy,
            SettledAt = entity.SettledAt?.ToString("dd/MM/yyyy HH:mm"),
            Remark = entity.Remark
        };

        int idx = 1;
        foreach (var d in entity.Details.OrderBy(x => x.Id))
        {
            advice.Items.Add(new ContractCancelDetailAdviceDto
            {
                No = idx++,
                VIN = d.VIN,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? GetModelNameByCode(d.ModelCode),
                SpecCode = d.SpecCode,
                ColorCode = d.ColorCode,
                UnitPrice = d.UnitPrice,
                DepositPaid = d.DepositPaid,
                RefundAmount = d.RefundAmount,
                PenaltyAmount = d.PenaltyAmount,
                GuaranteeAmount = d.GuaranteeAmount,
                TransferContractNo = d.TransferContractNo,
                Status = d.Status.ToString(),
                Remark = d.Remark
            });
        }

        return advice;
    }

    /// <summary>
    /// Tự động tái tính toán tổng số lượng và các khoản tiền tài chính của biên bản thỏa thuận hủy.
    /// </summary>
    private static void RecalculateTotals(ContractCancellation entity)
    {
        entity.TotalVehicles = entity.Details.Count;
        entity.TotalContractAmount = entity.Details.Sum(d => d.UnitPrice);
        entity.TotalDepositPaid = entity.Details.Sum(d => d.DepositPaid);
        entity.TotalRefundAmount = entity.Details.Sum(d => d.RefundAmount);
        entity.TotalPenaltyAmount = entity.Details.Sum(d => d.PenaltyAmount);
        entity.TotalGuaranteeRelease = entity.Details.Sum(d => d.GuaranteeAmount);
    }

    public static string GetDealerNameByCode(string code) => code.ToUpperInvariant() switch
    {
        "VN001" => "Công ty CP Ô tô Hyundai Hà Nội",
        "VN002" => "Công ty TNHH Ô tô Hyundai Tây Hồ",
        "VN005" => "Công ty CP Thương mại Dịch vụ Hyundai Đông Anh",
        "VN012" => "Công ty CP Ô tô Hyundai Nam Định",
        "VN018" => "Công ty TNHH Hyundai Hải Phòng",
        "VN034" => "Công ty CP Dịch vụ Tổng hợp Hyundai Đà Nẵng",
        "VN040" => "Công ty CP Ô tô Hyundai Việt Hàn (Sài Gòn)",
        "VN041" => "Công ty TNHH MTV Hyundai Miền Đông",
        "VN054" => "Công ty CP Hyundai Cần Thơ",
        _ => $"Đại lý Hyundai Ủy quyền ({code})"
    };

    public static string GetBankNameByCode(string code) => code.ToUpperInvariant() switch
    {
        "CTG" or "VIETINBANK" => "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
        "MBB" or "MBBANK" => "Ngân hàng TMCP Quân Đội (MBBank)",
        "VCB" or "VIETCOMBANK" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
        "TCB" or "TECHCOMBANK" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
        "VPB" or "VPBANK" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
        "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
        "VIB" => "Ngân hàng TMCP Quốc Tế Việt Nam (VIB)",
        _ => $"Ngân hàng đối tác ({code})"
    };

    public static string GetModelNameByCode(string code) => code.ToUpperInvariant() switch
    {
        "SANTAFE" => "Hyundai Santa Fe (SUV 7 chỗ)",
        "TUCSON" => "Hyundai Tucson (C-SUV 5 chỗ)",
        "CRETA" => "Hyundai Creta (B-SUV Đô thị)",
        "ACCENT" => "Hyundai Accent (Sedan B)",
        "ELANTRA" => "Hyundai Elantra (Sedan C)",
        "PALISADE" => "Hyundai Palisade (SUV Flagship)",
        "CUSTIN" => "Hyundai Custin (MPV Trung cấp)",
        "IONIQ5" => "Hyundai Ioniq 5 (Xe điện EV)",
        "STARGAZER" => "Hyundai Stargazer (MPV 7 chỗ)",
        "STAREX" or "SOLATI" => "Hyundai Solati (Mini Bus 16 chỗ)",
        "PORTER150" => "Hyundai New Porter 150 (Tải nhẹ 1.5 tấn)",
        "MIGHTY_EX8" => "Hyundai Mighty EX8 GTL (Tải trung)",
        _ => $"Dòng xe Hyundai ({code})"
    };

    /// <summary>
    /// Thuật toán đọc số tiền tiền tệ VND thành chữ tiếng Việt chuẩn quy chuẩn tài chính ngân hàng.
    /// </summary>
    public static string ReadVietnameseMoneyNumber(long amount)
    {
        if (amount == 0) return "Không đồng";
        if (amount < 0) return "Âm " + ReadVietnameseMoneyNumber(-amount);

        string[] units = ["", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ"];
        string result = "";
        int unitIndex = 0;

        long temp = amount;
        while (temp > 0)
        {
            int block = (int)(temp % 1000);
            if (block > 0)
            {
                string blockText = ReadThreeDigits(block, temp >= 1000);
                result = blockText + units[unitIndex] + (string.IsNullOrWhiteSpace(result) ? "" : " ") + result;
            }
            temp /= 1000;
            unitIndex++;
        }

        result = result.Trim();
        if (result.Length > 0)
        {
            result = char.ToUpper(result[0]) + result[1..] + " đồng chẵn./.";
        }
        return result;
    }

    private static string ReadThreeDigits(int number, bool hasHigherBlock)
    {
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
        int hundreds = number / 100;
        int tens = (number % 100) / 10;
        int ones = number % 10;
        string res = "";

        if (hundreds > 0 || hasHigherBlock)
        {
            res += digits[hundreds] + " trăm";
        }

        if (tens > 1)
        {
            res += (string.IsNullOrEmpty(res) ? "" : " ") + digits[tens] + " mươi";
            if (ones == 1) res += " mốt";
            else if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else if (tens == 1)
        {
            res += (string.IsNullOrEmpty(res) ? "" : " ") + "mười";
            if (ones == 1) res += " một";
            else if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else if (tens == 0 && ones > 0)
        {
            if (!string.IsNullOrEmpty(res)) res += " lẻ";
            res += " " + digits[ones];
        }

        return res.Trim();
    }
}
