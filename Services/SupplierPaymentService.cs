using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Quản lý Công Nợ &amp; Thanh Toán Quyết Toán Cho Nhà Cung Cấp Phụ Tùng / Dịch Vụ Xe Ô Tô.
/// Tương ứng BizCarSv.Debit.cs (SerSupplierDebitDetailGet, SerPaymentCreate, SerPaymentUpdate, SerPaymentDelete, SerPaymentPaperRpt)
/// và FrmSupplierPaymentCreate, FrmSupplierDebitSearch, FrmSuplierDebitCreate trong TERP.HTCServiceClient/Views/Debit hệ nguồn HTC 2010.
/// </summary>
public sealed class SupplierPaymentService(AppDbContext db)
{
    // ==========================================
    // 1. Quản lý Khoản Công Nợ Nhà Cung Cấp Phụ Tùng (SupplierDebit)
    // ==========================================

    /// <summary>
    /// Lập hồ sơ công nợ phải trả mới theo Phiếu nhập kho phụ tùng / vật tư.
    /// </summary>
    public async Task<SupplierDebit> CreateDebitAsync(Guid orgId, CreateSupplierDebitDto dto, string? createdBy = "ThuKhoPhuTung")
    {
        if (string.IsNullOrWhiteSpace(dto.SupplierCode))
            throw new ArgumentException("Mã nhà cung cấp (SupplierCode) không được để trống.", nameof(dto.SupplierCode));
        if (string.IsNullOrWhiteSpace(dto.StockInNo))
            throw new ArgumentException("Số phiếu nhập kho (StockInNo) không được để trống.", nameof(dto.StockInNo));
        if (dto.DebitAmount <= 0)
            throw new ArgumentException("Số tiền công nợ nhập kho phải lớn hơn 0.", nameof(dto.DebitAmount));

        string suppCode = dto.SupplierCode.Trim().ToUpperInvariant();
        string stockInNo = dto.StockInNo.Trim().ToUpperInvariant();

        // Kiểm tra xem phiếu nhập kho này đã được tạo hồ sơ nợ chưa
        bool exists = await db.SupplierDebits.AnyAsync(d =>
            d.OrgId == orgId && d.StockInNo == stockInNo && d.Status != SupplierDebitStatus.Cancelled);
        if (exists)
            throw new InvalidOperationException($"Phiếu nhập kho '{stockInNo}' đã có hồ sơ công nợ nhà cung cấp đang hoạt động.");

        // Sinh số hồ sơ nợ: DEB-SUPP-yyyyMM-xxx
        string monthPrefix = $"DEB-SUPP-{DateTime.Now:yyyyMM}-";
        int seq = await db.SupplierDebits
            .Where(d => d.OrgId == orgId && d.DebitNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string debitNo = string.IsNullOrWhiteSpace(dto.DebitNo) ? $"{monthPrefix}{seq:D3}" : dto.DebitNo.Trim();

        DateTime dDate = dto.DebitDate ?? DateTime.Now;
        DateTime duDate = dto.DueDate ?? dDate.AddDays(30);

        var (defName, defPhone, defAddr) = GetSupplierInfoByCode(suppCode);

        var debit = new SupplierDebit
        {
            OrgId = orgId,
            DebitNo = debitNo,
            SupplierCode = suppCode,
            SupplierName = string.IsNullOrWhiteSpace(dto.SupplierName) ? defName : dto.SupplierName.Trim(),
            SupplierPhone = string.IsNullOrWhiteSpace(dto.SupplierPhone) ? defPhone : dto.SupplierPhone.Trim(),
            SupplierAddress = string.IsNullOrWhiteSpace(dto.SupplierAddress) ? defAddr : dto.SupplierAddress.Trim(),
            StockInNo = stockInNo,
            StockInDate = dto.StockInDate ?? dDate,
            OrderPartNo = dto.OrderPartNo?.Trim().ToUpperInvariant(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Linh kiện & Phụ tùng chính hãng" : dto.Category.Trim(),
            DebitDate = dDate,
            DueDate = duDate,
            DebitAmount = dto.DebitAmount,
            PaidAmount = 0,
            RemainAmount = dto.DebitAmount,
            Status = SupplierDebitStatus.Pending,
            Note = dto.Note?.Trim() ?? $"Công nợ nhập kho linh kiện phụ tùng theo phiếu {stockInNo}",
            CreatedBy = createdBy ?? dto.CreatedBy ?? "ThuKhoPhuTung",
            CreatedAt = DateTime.Now
        };

        db.SupplierDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit;
    }

    /// <summary>
    /// Tìm kiếm danh sách công nợ phải trả nhà cung cấp.
    /// </summary>
    public async Task<List<SupplierDebit>> GetDebitsAsync(
        Guid orgId,
        string? supplierCode = null,
        SupplierDebitStatus? status = null,
        string? keyword = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.SupplierDebits.Where(d => d.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(supplierCode))
        {
            string code = supplierCode.Trim().ToUpperInvariant();
            q = q.Where(d => d.SupplierCode == code);
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
                             d.StockInNo.Contains(kw) ||
                             (d.OrderPartNo != null && d.OrderPartNo.Contains(kw)) ||
                             (d.Category != null && d.Category.ToUpper().Contains(kw)) ||
                             d.SupplierName.ToUpper().Contains(kw));
        }

        return await q.OrderByDescending(d => d.DebitDate).ToListAsync();
    }

    /// <summary>
    /// Chi tiết một khoản nợ nhà cung cấp.
    /// </summary>
    public async Task<SupplierDebit?> GetDebitByIdAsync(long id, Guid orgId)
    {
        return await db.SupplierDebits.FirstOrDefaultAsync(d => d.Id == id && d.OrgId == orgId);
    }

    /// <summary>
    /// Lấy danh sách các phiếu nhập kho còn nợ của một nhà cung cấp (để xem trước hoặc phân bổ thanh toán).
    /// </summary>
    public async Task<List<EligibleSupplierDebitDto>> GetEligibleDebitsAsync(Guid orgId, string supplierCode)
    {
        string code = supplierCode.Trim().ToUpperInvariant();
        var debits = await db.SupplierDebits
            .Where(d => d.OrgId == orgId && d.SupplierCode == code && d.RemainAmount > 0 && d.Status != SupplierDebitStatus.Cancelled)
            .OrderBy(d => d.DebitDate)
            .ToListAsync();

        return debits.Select(d => new EligibleSupplierDebitDto
        {
            Id = d.Id,
            DebitNo = d.DebitNo,
            StockInNo = d.StockInNo,
            OrderPartNo = d.OrderPartNo,
            Category = d.Category,
            DebitDate = d.DebitDate,
            DueDate = d.DueDate,
            DebitAmount = d.DebitAmount,
            PaidAmount = d.PaidAmount,
            RemainAmount = d.RemainAmount,
            Status = d.Status.ToString()
        }).ToList();
    }

    // ==========================================
    // 2. Quản lý Phiếu Chi Thanh Toán Nhà Cung Cấp & Phân Bổ Nợ FIFO (SupplierPayment)
    // ==========================================

    /// <summary>
    /// Lập phiếu chi thanh toán công nợ nhà cung cấp mới với thuật toán phân bổ nợ tự động FIFO (SerPaymentCreate).
    /// </summary>
    public async Task<SupplierPayment> CreatePaymentAsync(Guid orgId, CreateSupplierPaymentDto dto, string? createdBy = "KeToanThanhToan")
    {
        if (string.IsNullOrWhiteSpace(dto.SupplierCode))
            throw new ArgumentException("Mã nhà cung cấp (SupplierCode) không được để trống.", nameof(dto.SupplierCode));
        if (dto.PaymentAmount <= 0)
            throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.", nameof(dto.PaymentAmount));
        if (string.IsNullOrWhiteSpace(dto.PayPersonName))
            throw new ArgumentException("Tên người nhận tiền / đại diện thanh toán không được để trống.", nameof(dto.PayPersonName));

        string suppCode = dto.SupplierCode.Trim().ToUpperInvariant();

        // Sinh số phiếu chi: PM-SUPP-yyyyMM-xxx
        string monthPrefix = $"PM-SUPP-{DateTime.Now:yyyyMM}-";
        int seq = await db.SupplierPayments
            .Where(p => p.OrgId == orgId && p.PaymentNo.StartsWith(monthPrefix))
            .CountAsync() + 1;
        string paymentNo = string.IsNullOrWhiteSpace(dto.PaymentNo) ? $"{monthPrefix}{seq:D3}" : dto.PaymentNo.Trim();

        DateTime pDate = dto.PayDate ?? DateTime.Now;
        var pMethod = dto.PaymentMethod ?? SupplierPaymentMethod.BankTransfer;

        var (defName, defPhone, _) = GetSupplierInfoByCode(suppCode);

        var payment = new SupplierPayment
        {
            OrgId = orgId,
            PaymentNo = paymentNo,
            SupplierCode = suppCode,
            SupplierName = string.IsNullOrWhiteSpace(dto.SupplierName) ? defName : dto.SupplierName.Trim(),
            PayDate = pDate,
            PayPersonName = dto.PayPersonName.Trim(),
            PayPersonIDCardNo = dto.PayPersonIDCardNo?.Trim(),
            PayPersonPhone = string.IsNullOrWhiteSpace(dto.PayPersonPhone) ? defPhone : dto.PayPersonPhone?.Trim(),
            PaymentAmount = dto.PaymentAmount,
            PaymentMethod = pMethod,
            BankCode = dto.BankCode?.Trim().ToUpperInvariant(),
            BankName = string.IsNullOrWhiteSpace(dto.BankName) && !string.IsNullOrWhiteSpace(dto.BankCode) ? GetBankNameByCode(dto.BankCode) : dto.BankName?.Trim(),
            BankAccountNo = dto.BankAccountNo?.Trim(),
            BankTxnRef = dto.BankTxnRef?.Trim(),
            Status = SupplierPaymentStatus.Confirmed, // Mặc định xác nhận khi có đề nghị chi
            ConfirmedBy = createdBy,
            ConfirmedAt = DateTime.Now,
            Note = dto.Note?.Trim() ?? $"Chi tiền thanh toán công nợ nhà cung cấp {suppCode}",
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now
        };

        // Thuật toán phân bổ tự động FIFO theo SerPaymentCreate trong BizCarSv.Debit.cs
        long amountToAllocate = dto.PaymentAmount;
        long totalAllocated = 0;

        if (dto.AutoAllocateFifo)
        {
            // Lấy danh sách nợ còn tồn của nhà cung cấp sắp xếp thời gian phát sinh từ cũ đến mới (FIFO)
            var debits = await db.SupplierDebits
                .Where(d => d.OrgId == orgId && d.SupplierCode == suppCode && d.RemainAmount > 0 && d.Status != SupplierDebitStatus.Cancelled)
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
                d.Status = debitLeft == 0 ? SupplierDebitStatus.Settled : SupplierDebitStatus.PartiallyPaid;

                payment.Details.Add(new SupplierPaymentDetail
                {
                    OrgId = orgId,
                    DebitId = d.Id,
                    DebitNo = d.DebitNo,
                    StockInNo = d.StockInNo,
                    OrderPartNo = d.OrderPartNo,
                    DebitAmount = d.DebitAmount,
                    DebitAmountBefore = debitBefore,
                    PaymentDetailAmount = alloc,
                    DebitAmountLeft = debitLeft,
                    Remark = debitLeft == 0 ? "Tất toán toàn bộ nợ phiếu nhập kho" : "Thanh toán một phần nợ phiếu nhập kho"
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
                var d = await db.SupplierDebits.FirstOrDefaultAsync(x => x.Id == item.DebitId && x.OrgId == orgId);
                if (d == null || d.Status == SupplierDebitStatus.Cancelled) continue;

                long alloc = Math.Min(item.Amount, d.RemainAmount);
                long debitBefore = d.RemainAmount;
                long debitLeft = debitBefore - alloc;

                d.PaidAmount += alloc;
                d.RemainAmount = debitLeft;
                d.Status = debitLeft == 0 ? SupplierDebitStatus.Settled : SupplierDebitStatus.PartiallyPaid;

                payment.Details.Add(new SupplierPaymentDetail
                {
                    OrgId = orgId,
                    DebitId = d.Id,
                    DebitNo = d.DebitNo,
                    StockInNo = d.StockInNo,
                    OrderPartNo = d.OrderPartNo,
                    DebitAmount = d.DebitAmount,
                    DebitAmountBefore = debitBefore,
                    PaymentDetailAmount = alloc,
                    DebitAmountLeft = debitLeft,
                    Remark = debitLeft == 0 ? "Tất toán phiếu nhập (chỉ định)" : "Thanh toán một phần (chỉ định)"
                });

                totalAllocated += alloc;
                amountToAllocate -= alloc;
            }
        }

        payment.TotalAllocated = totalAllocated;
        payment.UnallocatedAmount = amountToAllocate > 0 ? amountToAllocate : 0;

        db.SupplierPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Tìm kiếm danh sách phiếu chi thanh toán nhà cung cấp.
    /// </summary>
    public async Task<List<SupplierPayment>> GetPaymentsAsync(
        Guid orgId,
        string? supplierCode = null,
        SupplierPaymentStatus? status = null,
        SupplierPaymentMethod? method = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var q = db.SupplierPayments
            .Include(p => p.Details)
            .Where(p => p.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(supplierCode))
        {
            string code = supplierCode.Trim().ToUpperInvariant();
            q = q.Where(p => p.SupplierCode == code);
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
    /// Chi tiết một phiếu chi thanh toán kèm chi tiết phân bổ nợ các phiếu nhập kho.
    /// </summary>
    public async Task<SupplierPayment?> GetPaymentByIdAsync(long id, Guid orgId)
    {
        return await db.SupplierPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
    }

    /// <summary>
    /// Kế toán trưởng thẩm định xác nhận phiếu chi (Draft -> Confirmed).
    /// </summary>
    public async Task<SupplierPayment> ConfirmPaymentAsync(long id, Guid orgId, ConfirmSupplierPaymentDto? dto)
    {
        var payment = await db.SupplierPayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu chi #{id}.");

        if (payment.Status != SupplierPaymentStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể xác nhận phiếu chi ở trạng thái 'Draft'. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = SupplierPaymentStatus.Confirmed;
        payment.ConfirmedBy = dto?.ConfirmedBy ?? "KeToanTruong";
        payment.ConfirmedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hoàn tất xuất quỹ / chuyển khoản ngân hàng và chốt sổ kế toán (Confirmed -> Settled).
    /// </summary>
    public async Task<SupplierPayment> SettlePaymentAsync(long id, Guid orgId, SettleSupplierPaymentDto? dto)
    {
        var payment = await db.SupplierPayments.FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);
        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu chi #{id}.");

        if (payment.Status != SupplierPaymentStatus.Confirmed && payment.Status != SupplierPaymentStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể quyết toán phiếu chi ở trạng thái 'Confirmed' hoặc 'Draft'. Trạng thái hiện tại: {payment.Status}.");

        payment.Status = SupplierPaymentStatus.Settled;
        payment.SettledBy = dto?.SettledBy ?? "KeToanThanhToan";
        payment.SettledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto?.BankTxnRef))
        {
            payment.BankTxnRef = dto.BankTxnRef.Trim();
        }

        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Hủy phiếu chi thanh toán nhà cung cấp và HOÀN TÁC (ROLLBACK) nợ trên các phiếu nhập kho tương ứng.
    /// </summary>
    public async Task<SupplierPayment> CancelPaymentAsync(long id, Guid orgId, CancelSupplierPaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Lý do hủy phiếu chi không được để trống.", nameof(dto.Reason));

        var payment = await db.SupplierPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu chi #{id}.");

        if (payment.Status == SupplierPaymentStatus.Cancelled)
            throw new InvalidOperationException($"Phiếu chi '{payment.PaymentNo}' đã bị hủy trước đó.");

        // Hoàn tác trừ nợ trên các phiếu nhập kho tương ứng
        foreach (var dtl in payment.Details)
        {
            var debit = await db.SupplierDebits.FirstOrDefaultAsync(x => x.Id == dtl.DebitId && x.OrgId == orgId);
            if (debit != null && debit.Status != SupplierDebitStatus.Cancelled)
            {
                debit.PaidAmount = Math.Max(0, debit.PaidAmount - dtl.PaymentDetailAmount);
                debit.RemainAmount = Math.Min(debit.DebitAmount, debit.RemainAmount + dtl.PaymentDetailAmount);
                debit.Status = debit.PaidAmount == 0 ? SupplierDebitStatus.Pending : SupplierDebitStatus.PartiallyPaid;
            }
        }

        payment.Status = SupplierPaymentStatus.Cancelled;
        payment.CancelledBy = dto.CancelledBy ?? "KeToanTruong";
        payment.CancelledAt = DateTime.Now;
        payment.CancelReason = dto.Reason.Trim();

        await db.SaveChangesAsync();
        return payment;
    }

    // ==========================================
    // 3. Mẫu In Phiếu Chi Quyết Toán & Báo Cáo Tổng Hợp
    // ==========================================

    /// <summary>
    /// Sinh dữ liệu mẫu in Giấy Báo Chi Tiền Thanh Toán Nhà Cung Cấp Phụ Tùng (SerPaymentPaperRpt &amp; FrmDebitShow).
    /// </summary>
    public async Task<SupplierPaymentAdviceDto?> GenerateAdviceAsync(long id, Guid orgId)
    {
        var payment = await db.SupplierPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (payment == null) return null;

        var (_, defPhone, defAddr) = GetSupplierInfoByCode(payment.SupplierCode);

        var detailsAdvice = payment.Details.Select((d, idx) => new SupplierPaymentDetailAdviceDto
        {
            No = idx + 1,
            DebitNo = d.DebitNo,
            StockInNo = d.StockInNo,
            OrderPartNo = d.OrderPartNo,
            DebitAmount = d.DebitAmount,
            DebitAmountBefore = d.DebitAmountBefore,
            PaymentDetailAmount = d.PaymentDetailAmount,
            DebitAmountLeft = d.DebitAmountLeft,
            StatusAfterPayment = d.DebitAmountLeft == 0 ? "Tất toán" : "Còn nợ"
        }).ToList();

        string methodText = payment.PaymentMethod switch
        {
            SupplierPaymentMethod.BankTransfer => "Ủy nhiệm chi chuyển khoản ngân hàng",
            SupplierPaymentMethod.VnPay => "Cổng thanh toán điện tử VNPay QR",
            SupplierPaymentMethod.Momo => "Ví điện tử MoMo B2B",
            SupplierPaymentMethod.Cash => "Tiền mặt xuất quỹ",
            SupplierPaymentMethod.Offset => "Bù trừ công nợ đối ứng linh kiện/phụ tùng",
            _ => payment.PaymentMethod.ToString()
        };

        string statusText = payment.Status switch
        {
            SupplierPaymentStatus.Draft => "Bản nháp",
            SupplierPaymentStatus.Confirmed => "Đã duyệt chi",
            SupplierPaymentStatus.Settled => "Đã hoàn tất thanh toán",
            SupplierPaymentStatus.Cancelled => "Đã hủy phiếu chi",
            _ => payment.Status.ToString()
        };

        return new SupplierPaymentAdviceDto
        {
            PaymentNo = payment.PaymentNo,
            PrintDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            SupplierCode = payment.SupplierCode,
            SupplierName = payment.SupplierName,
            SupplierAddress = defAddr,
            SupplierPhone = payment.PayPersonPhone ?? defPhone,
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
    /// Dashboard KPI tổng hợp công nợ và quyết toán nhà cung cấp.
    /// </summary>
    public async Task<SupplierDebitSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var debits = await db.SupplierDebits
            .Where(d => d.OrgId == orgId && d.Status != SupplierDebitStatus.Cancelled)
            .ToListAsync();

        var payments = await db.SupplierPayments
            .Where(p => p.OrgId == orgId && p.Status != SupplierPaymentStatus.Cancelled)
            .ToListAsync();

        int totalInvoices = debits.Count;
        int pending = debits.Count(d => d.Status == SupplierDebitStatus.Pending);
        int partially = debits.Count(d => d.Status == SupplierDebitStatus.PartiallyPaid);
        int settled = debits.Count(d => d.Status == SupplierDebitStatus.Settled);
        int cancelled = await db.SupplierDebits.CountAsync(d => d.OrgId == orgId && d.Status == SupplierDebitStatus.Cancelled);

        long totalDebtAmt = debits.Sum(d => d.DebitAmount);
        long totalPaidAmt = debits.Sum(d => d.PaidAmount);
        long remainingDebt = debits.Sum(d => d.RemainAmount);
        decimal settlementRate = totalDebtAmt > 0 ? Math.Round((decimal)totalPaidAmt / totalDebtAmt * 100, 2) : 0;

        // Nhóm theo từng nhà cung cấp
        var supplierStats = debits
            .GroupBy(d => new { d.SupplierCode, d.SupplierName })
            .Select(g => new SupplierStatDto
            {
                SupplierCode = g.Key.SupplierCode,
                SupplierName = g.Key.SupplierName,
                InvoiceCount = g.Count(),
                TotalDebtAmount = g.Sum(x => x.DebitAmount),
                TotalPaidAmount = g.Sum(x => x.PaidAmount),
                RemainingDebt = g.Sum(x => x.RemainAmount)
            })
            .OrderByDescending(s => s.RemainingDebt)
            .ToList();

        return new SupplierDebitSummaryDto
        {
            TotalInvoices = totalInvoices,
            PendingInvoices = pending,
            PartiallyPaidInvoices = partially,
            SettledInvoices = settled,
            CancelledInvoices = cancelled,
            TotalDebtAmount = totalDebtAmt,
            TotalPaidAmount = totalPaidAmt,
            TotalRemainingDebt = remainingDebt,
            SettlementRate = settlementRate,
            TotalPaymentReceipts = payments.Count,
            TotalReceiptsAmount = payments.Sum(p => p.PaymentAmount),
            SupplierStats = supplierStats
        };
    }

    // ==========================================
    // 4. Các Hàm Tiện Ích Nội Bộ (Helper Functions)
    // ==========================================

    private static (string Name, string Phone, string Address) GetSupplierInfoByCode(string supplierCode) => supplierCode.ToUpperInvariant() switch
    {
        "MOBIS-VN" or "MOBIS" => (
            "Công ty TNHH Mobis Auto Parts Việt Nam",
            "024-3768-9988",
            "Lô E3, KCN Thăng Long, Huyện Đông Anh, Hà Nội"
        ),
        "BOSCH-VN" or "BOSCH" => (
            "Công ty TNHH Robert Bosch Việt Nam",
            "028-6258-3690",
            "Tầng 14, Tòa nhà Deutsches Haus, 33 Lê Duẩn, Quận 1, TP. HCM"
        ),
        "CASTROL-VN" or "CASTROL" => (
            "Công ty TNHH Castrol BP Petco Việt Nam",
            "028-3821-9153",
            "Lầu 9, Tòa nhà Times Square, 22-36 Nguyễn Huệ, Quận 1, TP. HCM"
        ),
        "MICHELIN-VN" or "MICHELIN" => (
            "Công ty TNHH Michelin Việt Nam",
            "028-3824-3456",
            "Tầng 10, Tòa nhà Empress Tower, 138-142 Hai Bà Trưng, Quận 1, TP. HCM"
        ),
        "3M-VN" or "3M" => (
            "Công ty TNHH 3M Việt Nam",
            "028-5416-0429",
            "Tầng 20, Tòa nhà Mapletree Business Centre, 1060 Nguyễn Văn Linh, Quận 7, TP. HCM"
        ),
        "DENSO-VN" or "DENSO" => (
            "Công ty TNHH Denso Việt Nam",
            "024-3881-1606",
            "Lô E1, KCN Thăng Long, Huyện Đông Anh, Hà Nội"
        ),
        "PPG-VN" or "PPG" => (
            "Công ty TNHH Sơn PPG Việt Nam",
            "0274-375-7260",
            "Đường số 6, KCN Việt Hương 1, Thị xã Thuận An, Bình Dương"
        ),
        _ => (
            $"Nhà cung cấp {supplierCode}",
            "024-3999-8888",
            "Khu công nghiệp Gia Viễn, Tỉnh Ninh Bình"
        )
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

        if (t > 1)
        {
            res += $"{digits[t]} mươi ";
            if (u == 1) res += "mốt";
            else if (u == 5) res += "lăm";
            else if (u > 0) res += digits[u];
        }
        else if (t == 1)
        {
            res += "mười ";
            if (u == 5) res += "lăm";
            else if (u > 0) res += digits[u];
        }
        else // t == 0
        {
            if (h > 0 && u > 0)
            {
                res += $"lẻ {digits[u]}";
            }
            else if (u > 0)
            {
                res += digits[u];
            }
        }

        return res.Trim();
    }
}
