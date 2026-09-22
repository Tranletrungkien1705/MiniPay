using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

public sealed class ReconcileService(AppDbContext db)
{
    /// <summary>
    /// Chạy đối soát cho một đợt sao kê ngân hàng (tương ứng OS_DMS_TCF_WA_Bank_BankStatementDtl_Get trong BizHTC).
    /// So khớp từng dòng sao kê với PaymentIntent theo mã giao dịch và số tiền.
    /// </summary>
    public async Task<ReconcileBatch> ExecuteReconcileAsync(
        Guid orgId,
        string batchCode,
        string bankCode,
        string? accountNo,
        DateTime statementDate,
        List<BankStatementInputDto> statementItems,
        string? note = null)
    {
        // 1. Khởi tạo lô đối soát
        var batch = new ReconcileBatch
        {
            OrgId = orgId,
            BatchCode = batchCode,
            BankCode = bankCode,
            AccountNo = accountNo,
            StatementDate = statementDate,
            Note = note,
            TotalRecords = statementItems.Count,
            TotalAmount = statementItems.Sum(x => x.Amount),
            CreatedAt = DateTime.Now
        };

        db.ReconcileBatches.Add(batch);
        await db.SaveChangesAsync();

        // 2. Tải các giao dịch của Org để đối soát nhanh trong bộ nhớ (hoặc query theo batch)
        var systemPayments = await db.Payments
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        var refLookup = systemPayments
            .GroupBy(p => p.TxnRef, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var orderLookup = systemPayments
            .GroupBy(p => p.OrderId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        int matchedCount = 0;
        int mismatchedCount = 0;
        int unmatchedCount = 0;
        long matchedAmount = 0;

        var details = new List<ReconcileDetail>();

        foreach (var item in statementItems)
        {
            var detail = new ReconcileDetail
            {
                BatchId = batch.Id,
                OrgId = orgId,
                BankTxnNo = item.BankTxnNo,
                TxnRef = item.TxnRef,
                TxnTime = item.TxnTime ?? DateTime.Now,
                Amount = item.Amount,
                SenderAccount = item.SenderAccount,
                ReceiverAccount = item.ReceiverAccount ?? accountNo,
                Remark = item.Remark ?? ""
            };

            // Tìm PaymentIntent tương ứng
            PaymentIntent? matchedPayment = null;

            // 1. Thử khớp theo TxnRef trực tiếp nếu có
            if (!string.IsNullOrWhiteSpace(detail.TxnRef) && refLookup.TryGetValue(detail.TxnRef.Trim(), out var pByRef))
            {
                matchedPayment = pByRef;
            }
            // 2. Thử khớp theo OrderId
            else if (!string.IsNullOrWhiteSpace(detail.TxnRef) && orderLookup.TryGetValue(detail.TxnRef.Trim(), out var pByOrd))
            {
                matchedPayment = pByOrd;
            }
            // 3. Quét tìm TxnRef hoặc OrderId trong RemarkNDCT (nội dung chuyển khoản) giống cơ chế BizHTC
            if (matchedPayment == null && !string.IsNullOrWhiteSpace(detail.Remark))
            {
                matchedPayment = systemPayments.FirstOrDefault(p =>
                    detail.Remark.Contains(p.TxnRef, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(p.OrderId) && detail.Remark.Contains(p.OrderId, StringComparison.OrdinalIgnoreCase)));
            }

            if (matchedPayment != null)
            {
                detail.PaymentIntentId = matchedPayment.Id;
                detail.SystemAmount = matchedPayment.Amount;
                if (string.IsNullOrWhiteSpace(detail.TxnRef))
                    detail.TxnRef = matchedPayment.TxnRef;

                if (detail.Amount == matchedPayment.Amount)
                {
                    detail.MatchStatus = MatchStatus.Matched;
                    detail.DiscrepancyReason = "Khớp chuẩn mã giao dịch và số tiền.";
                    detail.MatchedAt = DateTime.Now;

                    matchedCount++;
                    matchedAmount += detail.Amount;

                    // Nếu giao dịch hệ thống còn Pending, tự động xác nhận đã thanh toán qua đối soát
                    if (matchedPayment.Status == PayStatus.Pending)
                    {
                        matchedPayment.Status = PayStatus.Paid;
                        matchedPayment.PaidAt = detail.TxnTime;
                        matchedPayment.ResponseCode = "00";
                        matchedPayment.BankCode = bankCode;
                        matchedPayment.VnpTransactionNo = detail.BankTxnNo;
                    }
                }
                else
                {
                    detail.MatchStatus = MatchStatus.AmountMismatch;
                    var diff = detail.Amount - matchedPayment.Amount;
                    detail.DiscrepancyReason = $"Lệch số tiền: Sao kê {detail.Amount:N0} đ, Hệ thống {matchedPayment.Amount:N0} đ (chênh lệch: {(diff > 0 ? "+" : "")}{diff:N0} đ).";
                    mismatchedCount++;
                }
            }
            else
            {
                detail.MatchStatus = MatchStatus.NotFound;
                detail.DiscrepancyReason = "Không tìm thấy giao dịch tương ứng trong hệ thống.";
                unmatchedCount++;
            }

            details.Add(detail);
        }

        db.ReconcileDetails.AddRange(details);

        // Cập nhật tổng kết lô đối soát
        batch.MatchedCount = matchedCount;
        batch.MismatchedCount = mismatchedCount;
        batch.UnmatchedCount = unmatchedCount;
        batch.MatchedAmount = matchedAmount;
        batch.Status = (mismatchedCount > 0 || unmatchedCount > 0) ? ReconcileStatus.Discrepant : ReconcileStatus.Completed;
        batch.CompletedAt = DateTime.Now;

        await db.SaveChangesAsync();
        batch.Details = details;
        return batch;
    }

    /// <summary>
    /// Chạy lại đối soát cho 1 đợt đã có (ví dụ sau khi nhập thêm giao dịch vào hệ thống).
    /// </summary>
    public async Task<ReconcileBatch?> ReRunBatchAsync(long batchId, Guid orgId)
    {
        var batch = await db.ReconcileBatches
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.OrgId == orgId);

        if (batch == null) return null;

        var systemPayments = await db.Payments
            .Where(p => p.OrgId == orgId)
            .ToListAsync();

        var refLookup = systemPayments
            .GroupBy(p => p.TxnRef, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var orderLookup = systemPayments
            .GroupBy(p => p.OrderId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        int matchedCount = 0;
        int mismatchedCount = 0;
        int unmatchedCount = 0;
        long matchedAmount = 0;

        foreach (var detail in batch.Details)
        {
            PaymentIntent? matchedPayment = null;

            if (!string.IsNullOrWhiteSpace(detail.TxnRef) && refLookup.TryGetValue(detail.TxnRef.Trim(), out var pByRef))
            {
                matchedPayment = pByRef;
            }
            else if (!string.IsNullOrWhiteSpace(detail.TxnRef) && orderLookup.TryGetValue(detail.TxnRef.Trim(), out var pByOrd))
            {
                matchedPayment = pByOrd;
            }
            if (matchedPayment == null && !string.IsNullOrWhiteSpace(detail.Remark))
            {
                matchedPayment = systemPayments.FirstOrDefault(p =>
                    detail.Remark.Contains(p.TxnRef, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(p.OrderId) && detail.Remark.Contains(p.OrderId, StringComparison.OrdinalIgnoreCase)));
            }

            if (matchedPayment != null)
            {
                detail.PaymentIntentId = matchedPayment.Id;
                detail.SystemAmount = matchedPayment.Amount;
                if (string.IsNullOrWhiteSpace(detail.TxnRef))
                    detail.TxnRef = matchedPayment.TxnRef;

                if (detail.Amount == matchedPayment.Amount)
                {
                    detail.MatchStatus = MatchStatus.Matched;
                    detail.DiscrepancyReason = "Khớp chuẩn mã giao dịch và số tiền.";
                    detail.MatchedAt = DateTime.Now;
                    matchedCount++;
                    matchedAmount += detail.Amount;

                    if (matchedPayment.Status == PayStatus.Pending)
                    {
                        matchedPayment.Status = PayStatus.Paid;
                        matchedPayment.PaidAt = detail.TxnTime;
                        matchedPayment.ResponseCode = "00";
                        matchedPayment.BankCode = batch.BankCode;
                        matchedPayment.VnpTransactionNo = detail.BankTxnNo;
                    }
                }
                else
                {
                    detail.MatchStatus = MatchStatus.AmountMismatch;
                    var diff = detail.Amount - matchedPayment.Amount;
                    detail.DiscrepancyReason = $"Lệch số tiền: Sao kê {detail.Amount:N0} đ, Hệ thống {matchedPayment.Amount:N0} đ (chênh lệch: {(diff > 0 ? "+" : "")}{diff:N0} đ).";
                    mismatchedCount++;
                }
            }
            else
            {
                detail.MatchStatus = MatchStatus.NotFound;
                detail.DiscrepancyReason = "Không tìm thấy giao dịch tương ứng trong hệ thống.";
                unmatchedCount++;
            }
        }

        batch.MatchedCount = matchedCount;
        batch.MismatchedCount = mismatchedCount;
        batch.UnmatchedCount = unmatchedCount;
        batch.MatchedAmount = matchedAmount;
        batch.Status = (mismatchedCount > 0 || unmatchedCount > 0) ? ReconcileStatus.Discrepant : ReconcileStatus.Completed;
        batch.CompletedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return batch;
    }
}

public sealed record BankStatementInputDto(
    string BankTxnNo,
    string? TxnRef,
    long Amount,
    string? Remark,
    DateTime? TxnTime,
    string? SenderAccount,
    string? ReceiverAccount);
