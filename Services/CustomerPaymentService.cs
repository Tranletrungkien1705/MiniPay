using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Công Nợ Khách Hàng Sửa Chữa &amp; Thu Tiền Quyết Toán Dịch Vụ Xe Ô Tô.
/// Tương ứng BizCarSv.Debit.cs (SerCusDebitCreate, SerCusDebitUpdate, SerCusDebitDelete, SerCusDebitDetailGet, SerCusDebitSearch,
/// SerPaymentCreate, SerPaymentUpdate, SerPaymentDelete, SerPaymentPaperRpt, TConst.SerDebitType.CusDebit = "1", TConst.SerPaymentType.CusPayment = "1")
/// và FrmCusDebitCreate, FrmCusDebitSearch, FrmCusPaymentCreate, FrmDebitShow trong TERP.HTCServiceClient/Views/Debit hệ nguồn HTC 2010.
/// </summary>
public sealed class CustomerPaymentService(AppDbContext db)
{
    // ==========================================
    // 1. Quản lý Khoản Công Nợ Khách Hàng Dịch Vụ Sửa Chữa (CustomerDebit)
    // ==========================================

    /// <summary>
    /// Lập hồ sơ công nợ phải thu mới theo Lệnh sửa chữa RO (SerCusDebitCreate / FrmCusDebitCreate).
    /// </summary>
    public async Task<CustomerDebit> CreateDebitAsync(Guid orgId, CreateCustomerDebitDto dto, string? createdBy = "CoVanDichVu")
    {
        if (string.IsNullOrWhiteSpace(dto.CusId))
            throw new ArgumentException("Mã khách hàng (CusId) không được để trống.", nameof(dto.CusId));
        if (string.IsNullOrWhiteSpace(dto.CusName))
            throw new ArgumentException("Tên khách hàng (CusName) không được để trống.", nameof(dto.CusName));
        if (string.IsNullOrWhiteSpace(dto.RONo))
            throw new ArgumentException("Số lệnh sửa chữa (RONo) không được để trống.", nameof(dto.RONo));
        if (string.IsNullOrWhiteSpace(dto.PlateNo))
            throw new ArgumentException("Biển số xe (PlateNo) không được để trống.", nameof(dto.PlateNo));
        if (dto.DebitAmount <= 0)
            throw new ArgumentException("Số tiền công nợ dịch vụ phải lớn hơn 0.", nameof(dto.DebitAmount));

        string cusId = dto.CusId.Trim().ToUpperInvariant();
        string roNo = dto.RONo.Trim().ToUpperInvariant();
        string plateNo = dto.PlateNo.Trim().ToUpperInvariant();

        // Kiểm tra xem lệnh sửa chữa này đã được tạo hồ sơ nợ chưa
        bool exists = await db.CustomerDebits.AnyAsync(d =>
            d.OrgId == orgId && d.RONo == roNo && d.Status != CustomerDebitStatus.Cancelled);
        if (exists)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roNo}' đã có hồ sơ công nợ khách hàng đang hoạt động.");

        // Sinh số hồ sơ nợ: DEB-CUS-yyyyMM-xxx
        string monthPrefix = $"DEB-CUS-{DateTime.Now:yyyyMM}-";
        int seq = await db.CustomerDebits
            .Where(d => d.OrgId == orgId && d.DebitNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string debitNo = string.IsNullOrWhiteSpace(dto.DebitNo) ? $"{monthPrefix}{seq:D3}" : dto.DebitNo.Trim();

        DateTime dDate = dto.DebitDate ?? DateTime.Now;
        DateTime duDate = dto.DueDate ?? dDate.AddDays(15); // Mặc định hạn nợ dịch vụ khách hàng 15 ngày

        var debit = new CustomerDebit
        {
            OrgId = orgId,
            DebitNo = debitNo,
            CusId = cusId,
            CusName = dto.CusName.Trim(),
            Phone = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            RONo = roNo,
            RODate = dto.RODate ?? dDate,
            PlateNo = plateNo,
            VIN = string.IsNullOrWhiteSpace(dto.VIN) ? $"VIN{DateTime.Now:yyMMddHHmmss}" : dto.VIN.Trim().ToUpperInvariant(),
            ModelCode = string.IsNullOrWhiteSpace(dto.ModelCode) ? "SANTAFE" : dto.ModelCode.Trim().ToUpperInvariant(),
            ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Bảo dưỡng định kỳ & Sửa chữa chung" : dto.ServiceType.Trim(),
            DebitDate = dDate,
            DueDate = duDate,
            DebitAmount = dto.DebitAmount,
            PaidAmount = 0,
            RemainAmount = dto.DebitAmount,
            Status = CustomerDebitStatus.Pending,
            Note = dto.Note?.Trim() ?? $"Công nợ dịch vụ sửa chữa theo lệnh {roNo} xe {plateNo}",
            CreatedBy = createdBy ?? dto.CreatedBy ?? "CoVanDichVu",
            CreatedAt = DateTime.Now
        };

        db.CustomerDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit;
    }

