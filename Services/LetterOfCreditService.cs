using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Thư Tín Dụng Nhập Khẩu (Letter of Credit - LC) thanh toán hợp đồng ngoại.
/// Tương ứng CT_LC, CT_ContractOversea, CT_PackingList trong TERP.BizHTC/BizHTC.Contract.cs
/// (ContractLCGet, ContractLCCreate, ContractLCDelete) và các màn hình FrmMngLC, FrmNewLC
/// trong TERP.HTCClient/Views/Sales hệ nguồn HTC 2010.
/// </summary>
public sealed class LetterOfCreditService(AppDbContext db)
{
    /// <summary>
    /// Lấy danh sách LC kèm bộ lọc (số LC, ngân hàng, hợp đồng ngoại, ngày tạo, trạng thái).
    /// </summary>
    public async Task<List<LetterOfCredit>> GetListAsync(
        Guid orgId,
        string? status = null,
        string? bank = null,
        string? contractNo = null,
        string? query = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.LettersOfCredit
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LCStatus>(status, true, out var st))
            q = q.Where(x => x.Status == st);

        if (!string.IsNullOrWhiteSpace(bank))
        {
            string b = bank.Trim().ToUpperInvariant();
            q = q.Where(x => (x.BankCode != null && x.BankCode.ToUpper().Contains(b))
                || x.BankName.ToUpper().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(contractNo))
        {
            string c = contractNo.Trim().ToUpperInvariant();
            q = q.Where(x => x.ContractNo.ToUpper().Contains(c));
        }

        if (fromDate.HasValue)
        {
            var fd = fromDate.Value.Date;
            q = q.Where(x => x.DateOpen >= fd);
        }

        if (toDate.HasValue)
        {
            var td = toDate.Value.Date.AddDays(1);
            q = q.Where(x => x.DateOpen < td);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToUpperInvariant();
            q = q.Where(x => x.LCNo.ToUpper().Contains(s)
                || x.ContractNo.ToUpper().Contains(s)
                || x.BankName.ToUpper().Contains(s)
                || (x.BeneficiaryName != null && x.BeneficiaryName.ToUpper().Contains(s))
                || (x.PackingListNo != null && x.PackingListNo.ToUpper().Contains(s))
                || x.Details.Any(d => d.VIN.ToUpper().Contains(s) || d.ModelCode.ToUpper().Contains(s)));
        }

        return await q.OrderByDescending(x => x.DateOpen).ThenByDescending(x => x.Id).ToListAsync();
    }

