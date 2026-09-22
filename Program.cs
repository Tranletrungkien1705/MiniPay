using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniPay.Data;
using MiniPay.Models;
using MiniPay.Services;
using Serilog;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;   // giữ claim gốc từ MiniSSO
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FleetObs.ConfigureLogger("minipay");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=minipay.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<VnPayService>();
builder.Services.AddSingleton<MomoService>();
builder.Services.AddScoped<ReconcileService>();
builder.Services.AddScoped<BankingPayoutService>();
builder.Services.AddScoped<PaymentDiscountService>();
builder.Services.AddScoped<PaymentGuaranteeService>();
builder.Services.AddScoped<MortgageRedeemService>();

// SSO chung: tin token MiniSSO (OIDC RS256).
var ssoAuthority = Environment.GetEnvironmentVariable("SSO_AUTHORITY") ?? "https://minisso.onrender.com";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.Authority = ssoAuthority;
    o.RequireHttpsMetadata = ssoAuthority.StartsWith("https");
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = ssoAuthority,
        ValidateAudience = false, ValidateLifetime = true, NameClaimType = "name", RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization();
builder.Services.AddFleetObs();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.UseFleetObs();
FleetObs.ReportLicense(ssoAuthority, "minipay");
app.UseAuthentication();
app.UseAuthorization();

// SSO chung: endpoint xác thực bằng token MiniSSO.
app.MapGet("/api/whoami", (ClaimsPrincipal u) => Results.Ok(new
{
    app = "minipay",
    sub = u.FindFirst("sub")?.Value, name = u.Identity?.Name ?? u.FindFirst("name")?.Value,
    email = u.FindFirst("email")?.Value, tenant = u.FindFirst("tenant")?.Value,
    roles = u.FindAll("role").Select(c => c.Value)
})).RequireAuthorization();

// Multi-tenant (merchant): org = header X-Api-Key / cookie org_key.
app.Use(async (ctx, next) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) ctx.Request.Cookies.TryGetValue(TenantContext.CookieName, out key);
    if (!string.IsNullOrWhiteSpace(key))
    {
        using var lookup = app.Services.CreateScope();
        var ldb = lookup.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = await ldb.Orgs.FirstOrDefaultAsync(o => o.ApiKey == key);
        if (org != null) ctx.RequestServices.GetRequiredService<ITenantContext>().OrgId = org.Id;
    }
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/healthz", () => "ok");

// ===== Cổng thanh toán VNPay =====

// 1) Tạo giao dịch → trả URL chuyển hướng sang VNPay. Cần X-Api-Key (merchant).
app.MapPost("/api/pay/create", async (CreatePayDto dto, VnPayService vnp, AppDbContext db, ITenantContext tenant, HttpContext ctx) =>
{
    if (dto.Amount <= 0) return Results.BadRequest(new { error = "Amount phải > 0 (VND)." });
    var txnRef = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Random.Shared.Next(100, 999);
    var orderInfo = string.IsNullOrWhiteSpace(dto.OrderInfo) ? $"ThanhToanDon{dto.OrderId}" : dto.OrderInfo!;
    var intent = new PaymentIntent
    {
        OrgId = tenant.OrgId, TxnRef = txnRef, OrderId = dto.OrderId ?? txnRef,
        Amount = dto.Amount, OrderInfo = orderInfo, Status = PayStatus.Pending
    };
    db.Payments.Add(intent);
    await db.SaveChangesAsync();

    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    var returnUrl = Environment.GetEnvironmentVariable("VNP_RETURNURL") ?? $"{baseUrl}/api/pay/return";
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var payUrl = vnp.BuildPaymentUrl(txnRef, dto.Amount, orderInfo, ip, returnUrl, dto.BankCode);
    return Results.Ok(new { txnRef, amount = dto.Amount, payUrl, status = "Pending" });
}).RequireAuthorization();

// 2) IPN (server→server VNPay gọi): xác thực chữ ký + cập nhật trạng thái. Trả RspCode theo chuẩn VNPay.
app.MapGet("/api/pay/ipn", async (VnPayService vnp, AppDbContext db, HttpContext ctx) =>
{
    var q = ctx.Request.Query.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.ToString())).ToList();
    if (!vnp.ValidateSignature(q, out _))
        return Results.Json(new { RspCode = "97", Message = "Invalid signature" });

    var txnRef = ctx.Request.Query["vnp_TxnRef"].ToString();
    var rsp = ctx.Request.Query["vnp_ResponseCode"].ToString();
    var amount = ctx.Request.Query["vnp_Amount"].ToString();
    var p = await db.Payments.FirstOrDefaultAsync(x => x.TxnRef == txnRef);
    if (p is null) return Results.Json(new { RspCode = "01", Message = "Order not found" });
    if ((p.Amount * 100).ToString() != amount) return Results.Json(new { RspCode = "04", Message = "Invalid amount" });
    if (p.Status != PayStatus.Pending) return Results.Json(new { RspCode = "02", Message = "Order already confirmed" });

    p.ResponseCode = rsp;
    p.BankCode = ctx.Request.Query["vnp_BankCode"].ToString();
    p.VnpTransactionNo = ctx.Request.Query["vnp_TransactionNo"].ToString();
    if (rsp == "00") { p.Status = PayStatus.Paid; p.PaidAt = DateTime.Now; }
    else p.Status = PayStatus.Failed;
    await db.SaveChangesAsync();
    return Results.Json(new { RspCode = "00", Message = "Confirm Success" });
});

// 3) Return (trình duyệt KH quay lại): xác thực chữ ký, hiển thị kết quả.
app.MapGet("/api/pay/return", (VnPayService vnp, HttpContext ctx) =>
{
    var q = ctx.Request.Query.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.ToString())).ToList();
    var ok = vnp.ValidateSignature(q, out _);
    var rsp = ctx.Request.Query["vnp_ResponseCode"].ToString();
    return Results.Ok(new
    {
        verified = ok,
        txnRef = ctx.Request.Query["vnp_TxnRef"].ToString(),
        responseCode = rsp,
        success = ok && rsp == "00",
        message = !ok ? "Sai chữ ký" : rsp == "00" ? "Thanh toán thành công" : "Thanh toán thất bại/hủy"
    });
});

// 4) Tra trạng thái giao dịch (merchant tự đối soát).
app.MapGet("/api/pay/status", async (string txnRef, AppDbContext db) =>
{
    var p = await db.Payments.FirstOrDefaultAsync(x => x.TxnRef == txnRef);
    if (p is null) return Results.NotFound(new { txnRef, found = false });
    return Results.Ok(new
    {
        txnRef = p.TxnRef, orderId = p.OrderId, amount = p.Amount, status = p.Status.ToString(),
        responseCode = p.ResponseCode, bankCode = p.BankCode, transactionNo = p.VnpTransactionNo,
        createdAt = p.CreatedAt, paidAt = p.PaidAt
    });
}).RequireAuthorization();

