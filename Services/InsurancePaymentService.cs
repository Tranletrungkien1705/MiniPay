using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Thu Tiền Thanh Toán &amp; Quyết Toán Bồi Thường Bảo Hiểm Xe Ô Tô.
/// Tương ứng BizCarSv.Debit.cs (SerInsuranceDebitSearch, SerInsuranceDebitDetailGet, SerPaymentCreate, SerPaymentPaperRpt)
/// và FrmInsPaymentCreate trong TERP.HTCServiceClient/Views/Debit hệ nguồn HTC 2010.
/// </summary>
public sealed class InsurancePaymentService(AppDbContext db)
{
    // ==========================================
    // 1. Quản lý Khoản Công Nợ Bồi Thường Bảo Hiểm (InsuranceClaimDebit)
    // ==========================================

    /// <summary>
    /// Lập hồ sơ công nợ bồi thường bảo hiểm mới theo Lệnh sửa chữa RO xưởng dịch vụ.
    /// </summary>
    public async Task<InsuranceClaimDebit> CreateDebitAsync(Guid orgId, CreateInsuranceDebitDto dto, string? createdBy = "CoVanDichVu")
    {
        if (string.IsNullOrWhiteSpace(dto.InsNo))
            throw new ArgumentException("Mã công ty bảo hiểm (InsNo) không được để trống.", nameof(dto.InsNo));
        if (string.IsNullOrWhiteSpace(dto.RONo))
            throw new ArgumentException("Số Lệnh sửa chữa (RONo) không được để trống.", nameof(dto.RONo));
        if (string.IsNullOrWhiteSpace(dto.VIN))
            throw new ArgumentException("Số khung xe (VIN) không được để trống.", nameof(dto.VIN));
        if (string.IsNullOrWhiteSpace(dto.PlateNo))
            throw new ArgumentException("Biển số xe (PlateNo) không được để trống.", nameof(dto.PlateNo));
        if (dto.DebitAmount <= 0)
            throw new ArgumentException("Số tiền bồi thường được duyệt phải lớn hơn 0.", nameof(dto.DebitAmount));

        string insNo = dto.InsNo.Trim().ToUpperInvariant();
        string roNo = dto.RONo.Trim().ToUpperInvariant();
        string vin = dto.VIN.Trim().ToUpperInvariant();
        string plateNo = dto.PlateNo.Trim().ToUpperInvariant();

        // Kiểm tra xem lệnh RO này đã được tạo hồ sơ nợ bảo hiểm chưa
        bool roExists = await db.InsuranceClaimDebits.AnyAsync(d =>
            d.OrgId == orgId && d.RONo == roNo && d.Status != InsuranceDebitStatus.Cancelled);
        if (roExists)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roNo}' đã có hồ sơ công nợ bảo hiểm đang hoạt động.");

        // Sinh số hồ sơ nợ: DEB-INS-yyyyMM-xxx
        string monthPrefix = $"DEB-INS-{DateTime.Now:yyyyMM}-";
        int seq = await db.InsuranceClaimDebits
            .Where(d => d.OrgId == orgId && d.DebitNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string debitNo = string.IsNullOrWhiteSpace(dto.DebitNo) ? $"{monthPrefix}{seq:D3}" : dto.DebitNo.Trim();

        DateTime dDate = dto.DebitDate ?? DateTime.Now;
        DateTime duDate = dto.DueDate ?? dDate.AddDays(30);

        var debit = new InsuranceClaimDebit
        {
            OrgId = orgId,
            DebitNo = debitNo,
            InsNo = insNo,
            InsName = string.IsNullOrWhiteSpace(dto.InsName) ? GetInsuranceNameByCode(insNo) : dto.InsName.Trim(),
            RONo = roNo,
            VIN = vin,
            PlateNo = plateNo,
            ModelCode = string.IsNullOrWhiteSpace(dto.ModelCode) ? "SANTAFE" : dto.ModelCode.Trim().ToUpperInvariant(),
            ModelName = dto.ModelName?.Trim() ?? GetModelNameByCode(dto.ModelCode),
            CustomerName = dto.CustomerName?.Trim() ?? "Khách hàng dịch vụ",
            CustomerPhone = dto.CustomerPhone?.Trim(),
            DebitDate = dDate,
            DueDate = duDate,
            DebitAmount = dto.DebitAmount,
            PaidAmount = 0,
            RemainAmount = dto.DebitAmount,
            Status = InsuranceDebitStatus.Pending,
            Note = dto.Note?.Trim() ?? $"Hồ sơ bồi thường sơn sấy & sửa chữa thân vỏ xe theo RO {roNo}",
            CreatedBy = createdBy ?? dto.CreatedBy ?? "CoVanDichVu",
            CreatedAt = DateTime.Now
        };

        db.InsuranceClaimDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit;
    }

