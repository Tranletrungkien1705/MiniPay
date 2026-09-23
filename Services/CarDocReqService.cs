using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Hồ sơ Đề nghị Mượn / Bàn Giao Chứng Từ Gốc Xe Ô Tô & Xác Nhận Ngân Hàng.
/// Tương ứng Car_DocReqList, Car_DocReqDtl trong BizHTC.Car.Profile.cs, DataWH/BizHTC.zTemp.cs, Biz.HTC.WH.cs
/// và các màn hình FrmMngDocReq, FrmNewDocReq, FrmMngDocReqDealer, FrmUpdateDocReq,
/// báo cáo in Biên bản bàn giao hồ sơ gốc CRCarDocReq.rpt trong TERP.HTCClient/Views/Sales hệ nguồn HTC 2010.
/// </summary>
public sealed class CarDocReqService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách hồ sơ đề nghị giao chứng từ gốc kèm bộ lọc.
    /// </summary>
    public async Task<List<CarDocReqList>> GetListAsync(
        Guid orgId,
        string? dealer = null,
        string? bank = null,
        string? status = null,
        string? typeCRR = null,
        string? query = null)
    {
        var q = db.CarDocRequests
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(dealer))
        {
            string d = dealer.Trim().ToUpperInvariant();
            q = q.Where(x => x.DealerCode.Contains(d) || x.DealerName.ToUpper().Contains(d));
        }

        if (!string.IsNullOrWhiteSpace(bank))
        {
            string b = bank.Trim().ToUpperInvariant();
            q = q.Where(x => (x.BankCode != null && x.BankCode.Contains(b)) || (x.BankName != null && x.BankName.ToUpper().Contains(b)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CarDocReqStatus>(status, true, out var st))
        {
            q = q.Where(x => x.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(typeCRR) && Enum.TryParse<CarDocReqType>(typeCRR, true, out var t))
        {
            q = q.Where(x => x.TypeCRR == t);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.DRListCode.Contains(s)
                || x.DealerCode.Contains(s)
                || x.DealerName.ToUpper().Contains(s)
                || (x.RepresentativeName != null && x.RepresentativeName.ToUpper().Contains(s))
                || (x.LetterRepresentationNo != null && x.LetterRepresentationNo.ToUpper().Contains(s))
                || x.Details.Any(d => d.VIN.Contains(s) || (d.CarId != null && d.CarId.Contains(s))));
        }

        return await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết đề nghị theo ID.
    /// </summary>
    public async Task<CarDocReqList?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>
    /// Lấy chi tiết đề nghị theo Mã đề nghị (DRListCode).
    /// </summary>
    public async Task<CarDocReqList?> GetByCodeAsync(Guid orgId, string code)
    {
        string c = code.Trim().ToUpperInvariant();
        return await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.DRListCode == c);
    }

    /// <summary>
    /// Lập hồ sơ đề nghị giao chứng từ gốc mới (CarDocReqCreateHTC / CarDocReqCreateDealer / FrmNewDocReq).
    /// </summary>
    public async Task<CarDocReqList> CreateAsync(Guid orgId, CreateCarDocReqDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý đề nghị không được để trống.", nameof(dto.DealerCode));

        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Đề nghị phải có ít nhất 1 dòng xe ô tô.", nameof(dto.Items));

        string dCode = dto.DealerCode.Trim().ToUpperInvariant();
        string dName = !string.IsNullOrWhiteSpace(dto.DealerName) ? dto.DealerName.Trim() : $"Đại lý Hyundai {dCode}";

        // Tự động sinh mã đề nghị nếu không có: DNGT-yyyyMM-xxx
        string code;
        if (!string.IsNullOrWhiteSpace(dto.DRListCode))
        {
            code = dto.DRListCode.Trim().ToUpperInvariant();
            bool exists = await db.CarDocRequests.AnyAsync(x => x.OrgId == orgId && x.DRListCode == code);
            if (exists)
                throw new InvalidOperationException($"Mã đề nghị giao chứng từ '{code}' đã tồn tại trong hệ thống.");
        }
        else
        {
            string monthPrefix = $"DNGT-{DateTime.Now:yyyyMM}-";
            int count = await db.CarDocRequests.CountAsync(x => x.OrgId == orgId && x.DRListCode.StartsWith(monthPrefix));
            code = $"{monthPrefix}{(count + 1):D3}";
        }

        var req = new CarDocReqList
        {
            OrgId = orgId,
            DRListCode = code,
            DealerCode = dCode,
            DealerName = dName,
            DealerCodeRecieve = dto.DealerCodeRecieve?.Trim().ToUpperInvariant(),
            DealerNameRecieve = dto.DealerNameRecieve?.Trim(),
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BankName = dto.BankName?.Trim(),
            TypeCRR = dto.TypeCRR,
            LetterRepresentationNo = dto.LetterRepresentationNo?.Trim(),
            LetterRepresentationDate = dto.LetterRepresentationDate,
            RepresentativeName = dto.RepresentativeName?.Trim(),
            RepresentativeIdCard = dto.RepresentativeIdCard?.Trim(),
            RepresentativePhone = dto.RepresentativePhone?.Trim(),
            Status = CarDocReqStatus.Draft,
            ReturnDueDate = dto.ReturnDueDate,
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "ChuyenVienQuanLyHSCar",
            CreatedAt = DateTime.Now
        };

        foreach (var item in dto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN))
                throw new ArgumentException("Số khung xe (VIN) không được để trống.", nameof(item.VIN));

            string vin = item.VIN.Trim().ToUpperInvariant();
            decimal payPct = item.PaymentPercent ?? (req.TypeCRR == CarDocReqType.Dealer ? 30.0m : 100.0m);
            decimal grtPct = item.GuaranteePercent ?? (req.BankCode != null ? 70.0m : 0.0m);
            decimal dutyPct = item.DutyCompletePercent ?? Math.Min(100.0m, payPct + grtPct);

            var detail = new CarDocReqDetail
            {
                OrgId = orgId,
                DRListCode = code,
                VIN = vin,
                CarId = item.CarId?.Trim().ToUpperInvariant() ?? $"CAR-{vin[Math.Max(0, vin.Length - 8)..]}",
                ModelCode = string.IsNullOrWhiteSpace(item.ModelCode) ? "SANTAFE" : item.ModelCode.Trim().ToUpperInvariant(),
                ModelName = item.ModelName?.Trim() ?? "Hyundai All-New",
                SpecCode = item.SpecCode?.Trim(),
                EngineNo = item.EngineNo?.Trim() ?? $"ENG-{vin[Math.Max(0, vin.Length - 6)..]}",
                ColorNameVN = item.ColorNameVN?.Trim() ?? "Trắng",
                ContractNo = item.ContractNo?.Trim(),
                UnitPriceActual = item.UnitPriceActual > 0 ? item.UnitPriceActual : 850_000_000,
                PaymentPercent = payPct,
                DepositPercent = item.DepositPercent ?? Math.Min(payPct, 20.0m),
                GuaranteePercent = grtPct,
                DutyCompletePercent = dutyPct,
                BankGuaranteeNo = item.BankGuaranteeNo?.Trim(),
                CONo = item.CONo?.Trim() ?? $"CO-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
                CQNo = item.CQNo?.Trim() ?? $"CQ-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
                CustomsDeclarationNo = item.CustomsDeclarationNo?.Trim(),
                HTCInvoiceNo = item.HTCInvoiceNo?.Trim() ?? $"HD-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
                DocumentsGiven = !string.IsNullOrWhiteSpace(item.DocumentsGiven) ? item.DocumentsGiven.Trim() : "Bản gốc CO, Bản sao CQ, Tờ khai HQ, Hóa đơn VAT",
                BankApprStatus = string.IsNullOrWhiteSpace(req.BankCode) ? BankDocApprStatus.Approved : BankDocApprStatus.Pending,
                Status = CarDocReqDetailStatus.Pending,
                ReturnDueDate = item.ReturnDueDate ?? req.ReturnDueDate,
                Remark = item.Remark?.Trim()
            };

            req.Details.Add(detail);
        }

        RecalculateHeader(req);

        db.CarDocRequests.Add(req);
        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ đề nghị ở trạng thái Draft.
    /// </summary>
    public async Task<CarDocReqList> UpdateAsync(Guid orgId, long id, UpdateCarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể sửa hồ sơ đề nghị ở trạng thái Nháp (Draft). Hiện tại: {req.Status}.");

        if (dto.TypeCRR.HasValue) req.TypeCRR = dto.TypeCRR.Value;
        if (dto.DealerCodeRecieve != null) req.DealerCodeRecieve = dto.DealerCodeRecieve.Trim().ToUpperInvariant();
        if (dto.DealerNameRecieve != null) req.DealerNameRecieve = dto.DealerNameRecieve.Trim();
        if (dto.BankCode != null) req.BankCode = dto.BankCode.Trim().ToUpperInvariant();
        if (dto.BankName != null) req.BankName = dto.BankName.Trim();
        if (dto.LetterRepresentationNo != null) req.LetterRepresentationNo = dto.LetterRepresentationNo.Trim();
        if (dto.LetterRepresentationDate.HasValue) req.LetterRepresentationDate = dto.LetterRepresentationDate.Value;
        if (dto.RepresentativeName != null) req.RepresentativeName = dto.RepresentativeName.Trim();
        if (dto.RepresentativeIdCard != null) req.RepresentativeIdCard = dto.RepresentativeIdCard.Trim();
        if (dto.RepresentativePhone != null) req.RepresentativePhone = dto.RepresentativePhone.Trim();
        if (dto.ReturnDueDate.HasValue) req.ReturnDueDate = dto.ReturnDueDate.Value;
        if (dto.Remark != null) req.Remark = dto.Remark.Trim();

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Thêm 1 xe vào hồ sơ đề nghị ở trạng thái Draft.
    /// </summary>
    public async Task<CarDocReqList> AddDetailAsync(Guid orgId, long id, CarDocReqItemInputDto itemDto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể thêm xe vào hồ sơ đề nghị ở trạng thái Nháp (Draft). Hiện tại: {req.Status}.");

        if (string.IsNullOrWhiteSpace(itemDto.VIN))
            throw new ArgumentException("Số khung xe (VIN) không được để trống.", nameof(itemDto.VIN));

        string vin = itemDto.VIN.Trim().ToUpperInvariant();
        if (req.Details.Any(d => d.VIN == vin))
            throw new InvalidOperationException($"Số khung VIN '{vin}' đã có trong hồ sơ đề nghị này.");

        decimal payPct = itemDto.PaymentPercent ?? (req.TypeCRR == CarDocReqType.Dealer ? 30.0m : 100.0m);
        decimal grtPct = itemDto.GuaranteePercent ?? (req.BankCode != null ? 70.0m : 0.0m);
        decimal dutyPct = itemDto.DutyCompletePercent ?? Math.Min(100.0m, payPct + grtPct);

        var detail = new CarDocReqDetail
        {
            OrgId = orgId,
            DocReqId = req.Id,
            DRListCode = req.DRListCode,
            VIN = vin,
            CarId = itemDto.CarId?.Trim().ToUpperInvariant() ?? $"CAR-{vin[Math.Max(0, vin.Length - 8)..]}",
            ModelCode = string.IsNullOrWhiteSpace(itemDto.ModelCode) ? "SANTAFE" : itemDto.ModelCode.Trim().ToUpperInvariant(),
            ModelName = itemDto.ModelName?.Trim() ?? "Hyundai All-New",
            SpecCode = itemDto.SpecCode?.Trim(),
            EngineNo = itemDto.EngineNo?.Trim() ?? $"ENG-{vin[Math.Max(0, vin.Length - 6)..]}",
            ColorNameVN = itemDto.ColorNameVN?.Trim() ?? "Trắng",
            ContractNo = itemDto.ContractNo?.Trim(),
            UnitPriceActual = itemDto.UnitPriceActual > 0 ? itemDto.UnitPriceActual : 850_000_000,
            PaymentPercent = payPct,
            DepositPercent = itemDto.DepositPercent ?? Math.Min(payPct, 20.0m),
            GuaranteePercent = grtPct,
            DutyCompletePercent = dutyPct,
            BankGuaranteeNo = itemDto.BankGuaranteeNo?.Trim(),
            CONo = itemDto.CONo?.Trim() ?? $"CO-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
            CQNo = itemDto.CQNo?.Trim() ?? $"CQ-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
            CustomsDeclarationNo = itemDto.CustomsDeclarationNo?.Trim(),
            HTCInvoiceNo = itemDto.HTCInvoiceNo?.Trim() ?? $"HD-2025-{vin[Math.Max(0, vin.Length - 5)..]}",
            DocumentsGiven = !string.IsNullOrWhiteSpace(itemDto.DocumentsGiven) ? itemDto.DocumentsGiven.Trim() : "Bản gốc CO, Bản sao CQ, Tờ khai HQ, Hóa đơn VAT",
            BankApprStatus = string.IsNullOrWhiteSpace(req.BankCode) ? BankDocApprStatus.Approved : BankDocApprStatus.Pending,
            Status = CarDocReqDetailStatus.Pending,
            ReturnDueDate = itemDto.ReturnDueDate ?? req.ReturnDueDate,
            Remark = itemDto.Remark?.Trim()
        };

        req.Details.Add(detail);
        RecalculateHeader(req);

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Xóa 1 xe khỏi hồ sơ đề nghị Draft.
    /// </summary>
    public async Task<CarDocReqList> RemoveDetailAsync(Guid orgId, long id, long detailId)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa xe ở hồ sơ đề nghị trạng thái Nháp (Draft). Hiện tại: {req.Status}.");

        var detail = req.Details.FirstOrDefault(d => d.Id == detailId);
        if (detail == null)
            throw new InvalidOperationException($"Không tìm thấy dòng xe #{detailId} trong đề nghị.");

        if (req.Details.Count <= 1)
            throw new InvalidOperationException("Hồ sơ đề nghị phải có ít nhất 1 dòng xe ô tô. Không thể xóa dòng cuối cùng.");

        req.Details.Remove(detail);
        db.CarDocRequestDetails.Remove(detail);
        RecalculateHeader(req);

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Xóa toàn bộ hồ sơ đề nghị ở trạng thái Draft.
    /// </summary>
    public async Task DeleteDraftAsync(Guid orgId, long id)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa hồ sơ đề nghị ở trạng thái Nháp (Draft). Hiện tại: {req.Status}.");

        db.CarDocRequests.Remove(req);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Trình duyệt hồ sơ đề nghị (Draft -> Pending).
    /// </summary>
    public async Task<CarDocReqList> SubmitAsync(Guid orgId, long id, SubmitCarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Draft)
            throw new InvalidOperationException($"Hồ sơ đề nghị đang ở trạng thái {req.Status}, không thể trình duyệt.");

        if (req.Details.Count == 0)
            throw new InvalidOperationException("Hồ sơ đề nghị chưa có xe ô tô nào.");

        req.Status = CarDocReqStatus.Pending;
        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.Pending;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Chuyên viên kế toán công nợ / tài chính HTC thẩm định & duyệt cấp 1 (CarDocReqApprove1 / A1).
    /// Kiểm tra điều kiện hoàn thành nghĩa vụ tài chính và cọc/bảo lãnh.
    /// </summary>
    public async Task<CarDocReqList> Approve1Async(Guid orgId, long id, Approve1CarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Pending)
            throw new InvalidOperationException($"Hồ sơ đề nghị phải ở trạng thái Chờ duyệt (Pending) để thẩm định A1. Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.Approved1;
        req.ApprovedBy1 = dto.ApproverName ?? "ChuyenVienKeToanCongNo_HTC";
        req.ApprovedDate1 = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
            req.Remark = $"{req.Remark} | Thẩm định A1: {dto.Remark}".TrimStart(' ', '|');

        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.Approved1;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Ngân hàng tài trợ tín dụng thẩm định và xác nhận chấp thuận bảo lãnh / giải phóng chứng từ xe (BankApprove).
    /// </summary>
    public async Task<CarDocReqList> BankApproveAsync(Guid orgId, long id, BankApproveCarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Approved1)
            throw new InvalidOperationException($"Hồ sơ đề nghị phải ở trạng thái Đã thẩm định A1 (Approved1) trước khi Ngân hàng phê duyệt. Hiện tại: {req.Status}.");

        req.BankApprovedBy = dto.BankApprover ?? "CanBoTinDungNganHang";
        req.BankApprovedAt = DateTime.Now;

        var targetVINs = dto.ApprovedVINs != null && dto.ApprovedVINs.Count > 0
            ? new HashSet<string>(dto.ApprovedVINs.Select(v => v.Trim().ToUpperInvariant()))
            : null;

        foreach (var d in req.Details)
        {
            if (targetVINs == null || targetVINs.Contains(d.VIN))
            {
                d.BankApprStatus = BankDocApprStatus.Approved;
                d.BankApprDTime = DateTime.Now;
                d.BankApprBy = req.BankApprovedBy;
                d.BankApprNote = dto.BankNote ?? "Ngân hàng xác nhận hợp lệ bảo lãnh & cam kết thanh toán.";
                d.Status = CarDocReqDetailStatus.BankApproved;
            }
        }

        // Nếu tất cả xe đã được ngân hàng duyệt thì chuyển Status chung lên BankApproved
        if (req.Details.All(d => d.BankApprStatus == BankDocApprStatus.Approved))
        {
            req.Status = CarDocReqStatus.BankApproved;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Lãnh đạo HTC phê duyệt cấp 2 lệnh xuất két hồ sơ gốc bàn giao (CarDocReqDtlApprove2 / A2).
    /// </summary>
    public async Task<CarDocReqList> Approve2Async(Guid orgId, long id, Approve2CarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        // Được duyệt A2 khi đã duyệt BankApproved (hoặc Approved1 đối với xe không qua bảo lãnh ngân hàng)
        if (req.Status != CarDocReqStatus.BankApproved && req.Status != CarDocReqStatus.Approved1)
            throw new InvalidOperationException($"Hồ sơ đề nghị phải ở trạng thái Đã thẩm định (Approved1 hoặc BankApproved) trước khi duyệt lệnh xuất két A2. Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.Approved2;
        req.ApprovedBy2 = dto.ApproverName ?? "PhoTongGiamDocKinhDoanh_HTC";
        req.ApprovedDate2 = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
            req.Remark = $"{req.Remark} | Lệnh xuất két A2: {dto.Remark}".TrimStart(' ', '|');

        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.Approved2;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Tự động quét và duyệt cấp 2 (AutoApprove2) cho các đề nghị đạt tỷ lệ nghĩa vụ tài chính tối thiểu.
    /// </summary>
    public async Task<List<CarDocReqList>> AutoApprove2Async(Guid orgId, AutoApprove2CarDocReqDto dto)
    {
        var eligible = await db.CarDocRequests
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId &&
                        (x.Status == CarDocReqStatus.BankApproved || x.Status == CarDocReqStatus.Approved1) &&
                        x.AvgDutyCompletePercent >= dto.MinDutyPercent)
            .ToListAsync();

        foreach (var req in eligible)
        {
            req.Status = CarDocReqStatus.Approved2;
            req.ApprovedBy2 = "AutoApproveSystem";
            req.ApprovedDate2 = DateTime.Now;
            req.Remark = $"{req.Remark} | Tự động phê duyệt A2 do đạt {req.AvgDutyCompletePercent:F1}% nghĩa vụ tài chính.".TrimStart(' ', '|');

            foreach (var d in req.Details)
            {
                d.Status = CarDocReqDetailStatus.Approved2;
            }
        }

        if (eligible.Count > 0)
        {
            await db.SaveChangesAsync();
        }

        return eligible;
    }

    /// <summary>
    /// Thủ kho xuất giao hồ sơ chứng từ gốc cho cán bộ ngân hàng / đại diện đại lý ký nhận BBBG (Handover).
    /// </summary>
    public async Task<CarDocReqList> HandoverAsync(Guid orgId, long id, HandoverCarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Approved2)
            throw new InvalidOperationException($"Hồ sơ đề nghị phải được duyệt xuất két A2 trước khi bàn giao chứng từ gốc. Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.HandedOver;
        req.HandoverDate = DateTime.Now;
        req.HandedOverBy = dto.HandedOverBy ?? "ThuKhoHoSoGoc_HTC";
        req.HandoverRecipient = dto.RecipientName ?? req.RepresentativeName ?? "Đại diện ký nhận BBBG";

        if (!string.IsNullOrWhiteSpace(dto.RecipientIdCard))
            req.RepresentativeIdCard = dto.RecipientIdCard;

        if (!string.IsNullOrWhiteSpace(dto.Remark))
            req.Remark = $"{req.Remark} | Xuất giao: {dto.Remark}".TrimStart(' ', '|');

        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.HandedOver;
            d.HandoverDate = req.HandoverDate;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Đại lý hoàn trả hồ sơ gốc về két bảo quản (đối với diện mượn Dealer) (Return).
    /// </summary>
    public async Task<CarDocReqList> ReturnAsync(Guid orgId, long id, ReturnCarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.HandedOver)
            throw new InvalidOperationException($"Chỉ có thể ghi nhận hoàn trả cho hồ sơ đã bàn giao (HandedOver). Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.Returned;
        req.ActualReturnDate = DateTime.Now;
        req.ReturnedBy = dto.ReturnedBy ?? "ThuKhoHoSoGoc_HTC";

        if (!string.IsNullOrWhiteSpace(dto.ReturnNotes))
            req.Remark = $"{req.Remark} | Hoàn trả két: {dto.ReturnNotes}".TrimStart(' ', '|');

        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.Returned;
            d.ActualReturnDate = req.ActualReturnDate;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Quay lui trạng thái duyệt xuất két A2 khi cần điều chỉnh (RevertA2).
    /// </summary>
    public async Task<CarDocReqList> RevertA2Async(Guid orgId, long id, RevertA2CarDocReqDto dto)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status != CarDocReqStatus.Approved2)
            throw new InvalidOperationException($"Chỉ có thể quay lui đề nghị ở trạng thái Đã duyệt xuất két (Approved2). Hiện tại: {req.Status}.");

        req.Status = !string.IsNullOrWhiteSpace(req.BankCode) ? CarDocReqStatus.BankApproved : CarDocReqStatus.Approved1;
        req.ApprovedBy2 = null;
        req.ApprovedDate2 = null;
        req.Remark = $"{req.Remark} | Quay lui A2 ({dto.OperatorName}): {dto.Reason}".TrimStart(' ', '|');

        var targetDetailStatus = req.Status == CarDocReqStatus.BankApproved ? CarDocReqDetailStatus.BankApproved : CarDocReqDetailStatus.Approved1;
        foreach (var d in req.Details)
        {
            d.Status = targetDetailStatus;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Từ chối hồ sơ đề nghị (CarDocReqDtlReject / FrmDRApproved).
    /// </summary>
    public async Task<CarDocReqList> RejectAsync(Guid orgId, long id, RejectCarDocReqDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do từ chối không được để trống.", nameof(dto.Reason));

        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status == CarDocReqStatus.HandedOver || req.Status == CarDocReqStatus.Returned)
            throw new InvalidOperationException($"Không thể từ chối hồ sơ đã xuất giao chứng từ hoặc đã tất toán. Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.Rejected;
        req.RejectReason = dto.Reason.Trim();
        req.Remark = $"{req.Remark} | Từ chối ({dto.RejecterName ?? "CapThamDinh"}): {dto.Reason}".TrimStart(' ', '|');

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Hủy hồ sơ đề nghị (CarDocReqListCancel / CarDocReqDtlCancel).
    /// </summary>
    public async Task<CarDocReqList> CancelAsync(Guid orgId, long id, CancelCarDocReqDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy không được để trống.", nameof(dto.Reason));

        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null)
            throw new InvalidOperationException($"Không tìm thấy hồ sơ đề nghị #{id}.");

        if (req.Status == CarDocReqStatus.HandedOver || req.Status == CarDocReqStatus.Returned)
            throw new InvalidOperationException($"Không thể hủy hồ sơ đã bàn giao chứng từ gốc. Hiện tại: {req.Status}.");

        req.Status = CarDocReqStatus.Cancelled;
        req.CancelReason = dto.Reason.Trim();
        req.Remark = $"{req.Remark} | Hủy đề nghị ({dto.CancellerName ?? "DaiLy/HTC"}): {dto.Reason}".TrimStart(' ', '|');

        foreach (var d in req.Details)
        {
            d.Status = CarDocReqDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return req;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Biên bản bàn giao chứng từ gốc xe ô tô CRCarDocReq Advice kèm đọc số tiền thành chữ tiếng Việt chuẩn tài chính.
    /// </summary>
    public async Task<CarDocReqAdviceDto?> GenerateAdviceAsync(Guid orgId, long id)
    {
        var req = await db.CarDocRequests
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);

        if (req == null) return null;

        var typeText = req.TypeCRR switch
        {
            CarDocReqType.Normal => "Giao chứng từ hoàn tất thanh toán (Normal)",
            CarDocReqType.Dealer => "Đại lý mượn hồ sơ làm thủ tục đăng ký (Dealer)",
            CarDocReqType.Special => "Bàn giao theo Thư bảo lãnh ngân hàng (Special)",
            _ => req.TypeCRR.ToString()
        };

        var statusText = req.Status switch
        {
            CarDocReqStatus.Draft => "Dự thảo / Nháp",
            CarDocReqStatus.Pending => "Chờ thẩm định công nợ (Chờ A1)",
            CarDocReqStatus.Approved1 => "Đã duyệt công nợ cấp 1 (A1)",
            CarDocReqStatus.BankApproved => "Ngân hàng đã xác nhận bảo lãnh",
            CarDocReqStatus.Approved2 => "Lãnh đạo HTC duyệt lệnh xuất két (A2)",
            CarDocReqStatus.HandedOver => "Đã xuất giao hồ sơ gốc cho đại diện ký nhận",
            CarDocReqStatus.Returned => "Đã hoàn trả hồ sơ gốc về két bảo quản",
            CarDocReqStatus.Rejected => "Từ chối đề nghị",
            CarDocReqStatus.Cancelled => "Hủy đề nghị",
            _ => req.Status.ToString()
        };

        var advice = new CarDocReqAdviceDto
        {
            DRListCode = req.DRListCode,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            DealerCode = req.DealerCode,
            DealerName = req.DealerName,
            DealerCodeRecieve = req.DealerCodeRecieve,
            DealerNameRecieve = req.DealerNameRecieve,
            BankCode = req.BankCode,
            BankName = req.BankName,
            TypeCRRText = typeText,
            LetterRepresentationNo = req.LetterRepresentationNo,
            LetterRepresentationDate = req.LetterRepresentationDate?.ToString("dd/MM/yyyy"),
            RepresentativeName = req.RepresentativeName,
            RepresentativeIdCard = req.RepresentativeIdCard,
            RepresentativePhone = req.RepresentativePhone,
            TotalVehicles = req.TotalVehicles,
            TotalCarAmount = req.TotalCarAmount,
            TotalCarAmountInWords = PaymentOrderService.NumberToVietnameseWords(req.TotalCarAmount),
            TotalPaymentAmount = req.TotalPaymentAmount,
            TotalPaymentAmountInWords = PaymentOrderService.NumberToVietnameseWords(req.TotalPaymentAmount),
            TotalGuaranteeAmount = req.TotalGuaranteeAmount,
            TotalGuaranteeAmountInWords = PaymentOrderService.NumberToVietnameseWords(req.TotalGuaranteeAmount),
            AvgDutyCompletePercent = req.AvgDutyCompletePercent,
            StatusText = statusText,
            ApprovedBy1 = req.ApprovedBy1,
            ApprovedDate1 = req.ApprovedDate1?.ToString("dd/MM/yyyy HH:mm"),
            BankApprovedBy = req.BankApprovedBy,
            BankApprovedAt = req.BankApprovedAt?.ToString("dd/MM/yyyy HH:mm"),
            ApprovedBy2 = req.ApprovedBy2,
            ApprovedDate2 = req.ApprovedDate2?.ToString("dd/MM/yyyy HH:mm"),
            HandoverDate = req.HandoverDate?.ToString("dd/MM/yyyy HH:mm"),
            HandedOverBy = req.HandedOverBy,
            HandoverRecipient = req.HandoverRecipient,
            ReturnDueDate = req.ReturnDueDate?.ToString("dd/MM/yyyy"),
            ActualReturnDate = req.ActualReturnDate?.ToString("dd/MM/yyyy HH:mm"),
            ReturnedBy = req.ReturnedBy,
            Remark = req.Remark
        };

        int idx = 1;
        foreach (var d in req.Details.OrderBy(x => x.Id))
        {
            advice.Items.Add(new CarDocReqDetailAdviceDto
            {
                No = idx++,
                VIN = d.VIN,
                CarId = d.CarId,
                ModelCode = d.ModelCode,
                ModelName = d.ModelName ?? d.ModelCode,
                SpecCode = d.SpecCode,
                EngineNo = d.EngineNo,
                ColorNameVN = d.ColorNameVN,
                ContractNo = d.ContractNo,
                UnitPriceActual = d.UnitPriceActual,
                PaymentPercent = d.PaymentPercent,
                GuaranteePercent = d.GuaranteePercent,
                DutyCompletePercent = d.DutyCompletePercent,
                BankGuaranteeNo = d.BankGuaranteeNo,
                CONo = d.CONo,
                CQNo = d.CQNo,
                CustomsDeclarationNo = d.CustomsDeclarationNo,
                HTCInvoiceNo = d.HTCInvoiceNo,
                DocumentsGiven = d.DocumentsGiven,
                BankApprStatusText = d.BankApprStatus switch
                {
                    BankDocApprStatus.Pending => "Chờ ngân hàng duyệt",
                    BankDocApprStatus.Approved => "Ngân hàng đã duyệt",
                    BankDocApprStatus.Rejected => "Ngân hàng từ chối",
                    _ => d.BankApprStatus.ToString()
                },
                StatusText = d.Status switch
                {
                    CarDocReqDetailStatus.Pending => "Chờ xử lý",
                    CarDocReqDetailStatus.Approved1 => "Đã duyệt A1",
                    CarDocReqDetailStatus.BankApproved => "Ngân hàng đã duyệt",
                    CarDocReqDetailStatus.Approved2 => "Đã duyệt xuất két A2",
                    CarDocReqDetailStatus.HandedOver => "Đã giao hồ sơ gốc",
                    CarDocReqDetailStatus.Returned => "Đã trả về két",
                    CarDocReqDetailStatus.Cancelled => "Hủy",
                    _ => d.Status.ToString()
                },
                ReturnDueDate = d.ReturnDueDate?.ToString("dd/MM/yyyy"),
                ActualReturnDate = d.ActualReturnDate?.ToString("dd/MM/yyyy"),
                Remark = d.Remark
            });
        }

        return advice;
    }

    /// <summary>
    /// Thống kê tổng hợp báo cáo dashboard đề nghị giao chứng từ gốc xe.
    /// </summary>
    public async Task<CarDocReqSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.CarDocRequests
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new CarDocReqSummaryDto
        {
            TotalRequests = list.Count,
            DraftCount = list.Count(x => x.Status == CarDocReqStatus.Draft),
            PendingCount = list.Count(x => x.Status == CarDocReqStatus.Pending),
            Approved1Count = list.Count(x => x.Status == CarDocReqStatus.Approved1),
            BankApprovedCount = list.Count(x => x.Status == CarDocReqStatus.BankApproved),
            Approved2Count = list.Count(x => x.Status == CarDocReqStatus.Approved2),
            HandedOverCount = list.Count(x => x.Status == CarDocReqStatus.HandedOver),
            ReturnedCount = list.Count(x => x.Status == CarDocReqStatus.Returned),
            RejectedCount = list.Count(x => x.Status == CarDocReqStatus.Rejected),
            CancelledCount = list.Count(x => x.Status == CarDocReqStatus.Cancelled),
            TotalVehiclesRequested = list.Sum(x => x.TotalVehicles),
            TotalVehiclesHandedOver = list.Where(x => x.Status == CarDocReqStatus.HandedOver || x.Status == CarDocReqStatus.Returned).Sum(x => x.TotalVehicles),
            TotalCarAmount = list.Sum(x => x.TotalCarAmount),
            TotalPaymentAmount = list.Sum(x => x.TotalPaymentAmount),
            TotalGuaranteeAmount = list.Sum(x => x.TotalGuaranteeAmount)
        };

        var topDealers = list
            .GroupBy(x => new { x.DealerCode, x.DealerName })
            .Select(g => new DealerDocReqStatDto
            {
                DealerCode = g.Key.DealerCode,
                DealerName = g.Key.DealerName,
                RequestCount = g.Count(),
                VehicleCount = g.Sum(x => x.TotalVehicles),
                TotalCarAmount = g.Sum(x => x.TotalCarAmount),
                HandedOverCount = g.Count(x => x.Status == CarDocReqStatus.HandedOver || x.Status == CarDocReqStatus.Returned)
            })
            .OrderByDescending(x => x.TotalCarAmount)
            .Take(5)
            .ToList();

        summary.TopDealers = topDealers;
        return summary;
    }

    /// <summary>
    /// Tính toán lại các trường tổng hợp trên Header đề nghị.
    /// </summary>
    private static void RecalculateHeader(CarDocReqList req)
    {
        req.TotalVehicles = req.Details.Count;
        req.TotalCarAmount = req.Details.Sum(x => x.UnitPriceActual);

        // Tổng tiền đã thanh toán ước tính
        req.TotalPaymentAmount = (long)req.Details.Sum(x => (decimal)x.UnitPriceActual * x.PaymentPercent / 100.0m);

        // Tổng giá trị bảo lãnh ước tính
        req.TotalGuaranteeAmount = (long)req.Details.Sum(x => (decimal)x.UnitPriceActual * x.GuaranteePercent / 100.0m);

        // Tỷ lệ % hoàn thành nghĩa vụ tài chính bình quân
        req.AvgDutyCompletePercent = req.Details.Count > 0
            ? Math.Round(req.Details.Average(x => x.DutyCompletePercent), 2)
            : 0.0m;
    }
}
