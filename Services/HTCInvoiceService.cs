using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Bảng kê Hóa đơn Giá trị Gia tăng (VAT E-Invoice) Bán Buôn Xe Ô Tô Cho Đại Lý & Đồng Bộ Quyết Toán Doanh Thu Thuế.
/// Tương ứng VAT_HTCInvoice, VAT_HTCInvoiceDetail, VAT_TCGInvoice trong TERP.BizHTC/BizHTC.InvoiceHTC_TCG.cs,
/// CommonSQLQuery.cs (mySql_VAT_HTCInvoice_GetX_New20190816), PrintVATService.cs, HTCInvoice.cs, HTCInvoiceDetail.cs
/// và các màn hình FrmMngHTCInvoice, FrmNewHTCInvoice, FrmDetailHTCInvoice, FrmImportNewHTCInvoice, FrmThuHoiHDHTC trong TERP.HTCClient/Views/Sales/PrintVAT hệ nguồn HTC 2010.
/// </summary>
public sealed class HTCInvoiceService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách hóa đơn GTGT bán buôn xe ô tô kèm bộ lọc.
    /// </summary>
    public async Task<List<HTCInvoice>> GetListAsync(
        Guid orgId,
        string? status = null,
        string? dealer = null,
        string? bank = null,
        string? source = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.HTCInvoices
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HTCInvoiceStatus>(status, true, out var st))
        {
            q = q.Where(x => x.Status == st);
        }

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

        if (!string.IsNullOrWhiteSpace(source) && Enum.TryParse<HTCInvoiceSource>(source, true, out var src))
        {
            q = q.Where(x => x.SourceInvoiceCode == src);
        }

        if (fromDate.HasValue)
        {
            var fd = fromDate.Value.Date;
            q = q.Where(x => x.InvoiceDate >= fd);
        }

        if (toDate.HasValue)
        {
            var td = toDate.Value.Date.AddDays(1);
            q = q.Where(x => x.InvoiceDate < td);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.HTCInvoiceCode.Contains(s)
                || (x.HTCInvoiceNo != null && x.HTCInvoiceNo.Contains(s))
                || x.InvoiceSymbol.Contains(s)
                || x.DealerCode.Contains(s)
                || x.DealerName.ToUpper().Contains(s)
                || (x.OS_HDDT_InvoiceCode != null && x.OS_HDDT_InvoiceCode.Contains(s))
                || x.Details.Any(d => d.VIN.Contains(s) || d.ModelCode.Contains(s)));
        }

        return await q.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Id).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết hóa đơn theo ID.
    /// </summary>
    public async Task<HTCInvoice?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.HTCInvoices
            .Include(x => x.Details.OrderBy(d => d.ItemNo))
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>
    /// Lấy chi tiết hóa đơn theo mã hóa đơn hệ thống.
    /// </summary>
    public async Task<HTCInvoice?> GetByCodeAsync(Guid orgId, string code)
    {
        string c = code.Trim().ToUpperInvariant();
        return await db.HTCInvoices
            .Include(x => x.Details.OrderBy(d => d.ItemNo))
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.HTCInvoiceCode.ToUpper() == c);
    }

    /// <summary>
    /// Lập hóa đơn GTGT bán buôn xe ô tô mới (Draft / Chờ thẩm định).
    /// </summary>
    public async Task<HTCInvoice> CreateInvoiceAsync(Guid orgId, CreateHTCInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            throw new ArgumentException("Mã đại lý không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.BuyerTaxCode))
            throw new ArgumentException("Mã số thuế đại lý không được để trống.");

        var now = DateTime.UtcNow;
        var invoiceDate = dto.InvoiceDate ?? DateTime.UtcNow;

        // Sinh mã hóa đơn hệ thống: HD-HTC-yyyyMM-xxx
        string prefix = $"HD-HTC-{invoiceDate:yyyyMM}-";
        int seq = await db.HTCInvoices
            .Where(x => x.OrgId == orgId && x.HTCInvoiceCode.StartsWith(prefix))
            .CountAsync() + 1;
        string invoiceCode = $"{prefix}{seq:D3}";

        var invoice = new HTCInvoice
        {
            OrgId = orgId,
            HTCInvoiceCode = invoiceCode,
            InvoiceSymbol = !string.IsNullOrWhiteSpace(dto.InvoiceSymbol) ? dto.InvoiceSymbol.Trim() : "1C25THC",
            InvoiceDate = invoiceDate,
            DealerCode = dto.DealerCode.Trim().ToUpperInvariant(),
            DealerName = !string.IsNullOrWhiteSpace(dto.DealerName) ? dto.DealerName.Trim() : dto.DealerCode.Trim(),
            BuyerTaxCode = dto.BuyerTaxCode.Trim(),
            BuyerAddress = dto.BuyerAddress.Trim(),
            BuyerLegalRepresentative = dto.BuyerLegalRepresentative?.Trim(),
            PaymentMethod = !string.IsNullOrWhiteSpace(dto.PaymentMethod) ? dto.PaymentMethod.Trim() : "CK",
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BankName = dto.BankName?.Trim(),
            BankAccountNo = dto.BankAccountNo?.Trim(),
            SourceInvoiceCode = dto.SourceInvoiceCode,
            Root_HTCInvoiceNo = dto.Root_HTCInvoiceNo?.Trim(),
            Status = HTCInvoiceStatus.Draft,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "KeToanBanHang_HTC",
            CreatedAt = now,
            Remark = dto.Remark?.Trim()
        };

        long totalAmount = 0;
        long totalVat = 0;
        int itemNo = 1;

        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.VIN)) continue;
                string vin = item.VIN.Trim().ToUpperInvariant();

                decimal vatRate = item.VATRate ?? 10.0m;
                long vatAmt = (long)Math.Round(item.UnitPrice * vatRate / 100m, MidpointRounding.AwayFromZero);
                long totalLine = item.UnitPrice + vatAmt;

                totalAmount += item.UnitPrice;
                totalVat += vatAmt;

                invoice.Details.Add(new HTCInvoiceDetail
                {
                    OrgId = orgId,
                    HTCInvoiceCode = invoiceCode,
                    ItemNo = itemNo++,
                    CarId = item.CarId?.Trim(),
                    VIN = vin,
                    ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                    ModelName = !string.IsNullOrWhiteSpace(item.ModelName) ? item.ModelName.Trim() : item.ModelCode.Trim(),
                    SpecCode = item.SpecCode?.Trim(),
                    EngineNo = item.EngineNo?.Trim(),
                    ColorVN = item.ColorVN?.Trim(),
                    ProductionYear = !string.IsNullOrWhiteSpace(item.ProductionYear) ? item.ProductionYear.Trim() : "2025",
                    CabinCONo = item.CabinCONo?.Trim(),
                    CQNo = item.CQNo?.Trim(),
                    CustomsDeclarationNo = item.CustomsDeclarationNo?.Trim(),
                    SOCode = item.SOCode?.Trim(),
                    UnitPrice = item.UnitPrice,
                    VATRate = vatRate,
                    VATAmount = vatAmt,
                    TotalPrice = totalLine,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = item.Remark?.Trim()
                });
            }
        }

        invoice.TotalVehicles = invoice.Details.Count;
        invoice.TotalAmount = totalAmount;
        invoice.VATRate = 10.0m;
        invoice.VATAmount = totalVat;
        invoice.TotalPayment = totalAmount + totalVat;
        invoice.PaidAmount = 0;
        invoice.RemainAmount = invoice.TotalPayment;

        db.HTCInvoices.Add(invoice);
        await db.SaveChangesAsync();

        return invoice;
    }

    /// <summary>
    /// Bổ sung xe vào hóa đơn khi còn ở trạng thái Draft.
    /// </summary>
    public async Task<HTCInvoice> AddCarToInvoiceAsync(Guid orgId, long invoiceId, AddCarToHTCInvoiceDto dto)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status != HTCInvoiceStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể thêm xe vào hóa đơn khi ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {invoice.Status}.");

        string vin = dto.Item.VIN.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(vin))
            throw new ArgumentException("Số khung VIN không được để trống.");

        if (invoice.Details.Any(d => d.VIN == vin && d.Status == HTCInvoiceDetailStatus.Active))
            throw new InvalidOperationException($"Số khung {vin} đã tồn tại trong hóa đơn này.");

        decimal vatRate = dto.Item.VATRate ?? invoice.VATRate;
        long vatAmt = (long)Math.Round(dto.Item.UnitPrice * vatRate / 100m, MidpointRounding.AwayFromZero);
        long totalLine = dto.Item.UnitPrice + vatAmt;

        int nextNo = invoice.Details.Count > 0 ? invoice.Details.Max(d => d.ItemNo) + 1 : 1;

        invoice.Details.Add(new HTCInvoiceDetail
        {
            OrgId = orgId,
            InvoiceId = invoice.Id,
            HTCInvoiceCode = invoice.HTCInvoiceCode,
            ItemNo = nextNo,
            CarId = dto.Item.CarId?.Trim(),
            VIN = vin,
            ModelCode = dto.Item.ModelCode.Trim().ToUpperInvariant(),
            ModelName = !string.IsNullOrWhiteSpace(dto.Item.ModelName) ? dto.Item.ModelName.Trim() : dto.Item.ModelCode.Trim(),
            SpecCode = dto.Item.SpecCode?.Trim(),
            EngineNo = dto.Item.EngineNo?.Trim(),
            ColorVN = dto.Item.ColorVN?.Trim(),
            ProductionYear = !string.IsNullOrWhiteSpace(dto.Item.ProductionYear) ? dto.Item.ProductionYear.Trim() : "2025",
            CabinCONo = dto.Item.CabinCONo?.Trim(),
            CQNo = dto.Item.CQNo?.Trim(),
            CustomsDeclarationNo = dto.Item.CustomsDeclarationNo?.Trim(),
            SOCode = dto.Item.SOCode?.Trim(),
            UnitPrice = dto.Item.UnitPrice,
            VATRate = vatRate,
            VATAmount = vatAmt,
            TotalPrice = totalLine,
            Status = HTCInvoiceDetailStatus.Active,
            Remark = dto.Item.Remark?.Trim()
        });

        RecalculateTotals(invoice);
        invoice.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Xóa xe khỏi hóa đơn khi còn ở trạng thái Draft.
    /// </summary>
    public async Task<HTCInvoice> RemoveCarFromInvoiceAsync(Guid orgId, long invoiceId, long detailId)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status != HTCInvoiceStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa xe khi hóa đơn ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {invoice.Status}.");

        var detail = invoice.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new KeyNotFoundException($"Không tìm thấy dòng xe chi tiết ID {detailId}.");

        invoice.Details.Remove(detail);
        db.HTCInvoiceDetails.Remove(detail);

        // Đánh lại số thứ tự dòng
        int no = 1;
        foreach (var d in invoice.Details.OrderBy(x => x.ItemNo))
        {
            d.ItemNo = no++;
        }

        RecalculateTotals(invoice);
        invoice.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Kế toán trưởng thẩm định & phê duyệt hóa đơn (Draft -> Approved).
    /// </summary>
    public async Task<HTCInvoice> ApproveInvoiceAsync(Guid orgId, long invoiceId, ApproveHTCInvoiceDto dto)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status != HTCInvoiceStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể phê duyệt hóa đơn ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {invoice.Status}.");

        if (invoice.Details.Count == 0)
            throw new InvalidOperationException("Hóa đơn phải có ít nhất 1 dòng xe mới được phép phê duyệt.");

        invoice.Status = HTCInvoiceStatus.Approved;
        invoice.ApprovedBy = !string.IsNullOrWhiteSpace(dto.ApproverName) ? dto.ApproverName.Trim() : "KeToanTruong_HTC";
        invoice.ApprovedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
        {
            invoice.Remark = string.IsNullOrWhiteSpace(invoice.Remark) ? dto.Remark.Trim() : $"{invoice.Remark} | {dto.Remark.Trim()}";
        }
        invoice.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Phát hành hóa đơn GTGT điện tử & ký số CA chính thức (Approved -> Issued).
    /// Gán số hóa đơn HTCInvoiceNo chuẩn 7 chữ số và mã tra cứu HĐĐT Cục Thuế.
    /// </summary>
    public async Task<HTCInvoice> IssueInvoiceAsync(Guid orgId, long invoiceId, IssueHTCInvoiceDto dto)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status != HTCInvoiceStatus.Approved)
            throw new InvalidOperationException($"Chỉ có thể phát hành hóa đơn đã được Kế toán trưởng phê duyệt (Approved). Trạng thái hiện tại: {invoice.Status}.");

        var now = DateTime.UtcNow;

        // Sinh số hóa đơn GTGT chính thức: 7 chữ số (ví dụ: 0012891)
        string invoiceNo;
        if (!string.IsNullOrWhiteSpace(dto.CustomInvoiceNo))
        {
            invoiceNo = dto.CustomInvoiceNo.Trim();
        }
        else
        {
            int maxSeq = await db.HTCInvoices
                .Where(x => x.OrgId == orgId && x.HTCInvoiceNo != null)
                .CountAsync() + 12890;
            invoiceNo = $"{maxSeq:D7}";
        }

        // Sinh mã tra cứu hóa đơn điện tử TVAN / Thuế
        string hddtCode = $"EINV-{now:yyyyMM}-HTC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        invoice.HTCInvoiceNo = invoiceNo;
        invoice.OS_HDDT_InvoiceCode = hddtCode;
        invoice.Status = HTCInvoiceStatus.Issued;
        invoice.IssuedBy = !string.IsNullOrWhiteSpace(dto.IssuerName) ? dto.IssuerName.Trim() : "GiamDocTaiChinh_HTC";
        invoice.IssuedDate = now;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
        {
            invoice.Remark = string.IsNullOrWhiteSpace(invoice.Remark) ? dto.Remark.Trim() : $"{invoice.Remark} | {dto.Remark.Trim()}";
        }
        invoice.UpdatedAt = now;

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Ghi nhận thanh toán tiền bán xe từ đại lý (liên kết UNC ngân hàng / Phiếu thanh toán).
    /// Cập nhật số tiền đã thu và số dư nợ còn lại, tự động chuyển Settled khi thanh toán đủ 100%.
    /// </summary>
    public async Task<HTCInvoice> RecordPaymentAsync(Guid orgId, long invoiceId, RecordHTCInvoicePaymentDto dto)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status != HTCInvoiceStatus.Issued && invoice.Status != HTCInvoiceStatus.PartiallyPaid)
            throw new InvalidOperationException($"Chỉ có thể ghi nhận thanh toán cho hóa đơn Đã phát hành hoặc Đã thanh toán một phần. Trạng thái hiện tại: {invoice.Status}.");

        if (dto.Amount <= 0)
            throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0 VNĐ.");

        if (dto.Amount > invoice.RemainAmount)
            throw new InvalidOperationException($"Số tiền thanh toán ({dto.Amount:N0} đ) vượt quá số nợ còn lại của hóa đơn ({invoice.RemainAmount:N0} đ).");

        invoice.PaidAmount += dto.Amount;
        invoice.RemainAmount = invoice.TotalPayment - invoice.PaidAmount;

        if (invoice.RemainAmount == 0)
        {
            invoice.Status = HTCInvoiceStatus.Settled;
        }
        else
        {
            invoice.Status = HTCInvoiceStatus.PartiallyPaid;
        }

        string payNote = $"TT {dto.Amount:N0} đ qua UNC {dto.PaymentRef} ({dto.BankCode ?? "NH"}) ngày {DateTime.UtcNow:dd/MM/yyyy HH:mm}";
        invoice.Remark = string.IsNullOrWhiteSpace(invoice.Remark) ? payNote : $"{invoice.Remark} | {payNote}";
        invoice.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Thu hồi / Hủy hóa đơn GTGT theo thỏa thuận 2 bên (tương ứng FrmThuHoiHDHTC.cs).
    /// </summary>
    public async Task<HTCInvoice> RevokeInvoiceAsync(Guid orgId, long invoiceId, RevokeHTCInvoiceDto dto)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        if (invoice.Status == HTCInvoiceStatus.Cancelled)
            throw new InvalidOperationException("Hóa đơn đã ở trạng thái Thu hồi / Hủy trước đó.");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do thu hồi / hủy hóa đơn không được để trống.");

        invoice.Status = HTCInvoiceStatus.Cancelled;
        invoice.Adj_DeleteReason = $"BB số: {dto.CancellationMinutesNo.Trim()} - Lý do: {dto.Reason.Trim()} (Người thu hồi: {dto.RevokedBy ?? "KeToanTruong_HTC"})";
        invoice.UpdatedAt = DateTime.UtcNow;

        foreach (var d in invoice.Details)
        {
            d.Status = HTCInvoiceDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return invoice;
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Hóa Đơn Giá Trị Gia Tăng Điện Tử chuẩn Bộ Tài chính (TT78/2021/TT-BTC) và HTC.
    /// Kèm thuật toán đọc số tiền thành chữ tiếng Việt chuẩn quy chuẩn kế toán.
    /// </summary>
    public async Task<HTCInvoiceAdviceDto> GenerateInvoiceAdviceAsync(Guid orgId, long invoiceId)
    {
        var invoice = await db.HTCInvoices
            .Include(x => x.Details.OrderBy(d => d.ItemNo))
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn ID {invoiceId}.");

        string statusText = invoice.Status switch
        {
            HTCInvoiceStatus.Draft => "Dự thảo (Chờ duyệt)",
            HTCInvoiceStatus.Approved => "Đã duyệt thẩm định",
            HTCInvoiceStatus.Issued => "Đã phát hành điện tử",
            HTCInvoiceStatus.PartiallyPaid => "Đã thanh toán một phần",
            HTCInvoiceStatus.Settled => "Đã quyết toán dứt điểm",
            HTCInvoiceStatus.Adjusted => "Hóa đơn điều chỉnh",
            HTCInvoiceStatus.Cancelled => "Đã thu hồi / Hủy bỏ",
            _ => invoice.Status.ToString()
        };

        var advice = new HTCInvoiceAdviceDto
        {
            HTCInvoiceCode = invoice.HTCInvoiceCode,
            HTCInvoiceNo = invoice.HTCInvoiceNo ?? "CHƯA PHÁT HÀNH",
            InvoiceSymbol = invoice.InvoiceSymbol,
            InvoiceDateStr = invoice.InvoiceDate.ToString("dd/MM/yyyy"),
            DealerCode = invoice.DealerCode,
            DealerName = invoice.DealerName,
            BuyerTaxCode = invoice.BuyerTaxCode,
            BuyerAddress = invoice.BuyerAddress,
            BuyerLegalRepresentative = invoice.BuyerLegalRepresentative,
            PaymentMethod = invoice.PaymentMethod,
            SellerBankAccount = $"{invoice.BankAccountNo} tại {invoice.BankName ?? "VietinBank"}",
            TotalVehicles = invoice.TotalVehicles,
            TotalAmount = invoice.TotalAmount,
            TotalAmountInWords = NumberToVietnameseText(invoice.TotalAmount),
            VATRate = invoice.VATRate,
            VATAmount = invoice.VATAmount,
            VATAmountInWords = NumberToVietnameseText(invoice.VATAmount),
            TotalPayment = invoice.TotalPayment,
            TotalPaymentInWords = NumberToVietnameseText(invoice.TotalPayment),
            PaidAmount = invoice.PaidAmount,
            RemainAmount = invoice.RemainAmount,
            OS_HDDT_InvoiceCode = invoice.OS_HDDT_InvoiceCode,
            StatusText = statusText,
            ApprovedBy = invoice.ApprovedBy,
            ApprovedDateStr = invoice.ApprovedDate?.ToString("dd/MM/yyyy HH:mm"),
            IssuedBy = invoice.IssuedBy,
            IssuedDateStr = invoice.IssuedDate?.ToString("dd/MM/yyyy HH:mm"),
            DigitalSignatureHTV = invoice.Status >= HTCInvoiceStatus.Issued
                ? $"Được ký số bởi CÔNG TY CỔ PHẦN HYUNDAI THÀNH CÔNG VIỆT NAM vào ngày {invoice.IssuedDate:dd/MM/yyyy HH:mm:ss} (Chứng thư số Viettel-CA, Serial: 5404B678A0091)"
                : null,
            Remark = invoice.Remark
        };

        foreach (var d in invoice.Details)
        {
            string lineStatus = d.Status switch
            {
                HTCInvoiceDetailStatus.Active => "Có hiệu lực",
                HTCInvoiceDetailStatus.Adjusted => "Điều chỉnh",
                HTCInvoiceDetailStatus.Cancelled => "Đã hủy",
                _ => d.Status.ToString()
            };

            advice.Items.Add(new HTCInvoiceDetailAdviceDto
            {
                ItemNo = d.ItemNo,
                VIN = d.VIN,
                CarId = d.CarId,
                ModelName = d.ModelName,
                SpecCode = d.SpecCode,
                EngineNo = d.EngineNo,
                ColorVN = d.ColorVN,
                ProductionYear = d.ProductionYear,
                CabinCONo = d.CabinCONo,
                CQNo = d.CQNo,
                SOCode = d.SOCode,
                UnitPrice = d.UnitPrice,
                VATRate = d.VATRate,
                VATAmount = d.VATAmount,
                TotalPrice = d.TotalPrice,
                StatusText = lineStatus
            });
        }

        return advice;
    }

    /// <summary>
    /// Báo cáo thống kê tổng hợp hóa đơn GTGT bán buôn xe.
    /// </summary>
    public async Task<HTCInvoiceSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var invoices = await db.HTCInvoices
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new HTCInvoiceSummaryDto
        {
            TotalInvoices = invoices.Count,
            DraftCount = invoices.Count(x => x.Status == HTCInvoiceStatus.Draft),
            ApprovedCount = invoices.Count(x => x.Status == HTCInvoiceStatus.Approved),
            IssuedCount = invoices.Count(x => x.Status == HTCInvoiceStatus.Issued),
            PartiallyPaidCount = invoices.Count(x => x.Status == HTCInvoiceStatus.PartiallyPaid),
            SettledCount = invoices.Count(x => x.Status == HTCInvoiceStatus.Settled),
            CancelledCount = invoices.Count(x => x.Status == HTCInvoiceStatus.Cancelled),
            TotalVehicles = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.TotalVehicles),
            TotalAmount = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.TotalAmount),
            TotalVATAmount = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.VATAmount),
            TotalPayment = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.TotalPayment),
            TotalPaidAmount = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.PaidAmount),
            TotalRemainAmount = invoices.Where(x => x.Status != HTCInvoiceStatus.Cancelled).Sum(x => x.RemainAmount)
        };

        var topDealers = invoices
            .Where(x => x.Status != HTCInvoiceStatus.Cancelled)
            .GroupBy(x => new { x.DealerCode, x.DealerName })
            .Select(g => new DealerHTCInvoiceStatDto
            {
                DealerCode = g.Key.DealerCode,
                DealerName = g.Key.DealerName,
                InvoiceCount = g.Count(),
                VehicleCount = g.Sum(x => x.TotalVehicles),
                TotalPayment = g.Sum(x => x.TotalPayment),
                PaidAmount = g.Sum(x => x.PaidAmount),
                RemainAmount = g.Sum(x => x.RemainAmount)
            })
            .OrderByDescending(x => x.TotalPayment)
            .Take(5)
            .ToList();

        summary.TopDealers = topDealers;
        return summary;
    }

    private static void RecalculateTotals(HTCInvoice invoice)
    {
        var activeItems = invoice.Details.Where(d => d.Status != HTCInvoiceDetailStatus.Cancelled).ToList();
        invoice.TotalVehicles = activeItems.Count;
        invoice.TotalAmount = activeItems.Sum(d => d.UnitPrice);
        invoice.VATAmount = activeItems.Sum(d => d.VATAmount);
        invoice.TotalPayment = invoice.TotalAmount + invoice.VATAmount;
        invoice.RemainAmount = Math.Max(0, invoice.TotalPayment - invoice.PaidAmount);
    }

    /// <summary>
    /// Thuật toán đọc số tiền thành chữ tiếng Việt chuẩn tài chính kế toán ngân hàng HTC.
    /// </summary>
    public static string NumberToVietnameseText(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseText(-number);

        string[] units = ["", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ"];
        string result = "";
        int unitIndex = 0;

        long temp = number;
        while (temp > 0)
        {
            long group = temp % 1000;
            if (group > 0)
            {
                string groupText = ReadThreeDigits((int)group, temp >= 1000);
                result = groupText + units[unitIndex] + (result.Length > 0 ? " " + result : "");
            }
            temp /= 1000;
            unitIndex++;
        }

        result = result.Trim();
        if (result.Length > 0)
        {
            result = char.ToUpper(result[0]) + result[1..] + " đồng chẵn.";
        }
        return result;
    }

    private static string ReadThreeDigits(int n, bool hasHigherGroups)
    {
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
        int hundreds = n / 100;
        int tens = (n % 100) / 10;
        int ones = n % 10;
        string res = "";

        if (hundreds > 0 || hasHigherGroups)
        {
            res += digits[hundreds] + " trăm";
        }

        if (tens > 1)
        {
            res += (res.Length > 0 ? " " : "") + digits[tens] + " mươi";
            if (ones == 1) res += " mốt";
            else if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else if (tens == 1)
        {
            res += (res.Length > 0 ? " " : "") + "mười";
            if (ones == 5) res += " lăm";
            else if (ones > 0) res += " " + digits[ones];
        }
        else if (ones > 0)
        {
            if (hundreds > 0 || hasHigherGroups) res += " lẻ";
            res += " " + digits[ones];
        }

        return res.Trim();
    }
}
