using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hồ sơ Đề nghị Giao dịch Ngân hàng & Tài trợ Vốn Vay / Bảo lãnh / L/C cho Đại lý Xe Ô tô.
/// Tương ứng module RQ_BankingTransactions, RQ_BankingTransCtr, RQ_BankingTransBankFile trong BizHTC.Payment / FrmDeNghiGDNganHang &amp; FrmQL_DeNghiGDNganHang.
/// </summary>
public sealed class BankingDisbursementService(AppDbContext db)
{
    /// <summary>
    /// Lập đề nghị giao dịch ngân hàng mới kèm phụ lục hợp đồng xe và danh mục hồ sơ ký số CA.
    /// </summary>
    public async Task<BankingDisbursementRequest> CreateRequestAsync(Guid orgId, CreateDisbursementRequestDto dto, string? createdBy = "DealerCreditOfficer")
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý không được để trống.", nameof(dto.DealerCode));

        if (string.IsNullOrWhiteSpace(dto.BankCode))
            throw new ArgumentException("Mã ngân hàng tài trợ không được để trống.", nameof(dto.BankCode));

        if (dto.Details == null || dto.Details.Count == 0)
            throw new ArgumentException("Danh sách xe / phụ lục hợp đồng đề nghị tài trợ không được để trống.", nameof(dto.Details));

