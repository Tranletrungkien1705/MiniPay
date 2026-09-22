namespace MiniPay.Models;

/// <summary>Tổ chức (merchant tenant). Mỗi org 1 ApiKey để tạo giao dịch.</summary>
public sealed class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum PayStatus { Pending = 0, Paid = 1, Failed = 2 }

public enum ReconcileStatus { Pending = 0, Completed = 1, Discrepant = 2 }

public enum MatchStatus { Pending = 0, Matched = 1, AmountMismatch = 2, NotFound = 3 }

/// <summary>Ý định thanh toán (payment intent) — 1 dòng / 1 lần khởi tạo cổng.</summary>
public sealed class PaymentIntent
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string TxnRef { get; set; } = "";      // vnp_TxnRef (duy nhất)
    public string OrderId { get; set; } = "";      // mã đơn phía merchant
    public long Amount { get; set; }               // VND (đồng), chưa ×100
    public string OrderInfo { get; set; } = "";
    public string Provider { get; set; } = "vnpay";
    public PayStatus Status { get; set; } = PayStatus.Pending;
    public string? ResponseCode { get; set; }      // vnp_ResponseCode
    public string? BankCode { get; set; }
    public string? VnpTransactionNo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? PaidAt { get; set; }
}

/// <summary>Đợt đối soát sao kê ngân hàng (Reconcile Batch) — tương ứng Bank_BankStatement trong BizHTC.</summary>
public sealed class ReconcileBatch
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string BatchCode { get; set; } = "";           // Mã đợt đối soát (ví dụ REC-20250514-001)
    public string BankCode { get; set; } = "";            // Mã ngân hàng: VCB, TCB, MB, VIB, VNPAY, MOMO
    public string? AccountNo { get; set; }                // Số tài khoản nhận sao kê
    public DateTime StatementDate { get; set; } = DateTime.Today; // Ngày/kỳ sao kê
    public int TotalRecords { get; set; }                 // Tổng số dòng giao dịch sao kê
    public int MatchedCount { get; set; }                 // Số dòng khớp thành công
    public int MismatchedCount { get; set; }              // Số dòng lệch số tiền
    public int UnmatchedCount { get; set; }               // Số dòng không tìm thấy trên hệ thống
    public long TotalAmount { get; set; }                 // Tổng tiền sao kê (VND)
    public long MatchedAmount { get; set; }               // Tổng tiền đã khớp (VND)
    public ReconcileStatus Status { get; set; } = ReconcileStatus.Pending;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    public List<ReconcileDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng sao kê đối soát — tương ứng Bank_BankStatementDtl trong BizHTC.</summary>
public sealed class ReconcileDetail
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public Guid OrgId { get; set; }
    public string BankTxnNo { get; set; } = "";           // Mã giao dịch ngân hàng / AutoId sao kê
    public string? TxnRef { get; set; }                   // Mã đơn/giao dịch trích xuất từ nội dung
    public DateTime TxnTime { get; set; } = DateTime.Now; // Thời điểm giao dịch ngân hàng
    public long Amount { get; set; }                      // Số tiền ghi có (ValGhiCo)
    public string? SenderAccount { get; set; }            // Tài khoản gửi
    public string? ReceiverAccount { get; set; }          // Tài khoản nhận
    public string Remark { get; set; } = "";              // Nội dung chuyển khoản (RemarkNDCT)
    public MatchStatus MatchStatus { get; set; } = MatchStatus.Pending;
    public long? PaymentIntentId { get; set; }            // Id giao dịch hệ thống nếu khớp
    public long? SystemAmount { get; set; }               // Số tiền ghi nhận trên hệ thống
    public string? DiscrepancyReason { get; set; }         // Diễn giải kết quả / lý do sai lệch
    public DateTime? MatchedAt { get; set; }
}
