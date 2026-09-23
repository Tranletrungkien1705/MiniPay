using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý & Quyết toán Bảng tính Hỗ trợ Chi phí Tài chính (CPTC) và Chiết khấu Thanh toán TCG (CKTT) cho Đại lý.
/// Tương ứng module DMS40_FnExp_Calc_FnExp_PmDc trong BizHTC.Payment / 0.41.CalcFnExp &amp; FrmDMS40_2019_MngDMS40_FnExp_Calc_FnExp_PmDc.
/// </summary>
public sealed class FinancialExpenseService(AppDbContext db)
{
    // Các tỷ lệ định mức cọc & bảo lãnh chuẩn HTC (TERP.Constants.FnExp_PmtDs)
    public const decimal CKD_DEPOSIT_RATIO = 0.15m;   // CKD cọc 15%
    public const decimal CKD_GRT_RATIO = 0.85m;       // CKD bảo lãnh 85%
    public const int CKD_MAX_PD_DAYS = 15;            // CKD chiết khấu tối đa 15 ngày

    public const decimal CBU_DEPOSIT_RATIO = 0.30m;   // CBU cọc 30%
    public const decimal CBU_GRT_RATIO = 0.70m;       // CBU bảo lãnh 70%
    public const int CBU_MAX_PD_DAYS = 30;            // CBU chiết khấu tối đa 30 ngày

    public const decimal DAYS_IN_YEAR = 360m;         // Quy ước tài chính ngân hàng 360 ngày

    /// <summary>
    /// Tính toán tài chính chi tiết cho một dòng xe theo công thức chuẩn HTC.
    /// </summary>
    public static (long FnDepositAmount, long FnGrtAmount, long FnTotalAmount, int PDCountDate, long PDAmount, long CarTotalSettlement)
        CalculateCarFinancials(
            VehicleAssemblyType assemblyType,
            long unitPriceActual,
            decimal fnExpPercent,
            decimal pmtDsTCGPercent,
            int fnDepositCountDate,
            int fnGrtCountDate,
            int? explicitPDCountDate,
            DateTime? totalCompletedDate,
            DateTime? dateEnd)
    {
        if (unitPriceActual <= 0) return (0, 0, 0, 0, 0, 0);

        decimal depRatio = assemblyType == VehicleAssemblyType.CBU ? CBU_DEPOSIT_RATIO : CKD_DEPOSIT_RATIO;
        decimal grtRatio = assemblyType == VehicleAssemblyType.CBU ? CBU_GRT_RATIO : CKD_GRT_RATIO;
        int maxPdDays = assemblyType == VehicleAssemblyType.CBU ? CBU_MAX_PD_DAYS : CKD_MAX_PD_DAYS;

        // 1. Tiền CPTC cọc = (Tỷ lệ cọc * Giá xe * % Lãi suất CPTC * Số ngày cọc) / 360
        long fnDepositAmount = (long)Math.Round((depRatio * unitPriceActual * (fnExpPercent / 100m) * fnDepositCountDate) / DAYS_IN_YEAR, MidpointRounding.AwayFromZero);

        // 2. Tiền CPTC bảo lãnh = (Tỷ lệ bảo lãnh * Giá xe * % Lãi suất CPTC * Số ngày bảo lãnh) / 360
        long fnGrtAmount = (long)Math.Round((grtRatio * unitPriceActual * (fnExpPercent / 100m) * fnGrtCountDate) / DAYS_IN_YEAR, MidpointRounding.AwayFromZero);

        // 3. Tổng CPTC = Cọc + Bảo lãnh
        long fnTotalAmount = fnDepositAmount + fnGrtAmount;

        // 4. Số ngày thanh toán sớm được hưởng chiết khấu TCG (PDCountDate)
        int pdCountDate = 0;
        if (explicitPDCountDate.HasValue)
        {
            pdCountDate = Math.Clamp(explicitPDCountDate.Value, 0, maxPdDays);
        }
        else if (totalCompletedDate.HasValue && dateEnd.HasValue)
        {
            int diffDays = (dateEnd.Value.Date - totalCompletedDate.Value.Date).Days;
            pdCountDate = Math.Clamp(diffDays > 0 ? diffDays : 0, 0, maxPdDays);
        }

        // 5. Tiền chiết khấu thanh toán sớm TCG = (Tỷ lệ bảo lãnh * Giá xe * % CKTT * Số ngày sớm) / 360
        long pdAmount = (long)Math.Round((grtRatio * unitPriceActual * (pmtDsTCGPercent / 100m) * pdCountDate) / DAYS_IN_YEAR, MidpointRounding.AwayFromZero);

        // 6. Tổng quyết toán dòng xe = CPTC + CKTT
        long carTotalSettlement = fnTotalAmount + pdAmount;

        return (fnDepositAmount, fnGrtAmount, fnTotalAmount, pdCountDate, pdAmount, carTotalSettlement);
    }

