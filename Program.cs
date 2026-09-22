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

app.Run();

record CreatePayDto(long Amount, string? OrderId, string? OrderInfo, string? BankCode);
record RegisterOrgDto(string Name);
record ImportPayDto(string? TxnRef, string? OrderId, long Amount, string? OrderInfo, string? BankCode, bool IsPaid, DateTime? CreatedAt);
record CreateReconcileBatchDto(string? BatchCode, string? BankCode, string? AccountNo, DateTime? StatementDate, string? Note, List<BankStatementInputDto> Items);
record CreatePayoutBatchDto(string? BatchNo, string BankCode, string SourceAccount, string? SourceAccountName, string? BizResNumber, string? Remark, List<PayoutItemInputDto> Items);
record RejectPayoutDto(string? Reason);
