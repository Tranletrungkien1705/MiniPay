using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ quản lý Phiếu thanh toán & Ủy nhiệm chi ngân hàng (Payment Order & Bank UNC Settlement).
/// Tương ứng khối Pmt_Payment & Pmt_PaymentDetail, FrmMngPM, FrmNewPM, FrmUpdate_Pmt_Payment trong BizHTC.Payment.
/// </summary>
public sealed class PaymentOrderService(AppDbContext db)
{
    /// <summary>
    /// Tạo mới phiếu thanh toán & lập chứng từ UNC (Pmt_Payment_Save / FrmNewPM.MODE_NEW_PM).
    /// </summary>
    public async Task<PaymentOrder> CreatePaymentOrderAsync(
        Guid orgId,
        string? paymentNo,
        PaymentOrderType paymentType,
        string? bankPaymentNo,
        DateTime? paymentEndDate,
        string partnerCode,
        string? partnerName,
        string bankCodeSend,
        string? bankNameSend,
        string bankAccountSend,
        string bankCodeReceive,
        string? bankNameReceive,
        string bankAccountReceive,
        PaymentFundType funds,
        string? bankLending,
        decimal? interestRate,
        int? loanPeriodMonths,
        string? accountingRecordNo,
        string? remark,
        string? createdBy,
        List<PaymentOrderItemInputDto> items)
    {
        if (string.IsNullOrWhiteSpace(partnerCode))
            throw new ArgumentException("Mã đối tác / đại lý (PartnerCode / DealerCode) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankAccountSend))
            throw new ArgumentException("Số tài khoản trích nợ (BankAccountSend) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankCodeSend))
            throw new ArgumentException("Ngân hàng trích nợ (BankCodeSend) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankAccountReceive))
            throw new ArgumentException("Số tài khoản thụ hưởng (BankAccountReceive) không được để trống.");
        if (string.IsNullOrWhiteSpace(bankCodeReceive))
            throw new ArgumentException("Ngân hàng thụ hưởng (BankCodeReceive) không được để trống.");

        if (funds == PaymentFundType.BankLoan)
        {
            if (string.IsNullOrWhiteSpace(bankLending))
                throw new ArgumentException("Nguồn tiền vay ngân hàng yêu cầu chọn Ngân hàng cho vay (BankLending).");
            if (!interestRate.HasValue || interestRate.Value <= 0 || interestRate.Value > 100)
                throw new ArgumentException("Lãi suất vay phải lớn hơn 0% và không quá 100%/năm.");
            if (!loanPeriodMonths.HasValue || loanPeriodMonths.Value <= 0)
                throw new ArgumentException("Kỳ hạn vay phải lớn hơn 0 tháng.");
        }

        if (items == null || items.Count == 0)
            throw new ArgumentException("Phiếu thanh toán phải có ít nhất 1 dòng xe / đơn hàng (ListPMDetail).");

        var finalPaymentNo = string.IsNullOrWhiteSpace(paymentNo)
            ? $"PM-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}"
            : paymentNo.Trim();

        var exists = await db.PaymentOrders.AnyAsync(p => p.OrgId == orgId && p.PaymentNo == finalPaymentNo);
        if (exists)
            throw new InvalidOperationException($"Số phiếu thanh toán '{finalPaymentNo}' đã tồn tại trong hệ thống.");

        long totalAmount = 0;
        long totalAccumAmount = 0;
        var details = new List<PaymentOrderDetail>();

        int idx = 1;
        foreach (var item in items)
        {
            if (item.UnitPriceActual <= 0)
                throw new ArgumentException($"Dòng #{idx}: Đơn giá thực tế xe/hàng (UnitPriceActual) phải > 0.");
            if (item.Amount <= 0)
                throw new ArgumentException($"Dòng #{idx}: Số tiền thanh toán đợt này (Amount) phải > 0.");

            long itemAccum = item.AmountAccum >= 0 ? item.AmountAccum : 0;
            long itemTotal = item.Amount + itemAccum;

            // Kiểm tra quy tắc nghiệp vụ BizHTC: Tích lũy thanh toán không vượt quá đơn giá xe (myPmt_Payment_CheckTotalValue)
            if (itemTotal > item.UnitPriceActual)
            {
                throw new InvalidOperationException(
                    $"Dòng #{idx} ({item.ItemRefNo}): Tổng tích lũy thanh toán ({itemTotal:N0} đ) vượt quá đơn giá thực tế ({item.UnitPriceActual:N0} đ)!");
            }

            var pctAccum = item.UnitPriceActual > 0
                ? Math.Round((decimal)itemAccum * 100m / item.UnitPriceActual, 2)
                : 0m;
            var pctCur = item.UnitPriceActual > 0
                ? Math.Round((decimal)item.Amount * 100m / item.UnitPriceActual, 2)
                : 0m;
            var pctTotal = Math.Round(pctAccum + pctCur, 2);

            totalAmount += item.Amount;
            totalAccumAmount += itemTotal;

            details.Add(new PaymentOrderDetail
            {
                OrgId = orgId,
                ItemRefNo = string.IsNullOrWhiteSpace(item.ItemRefNo) ? $"ITEM-{idx:D2}" : item.ItemRefNo.Trim(),
                Description = string.IsNullOrWhiteSpace(item.Description)
                    ? $"Thanh toán đơn hàng / xe #{idx}"
                    : item.Description.Trim(),
                ModelCode = item.ModelCode?.Trim(),
                UnitPriceActual = item.UnitPriceActual,
                AmountAccum = itemAccum,
                PercentAccum = pctAccum,
                Amount = item.Amount,
                PercentCurrent = pctCur,
                AmountTotal = itemTotal,
                PercentTotal = pctTotal,
                GuaranteeNo = item.GuaranteeNo?.Trim(),
                BankGrtNo = item.BankGrtNo?.Trim(),
                Status = PaymentOrderDetailStatus.Pending,
                Note = item.Note
            });

            idx++;
        }

        var order = new PaymentOrder
        {
            OrgId = orgId,
            PaymentNo = finalPaymentNo,
            PaymentType = paymentType,
            BankPaymentNo = bankPaymentNo?.Trim(),
            PaymentEndDate = paymentEndDate ?? DateTime.Today.AddDays(30),
            PartnerCode = partnerCode.Trim().ToUpper(),
            PartnerName = string.IsNullOrWhiteSpace(partnerName) ? partnerCode.Trim() : partnerName.Trim(),
            BankCodeSend = bankCodeSend.Trim().ToUpper(),
            BankNameSend = bankNameSend?.Trim(),
            BankAccountSend = bankAccountSend.Trim(),
            BankCodeReceive = bankCodeReceive.Trim().ToUpper(),
            BankNameReceive = bankNameReceive?.Trim(),
            BankAccountReceive = bankAccountReceive.Trim(),
            Funds = funds,
            BankLending = bankLending?.Trim().ToUpper(),
            InterestRate = interestRate ?? 0m,
            LoanPeriodMonths = loanPeriodMonths ?? 0,
            AccountingRecordNo = accountingRecordNo?.Trim(),
            TotalAmount = totalAmount,
            TotalAccumAmount = totalAccumAmount,
            Status = PaymentOrderStatus.PendingApproval,
            Remark = string.IsNullOrWhiteSpace(remark)
                ? $"Thanh toan don hang {finalPaymentNo} - {partnerCode}"
                : remark.Trim(),
            CreatedBy = createdBy ?? "KeToanThanhToan",
            CreatedAt = DateTime.Now,
            Details = details
        };

        db.PaymentOrders.Add(order);
        await db.SaveChangesAsync();

        return order;
    }

    /// <summary>
    /// Phê duyệt phiếu thanh toán (FrmMngPM.btnApprove_Click / FrmNewPM.MODE_APPROVE_PM).
    /// </summary>
    public async Task<PaymentOrder?> ApprovePaymentOrderAsync(long id, Guid orgId, string? approverName)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        if (order.Status != PaymentOrderStatus.PendingApproval && order.Status != PaymentOrderStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể phê duyệt phiếu ở trạng thái Chờ duyệt hoặc Nháp (Hiện tại: {order.Status}).");

        order.Status = PaymentOrderStatus.Approved;
        order.ApprovedBy = approverName ?? "KTT_Approve";
        order.ApprovedAt = DateTime.Now;

        foreach (var dtl in order.Details)
        {
            if (dtl.Status == PaymentOrderDetailStatus.Pending)
                dtl.Status = PaymentOrderDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Xác nhận hoàn tất thanh toán & hạn thanh toán thực tế (FrmMngPM.btnConfirmEndDate_Click).
    /// </summary>
    public async Task<PaymentOrder?> ConfirmFinishPaymentOrderAsync(
        long id,
        Guid orgId,
        DateTime? paymentEndDate,
        string? finisherName,
        string? bankPaymentNo)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        if (order.Status != PaymentOrderStatus.Approved)
            throw new InvalidOperationException($"Chỉ có thể hoàn tất thanh toán cho phiếu Đã duyệt (Hiện tại: {order.Status}).");

        order.Status = PaymentOrderStatus.Finished;
        order.FinishedBy = finisherName ?? "KeToanNganHang";
        order.FinishedAt = DateTime.Now;

        if (paymentEndDate.HasValue)
            order.PaymentEndDate = paymentEndDate.Value;

        if (!string.IsNullOrWhiteSpace(bankPaymentNo))
            order.BankPaymentNo = bankPaymentNo.Trim();

        foreach (var dtl in order.Details)
        {
            if (dtl.Status == PaymentOrderDetailStatus.Approved)
                dtl.Status = PaymentOrderDetailStatus.Finished;
        }

        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Quay lui trạng thái phiếu thanh toán từ Finished về Approved (FrmMngPM.btnRevertConfirmed_Click).
    /// </summary>
    public async Task<PaymentOrder?> RevertPaymentOrderAsync(long id, Guid orgId)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        if (order.Status != PaymentOrderStatus.Finished)
            throw new InvalidOperationException($"Chỉ có thể quay lui phiếu ở trạng thái Đã hoàn tất (Finished). Hiện tại: {order.Status}.");

        order.Status = PaymentOrderStatus.Approved;
        order.FinishedBy = null;
        order.FinishedAt = null;

        foreach (var dtl in order.Details)
        {
            if (dtl.Status == PaymentOrderDetailStatus.Finished)
                dtl.Status = PaymentOrderDetailStatus.Approved;
        }

        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Từ chối phê duyệt phiếu thanh toán (FrmMngPM.btnDealerReject_Click).
    /// </summary>
    public async Task<PaymentOrder?> RejectPaymentOrderAsync(long id, Guid orgId, string? reason, string? rejecterName)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        if (order.Status != PaymentOrderStatus.PendingApproval && order.Status != PaymentOrderStatus.Draft)
            throw new InvalidOperationException($"Chỉ có thể từ chối phiếu ở trạng thái Chờ duyệt hoặc Nháp (Hiện tại: {order.Status}).");

        order.Status = PaymentOrderStatus.Rejected;
        order.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Kế toán từ chối chứng từ thanh toán." : reason.Trim();
        order.ApprovedBy = rejecterName ?? "KTT_Reject";
        order.ApprovedAt = DateTime.Now;

        foreach (var dtl in order.Details)
        {
            dtl.Status = PaymentOrderDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Hủy phiếu thanh toán (FrmMngPM.btnCancel_Click).
    /// </summary>
    public async Task<PaymentOrder?> CancelPaymentOrderAsync(long id, Guid orgId, string? reason)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        if (order.Status == PaymentOrderStatus.Finished || order.Status == PaymentOrderStatus.Cancelled)
            throw new InvalidOperationException($"Không thể hủy phiếu thanh toán ở trạng thái {order.Status}.");

        order.Status = PaymentOrderStatus.Cancelled;
        order.CancelledAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            order.RejectReason = reason.Trim();
        }

        foreach (var dtl in order.Details)
        {
            dtl.Status = PaymentOrderDetailStatus.Cancelled;
        }

        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Sinh nội dung mẫu Ủy nhiệm chi ngân hàng (Pmt_Payment_GetUNCContent_New20221111).
    /// </summary>
    public async Task<UNCAdviceDto?> GenerateUNCContentAsync(long id, Guid orgId)
    {
        var order = await db.PaymentOrders
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == orgId);

        if (order == null) return null;

        var amountInWords = NumberToVietnameseWords(order.TotalAmount);

        // Sinh danh sách số khung / xe / đơn tham chiếu
        var itemSummary = string.Join(", ", order.Details.Select(d => d.ItemRefNo));
        var uncContent = $"Thanh toan tien xe/hang hoa dot {order.PaymentNo} cho {order.PartnerCode}. DS: {itemSummary}";

        return new UNCAdviceDto(
            OrderNo: order.PaymentNo,
            BankPaymentNo: order.BankPaymentNo ?? "UNC-" + order.BankCodeSend + "-" + DateTime.Now.ToString("yyyyMMdd-HHmm"),
            SenderUnit: order.PartnerName,
            SenderAccount: order.BankAccountSend,
            SenderBank: order.BankNameSend ?? order.BankCodeSend,
            BeneficiaryUnit: "CONG TY CO PHAN LIEN DOANH O TO HYUNDAI THANH CONG VIET NAM",
            BeneficiaryAccount: order.BankAccountReceive,
            BeneficiaryBank: order.BankNameReceive ?? order.BankCodeReceive,
            AmountNumber: order.TotalAmount,
            AmountWords: amountInWords,
            TransferRemark: uncContent,
            PaymentType: order.PaymentType.ToString(),
            Funds: order.Funds.ToString(),
            BankLending: order.BankLending,
            AccountingRecordNo: order.AccountingRecordNo,
            Status: order.Status.ToString(),
            GeneratedAt: DateTime.Now
        );
    }

    /// <summary>
    /// Cập nhật nhanh lãi suất và kỳ hạn vay theo lô (Pmt_Payment_UpdateInterestRate_LoanPeriod / FrmUpdate_Pmt_Payment).
    /// </summary>
    public async Task<int> UpdateInterestRateLoanPeriodAsync(
        Guid orgId,
        List<long> paymentIds,
        decimal interestRate,
        int loanPeriodMonths)
    {
        if (interestRate <= 0 || interestRate > 100)
            throw new ArgumentException("Lãi suất vay phải lớn hơn 0 và không vượt quá 100%/năm.");
        if (loanPeriodMonths <= 0)
            throw new ArgumentException("Kỳ hạn vay phải lớn hơn 0 tháng.");
        if (paymentIds == null || paymentIds.Count == 0)
            return 0;

        var orders = await db.PaymentOrders
            .Where(p => p.OrgId == orgId && paymentIds.Contains(p.Id))
            .ToListAsync();

        foreach (var order in orders)
        {
            order.InterestRate = interestRate;
            order.LoanPeriodMonths = loanPeriodMonths;
        }

        await db.SaveChangesAsync();
        return orders.Count;
    }

    /// <summary>
    /// Thống kê tổng hợp số liệu thanh toán (Payment Order Summary).
    /// </summary>
    public async Task<PaymentOrderSummaryDto> GetSummaryAsync(Guid orgId)
    {
        var orders = await db.PaymentOrders
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        return new PaymentOrderSummaryDto(
            TotalOrders: orders.Count,
            PendingCount: orders.Count(o => o.Status == PaymentOrderStatus.PendingApproval || o.Status == PaymentOrderStatus.Draft),
            ApprovedCount: orders.Count(o => o.Status == PaymentOrderStatus.Approved),
            FinishedCount: orders.Count(o => o.Status == PaymentOrderStatus.Finished),
            RejectedCount: orders.Count(o => o.Status == PaymentOrderStatus.Rejected),
            CancelledCount: orders.Count(o => o.Status == PaymentOrderStatus.Cancelled),
            TotalPaymentAmount: orders.Sum(o => o.TotalAmount),
            FinishedAmount: orders.Where(o => o.Status == PaymentOrderStatus.Finished).Sum(o => o.TotalAmount),
            PendingAmount: orders.Where(o => o.Status == PaymentOrderStatus.PendingApproval).Sum(o => o.TotalAmount),
            TotalDepositOrders: orders.Count(o => o.PaymentType == PaymentOrderType.Deposit),
            TotalUNCOrders: orders.Count(o => o.PaymentType == PaymentOrderType.UNC),
            TotalGuaranteeOrders: orders.Count(o => o.PaymentType == PaymentOrderType.GuaranteePayment),
            TotalBankLoanOrders: orders.Count(o => o.Funds == PaymentFundType.BankLoan)
        );
    }

    /// <summary>
    /// Chuyển đổi số tiền thành chữ tiếng Việt chuẩn hóa phục vụ in Ủy nhiệm chi ngân hàng.
    /// </summary>
    public static string NumberToVietnameseWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + NumberToVietnameseWords(Math.Abs(number));

        string[] units = ["", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ"];
        string words = "";
        int unitIndex = 0;

        while (number > 0)
        {
            long chunk = number % 1000;
            if (chunk > 0)
            {
                string chunkWords = ChunkToWords((int)chunk);
                words = chunkWords + units[unitIndex] + (string.IsNullOrWhiteSpace(words) ? "" : " " + words);
            }
            number /= 1000;
            unitIndex++;
        }

        words = words.Trim();
        if (words.Length > 0)
        {
            words = char.ToUpper(words[0]) + words[1..] + " đồng chẵn.";
        }
        return words;
    }

    private static string ChunkToWords(int n)
    {
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
        int hundreds = n / 100;
        int remainder = n % 100;
        int tens = remainder / 10;
        int ones = remainder % 10;

        var result = new List<string>();

        if (hundreds > 0)
        {
            result.Add(digits[hundreds] + " trăm");
        }

        if (tens > 1)
        {
            result.Add(digits[tens] + " mươi");
            if (ones == 1) result.Add("mốt");
            else if (ones == 5) result.Add("lăm");
            else if (ones > 0) result.Add(digits[ones]);
        }
        else if (tens == 1)
        {
            result.Add("mười");
            if (ones == 5) result.Add("lăm");
            else if (ones > 0) result.Add(digits[ones]);
        }
        else if (tens == 0 && ones > 0)
        {
            if (hundreds > 0) result.Add("lẻ");
            result.Add(digits[ones]);
        }

        return string.Join(" ", result);
    }
}

public sealed record PaymentOrderItemInputDto(
    string ItemRefNo,
    string? Description,
    string? ModelCode,
    long UnitPriceActual,
    long AmountAccum,
    long Amount,
    string? GuaranteeNo,
    string? BankGrtNo,
    string? Note
);

public sealed record UNCAdviceDto(
    string OrderNo,
    string BankPaymentNo,
    string SenderUnit,
    string SenderAccount,
    string SenderBank,
    string BeneficiaryUnit,
    string BeneficiaryAccount,
    string BeneficiaryBank,
    long AmountNumber,
    string AmountWords,
    string TransferRemark,
    string PaymentType,
    string Funds,
    string? BankLending,
    string? AccountingRecordNo,
    string Status,
    DateTime GeneratedAt
);

public sealed record PaymentOrderSummaryDto(
    int TotalOrders,
    int PendingCount,
    int ApprovedCount,
    int FinishedCount,
    int RejectedCount,
    int CancelledCount,
    long TotalPaymentAmount,
    long FinishedAmount,
    long PendingAmount,
    int TotalDepositOrders,
    int TotalUNCOrders,
    int TotalGuaranteeOrders,
    int TotalBankLoanOrders
);