    /// <summary>
    /// Ước tính trước số tiền CPTC và CKTT theo danh sách xe và tỷ lệ % lãi suất (phục vụ chức năng Preview trên UI).
    /// </summary>
    public object PreviewCalculation(
        decimal fnExpPercent,
        decimal pmtDsTCGPercent,
        List<FinancialExpenseItemInputDto> items)
    {
        long totalDeposit = 0;
        long totalGrt = 0;
        long totalFn = 0;
        long totalPd = 0;
        long grandTotal = 0;

        var previewItems = new List<object>();

        foreach (var item in items)
        {
            var assembly = item.AssemblyType.Equals("CBU", StringComparison.OrdinalIgnoreCase)
                ? VehicleAssemblyType.CBU
                : VehicleAssemblyType.CKD;

            int depDays = item.FnDepositCountDate ?? 0;
            int grtDays = item.FnGrtCountDate ?? 0;

            var (fnDep, fnGrt, fnTot, pdDays, pdAmt, carTot) = CalculateCarFinancials(
                assembly,
                item.UnitPriceActual,
                fnExpPercent,
                pmtDsTCGPercent,
                depDays,
                grtDays,
                item.PDCountDate,
                item.TotalCompletedDate,
                item.DateEnd
            );

            totalDeposit += fnDep;
            totalGrt += fnGrt;
            totalFn += fnTot;
            totalPd += pdAmt;
            grandTotal += carTot;

            previewItems.Add(new
            {
                item.VIN,
                item.ModelCode,
                item.AssemblyType,
                item.UnitPriceActual,
                FnDepositCountDate = depDays,
                FnDepositAmount = fnDep,
                FnGrtCountDate = grtDays,
                FnGrtAmount = fnGrt,
                FnTotalAmount = fnTot,
                PDCountDate = pdDays,
                PDAmount = pdAmt,
                CarTotalSettlement = carTot
            });
        }

        return new
        {
            FnExpPercent = fnExpPercent,
            PmtDsTCGPercent = pmtDsTCGPercent,
            TotalVehicles = items.Count,
            TotalFnDepositAmount = totalDeposit,
            TotalFnGrtAmount = totalGrt,
            TotalFnAmount = totalFn,
            TotalPDAmount = totalPd,
            TotalSettlementAmount = grandTotal,
            Items = previewItems
        };
    }

    /// <summary>
    /// Tạo bảng tính Hỗ trợ Chi phí tài chính & Chiết khấu thanh toán TCG mới (tương ứng DMS40_FnExp_Calc_FnExp_PmDc_Save).
    /// </summary>
    public async Task<FinancialExpenseStatement> CreateStatementAsync(
        Guid orgId,
        string? caNo,
        string dealerCode,
        string? dealerName,
        string? caName,
        DateTime termFrom,
        DateTime termTo,
        DateTime termPrevFrom,
        DateTime termPrevTo,
        decimal fnExpPercent,
        decimal pmtDsTCGPercent,
        string? remark,
        string? createdBy,
        List<FinancialExpenseItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(dealerCode))
            throw new ArgumentException("Mã đại lý thụ hưởng (DealerCode) không được để trống.");

        if (termFrom > termTo)
            throw new ArgumentException("Kỳ tính hiện tại không hợp lệ (TermFrom phải <= TermTo).");

        if (termPrevFrom > termPrevTo)
            throw new ArgumentException("Kỳ tính liền trước không hợp lệ (TermPrevFrom phải <= TermPrevTo).");

        if (items == null || items.Count == 0)
            throw new ArgumentException("Bảng tính phải có ít nhất 1 dòng xe.");

        var finalCaNo = string.IsNullOrWhiteSpace(caNo)
            ? $"CAN-{termTo:yyyyMM}-{dealerCode.Trim().ToUpperInvariant()}-{Random.Shared.Next(100, 999)}"
            : caNo.Trim().ToUpperInvariant();

        var exists = await db.FnExpStatements.AnyAsync(s => s.OrgId == orgId && s.CaNo == finalCaNo);
        if (exists)
            throw new InvalidOperationException($"Số bảng tính '{finalCaNo}' đã tồn tại trong hệ thống.");

        var finalDealerName = !string.IsNullOrWhiteSpace(dealerName)
            ? dealerName.Trim()
            : GetDealerNameByCode(dealerCode);

        var finalCaName = !string.IsNullOrWhiteSpace(caName)
            ? caName.Trim()
            : $"Bảng tính CPTC & CKTT TCG kỳ {termFrom:dd/MM/yyyy} - {termTo:dd/MM/yyyy} ({finalDealerName})";

        var statement = new FinancialExpenseStatement
        {
            OrgId = orgId,
            CaNo = finalCaNo,
            DealerCode = dealerCode.Trim().ToUpperInvariant(),
            DealerName = finalDealerName,
            CAName = finalCaName,
            TermFrom = termFrom,
            TermTo = termTo,
            TermPrevFrom = termPrevFrom,
            TermPrevTo = termPrevTo,
            FnExpPercent = fnExpPercent,
            PmtDsTCGPercent = pmtDsTCGPercent,
            Status = FinancialExpenseStatus.Draft,
            DlrSignStatus = FnExpSignCAStatus.Pending,
            HTCSignStatus = FnExpSignCAStatus.Pending,
            Remark = remark,
            CreatedBy = createdBy ?? "ChuyenVienKeToanHTC",
            CreatedAt = DateTime.Now
        };

