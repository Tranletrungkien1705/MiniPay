using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MiniPay.Services;

/// <summary>
/// Cổng VNPay v2.1.0: dựng URL thanh toán ký HMAC-SHA512 + xác thực chữ ký callback (Return/IPN).
/// Cấu hình qua env: VNP_TMNCODE, VNP_HASHSECRET, VNP_PAYURL (sandbox mặc định). Không hardcode secret thật.
/// </summary>
public sealed class VnPayService
{
    public string TmnCode { get; }
    private readonly string _hashSecret;
    public string PayUrl { get; }

    public VnPayService(IConfiguration cfg)
    {
        TmnCode = Environment.GetEnvironmentVariable("VNP_TMNCODE") ?? cfg["VNP_TMNCODE"] ?? "2QXUI4J4";
        _hashSecret = Environment.GetEnvironmentVariable("VNP_HASHSECRET") ?? cfg["VNP_HASHSECRET"]
            ?? "RAOEXHYVSDDIIENYWSLDIIZTANRPADBB";
        PayUrl = Environment.GetEnvironmentVariable("VNP_PAYURL") ?? cfg["VNP_PAYURL"]
            ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    }

    /// <summary>Chuỗi hashData/query = các key vnp_* (bỏ chữ ký) sắp xếp tăng dần, value URL-encode form (space→+), nối &.</summary>
    private static string BuildData(SortedDictionary<string, string> p)
    {
        var sb = new StringBuilder();
        foreach (var kv in p)
        {
            if (string.IsNullOrEmpty(kv.Value)) continue;
            if (sb.Length > 0) sb.Append('&');
            sb.Append(kv.Key).Append('=').Append(WebUtility.UrlEncode(kv.Value));
        }
        return sb.ToString();
    }

    private string Sign(string data)
    {
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(_hashSecret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    /// <summary>Dựng URL chuyển hướng người dùng sang VNPay.</summary>
    public string BuildPaymentUrl(string txnRef, long amountVnd, string orderInfo, string ipAddr, string returnUrl, string? bankCode = null)
    {
        var now = DateTime.Now;   // GMT+7 trên Render nếu TZ set; định dạng yyyyMMddHHmmss
        var p = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = TmnCode,
            ["vnp_Amount"] = (amountVnd * 100).ToString(),   // VNPay tính theo đơn vị ×100
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = txnRef,
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(ipAddr) ? "127.0.0.1" : ipAddr,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
        };
        if (!string.IsNullOrWhiteSpace(bankCode)) p["vnp_BankCode"] = bankCode;
        var data = BuildData(p);
        var secureHash = Sign(data);
        return $"{PayUrl}?{data}&vnp_SecureHash={secureHash}";
    }

    /// <summary>Xác thực chữ ký callback: lấy mọi tham số vnp_* trừ vnp_SecureHash/Type, ký lại và so.</summary>
    public bool ValidateSignature(IEnumerable<KeyValuePair<string, string>> query, out string? secureHash)
    {
        var p = new SortedDictionary<string, string>(StringComparer.Ordinal);
        secureHash = null;
        foreach (var kv in query)
        {
            if (kv.Key == "vnp_SecureHash") { secureHash = kv.Value; continue; }
            if (kv.Key == "vnp_SecureHashType") continue;
            if (kv.Key.StartsWith("vnp_", StringComparison.Ordinal)) p[kv.Key] = kv.Value;
        }
        if (string.IsNullOrEmpty(secureHash)) return false;
        var expected = Sign(BuildData(p));
        return string.Equals(expected, secureHash, StringComparison.OrdinalIgnoreCase);
    }
}