    /// <summary>
    /// Tìm kiếm danh sách công nợ bồi thường bảo hiểm.
    /// </summary>
    public async Task<List<InsuranceClaimDebit>> GetDebitsAsync(
        Guid orgId,
        string? insNo = null,
        InsuranceDebitStatus? status = null,
        string? keyword = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.InsuranceClaimDebits.Where(d => d.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(insNo))
        {
            string code = insNo.Trim().ToUpperInvariant();
            q = q.Where(d => d.InsNo == code);
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
                             d.VIN.Contains(kw) ||
                             d.PlateNo.Contains(kw) ||
                             (d.CustomerName != null && d.CustomerName.ToUpper().Contains(kw)));
        }

        return await q.OrderByDescending(d => d.DebitDate).ToListAsync();
    }

    /// <summary>
    /// Chi tiết một khoản nợ bảo hiểm.
    /// </summary>
    public async Task<InsuranceClaimDebit?> GetDebitByIdAsync(long id, Guid orgId)
    {
        return await db.InsuranceClaimDebits.FirstOrDefaultAsync(d => d.Id == id && d.OrgId == orgId);
    }

    /// <summary>
    /// Lấy danh sách hồ sơ bồi thường còn nợ của một công ty bảo hiểm (để xem trước hoặc phân bổ thanh toán).
    /// </summary>
    public async Task<List<EligibleDebitForPaymentDto>> GetEligibleDebitsAsync(Guid orgId, string insNo)
    {
        string code = insNo.Trim().ToUpperInvariant();
        var debits = await db.InsuranceClaimDebits
            .Where(d => d.OrgId == orgId && d.InsNo == code && d.RemainAmount > 0 && d.Status != InsuranceDebitStatus.Cancelled)
            .OrderBy(d => d.DebitDate)
            .ToListAsync();

        return debits.Select(d => new EligibleDebitForPaymentDto
        {
            Id = d.Id,
            DebitNo = d.DebitNo,
            RONo = d.RONo,
            VIN = d.VIN,
            PlateNo = d.PlateNo,
            ModelCode = d.ModelCode,
            CustomerName = d.CustomerName,
            DebitDate = d.DebitDate,
            DueDate = d.DueDate,
            DebitAmount = d.DebitAmount,
            PaidAmount = d.PaidAmount,
            RemainAmount = d.RemainAmount,
            Status = d.Status.ToString()
        }).ToList();
    }

    // ==========================================
    // 2. Quản lý Phiếu Thu Thanh Toán Bảo Hiểm & Phân Bổ Nợ FIFO (InsurancePayment)
    // ==========================================

    /// <summary>
    /// Lập phiếu thu thanh toán bảo hiểm mới với thuật toán phân bổ nợ tự động FIFO (SerPaymentCreate).
    /// </summary>
    public async Task<InsurancePayment> CreatePaymentAsync(Guid orgId, CreateInsurancePaymentDto dto, string? createdBy = "KeToanThuNgan")
    {
        if (string.IsNullOrWhiteSpace(dto.InsNo))
            throw new ArgumentException("Mã công ty bảo hiểm (InsNo) không được để trống.", nameof(dto.InsNo));
        if (dto.PaymentAmount <= 0)
            throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.", nameof(dto.PaymentAmount));
        if (string.IsNullOrWhiteSpace(dto.PayPersonName))
            throw new ArgumentException("Tên người đại diện thanh toán không được để trống.", nameof(dto.PayPersonName));

        string insNo = dto.InsNo.Trim().ToUpperInvariant();

        // Sinh số phiếu thu: PM-INS-yyyyMM-xxx
        string monthPrefix = $"PM-INS-{DateTime.Now:yyyyMM}-";
        int seq = await db.InsurancePayments
            .Where(p => p.OrgId == orgId && p.PaymentNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string paymentNo = string.IsNullOrWhiteSpace(dto.PaymentNo) ? $"{monthPrefix}{seq:D3}" : dto.PaymentNo.Trim();

        DateTime pDate = dto.PayDate ?? DateTime.Now;
        var pMethod = dto.PaymentMethod ?? InsurancePaymentMethod.BankTransfer;

        var payment = new InsurancePayment
        {
            OrgId = orgId,
            PaymentNo = paymentNo,
            InsNo = insNo,
            InsName = string.IsNullOrWhiteSpace(dto.InsName) ? GetInsuranceNameByCode(insNo) : dto.InsName.Trim(),
            PayDate = pDate,
            PayPersonName = dto.PayPersonName.Trim(),
            PayPersonIDCardNo = dto.PayPersonIDCardNo?.Trim(),
            PayPersonPhone = dto.PayPersonPhone?.Trim(),
            PaymentAmount = dto.PaymentAmount,
            PaymentMethod = pMethod,
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BankName = string.IsNullOrWhiteSpace(dto.BankName) && !string.IsNullOrWhiteSpace(dto.BankCode) ? GetBankNameByCode(dto.BankCode) : dto.BankName?.Trim(),
            BankAccountNo = dto.BankAccountNo?.Trim(),
            BankTxnRef = dto.BankTxnRef?.Trim(),
            Status = InsurancePaymentStatus.Confirmed, // Mặc định xác nhận khi có phiếu thu
            ConfirmedBy = createdBy,
            ConfirmedAt = DateTime.Now,
            Note = dto.Note?.Trim() ?? $"Thu tiền thanh toán bồi thường bảo hiểm {insNo}",
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now
        };

        // Thuật toán phân bổ tự động FIFO theo SerPaymentCreate trong BizCarSv.Debit.cs
        long amountToAllocate = dto.PaymentAmount;
        long totalAllocated = 0;

        if (dto.AutoAllocateFifo)
        {
            // Lấy danh sách nợ còn tồn của công ty bảo hiểm sắp xếp thời gian phát sinh từ cũ đến mới (FIFO)
            var debits = await db.InsuranceClaimDebits
                .Where(d => d.OrgId == orgId && d.InsNo == insNo && d.RemainAmount > 0 && d.Status != InsuranceDebitStatus.Cancelled)
                .OrderBy(d => d.DebitDate)
                .ToListAsync();

            foreach (var d in debits)
            {
                if (amountToAllocate <= 0) break;

                long debitBefore = d.RemainAmount;
                long alloc = Math.Min(amountToAllocate, debitBefore);
                long debitLeft = debitBefore - alloc;

                d.PaidAmount += alloc;
                d.RemainAmount = debitLeft;
                d.Status = debitLeft == 0 ? InsuranceDebitStatus.Settled : InsuranceDebitStatus.PartiallyPaid;

                payment.Details.Add(new InsurancePaymentDetail
                {
                    OrgId = orgId,
                    DebitId = d.Id,
                    DebitNo = d.DebitNo,
                    RONo = d.RONo,
                    VIN = d.VIN,
                    PlateNo = d.PlateNo,
                    DebitAmount = d.DebitAmount,
                    DebitAmountBefore = debitBefore,
                    PaymentDetailAmount = alloc,
                    DebitAmountLeft = debitLeft,
                    Remark = debitLeft == 0 ? "Tất toán toàn bộ lệnh RO" : "Thanh toán một phần nợ RO"
                });

                totalAllocated += alloc;
                amountToAllocate -= alloc;
            }
        }
        else if (dto.ManualAllocations != null && dto.ManualAllocations.Count > 0)
        {
            // Phân bổ thủ công chỉ định
            foreach (var item in dto.ManualAllocations)
            {
                if (item.Amount <= 0) continue;
                var d = await db.InsuranceClaimDebits.FirstOrDefaultAsync(x => x.Id == item.DebitId && x.OrgId == orgId);
                if (d == null || d.Status == InsuranceDebitStatus.Cancelled) continue;

                long alloc = Math.Min(item.Amount, d.RemainAmount);
                long debitBefore = d.RemainAmount;
                long debitLeft = debitBefore - alloc;

                d.PaidAmount += alloc;
                d.RemainAmount = debitLeft;
                d.Status = debitLeft == 0 ? InsuranceDebitStatus.Settled : InsuranceDebitStatus.PartiallyPaid;

                payment.Details.Add(new InsurancePaymentDetail
                {
                    OrgId = orgId,
                    DebitId = d.Id,
                    DebitNo = d.DebitNo,
                    RONo = d.RONo,
                    VIN = d.VIN,
                    PlateNo = d.PlateNo,
                    DebitAmount = d.DebitAmount,
                    DebitAmountBefore = debitBefore,
                    PaymentDetailAmount = alloc,
                    DebitAmountLeft = debitLeft,
                    Remark = debitLeft == 0 ? "Tất toán toàn bộ lệnh RO (chỉ định)" : "Thanh toán một phần nợ RO (chỉ định)"
                });

                totalAllocated += alloc;
                amountToAllocate -= alloc;
            }
        }

        payment.TotalAllocated = totalAllocated;
        payment.UnallocatedAmount = amountToAllocate > 0 ? amountToAllocate : 0;

        db.InsurancePayments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Tìm kiếm danh sách phiếu thu thanh toán bảo hiểm.
    /// </summary>
    public async Task<List<InsurancePayment>> GetPaymentsAsync(
        Guid orgId,
        string? insNo = null,
        InsurancePaymentStatus? status = null,
        InsurancePaymentMethod? method = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.InsurancePayments
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(insNo))
        {
            string code = insNo.Trim().ToUpperInvariant();
            q = q.Where(p => p.InsNo == code);
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
    /// Chi tiết một phiếu thu thanh toán kèm chi tiết phân bổ nợ.
    /// </summary>
    public async Task<InsurancePayment?> GetPaymentByIdAsync(long id, Guid orgId)
    {
        return await db.InsurancePayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// Kế toán xác nhận phiếu thu thanh toán bảo hiểm (Draft -> Confirmed).
    /// </summary>
    public async Task<InsurancePayment> ConfirmPaymentAsync(long id, Guid orgId, ConfirmInsurancePaymentDto? dto)
    {
        var payment = await db.InsurancePayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu bảo hiểm #{id}.");

        if (payment.Status != InsurancePaymentStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xác nhận phiếu thu ở trạng thái 'Draft'. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = InsurancePaymentStatus.Confirmed;
        payment.ConfirmedBy = dto?.ConfirmedBy ?? "KeToanTruong";
        payment.ConfirmedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hoàn tất quyết toán công nợ và chốt sổ kế toán (Confirmed -> Settled).
    /// </summary>
    public async Task<InsurancePayment> SettlePaymentAsync(long id, Guid orgId, SettleInsurancePaymentDto? dto)
    {
        var payment = await db.InsurancePayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu bảo hiểm #{id}.");

        if (payment.Status != InsurancePaymentStatus.Confirmed && payment.Status != InsurancePaymentStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể quyết toán phiếu thu ở trạng thái 'Confirmed' hoặc 'Draft'. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = InsurancePaymentStatus.Settled;
        payment.SettledBy = dto?.SettledBy ?? "KeToanThanhToan";
        payment.SettledAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy phiếu thu thanh toán bảo hiểm và HOÀN TÁC (ROLLBACK) nợ trên các hồ sơ RO.
    /// </summary>
    public async Task<InsurancePayment> CancelPaymentAsync(long id, Guid orgId, CancelInsurancePaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy phiếu thu không được để trống.", nameof(dto.Reason));

        var payment = await db.InsurancePayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu bảo hiểm #{id}.");

        if (payment.Status == InsurancePaymentStatus.Cancelled)
            throw new InvalidOperationException($"Phiếu thu '{payment.PaymentNo}' đã bị hủy trước đó.");

        // Hoàn tác trừ nợ trên các hồ sơ RO tương ứng
        foreach (var dtl in payment.Details)
        {
            var debit = await db.InsuranceClaimDebits.FirstOrDefaultAsync(x => x.Id == dtl.DebitId && x.OrgId == orgId);
            if (debit != null && debit.Status != InsuranceDebitStatus.Cancelled)
            {
                debit.PaidAmount = Math.Max(0, debit.PaidAmount - dtl.PaymentDetailAmount);
                debit.RemainAmount = Math.Min(debit.DebitAmount, debit.RemainAmount + dtl.PaymentDetailAmount);
                debit.Status = debit.PaidAmount == 0 ? InsuranceDebitStatus.Pending : InsuranceDebitStatus.PartiallyPaid;
            }
        }

        payment.Status = InsurancePaymentStatus.Cancelled;
        payment.CancelledBy = dto.CancelledBy ?? "KeToanTruong";
        payment.CancelledAt = DateTime.Now;
        payment.CancelReason = dto.Reason.Trim();

        await db.SaveChangesAsync();
        return payment;
    }

    // ==========================================
    // 3. Mẫu In Phiếu Thu Quyết Toán & Báo Cáo Tổng Hợp
    // ==========================================

    /// <summary>
    /// Sinh dữ liệu mẫu in Giấy Báo Thu Tiền Quyết Toán Bảo Hiểm (SerPaymentPaperRpt &amp; RptPaymentInvoice).
    /// </summary>
    public async Task<InsurancePaymentAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.InsurancePayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        var detailsAdvice = payment.Details.Select((d, idx) => new InsurancePaymentDetailAdviceDto
        {
            No = idx + 1,
            DebitNo = d.DebitNo,
            RONo = d.RONo,
            PlateNo = d.PlateNo,
            VIN = d.VIN,
            DebitAmount = d.DebitAmount,
            DebitAmountBefore = d.DebitAmountBefore,
            PaymentDetailAmount = d.PaymentDetailAmount,
            DebitAmountLeft = d.DebitAmountLeft,
            StatusAfterPayment = d.DebitAmountLeft == 0 ? "Tất toán" : "Còn nợ"
        }).ToList();

        string methodText = payment.PaymentMethod switch
        {
            InsurancePaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
            InsurancePaymentMethod.VnPay => "Cổng thanh toán VNPay QR",
            InsurancePaymentMethod.Momo => "Ví điện tử MoMo",
            InsurancePaymentMethod.Cash => "Tiền mặt tại quầy thu ngân",
            InsurancePaymentMethod.Offset => "Bù trừ công nợ đối ứng",
            _ => payment.PaymentMethod.ToString()
        };

        string statusText = payment.Status switch
        {
            InsurancePaymentStatus.Draft => "Bản nháp",
            InsurancePaymentStatus.Confirmed => "Đã xác nhận thu tiền",
            InsurancePaymentStatus.Settled => "Đã quyết toán hoàn tất",
            InsurancePaymentStatus.Cancelled => "Đã hủy",
            _ => payment.Status.ToString()
        };

        return new InsurancePaymentAdviceDto
        {
            PaymentNo = payment.PaymentNo,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            InsNo = payment.InsNo,
            InsName = payment.InsName,
            PayPersonName = payment.PayPersonName,
            PayPersonIDCardNo = payment.PayPersonIDCardNo,
            PayPersonPhone = payment.PayPersonPhone,
            PaymentMethodText = methodText,
            BankCode = payment.BankCode,
            BankName = payment.BankName,
            BankAccountNo = payment.BankAccountNo,
            BankTxnRef = payment.BankTxnRef,
            PaymentAmount = payment.PaymentAmount,
            PaymentAmountInWords = NumberToVietnameseWords(payment.PaymentAmount),
            TotalAllocated = payment.TotalAllocated,
            UnallocatedAmount = payment.UnallocatedAmount,
            StatusText = statusText,
            Note = payment.Note,
            CreatedBy = payment.CreatedBy,
            ConfirmedBy = payment.ConfirmedBy,
            SettledBy = payment.SettledBy,
            Details = detailsAdvice
        };
    }

    /// <summary>
    /// Dashboard KPI tổng hợp công nợ và quyết toán bảo hiểm.
    /// </summary>
    public async Task<InsuranceDebitSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var debits = await db.InsuranceClaimDebits
            .Where(d => d.OrgId == orgId && d.Status != InsuranceDebitStatus.Cancelled)
            .ToListAsync();

        var payments = await db.InsurancePayments
            .Where(p => p.OrgId == orgId && p.Status != InsurancePaymentStatus.Cancelled)
            .ToListAsync();

        int totalClaims = debits.Count;
        int pending = debits.Count(d => d.Status == InsuranceDebitStatus.Pending);
        int partially = debits.Count(d => d.Status == InsuranceDebitStatus.PartiallyPaid);
        int settled = debits.Count(d => d.Status == InsuranceDebitStatus.Settled);
        int cancelled = await db.InsuranceClaimDebits.CountAsync(d => d.OrgId == orgId && d.Status == InsuranceDebitStatus.Cancelled);

        long totalClaimAmt = debits.Sum(d => d.DebitAmount);
        long totalPaidAmt = debits.Sum(d => d.PaidAmount);
        long remainingDebt = debits.Sum(d => d.RemainAmount);
        decimal settlementRate = totalClaimAmt > 0 ? Math.Round((decimal)totalPaidAmt / totalClaimAmt * 100, 2) : 0;

        // Nhóm theo từng công ty bảo hiểm
        var companyStats = debits
            .GroupBy(d => new { d.InsNo, d.InsName })
            .Select(g => new InsuranceCompanyStatDto
            {
                InsNo = g.Key.InsNo,
                InsName = g.Key.InsName,
                ClaimCount = g.Count(),
                TotalClaimAmount = g.Sum(x => x.DebitAmount),
                TotalPaidAmount = g.Sum(x => x.PaidAmount),
                RemainingDebt = g.Sum(x => x.RemainAmount)
            })
            .OrderByDescending(c => c.RemainingDebt)
            .ToList();

        return new InsuranceDebitSummaryDto
        {
            TotalClaims = totalClaims,
            PendingClaims = pending,
            PartiallyPaidClaims = partially,
            SettledClaims = settled,
            CancelledClaims = cancelled,
            TotalClaimAmount = totalClaimAmt,
            TotalPaidAmount = totalPaidAmt,
            TotalRemainingDebt = remainingDebt,
            SettlementRate = settlementRate,
            TotalPaymentReceipts = payments.Count,
            TotalReceiptsAmount = payments.Sum(p => p.PaymentAmount),
            CompanyStats = companyStats
        };
    }

    // ==========================================
    // 4. Các Hàm Tiện Ích Nội Bộ (Helper Functions)
    // ==========================================

    private static string GetInsuranceNameByCode(string insNo) => insNo.ToUpperInvariant() switch
    {
        "INS-PTI" or "PTI" => "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
        "INS-BV" or "BAOVIET" => "Tổng Công ty Bảo hiểm Bảo Việt",
        "INS-PJICO" or "PJICO" => "Tổng Công ty Cổ phần Bảo hiểm Petrolimex (PJICO)",
        "INS-PVI" or "PVI" => "Tổng Công ty Bảo hiểm PVI (Dầu khí)",
        "INS-MIC" or "MIC" => "Tổng Công ty Cổ phần Bảo hiểm Quân đội (MIC)",
        "INS-BIC" or "BIC" => "Tổng Công ty Bảo hiểm BIDV (BIC)",
        "INS-LIBERTY" or "LIBERTY" => "Công ty TNHH Bảo hiểm Liberty Việt Nam",
        _ => $"Công ty Bảo hiểm {insNo}"
    };

    private static string GetModelNameByCode(string? modelCode) => (modelCode?.ToUpperInvariant()) switch
    {
        "SANTAFE" => "Hyundai Santa Fe 2.5 HTRAC",
        "TUCSON" => "Hyundai Tucson 2.0 AT",
        "CRETA" => "Hyundai Creta 1.5 Cao Cấp",
        "ACCENT" => "Hyundai Accent 1.5 AT Đặc Biệt",
        "ELANTRA" => "Hyundai Elantra N-Line 1.6 Turbo",
        "PALISADE" => "Hyundai Palisade 2.2D Prestige",
        "CUSTIN" => "Hyundai Custin 2.0 Turbo",
        "IONIQ5" => "Hyundai Ioniq 5 Electric AWD",
        _ => "Hyundai Passenger Car"
    };

    private static string GetBankNameByCode(string bankCode) => bankCode.ToUpperInvariant() switch
    {
        "CTG" or "VIETINBANK" => "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
        "MBB" or "MBBANK" => "Ngân hàng TMCP Quân Đội (MBBank)",
        "VCB" or "VIETCOMBANK" => "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
        "TCB" or "TECHCOMBANK" => "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
        "VPB" or "VPBANK" => "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
        "BIDV" => "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam (BIDV)",
        _ => $"Ngân hàng {bankCode}"
    };

    /// <summary>
    /// Thuật toán đọc số tiền thành chữ tiếng Việt tài chính ngân hàng chuẩn mực.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(Math.Abs(number));

        string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
        string res = "";
        int unitIdx = 0;

        while (number > 0)
        {
            long chunk = number % 1000;
            if (chunk > 0)
            {
                string chunkStr = ReadThreeDigits((int)chunk, number >= 1000);
                string unit = units[unitIdx];
                res = $"{chunkStr} {unit} {res}".Trim();
            }
            number /= 1000;
            unitIdx++;
        }

        res = res.Trim();
        if (res.Length > 0)
        {
            res = char.ToUpper(res[0]) + res.Substring(1);
        }
        return res + " đồng chẵn.";
    }

    private static string ReadThreeDigits(int n, bool hasHigherChunk)
    {
        string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        int h = n / 100;
        int t = (n % 100) / 10;
        int u = n % 10;
        string res = "";

        if (h > 0 || hasHigherChunk)
        {
            res += $"{digits[h]} trăm ";
        }

        if (t == 0)
        {
            if (u > 0 && (h > 0 || hasHigherChunk))
                res += $"lẻ {digits[u]}";
            else if (u > 0)
                res += digits[u];
        }
        else if (t == 1)
        {
            res += "mười ";
            if (u == 1) res += "một";
            else if (u == 5) res += "lăm";
            else if (u > 0) res += digits[u];
        }
        else
        {
            res += $"{digits[t]} mươi ";
            if (u == 1) res += "mốt";
            else if (u == 5) res += "lăm";
            else if (u > 0) res += digits[u];
        }

        return res.Trim();
    }
}
