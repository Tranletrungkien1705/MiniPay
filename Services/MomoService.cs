using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MiniPay.Services;

/// <summary>
/// Cổng MoMo (API v2): tạo giao dịch (POST /v2/gateway/api/create ký HMAC-SHA256) + xác thực chữ ký IPN.
/// Cấu hình env: MOMO_PARTNERCODE, MOMO_ACCESSKEY, MOMO_SECRETKEY, MOMO_ENDPOINT (mặc định test sandbox).
/// </summary>
public sealed class MomoService(IHttpClientFactory httpFactory, IConfiguration cfg)
{
    private string Cfg(string k, string def) => Environment.GetEnvironmentVariable(k) ?? cfg[k] ?? def;
    public string PartnerCode => Cfg("MOMO_PARTNERCODE", "MOMO");
    private string AccessKey => Cfg("MOMO_ACCESSKEY", "F8BBA842ECF85");
    private string SecretKey => Cfg("MOMO_SECRETKEY", "K951B6PE1waDMi640xX08PD3vg6EkVlz");
    private string Endpoint => Cfg("MOMO_ENDPOINT", "https://test-payment.momo.vn/v2/gateway/api/create");

    private string Sign(string raw)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(raw));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    /// <summary>Gọi MoMo tạo giao dịch → trả (payUrl, deeplink, rawResult). requestType captureWallet.</summary>
    public async Task<(string? PayUrl, string? Deeplink, int ResultCode, string Message, string Raw)> CreateAsync(
        string orderId, string requestId, long amount, string orderInfo, string redirectUrl, string ipnUrl, string extraData = "")
    {
        const string requestType = "captureWallet";
        var raw = $"accessKey={AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}"
                + $"&orderInfo={orderInfo}&partnerCode={PartnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
        var signature = Sign(raw);
        var body = new
        {
            partnerCode = PartnerCode, accessKey = AccessKey, requestId, amount = amount.ToString(), orderId,
            orderInfo, redirectUrl, ipnUrl, extraData, requestType, signature, lang = "vi"
        };
        var http = httpFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(30);
        try
        {
            var resp = await http.PostAsync(Endpoint,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var txt = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(txt);
            var root = doc.RootElement;
            var rc = root.TryGetProperty("resultCode", out var rcEl) ? rcEl.GetInt32() : -1;
            var msg = root.TryGetProperty("message", out var mEl) ? mEl.GetString() ?? "" : "";
            var pay = root.TryGetProperty("payUrl", out var pEl) ? pEl.GetString() : null;
            var dl = root.TryGetProperty("deeplink", out var dEl) ? dEl.GetString() : null;
            return (pay, dl, rc, msg, txt);
        }
        catch (Exception ex)
        {
            return (null, null, -1, "Không gọi được MoMo: " + ex.Message, "");
        }
    }

    /// <summary>Xác thực chữ ký IPN MoMo (POST JSON). Ký theo thứ tự cố định các trường v2.</summary>
    public bool ValidateIpn(MomoIpn d)
    {
        var raw = $"accessKey={AccessKey}&amount={d.amount}&extraData={d.extraData}&message={d.message}"
                + $"&orderId={d.orderId}&orderInfo={d.orderInfo}&orderType={d.orderType}&partnerCode={d.partnerCode}"
                + $"&payType={d.payType}&requestId={d.requestId}&responseTime={d.responseTime}&resultCode={d.resultCode}&transId={d.transId}";
        return string.Equals(Sign(raw), d.signature, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>Payload IPN MoMo v2 (POST JSON server→server).</summary>
public sealed class MomoIpn
{
    public string partnerCode { get; set; } = "";
    public string orderId { get; set; } = "";
    public string requestId { get; set; } = "";
    public long amount { get; set; }
    public string orderInfo { get; set; } = "";
    public string orderType { get; set; } = "";
    public long transId { get; set; }
    public int resultCode { get; set; }
    public string message { get; set; } = "";
    public string payType { get; set; } = "";
    public long responseTime { get; set; }
    public string extraData { get; set; } = "";
    public string signature { get; set; } = "";
}