        // Sinh số đề nghị giao dịch ngân hàng tự động: DNTT-yyyyMM-xxx (tương ứng GetBankingTransNo trong SalesService)
        string monthPrefix = $"DNTT-{DateTime.Now:yyyyMM}-";
        int seq = await db.DisbursementRequests
            .Where(r => r.OrgId == orgId && r.TransNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string finalTransNo = $"{monthPrefix}{seq:D3}";

        var request = new BankingDisbursementRequest
        {
            OrgId = orgId,
            TransNo = finalTransNo,
            TransType = dto.TransType,
            DealerCode = dto.DealerCode.Trim().ToUpperInvariant(),
            DealerName = string.IsNullOrWhiteSpace(dto.DealerName) ? GetDealerNameByCode(dto.DealerCode) : dto.DealerName.Trim(),
            BizResNumber = string.IsNullOrWhiteSpace(dto.BizResNumber) ? "0102839102-001" : dto.BizResNumber.Trim(),
            BankCode = dto.BankCode.Trim().ToUpperInvariant(),
            BankName = string.IsNullOrWhiteSpace(dto.BankName) ? GetBankNameByCode(dto.BankCode) : dto.BankName.Trim(),
            PaymentAccount = dto.PaymentAccount?.Trim() ?? "108008899888",
            PaymentBankName = dto.PaymentBankName?.Trim() ?? GetBankNameByCode(dto.BankCode),
            ReceivingUnit = dto.ReceivingUnit?.Trim() ?? "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
            ReceivingAccount = dto.ReceivingAccount?.Trim() ?? "113000088999",
            ReceivingBank = dto.ReceivingBank?.Trim() ?? "VietinBank - CN Đống Đa",
            DisbursementTerm = dto.DisbursementTerm?.Trim() ?? (dto.TransType == BankingTransType.PhatHanhBLLC ? "45 ngày" : "03 tháng"),
            DisbursementInterestRate = dto.DisbursementInterestRate ?? (dto.TransType == BankingTransType.PhatHanhBLLC ? 0m : 8.5m),
            Status = BankingTransStatus.Draft,
            BankStatus = BankingTransBankStatus.Pending,
            Remark = dto.Remark?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now
        };

        long totalContractAmt = 0;
        long totalDisbursedAmt = 0;

        foreach (var d in dto.Details)
        {
            int qty = d.Qty > 0 ? d.Qty : 1;
            long total = qty * d.UnitPrice;
            decimal ltv = d.LtvRate > 0 ? d.LtvRate : 80.0m;
            long disbAmt = (long)Math.Round(total * (ltv / 100m), MidpointRounding.AwayFromZero);

            totalContractAmt += total;
            totalDisbursedAmt += disbAmt;

            request.Details.Add(new BankingDisbursementDetail
            {
                OrgId = orgId,
                TransNo = finalTransNo,
                DlrCtrNo = d.DlrCtrNo.Trim().ToUpperInvariant(),
                ModelCode = d.ModelCode.Trim().ToUpperInvariant(),
                ModelName = d.ModelName?.Trim() ?? d.ModelCode.Trim().ToUpperInvariant(),
                SpecCode = d.SpecCode.Trim().ToUpperInvariant(),
                SpecDescription = d.SpecDescription?.Trim(),
                AssemblyType = d.AssemblyType,
                ContractDate = d.ContractDate ?? DateTime.Today.AddDays(-2),
                PrincipalContractNo = d.PrincipalContractNo?.Trim() ?? "HDNT-2025/HTC-DLR",
                PrincipalContractDate = d.PrincipalContractDate ?? new DateTime(2025, 1, 1),
                DeliveryDate = d.DeliveryDate ?? DateTime.Today.AddDays(15),
                Qty = qty,
                UnitPrice = d.UnitPrice,
                TotalAmount = total,
                LtvRate = ltv,
                DisbursementAmount = disbAmt,
                Remark = d.Remark?.Trim()
            });
        }

        request.TotalCars = request.Details.Sum(x => x.Qty);
        request.TotalContractAmount = totalContractAmt;
        request.TotalDisbursementAmount = totalDisbursedAmt;

        // Thêm các file chứng từ tài chính gửi ngân hàng
        if (dto.Files != null && dto.Files.Count > 0)
        {
            foreach (var f in dto.Files)
            {
                request.BankFiles.Add(new BankingDisbursementFile
                {
                    OrgId = orgId,
                    TransNo = finalTransNo,
                    DocType = f.DocType,
                    FileName = f.FileName.Trim(),
                    FilePath = f.FilePath?.Trim() ?? $"/storage/banking/{finalTransNo}/{f.FileName.Trim()}",
                    SignStatus = BankFileSignStatus.Pending,
                    UploadDate = DateTime.Now
                });
            }
        }
        else
        {
            // Hồ sơ chuẩn mặc định
            request.BankFiles.Add(new BankingDisbursementFile
            {
                OrgId = orgId,
                TransNo = finalTransNo,
                DocType = BankFileDocumentType.DeNghiVay,
                FileName = $"DeNghiGiaoDich_{finalTransNo}.pdf",
                FilePath = $"/storage/banking/{finalTransNo}/DeNghiGiaoDich_{finalTransNo}.pdf",
                SignStatus = BankFileSignStatus.Pending,
                UploadDate = DateTime.Now
            });
            request.BankFiles.Add(new BankingDisbursementFile
            {
                OrgId = orgId,
                TransNo = finalTransNo,
                DocType = BankFileDocumentType.PhuLucHopDong,
                FileName = $"PhuLucHopDong_{dto.Details[0].DlrCtrNo}.pdf",
                FilePath = $"/storage/banking/{finalTransNo}/PhuLucHopDong_{dto.Details[0].DlrCtrNo}.pdf",
                SignStatus = BankFileSignStatus.Pending,
                UploadDate = DateTime.Now
            });
            request.BankFiles.Add(new BankingDisbursementFile
            {
                OrgId = orgId,
                TransNo = finalTransNo,
                DocType = BankFileDocumentType.DangKyKinhDoanh,
                FileName = $"GPKD_BCTC_{dto.DealerCode}.pdf",
                FilePath = $"/storage/banking/{finalTransNo}/GPKD_BCTC_{dto.DealerCode}.pdf",
                SignStatus = BankFileSignStatus.Pending,
                UploadDate = DateTime.Now
            });
            request.BankFiles.Add(new BankingDisbursementFile
            {
                OrgId = orgId,
                TransNo = finalTransNo,
                DocType = BankFileDocumentType.CamKetTraNo,
                FileName = $"CamKetTraNo_{finalTransNo}.pdf",
                FilePath = $"/storage/banking/{finalTransNo}/CamKetTraNo_{finalTransNo}.pdf",
                SignStatus = BankFileSignStatus.Pending,
                UploadDate = DateTime.Now
            });
        }

        db.DisbursementRequests.Add(request);
        await db.SaveChangesAsync();

        return request;
    }