// ===== Cổng thanh toán MoMo =====

// 1) Tạo giao dịch MoMo → gọi MoMo lấy payUrl/deeplink. Cần X-Api-Key (merchant).
app.MapPost("/api/pay/momo/create", async (CreatePayDto dto, MomoService momo, AppDbContext db, ITenantContext tenant, HttpContext ctx) =>
{
    if (dto.Amount <= 0) return Results.BadRequest(new { error = "Amount phải > 0 (VND)." });
    var orderId = (dto.OrderId ?? "OD") + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
    var requestId = orderId;
    var orderInfo = string.IsNullOrWhiteSpace(dto.OrderInfo) ? $"ThanhToanDon{dto.OrderId}" : dto.OrderInfo!;
    var intent = new PaymentIntent
    {
        OrgId = tenant.OrgId, TxnRef = orderId, OrderId = dto.OrderId ?? orderId,
        Amount = dto.Amount, OrderInfo = orderInfo, Provider = "momo", Status = PayStatus.Pending
    };
    db.Payments.Add(intent);
    await db.SaveChangesAsync();

    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    var redirectUrl = Environment.GetEnvironmentVariable("MOMO_REDIRECTURL") ?? $"{baseUrl}/api/pay/momo/return";
    var ipnUrl = Environment.GetEnvironmentVariable("MOMO_IPNURL") ?? $"{baseUrl}/api/pay/momo/ipn";
    var (payUrl, deeplink, rc, msg, _) = await momo.CreateAsync(orderId, requestId, dto.Amount, orderInfo, redirectUrl, ipnUrl);
    return Results.Ok(new { orderId, amount = dto.Amount, resultCode = rc, message = msg, payUrl, deeplink, status = "Pending" });
}).RequireAuthorization();

// 2) IPN MoMo (server→server, POST JSON): xác thực chữ ký + cập nhật trạng thái. Trả 204/không body theo chuẩn MoMo.
app.MapPost("/api/pay/momo/ipn", async (MomoIpn dto, MomoService momo, AppDbContext db) =>
{
    if (!momo.ValidateIpn(dto)) return Results.Json(new { RspCode = "97", Message = "Invalid signature" });
    var p = await db.Payments.FirstOrDefaultAsync(x => x.TxnRef == dto.orderId);
    if (p is null) return Results.Json(new { RspCode = "01", Message = "Order not found" });
    if (p.Amount != dto.amount) return Results.Json(new { RspCode = "04", Message = "Invalid amount" });
    if (p.Status != PayStatus.Pending) return Results.Json(new { RspCode = "02", Message = "Order already confirmed" });
    p.ResponseCode = dto.resultCode.ToString();
    p.VnpTransactionNo = dto.transId.ToString();
    if (dto.resultCode == 0) { p.Status = PayStatus.Paid; p.PaidAt = DateTime.Now; }
    else p.Status = PayStatus.Failed;
    await db.SaveChangesAsync();
    return Results.Json(new { RspCode = "00", Message = "Confirm Success" });
});

// Đăng ký merchant mới → ApiKey.
app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "pay_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org); await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

// Import giao dịch thanh toán thật từ Pmt_Payment (dedupe theo TxnRef)
app.MapPost("/api/import/payments", async (List<ImportPayDto> rows, AppDbContext db, ITenantContext tc) =>
{
    if (rows == null || rows.Count == 0) return Results.BadRequest(new { error = "Không có dữ liệu." });
    int added = 0, skipped = 0;
    var orgId = tc.OrgId;
    var existRefs = db.Payments.Where(p => p.OrgId == orgId).Select(p => p.TxnRef).ToHashSet();
    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.TxnRef)) { skipped++; continue; }
        if (existRefs.Contains(row.TxnRef.Trim())) { skipped++; continue; }
        db.Payments.Add(new PaymentIntent
        {
            OrgId = orgId, TxnRef = row.TxnRef.Trim(), OrderId = row.OrderId ?? row.TxnRef.Trim(),
            Amount = row.Amount, OrderInfo = row.OrderInfo ?? "", Provider = "internal",
            Status = row.IsPaid ? PayStatus.Paid : PayStatus.Pending,
            BankCode = row.BankCode, CreatedAt = row.CreatedAt ?? DateTime.Now,
            PaidAt = row.IsPaid ? row.CreatedAt : null
        });
        existRefs.Add(row.TxnRef.Trim()); added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = added + skipped });
});

// ===== Đối soát sao kê ngân hàng (Bank Statement Reconciliation) =====

// 1) Tạo lô đối soát & tự động so khớp giao dịch ngân hàng
app.MapPost("/api/reconcile/batches", async (CreateReconcileBatchDto dto, ReconcileService recService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Cần ít nhất 1 dòng giao dịch sao kê." });

    var batchCode = string.IsNullOrWhiteSpace(dto.BatchCode)
        ? $"REC-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}"
        : dto.BatchCode.Trim();

    var batch = await recService.ExecuteReconcileAsync(
        tc.OrgId,
        batchCode,
        dto.BankCode ?? "BANK",
        dto.AccountNo,
        dto.StatementDate ?? DateTime.Today,
        dto.Items,
        dto.Note
    );

    return Results.Ok(new
    {
        batch.Id,
        batch.BatchCode,
        batch.BankCode,
        batch.AccountNo,
        batch.StatementDate,
        batch.TotalRecords,
        batch.MatchedCount,
        batch.MismatchedCount,
        batch.UnmatchedCount,
        batch.TotalAmount,
        batch.MatchedAmount,
        status = batch.Status.ToString(),
        batch.CreatedAt,
        batch.CompletedAt,
        details = batch.Details.Select(d => new
        {
            d.Id,
            d.BankTxnNo,
            d.TxnRef,
            d.TxnTime,
            d.Amount,
            d.SenderAccount,
            d.ReceiverAccount,
            d.Remark,
            matchStatus = d.MatchStatus.ToString(),
            d.PaymentIntentId,
            d.SystemAmount,
            d.DiscrepancyReason,
            d.MatchedAt
        })
    });
});

