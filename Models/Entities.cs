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