    /// <summary>
    /// Tìm kiếm danh sách đề nghị giao dịch ngân hàng theo các điều kiện lọc (RQ_BankingTransactions_Get).
    /// </summary>
    public async Task<List<BankingDisbursementRequest>> GetRequestsAsync(
        Guid orgId,
        string? dealerCode = null,
        string? bankCode = null,
        BankingTransType? transType = null,
        BankingTransStatus? status = null,
        BankingTransBankStatus? bankStatus = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.DisbursementRequests
            .Include(r => r.Details)
            .Include(r => r.BankFiles)
            .Where(r => r.OrgId == orgId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            var code = dealerCode.Trim().ToUpperInvariant();
            q = q.Where(r => r.DealerCode == code);
        }

        if (!string.IsNullOrWhiteSpace(bankCode))
        {
            var bank = bankCode.Trim().ToUpperInvariant();
            q = q.Where(r => r.BankCode == bank);
        }

        if (transType.HasValue)
        {
            q = q.Where(r => r.TransType == transType.Value);
        }

        if (status.HasValue)
        {
            q = q.Where(r => r.Status == status.Value);
        }

        if (bankStatus.HasValue)
        {
            q = q.Where(r => r.BankStatus == bankStatus.Value);
        }

        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            q = q.Where(r => r.CreatedAt >= start);
        }

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            q = q.Where(r => r.CreatedAt < end);
        }

        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết 1 đề nghị giao dịch ngân hàng (RQ_BankingTransactions_GetDetail).
    /// </summary>
    public async Task<BankingDisbursementRequest?> GetRequestByIdAsync(long id, Guid orgId)
    {
        return await db.DisbursementRequests
            .Include(r => r.Details)
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId);
    }

    /// <summary>
    /// Đẩy đề nghị sang cổng kết nối e-Banking ngân hàng đối tác (RQ_BankingTransactions_PushBank).
    /// </summary>
    public async Task<BankingDisbursementRequest> PushToBankAsync(long id, Guid orgId)
    {
        var request = await db.DisbursementRequests
            .Include(r => r.Details)
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status != BankingTransStatus.Draft)
            throw new InvalidOperationException($"Đề nghị đang ở trạng thái '{request.Status}', chỉ được đẩy sang ngân hàng khi ở trạng thái Draft.");

        if (request.Details.Count == 0)
            throw new InvalidOperationException("Hồ sơ không có xe / phụ lục hợp đồng nào, không thể gửi ngân hàng.");

        request.Status = BankingTransStatus.SentToBank;
        request.BankStatus = BankingTransBankStatus.SentWaiting;
        request.SentToBankAt = DateTime.Now;

        // Giả lập mã số hồ sơ e-Banking tiếp nhận phía ngân hàng (REFBANKCODE)
        request.RefBankCode = $"{request.BankCode}-LN-{DateTime.Now:yyyyMMdd}-{request.Id:D4}";
        request.BankRemark = $"Hồ sơ đã được đẩy thành công sang cổng B2B {request.BankName}. Chờ cán bộ tín dụng ngân hàng tiếp nhận xử lý.";

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Ngân hàng tiếp nhận thẩm định hồ sơ tín dụng.
    /// </summary>
    public async Task<BankingDisbursementRequest> ReviewByBankAsync(long id, Guid orgId, BankReviewDto? dto)
    {
        var request = await db.DisbursementRequests
            .Include(r => r.Details)
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status != BankingTransStatus.SentToBank && request.Status != BankingTransStatus.Processing)
            throw new InvalidOperationException($"Đề nghị ở trạng thái '{request.Status}' không thể chuyển sang thẩm định ngân hàng.");

        request.Status = BankingTransStatus.Processing;
        request.BankStatus = BankingTransBankStatus.Reviewing;
        if (!string.IsNullOrWhiteSpace(dto?.RefBankCode)) request.RefBankCode = dto.RefBankCode.Trim();
        request.BankRemark = dto?.BankRemark?.Trim() ?? "Cán bộ tín dụng Hội sở đang thẩm định hạn mức, tài sản bảo đảm và hồ sơ năng lực tài chính.";

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Ngân hàng yêu cầu bổ sung tài liệu hoặc hồ sơ pháp lý (Approve1 / Approve2 / Approve3).
    /// </summary>
    public async Task<BankingDisbursementRequest> RequestMoreDocsAsync(long id, Guid orgId, RequestMoreDocsDto dto)
    {
        var request = await db.DisbursementRequests
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status != BankingTransStatus.Processing && request.Status != BankingTransStatus.SentToBank)
            throw new InvalidOperationException($"Đề nghị đang ở trạng thái '{request.Status}', không thể yêu cầu bổ sung hồ sơ.");

        request.BankStatus = dto.IsMissingFiles ? BankingTransBankStatus.RequireMoreFiles : BankingTransBankStatus.RequireMoreDocs;
        request.BankRemark = string.IsNullOrWhiteSpace(dto.Reason)
            ? "Ngân hàng yêu cầu đại lý bổ sung chứng từ giải trình nguồn vốn và sao kê tài khoản ngân hàng."
            : dto.Reason.Trim();

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Đại lý ký số điện tử CA trên file chứng từ gửi ngân hàng (RQ_BankingTransactions_SignBankFile).
    /// </summary>
    public async Task<BankingDisbursementFile> SignBankFileAsync(long fileId, Guid orgId, SignBankFileDto dto)
    {
        var file = await db.DisbursementFiles.FirstOrDefaultAsync(f => f.Id == fileId && f.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy tài liệu chứng từ #{fileId}.");

        var request = await db.DisbursementRequests
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == file.RequestId && r.OrgId == orgId)
            ?? throw new InvalidOperationException("Không tìm thấy hồ sơ đề nghị tương ứng.");

        if (request.Status == BankingTransStatus.Completed || request.Status == BankingTransStatus.Cancelled)
            throw new InvalidOperationException("Hồ sơ đã hoàn tất hoặc đã hủy, không thể ký số tài liệu.");

        file.SignStatus = BankFileSignStatus.Signed;
        file.SignedUser = string.IsNullOrWhiteSpace(dto.SignedUser) ? "Nguyen Van A - Tong Giam Doc" : dto.SignedUser.Trim();
        file.CertSerialNumber = string.IsNullOrWhiteSpace(dto.CertSerialNumber) ? "54018899AACC4520" : dto.CertSerialNumber.Trim();
        file.SignedAt = DateTime.Now;

        // Nếu tất cả tài liệu đã ký số CA xong, tự động cập nhật trạng thái ngân hàng sang Đang thẩm định
        if (request.BankFiles.All(f => f.SignStatus == BankFileSignStatus.Signed) && request.BankStatus == BankingTransBankStatus.RequireSignCA)
        {
            request.BankStatus = BankingTransBankStatus.Reviewing;
            request.BankRemark = "Đại lý đã hoàn tất ký số CA toàn bộ tài liệu tín dụng. Đang chờ phê duyệt giải ngân cuối cùng.";
        }

        await db.SaveChangesAsync();
        return file;
    }

    /// <summary>
    /// Ngân hàng phê duyệt cấp tín dụng & giải ngân vốn vay / cấp bảo lãnh / cấp L/C (Finish / Disbursed).
    /// </summary>
    public async Task<BankingDisbursementRequest> ApproveAndDisburseAsync(long id, Guid orgId, ApproveDisburseDto dto)
    {
        var request = await db.DisbursementRequests
            .Include(r => r.Details)
            .Include(r => r.BankFiles)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status != BankingTransStatus.Processing && request.Status != BankingTransStatus.SentToBank)
            throw new InvalidOperationException($"Đề nghị đang ở trạng thái '{request.Status}', chỉ hồ sơ đang xử lý mới được giải ngân.");

        request.Status = BankingTransStatus.Completed;
        request.BankStatus = BankingTransBankStatus.Disbursed;
        request.CompletedAt = DateTime.Now;

        long actualAmt = dto.ActualAmount.HasValue && dto.ActualAmount.Value > 0
            ? dto.ActualAmount.Value
            : request.TotalDisbursementAmount;
        request.ActualDisbursedAmount = actualAmt;

        if (dto.InterestRate.HasValue && dto.InterestRate.Value > 0)
            request.DisbursementInterestRate = dto.InterestRate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Term))
            request.DisbursementTerm = dto.Term.Trim();

        // Cập nhật mã chứng từ nghiệp vụ tương ứng loại giao dịch
        switch (request.TransType)
        {
            case BankingTransType.GNTT:
            case BankingTransType.GNTTLC:
                request.LDNo = !string.IsNullOrWhiteSpace(dto.LDNo) ? dto.LDNo.Trim() : $"LD-{request.BankCode}-{DateTime.Now:yyyyMMdd}-{request.Id:D3}";
                request.DisbursementDate = DateTime.Now;
                request.FirstInterestPmtDate = DateTime.Now.AddMonths(1);
                request.LoanLimit = Math.Max(request.LoanLimit, actualAmt * 2);
                request.BankRemark = dto.BankRemark?.Trim() ?? $"Ngân hàng {request.BankName} đã giải ngân thành công số tiền {actualAmt:N0} VND vào tài khoản thụ hưởng {request.ReceivingAccount}. Số khế ước nhận nợ: {request.LDNo}.";
                break;

            case BankingTransType.PhatHanhBLLC:
                request.MDNo = !string.IsNullOrWhiteSpace(dto.MDNo) ? dto.MDNo.Trim() : $"MD-{request.BankCode}-{DateTime.Now:yyyyMMdd}-{request.Id:D3}";
                request.GrtAmount = actualAmt;
                request.GrtDateStart = DateTime.Now;
                request.GrtDateEnd = DateTime.Now.AddDays(45);
                request.GrtFee = (long)Math.Round(actualAmt * 0.002m, MidpointRounding.AwayFromZero); // Phí BL 0.2%
                request.BankRemark = dto.BankRemark?.Trim() ?? $"Ngân hàng {request.BankName} đã phát hành Thư bảo lãnh thanh toán mở L/C số {request.MDNo}, giá trị {actualAmt:N0} VND.";
                break;

            case BankingTransType.PhatHanhLC:
                request.LCNo = !string.IsNullOrWhiteSpace(dto.LCNo) ? dto.LCNo.Trim() : $"LC-{request.BankCode}-{DateTime.Now:yyyyMMdd}-{request.Id:D3}";
                request.LCAmount = actualAmt;
                request.LCStartDate = DateTime.Now;
                request.LCEndDate = DateTime.Now.AddDays(60);
                request.BankRemark = dto.BankRemark?.Trim() ?? $"Ngân hàng {request.BankName} đã phát hành Thư tín dụng L/C số {request.LCNo}, giá trị {actualAmt:N0} VND.";
                break;

            case BankingTransType.HTDB:
                request.BankRemark = dto.BankRemark?.Trim() ?? $"Ngân hàng {request.BankName} đã xác nhận tất toán nghĩa vụ nợ và giải tỏa toàn bộ tài sản bảo đảm của các xe trong phụ lục hợp đồng.";
                break;
        }

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Ngân hàng từ chối cấp tín dụng / giải ngân (Rejected).
    /// </summary>
    public async Task<BankingDisbursementRequest> RejectByBankAsync(long id, Guid orgId, RejectDisbursementDto dto)
    {
        var request = await db.DisbursementRequests.FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status == BankingTransStatus.Completed || request.Status == BankingTransStatus.Cancelled)
            throw new InvalidOperationException("Hồ sơ đã hoàn tất hoặc đã hủy, không thể từ chối.");

        request.BankStatus = BankingTransBankStatus.Rejected;
        request.BankRemark = string.IsNullOrWhiteSpace(dto.Reason)
            ? "Hội đồng tín dụng ngân hàng không thông qua do vượt tỷ lệ đòn bẩy tài chính cho phép."
            : dto.Reason.Trim();

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Đại lý hủy đề nghị khi chưa giải ngân (RQ_BankingTransactions_Cancel).
    /// </summary>
    public async Task<BankingDisbursementRequest> CancelRequestAsync(long id, Guid orgId, string? reason)
    {
        var request = await db.DisbursementRequests.FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy đề nghị #{id}.");

        if (request.Status == BankingTransStatus.Completed)
            throw new InvalidOperationException("Hồ sơ đã được ngân hàng giải ngân hoàn tất (Completed), không thể hủy.");

        request.Status = BankingTransStatus.Cancelled;
        request.BankStatus = BankingTransBankStatus.Cancelled;
        request.CancelledAt = DateTime.Now;
        request.CancelReason = reason?.Trim() ?? "Đại lý hủy đề nghị theo yêu cầu điều chỉnh kế hoạch tài chính";

        await db.SaveChangesAsync();
        return request;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Giấy đề nghị giao dịch ngân hàng & cam kết trả nợ kèm đọc số tiền thành chữ tiếng Việt chuẩn HTC.
    /// </summary>
    public async Task<DisbursementAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var r = await db.DisbursementRequests
            .Include(x => x.Details)
            .Include(x => x.BankFiles)
            .FirstOrDefaultAsync(x => x.Id == id && x.OrgId == orgId);

        if (r == null) return null;

        int idx = 1;
        var carAdvices = r.Details.OrderBy(d => d.Id).Select(d => new DisbursementDetailAdviceDto
        {
            No = idx++,
            DlrCtrNo = d.DlrCtrNo,
            ModelCode = d.ModelCode,
            SpecCode = d.SpecCode,
            AssemblyTypeText = d.AssemblyType == VehicleAssemblyType.CKD ? "CKD (Lắp ráp trong nước)" : "CBU (Nhập khẩu nguyên chiếc)",
            Qty = d.Qty,
            UnitPrice = d.UnitPrice,
            TotalAmount = d.TotalAmount,
            LtvRate = d.LtvRate,
            DisbursementAmount = d.DisbursementAmount
        }).ToList();

        string transTypeText = r.TransType switch
        {
            BankingTransType.GNTT => "Giải ngân thanh toán vốn vay mua xe (GNTT)",
            BankingTransType.PhatHanhBLLC => "Phát hành thư bảo lãnh mở L/C (Phát hành BL/LC)",
            BankingTransType.PhatHanhLC => "Phát hành thư tín dụng mua xe (Phát hành L/C)",
            BankingTransType.HTDB => "Hoàn trả dư nợ và giải tỏa tài sản bảo đảm (HTĐB)",
            BankingTransType.GNTTLC => "Giải ngân thanh toán theo L/C (GNTT L/C)",
            _ => r.TransType.ToString()
        };

        string statusText = r.Status switch
        {
            BankingTransStatus.Draft => "Mới tạo / Dự thảo (Draft)",
            BankingTransStatus.SentToBank => "Đã gửi sang e-Banking (SentToBank)",
            BankingTransStatus.Processing => "Ngân hàng đang xử lý (Processing)",
            BankingTransStatus.Completed => "Hoàn thành giải ngân (Completed)",
            BankingTransStatus.Cancelled => "Đã hủy đề nghị (Cancelled)",
            _ => r.Status.ToString()
        };

        string bankStatusText = r.BankStatus switch
        {
            BankingTransBankStatus.Pending => "Chưa gửi ngân hàng",
            BankingTransBankStatus.SentWaiting => "Chờ ngân hàng tiếp nhận",
            BankingTransBankStatus.Reviewing => "Ngân hàng đang thẩm định",
            BankingTransBankStatus.InvalidFile => "File tài liệu không hợp lệ",
            BankingTransBankStatus.RequireMoreFiles => "Yêu cầu bổ sung tài liệu file",
            BankingTransBankStatus.RequireMoreDocs => "Yêu cầu bổ sung hồ sơ pháp lý",
            BankingTransBankStatus.RequireSignCA => "Chờ đại lý ký số điện tử CA",
            BankingTransBankStatus.Disbursed => "Đã giải ngân / Cấp tín dụng thành công",
            BankingTransBankStatus.Rejected => "Ngân hàng từ chối cấp tín dụng",
            BankingTransBankStatus.Cancelled => "Hồ sơ ngân hàng đã hủy",
            _ => r.BankStatus.ToString()
        };

        var signedDocs = r.BankFiles
            .Where(f => f.SignStatus == BankFileSignStatus.Signed)
            .Select(f => $"{f.FileName} (Ký bởi: {f.SignedUser} lúc {f.SignedAt:dd/MM/yyyy HH:mm})")
            .ToList();

        long amountToRead = r.ActualDisbursedAmount > 0 ? r.ActualDisbursedAmount : r.TotalDisbursementAmount;

        return new DisbursementAdviceDto
        {
            TransNo = r.TransNo,
            TransTypeText = transTypeText,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            DealerCode = r.DealerCode,
            DealerName = r.DealerName,
            BizResNumber = r.BizResNumber,
            BankName = r.BankName,
            BankCode = r.BankCode,
            PaymentAccount = r.PaymentAccount ?? "Chưa chỉ định",
            ReceivingUnit = r.ReceivingUnit,
            ReceivingAccount = r.ReceivingAccount,
            ReceivingBank = r.ReceivingBank,
            TotalCars = r.TotalCars,
            TotalContractAmount = r.TotalContractAmount,
            TotalDisbursementAmount = r.TotalDisbursementAmount,
            ActualDisbursedAmount = r.ActualDisbursedAmount,
            AmountInWords = LongInt2VNSpeakString(amountToRead, "đồng"),
            StatusText = statusText,
            BankStatusText = bankStatusText,
            RefBankCode = r.RefBankCode,
            LDNo = r.LDNo,
            MDNo = r.MDNo,
            LCNo = r.LCNo,
            DisbursementTerm = r.DisbursementTerm,
            DisbursementInterestRate = r.DisbursementInterestRate,
            DisbursementDate = r.DisbursementDate?.ToString("dd/MM/yyyy"),
            Cars = carAdvices,
            SignedDocuments = signedDocs
        };
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu hồ sơ tín dụng đại lý (Disbursement Summary).
    /// </summary>
    public async Task<DisbursementSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.DisbursementRequests.Where(r => r.OrgId == orgId).AsNoTracking().ToListAsync();

        return new DisbursementSummaryDto
        {
            TotalRequests = list.Count,
            DraftCount = list.Count(r => r.Status == BankingTransStatus.Draft),
            SentToBankCount = list.Count(r => r.Status == BankingTransStatus.SentToBank),
            ProcessingCount = list.Count(r => r.Status == BankingTransStatus.Processing),
            CompletedCount = list.Count(r => r.Status == BankingTransStatus.Completed),
            CancelledCount = list.Count(r => r.Status == BankingTransStatus.Cancelled),
            TotalVehiclesFinanced = list.Where(r => r.Status != BankingTransStatus.Cancelled).Sum(r => r.TotalCars),
            TotalContractAmount = list.Where(r => r.Status != BankingTransStatus.Cancelled).Sum(r => r.TotalContractAmount),
            TotalRequestedDisbursement = list.Where(r => r.Status != BankingTransStatus.Cancelled).Sum(r => r.TotalDisbursementAmount),
            TotalActualDisbursed = list.Where(r => r.Status == BankingTransStatus.Completed).Sum(r => r.ActualDisbursedAmount)
        };
    }

    /// <summary>
    /// Danh sách phụ lục hợp đồng xe mẫu ứng viên phục vụ lập nhanh hồ sơ đề nghị giải ngân.
    /// </summary>
    public List<DisbursementDetailInputDto> GetCandidateContracts(string? dealerCode = null)
    {
        var candidates = new List<DisbursementDetailInputDto>
        {
            new()
            {
                DlrCtrNo = "PLHD-2025-05/GP01",
                ModelCode = "SANTAFE-CAL",
                ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                SpecCode = "SF-2.5T-CAL6",
                SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                AssemblyType = VehicleAssemblyType.CKD,
                ContractDate = DateTime.Today.AddDays(-5),
                PrincipalContractNo = "HDNT-2025/HTC-GP",
                PrincipalContractDate = new DateTime(2025, 1, 10),
                DeliveryDate = DateTime.Today.AddDays(10),
                Qty = 5,
                UnitPrice = 1_365_000_000,
                LtvRate = 80.0m,
                Remark = "Đơn hàng theo chỉ tiêu doanh số tháng 5"
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/GP02",
                ModelCode = "TUCSON-TURBO",
                ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                SpecCode = "TUC-1.6T-PREM",
                SpecDescription = "Bản máy xăng tăng áp HTRAC",
                AssemblyType = VehicleAssemblyType.CKD,
                ContractDate = DateTime.Today.AddDays(-4),
                PrincipalContractNo = "HDNT-2025/HTC-GP",
                PrincipalContractDate = new DateTime(2025, 1, 10),
                DeliveryDate = DateTime.Today.AddDays(12),
                Qty = 4,
                UnitPrice = 989_000_000,
                LtvRate = 85.0m,
                Remark = "Lô xe giao showroom phục vụ lái thử & bán lẻ"
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/TX01",
                ModelCode = "IONIQ5-PREM",
                ModelName = "Hyundai Ioniq 5 Prestige EV",
                SpecCode = "IQ5-EV-PREM",
                SpecDescription = "Bản xe điện thông minh E-GMP",
                AssemblyType = VehicleAssemblyType.CKD,
                ContractDate = DateTime.Today.AddDays(-3),
                PrincipalContractNo = "HDNT-2025/HTC-TX",
                PrincipalContractDate = new DateTime(2025, 1, 15),
                DeliveryDate = DateTime.Today.AddDays(20),
                Qty = 3,
                UnitPrice = 1_450_000_000,
                LtvRate = 75.0m,
                Remark = "Đơn hàng xe điện phục vụ đối tác doanh nghiệp xanh"
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/PVD01",
                ModelCode = "PALISADE-PREM",
                ModelName = "Hyundai Palisade 2.2D Prestige",
                SpecCode = "PAL-2.2D-PREM",
                SpecDescription = "Bản SUV cỡ lớn máy dầu 7 chỗ",
                AssemblyType = VehicleAssemblyType.CKD,
                ContractDate = DateTime.Today.AddDays(-2),
                PrincipalContractNo = "HDNT-2025/HTC-PVD",
                PrincipalContractDate = new DateTime(2025, 1, 5),
                DeliveryDate = DateTime.Today.AddDays(15),
                Qty = 2,
                UnitPrice = 1_589_000_000,
                LtvRate = 80.0m,
                Remark = "Giao gấp khách hàng doanh nghiệp"
            },
            new()
            {
                DlrCtrNo = "PLHD-2025-05/SG01",
                ModelCode = "CUSTIN-TURBO",
                ModelName = "Hyundai Custin 2.0T Cao Cấp",
                SpecCode = "CUS-2.0T-PREM",
                SpecDescription = "Bản MPV cửa trượt điện 7 chỗ",
                AssemblyType = VehicleAssemblyType.CKD,
                ContractDate = DateTime.Today.AddDays(-6),
                PrincipalContractNo = "HDNT-2025/HTC-SG",
                PrincipalContractDate = new DateTime(2025, 1, 8),
                DeliveryDate = DateTime.Today.AddDays(18),
                Qty = 6,
                UnitPrice = 974_000_000,
                LtvRate = 80.0m,
                Remark = "Bán buôn mở rộng thị trường miền Nam"
            }
        };

        return candidates;
    }

    private static string GetDealerNameByCode(string dealerCode)
    {
        return dealerCode.Trim().ToUpperInvariant() switch
        {
            "HYUNDAI-GP" or "DLR-GIAIPHONG" => "Hyundai Giải Phóng",
            "HYUNDAI-TX" or "DLR-THANHXUAN" => "Hyundai Thanh Xuân",
            "HYUNDAI-PVD" or "DLR-PHAMVANDONG" => "Hyundai Phạm Văn Đồng",
            "HYUNDAI-HD" or "DLR-HADONG" => "Hyundai Hà Đông",
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
            "VIB" => "Ngân hàng TMCP Quốc tế Việt Nam (VIB)",
            "TECHCOMBANK" or "TCB" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
            "MBBANK" or "MBB" => "Ngân hàng TMCP Quân Đội (MBBank)",
            "VIETCOMBANK" or "VCB" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
            "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
            _ => $"Ngân hàng {bankCode}"
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
            result = char.ToUpper(result[0]) + result.Substring(1);
        }

        return result + " " + dvt;
    }
}