    /// <summary>
    /// Tìm kiếm danh sách công nợ phải thu của khách hàng dịch vụ (SerCusDebitSearch / FrmCusDebitSearch).
    /// </summary>
    public async Task<List<CustomerDebit>> GetDebitsAsync(
        Guid orgId,
        string? cusId = null,
        CustomerDebitStatus? status = null,
        string? keyword = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.CustomerDebits.Where(d => d.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(cusId))
        {
            string cid = cusId.Trim().ToUpperInvariant();
            q = q.Where(d => d.CusId == cid);
        }

        if (status.HasValue)
        {
            q = q.Where(d => d.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            q = q.Where(d => d.DebitDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            q = q.Where(d => d.DebitDate < end);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToUpperInvariant();
            q = q.Where(d => d.DebitNo.Contains(kw) ||
                             d.RONo.Contains(kw) ||
                             d.PlateNo.Contains(kw) ||
                             d.VIN.Contains(kw) ||
                             d.CusName.ToUpper().Contains(kw) ||
                             d.ModelCode.Contains(kw) ||
                             (d.Phone != null && d.Phone.Contains(kw)));
        }

        return await q.OrderByDescending(d => d.DebitDate).ToListAsync();
    }

    /// <summary>
    /// Chi tiết một khoản nợ khách hàng dịch vụ (SerCusDebitDetailGet).
    /// </summary>
    public async Task<CustomerDebit?> GetDebitByIdAsync(long id, Guid orgId)
    {
        return await db.CustomerDebits.FirstOrDefaultAsync(d => d.Id == id && d.OrgId == orgId);
    }

    /// <summary>
    /// Lấy danh sách các lệnh sửa chữa còn nợ của một khách hàng (để xem trước hoặc phân bổ thanh toán).
    /// </summary>
    public async Task<List<EligibleCustomerDebitDto>> GetEligibleDebitsAsync(Guid orgId, string cusId)
    {
        string cid = cusId.Trim().ToUpperInvariant();
        var debits = await db.CustomerDebits
            .Where(d => d.OrgId == orgId && d.CusId == cid && d.RemainAmount > 0 && d.Status != CustomerDebitStatus.Cancelled)
            .OrderBy(d => d.DebitDate)
            .ToListAsync();

        return debits.Select(d => new EligibleCustomerDebitDto
        {
            Id = d.Id,
            DebitNo = d.DebitNo,
            CusId = d.CusId,
            CusName = d.CusName,
            RONo = d.RONo,
            PlateNo = d.PlateNo,
            VIN = d.VIN,
            ModelCode = d.ModelCode,
            ServiceType = d.ServiceType,
            DebitDate = d.DebitDate,
            DueDate = d.DueDate,
            DebitAmount = d.DebitAmount,
            PaidAmount = d.PaidAmount,
            RemainAmount = d.RemainAmount,
            Status = d.Status.ToString()
        }).ToList();
    }

    // ==========================================
    // 2. Quản lý Phiếu Thu Thanh Toán Dịch Vụ Khách Hàng & Phân Bổ Nợ FIFO (CustomerPayment)
    // ==========================================

    /// <summary>
    /// Lập phiếu thu thanh toán công nợ dịch vụ khách hàng với thuật toán phân bổ nợ tự động FIFO (SerPaymentCreate / FrmPaymentCreate).
    /// </summary>
    public async Task<CustomerPayment> CreatePaymentAsync(Guid orgId, CreateCustomerPaymentDto dto, string? createdBy = "ThuNganDichVu")
    {
        if (string.IsNullOrWhiteSpace(dto.CusId))
            throw new ArgumentException("Mã khách hàng (CusId) không được để trống.", nameof(dto.CusId));
        if (dto.PaymentAmount <= 0)
            throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.", nameof(dto.PaymentAmount));
        if (string.IsNullOrWhiteSpace(dto.PayPersonName))
            throw new ArgumentException("Tên người nộp tiền / đại diện thanh toán không được để trống.", nameof(dto.PayPersonName));

        string cusId = dto.CusId.Trim().ToUpperInvariant();

        // Lấy thông tin khách hàng từ hồ sơ nợ gần nhất nếu DTO chưa truyền
        var latestDebit = await db.CustomerDebits
            .Where(d => d.OrgId == orgId && d.CusId == cusId)
            .OrderByDescending(d => d.DebitDate)
            .FirstOrDefaultAsync();

        string cusName = string.IsNullOrWhiteSpace(dto.CusName)
            ? (latestDebit?.CusName ?? cusId)
            : dto.CusName.Trim();
        string? cusPhone = string.IsNullOrWhiteSpace(dto.CusPhone)
            ? latestDebit?.Phone
            : dto.CusPhone.Trim();
        string? plateNo = string.IsNullOrWhiteSpace(dto.PlateNo)
            ? latestDebit?.PlateNo
            : dto.PlateNo.Trim().ToUpperInvariant();

        // Sinh số phiếu thu: PT-CUS-yyyyMM-xxx (tương ứng SerDebitGeneratePaymentNo)
        string monthPrefix = $"PT-CUS-{DateTime.Now:yyyyMM}-";
        int pmtSeq = await db.CustomerPayments
            .Where(p => p.OrgId == orgId && p.PaymentNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string pmtNo = string.IsNullOrWhiteSpace(dto.PaymentNo) ? $"{monthPrefix}{pmtSeq:D3}" : dto.PaymentNo.Trim();

        DateTime payDate = dto.PayDate ?? DateTime.Now;

        var payment = new CustomerPayment
        {
            OrgId = orgId,
            PaymentNo = pmtNo,
            CusId = cusId,
            CusName = cusName,
            CusPhone = cusPhone,
            PlateNo = plateNo,
            PayDate = payDate,
            PayPersonName = dto.PayPersonName.Trim(),
            PayPersonIDCardNo = dto.PayPersonIDCardNo?.Trim(),
            PayPersonPhone = dto.PayPersonPhone?.Trim(),
            PaymentAmount = dto.PaymentAmount,
            PaymentMethod = dto.PaymentMethod,
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BankName = dto.BankName?.Trim(),
            BankAccountNo = dto.BankAccountNo?.Trim(),
            BankTxnRef = dto.BankTxnRef?.Trim(),
            Status = CustomerPaymentStatus.Confirmed, // Mặc định thu ngân tạo phiếu là đã nhận tiền
            Note = dto.Note?.Trim() ?? $"Thu tiền thanh toán dịch vụ sửa chữa xe {plateNo} của khách hàng {cusName}",
            CreatedBy = createdBy ?? dto.CreatedBy ?? "ThuNganDichVu",
            CreatedAt = DateTime.Now,
            ConfirmedBy = createdBy ?? dto.CreatedBy ?? "ThuNganDichVu",
            ConfirmedAt = DateTime.Now
        };

        db.CustomerPayments.Add(payment);
        await db.SaveChangesAsync(); // Lưu để sinh PaymentId

        // Phân bổ thanh toán vào các lệnh RO còn nợ
        long remainingToAllocate = dto.PaymentAmount;
        long totalAllocated = 0;

        if (dto.ManualAllocations != null && dto.ManualAllocations.Count > 0)
        {
            // Phân bổ chỉ định thủ công theo danh sách
            foreach (var alloc in dto.ManualAllocations)
            {
                if (remainingToAllocate <= 0) break;
                if (alloc.Amount <= 0) continue;

                var deb = await db.CustomerDebits.FirstOrDefaultAsync(d =>
                    d.Id == alloc.DebitId && d.OrgId == orgId && d.CusId == cusId && d.Status != CustomerDebitStatus.Cancelled);
                if (deb == null || deb.RemainAmount <= 0) continue;

                long allocAmt = Math.Min(alloc.Amount, Math.Min(remainingToAllocate, deb.RemainAmount));
                long before = deb.RemainAmount;
                deb.PaidAmount += allocAmt;
                deb.RemainAmount = Math.Max(0, deb.DebitAmount - deb.PaidAmount);
                deb.Status = deb.RemainAmount == 0 ? CustomerDebitStatus.Settled : CustomerDebitStatus.PartiallyPaid;

                var dtl = new CustomerPaymentDetail
                {
                    PaymentId = payment.Id,
                    DebitId = deb.Id,
                    OrgId = orgId,
                    DebitNo = deb.DebitNo,
                    RONo = deb.RONo,
                    PlateNo = deb.PlateNo,
                    VIN = deb.VIN,
                    DebitAmount = deb.DebitAmount,
                    DebitAmountBefore = before,
                    PaymentDetailAmount = allocAmt,
                    DebitAmountLeft = deb.RemainAmount,
                    Remark = deb.RemainAmount == 0 ? "Tất toán toàn bộ nợ lệnh sửa chữa" : "Thu nợ một phần lệnh sửa chữa"
                };
                payment.Details.Add(dtl);
                totalAllocated += allocAmt;
                remainingToAllocate -= allocAmt;
            }
        }
        else
        {
            // Thuật toán phân bổ tự động FIFO theo SerPaymentCreate trong 2010.HTC:
            // Quét danh sách các khoản nợ của khách hàng theo thứ tự thời gian cũ trước mới sau
            var openDebits = await db.CustomerDebits
                .Where(d => d.OrgId == orgId && d.CusId == cusId && d.RemainAmount > 0 && d.Status != CustomerDebitStatus.Cancelled)
                .OrderBy(d => d.DebitDate)
                .ThenBy(d => d.Id)
                .ToListAsync();

            foreach (var deb in openDebits)
            {
                if (remainingToAllocate <= 0) break;

                long allocAmt = Math.Min(remainingToAllocate, deb.RemainAmount);
                long before = deb.RemainAmount;

                deb.PaidAmount += allocAmt;
                deb.RemainAmount = Math.Max(0, deb.DebitAmount - deb.PaidAmount);
                deb.Status = deb.RemainAmount == 0 ? CustomerDebitStatus.Settled : CustomerDebitStatus.PartiallyPaid;

                var dtl = new CustomerPaymentDetail
                {
                    PaymentId = payment.Id,
                    DebitId = deb.Id,
                    OrgId = orgId,
                    DebitNo = deb.DebitNo,
                    RONo = deb.RONo,
                    PlateNo = deb.PlateNo,
                    VIN = deb.VIN,
                    DebitAmount = deb.DebitAmount,
                    DebitAmountBefore = before,
                    PaymentDetailAmount = allocAmt,
                    DebitAmountLeft = deb.RemainAmount,
                    Remark = deb.RemainAmount == 0 ? "Tất toán toàn bộ nợ lệnh sửa chữa" : "Thu nợ một phần lệnh sửa chữa"
                };
                payment.Details.Add(dtl);
                totalAllocated += allocAmt;
                remainingToAllocate -= allocAmt;
            }
        }

        payment.TotalAllocated = totalAllocated;
        payment.UnallocatedAmount = remainingToAllocate; // Tiền nộp dư giữ lại làm khoản cọc dịch vụ lần sau

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Tìm kiếm danh sách phiếu thu tiền dịch vụ của khách hàng (SerPaymentGet).
    /// </summary>
    public async Task<List<CustomerPayment>> GetPaymentsAsync(
        Guid orgId,
        string? cusId = null,
        CustomerPaymentStatus? status = null,
        CustomerPaymentMethod? method = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.CustomerPayments
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(cusId))
        {
            string cid = cusId.Trim().ToUpperInvariant();
            q = q.Where(p => p.CusId == cid);
        }

        if (status.HasValue)
        {
            q = q.Where(p => p.Status == status.Value);
        }

        if (method.HasValue)
        {
            q = q.Where(p => p.PaymentMethod == method.Value);
        }

        if (fromDate.HasValue)
        {
            q = q.Where(p => p.PayDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            q = q.Where(p => p.PayDate < end);
        }

        return await q.OrderByDescending(p => p.PayDate).ToListAsync();
    }

    /// <summary>
    /// Chi tiết 1 phiếu thu kèm danh sách phân bổ RO (SerPaymentGet).
    /// </summary>
    public async Task<CustomerPayment?> GetPaymentByIdAsync(long id, Guid orgId)
    {
        return await db.CustomerPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// Thu ngân / Kế toán trưởng thẩm định xác nhận phiếu thu (Draft -> Confirmed).
    /// </summary>
    public async Task<CustomerPayment> ConfirmPaymentAsync(long id, Guid orgId, ConfirmCustomerPaymentDto? dto)
    {
        var payment = await db.CustomerPayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu #{id}.");

        if (payment.Status != CustomerPaymentStatus.Draft)
            throw new InvalidOperationException($"Chỉ phiếu thu ở trạng thái Nháp (Draft) mới có thể xác nhận.");

        payment.Status = CustomerPaymentStatus.Confirmed;
        payment.ConfirmedBy = dto?.ConfirmedBy ?? "ThuNganTruong";
        payment.ConfirmedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Kế toán dịch vụ / Kế toán trưởng đối soát chốt ca quyết toán vào sổ quỹ (Confirmed -> Settled).
    /// </summary>
    public async Task<CustomerPayment> SettlePaymentAsync(long id, Guid orgId, SettleCustomerPaymentDto? dto)
    {
        var payment = await db.CustomerPayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu #{id}.");

        if (payment.Status == CustomerPaymentStatus.Settled)
            throw new InvalidOperationException($"Phiếu thu #{payment.PaymentNo} đã được quyết toán chốt ca trước đó.");

        if (payment.Status == CustomerPaymentStatus.Cancelled)
            throw new InvalidOperationException($"Không thể quyết toán phiếu thu đã bị hủy.");

        payment.Status = CustomerPaymentStatus.Settled;
        payment.SettledBy = dto?.SettledBy ?? "KeToanTruong";
        payment.SettledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto?.BankTxnRef))
        {
            payment.BankTxnRef = dto.BankTxnRef.Trim();
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy phiếu thu tiền dịch vụ và HOÀN TÁC ROLLBACK toàn bộ nợ đã trừ trên các lệnh sửa chữa RO (SerPaymentDelete).
    /// </summary>
    public async Task<CustomerPayment> CancelPaymentAsync(long id, Guid orgId, CancelCustomerPaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy phiếu thu không được để trống.", nameof(dto.Reason));

        var payment = await db.CustomerPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu #{id}.");

        if (payment.Status == CustomerPaymentStatus.Cancelled)
            throw new InvalidOperationException($"Phiếu thu #{payment.PaymentNo} đã ở trạng thái hủy.");

        // Hoàn tác (Rollback) số tiền nợ đã trừ trên các lệnh RO
        foreach (var dtl in payment.Details)
        {
            var deb = await db.CustomerDebits.FirstOrDefaultAsync(d => d.Id == dtl.DebitId && d.OrgId == orgId);
            if (deb != null)
            {
                deb.PaidAmount = Math.Max(0, deb.PaidAmount - dtl.PaymentDetailAmount);
                deb.RemainAmount = Math.Min(deb.DebitAmount, deb.RemainAmount + dtl.PaymentDetailAmount);

                if (deb.PaidAmount == 0)
                {
                    deb.Status = CustomerDebitStatus.Pending;
                }
                else if (deb.RemainAmount > 0)
                {
                    deb.Status = CustomerDebitStatus.PartiallyPaid;
                }
                else
                {
                    deb.Status = CustomerDebitStatus.Settled;
                }
            }
        }

        payment.Status = CustomerPaymentStatus.Cancelled;
        payment.CancelledBy = dto.CancelledBy ?? "KeToanDichVu";
        payment.CancelledAt = DateTime.Now;
        payment.CancelReason = dto.Reason.Trim();

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Thống kê Dashboard KPI tổng hợp công nợ và quyết toán thu tiền dịch vụ khách hàng.
    /// </summary>
    public async Task<CustomerDebitSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var debits = await db.CustomerDebits.Where(d => d.OrgId == orgId).ToListAsync();
        var payments = await db.CustomerPayments.Where(p => p.OrgId == orgId && p.Status != CustomerPaymentStatus.Cancelled).ToListAsync();

        int totalROs = debits.Count;
        int pendingROs = debits.Count(d => d.Status == CustomerDebitStatus.Pending);
        int partiallyPaidROs = debits.Count(d => d.Status == CustomerDebitStatus.PartiallyPaid);
        int settledROs = debits.Count(d => d.Status == CustomerDebitStatus.Settled);
        int cancelledROs = debits.Count(d => d.Status == CustomerDebitStatus.Cancelled);

        var activeDebits = debits.Where(d => d.Status != CustomerDebitStatus.Cancelled).ToList();
        long totalDebitAmount = activeDebits.Sum(d => d.DebitAmount);
        long totalPaidAmount = activeDebits.Sum(d => d.PaidAmount);
        long totalRemainingDebt = activeDebits.Sum(d => d.RemainAmount);

        decimal collectionRate = totalDebitAmount > 0
            ? Math.Round((decimal)totalPaidAmount * 100m / totalDebitAmount, 2)
            : 0;

        int totalPaymentReceipts = payments.Count;
        long totalReceiptsAmount = payments.Sum(p => p.PaymentAmount);

        // Top 5 khách hàng có công nợ lớn nhất
        var topCusts = activeDebits
            .GroupBy(d => new { d.CusId, d.CusName, d.Phone, d.PlateNo })
            .Select(g => new CustomerStatDto
            {
                CusId = g.Key.CusId,
                CusName = g.Key.CusName,
                Phone = g.Key.Phone,
                PlateNo = g.Key.PlateNo,
                ROCount = g.Count(),
                TotalDebitAmount = g.Sum(x => x.DebitAmount),
                TotalPaidAmount = g.Sum(x => x.PaidAmount),
                RemainingDebt = g.Sum(x => x.RemainAmount)
            })
            .OrderByDescending(c => c.RemainingDebt)
            .Take(5)
            .ToList();

        return new CustomerDebitSummaryDto
        {
            TotalROs = totalROs,
            PendingROs = pendingROs,
            PartiallyPaidROs = partiallyPaidROs,
            SettledROs = settledROs,
            CancelledROs = cancelledROs,
            TotalDebitAmount = totalDebitAmount,
            TotalPaidAmount = totalPaidAmount,
            TotalRemainingDebt = totalRemainingDebt,
            CollectionRate = collectionRate,
            TotalPaymentReceipts = totalPaymentReceipts,
            TotalReceiptsAmount = totalReceiptsAmount,
            TopCustomerDebts = topCusts
        };
    }

    /// <summary>
    /// Sinh dữ liệu mẫu in Phiếu Thu Tiền Quyết Toán Dịch Vụ Sửa Chữa (SerPaymentPaperRpt / FrmDebitShow).
    /// </summary>
    public async Task<CustomerPaymentAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.CustomerPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        var latestDebit = await db.CustomerDebits
            .Where(d => d.OrgId == orgId && d.CusId == payment.CusId)
            .OrderByDescending(d => d.DebitDate)
            .FirstOrDefaultAsync();

        string methodText = payment.PaymentMethod switch
        {
            CustomerPaymentMethod.Cash => "Tiền mặt tại quầy thu ngân dịch vụ",
            CustomerPaymentMethod.BankTransfer => "Chuyển khoản ngân hàng (Ủy nhiệm chi / VietQR)",
            CustomerPaymentMethod.VnPay => "Cổng thanh toán điện tử VNPay QR",
            CustomerPaymentMethod.Momo => "Ví điện tử MoMo Pay",
            CustomerPaymentMethod.Offset => "Bù trừ công nợ / Voucher bảo dưỡng VIP",
            _ => "Khác"
        };

        string statusText = payment.Status switch
        {
            CustomerPaymentStatus.Draft => "Mới lập (Bản nháp)",
            CustomerPaymentStatus.Confirmed => "Đã xác nhận thu tiền",
            CustomerPaymentStatus.Settled => "Đã quyết toán chốt ca kế toán",
            CustomerPaymentStatus.Cancelled => "Đã hủy phiếu thu",
            _ => payment.Status.ToString()
        };

        var detailAdvices = new List<CustomerPaymentDetailAdviceDto>();
        int idx = 1;
        foreach (var d in payment.Details)
        {
            var deb = await db.CustomerDebits.FirstOrDefaultAsync(x => x.Id == d.DebitId && x.OrgId == orgId);
            detailAdvices.Add(new CustomerPaymentDetailAdviceDto
            {
                No = idx++,
                DebitNo = d.DebitNo,
                RONo = d.RONo,
                PlateNo = d.PlateNo,
                VIN = d.VIN,
                ModelCode = deb?.ModelCode ?? "HYUNDAI",
                DebitAmount = d.DebitAmount,
                DebitAmountBefore = d.DebitAmountBefore,
                PaymentDetailAmount = d.PaymentDetailAmount,
                DebitAmountLeft = d.DebitAmountLeft,
                StatusAfterPayment = d.DebitAmountLeft == 0 ? "Tất toán (100%)" : "Còn nợ"
            });
        }

        return new CustomerPaymentAdviceDto
        {
            PaymentNo = payment.PaymentNo,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            CusId = payment.CusId,
            CusName = payment.CusName,
            CusPhone = payment.CusPhone ?? latestDebit?.Phone,
            CusAddress = latestDebit?.Address ?? "Hà Nội, Việt Nam",
            PlateNo = payment.PlateNo ?? latestDebit?.PlateNo ?? "",
            PayPersonName = payment.PayPersonName,
            PayPersonIDCardNo = payment.PayPersonIDCardNo,
            PayPersonPhone = payment.PayPersonPhone,
            PaymentMethodText = methodText,
            BankCode = payment.BankCode,
            BankName = payment.BankName,
            BankAccountNo = payment.BankAccountNo,
            BankTxnRef = payment.BankTxnRef,
            PaymentAmount = payment.PaymentAmount,
            PaymentAmountInWords = NumberToWordsVietnamese(payment.PaymentAmount),
            TotalAllocated = payment.TotalAllocated,
            UnallocatedAmount = payment.UnallocatedAmount,
            StatusText = statusText,
            Note = payment.Note,
            CreatedBy = payment.CreatedBy,
            ConfirmedBy = payment.ConfirmedBy,
            SettledBy = payment.SettledBy,
            Details = detailAdvices
        };
    }

    /// <summary>
    /// Thuật toán chuyển đổi số tiền thành chữ tiếng Việt chuẩn quy chuẩn tài chính ngân hàng HTC.
    /// </summary>
    private static string NumberToWordsVietnamese(long totalNumber)
    {
        if (totalNumber == 0) return "Không đồng";
        if (totalNumber < 0) return "Âm " + NumberToWordsVietnamese(-totalNumber);

        string[] units = ["", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ"];
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

        long temp = totalNumber;
        string result = "";
        int unitIndex = 0;

        while (temp > 0)
        {
            long chunk = temp % 1000;
            if (chunk > 0)
            {
                string chunkText = ReadChunk(chunk, digits, temp >= 1000);
                result = chunkText + units[unitIndex] + (string.IsNullOrWhiteSpace(result) ? "" : " " + result);
            }
            temp /= 1000;
            unitIndex++;
        }

        result = result.Trim();
        if (result.Length > 0)
        {
            result = char.ToUpperInvariant(result[0]) + result[1..] + " đồng chẵn./.";
        }
        return result;
    }

    private static string ReadChunk(long chunk, string[] digits, bool needLeadingZero)
    {
        int h = (int)(chunk / 100);
        int t = (int)((chunk % 100) / 10);
        int u = (int)(chunk % 10);

        string text = "";

        if (h > 0 || needLeadingZero)
        {
            text += digits[h] + " trăm";
        }

        if (t > 1)
        {
            text += (text.Length > 0 ? " " : "") + digits[t] + " mươi";
        }
        else if (t == 1)
        {
            text += (text.Length > 0 ? " " : "") + "mười";
        }
        else if (t == 0 && u > 0 && (h > 0 || needLeadingZero))
        {
            text += (text.Length > 0 ? " " : "") + "lẻ";
        }

        if (u == 1 && t > 1)
        {
            text += " mốt";
        }
        else if (u == 5 && t >= 1)
        {
            text += " lăm";
        }
        else if (u > 0)
        {
            text += (text.Length > 0 ? " " : "") + digits[u];
        }

        return text;
    }
}