// 2) Lấy danh sách các đợt đối soát
app.MapGet("/api/reconcile/batches", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode) =>
{
    var q = db.ReconcileBatches.Where(b => b.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(b => b.BankCode == bankCode);
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReconcileStatus>(status, true, out var st))
        q = q.Where(b => b.Status == st);

    var list = await q.OrderByDescending(b => b.CreatedAt)
        .Select(b => new
        {
            b.Id,
            b.BatchCode,
            b.BankCode,
            b.AccountNo,
            b.StatementDate,
            b.TotalRecords,
            b.MatchedCount,
            b.MismatchedCount,
            b.UnmatchedCount,
            b.TotalAmount,
            b.MatchedAmount,
            status = b.Status.ToString(),
            b.Note,
            b.CreatedAt,
            b.CompletedAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Lấy chi tiết 1 đợt đối soát kèm các dòng giao dịch
app.MapGet("/api/reconcile/batches/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var batch = await db.ReconcileBatches
        .Include(b => b.Details)
        .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == tc.OrgId);

    if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy đợt đối soát #{id}." });

    return Results.Ok(new
    {
        batch.Id,
        batch.BatchCode,
        batch.BankCode,
        batch.AccountNo,
        batch.StatementDate,
        batch.TotalRecords,
        batch.MatchedCount,
        batch.MismatchedCount,
        batch.UnmatchedCount,
        batch.TotalAmount,
        batch.MatchedAmount,
        status = batch.Status.ToString(),
        batch.Note,
        batch.CreatedAt,
        batch.CompletedAt,
        details = batch.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.BankTxnNo,
            d.TxnRef,
            d.TxnTime,
            d.Amount,
            d.SenderAccount,
            d.ReceiverAccount,
            d.Remark,
            matchStatus = d.MatchStatus.ToString(),
            d.PaymentIntentId,
            d.SystemAmount,
            d.DiscrepancyReason,
            d.MatchedAt
        })
    });
});

// 4) Chạy lại đối soát cho đợt cũ
app.MapPost("/api/reconcile/batches/{id:long}/re-run", async (long id, ReconcileService recService, ITenantContext tc) =>
{
    var batch = await recService.ReRunBatchAsync(id, tc.OrgId);
    if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy đợt đối soát #{id}." });

    return Results.Ok(new
    {
        batch.Id,
        batch.BatchCode,
        batch.TotalRecords,
        batch.MatchedCount,
        batch.MismatchedCount,
        batch.UnmatchedCount,
        batch.TotalAmount,
        batch.MatchedAmount,
        status = batch.Status.ToString(),
        batch.CompletedAt
    });
});

// 5) Báo cáo tổng hợp đối soát của merchant
app.MapGet("/api/reconcile/summary", async (AppDbContext db, ITenantContext tc) =>
{
    var batches = await db.ReconcileBatches
        .Where(b => b.OrgId == tc.OrgId)
        .ToListAsync();

    var totalBatches = batches.Count;
    var totalRecords = batches.Sum(b => b.TotalRecords);
    var matchedCount = batches.Sum(b => b.MatchedCount);
    var mismatchedCount = batches.Sum(b => b.MismatchedCount);
    var unmatchedCount = batches.Sum(b => b.UnmatchedCount);
    var totalAmount = batches.Sum(b => b.TotalAmount);
    var matchedAmount = batches.Sum(b => b.MatchedAmount);
    var matchRate = totalRecords > 0 ? Math.Round((double)matchedCount * 100.0 / totalRecords, 1) : 0;

    return Results.Ok(new
    {
        totalBatches,
        totalRecords,
        matchedCount,
        mismatchedCount,
        unmatchedCount,
        totalAmount,
        matchedAmount,
        matchRatePercent = matchRate,
        recentBatches = batches.OrderByDescending(b => b.CreatedAt).Take(5).Select(b => new
        {
            b.Id,
            b.BatchCode,
            b.BankCode,
            b.TotalRecords,
            b.MatchedCount,
            b.MismatchedCount,
            status = b.Status.ToString(),
            b.CreatedAt
        })
    });
});

// ===== Lệnh chi chuyển tiền ngân hàng tự động (Banking Payout / Bulk Payment) =====

