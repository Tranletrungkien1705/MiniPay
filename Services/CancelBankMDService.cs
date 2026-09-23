using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hồ sơ Đề nghị Hủy Gán Ngân Hàng Bảo Lãnh Cho Hợp Đồng Xe Ô Tô.
/// Tương ứng module DMS40_DlrCtr_CancelBankMD trong BizHTC.Payment / DMS40.Contract /
/// FrmDMS40_DlrCtr_CancelBankMD, FrmMngDMS40_DlrCtr_CancelBankMDDealer, FrmMngDMS40_DlrCtr_CancelBankMDHtc, FrmMngBankDMS40_DlrCtr_CancelBankMD.
/// </summary>
public sealed class CancelBankMDService(AppDbContext db)
{
    /// <summary>
    /// Đại lý lập đề nghị hủy gán ngân hàng bảo lãnh cho hợp đồng mua xe (DMS40_DlrCtr_CancelBankMD_Save).
    /// </summary>
    public async Task<ContractBankMDCancel> CreateRequestAsync(Guid orgId, CreateCancelBankMDDto dto, string? createdBy = "DealerCreditOfficer")
    {
        if (string.IsNullOrWhiteSpace(dto.DlrCtrNo))
            throw new ArgumentException("Số phụ lục hợp đồng đại lý (DlrCtrNo) không được để trống.", nameof(dto.DlrCtrNo));

        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý (DealerCode) không được để trống.", nameof(dto.DealerCode));

        if (string.IsNullOrWhiteSpace(dto.BankCodeMD))
            throw new ArgumentException("Mã ngân hàng bảo lãnh cần hủy (BankCodeMD) không được để trống.", nameof(dto.BankCodeMD));

        // Kiểm tra xem hợp đồng đã có đề nghị hủy đang xử lý chưa (Pending hoặc Approved)
        bool hasActiveRequest = await db.CancelBankMDRequests.AnyAsync(r =>
            r.OrgId == orgId &&
            r.DlrCtrNo == dto.DlrCtrNo.Trim().ToUpperInvariant() &&
            (r.Status == CancelBankMDStatus.Pending || r.Status == CancelBankMDStatus.Approved));

        if (hasActiveRequest)
            throw new InvalidOperationException($"Phụ lục hợp đồng '{dto.DlrCtrNo}' đang có hồ sơ đề nghị hủy gán ngân hàng bảo lãnh chờ xử lý. Không thể tạo thêm.");

        // Sinh số đề nghị tự động: CANMD-yyyyMM-xxx (tương ứng CancelBankMDNo trong BizHTC)
        string monthPrefix = $"CANMD-{DateTime.Now:yyyyMM}-";
        int seq = await db.CancelBankMDRequests
            .Where(r => r.OrgId == orgId && r.CancelBankMDNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string finalReqNo = $"{monthPrefix}{seq:D3}";

        string dlrCode = dto.DealerCode.Trim().ToUpperInvariant();
        string bankCode = dto.BankCodeMD.Trim().ToUpperInvariant();
        string? newBankCode = string.IsNullOrWhiteSpace(dto.NewBankCodeMD) ? null : dto.NewBankCodeMD.Trim().ToUpperInvariant();

        var request = new ContractBankMDCancel
        {
            OrgId = orgId,
            CancelBankMDNo = finalReqNo,
            DlrCtrNo = dto.DlrCtrNo.Trim().ToUpperInvariant(),
            DealerCode = dlrCode,
            DealerName = string.IsNullOrWhiteSpace(dto.DealerName) ? GetDealerNameByCode(dlrCode) : dto.DealerName.Trim(),
            BankCodeMD = bankCode,
            BankNameMD = string.IsNullOrWhiteSpace(dto.BankNameMD) ? GetBankNameByCode(bankCode) : dto.BankNameMD.Trim(),
            NewBankCodeMD = newBankCode,
            NewBankNameMD = string.IsNullOrWhiteSpace(newBankCode) ? null : (dto.NewBankNameMD ?? GetBankNameByCode(newBankCode)),
            GuaranteeType = dto.GuaranteeType ?? GuaranteeType.Payment,
            ReasonType = dto.ReasonType ?? CancelBankMDReasonType.ChangeBank,
            ReasonDescription = dto.ReasonDescription?.Trim(),
            Status = CancelBankMDStatus.Pending,
            RemarkDlr = dto.RemarkDlr?.Trim() ?? "Đại lý đề nghị hủy gán ngân hàng bảo lãnh để chuyển sang phương án tài chính mới.",
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now
        };

        long totalContractAmt = 0;
        long totalGuaranteeAmt = 0;

        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.VIN)) continue;