    /// <summary>Lấy chi tiết LC theo ID.</summary>
    public async Task<LetterOfCredit?> GetByIdAsync(Guid orgId, long id)
    {
        return await db.LettersOfCredit
            .Include(x => x.Details.OrderBy(d => d.ItemNo))
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == id);
    }

    /// <summary>Lấy chi tiết LC theo số LC.</summary>
    public async Task<LetterOfCredit?> GetByNoAsync(Guid orgId, string lcNo)
    {
        string c = lcNo.Trim().ToUpperInvariant();
        return await db.LettersOfCredit
            .Include(x => x.Details.OrderBy(d => d.ItemNo))
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.LCNo.ToUpper() == c);
    }

    /// <summary>
    /// Mở (lập) Thư tín dụng nhập khẩu mới cho hợp đồng ngoại (tương ứng ContractLCCreate / FrmNewLC).
    /// </summary>
    public async Task<LetterOfCredit> CreateAsync(Guid orgId, CreateLetterOfCreditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ContractNo))
            throw new ArgumentException("Số hợp đồng ngoại không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.BankName))
            throw new ArgumentException("Tên ngân hàng phát hành LC không được để trống.");
        if (dto.LCAmount <= 0)
            throw new ArgumentException("Giá trị LC phải lớn hơn 0.");

        var now = DateTime.UtcNow;
        var dateOpen = dto.DateOpen ?? DateTime.Today;

        // Sinh số LC nếu không chỉ định: LC-yyyyMM-xxx
        string lcNo;
        if (!string.IsNullOrWhiteSpace(dto.LCNo))
        {
            lcNo = dto.LCNo.Trim().ToUpperInvariant();
            if (await db.LettersOfCredit.AnyAsync(x => x.OrgId == orgId && x.LCNo == lcNo))
                throw new InvalidOperationException($"Số LC {lcNo} đã tồn tại.");
        }
        else
        {
            string prefix = $"LC-{dateOpen:yyyyMM}-";
            int seq = await db.LettersOfCredit
                .Where(x => x.OrgId == orgId && x.LCNo.StartsWith(prefix))
                .CountAsync() + 1;
            lcNo = $"{prefix}{seq:D3}";
        }

        decimal rate = dto.ExchangeRate ?? 1m;
        int termDays = dto.TermDays ?? (dto.DateExpired.HasValue ? (dto.DateExpired.Value.Date - dateOpen.Date).Days : 0);

        var lc = new LetterOfCredit
        {
            OrgId = orgId,
            LCNo = lcNo,
            ContractNo = dto.ContractNo.Trim().ToUpperInvariant(),
            BankName = dto.BankName.Trim(),
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BeneficiaryName = dto.BeneficiaryName?.Trim(),
            BeneficiaryCountry = dto.BeneficiaryCountry?.Trim(),
            ApplicantName = !string.IsNullOrWhiteSpace(dto.ApplicantName) ? dto.ApplicantName.Trim() : "CÔNG TY CỔ PHẦN HYUNDAI THÀNH CÔNG VIỆT NAM (HTC)",
            LCType = dto.LCType ?? LCType.Irrevocable,
            Currency = !string.IsNullOrWhiteSpace(dto.Currency) ? dto.Currency.Trim().ToUpperInvariant() : "USD",
            LCAmount = dto.LCAmount,
            ExchangeRate = rate,
            LCAmountVND = (long)Math.Round(dto.LCAmount * rate, MidpointRounding.AwayFromZero),
            UtilizedAmount = 0,
            RemainingAmount = dto.LCAmount,
            DateOpen = dateOpen,
            DateExpired = dto.DateExpired,
            LatestShipmentDate = dto.LatestShipmentDate,
            TermDays = termDays,
            PackingListNo = dto.PackingListNo?.Trim(),
            PaymentTerm = dto.PaymentTerm?.Trim(),
            PortOfLoading = dto.PortOfLoading?.Trim(),
            PortOfDischarge = dto.PortOfDischarge?.Trim(),
            Status = LCStatus.Draft,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy.Trim() : "KeToanNhapKhau_HTC",
            CreatedAt = now,
            Remark = dto.Remark?.Trim()
        };

        int itemNo = 1;
        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.VIN)) continue;
                string vin = item.VIN.Trim().ToUpperInvariant();
                if (lc.Details.Any(d => d.VIN == vin))
                    throw new InvalidOperationException($"Số khung {vin} bị trùng trong LC.");

                lc.Details.Add(new LetterOfCreditDetail
                {
                    OrgId = orgId,
                    LCNo = lcNo,
                    ItemNo = itemNo++,
                    VIN = vin,
                    ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                    ModelName = !string.IsNullOrWhiteSpace(item.ModelName) ? item.ModelName.Trim() : item.ModelCode.Trim(),
                    SpecCode = item.SpecCode?.Trim(),
                    EngineNo = item.EngineNo?.Trim(),
                    ColorCode = item.ColorCode?.Trim(),
                    WorkOrderNo = item.WorkOrderNo?.Trim(),
                    PortCode = item.PortCode?.Trim(),
                    PlantCode = item.PlantCode?.Trim(),
                    UnitPrice = item.UnitPrice,
                    Amount = item.UnitPrice,
                    Status = LCDetailStatus.Pending,
                    Remark = item.Remark?.Trim()
                });
            }
        }

        lc.TotalVehicles = lc.Details.Count;

        db.LettersOfCredit.Add(lc);
        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>Bổ sung xe nhập khẩu vào LC khi còn ở trạng thái Draft.</summary>
    public async Task<LetterOfCredit> AddCarAsync(Guid orgId, long lcId, AddCarToLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể thêm xe khi LC ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {lc.Status}.");

        string vin = dto.Item.VIN.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(vin))
            throw new ArgumentException("Số khung VIN không được để trống.");
        if (lc.Details.Any(d => d.VIN == vin))
            throw new InvalidOperationException($"Số khung {vin} đã tồn tại trong LC này.");

        int nextNo = lc.Details.Count > 0 ? lc.Details.Max(d => d.ItemNo) + 1 : 1;
        lc.Details.Add(new LetterOfCreditDetail
        {
            OrgId = orgId,
            LCId = lc.Id,
            LCNo = lc.LCNo,
            ItemNo = nextNo,
            VIN = vin,
            ModelCode = dto.Item.ModelCode.Trim().ToUpperInvariant(),
            ModelName = !string.IsNullOrWhiteSpace(dto.Item.ModelName) ? dto.Item.ModelName.Trim() : dto.Item.ModelCode.Trim(),
            SpecCode = dto.Item.SpecCode?.Trim(),
            EngineNo = dto.Item.EngineNo?.Trim(),
            ColorCode = dto.Item.ColorCode?.Trim(),
            WorkOrderNo = dto.Item.WorkOrderNo?.Trim(),
            PortCode = dto.Item.PortCode?.Trim(),
            PlantCode = dto.Item.PlantCode?.Trim(),
            UnitPrice = dto.Item.UnitPrice,
            Amount = dto.Item.UnitPrice,
            Status = LCDetailStatus.Pending,
            Remark = dto.Item.Remark?.Trim()
        });

        lc.TotalVehicles = lc.Details.Count;
        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>Xóa xe khỏi LC khi còn ở trạng thái Draft.</summary>
    public async Task<LetterOfCredit> RemoveCarAsync(Guid orgId, long lcId, long detailId)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xóa xe khi LC ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {lc.Status}.");

        var detail = lc.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new KeyNotFoundException($"Không tìm thấy dòng xe chi tiết ID {detailId}.");

        lc.Details.Remove(detail);
        db.LetterOfCreditDetails.Remove(detail);

        int no = 1;
        foreach (var d in lc.Details.OrderBy(x => x.ItemNo))
            d.ItemNo = no++;

        lc.TotalVehicles = lc.Details.Count;
        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>
    /// Trình ngân hàng phát hành LC (Draft -> Opened). Tương ứng nghiệp vụ mở LC.
    /// </summary>
    public async Task<LetterOfCredit> OpenAsync(Guid orgId, long lcId, OpenLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể phát hành LC ở trạng thái Dự thảo (Draft). Trạng thái hiện tại: {lc.Status}.");

        lc.Status = LCStatus.Opened;
        lc.OpenedBy = !string.IsNullOrWhiteSpace(dto.OpenedBy) ? dto.OpenedBy.Trim() : "KeToanNhapKhau_HTC";
        lc.OpenedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
            lc.Remark = string.IsNullOrWhiteSpace(lc.Remark) ? dto.Remark.Trim() : $"{lc.Remark} | {dto.Remark.Trim()}";

        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>
    /// Tu chỉnh (sửa đổi) điều khoản LC: giá trị, ngày hết hiệu lực, điều khoản thanh toán (Opened -> Amended).
    /// </summary>
    public async Task<LetterOfCredit> AmendAsync(Guid orgId, long lcId, AmendLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Opened && lc.Status != LCStatus.Amended)
            throw new InvalidOperationException($"Chỉ có thể tu chỉnh LC đã phát hành (Opened/Amended). Trạng thái hiện tại: {lc.Status}.");

        if (dto.NewLCAmount.HasValue)
        {
            if (dto.NewLCAmount.Value <= 0)
                throw new ArgumentException("Giá trị LC mới phải lớn hơn 0.");
            if (dto.NewLCAmount.Value < lc.UtilizedAmount)
                throw new InvalidOperationException($"Giá trị LC mới ({dto.NewLCAmount.Value:N2}) không được nhỏ hơn giá trị đã sử dụng ({lc.UtilizedAmount:N2}).");
            lc.LCAmount = dto.NewLCAmount.Value;
            lc.LCAmountVND = (long)Math.Round(lc.LCAmount * lc.ExchangeRate, MidpointRounding.AwayFromZero);
            lc.RemainingAmount = lc.LCAmount - lc.UtilizedAmount;
        }

        if (dto.NewDateExpired.HasValue)
        {
            lc.DateExpired = dto.NewDateExpired;
            lc.TermDays = (dto.NewDateExpired.Value.Date - lc.DateOpen.Date).Days;
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPaymentTerm))
            lc.PaymentTerm = dto.NewPaymentTerm.Trim();

        lc.Status = LCStatus.Amended;
        lc.AmendedBy = !string.IsNullOrWhiteSpace(dto.AmendedBy) ? dto.AmendedBy.Trim() : "KeToanNhapKhau_HTC";
        lc.AmendedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Remark))
            lc.Remark = string.IsNullOrWhiteSpace(lc.Remark) ? dto.Remark.Trim() : $"{lc.Remark} | {dto.Remark.Trim()}";

        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>
    /// Xuất trình bộ chứng từ để sử dụng LC (Opened/Amended -> Utilized).
    /// Ghi nhận giá trị đã sử dụng theo tổng đơn giá xe nhập khẩu và gắn số PackingList.
    /// </summary>
    public async Task<LetterOfCredit> PresentAsync(Guid orgId, long lcId, PresentLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Opened && lc.Status != LCStatus.Amended)
            throw new InvalidOperationException($"Chỉ có thể xuất trình bộ chứng từ cho LC đã phát hành (Opened/Amended). Trạng thái hiện tại: {lc.Status}.");

        if (lc.Details.Count == 0)
            throw new InvalidOperationException("LC phải có ít nhất 1 dòng xe nhập khẩu mới được xuất trình bộ chứng từ.");

        decimal utilized = lc.Details.Sum(d => d.Amount);
        if (utilized > lc.LCAmount)
            throw new InvalidOperationException($"Tổng giá trị xuất trình ({utilized:N2} {lc.Currency}) vượt quá giá trị LC ({lc.LCAmount:N2} {lc.Currency}).");

        lc.UtilizedAmount = utilized;
        lc.RemainingAmount = lc.LCAmount - utilized;
        if (!string.IsNullOrWhiteSpace(dto.PackingListNo))
            lc.PackingListNo = dto.PackingListNo.Trim();
        lc.Status = LCStatus.Utilized;

        foreach (var d in lc.Details)
        {
            d.Status = LCDetailStatus.Presented;
        }

        if (!string.IsNullOrWhiteSpace(dto.Remark))
            lc.Remark = string.IsNullOrWhiteSpace(lc.Remark) ? dto.Remark.Trim() : $"{lc.Remark} | {dto.Remark.Trim()}";

        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>
    /// Tất toán LC: ngân hàng thanh toán cho nhà xuất khẩu (Utilized -> Settled).
    /// </summary>
    public async Task<LetterOfCredit> SettleAsync(Guid orgId, long lcId, SettleLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status != LCStatus.Utilized)
            throw new InvalidOperationException($"Chỉ có thể tất toán LC đã xuất trình bộ chứng từ (Utilized). Trạng thái hiện tại: {lc.Status}.");

        lc.Status = LCStatus.Settled;
        lc.SettledBy = !string.IsNullOrWhiteSpace(dto.SettledBy) ? dto.SettledBy.Trim() : "KeToanTruong_HTC";
        lc.SettledAt = DateTime.UtcNow;

        foreach (var d in lc.Details)
        {
            d.Status = LCDetailStatus.Paid;
        }

        if (!string.IsNullOrWhiteSpace(dto.Remark))
            lc.Remark = string.IsNullOrWhiteSpace(lc.Remark) ? dto.Remark.Trim() : $"{lc.Remark} | {dto.Remark.Trim()}";

        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>Hủy LC (khi chưa tất toán).</summary>
    public async Task<LetterOfCredit> CancelAsync(Guid orgId, long lcId, CancelLetterOfCreditDto dto)
    {
        var lc = await db.LettersOfCredit
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.Id == lcId)
            ?? throw new KeyNotFoundException($"Không tìm thấy LC ID {lcId}.");

        if (lc.Status == LCStatus.Settled)
            throw new InvalidOperationException("Không thể hủy LC đã tất toán.");
        if (lc.Status == LCStatus.Cancelled)
            throw new InvalidOperationException("LC đã ở trạng thái Hủy trước đó.");
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy LC không được để trống.");

        lc.Status = LCStatus.Cancelled;
        lc.RejectReason = dto.Reason.Trim();

        foreach (var d in lc.Details)
        {
            d.Status = LCDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return lc;
    }

    /// <summary>Báo cáo tổng hợp số liệu LC theo trạng thái và theo ngân hàng phát hành.</summary>
    public async Task<LetterOfCreditSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.LettersOfCredit
            .Include(x => x.Details)
            .Where(x => x.OrgId == orgId)
            .ToListAsync();

        var summary = new LetterOfCreditSummaryDto
        {
            TotalLCs = list.Count,
            DraftCount = list.Count(x => x.Status == LCStatus.Draft),
            OpenedCount = list.Count(x => x.Status == LCStatus.Opened),
            AmendedCount = list.Count(x => x.Status == LCStatus.Amended),
            UtilizedCount = list.Count(x => x.Status == LCStatus.Utilized),
            SettledCount = list.Count(x => x.Status == LCStatus.Settled),
            ExpiredCount = list.Count(x => x.Status == LCStatus.Expired),
            CancelledCount = list.Count(x => x.Status == LCStatus.Cancelled),
            TotalVehicles = list.Sum(x => x.TotalVehicles),
            TotalLCAmount = list.Sum(x => x.LCAmount),
            TotalUtilizedAmount = list.Sum(x => x.UtilizedAmount),
            TotalRemainingAmount = list.Sum(x => x.RemainingAmount),
            TotalLCAmountVND = list.Sum(x => x.LCAmountVND)
        };

        summary.ByBank = list
            .GroupBy(x => string.IsNullOrWhiteSpace(x.BankName) ? "Khác" : x.BankName)
            .Select(g => new BankLetterOfCreditStatDto
            {
                BankName = g.Key,
                LCCount = g.Count(),
                VehicleCount = g.Sum(x => x.TotalVehicles),
                TotalLCAmount = g.Sum(x => x.LCAmount),
                UtilizedAmount = g.Sum(x => x.UtilizedAmount),
                RemainingAmount = g.Sum(x => x.RemainingAmount)
            })
            .OrderByDescending(x => x.TotalLCAmount)
            .ToList();

        return summary;
    }
}