// 1) Tạo lô lệnh chi chuyển khoản ngân hàng (RQ_BankingTransactions_Save)
app.MapPost("/api/payout/batches", async (CreatePayoutBatchDto dto, BankingPayoutService payoutService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Cần ít nhất 1 dòng người nhận chuyển khoản." });
    if (string.IsNullOrWhiteSpace(dto.BankCode))
        return Results.BadRequest(new { error = "Cần chỉ định mã ngân hàng trích nợ (BankCode: CTG, MBB, VCB, TCB...)." });
    if (string.IsNullOrWhiteSpace(dto.SourceAccount))
        return Results.BadRequest(new { error = "Cần số tài khoản trích nợ (SourceAccount)." });

    try
    {
        var batch = await payoutService.CreateBatchAsync(
            tc.OrgId,
            dto.BatchNo,
            dto.BankCode,
            dto.SourceAccount,
            dto.SourceAccountName,
            dto.BizResNumber,
            dto.Remark,
            dto.Items
        );

        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            batch.BankCode,
            batch.SourceAccount,
            batch.SourceAccountName,
            batch.BizResNumber,
            batch.Remark,
            batch.TotalTrans,
            batch.TotalAmount,
            status = batch.Status.ToString(),
            batch.CreatedAt,
            details = batch.Details.Select(d => new
            {
                d.Id,
                d.TransNo,
                transType = d.TransType.ToString(),
                disbursementType = d.DisbursementType.ToString(),
                d.RefNo,
                d.ReceivingUnit,
                d.BankAccountReceive,
                d.BankNameReceive,
                d.ProvinceName,
                d.TransferAmount,
                d.TransferRemark,
                status = d.Status.ToString()
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Lấy danh sách các lô lệnh chi (RQ_BankingTransactions_Get)
app.MapGet("/api/payout/batches", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode) =>
{
    var q = db.PayoutBatches.Where(b => b.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(b => b.BankCode == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PayoutStatus>(status, true, out var st))
        q = q.Where(b => b.Status == st);

    var list = await q.OrderByDescending(b => b.CreatedAt)
        .Select(b => new
        {
            b.Id,
            b.BatchNo,
            b.BankCode,
            b.SourceAccount,
            b.SourceAccountName,
            b.BizResNumber,
            b.Remark,
            b.TotalTrans,
            b.SuccessTrans,
            b.FailedTrans,
            b.TotalAmount,
            b.SuccessAmount,
            status = b.Status.ToString(),
            b.BankStatusCode,
            b.RefBankCode,
            b.BankRemark,
            b.CreatedBy,
            b.CreatedAt,
            b.ApprovedBy,
            b.ApprovedAt,
            b.CompletedAt,
            b.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Lấy chi tiết 1 lô lệnh chi kèm danh sách món chuyển tiền (RQ_BankingTransactions_GetDetail)
app.MapGet("/api/payout/batches/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var batch = await db.PayoutBatches
        .Include(b => b.Details)
        .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == tc.OrgId);

    if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô lệnh chi #{id}." });

    return Results.Ok(new
    {
        batch.Id,
        batch.BatchNo,
        batch.BankCode,
        batch.SourceAccount,
        batch.SourceAccountName,
        batch.BizResNumber,
        batch.Remark,
        batch.TotalTrans,
        batch.SuccessTrans,
        batch.FailedTrans,
        batch.TotalAmount,
        batch.SuccessAmount,
        status = batch.Status.ToString(),
        batch.BankStatusCode,
        batch.RefBankCode,
        batch.BankRemark,
        batch.CreatedBy,
        batch.CreatedAt,
        batch.ApprovedBy,
        batch.ApprovedAt,
        batch.RejectReason,
        batch.CompletedAt,
        batch.CancelledAt,
        details = batch.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.TransNo,
            transType = d.TransType.ToString(),
            disbursementType = d.DisbursementType.ToString(),
            d.RefNo,
            d.ReceivingUnit,
            d.BankAccountReceive,
            d.BankNameReceive,
            d.ProvinceName,
            d.TransferAmount,
            d.TransferRemark,
            status = d.Status.ToString(),
            d.BankTxnRef,
            d.ErrorMessage,
            d.ExecutedAt
        })
    });
});

// 4) Phê duyệt lô lệnh chi (RQ_BankingTransactions_Approve)
app.MapPost("/api/payout/batches/{id:long}/approve", async (long id, BankingPayoutService payoutService, ITenantContext tc) =>
{
    try
    {
        var batch = await payoutService.ApproveBatchAsync(id, tc.OrgId, "AccountingManager");
        if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô lệnh chi #{id}." });

        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            status = batch.Status.ToString(),
            batch.ApprovedBy,
            batch.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Từ chối phê duyệt lô lệnh chi
app.MapPost("/api/payout/batches/{id:long}/reject", async (long id, RejectPayoutDto? dto, BankingPayoutService payoutService, ITenantContext tc) =>
{
    try
    {
        var batch = await payoutService.RejectBatchAsync(id, tc.OrgId, dto?.Reason, "AccountingManager");
        if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô lệnh chi #{id}." });

        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            status = batch.Status.ToString(),
            batch.RejectReason,
            batch.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Hủy lô lệnh chi khi chưa đẩy ngân hàng (RQ_BankingTransactions_Cancel)
app.MapPost("/api/payout/batches/{id:long}/cancel", async (long id, BankingPayoutService payoutService, ITenantContext tc) =>
{
    try
    {
        var batch = await payoutService.CancelBatchAsync(id, tc.OrgId);
        if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô lệnh chi #{id}." });

        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            status = batch.Status.ToString(),
            batch.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Đẩy lệnh chi sang cổng ngân hàng điện tử (RQ_BankingTransactions_PushBank / MBBank_MakeBulkPayment_v2_1)
app.MapPost("/api/payout/batches/{id:long}/push-bank", async (long id, BankingPayoutService payoutService, ITenantContext tc) =>
{
    try
    {
        var batch = await payoutService.PushToBankAsync(id, tc.OrgId);
        if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô lệnh chi #{id}." });

        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            batch.BankCode,
            status = batch.Status.ToString(),
            batch.BankStatusCode,
            batch.RefBankCode,
            batch.BankRemark,
            batch.TotalTrans,
            batch.SuccessTrans,
            batch.FailedTrans,
            batch.TotalAmount,
            batch.SuccessAmount,
            batch.CompletedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Báo cáo tổng hợp số liệu Payout
app.MapGet("/api/payout/summary", async (BankingPayoutService payoutService, ITenantContext tc) =>
{
    var summary = await payoutService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Chiết khấu thanh toán sớm (Payment Discount Request - BizHTC.PaymentDiscount) =====

// 1) Tính toán preview chiết khấu nhanh
app.MapPost("/api/discount/calculate-preview", (PreviewDiscountDto dto) =>
{
    var (earlyDays, discountAmount, netPayAmount) = PaymentDiscountService.CalculateDiscount(
        dto.OriginalAmount,
        dto.AnnualDiscountRate ?? 7.5m,
        dto.DueDate,
        dto.ActualPaymentDate ?? DateTime.Today
    );
    return Results.Ok(new { earlyDays, discountAmount, netPayAmount });
});

// 2) Tạo hồ sơ yêu cầu chiết khấu thanh toán sớm (Req_PaymentDiscount_Save)
app.MapPost("/api/discount/requests", async (CreateDiscountRequestDto dto, PaymentDiscountService discountService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Hồ sơ chiết khấu phải có ít nhất 1 dòng thanh toán." });
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Cần mã đối tác / đại lý (PartnerCode)." });

    try
    {
        var req = await discountService.CreateRequestAsync(
            tc.OrgId,
            dto.DiscountNo,
            dto.PartnerCode,
            dto.PartnerName,
            dto.ContractNo,
            dto.DefaultAnnualRate,
            dto.Remark,
            dto.Items
        );

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            req.PartnerCode,
            req.PartnerName,
            req.ContractNo,
            req.TotalPaymentAmount,
            req.TotalDiscountAmount,
            req.NetPaymentAmount,
            req.DefaultAnnualRate,
            status = req.Status.ToString(),
            partnerSignStatus = req.PartnerSignStatus.ToString(),
            approverSignStatus = req.ApproverSignStatus.ToString(),
            req.Remark,
            req.CreatedAt,
            details = req.Details.Select(d => new
            {
                d.Id,
                d.ItemRefNo,
                d.Description,
                d.DueDate,
                d.ActualPaymentDate,
                d.EarlyDays,
                d.OriginalAmount,
                d.AnnualDiscountRate,
                d.DiscountAmount,
                d.NetPayAmount,
                status = d.Status.ToString(),
                d.Note
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 3) Lấy danh sách hồ sơ chiết khấu (Req_PaymentDiscount_Get)
app.MapGet("/api/discount/requests", async (AppDbContext db, ITenantContext tc, string? status, string? partnerCode) =>
{
    var q = db.DiscountRequests.Where(r => r.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(partnerCode))
        q = q.Where(r => r.PartnerCode == partnerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DiscountRequestStatus>(status, true, out var st))
        q = q.Where(r => r.Status == st);

    var list = await q.OrderByDescending(r => r.CreatedAt)
        .Select(r => new
        {
            r.Id,
            r.DiscountNo,
            r.PartnerCode,
            r.PartnerName,
            r.ContractNo,
            r.TotalPaymentAmount,
            r.TotalDiscountAmount,
            r.NetPaymentAmount,
            r.DefaultAnnualRate,
            status = r.Status.ToString(),
            partnerSignStatus = r.PartnerSignStatus.ToString(),
            r.PartnerSignedBy,
            r.PartnerSignedAt,
            approverSignStatus = r.ApproverSignStatus.ToString(),
            r.ApprovedBy,
            r.ApprovedAt,
            r.SettledBy,
            r.SettledAt,
            r.RejectReason,
            r.Remark,
            r.CreatedAt,
            r.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 4) Lấy chi tiết 1 hồ sơ chiết khấu kèm các dòng thanh toán (Req_PaymentDiscount_Get / Req_PaymentDiscountDtl)
app.MapGet("/api/discount/requests/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var req = await db.DiscountRequests
        .Include(r => r.Details)
        .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == tc.OrgId);

    if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

    return Results.Ok(new
    {
        req.Id,
        req.DiscountNo,
        req.PartnerCode,
        req.PartnerName,
        req.ContractNo,
        req.TotalPaymentAmount,
        req.TotalDiscountAmount,
        req.NetPaymentAmount,
        req.DefaultAnnualRate,
        status = req.Status.ToString(),
        partnerSignStatus = req.PartnerSignStatus.ToString(),
        req.PartnerSignedBy,
        req.PartnerSignedAt,
        approverSignStatus = req.ApproverSignStatus.ToString(),
        req.ApprovedBy,
        req.ApprovedAt,
        req.SettledBy,
        req.SettledAt,
        req.RejectReason,
        req.Remark,
        req.CreatedAt,
        req.CancelledAt,
        details = req.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.ItemRefNo,
            d.Description,
            d.DueDate,
            d.ActualPaymentDate,
            d.EarlyDays,
            d.OriginalAmount,
            d.AnnualDiscountRate,
            d.DiscountAmount,
            d.NetPayAmount,
            status = d.Status.ToString(),
            d.Note
        })
    });
});

// 5) Đại lý / Đối tác ký số xác nhận hồ sơ (Req_PaymentDiscount_DlrSign)
app.MapPost("/api/discount/requests/{id:long}/partner-sign", async (long id, SignDiscountDto? dto, PaymentDiscountService discountService, ITenantContext tc) =>
{
    try
    {
        var req = await discountService.PartnerSignAsync(id, tc.OrgId, dto?.SignerName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            status = req.Status.ToString(),
            partnerSignStatus = req.PartnerSignStatus.ToString(),
            req.PartnerSignedBy,
            req.PartnerSignedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Kế toán trưởng duyệt hồ sơ chiết khấu (Req_PaymentDiscount_HTCApprove)
app.MapPost("/api/discount/requests/{id:long}/approve", async (long id, SignDiscountDto? dto, PaymentDiscountService discountService, ITenantContext tc) =>
{
    try
    {
        var req = await discountService.ApproveRequestAsync(id, tc.OrgId, dto?.SignerName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            status = req.Status.ToString(),
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ký số lãnh đạo & Hoàn tất quyết toán cấn trừ công nợ (Req_PaymentDiscount_HTCSign)
app.MapPost("/api/discount/requests/{id:long}/settle", async (long id, SignDiscountDto? dto, PaymentDiscountService discountService, ITenantContext tc) =>
{
    try
    {
        var req = await discountService.SettleRequestAsync(id, tc.OrgId, dto?.SignerName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            status = req.Status.ToString(),
            approverSignStatus = req.ApproverSignStatus.ToString(),
            req.SettledBy,
            req.SettledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Từ chối yêu cầu chiết khấu (Req_PaymentDiscount_HTCReject)
app.MapPost("/api/discount/requests/{id:long}/reject", async (long id, RejectDiscountDto? dto, PaymentDiscountService discountService, ITenantContext tc) =>
{
    try
    {
        var req = await discountService.RejectRequestAsync(id, tc.OrgId, dto?.Reason, dto?.RejecterName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            status = req.Status.ToString(),
            req.RejectReason,
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Hủy hồ sơ chiết khấu (Req_PaymentDiscount_HTCCancel)
app.MapPost("/api/discount/requests/{id:long}/cancel", async (long id, PaymentDiscountService discountService, ITenantContext tc) =>
{
    try
    {
        var req = await discountService.CancelRequestAsync(id, tc.OrgId);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ chiết khấu #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.DiscountNo,
            status = req.Status.ToString(),
            req.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Báo cáo tổng hợp số liệu chiết khấu thanh toán
app.MapGet("/api/discount/summary", async (PaymentDiscountService discountService, ITenantContext tc) =>
{
    var summary = await discountService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Thư Bảo Lãnh Thanh Toán Ngân Hàng (Bank Payment Guarantee - BizHTC.Payment / FrmMngGrt) =====

// 1) Tạo mới Thư bảo lãnh thanh toán (Pmt_Guarantee_Save / FrmNewGrt.MODE_NEW)
app.MapPost("/api/guarantee", async (CreateGuaranteeDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BankGuaranteeNo))
        return Results.BadRequest(new { error = "Cần số thư bảo lãnh ngân hàng (BankGuaranteeNo)." });
    if (string.IsNullOrWhiteSpace(dto.BankCode))
        return Results.BadRequest(new { error = "Cần mã ngân hàng (BankCode: VCB, TCB, MBB, CTG...)." });
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Cần mã đối tác / đại lý (PartnerCode)." });

    try
    {
        var grt = await grtService.CreateGuaranteeAsync(
            tc.OrgId,
            dto.GuaranteeNo,
            dto.BankGuaranteeNo,
            dto.BankCode,
            dto.BankName,
            dto.PartnerCode,
            dto.PartnerName,
            dto.ContractNo,
            dto.GuaranteeType ?? GuaranteeType.Payment,
            dto.TotalAmount,
            dto.DateOpen,
            dto.DateExpired,
            dto.DateEnd,
            dto.FeePercent,
            dto.Remark,
            dto.Items
        );

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            grt.BankGuaranteeNo,
            grt.BankCode,
            grt.BankName,
            grt.PartnerCode,
            grt.PartnerName,
            grt.ContractNo,
            guaranteeType = grt.GuaranteeType.ToString(),
            grt.TotalAmount,
            grt.UtilizedAmount,
            grt.RemainingAmount,
            grt.DateOpen,
            grt.DateEnd,
            grt.DateExpired,
            grt.TermDays,
            grt.FeePercent,
            status = grt.Status.ToString(),
            grt.Remark,
            grt.CreatedBy,
            grt.CreatedAt,
            details = grt.Details.Select(d => new
            {
                d.Id,
                d.ItemRefNo,
                d.Description,
                d.OrderAmount,
                d.GuaranteeValue,
                d.GuaranteePercent,
                d.DateStart,
                d.DateEnd,
                status = d.Status.ToString(),
                d.Note
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Danh sách Thư bảo lãnh thanh toán (Pmt_Guarantee_Get / FrmMngGrt)
app.MapGet("/api/guarantee", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode, string? partnerCode) =>
{
    var q = db.Guarantees.Where(g => g.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(g => g.BankCode == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(partnerCode))
        q = q.Where(g => g.PartnerCode == partnerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GuaranteeStatus>(status, true, out var st))
        q = q.Where(g => g.Status == st);

    var list = await q.OrderByDescending(g => g.CreatedAt)
        .Select(g => new
        {
            g.Id,
            g.GuaranteeNo,
            g.BankGuaranteeNo,
            g.BankCode,
            g.BankName,
            g.PartnerCode,
            g.PartnerName,
            g.ContractNo,
            guaranteeType = g.GuaranteeType.ToString(),
            g.TotalAmount,
            g.UtilizedAmount,
            g.RemainingAmount,
            g.DateOpen,
            g.DateEnd,
            g.DateExpired,
            g.TermDays,
            g.FeePercent,
            g.DateRecieveGrtRoot,
            status = g.Status.ToString(),
            g.Remark,
            g.RemarkReject,
            g.ClaimedAmount,
            g.ClaimReason,
            g.ClaimedAt,
            g.ClaimedBy,
            g.CreatedBy,
            g.CreatedAt,
            g.ApprovedBy,
            g.ApprovedAt,
            g.SettledBy,
            g.SettledAt,
            g.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 Thư bảo lãnh kèm danh sách món hàng / xe / hợp đồng (Pmt_GuaranteeDetail_Get)
app.MapGet("/api/guarantee/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var grt = await db.Guarantees
        .Include(g => g.Details)
        .FirstOrDefaultAsync(g => g.Id == id && g.OrgId == tc.OrgId);

    if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

    return Results.Ok(new
    {
        grt.Id,
        grt.GuaranteeNo,
        grt.BankGuaranteeNo,
        grt.BankCode,
        grt.BankName,
        grt.PartnerCode,
        grt.PartnerName,
        grt.ContractNo,
        guaranteeType = grt.GuaranteeType.ToString(),
        grt.TotalAmount,
        grt.UtilizedAmount,
        grt.RemainingAmount,
        grt.DateOpen,
        grt.DateEnd,
        grt.DateExpired,
        grt.TermDays,
        grt.FeePercent,
        grt.DateRecieveGrtRoot,
        status = grt.Status.ToString(),
        grt.Remark,
        grt.RemarkReject,
        grt.ClaimedAmount,
        grt.ClaimReason,
        grt.ClaimedAt,
        grt.ClaimedBy,
        grt.CreatedBy,
        grt.CreatedAt,
        grt.ApprovedBy,
        grt.ApprovedAt,
        grt.SettledBy,
        grt.SettledAt,
        grt.CancelledAt,
        details = grt.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.ItemRefNo,
            d.Description,
            d.OrderAmount,
            d.GuaranteeValue,
            d.GuaranteePercent,
            d.DateStart,
            d.DateEnd,
            status = d.Status.ToString(),
            d.Note
        })
    });
});

// 4) Phê duyệt Thư bảo lãnh (FrmNewGrt.btnApprove_Click)
app.MapPost("/api/guarantee/{id:long}/approve", async (long id, ApproveGuaranteeDto? dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.ApproveGuaranteeAsync(id, tc.OrgId, dto?.ApproverName);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            status = grt.Status.ToString(),
            grt.ApprovedBy,
            grt.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Gia hạn thời hạn Thư bảo lãnh (FrmEditGrtExpiredDate / FrmEditGrtEndDate)
app.MapPost("/api/guarantee/{id:long}/extend", async (long id, ExtendGuaranteeDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.ExtendGuaranteeAsync(id, tc.OrgId, dto.NewExpiredDate, dto.NewEndDate, dto.Remark);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            grt.DateExpired,
            grt.DateEnd,
            grt.TermDays,
            status = grt.Status.ToString(),
            grt.Remark
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Điều chỉnh hạn mức / giá trị bảo lãnh (FrmEditGrtValue)
app.MapPost("/api/guarantee/{id:long}/adjust-value", async (long id, AdjustGuaranteeValueDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.AdjustValueAsync(id, tc.OrgId, dto.NewTotalAmount, dto.Remark);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            grt.TotalAmount,
            grt.UtilizedAmount,
            grt.RemainingAmount,
            status = grt.Status.ToString(),
            grt.Remark
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Cập nhật ngày nhận bản gốc Thư bảo lãnh từ ngân hàng (FrmEditDateRecieveGrtRoot)
app.MapPost("/api/guarantee/{id:long}/receive-root", async (long id, ReceiveRootGuaranteeDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.UpdateReceiveRootDateAsync(id, tc.OrgId, dto.ReceiveDate, dto.Note);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            grt.DateRecieveGrtRoot,
            grt.Remark
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Kích hoạt đòi bảo lãnh ngân hàng (Pmt_GrtClaim / FrmMngGrtClaim)
app.MapPost("/api/guarantee/{id:long}/claim", async (long id, ClaimGuaranteeDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.ClaimGuaranteeAsync(id, tc.OrgId, dto.ClaimAmount, dto.ClaimReason, dto.ClaimedBy);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            status = grt.Status.ToString(),
            grt.ClaimedAmount,
            grt.ClaimReason,
            grt.ClaimedBy,
            grt.ClaimedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Hoàn tất tất toán / giải tỏa toàn bộ nghĩa vụ bảo lãnh (FrmQuanLyBBBGTheoHoiPhieu / Settle)
app.MapPost("/api/guarantee/{id:long}/settle", async (long id, SettleGuaranteeDto? dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.SettleGuaranteeAsync(id, tc.OrgId, dto?.SettlerName, dto?.Remark);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            status = grt.Status.ToString(),
            grt.SettledBy,
            grt.SettledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Từ chối phê duyệt Thư bảo lãnh (FrmNewGrt.btnReject_Click)
app.MapPost("/api/guarantee/{id:long}/reject", async (long id, RejectGuaranteeDto dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.RejectGuaranteeAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            status = grt.Status.ToString(),
            grt.RemarkReject,
            grt.ApprovedBy,
            grt.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Hủy Thư bảo lãnh (FrmNewGrt.btnCancelGrt_Click)
app.MapPost("/api/guarantee/{id:long}/cancel", async (long id, CancelGuaranteeDto? dto, PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    try
    {
        var grt = await grtService.CancelGuaranteeAsync(id, tc.OrgId, dto?.Reason);
        if (grt == null) return Results.NotFound(new { error = $"Không tìm thấy Thư bảo lãnh #{id}." });

        return Results.Ok(new
        {
            grt.Id,
            grt.GuaranteeNo,
            status = grt.Status.ToString(),
            grt.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Báo cáo tổng hợp số liệu bảo lãnh ngân hàng
app.MapGet("/api/guarantee/summary", async (PaymentGuaranteeService grtService, ITenantContext tc) =>
{
    var summary = await grtService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Thế chấp & Giải chấp tài sản ngân hàng (Bank Collateral Mortgage & Redemption - BizHTC.GiaiChap) =====

// 1) Tạo đề nghị thế chấp tài sản / kho xe vay ngân hàng (RM_ReqMortgage_Create)
app.MapPost("/api/mortgage/requests", async (CreateMortgageDto dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BankCode))
        return Results.BadRequest(new { error = "Cần mã ngân hàng nhận thế chấp (BankCode: CTG, MBB, TCB, VCB...)." });
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Cần mã đối tác / đại lý thế chấp (PartnerCode)." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Hồ sơ thế chấp phải có ít nhất 1 tài sản / xe bảo đảm." });

    try
    {
        var req = await mgService.CreateMortgageRequestAsync(
            tc.OrgId,
            dto.ReqRMNo,
            dto.BankCode,
            dto.BankName,
            dto.PartnerCode,
            dto.PartnerName,
            dto.CreditContractNo,
            dto.MortgageDate,
            dto.InterestRate,
            dto.LoanPeriodDays,
            dto.Remark,
            dto.Items
        );

        return Results.Ok(new
        {
            req.Id,
            req.ReqRMNo,
            req.BankCode,
            req.BankName,
            req.PartnerCode,
            req.PartnerName,
            req.CreditContractNo,
            req.MortgageDate,
            req.TotalItems,
            req.ActiveItems,
            req.RedeemedItems,
            req.TotalCollateralValue,
            req.TotalLoanAmount,
            req.RemainingLoanAmount,
            req.InterestRate,
            req.LoanPeriodDays,
            status = req.Status.ToString(),
            req.Remark,
            req.CreatedBy,
            req.CreatedAt,
            details = req.Details.Select(d => new
            {
                d.Id,
                d.ItemRefNo,
                d.ModelCode,
                d.EngineNo,
                d.CQNo,
                d.CONo,
                d.DeclarationNo,
                d.CODate,
                d.CollateralValue,
                d.LoanAmount,
                status = d.Status.ToString(),
                d.Note
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Danh sách hồ sơ thế chấp ngân hàng (FrmMngRM_ReqMortgage)
app.MapGet("/api/mortgage/requests", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode, string? partnerCode) =>
{
    var q = db.MortgageRequests.Where(r => r.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(r => r.BankCode == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(partnerCode))
        q = q.Where(r => r.PartnerCode == partnerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MortgageStatus>(status, true, out var st))
        q = q.Where(r => r.Status == st);

    var list = await q.OrderByDescending(r => r.CreatedAt)
        .Select(r => new
        {
            r.Id,
            r.ReqRMNo,
            r.BankCode,
            r.BankName,
            r.PartnerCode,
            r.PartnerName,
            r.CreditContractNo,
            r.MortgageDate,
            r.TotalItems,
            r.ActiveItems,
            r.RedeemedItems,
            r.TotalCollateralValue,
            r.TotalLoanAmount,
            r.RemainingLoanAmount,
            r.InterestRate,
            r.LoanPeriodDays,
            status = r.Status.ToString(),
            r.Remark,
            r.RejectReason,
            r.CreatedBy,
            r.CreatedAt,
            r.ApprovedBy,
            r.ApprovedAt,
            r.FinishedBy,
            r.FinishedAt,
            r.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 hồ sơ thế chấp kèm danh sách tài sản (FrmMngRM_ReqMortgage.Detail)
app.MapGet("/api/mortgage/requests/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var req = await db.MortgageRequests
        .Include(r => r.Details)
        .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == tc.OrgId);

    if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ thế chấp #{id}." });

    return Results.Ok(new
    {
        req.Id,
        req.ReqRMNo,
        req.BankCode,
        req.BankName,
        req.PartnerCode,
        req.PartnerName,
        req.CreditContractNo,
        req.MortgageDate,
        req.TotalItems,
        req.ActiveItems,
        req.RedeemedItems,
        req.TotalCollateralValue,
        req.TotalLoanAmount,
        req.RemainingLoanAmount,
        req.InterestRate,
        req.LoanPeriodDays,
        status = req.Status.ToString(),
        req.Remark,
        req.RejectReason,
        req.CreatedBy,
        req.CreatedAt,
        req.ApprovedBy,
        req.ApprovedAt,
        req.FinishedBy,
        req.FinishedAt,
        req.CancelledAt,
        details = req.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.ItemRefNo,
            d.ModelCode,
            d.EngineNo,
            d.CQNo,
            d.CONo,
            d.DeclarationNo,
            d.CODate,
            d.CollateralValue,
            d.LoanAmount,
            status = d.Status.ToString(),
            d.ApprovedBy,
            d.ApprovedAt,
            d.ReqDMNo,
            d.RedeemedAt,
            d.Note
        })
    });
});

// 4) Phê duyệt đề nghị thế chấp & phong tỏa tài sản bảo đảm (RM_ReqMortgageDtl_Approve)
app.MapPost("/api/mortgage/requests/{id:long}/approve", async (long id, ApproveMortgageDto? dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.ApproveMortgageRequestAsync(id, tc.OrgId, dto?.ApproverName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ thế chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqRMNo,
            status = req.Status.ToString(),
            req.ActiveItems,
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Từ chối hồ sơ thế chấp
app.MapPost("/api/mortgage/requests/{id:long}/reject", async (long id, RejectMortgageDto dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.RejectMortgageRequestAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ thế chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqRMNo,
            status = req.Status.ToString(),
            req.RejectReason,
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Hủy hồ sơ thế chấp
app.MapPost("/api/mortgage/requests/{id:long}/cancel", async (long id, CancelMortgageDto? dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.CancelMortgageRequestAsync(id, tc.OrgId, dto?.Reason);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ thế chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqRMNo,
            status = req.Status.ToString(),
            req.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Tạo đề nghị giải chấp tài sản ngân hàng (RD_ReqRedeem_Create)
app.MapPost("/api/redeem/requests", async (CreateRedeemDto dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BankCode))
        return Results.BadRequest(new { error = "Cần mã ngân hàng nhận giải chấp (BankCode)." });
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Cần mã đối tác / đại lý giải chấp (PartnerCode)." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Đề nghị giải chấp phải có ít nhất 1 tài sản / xe cần giải phóng." });

    try
    {
        var req = await mgService.CreateRedeemRequestAsync(
            tc.OrgId,
            dto.ReqDMNo,
            dto.BankCode,
            dto.BankName,
            dto.PartnerCode,
            dto.PartnerName,
            dto.RedeemDate,
            dto.TotalSettlementAmount,
            dto.PaymentProofNo,
            dto.Remark,
            dto.Items
        );

        return Results.Ok(new
        {
            req.Id,
            req.ReqDMNo,
            req.BankCode,
            req.BankName,
            req.PartnerCode,
            req.PartnerName,
            req.RedeemDate,
            req.TotalItems,
            req.ApprovedItems,
            req.TotalSettlementAmount,
            req.PaymentProofNo,
            status = req.Status.ToString(),
            req.Remark,
            req.CreatedBy,
            req.CreatedAt,
            details = req.Details.Select(d => new
            {
                d.Id,
                d.ItemRefNo,
                d.ReqRMNo,
                d.ModelCode,
                d.DealerCode,
                d.SettlementAmount,
                status = d.Status.ToString(),
                d.Note
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Danh sách đề nghị giải chấp (FrmMngRedeem)
app.MapGet("/api/redeem/requests", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode) =>
{
    var q = db.RedeemRequests.Where(r => r.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(r => r.BankCode == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RedeemStatus>(status, true, out var st))
        q = q.Where(r => r.Status == st);

    var list = await q.OrderByDescending(r => r.CreatedAt)
        .Select(r => new
        {
            r.Id,
            r.ReqDMNo,
            r.BankCode,
            r.BankName,
            r.PartnerCode,
            r.PartnerName,
            r.RedeemDate,
            r.TotalItems,
            r.ApprovedItems,
            r.TotalSettlementAmount,
            r.PaymentProofNo,
            status = r.Status.ToString(),
            r.Remark,
            r.RejectReason,
            r.CreatedBy,
            r.CreatedAt,
            r.ApprovedBy,
            r.ApprovedAt,
            r.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 9) Chi tiết 1 đề nghị giải chấp kèm danh mục xe
app.MapGet("/api/redeem/requests/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var req = await db.RedeemRequests
        .Include(r => r.Details)
        .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == tc.OrgId);

    if (req == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị giải chấp #{id}." });

    return Results.Ok(new
    {
        req.Id,
        req.ReqDMNo,
        req.BankCode,
        req.BankName,
        req.PartnerCode,
        req.PartnerName,
        req.RedeemDate,
        req.TotalItems,
        req.ApprovedItems,
        req.TotalSettlementAmount,
        req.PaymentProofNo,
        status = req.Status.ToString(),
        req.Remark,
        req.RejectReason,
        req.CreatedBy,
        req.CreatedAt,
        req.ApprovedBy,
        req.ApprovedAt,
        req.CancelledAt,
        details = req.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.ItemRefNo,
            d.ReqRMNo,
            d.ModelCode,
            d.DealerCode,
            d.SettlementAmount,
            status = d.Status.ToString(),
            d.ApprovedBy,
            d.ApprovedAt,
            d.Note
        })
    });
});

// 10) Phê duyệt đề nghị giải chấp & giải phóng tài sản thế chấp (RD_ReqRedeemDtl_Approve & Check Finish RM_ReqMortgage)
app.MapPost("/api/redeem/requests/{id:long}/approve", async (long id, ApproveRedeemDto? dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.ApproveRedeemRequestAsync(id, tc.OrgId, dto?.ApproverName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị giải chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqDMNo,
            status = req.Status.ToString(),
            req.ApprovedItems,
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Từ chối đề nghị giải chấp
app.MapPost("/api/redeem/requests/{id:long}/reject", async (long id, RejectRedeemDto dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.RejectRedeemRequestAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị giải chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqDMNo,
            status = req.Status.ToString(),
            req.RejectReason,
            req.ApprovedBy,
            req.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Hủy đề nghị giải chấp
app.MapPost("/api/redeem/requests/{id:long}/cancel", async (long id, CancelRedeemDto? dto, MortgageRedeemService mgService, ITenantContext tc) =>
{
    try
    {
        var req = await mgService.CancelRedeemRequestAsync(id, tc.OrgId, dto?.Reason);
        if (req == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị giải chấp #{id}." });

        return Results.Ok(new
        {
            req.Id,
            req.ReqDMNo,
            status = req.Status.ToString(),
            req.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Lấy danh sách tài sản đang thế chấp để chọn giải chấp
app.MapGet("/api/mortgage/active-items", async (MortgageRedeemService mgService, ITenantContext tc, string? bankCode) =>
{
    var list = await mgService.GetActiveMortgagedItemsAsync(tc.OrgId, bankCode);
    return Results.Ok(list);
});

// 14) Báo cáo tổng hợp số liệu thế chấp & giải chấp ngân hàng
app.MapGet("/api/mortgage/summary", async (MortgageRedeemService mgService, ITenantContext tc) =>
{
    var summary = await mgService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

app.Run();

record CreatePayDto(long Amount, string? OrderId, string? OrderInfo, string? BankCode);
record RegisterOrgDto(string Name);
record ImportPayDto(string? TxnRef, string? OrderId, long Amount, string? OrderInfo, string? BankCode, bool IsPaid, DateTime? CreatedAt);
record CreateReconcileBatchDto(string? BatchCode, string? BankCode, string? AccountNo, DateTime? StatementDate, string? Note, List<BankStatementInputDto> Items);
record CreatePayoutBatchDto(string? BatchNo, string BankCode, string SourceAccount, string? SourceAccountName, string? BizResNumber, string? Remark, List<PayoutItemInputDto> Items);
record RejectPayoutDto(string? Reason);
record CreateDiscountRequestDto(string? DiscountNo, string PartnerCode, string? PartnerName, string? ContractNo, decimal? DefaultAnnualRate, string? Remark, List<DiscountItemInputDto> Items);
record SignDiscountDto(string? SignerName);
record RejectDiscountDto(string? Reason, string? RejecterName);
record PreviewDiscountDto(long OriginalAmount, decimal? AnnualDiscountRate, DateTime DueDate, DateTime? ActualPaymentDate);
record CreateGuaranteeDto(string? GuaranteeNo, string BankGuaranteeNo, string BankCode, string? BankName, string PartnerCode, string? PartnerName, string? ContractNo, GuaranteeType? GuaranteeType, long? TotalAmount, DateTime? DateOpen, DateTime? DateExpired, DateTime? DateEnd, decimal? FeePercent, string? Remark, List<GuaranteeItemInputDto>? Items);
record ApproveGuaranteeDto(string? ApproverName);
record ExtendGuaranteeDto(DateTime NewExpiredDate, DateTime? NewEndDate, string? Remark);
record AdjustGuaranteeValueDto(long NewTotalAmount, string? Remark);
record ReceiveRootGuaranteeDto(DateTime ReceiveDate, string? Note);
record ClaimGuaranteeDto(long ClaimAmount, string ClaimReason, string? ClaimedBy);
record SettleGuaranteeDto(string? SettlerName, string? Remark);
record RejectGuaranteeDto(string Reason, string? RejecterName);
record CancelGuaranteeDto(string? Reason);
record CreateMortgageDto(string? ReqRMNo, string BankCode, string? BankName, string PartnerCode, string? PartnerName, string? CreditContractNo, DateTime? MortgageDate, decimal? InterestRate, int? LoanPeriodDays, string? Remark, List<MortgageItemInputDto> Items);
record ApproveMortgageDto(string? ApproverName);
record RejectMortgageDto(string Reason, string? RejecterName);
record CancelMortgageDto(string? Reason);
record CreateRedeemDto(string? ReqDMNo, string BankCode, string? BankName, string PartnerCode, string? PartnerName, DateTime? RedeemDate, long? TotalSettlementAmount, string? PaymentProofNo, string? Remark, List<RedeemItemInputDto> Items);
record ApproveRedeemDto(string? ApproverName);
record RejectRedeemDto(string Reason, string? RejecterName);
record CancelRedeemDto(string? Reason);