                long uPrice = item.UnitPrice > 0 ? item.UnitPrice : 800_000_000;
                long grtAmt = item.GuaranteeAmount ?? uPrice;

                totalContractAmt += uPrice;
                totalGuaranteeAmt += grtAmt;

                request.Details.Add(new ContractBankMDCancelDetail
                {
                    OrgId = orgId,
                    CancelBankMDNo = finalReqNo,
                    VIN = item.VIN.Trim().ToUpperInvariant(),
                    CarId = item.CarId?.Trim(),
                    ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                    ModelName = string.IsNullOrWhiteSpace(item.ModelName) ? GetModelNameByCode(item.ModelCode) : item.ModelName.Trim(),
                    SpecCode = item.SpecCode?.Trim(),
                    SpecDescription = item.SpecDescription?.Trim(),
                    ColorExtNameVN = item.ColorExtNameVN?.Trim() ?? "Trắng",
                    EngineNo = item.EngineNo?.Trim(),
                    UnitPrice = uPrice,
                    GuaranteeAmount = grtAmt,
                    Status = CancelBankMDDetailStatus.Pending,
                    Remark = item.Remark?.Trim()
                });
            }
        }

        request.TotalVehicles = request.Details.Count;
        request.ContractAmount = totalContractAmt;
        request.GuaranteeAmount = totalGuaranteeAmt;

        db.CancelBankMDRequests.Add(request);
        await db.SaveChangesAsync();

        return request;
    }

    /// <summary>
    /// Danh sách hồ sơ đề nghị hủy gán ngân hàng bảo lãnh (DMS40_DlrCtr_CancelBankMD_GetX).
    /// </summary>
    public async Task<List<ContractBankMDCancel>> GetRequestsAsync(
        Guid orgId,
        string? dealerCode = null,
        string? bankCode = null,
        CancelBankMDStatus? status = null,
        CancelBankMDReasonType? reasonType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.CancelBankMDRequests
            .Include(r => r.Details)
            .Where(r => r.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(dealerCode))
            q = q.Where(r => r.DealerCode == dealerCode.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(bankCode))
            q = q.Where(r => r.BankCodeMD == bankCode.Trim().ToUpperInvariant());

        if (status.HasValue)
            q = q.Where(r => r.Status == status.Value);

        if (reasonType.HasValue)
            q = q.Where(r => r.ReasonType == reasonType.Value);

        if (fromDate.HasValue)
            q = q.Where(r => r.CreatedAt >= fromDate.Value.Date);

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(r => r.CreatedAt <= endOfDay);
        }

        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Xem chi tiết 1 hồ sơ đề nghị hủy gán ngân hàng bảo lãnh kèm danh sách xe.
    /// </summary>
    public async Task<ContractBankMDCancel?> GetRequestByIdAsync(long id, Guid orgId)
    {
        return await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);
    }

    /// <summary>
    /// Ngân hàng phát hành bảo lãnh phê duyệt chấp thuận hủy nghĩa vụ bảo lãnh (DMS40_DlrCtr_CancelBankMD_Approve).
    /// </summary>
    public async Task<ContractBankMDCancel> ApproveByBankAsync(long id, Guid orgId, ApproveCancelBankMDBankDto? dto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status != CancelBankMDStatus.Pending)
            throw new InvalidOperationException($"Hồ sơ đang ở trạng thái '{request.Status}', chỉ hồ sơ 'Pending' (Chờ xử lý) mới được Ngân hàng phê duyệt.");

        request.Status = CancelBankMDStatus.Approved;
        request.ApproveBy = dto?.ApproverName ?? "TruongPhongTinDung_NganHang";
        request.ApproveDateTime = DateTime.Now;
        request.RemarkBank = dto?.RemarkBank ?? "Ngân hàng xác nhận không phát sinh nghĩa vụ bảo lãnh và chấp thuận hủy gán bảo lãnh cho hợp đồng.";

        foreach (var d in request.Details)
        {
            if (d.Status == CancelBankMDDetailStatus.Pending)
            {
                d.Status = CancelBankMDDetailStatus.Approved;
            }
        }

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// HTC thẩm định và duyệt hoàn tất hủy gán ngân hàng bảo lãnh (DMS40_DlrCtr_CancelBankMD_Finish_New20181115).
    /// Tự động cập nhật gỡ bỏ BankCodeMD trên hợp đồng đại lý.
    /// </summary>
    public async Task<ContractBankMDCancel> FinishByHTCAsync(long id, Guid orgId, FinishCancelBankMDHTCDto? dto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status != CancelBankMDStatus.Approved)
            throw new InvalidOperationException($"Hồ sơ đang ở trạng thái '{request.Status}'. Yêu cầu Ngân hàng phải phê duyệt chấp thuận (Approved) trước khi HTC duyệt hoàn tất.");

        request.Status = CancelBankMDStatus.Finished;
        request.FinishBy = dto?.FinisherName ?? "TruongPhongQuanLyDaiLy_HTC";
        request.FinishDTime = DateTime.Now;
        request.RemarkHTC = dto?.RemarkHTC ?? "HTC phê duyệt hoàn tất hủy gán bảo lãnh hợp đồng. Đã cập nhật BankCodeMD = NULL và cho phép đại lý đăng ký phương thức thanh toán mới.";

        foreach (var d in request.Details)
        {
            if (d.Status == CancelBankMDDetailStatus.Approved)
            {
                d.Status = CancelBankMDDetailStatus.Finished;
            }
        }

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Ngân hàng từ chối đề nghị hủy gán bảo lãnh (DMS40_DlrCtr_CancelBankMD_Cancel từ phía Bank).
    /// </summary>
    public async Task<ContractBankMDCancel> RejectByBankAsync(long id, Guid orgId, RejectCancelBankMDDto dto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status != CancelBankMDStatus.Pending)
            throw new InvalidOperationException($"Hồ sơ ở trạng thái '{request.Status}' không thể từ chối.");

        request.Status = CancelBankMDStatus.Rejected;
        request.RejectBy = dto.RejecterName ?? "CanBoTinDung_NganHang";
        request.RejectDateTime = DateTime.Now;
        request.RejectReason = dto.Reason;
        request.RemarkBank = $"[Ngân hàng từ chối] {dto.Reason}";

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// HTC từ chối đề nghị hủy gán bảo lãnh (DMS40_DlrCtr_CancelBankMD_Reject_New20181115).
    /// </summary>
    public async Task<ContractBankMDCancel> RejectByHTCAsync(long id, Guid orgId, RejectCancelBankMDDto dto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status == CancelBankMDStatus.Finished || request.Status == CancelBankMDStatus.Cancelled)
            throw new InvalidOperationException("Hồ sơ đã hoàn tất hoặc đã hủy, không thể từ chối.");

        request.Status = CancelBankMDStatus.Rejected;
        request.RejectBy = dto.RejecterName ?? "CanBoPhapChe_HTC";
        request.RejectDateTime = DateTime.Now;
        request.RejectReason = dto.Reason;
        request.RemarkHTC = $"[HTC từ chối] {dto.Reason}";

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Đại lý chủ động hủy rút đề nghị khi chưa xử lý xong (DMS40_DlrCtr_CancelBankMD_Cancel).
    /// </summary>
    public async Task<ContractBankMDCancel> CancelRequestAsync(long id, Guid orgId, CancelBankMDUserCancelDto? dto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status == CancelBankMDStatus.Finished)
            throw new InvalidOperationException("Hồ sơ đã được HTC duyệt hoàn tất (Finished), không thể hủy.");

        request.Status = CancelBankMDStatus.Cancelled;
        request.CancelBy = dto?.CancellerName ?? "DaiLy_NguoiLap";
        request.CancelDateTime = DateTime.Now;
        request.CancelReason = dto?.Reason ?? "Đại lý chủ động rút đề nghị hủy gán ngân hàng bảo lãnh";

        foreach (var d in request.Details)
        {
            d.Status = CancelBankMDDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Bổ sung xe vào danh sách đề nghị hủy gán bảo lãnh (chỉ khi hồ sơ ở trạng thái Pending).
    /// </summary>
    public async Task<ContractBankMDCancel> AddVehicleAsync(long id, Guid orgId, CancelBankMDItemInputDto itemDto)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status != CancelBankMDStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể thêm xe khi hồ sơ đang ở trạng thái Chờ xử lý (Pending).");

        string vin = itemDto.VIN.Trim().ToUpperInvariant();
        if (request.Details.Any(d => d.VIN == vin))
            throw new InvalidOperationException($"Xe có số khung VIN '{vin}' đã tồn tại trong đề nghị này.");

        long uPrice = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : 800_000_000;
        long grtAmt = itemDto.GuaranteeAmount ?? uPrice;

        var detail = new ContractBankMDCancelDetail
        {
            CancelBankMDId = request.Id,
            OrgId = orgId,
            CancelBankMDNo = request.CancelBankMDNo,
            VIN = vin,
            CarId = itemDto.CarId?.Trim(),
            ModelCode = itemDto.ModelCode.Trim().ToUpperInvariant(),
            ModelName = string.IsNullOrWhiteSpace(itemDto.ModelName) ? GetModelNameByCode(itemDto.ModelCode) : itemDto.ModelName.Trim(),
            SpecCode = itemDto.SpecCode?.Trim(),
            SpecDescription = itemDto.SpecDescription?.Trim(),
            ColorExtNameVN = itemDto.ColorExtNameVN?.Trim() ?? "Trắng",
            EngineNo = itemDto.EngineNo?.Trim(),
            UnitPrice = uPrice,
            GuaranteeAmount = grtAmt,
            Status = CancelBankMDDetailStatus.Pending,
            Remark = itemDto.Remark?.Trim()
        };

        request.Details.Add(detail);
        request.TotalVehicles = request.Details.Count;
        request.ContractAmount += uPrice;
        request.GuaranteeAmount += grtAmt;

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Xóa bớt xe khỏi đề nghị hủy gán bảo lãnh (chỉ khi hồ sơ ở trạng thái Pending).
    /// </summary>
    public async Task<ContractBankMDCancel> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var request = await db.CancelBankMDRequests
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (request.Status != CancelBankMDStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể xóa xe khi hồ sơ đang ở trạng thái Chờ xử lý (Pending).");

        var detail = request.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong đề nghị.");

        request.Details.Remove(detail);
        db.CancelBankMDDetails.Remove(detail);

        request.TotalVehicles = request.Details.Count;
        request.ContractAmount = request.Details.Sum(d => d.UnitPrice);
        request.GuaranteeAmount = request.Details.Sum(d => d.GuaranteeAmount);

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Thỏa thuận 3 bên Hủy Cam kết Bảo lãnh Ngân hàng (Tripartite Bank Guarantee Revocation Agreement Advice) kèm đọc số tiền thành chữ tiếng Việt chuẩn HTC.
    /// </summary>
    public async Task<CancelBankMDAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var r = await db.CancelBankMDRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == id && x.OrgId == orgId);

        if (r == null) return null;

        int idx = 1;
        var items = r.Details.OrderBy(d => d.Id).Select(d => new CancelBankMDDetailAdviceDto
        {
            No = idx++,
            VIN = d.VIN,
            ModelCode = d.ModelCode,
            ModelName = d.ModelName ?? d.ModelCode,
            SpecCode = d.SpecCode ?? "",
            EngineNo = d.EngineNo ?? "",
            ColorExtNameVN = d.ColorExtNameVN ?? "",
            UnitPrice = d.UnitPrice,
            GuaranteeAmount = d.GuaranteeAmount,
            Status = d.Status switch
            {
                CancelBankMDDetailStatus.Pending => "Chờ duyệt",
                CancelBankMDDetailStatus.Approved => "Ngân hàng đã duyệt",
                CancelBankMDDetailStatus.Finished => "HTC hoàn tất",
                CancelBankMDDetailStatus.Cancelled => "Đã hủy",
                _ => d.Status.ToString()
            }
        }).ToList();

        string grtTypeText = r.GuaranteeType switch
        {
            GuaranteeType.Payment => "Bảo lãnh thanh toán đơn hàng (Payment Guarantee)",
            GuaranteeType.ContractPerformance => "Bảo lãnh thực hiện hợp đồng (Performance Guarantee)",
            GuaranteeType.DeferredPayment => "Bảo lãnh trả chậm L/C (Deferred Payment Guarantee)",
            GuaranteeType.AdvancePayment => "Bảo lãnh tạm ứng (Advance Guarantee)",
            _ => r.GuaranteeType.ToString()
        };

        string reasonTypeText = r.ReasonType switch
        {
            CancelBankMDReasonType.ChangeBank => "Chuyển sang ngân hàng bảo lãnh tài trợ khác",
            CancelBankMDReasonType.SwitchToOwnCapital => "Chuyển đổi phương thức thanh toán sang vốn tự có / UNC trực tiếp",
            CancelBankMDReasonType.ContractRestructuring => "Tái cơ cấu danh mục xe và hạn mức hợp đồng",
            CancelBankMDReasonType.Other => "Lý do tài chính khác",
            _ => r.ReasonType.ToString()
        };

        string statusText = r.Status switch
        {
            CancelBankMDStatus.Pending => "Chờ xử lý (Pending)",
            CancelBankMDStatus.Approved => "Ngân hàng chấp thuận (Bank Approved)",
            CancelBankMDStatus.Finished => "HTC duyệt hoàn tất (Finished)",
            CancelBankMDStatus.Rejected => "Từ chối (Rejected)",
            CancelBankMDStatus.Cancelled => "Đã hủy (Cancelled)",
            _ => r.Status.ToString()
        };

        return new CancelBankMDAdviceDto
        {
            CancelBankMDNo = r.CancelBankMDNo,
            DlrCtrNo = r.DlrCtrNo,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            DealerCode = r.DealerCode,
            DealerName = r.DealerName,
            BankCodeMD = r.BankCodeMD,
            BankNameMD = r.BankNameMD,
            NewBankNameMD = r.NewBankNameMD,
            GuaranteeTypeText = grtTypeText,
            ReasonTypeText = reasonTypeText,
            ReasonDescription = r.ReasonDescription ?? reasonTypeText,
            TotalVehicles = r.TotalVehicles,
            ContractAmount = r.ContractAmount,
            GuaranteeAmount = r.GuaranteeAmount,
            GuaranteeAmountInWords = LongInt2VNSpeakString(r.GuaranteeAmount, "đồng"),
            StatusText = statusText,
            RemarkDlr = r.RemarkDlr,
            RemarkBank = r.RemarkBank,
            RemarkHTC = r.RemarkHTC,
            ApproveBy = r.ApproveBy,
            ApproveDateTime = r.ApproveDateTime?.ToString("dd/MM/yyyy HH:mm"),
            FinishBy = r.FinishBy,
            FinishDTime = r.FinishDTime?.ToString("dd/MM/yyyy HH:mm"),
            Items = items
        };
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu hồ sơ đề nghị hủy gán ngân hàng bảo lãnh (Dashboard Summary).
    /// </summary>
    public async Task<CancelBankMDSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.CancelBankMDRequests.Where(r => r.OrgId == orgId).AsNoTracking().ToListAsync();

        return new CancelBankMDSummaryDto
        {
            TotalRequests = list.Count,
            PendingCount = list.Count(r => r.Status == CancelBankMDStatus.Pending),
            BankApprovedCount = list.Count(r => r.Status == CancelBankMDStatus.Approved),
            FinishedCount = list.Count(r => r.Status == CancelBankMDStatus.Finished),
            RejectedCount = list.Count(r => r.Status == CancelBankMDStatus.Rejected),
            CancelledCount = list.Count(r => r.Status == CancelBankMDStatus.Cancelled),
            TotalVehiclesRevoked = list.Where(r => r.Status == CancelBankMDStatus.Finished).Sum(r => r.TotalVehicles),
            TotalGuaranteeAmountRevoked = list.Where(r => r.Status == CancelBankMDStatus.Finished).Sum(r => r.GuaranteeAmount),
            TotalContractValueRevoked = list.Where(r => r.Status == CancelBankMDStatus.Finished).Sum(r => r.ContractAmount)
        };
    }

    /// <summary>
    /// Danh sách phụ lục hợp đồng mẫu đang gán ngân hàng bảo lãnh để đại lý dễ dàng chọn lập đề nghị hủy.
    /// </summary>
    public List<CandidateContractForCancelBankMDDto> GetCandidateContracts(string? dealerCode = null)
    {
        var candidates = new List<CandidateContractForCancelBankMDDto>
        {
            new()
            {
                DlrCtrNo = "PLHD-2025-05/TX01",
                DealerCode = "HYUNDAI-TX",
                DealerName = "Hyundai Thanh Xuân",
                BankCodeMD = "VPB",
                BankNameMD = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                GuaranteeType = "Payment",
                ContractAmount = 3_178_000_000,
                GuaranteeAmount = 2_700_000_000,
                VehicleCount = 2,
                CandidateVehicles =
                [
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770101",
                        CarId = "CAR-SF-0101",
                        ModelCode = "SANTAFE-CAL",
                        ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                        SpecCode = "SF-2.5T-CAL6",
                        SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                        ColorExtNameVN = "Trắng ngọc trai",
                        EngineNo = "ENG-SF-2025-01",
                        UnitPrice = 1_589_000_000,
                        GuaranteeAmount = 1_350_000_000,
                        Remark = "Đề nghị chuyển sang VietinBank tài trợ"
                    },
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770102",
                        CarId = "CAR-SF-0102",
                        ModelCode = "SANTAFE-CAL",
                        ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                        SpecCode = "SF-2.5T-CAL6",
                        SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                        ColorExtNameVN = "Đen Midnight",
                        EngineNo = "ENG-SF-2025-02",
                        UnitPrice = 1_589_000_000,
                        GuaranteeAmount = 1_350_000_000,
                        Remark = "Đề nghị chuyển sang VietinBank tài trợ"
                    }
                ]
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/HD02",
                DealerCode = "HYUNDAI-HD",
                DealerName = "Hyundai Hà Đông",
                BankCodeMD = "CTG",
                BankNameMD = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                GuaranteeType = "Payment",
                ContractAmount = 2_447_000_000,
                GuaranteeAmount = 2_000_000_000,
                VehicleCount = 3,
                CandidateVehicles =
                [
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770201",
                        CarId = "CAR-CRT-0201",
                        ModelCode = "CRETA-PREM",
                        ModelName = "Hyundai Creta 1.5 Cao Cấp",
                        SpecCode = "CRT-1.5L-PRE",
                        SpecDescription = "Bản SUV đô thị SmartSense",
                        ColorExtNameVN = "Đỏ Mận",
                        EngineNo = "ENG-CR-2025-11",
                        UnitPrice = 699_000_000,
                        GuaranteeAmount = 570_000_000,
                        Remark = "Đại lý chuyển sang thanh toán vốn tự có"
                    },
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770202",
                        CarId = "CAR-CST-0202",
                        ModelCode = "CUSTIN-2.0T",
                        ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                        SpecCode = "CUS-2.0T-PREM",
                        SpecDescription = "Bản MPV cửa lùa điện kép",
                        ColorExtNameVN = "Trắng",
                        EngineNo = "ENG-CS-2025-22",
                        UnitPrice = 974_000_000,
                        GuaranteeAmount = 800_000_000,
                        Remark = "Đại lý chuyển sang thanh toán vốn tự có"
                    },
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770203",
                        CarId = "CAR-TUC-0203",
                        ModelCode = "TUCSON-TURBO",
                        ModelName = "Hyundai Tucson 1.6 T-GDi HTRAC",
                        SpecCode = "TUC-1.6T-TUR",
                        SpecDescription = "Bản máy xăng tăng áp thể thao",
                        ColorExtNameVN = "Xanh Dương",
                        EngineNo = "ENG-TC-2025-33",
                        UnitPrice = 774_000_000,
                        GuaranteeAmount = 630_000_000,
                        Remark = "Đại lý chuyển sang thanh toán vốn tự có"
                    }
                ]
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/DA03",
                DealerCode = "HYUNDAI-DA",
                DealerName = "Hyundai Đông Anh",
                BankCodeMD = "MBB",
                BankNameMD = "Ngân hàng TMCP Quân Đội (MBBank)",
                GuaranteeType = "Payment",
                ContractAmount = 3_118_000_000,
                GuaranteeAmount = 2_600_000_000,
                VehicleCount = 2,
                CandidateVehicles =
                [
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770301",
                        CarId = "CAR-PLS-0301",
                        ModelCode = "PALISADE-PRE",
                        ModelName = "Hyundai Palisade 2.2D Prestige",
                        SpecCode = "PLS-2.2D-PRE",
                        SpecDescription = "Bản SUV cỡ lớn 7 chỗ máy dầu",
                        ColorExtNameVN = "Xanh Bóng Đêm",
                        EngineNo = "ENG-PL-2025-41",
                        UnitPrice = 1_559_000_000,
                        GuaranteeAmount = 1_300_000_000,
                        Remark = "Chuyển sang gói tài trợ Techcombank ưu đãi lãi suất"
                    },
                    new CancelBankMDItemInputDto
                    {
                        VIN = "KMHCT81EPHU770302",
                        CarId = "CAR-PLS-0302",
                        ModelCode = "PALISADE-PRE",
                        ModelName = "Hyundai Palisade 2.2D Prestige",
                        SpecCode = "PLS-2.2D-PRE",
                        SpecDescription = "Bản SUV cỡ lớn 7 chỗ máy dầu",
                        ColorExtNameVN = "Đen",
                        EngineNo = "ENG-PL-2025-42",
                        UnitPrice = 1_559_000_000,
                        GuaranteeAmount = 1_300_000_000,
                        Remark = "Chuyển sang gói tài trợ Techcombank ưu đãi lãi suất"
                    }
                ]
            }
        };

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            candidates = candidates.Where(c => c.DealerCode.Equals(dealerCode.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return candidates;
    }

    private static string GetDealerNameByCode(string dealerCode)
    {
        return dealerCode.Trim().ToUpperInvariant() switch
        {
            "HYUNDAI-TX" or "DLR-THANHXUAN" => "Hyundai Thanh Xuân",
            "HYUNDAI-HD" or "DLR-HADONG" => "Hyundai Hà Đông",
            "HYUNDAI-DA" or "DLR-DONGANH" => "Hyundai Đông Anh",
            "HYUNDAI-GP" or "DLR-GIAIPHONG" => "Hyundai Giải Phóng",
            "HYUNDAI-PVD" or "DLR-PHAMVANDONG" => "Hyundai Phạm Văn Đồng",
            "HYUNDAI-SG" or "DLR-SAIGON" => "Hyundai Sài Gòn 1S",
            "HYUNDAI-DN" or "DLR-DANANG" => "Hyundai Sông Hàn - Đà Nẵng",
            _ => $"Đại lý Hyundai {dealerCode}"
        };
    }

    private static string GetBankNameByCode(string bankCode)
    {
        return bankCode.Trim().ToUpperInvariant() switch
        {
            "VPBANK" or "VPB" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
            "VIETINBANK" or "CTG" => "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
            "MBBANK" or "MBB" => "Ngân hàng TMCP Quân Đội (MBBank)",
            "TECHCOMBANK" or "TCB" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
            "VIETCOMBANK" or "VCB" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
            "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
            "VIB" => "Ngân hàng TMCP Quốc tế Việt Nam (VIB)",
            _ => $"Ngân hàng {bankCode}"
        };
    }

    private static string GetModelNameByCode(string modelCode)
    {
        return modelCode.Trim().ToUpperInvariant() switch
        {
            "SANTAFE-CAL" or "SANTAFE" => "Hyundai Santa Fe 2.5T",
            "TUCSON-TURBO" or "TUCSON" => "Hyundai Tucson 1.6 T-GDi",
            "CRETA-PREM" or "CRETA" => "Hyundai Creta 1.5L",
            "CUSTIN-2.0T" or "CUSTIN" => "Hyundai Custin 2.0 T-GDi",
            "PALISADE-PRE" or "PALISADE" => "Hyundai Palisade 2.2D",
            "ACCENT-AT" or "ACCENT" => "Hyundai Accent 1.5 AT",
            "ELANTRA-PREM" or "ELANTRA" => "Hyundai Elantra 2.0 AT",
            "IONIQ-5" => "Hyundai IONIQ 5 Exclusive",
            _ => $"Hyundai {modelCode}"
        };
    }

    /// <summary>
    /// Chuyển đổi số nguyên thành chuỗi đọc tiếng Việt tài chính ngân hàng chuẩn HTC.
    /// </summary>
    public static string LongInt2VNSpeakString(long number, string dvt = "đồng")
    {
        if (number == 0) return "Không " + dvt;
        if (number < 0) return "Âm " + LongInt2VNSpeakString(-number, dvt);

        string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
        string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

        string ReadThreeDigits(int n, bool readZeroHundred)
        {
            int h = n / 100;
            int t = (n % 100) / 10;
            int u = n % 10;
            string res = "";

            if (h > 0 || readZeroHundred)
            {
                res += digits[h] + " trăm ";
            }

            if (t > 1)
            {
                res += digits[t] + " mươi ";
                if (u == 1) res += "mốt ";
                else if (u == 5) res += "lăm ";
                else if (u > 0) res += digits[u] + " ";
            }
            else if (t == 1)
            {
                res += "mười ";
                if (u == 5) res += "lăm ";
                else if (u > 0) res += digits[u] + " ";
            }
            else if (t == 0 && u > 0)
            {
                if (h > 0 || readZeroHundred) res += "lẻ ";
                res += digits[u] + " ";
            }

            return res;
        }

        string result = "";
        long temp = number;
        int unitIndex = 0;

        while (temp > 0)
        {
            int threeDigits = (int)(temp % 1000);
            temp /= 1000;

            if (threeDigits > 0)
            {
                string groupStr = ReadThreeDigits(threeDigits, temp > 0);
                result = groupStr + units[unitIndex] + " " + result;
            }
            unitIndex++;
        }

        result = result.Trim();
        if (result.Length > 0)
        {
            result = char.ToUpper(result[0]) + result[1..] + " " + dvt;
        }

        return result;
    }
}