        long totalDeposit = 0;
        long totalGrt = 0;
        long totalFn = 0;
        long totalPd = 0;
        long grandTotal = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.VIN))
                throw new ArgumentException("Số khung (VIN) không được để trống.");

            var assembly = item.AssemblyType.Equals("CBU", StringComparison.OrdinalIgnoreCase)
                ? VehicleAssemblyType.CBU
                : VehicleAssemblyType.CKD;

            int depDays = item.FnDepositCountDate ?? 0;
            int grtDays = item.FnGrtCountDate ?? 0;

            var (fnDep, fnGrt, fnTot, pdDays, pdAmt, carTot) = CalculateCarFinancials(
                assembly,
                item.UnitPriceActual,
                fnExpPercent,
                pmtDsTCGPercent,
                depDays,
                grtDays,
                item.PDCountDate,
                item.TotalCompletedDate,
                item.DateEnd
            );

            totalDeposit += fnDep;
            totalGrt += fnGrt;
            totalFn += fnTot;
            totalPd += pdAmt;
            grandTotal += carTot;

            statement.Details.Add(new FinancialExpenseDetail
            {
                OrgId = orgId,
                CaNo = finalCaNo,
                CarId = item.CarId?.Trim(),
                VIN = item.VIN.Trim().ToUpperInvariant(),
                ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                ModelName = item.ModelName?.Trim() ?? item.ModelCode.Trim().ToUpperInvariant(),
                SpecCode = item.SpecCode?.Trim(),
                SpecDescription = item.SpecDescription?.Trim(),
                ColorName = item.ColorName?.Trim(),
                SOCode = item.SOCode?.Trim(),
                AssemblyType = assembly,
                UnitPriceActual = item.UnitPriceActual,
                SodApprovedDate = item.SodApprovedDate,
                SodDepositDutyEndDate = item.SodDepositDutyEndDate,
                TotalCompletedDate = item.TotalCompletedDate,
                DateStart = item.DateStart,
                DateEnd = item.DateEnd,
                TermActual = item.TermActual ?? (item.DateEnd.HasValue && item.DateStart.HasValue ? (item.DateEnd.Value.Date - item.DateStart.Value.Date).Days : 0),
                FnDepositCountDate = depDays,
                FnDepositAmount = fnDep,
                FnGrtCountDate = grtDays,
                FnGrtAmount = fnGrt,
                FnTotalAmount = fnTot,
                PDCountDate = pdDays,
                PDAmount = pdAmt,
                CarTotalSettlement = carTot,
                Status = FinancialExpenseDetailStatus.Active,
                Remark = item.Remark?.Trim()
            });
        }

        statement.TotalVehicles = statement.Details.Count;
        statement.TotalFnDepositAmount = totalDeposit;
        statement.TotalFnGrtAmount = totalGrt;
        statement.TotalFnAmount = totalFn;
        statement.TotalPDAmount = totalPd;
        statement.TotalSettlementAmount = grandTotal;

        db.FnExpStatements.Add(statement);
        await db.SaveChangesAsync();

        return statement;
    }

    /// <summary>
    /// Tìm kiếm danh sách bảng tính chi phí tài chính & chiết khấu thanh toán (DMS40_FnExp_Calc_FnExp_PmDc_Get).
    /// </summary>
    public async Task<List<FinancialExpenseStatement>> GetStatementsAsync(
        Guid orgId,
        string? dealerCode,
        string? status,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var q = db.FnExpStatements
            .Include(s => s.Details)
            .Where(s => s.OrgId == orgId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            var code = dealerCode.Trim().ToUpperInvariant();
            q = q.Where(s => s.DealerCode == code);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FinancialExpenseStatus>(status, true, out var st))
        {
            q = q.Where(s => s.Status == st);
        }

        if (fromDate.HasValue)
        {
            var d = fromDate.Value.Date;
            q = q.Where(s => s.TermFrom >= d);
        }

        if (toDate.HasValue)
        {
            var d = toDate.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(s => s.TermTo <= d);
        }

        return await q.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Lấy chi tiết 1 bảng tính theo ID kèm toàn bộ danh sách xe.
    /// </summary>
    public async Task<FinancialExpenseStatement?> GetStatementByIdAsync(long id, Guid orgId)
    {
        return await db.FnExpStatements
            .Include(s => s.Details)
            .FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId);
    }

    /// <summary>
    /// Đại lý duyệt cấp 1 - Thẩm định bảng tính (DMS40_FnExp_Calc_FnExp_PmDc_DlrApproved1Multi).
    /// </summary>
    public async Task<FinancialExpenseStatement> DlrApprove1Async(long id, Guid orgId, string? approverName)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.Draft)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ bảng tính 'Draft' mới được duyệt ĐL cấp 1.");

        statement.Status = FinancialExpenseStatus.DlrApproved1;
        statement.DlrAppr1By = approverName ?? "KeToanTruongDaiLy";
        statement.DlrAppr1DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Đại lý ký số điện tử CA cấp 2 - Giám đốc đại lý ký xác nhận (DMS40_FnExp_Calc_FnExp_PmDc_DlrApproved2).
    /// </summary>
    public async Task<FinancialExpenseStatement> DlrSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.DlrApproved1 && statement.Status != FinancialExpenseStatus.Draft)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', không hợp lệ để ký số đại lý.");

        statement.Status = FinancialExpenseStatus.DlrSigned;
        statement.DlrSignStatus = FnExpSignCAStatus.Signed;
        statement.DlrSignUser = signerName ?? "GiamDocDaiLy";
        statement.DlrSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) statement.FilePathFnExp = filePath;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// HTC Chuyên viên tài chính thẩm định duyệt cấp 1 (DMS40_FnExp_Calc_FnExp_PmDc_HTCApproved1Multi).
    /// </summary>
    public async Task<FinancialExpenseStatement> HTCApprove1Async(long id, Guid orgId, string? approverName)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.DlrSigned)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', phải qua bước đại lý ký số (DlrSigned) trước khi HTC duyệt.");

        statement.Status = FinancialExpenseStatus.HTCApproved1;
        statement.HTCAppr1By = approverName ?? "ChuyenVienTaiChinhHTC";
        statement.HTCAppr1DTime = DateTime.Now;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// HTC Lãnh đạo ký số điện tử CA duyệt quyết toán cấp 2 (DMS40_FnExp_Calc_FnExp_PmDc_HTCApproved2).
    /// </summary>
    public async Task<FinancialExpenseStatement> HTCSignCAAsync(long id, Guid orgId, string? signerName, string? filePath)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.HTCApproved1)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', phải qua bước HTC duyệt cấp 1 trước khi ký số HTC.");

        statement.Status = FinancialExpenseStatus.HTCSigned;
        statement.HTCSignStatus = FnExpSignCAStatus.Signed;
        statement.HTCSignUser = signerName ?? "PhoTongGiamDocTaiChinhHTC";
        statement.HTCSignDTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(filePath)) statement.FilePathPmtDc = filePath;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Kế toán hoàn tất quyết toán chi trả qua UNC ngân hàng hoặc bù trừ công nợ xe.
    /// </summary>
    public async Task<FinancialExpenseStatement> SettleAsync(long id, Guid orgId, string? bankTxnRef, string? settledBy)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.HTCSigned)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ bảng tính đã ký số CA 2 cấp (HTCSigned) mới được quyết toán.");

        statement.Status = FinancialExpenseStatus.Settled;
        statement.SettledBy = settledBy ?? "KeToanThanhToan";
        statement.SettledAt = DateTime.Now;
        statement.BankTxnRef = bankTxnRef ?? $"UNC-FNEXP-{statement.CaNo}";

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Hủy bảng tính (DMS40_FnExp_Calc_FnExp_PmDc_HTCCancelMulti).
    /// </summary>
    public async Task<FinancialExpenseStatement> CancelAsync(long id, Guid orgId, string? reason, string? cancelledBy)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status == FinancialExpenseStatus.Settled)
            throw new InvalidOperationException("Bảng tính đã hoàn tất thanh toán quyết toán (Settled), không thể hủy.");

        statement.Status = FinancialExpenseStatus.Cancelled;
        statement.CancelBy = cancelledBy ?? "AdminHTC";
        statement.CancelDTime = DateTime.Now;
        statement.CancelReason = reason ?? "Hủy theo yêu cầu điều chỉnh số liệu đối soát";

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Điều chỉnh số ngày cọc, bảo lãnh, chiết khấu và tự động tái tính toán bảng kê (DMS40_FnExp_Calc_FnExp_PmDc_Update).
    /// </summary>
    public async Task<FinancialExpenseStatement> UpdateDetailsAsync(
        long id,
        Guid orgId,
        List<UpdateFnExpDetailItemDto> updateItems)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.Draft && statement.Status != FinancialExpenseStatus.DlrApproved1)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ bảng tính Draft hoặc DlrApproved1 mới được chỉnh sửa chi tiết.");

        foreach (var upd in updateItems)
        {
            var detail = statement.Details.FirstOrDefault(d => d.Id == upd.DetailId);
            if (detail == null) continue;

            if (upd.FnDepositCountDate.HasValue) detail.FnDepositCountDate = upd.FnDepositCountDate.Value;
            if (upd.FnGrtCountDate.HasValue) detail.FnGrtCountDate = upd.FnGrtCountDate.Value;
            if (upd.TotalCompletedDate.HasValue) detail.TotalCompletedDate = upd.TotalCompletedDate.Value;
            if (upd.Remark != null) detail.Remark = upd.Remark;

            var (fnDep, fnGrt, fnTot, pdDays, pdAmt, carTot) = CalculateCarFinancials(
                detail.AssemblyType,
                detail.UnitPriceActual,
                statement.FnExpPercent,
                statement.PmtDsTCGPercent,
                detail.FnDepositCountDate,
                detail.FnGrtCountDate,
                upd.PDCountDate ?? detail.PDCountDate,
                detail.TotalCompletedDate,
                detail.DateEnd
            );

            detail.FnDepositAmount = fnDep;
            detail.FnGrtAmount = fnGrt;
            detail.FnTotalAmount = fnTot;
            detail.PDCountDate = pdDays;
            detail.PDAmount = pdAmt;
            detail.CarTotalSettlement = carTot;
        }

        // Tái tính toán Master
        statement.TotalVehicles = statement.Details.Count(d => d.Status == FinancialExpenseDetailStatus.Active);
        statement.TotalFnDepositAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnDepositAmount);
        statement.TotalFnGrtAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnGrtAmount);
        statement.TotalFnAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnTotalAmount);
        statement.TotalPDAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.PDAmount);
        statement.TotalSettlementAmount = statement.TotalFnAmount + statement.TotalPDAmount;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Thêm xe vào bảng tính nháp.
    /// </summary>
    public async Task<FinancialExpenseStatement> AddVehiclesAsync(long id, Guid orgId, List<FinancialExpenseItemInputDto> newItems)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.Draft)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ được thêm xe khi bảng tính ở trạng thái Draft.");

        foreach (var item in newItems)
        {
            if (string.IsNullOrWhiteSpace(item.VIN)) continue;
            var vin = item.VIN.Trim().ToUpperInvariant();
            if (statement.Details.Any(d => d.VIN == vin)) continue; // Bỏ qua trùng

            var assembly = item.AssemblyType.Equals("CBU", StringComparison.OrdinalIgnoreCase)
                ? VehicleAssemblyType.CBU
                : VehicleAssemblyType.CKD;

            int depDays = item.FnDepositCountDate ?? 0;
            int grtDays = item.FnGrtCountDate ?? 0;

            var (fnDep, fnGrt, fnTot, pdDays, pdAmt, carTot) = CalculateCarFinancials(
                assembly,
                item.UnitPriceActual,
                statement.FnExpPercent,
                statement.PmtDsTCGPercent,
                depDays,
                grtDays,
                item.PDCountDate,
                item.TotalCompletedDate,
                item.DateEnd
            );

            statement.Details.Add(new FinancialExpenseDetail
            {
                OrgId = orgId,
                CaNo = statement.CaNo,
                CarId = item.CarId?.Trim(),
                VIN = vin,
                ModelCode = item.ModelCode.Trim().ToUpperInvariant(),
                ModelName = item.ModelName?.Trim() ?? item.ModelCode.Trim().ToUpperInvariant(),
                SpecCode = item.SpecCode?.Trim(),
                SpecDescription = item.SpecDescription?.Trim(),
                ColorName = item.ColorName?.Trim(),
                SOCode = item.SOCode?.Trim(),
                AssemblyType = assembly,
                UnitPriceActual = item.UnitPriceActual,
                SodApprovedDate = item.SodApprovedDate,
                SodDepositDutyEndDate = item.SodDepositDutyEndDate,
                TotalCompletedDate = item.TotalCompletedDate,
                DateStart = item.DateStart,
                DateEnd = item.DateEnd,
                TermActual = item.TermActual ?? 0,
                FnDepositCountDate = depDays,
                FnDepositAmount = fnDep,
                FnGrtCountDate = grtDays,
                FnGrtAmount = fnGrt,
                FnTotalAmount = fnTot,
                PDCountDate = pdDays,
                PDAmount = pdAmt,
                CarTotalSettlement = carTot,
                Status = FinancialExpenseDetailStatus.Active,
                Remark = item.Remark?.Trim()
            });
        }

        statement.TotalVehicles = statement.Details.Count(d => d.Status == FinancialExpenseDetailStatus.Active);
        statement.TotalFnDepositAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnDepositAmount);
        statement.TotalFnGrtAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnGrtAmount);
        statement.TotalFnAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.FnTotalAmount);
        statement.TotalPDAmount = statement.Details.Where(d => d.Status == FinancialExpenseDetailStatus.Active).Sum(d => d.PDAmount);
        statement.TotalSettlementAmount = statement.TotalFnAmount + statement.TotalPDAmount;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Xóa 1 xe khỏi bảng tính nháp.
    /// </summary>
    public async Task<FinancialExpenseStatement> RemoveVehicleAsync(long id, long detailId, Guid orgId)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.Draft)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ được xóa xe khi bảng tính ở trạng thái Draft.");

        var detail = statement.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new InvalidOperationException($"Không tìm thấy xe #{detailId} trong bảng tính.");

        statement.Details.Remove(detail);
        db.FnExpDetails.Remove(detail);

        statement.TotalVehicles = statement.Details.Count;
        statement.TotalFnDepositAmount = statement.Details.Sum(d => d.FnDepositAmount);
        statement.TotalFnGrtAmount = statement.Details.Sum(d => d.FnGrtAmount);
        statement.TotalFnAmount = statement.Details.Sum(d => d.FnTotalAmount);
        statement.TotalPDAmount = statement.Details.Sum(d => d.PDAmount);
        statement.TotalSettlementAmount = statement.TotalFnAmount + statement.TotalPDAmount;

        await db.SaveChangesAsync();
        return statement;
    }

    /// <summary>
    /// Xóa toàn bộ bảng tính nháp.
    /// </summary>
    public async Task DeleteDraftAsync(long id, Guid orgId)
    {
        var statement = await db.FnExpStatements.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId)
            ?? throw new InvalidOperationException($"Không tìm thấy bảng tính #{id}.");

        if (statement.Status != FinancialExpenseStatus.Draft)
            throw new InvalidOperationException($"Bảng tính đang ở trạng thái '{statement.Status}', chỉ bảng tính Draft mới được phép xóa hoàn toàn.");

        db.FnExpStatements.Remove(statement);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Bảng kê Quyết toán Chi phí Tài chính & Chiết khấu (CR Advice) kèm số tiền bằng chữ tiếng Việt.
    /// </summary>
    public async Task<FinancialExpenseAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var s = await db.FnExpStatements.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id && x.OrgId == orgId);
        if (s == null) return null;

        int idx = 1;
        var detailDtos = s.Details.OrderBy(d => d.Id).Select(d => new FinancialExpenseDetailAdviceDto
        {
            No = idx++,
            VIN = d.VIN,
            ModelCode = d.ModelCode,
            ModelName = d.ModelName ?? d.ModelCode,
            SOCode = d.SOCode ?? "-",
            AssemblyType = d.AssemblyType.ToString(),
            UnitPriceActual = d.UnitPriceActual,
            TotalCompletedDate = d.TotalCompletedDate?.ToString("dd/MM/yyyy"),
            DateEnd = d.DateEnd?.ToString("dd/MM/yyyy"),
            FnDepositCountDate = d.FnDepositCountDate,
            FnDepositAmount = d.FnDepositAmount,
            FnGrtCountDate = d.FnGrtCountDate,
            FnGrtAmount = d.FnGrtAmount,
            FnTotalAmount = d.FnTotalAmount,
            PDCountDate = d.PDCountDate,
            PDAmount = d.PDAmount,
            CarTotalSettlement = d.CarTotalSettlement,
            Status = d.Status.ToString()
        }).ToList();

        string statusText = s.Status switch
        {
            FinancialExpenseStatus.Draft => "Dự thảo bảng tính (Draft)",
            FinancialExpenseStatus.DlrApproved1 => "Đại lý đã duyệt cấp 1 (DlrApproved1)",
            FinancialExpenseStatus.DlrSigned => "Đại lý đã ký số CA (DlrSigned)",
            FinancialExpenseStatus.HTCApproved1 => "HTC đã thẩm định duyệt cấp 1 (HTCApproved1)",
            FinancialExpenseStatus.HTCSigned => "HTC đã ký số CA hoàn tất (HTCSigned)",
            FinancialExpenseStatus.Settled => "Đã quyết toán chi trả qua UNC (Settled)",
            FinancialExpenseStatus.Cancelled => "Đã hủy bảng tính (Cancelled)",
            _ => s.Status.ToString()
        };

        string dlrSignInfo = s.DlrSignStatus == FnExpSignCAStatus.Signed
            ? $"ĐÃ KÝ SỐ CA: {s.DlrSignUser} ({s.DlrSignDTime:dd/MM/yyyy HH:mm})"
            : "CHƯA KÝ SỐ";

        string htcSignInfo = s.HTCSignStatus == FnExpSignCAStatus.Signed
            ? $"ĐÃ KÝ SỐ CA: {s.HTCSignUser} ({s.HTCSignDTime:dd/MM/yyyy HH:mm})"
            : "CHƯA KÝ SỐ";

        return new FinancialExpenseAdviceDto
        {
            CaNo = s.CaNo,
            CAName = s.CAName,
            DealerCode = s.DealerCode,
            DealerName = s.DealerName,
            TermFromFormatted = s.TermFrom.ToString("dd/MM/yyyy"),
            TermToFormatted = s.TermTo.ToString("dd/MM/yyyy"),
            TermPrevFromFormatted = s.TermPrevFrom.ToString("dd/MM/yyyy"),
            TermPrevToFormatted = s.TermPrevTo.ToString("dd/MM/yyyy"),
            FnExpPercent = s.FnExpPercent,
            PmtDsTCGPercent = s.PmtDsTCGPercent,
            TotalVehicles = s.TotalVehicles,
            TotalFnDepositAmount = s.TotalFnDepositAmount,
            TotalFnGrtAmount = s.TotalFnGrtAmount,
            TotalFnAmount = s.TotalFnAmount,
            TotalPDAmount = s.TotalPDAmount,
            TotalSettlementAmount = s.TotalSettlementAmount,
            TotalSettlementInWords = LongInt2VNSpeakString(s.TotalSettlementAmount, "đồng"),
            StatusText = statusText,
            DlrSignInfo = dlrSignInfo,
            HTCSignInfo = htcSignInfo,
            BankTxnRef = s.BankTxnRef,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            Items = detailDtos
        };
    }

    /// <summary>
    /// Báo cáo thống kê tổng hợp KPI cho Dashboard.
    /// </summary>
    public async Task<FinancialExpenseSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var list = await db.FnExpStatements.Where(s => s.OrgId == orgId).AsNoTracking().ToListAsync();

        return new FinancialExpenseSummaryDto
        {
            TotalStatements = list.Count,
            DraftCount = list.Count(s => s.Status == FinancialExpenseStatus.Draft),
            PendingDlrApprovalCount = list.Count(s => s.Status == FinancialExpenseStatus.Draft || s.Status == FinancialExpenseStatus.DlrApproved1),
            PendingHTCApprovalCount = list.Count(s => s.Status == FinancialExpenseStatus.DlrSigned || s.Status == FinancialExpenseStatus.HTCApproved1),
            SignedCount = list.Count(s => s.Status == FinancialExpenseStatus.HTCSigned),
            SettledCount = list.Count(s => s.Status == FinancialExpenseStatus.Settled),
            CancelledCount = list.Count(s => s.Status == FinancialExpenseStatus.Cancelled),
            TotalVehiclesSubsidized = list.Where(s => s.Status != FinancialExpenseStatus.Cancelled).Sum(s => s.TotalVehicles),
            TotalFnDepositSubsidized = list.Where(s => s.Status != FinancialExpenseStatus.Cancelled).Sum(s => s.TotalFnDepositAmount),
            TotalFnGrtSubsidized = list.Where(s => s.Status != FinancialExpenseStatus.Cancelled).Sum(s => s.TotalFnGrtAmount),
            TotalFnAmountSubsidized = list.Where(s => s.Status != FinancialExpenseStatus.Cancelled).Sum(s => s.TotalFnAmount),
            TotalEarlyPaymentDiscountSubsidized = list.Where(s => s.Status != FinancialExpenseStatus.Cancelled).Sum(s => s.TotalPDAmount),
            TotalSettlementDisbursed = list.Where(s => s.Status == FinancialExpenseStatus.Settled).Sum(s => s.TotalSettlementAmount)
        };
    }

    /// <summary>
    /// Lấy danh sách các xe ứng viên có thể đưa vào kỳ tính (mô phỏng nguồn xe hoàn tất thanh toán từ hệ thống DMS).
    /// </summary>
    public List<CandidateVehicleFnExpDto> GetCandidateVehicles(string? dealerCode = null)
    {
        var candidates = new List<CandidateVehicleFnExpDto>
        {
            new()
            {
                VIN = "KMHCT81EPHU880101",
                CarId = "CAR-FN-101",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                ModelCode = "SANTAFE-CAL",
                ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                SpecCode = "SF-2.5T-CAL6",
                SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                ColorName = "Trắng ngọc trai",
                SOCode = "SO-202505-HD01",
                AssemblyType = "CKD",
                UnitPriceActual = 1_365_000_000,
                SodApprovedDate = new DateTime(2025, 4, 15),
                SodDepositDutyEndDate = new DateTime(2025, 4, 20),
                TotalCompletedDate = new DateTime(2025, 5, 20),
                DateStart = new DateTime(2025, 4, 25),
                DateEnd = new DateTime(2025, 5, 30),
                TermActual = 35,
                FnDepositCountDate = 28,
                FnGrtCountDate = 25,
                PDCountDate = 10
            },
            new()
            {
                VIN = "KMHCT81EPHU880102",
                CarId = "CAR-FN-102",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                ModelCode = "TUCSON-TURBO",
                ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                SpecCode = "TUC-1.6T-PREM",
                SpecDescription = "Bản máy xăng tăng áp HTRAC",
                ColorName = "Đen huyền bí",
                SOCode = "SO-202505-HD02",
                AssemblyType = "CKD",
                UnitPriceActual = 989_000_000,
                SodApprovedDate = new DateTime(2025, 4, 18),
                SodDepositDutyEndDate = new DateTime(2025, 4, 23),
                TotalCompletedDate = new DateTime(2025, 5, 18),
                DateStart = new DateTime(2025, 4, 28),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 33,
                FnDepositCountDate = 25,
                FnGrtCountDate = 20,
                PDCountDate = 13
            },
            new()
            {
                VIN = "KMHCT81EPHU880103",
                CarId = "CAR-FN-103",
                DealerCode = "DLR-DONGANH",
                DealerName = "Hyundai Đông Anh",
                ModelCode = "PALISADE-PREM",
                ModelName = "Hyundai Palisade 2.2D Prestige",
                SpecCode = "PAL-2.2D-PRE7",
                SpecDescription = "Bản SUV 7 chỗ máy dầu",
                ColorName = "Xanh lục bảo",
                SOCode = "SO-202505-DA01",
                AssemblyType = "CKD",
                UnitPriceActual = 1_589_000_000,
                SodApprovedDate = new DateTime(2025, 4, 10),
                SodDepositDutyEndDate = new DateTime(2025, 4, 15),
                TotalCompletedDate = new DateTime(2025, 5, 22),
                DateStart = new DateTime(2025, 4, 20),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 41,
                FnDepositCountDate = 30,
                FnGrtCountDate = 31,
                PDCountDate = 9
            },
            new()
            {
                VIN = "KMHCT81EPHU880104",
                CarId = "CAR-FN-104",
                DealerCode = "DLR-SAIGON",
                DealerName = "Hyundai Sài Gòn 1S",
                ModelCode = "IONIQ5-PREM",
                ModelName = "Hyundai Ioniq 5 Prestige EV",
                SpecCode = "IQ5-EV-PREM",
                SpecDescription = "Bản xe điện thông minh E-GMP",
                ColorName = "Bạc ánh kim",
                SOCode = "SO-202505-SG01",
                AssemblyType = "CKD",
                UnitPriceActual = 1_450_000_000,
                SodApprovedDate = new DateTime(2025, 4, 20),
                SodDepositDutyEndDate = new DateTime(2025, 4, 25),
                TotalCompletedDate = new DateTime(2025, 5, 25),
                DateStart = new DateTime(2025, 4, 30),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 31,
                FnDepositCountDate = 26,
                FnGrtCountDate = 25,
                PDCountDate = 6
            },
            new()
            {
                VIN = "KMHCT81EPHU880105",
                CarId = "CAR-FN-105",
                DealerCode = "DLR-PHAMVANDONG",
                DealerName = "Hyundai Phạm Văn Đồng",
                ModelCode = "CUSTIN-TURBO",
                ModelName = "Hyundai Custin 2.0T Cao Cấp",
                SpecCode = "CUS-2.0T-PREM",
                SpecDescription = "Bản MPV cửa trượt điện 7 chỗ",
                ColorName = "Trắng tuyết",
                SOCode = "SO-202505-PVD01",
                AssemblyType = "CKD",
                UnitPriceActual = 974_000_000,
                SodApprovedDate = new DateTime(2025, 4, 22),
                SodDepositDutyEndDate = new DateTime(2025, 4, 27),
                TotalCompletedDate = new DateTime(2025, 5, 19),
                DateStart = new DateTime(2025, 5, 2),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 29,
                FnDepositCountDate = 22,
                FnGrtCountDate = 17,
                PDCountDate = 12
            },
            new()
            {
                VIN = "KMHCT81EPHU880106",
                CarId = "CAR-FN-106",
                DealerCode = "DLR-DANANG",
                DealerName = "Hyundai Sông Hàn - Đà Nẵng",
                ModelCode = "CRETA-PREM",
                ModelName = "Hyundai Creta 1.5 Cao Cấp",
                SpecCode = "CRE-1.5L-PREM",
                SpecDescription = "Bản SUV cỡ B SmartSense",
                ColorName = "Đỏ mận",
                SOCode = "SO-202505-DN01",
                AssemblyType = "CKD",
                UnitPriceActual = 699_000_000,
                SodApprovedDate = new DateTime(2025, 4, 25),
                SodDepositDutyEndDate = new DateTime(2025, 4, 30),
                TotalCompletedDate = new DateTime(2025, 5, 26),
                DateStart = new DateTime(2025, 5, 5),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 26,
                FnDepositCountDate = 20,
                FnGrtCountDate = 21,
                PDCountDate = 5
            },
            new()
            {
                VIN = "KMHCT81EPHU880107",
                CarId = "CAR-FN-107",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                ModelCode = "STARIA-LOUNGE",
                ModelName = "Hyundai Staria Lounge 9 Seats CBU",
                SpecCode = "STA-2.2D-CBU9",
                SpecDescription = "Bản nhập khẩu nguyên chiếc Hàn Quốc",
                ColorName = "Đen ngọc bích",
                SOCode = "SO-202505-HD03",
                AssemblyType = "CBU",
                UnitPriceActual = 1_850_000_000,
                SodApprovedDate = new DateTime(2025, 4, 5),
                SodDepositDutyEndDate = new DateTime(2025, 4, 12),
                TotalCompletedDate = new DateTime(2025, 5, 10),
                DateStart = new DateTime(2025, 4, 18),
                DateEnd = new DateTime(2025, 5, 31),
                TermActual = 43,
                FnDepositCountDate = 31,
                FnGrtCountDate = 31,
                PDCountDate = 21
            }
        };

        if (!string.IsNullOrWhiteSpace(dealerCode))
        {
            var code = dealerCode.Trim().ToUpperInvariant();
            return candidates.Where(c => c.DealerCode.Equals(code, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return candidates;
    }

    private static string GetDealerNameByCode(string dealerCode)
    {
        return dealerCode.Trim().ToUpperInvariant() switch
        {
            "DLR-HADONG" => "Hyundai Hà Đông",
            "DLR-DONGANH" => "Hyundai Đông Anh",
            "DLR-SAIGON" => "Hyundai Sài Gòn 1S",
            "DLR-PHAMVANDONG" => "Hyundai Phạm Văn Đồng",
            "DLR-DANANG" => "Hyundai Sông Hàn - Đà Nẵng",
            "DLR-CANTHO" => "Hyundai Tây Đô - Cần Thơ",
            "DLR-HAIPHONG" => "Hyundai Hải Phòng",
            _ => $"Đại lý Hyundai {dealerCode}"
        };
    }

    /// <summary>
    /// Chuyển đổi số nguyên thành chuỗi đọc tiếng Việt tài chính ngân hàng chuẩn HTC (LongInt2VNSpeakString).
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
            else if (t == 0 && (h > 0 || readZeroHundred) && u > 0)
            {
                res += "lẻ " + digits[u] + " ";
            }
            else if (u > 0)
            {
                res += digits[u] + " ";
            }

            return res;
        }

        var groups = new List<int>();
        long temp = number;
        while (temp > 0)
        {
            groups.Add((int)(temp % 1000));
            temp /= 1000;
        }

        string result = "";
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            int g = groups[i];
            if (g == 0) continue;
            bool readZeroHundred = (i < groups.Count - 1);
            string grpStr = ReadThreeDigits(g, readZeroHundred).Trim();
            result += grpStr + " " + units[i] + " ";
        }

        result = result.Trim();
        if (string.IsNullOrWhiteSpace(result)) return "Không " + dvt;

        // Viết hoa chữ cái đầu tiên
        result = char.ToUpperInvariant(result[0]) + result[1..] + " " + dvt;
        return result.Replace("  ", " ");
    }
}
