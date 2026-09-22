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
builder.Services.AddScoped<PaymentOrderService>();
builder.Services.AddScoped<BankBillService>();
builder.Services.AddScoped<PaymentPDIService>();
builder.Services.AddScoped<LatePaymentPenaltyService>();
builder.Services.AddScoped<TransportInsPaymentService>();
builder.Services.AddScoped<PaymentStorageService>();
builder.Services.AddScoped<GuaranteeExtensionService>();
builder.Services.AddScoped<BankGuaranteeClaimService>();

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

// ===== Quản lý Phiếu thanh toán & Ủy nhiệm chi ngân hàng (Payment Order & Bank UNC Settlement - BizHTC.Payment) =====

// 1) Tạo mới phiếu thanh toán & lập chứng từ UNC (Pmt_Payment_Save / FrmNewPM)
app.MapPost("/api/payment-orders", async (CreatePaymentOrderDto dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Mã đối tác / đại lý (PartnerCode / DealerCode) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.BankAccountSend))
        return Results.BadRequest(new { error = "Số tài khoản trích nợ (BankAccountSend) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.BankCodeSend))
        return Results.BadRequest(new { error = "Ngân hàng trích nợ (BankCodeSend) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.BankAccountReceive))
        return Results.BadRequest(new { error = "Số tài khoản thụ hưởng (BankAccountReceive) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.BankCodeReceive))
        return Results.BadRequest(new { error = "Ngân hàng thụ hưởng (BankCodeReceive) không được để trống." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Phiếu thanh toán cần ít nhất 1 dòng xe / đơn hàng (ListPMDetail)." });

    try
    {
        var order = await pmtService.CreatePaymentOrderAsync(
            tc.OrgId,
            dto.PaymentNo,
            dto.PaymentType ?? PaymentOrderType.UNC,
            dto.BankPaymentNo,
            dto.PaymentEndDate,
            dto.PartnerCode,
            dto.PartnerName,
            dto.BankCodeSend,
            dto.BankNameSend,
            dto.BankAccountSend,
            dto.BankCodeReceive,
            dto.BankNameReceive,
            dto.BankAccountReceive,
            dto.Funds ?? PaymentFundType.OwnCapital,
            dto.BankLending,
            dto.InterestRate,
            dto.LoanPeriodMonths,
            dto.AccountingRecordNo,
            dto.Remark,
            "KeToanThanhToan",
            dto.Items
        );

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            paymentType = order.PaymentType.ToString(),
            order.BankPaymentNo,
            order.PaymentEndDate,
            order.PartnerCode,
            order.PartnerName,
            order.BankCodeSend,
            order.BankNameSend,
            order.BankAccountSend,
            order.BankCodeReceive,
            order.BankNameReceive,
            order.BankAccountReceive,
            funds = order.Funds.ToString(),
            order.BankLending,
            order.InterestRate,
            order.LoanPeriodMonths,
            order.AccountingRecordNo,
            order.TotalAmount,
            order.TotalAccumAmount,
            status = order.Status.ToString(),
            order.Remark,
            order.CreatedBy,
            order.CreatedAt,
            details = order.Details.Select(d => new
            {
                d.Id,
                d.ItemRefNo,
                d.Description,
                d.ModelCode,
                d.UnitPriceActual,
                d.AmountAccum,
                d.PercentAccum,
                d.Amount,
                d.PercentCurrent,
                d.AmountTotal,
                d.PercentTotal,
                d.GuaranteeNo,
                d.BankGrtNo,
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

// 2) Danh sách phiếu thanh toán (Pmt_Payment_Get / FrmMngPM)
app.MapGet("/api/payment-orders", async (AppDbContext db, ITenantContext tc, string? status, string? paymentType, string? bankCode, string? partnerCode) =>
{
    var q = db.PaymentOrders.Where(p => p.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(p => p.BankCodeSend == bankCode.ToUpper() || p.BankCodeReceive == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(partnerCode))
        q = q.Where(p => p.PartnerCode == partnerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentOrderStatus>(status, true, out var st))
        q = q.Where(p => p.Status == st);
    if (!string.IsNullOrWhiteSpace(paymentType) && Enum.TryParse<PaymentOrderType>(paymentType, true, out var pt))
        q = q.Where(p => p.PaymentType == pt);

    var list = await q.OrderByDescending(p => p.CreatedAt)
        .Select(p => new
        {
            p.Id,
            p.PaymentNo,
            paymentType = p.PaymentType.ToString(),
            p.BankPaymentNo,
            p.PaymentEndDate,
            p.PartnerCode,
            p.PartnerName,
            p.BankCodeSend,
            p.BankNameSend,
            p.BankAccountSend,
            p.BankCodeReceive,
            p.BankNameReceive,
            p.BankAccountReceive,
            funds = p.Funds.ToString(),
            p.BankLending,
            p.InterestRate,
            p.LoanPeriodMonths,
            p.AccountingRecordNo,
            p.TotalAmount,
            p.TotalAccumAmount,
            status = p.Status.ToString(),
            p.Remark,
            p.RejectReason,
            p.CreatedBy,
            p.CreatedAt,
            p.ApprovedBy,
            p.ApprovedAt,
            p.FinishedBy,
            p.FinishedAt,
            p.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 phiếu thanh toán kèm danh sách xe / đơn hàng (Pmt_PaymentDetail_Get / FrmMngPM.LoadPMDetail)
app.MapGet("/api/payment-orders/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var order = await db.PaymentOrders
        .Include(p => p.Details)
        .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == tc.OrgId);

    if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

    return Results.Ok(new
    {
        order.Id,
        order.PaymentNo,
        paymentType = order.PaymentType.ToString(),
        order.BankPaymentNo,
        order.PaymentEndDate,
        order.PartnerCode,
        order.PartnerName,
        order.BankCodeSend,
        order.BankNameSend,
        order.BankAccountSend,
        order.BankCodeReceive,
        order.BankNameReceive,
        order.BankAccountReceive,
        funds = order.Funds.ToString(),
        order.BankLending,
        order.InterestRate,
        order.LoanPeriodMonths,
        order.AccountingRecordNo,
        order.TotalAmount,
        order.TotalAccumAmount,
        status = order.Status.ToString(),
        order.Remark,
        order.RejectReason,
        order.CreatedBy,
        order.CreatedAt,
        order.ApprovedBy,
        order.ApprovedAt,
        order.FinishedBy,
        order.FinishedAt,
        order.CancelledAt,
        details = order.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.ItemRefNo,
            d.Description,
            d.ModelCode,
            d.UnitPriceActual,
            d.AmountAccum,
            d.PercentAccum,
            d.Amount,
            d.PercentCurrent,
            d.AmountTotal,
            d.PercentTotal,
            d.GuaranteeNo,
            d.BankGrtNo,
            status = d.Status.ToString(),
            d.Note
        })
    });
});

// 4) Phê duyệt phiếu thanh toán (FrmMngPM.btnApprove_Click / FrmNewPM.MODE_APPROVE_PM)
app.MapPost("/api/payment-orders/{id:long}/approve", async (long id, ApprovePaymentOrderDto? dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var order = await pmtService.ApprovePaymentOrderAsync(id, tc.OrgId, dto?.ApproverName);
        if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            status = order.Status.ToString(),
            order.ApprovedBy,
            order.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Xác nhận hoàn tất thanh toán & hạn thanh toán thực tế (FrmMngPM.btnConfirmEndDate_Click)
app.MapPost("/api/payment-orders/{id:long}/finish", async (long id, FinishPaymentOrderDto? dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var order = await pmtService.ConfirmFinishPaymentOrderAsync(
            id, tc.OrgId, dto?.PaymentEndDate, dto?.FinisherName, dto?.BankPaymentNo);
        if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            status = order.Status.ToString(),
            order.PaymentEndDate,
            order.BankPaymentNo,
            order.FinishedBy,
            order.FinishedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Quay lui trạng thái phiếu thanh toán từ Finished về Approved (FrmMngPM.btnRevertConfirmed_Click)
app.MapPost("/api/payment-orders/{id:long}/revert", async (long id, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var order = await pmtService.RevertPaymentOrderAsync(id, tc.OrgId);
        if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            status = order.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Từ chối phê duyệt phiếu thanh toán (FrmMngPM.btnDealerReject_Click)
app.MapPost("/api/payment-orders/{id:long}/reject", async (long id, RejectPaymentOrderDto dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var order = await pmtService.RejectPaymentOrderAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            status = order.Status.ToString(),
            order.RejectReason,
            order.ApprovedBy,
            order.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Hủy phiếu thanh toán (FrmMngPM.btnCancel_Click)
app.MapPost("/api/payment-orders/{id:long}/cancel", async (long id, CancelPaymentOrderDto? dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var order = await pmtService.CancelPaymentOrderAsync(id, tc.OrgId, dto?.Reason);
        if (order == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });

        return Results.Ok(new
        {
            order.Id,
            order.PaymentNo,
            status = order.Status.ToString(),
            order.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Sinh nội dung mẫu Ủy nhiệm chi ngân hàng (Pmt_Payment_GetUNCContent_New20221111 / btnInUNC_Click)
app.MapGet("/api/payment-orders/{id:long}/unc-content", async (long id, PaymentOrderService pmtService, ITenantContext tc) =>
{
    var advice = await pmtService.GenerateUNCContentAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thanh toán #{id}." });
    return Results.Ok(advice);
});

// 10) Cập nhật nhanh lãi suất và kỳ hạn vay theo lô (Pmt_Payment_UpdateInterestRate_LoanPeriod / FrmUpdate_Pmt_Payment)
app.MapPost("/api/payment-orders/update-rates", async (UpdatePaymentRatesDto dto, PaymentOrderService pmtService, ITenantContext tc) =>
{
    try
    {
        var updatedCount = await pmtService.UpdateInterestRateLoanPeriodAsync(
            tc.OrgId, dto.PaymentIds, dto.InterestRate, dto.LoanPeriodMonths);
        return Results.Ok(new { updatedCount, dto.InterestRate, dto.LoanPeriodMonths });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Báo cáo tổng hợp số liệu thanh toán (Payment Order Summary)
app.MapGet("/api/payment-orders/summary", async (PaymentOrderService pmtService, ITenantContext tc) =>
{
    var summary = await pmtService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Biên bản Bàn giao Xe & Chứng từ theo Hối phiếu ngân hàng (Bank Bill of Exchange / Bank Draft - FrmQuanLyBBBGTheoHoiPhieu & FrmTaoBBBGTheoHoiPhieu) =====

// 1) Lập biên bản bàn giao xe & chứng từ theo hối phiếu mới (Car_BankBillMinutes_Add / FrmTaoBBBGTheoHoiPhieu)
app.MapPost("/api/bank-bills", async (CreateBankBillDto dto, BankBillService billService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BankCode))
        return Results.BadRequest(new { error = "Cần mã ngân hàng nhận hối phiếu (BankCode: VPB, CTG, MBB, TCB, VCB...)." });
    if (string.IsNullOrWhiteSpace(dto.PartnerCode))
        return Results.BadRequest(new { error = "Cần mã đại lý ký phát hối phiếu (PartnerCode / DealerCode)." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Biên bản bàn giao hối phiếu cần ít nhất 1 xe / hồ sơ gốc." });

    try
    {
        var minutes = await billService.CreateMinutesAsync(
            tc.OrgId,
            dto.BankBillMnNo,
            dto.BankCode,
            dto.BankName,
            dto.PartnerCode,
            dto.PartnerName,
            dto.BankBillDate,
            dto.BankBillPrintDate,
            dto.Remark,
            "ChuyenVienHopDong",
            dto.Items
        );

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            minutes.BankCode,
            minutes.BankName,
            minutes.PartnerCode,
            minutes.PartnerName,
            minutes.BankBillDate,
            minutes.BankBillPrintDate,
            minutes.BankBillReciveDate,
            minutes.TotalVehicles,
            minutes.TotalClaimAmount,
            status = minutes.Status.ToString(),
            minutes.Remark,
            minutes.CreatedBy,
            minutes.CreatedAt,
            details = minutes.Details.Select(d => new
            {
                d.Id,
                d.VIN,
                d.ModelCode,
                d.SpecCode,
                d.SpecDescription,
                d.EngineNo,
                d.CONo,
                d.CabinCONo,
                d.DeclarationNo,
                d.BankGuaranteeNo,
                d.HTCInvoiceNo,
                d.TCGInvoiceNo,
                d.TransportMinutesNo,
                d.ClaimAmount,
                d.GuaranteeDateStart,
                d.GuaranteeDateOpen,
                d.NumberOfDaysDeferred,
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

// 2) Danh sách biên bản bàn giao xe theo hối phiếu (Car_BankBillMinutes_Get / FrmQuanLyBBBGTheoHoiPhieu)
app.MapGet("/api/bank-bills", async (AppDbContext db, ITenantContext tc, string? status, string? bankCode, string? partnerCode) =>
{
    var q = db.BankBillMinutes.Where(b => b.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(bankCode))
        q = q.Where(b => b.BankCode == bankCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(partnerCode))
        q = q.Where(b => b.PartnerCode == partnerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BankBillMinutesStatus>(status, true, out var st))
        q = q.Where(b => b.Status == st);

    var list = await q.OrderByDescending(b => b.CreatedAt)
        .Select(b => new
        {
            b.Id,
            b.BankBillMnNo,
            b.BankCode,
            b.BankName,
            b.PartnerCode,
            b.PartnerName,
            b.BankBillDate,
            b.BankBillPrintDate,
            b.BankBillReciveDate,
            b.TotalVehicles,
            b.TotalClaimAmount,
            status = b.Status.ToString(),
            b.Remark,
            b.CreatedBy,
            b.CreatedAt,
            b.HandedOverBy,
            b.HandedOverAt,
            b.BankReceivedBy,
            b.BankReceivedAt,
            b.SettledBy,
            b.SettledAt,
            b.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 biên bản kèm toàn bộ danh sách hồ sơ xe (FrmQuanLyBBBGTheoHoiPhieu.LoadGridViewDetail)
app.MapGet("/api/bank-bills/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var minutes = await db.BankBillMinutes
        .Include(b => b.Details)
        .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == tc.OrgId);

    if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản bàn giao hối phiếu #{id}." });

    return Results.Ok(new
    {
        minutes.Id,
        minutes.BankBillMnNo,
        minutes.BankCode,
        minutes.BankName,
        minutes.PartnerCode,
        minutes.PartnerName,
        minutes.BankBillDate,
        minutes.BankBillPrintDate,
        minutes.BankBillReciveDate,
        minutes.TotalVehicles,
        minutes.TotalClaimAmount,
        status = minutes.Status.ToString(),
        minutes.Remark,
        minutes.CreatedBy,
        minutes.CreatedAt,
        minutes.HandedOverBy,
        minutes.HandedOverAt,
        minutes.BankReceivedBy,
        minutes.BankReceivedAt,
        minutes.SettledBy,
        minutes.SettledAt,
        minutes.CancelledAt,
        details = minutes.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.VIN,
            d.ModelCode,
            d.SpecCode,
            d.SpecDescription,
            d.EngineNo,
            d.CONo,
            d.CabinCONo,
            d.DeclarationNo,
            d.BankGuaranteeNo,
            d.HTCInvoiceNo,
            d.TCGInvoiceNo,
            d.TransportMinutesNo,
            d.ClaimAmount,
            d.GuaranteeDateStart,
            d.GuaranteeDateOpen,
            d.NumberOfDaysDeferred,
            status = d.Status.ToString(),
            d.Note
        })
    });
});

// 4) Xuất trình và bàn giao bộ chứng từ gốc sang Ngân hàng
app.MapPost("/api/bank-bills/{id:long}/handover", async (long id, HandoverBankBillDto? dto, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.HandoverToBankAsync(id, tc.OrgId, dto?.HandedOverBy, dto?.Note);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            status = minutes.Status.ToString(),
            minutes.HandedOverBy,
            minutes.HandedOverAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Ngân hàng xác nhận tiếp nhận đủ hồ sơ gốc xe theo hối phiếu (Car_BankBillMinutes_Save / BankBillReciveDate)
app.MapPost("/api/bank-bills/{id:long}/bank-receive", async (long id, BankReceiveDto dto, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.BankConfirmReceiveAsync(id, tc.OrgId, dto.ReceiveDate, dto.ReceivedBy, dto.Note);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            status = minutes.Status.ToString(),
            minutes.BankBillReciveDate,
            minutes.BankReceivedBy,
            minutes.BankReceivedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Quyết toán hoàn tất thanh toán hối phiếu khi dòng tiền đã chuyển đủ
app.MapPost("/api/bank-bills/{id:long}/settle", async (long id, SettleBankBillDto? dto, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.SettleBankBillAsync(id, tc.OrgId, dto?.SettlerName, dto?.Note);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            status = minutes.Status.ToString(),
            minutes.SettledBy,
            minutes.SettledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Hủy biên bản bàn giao xe theo hối phiếu (FrmQuanLyBBBGTheoHoiPhieu.btnDelete_Click)
app.MapPost("/api/bank-bills/{id:long}/cancel", async (long id, CancelBankBillDto? dto, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.CancelMinutesAsync(id, tc.OrgId, dto?.Reason);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            status = minutes.Status.ToString(),
            minutes.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Sinh dữ liệu mẫu in Hối phiếu thương mại ngân hàng (Bank Acceptance Draft Advice - FrmPopupChonMauNHInHoiPhieu / btnExportHoiPhieu_Click)
app.MapGet("/api/bank-bills/{id:long}/bill-advice", async (long id, BankBillService billService, ITenantContext tc) =>
{
    var advice = await billService.GenerateBillOfExchangeAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });
    return Results.Ok(advice);
});

// 9) Bổ sung danh sách xe VIN vào biên bản hiện có (FrmTaoBBBGTheoHoiPhieu.btnImport_Click)
app.MapPost("/api/bank-bills/{id:long}/import-vins", async (long id, ImportVinsDto dto, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.ImportVinsAsync(id, tc.OrgId, dto.Items);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            minutes.TotalVehicles,
            minutes.TotalClaimAmount,
            status = minutes.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Xóa 1 xe khỏi biên bản bàn giao
app.MapDelete("/api/bank-bills/{id:long}/vins/{detailId:long}", async (long id, long detailId, BankBillService billService, ITenantContext tc) =>
{
    try
    {
        var minutes = await billService.RemoveVinAsync(id, detailId, tc.OrgId);
        if (minutes == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản #{id}." });

        return Results.Ok(new
        {
            minutes.Id,
            minutes.BankBillMnNo,
            minutes.TotalVehicles,
            minutes.TotalClaimAmount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Báo cáo tổng hợp số liệu biên bản bàn giao hối phiếu
app.MapGet("/api/bank-bills/summary", async (BankBillService billService, ITenantContext tc) =>
{
    var summary = await billService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Bảng kê Thanh toán Chi phí Kiểm tra Xe PDI (Pre-Delivery Inspection Payment - BizHTC.Payment) =====

// 1) Tạo bảng kê thanh toán chi phí PDI mới (Job_Pmt_PaymentPDI_Create / FrmQuanLyThanhToanPDI)
app.MapPost("/api/payment-pdi", async (CreatePaymentPDIDto dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.PmtMonth))
        return Results.BadRequest(new { error = "Kỳ / tháng thanh toán (PmtMonth, ví dụ 2025-05) không được để trống." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Bảng kê chi phí PDI cần ít nhất 1 dòng xe kiểm tra." });

    try
    {
        var pdi = await pdiService.CreatePaymentPDIAsync(
            tc.OrgId,
            dto.PmtPDINo,
            dto.PmtMonth,
            dto.ServiceUnitCode,
            dto.ServiceUnitName,
            dto.VATRate ?? 10.0m,
            dto.Remark,
            "ChuyenVienKiemDinh",
            dto.Items
        );

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            pdi.PmtMonth,
            pdi.ServiceUnitCode,
            pdi.ServiceUnitName,
            pdi.TotalVehicles,
            pdi.TotalCostIn,
            pdi.TotalCostOut,
            pdi.TotalAmount,
            pdi.VATRate,
            pdi.AmountVAT,
            pdi.TotalAmountAfterVAT,
            status = pdi.Status.ToString(),
            tcmsSignStatus = pdi.TCMSSignStatus.ToString(),
            htvSignStatus = pdi.HTVSignStatus.ToString(),
            pdi.Remark,
            pdi.CreatedBy,
            pdi.CreatedAt,
            details = pdi.Details.Select(d => new
            {
                d.Id,
                d.VIN,
                d.CarId,
                d.ModelCode,
                d.ModelName,
                d.SpecCode,
                d.SpecDescription,
                d.ColorExtNameVN,
                d.StorageCodeInit,
                d.StoreDate,
                d.DeliveryOutDate,
                d.DlvMnNo,
                d.DealerCode,
                d.CostInCheck,
                d.CostOutCheck,
                d.TotalCostCheck,
                status = d.Status.ToString(),
                d.Remark
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Danh sách bảng kê chi phí PDI (Pmt_PaymentPDI_Get / FrmQuanLyThanhToanPDI)
app.MapGet("/api/payment-pdi", async (PaymentPDIService pdiService, ITenantContext tc, string? status, string? pmtMonth, string? serviceUnitCode) =>
{
    var list = await pdiService.GetPaymentPDIsAsync(tc.OrgId, status, pmtMonth, serviceUnitCode);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PmtPDINo,
        p.PmtMonth,
        p.ServiceUnitCode,
        p.ServiceUnitName,
        p.TotalVehicles,
        p.TotalCostIn,
        p.TotalCostOut,
        p.TotalAmount,
        p.VATRate,
        p.AmountVAT,
        p.TotalAmountAfterVAT,
        status = p.Status.ToString(),
        tcmsSignStatus = p.TCMSSignStatus.ToString(),
        p.TCMSSignUser,
        p.TCMSSignDTime,
        htvSignStatus = p.HTVSignStatus.ToString(),
        p.HTVSignUser,
        p.HTVSignDTime,
        p.BankTxnRef,
        p.PaidBy,
        p.PaidAt,
        p.Remark,
        p.CreatedAt
    }));
});

// 3) Chi tiết 1 bảng kê PDI kèm danh sách toàn bộ xe (FrmQuanLyThanhToanPDI.loadGridDetail)
app.MapGet("/api/payment-pdi/{id:long}", async (long id, PaymentPDIService pdiService, ITenantContext tc) =>
{
    var pdi = await pdiService.GetPaymentPDIByIdAsync(id, tc.OrgId);
    if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

    return Results.Ok(new
    {
        pdi.Id,
        pdi.PmtPDINo,
        pdi.PmtMonth,
        pdi.ServiceUnitCode,
        pdi.ServiceUnitName,
        pdi.TotalVehicles,
        pdi.TotalCostIn,
        pdi.TotalCostOut,
        pdi.TotalAmount,
        pdi.VATRate,
        pdi.AmountVAT,
        pdi.TotalAmountAfterVAT,
        status = pdi.Status.ToString(),
        tcmsSignStatus = pdi.TCMSSignStatus.ToString(),
        pdi.TCMSSignUser,
        pdi.TCMSSignDTime,
        htvSignStatus = pdi.HTVSignStatus.ToString(),
        pdi.HTVSignUser,
        pdi.HTVSignDTime,
        pdi.Appr1By,
        pdi.Appr1DTime,
        pdi.Appr2By,
        pdi.Appr2DTime,
        pdi.PaidBy,
        pdi.PaidAt,
        pdi.BankTxnRef,
        pdi.RejectReason,
        pdi.CancelledAt,
        pdi.FilePath,
        pdi.Remark,
        pdi.CreatedBy,
        pdi.CreatedAt,
        details = pdi.Details.Select(d => new
        {
            d.Id,
            d.VIN,
            d.CarId,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.SpecDescription,
            d.ColorExtNameVN,
            d.StorageCodeInit,
            d.StoreDate,
            d.DeliveryOutDate,
            d.DlvMnNo,
            d.DealerCode,
            d.CostInCheck,
            d.CostOutCheck,
            d.TotalCostCheck,
            status = d.Status.ToString(),
            d.Remark
        })
    });
});

// 4) TCMS Duyệt cấp 1 & Ký số điện tử (Pmt_PaymentPDI_TCMSApproveAndSign / FrmQuanLyThanhToanPDI.btnTCMSApprove_Click)
app.MapPost("/api/payment-pdi/{id:long}/tcms-approve", async (long id, SignPDIDto? dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    try
    {
        var pdi = await pdiService.ApproveTCMSAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            status = pdi.Status.ToString(),
            tcmsSignStatus = pdi.TCMSSignStatus.ToString(),
            pdi.TCMSSignUser,
            pdi.TCMSSignDTime
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) HTV Duyệt cấp 2 & Ký số điện tử (Pmt_PaymentPDI_HTVApproveAndSign / FrmQuanLyThanhToanPDI.btnHTVApprove_Click)
app.MapPost("/api/payment-pdi/{id:long}/htv-approve", async (long id, SignPDIDto? dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    try
    {
        var pdi = await pdiService.ApproveHTVAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            status = pdi.Status.ToString(),
            htvSignStatus = pdi.HTVSignStatus.ToString(),
            pdi.HTVSignUser,
            pdi.HTVSignDTime
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Quyết toán / Hoàn tất thanh toán chi phí PDI (Paid)
app.MapPost("/api/payment-pdi/{id:long}/settle", async (long id, SettlePDIDto? dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    try
    {
        var pdi = await pdiService.SettlePaymentPDIAsync(id, tc.OrgId, dto?.BankTxnRef, dto?.PayerName);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            status = pdi.Status.ToString(),
            pdi.PaidBy,
            pdi.PaidAt,
            pdi.BankTxnRef
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Từ chối phê duyệt bảng kê PDI kèm lý do (FrmQuanLyThanhToanPDI.btnDeny_Click)
app.MapPost("/api/payment-pdi/{id:long}/reject", async (long id, RejectPDIDto dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.Reason))
        return Results.BadRequest(new { error = "Cần cung cấp lý do từ chối (Reason)." });

    try
    {
        var pdi = await pdiService.RejectPaymentPDIAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            status = pdi.Status.ToString(),
            pdi.RejectReason
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Hủy bảng kê chi phí PDI (Pmt_PaymentPDI_Cancel / FrmQuanLyThanhToanPDI.btnDelete_Click)
app.MapPost("/api/payment-pdi/{id:long}/cancel", async (long id, CancelPDIDto? dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    try
    {
        var pdi = await pdiService.CancelPaymentPDIAsync(id, tc.OrgId, dto?.Reason);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            status = pdi.Status.ToString(),
            pdi.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Cập nhật chi phí PDI xe hàng loạt (Pmt_PaymentPDI_UpdateMulti / FrmSuaThanhToanPDI.btnSave_Click)
app.MapPut("/api/payment-pdi/{id:long}/details", async (long id, UpdatePDIDetailsDto dto, PaymentPDIService pdiService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Danh sách cập nhật chi phí (Items) không được để trống." });

    try
    {
        var pdi = await pdiService.UpdateDetailsAsync(id, tc.OrgId, dto.Items);
        if (pdi == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });

        return Results.Ok(new
        {
            pdi.Id,
            pdi.PmtPDINo,
            pdi.TotalVehicles,
            pdi.TotalCostIn,
            pdi.TotalCostOut,
            pdi.TotalAmount,
            pdi.AmountVAT,
            pdi.TotalAmountAfterVAT,
            status = pdi.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Sinh dữ liệu mẫu in Bảng kê quyết toán PDI (PDI Statement Advice - FrmQuanLyThanhToanPDI.btnPrint_Click)
app.MapGet("/api/payment-pdi/{id:long}/statement-advice", async (long id, PaymentPDIService pdiService, ITenantContext tc) =>
{
    var advice = await pdiService.GenerateStatementAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê PDI #{id}." });
    return Results.Ok(advice);
});

// 11) Báo cáo tổng hợp số liệu thanh toán PDI
app.MapGet("/api/payment-pdi/summary", async (PaymentPDIService pdiService, ITenantContext tc) =>
{
    var summary = await pdiService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý & Tính Phạt Chậm Thanh Toán Đơn Hàng / Hợp Đồng Xe (Late Payment Delay Penalty Settlement - BizHTC.Report / BizHTC.Payment / FrmRptPenaltyPmtDelay & FrmUpdatePenaltyPmtDelayReal) =====

// 1) Lập hồ sơ tính phạt chậm thanh toán mới
app.MapPost("/api/penalty-delay", async (CreateLatePaymentPenaltyDto dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.SOCode))
        return Results.BadRequest(new { error = "Số đơn hàng (SOCode) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.DealerCode))
        return Results.BadRequest(new { error = "Mã đại lý (DealerCode) không được để trống." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Cần ít nhất 1 dòng xe kiểm tra nghĩa vụ thanh toán." });

    try
    {
        var penalty = await penaltyService.CreatePenaltyRecordAsync(
            tc.OrgId,
            dto.PenaltyRecordNo,
            dto.SOCode,
            dto.DealerCode,
            dto.DealerName,
            dto.ContractNo,
            dto.SOApprovedDate,
            dto.PenaltyRateAnnual,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.SOCode,
            penalty.DealerCode,
            penalty.DealerName,
            penalty.ContractNo,
            penalty.TotalApprovedQuantity,
            penalty.TotalUnitPriceActual,
            penalty.TotalDatePenalty,
            penalty.PenaltyRateAnnual,
            penalty.AmountPenaltySystem,
            penalty.PenalizeActual,
            penalty.WaivedAmount,
            status = penalty.Status.ToString(),
            penalty.CreatedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Danh sách hồ sơ phạt chậm thanh toán (kèm bộ lọc đa chiều)
app.MapGet("/api/penalty-delay", async (AppDbContext db, ITenantContext tc, string? status, string? dealerCode, string? soCode) =>
{
    var q = db.LatePaymentPenalties.Where(p => p.OrgId == tc.OrgId);
    if (!string.IsNullOrWhiteSpace(dealerCode))
        q = q.Where(p => p.DealerCode == dealerCode.ToUpper());
    if (!string.IsNullOrWhiteSpace(soCode))
        q = q.Where(p => p.SOCode.Contains(soCode.ToUpper()));
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LatePaymentPenaltyStatus>(status, true, out var st))
        q = q.Where(p => p.Status == st);

    var list = await q.OrderByDescending(p => p.CreatedAt)
        .Select(p => new
        {
            p.Id,
            p.PenaltyRecordNo,
            p.SOCode,
            p.DealerCode,
            p.DealerName,
            p.ContractNo,
            p.SOApprovedDate,
            p.TotalApprovedQuantity,
            p.TotalUnitPriceActual,
            p.MaxDelayDaysDeposit,
            p.MaxDelayDaysGrtOpen,
            p.MaxDelayDaysGrtPay,
            p.MaxDelayDays60Pmt,
            p.MaxDelayDaysRemain,
            p.TotalDatePenalty,
            p.PenaltyRateAnnual,
            p.AmountPenaltySystem,
            p.PenalizeActual,
            p.WaivedAmount,
            status = p.Status.ToString(),
            p.AdjustmentReason,
            p.PaymentProofRef,
            p.CreatedBy,
            p.CreatedAt,
            p.ReviewedBy,
            p.ReviewedAt,
            p.ApprovedBy,
            p.ApprovedAt,
            p.SettledBy,
            p.SettledAt,
            p.CancelledAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 hồ sơ phạt kèm danh sách xe & các mốc tiến độ thanh toán
app.MapGet("/api/penalty-delay/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var penalty = await db.LatePaymentPenalties
        .Include(p => p.Details)
        .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == tc.OrgId);

    if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

    return Results.Ok(new
    {
        penalty.Id,
        penalty.PenaltyRecordNo,
        penalty.SOCode,
        penalty.DealerCode,
        penalty.DealerName,
        penalty.ContractNo,
        penalty.SOApprovedDate,
        penalty.TotalApprovedQuantity,
        penalty.TotalUnitPriceActual,
        penalty.MaxDelayDaysDeposit,
        penalty.MaxDelayDaysGrtOpen,
        penalty.MaxDelayDaysGrtPay,
        penalty.MaxDelayDays60Pmt,
        penalty.MaxDelayDaysRemain,
        penalty.TotalDatePenalty,
        penalty.PenaltyRateAnnual,
        penalty.AmountPenaltySystem,
        penalty.PenalizeActual,
        penalty.WaivedAmount,
        status = penalty.Status.ToString(),
        penalty.Remark,
        penalty.AdjustmentReason,
        penalty.PaymentProofRef,
        penalty.CreatedBy,
        penalty.CreatedAt,
        penalty.ReviewedBy,
        penalty.ReviewedAt,
        penalty.ApprovedBy,
        penalty.ApprovedAt,
        penalty.SettledBy,
        penalty.SettledAt,
        penalty.CancelledAt,
        details = penalty.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.CarId,
            d.VIN,
            d.ModelCode,
            d.ModelName,
            d.ColorName,
            d.UnitPriceActual,
            d.DepositDueDate,
            d.ActualDepositDate,
            d.GrtDueDate,
            d.ActualGrtDate,
            d.GrtPayDueDate,
            d.ActualGrtPayDate,
            d.Payment60DueDate,
            d.Actual60PayDate,
            d.PaymentRemainDueDate,
            d.ActualRemainPayDate,
            d.DelayDaysDeposit,
            d.DelayDaysGrtOpen,
            d.DelayDaysGrtPay,
            d.DelayDays60Pmt,
            d.DelayDaysRemain,
            d.MaxDelayDays,
            d.ItemPenaltyAmount,
            d.ActualItemPenalty,
            status = d.Status.ToString(),
            d.Note
        })
    });
});

// 4) Tự động tái tính toán số ngày và tiền phạt hệ thống (Rpt_PenaltyPmtDelay)
app.MapPost("/api/penalty-delay/{id:long}/calculate", async (long id, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.CalculatePenaltyAsync(id, tc.OrgId);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.TotalDatePenalty,
            penalty.AmountPenaltySystem,
            penalty.PenalizeActual,
            penalty.WaivedAmount,
            status = penalty.Status.ToString(),
            penalty.CalculatedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Thẩm định tài chính đề xuất mức phạt thực tế & miễn giảm (FrmUpdatePenaltyPmtDelayReal / Review)
app.MapPost("/api/penalty-delay/{id:long}/review", async (long id, ReviewPenaltyDto dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.ReviewPenaltyAsync(
            id, tc.OrgId, dto.ProposedPenalizeActual, dto.AdjustmentReason, dto.ReviewerName);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.AmountPenaltySystem,
            penalty.PenalizeActual,
            penalty.WaivedAmount,
            status = penalty.Status.ToString(),
            penalty.AdjustmentReason,
            penalty.ReviewedBy,
            penalty.ReviewedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ban Giám đốc phê duyệt chốt mức phạt thực tế (Ord_SalesOrder_UpdatePenalizeActual / Approve)
app.MapPost("/api/penalty-delay/{id:long}/approve", async (long id, ApprovePenaltyDto? dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.ApprovePenaltyAsync(id, tc.OrgId, dto?.ApproverName);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.PenalizeActual,
            status = penalty.Status.ToString(),
            penalty.ApprovedBy,
            penalty.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Quyết toán thu nộp phạt hoặc cấn trừ công nợ bán xe (Settled)
app.MapPost("/api/penalty-delay/{id:long}/settle", async (long id, SettlePenaltyDto? dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.SettlePenaltyAsync(
            id, tc.OrgId, dto?.PaymentProofRef, dto?.SettlerName);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.PenalizeActual,
            status = penalty.Status.ToString(),
            penalty.PaymentProofRef,
            penalty.SettledBy,
            penalty.SettledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Miễn phạt 100% khi có phê duyệt bất khả kháng (Waived)
app.MapPost("/api/penalty-delay/{id:long}/waive", async (long id, WaivePenaltyDto? dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.WaivePenaltyAsync(id, tc.OrgId, dto?.WaiveReason, dto?.ApproverName);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.PenalizeActual,
            penalty.WaivedAmount,
            status = penalty.Status.ToString(),
            penalty.AdjustmentReason,
            penalty.ApprovedBy,
            penalty.ApprovedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Hủy hồ sơ tính phạt (Cancelled)
app.MapPost("/api/penalty-delay/{id:long}/cancel", async (long id, CancelPenaltyDto? dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    try
    {
        var penalty = await penaltyService.CancelPenaltyAsync(id, tc.OrgId, dto?.Reason);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            status = penalty.Status.ToString(),
            penalty.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Cập nhật số tiền phạt chốt thực tế hàng loạt (FrmUpdatePenaltyPmtDelayReal.btnSave_Click)
app.MapPut("/api/penalty-delay/update-actual-multi", async (UpdateActualPenaltyMultiDto dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Danh sách cập nhật phạt thực tế không được để trống." });

    try
    {
        var list = await penaltyService.UpdatePenalizeActualMultiAsync(tc.OrgId, dto.Items, dto.UpdatedBy);
        return Results.Ok(new
        {
            updatedCount = list.Count,
            items = list.Select(p => new
            {
                p.Id,
                p.PenaltyRecordNo,
                p.AmountPenaltySystem,
                p.PenalizeActual,
                p.WaivedAmount,
                status = p.Status.ToString()
            })
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Bổ sung danh sách xe vào hồ sơ phạt hiện có
app.MapPost("/api/penalty-delay/{id:long}/import-vehicles", async (long id, ImportPenaltyVehiclesDto dto, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Danh sách xe bổ sung không được để trống." });

    try
    {
        var penalty = await penaltyService.ImportVehiclesAsync(id, tc.OrgId, dto.Items);
        if (penalty == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });

        return Results.Ok(new
        {
            penalty.Id,
            penalty.PenaltyRecordNo,
            penalty.TotalApprovedQuantity,
            penalty.TotalUnitPriceActual,
            penalty.TotalDatePenalty,
            penalty.AmountPenaltySystem,
            status = penalty.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Sinh dữ liệu mẫu in Thông báo tính & quyết toán phạt chậm thanh toán xe (Penalty Settlement Advice)
app.MapGet("/api/penalty-delay/{id:long}/penalty-advice", async (long id, LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    var advice = await penaltyService.GeneratePenaltyAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ phạt #{id}." });
    return Results.Ok(advice);
});

// 13) Báo cáo dashboard tổng hợp số liệu tính phạt TTC
app.MapGet("/api/penalty-delay/summary", async (LatePaymentPenaltyService penaltyService, ITenantContext tc) =>
{
    var summary = await penaltyService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Bảng kê Thanh toán Chi phí Vận tải & Bảo hiểm Xe (Transport & Freight Insurance Payment Settlement - BizHTC.Payment / 0.34.Contract / FrmQuanLyThanhToanVanTaiBaoHiem) =====

// 1) Lập bảng kê thanh toán chi phí vận chuyển & bảo hiểm mới (Pmt_TransportIns_Save / FrmTaoThanhToanVanTaiBaoHiem)
app.MapPost("/api/transport-insurance", async (CreateTransportInsDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.PmtMonth))
        return Results.BadRequest(new { error = "Kỳ / tháng thanh toán (PmtMonth) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.TransporterCode))
        return Results.BadRequest(new { error = "Mã đơn vị vận tải (TransporterCode) không được để trống." });
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Bảng kê chi phí vận tải & bảo hiểm cần ít nhất 1 dòng xe vận chuyển." });

    try
    {
        var payment = await transService.CreatePaymentAsync(
            tc.OrgId,
            dto.TransportInsNo,
            dto.PmtMonth,
            dto.TransporterCode,
            dto.TransporterName,
            dto.InsuranceCompanyCode,
            dto.InsuranceCompanyName,
            dto.InsuranceContractNo,
            dto.VATRate ?? 10.0m,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            payment.PmtMonth,
            payment.TransporterCode,
            payment.TransporterName,
            payment.InsuranceCompanyCode,
            payment.InsuranceCompanyName,
            payment.InsuranceContractNo,
            payment.TotalVehicles,
            payment.TotalTransportCost,
            payment.TotalDelayPenalty,
            payment.TotalInsuranceCost,
            payment.TotalBeforeVAT,
            payment.VATRate,
            payment.AmountVAT,
            payment.TotalAmount,
            status = payment.Status.ToString(),
            payment.CreatedAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Danh sách tìm kiếm bảng kê thanh toán vận tải & bảo hiểm (Pmt_TransportIns_Get / FrmQuanLyThanhToanVanTaiBaoHiem)
app.MapGet("/api/transport-insurance", async (AppDbContext db, ITenantContext tc, string? pmtMonth, string? status, string? transporterCode) =>
{
    var q = db.TransportInsPayments.Where(p => p.OrgId == tc.OrgId);

    if (!string.IsNullOrWhiteSpace(pmtMonth))
        q = q.Where(p => p.PmtMonth == pmtMonth.Trim());

    if (!string.IsNullOrWhiteSpace(transporterCode))
        q = q.Where(p => p.TransporterCode == transporterCode.Trim());

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransportInsStatus>(status, true, out var st))
        q = q.Where(p => p.Status == st);

    var list = await q
        .OrderByDescending(p => p.CreatedAt)
        .Select(p => new
        {
            p.Id,
            p.TransportInsNo,
            p.PmtMonth,
            p.TransporterCode,
            p.TransporterName,
            p.InsuranceCompanyCode,
            p.InsuranceCompanyName,
            p.InsuranceContractNo,
            p.TotalVehicles,
            p.TotalTransportCost,
            p.TotalDelayPenalty,
            p.TotalInsuranceCost,
            p.TotalBeforeVAT,
            p.VATRate,
            p.AmountVAT,
            p.TotalAmount,
            status = p.Status.ToString(),
            tcmsSignStatus = p.TCMSSignStatus.ToString(),
            p.TCMSSignUser,
            p.TCMSSignDTime,
            htvSignStatus = p.HTVSignStatus.ToString(),
            p.HTVSignUser,
            p.HTVSignDTime,
            p.Appr1By,
            p.Appr1DTime,
            p.Appr2By,
            p.Appr2DTime,
            p.SettledBy,
            p.SettledAt,
            p.BankTxnRef,
            p.RejectReason,
            p.CancelledAt,
            p.FilePath,
            p.Remark,
            p.CreatedBy,
            p.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

// 3) Chi tiết 1 bảng kê vận tải & bảo hiểm kèm danh sách xe (Pmt_TransportInsDetail_Get)
app.MapGet("/api/transport-insurance/{id:long}", async (long id, AppDbContext db, ITenantContext tc) =>
{
    var payment = await db.TransportInsPayments
        .Include(p => p.Details)
        .FirstOrDefaultAsync(p => p.Id == id && p.OrgId == tc.OrgId);

    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

    return Results.Ok(new
    {
        payment.Id,
        payment.TransportInsNo,
        payment.PmtMonth,
        payment.TransporterCode,
        payment.TransporterName,
        payment.InsuranceCompanyCode,
        payment.InsuranceCompanyName,
        payment.InsuranceContractNo,
        payment.TotalVehicles,
        payment.TotalTransportCost,
        payment.TotalDelayPenalty,
        payment.TotalInsuranceCost,
        payment.TotalBeforeVAT,
        payment.VATRate,
        payment.AmountVAT,
        payment.TotalAmount,
        status = payment.Status.ToString(),
        tcmsSignStatus = payment.TCMSSignStatus.ToString(),
        payment.TCMSSignUser,
        payment.TCMSSignDTime,
        htvSignStatus = payment.HTVSignStatus.ToString(),
        payment.HTVSignUser,
        payment.HTVSignDTime,
        payment.Appr1By,
        payment.Appr1DTime,
        payment.Appr2By,
        payment.Appr2DTime,
        payment.SettledBy,
        payment.SettledAt,
        payment.BankTxnRef,
        payment.RejectReason,
        payment.CancelledAt,
        payment.FilePath,
        payment.Remark,
        payment.CreatedBy,
        payment.CreatedAt,
        details = payment.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.VIN,
            d.CarId,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.SpecDescription,
            d.ColorName,
            d.FStorageCode,
            d.FProvinceName,
            d.TStorageCode,
            d.TProvinceName,
            d.TranspReqType,
            d.DlvMnNo,
            d.DlvStartDate,
            d.ExpectedDays,
            d.ExpectedDlvEndDate,
            d.DlvEndDate,
            d.DelayDays,
            d.TFValReal,
            d.TPValReal,
            d.PriceCar,
            d.InsurancePercent,
            d.InsuranceCost,
            d.Val_Transport,
            d.StandardRemark,
            d.FProvinceRemark,
            status = d.Status.ToString(),
            d.Remark
        })
    });
});

// 4) TCMS Thẩm định duyệt cấp 1 (Pmt_TransportIns_Approve1)
app.MapPost("/api/transport-insurance/{id:long}/approve-tcms", async (long id, ApproveTransportInsDto? dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.ApproveStep1TCMSAsync(id, tc.OrgId, dto?.ApproverName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            status = payment.Status.ToString(),
            payment.Appr1By,
            payment.Appr1DTime
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) HTV Lãnh đạo duyệt cấp 2 (Pmt_TransportIns_Approve2)
app.MapPost("/api/transport-insurance/{id:long}/approve-htv", async (long id, ApproveTransportInsDto? dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.ApproveStep2HTVAsync(id, tc.OrgId, dto?.ApproverName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            status = payment.Status.ToString(),
            payment.Appr2By,
            payment.Appr2DTime
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ký số điện tử CA đại diện TCMS (Pmt_TransportIns_TCMSApproveAndSign)
app.MapPost("/api/transport-insurance/{id:long}/sign-tcms", async (long id, SignTransportInsDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.SignTCMSAsync(id, tc.OrgId, dto.SignerName, dto.FilePath);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            tcmsSignStatus = payment.TCMSSignStatus.ToString(),
            payment.TCMSSignUser,
            payment.TCMSSignDTime,
            status = payment.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ký số điện tử CA đại diện HTV (Pmt_TransportIns_HTVApproveAndSign)
app.MapPost("/api/transport-insurance/{id:long}/sign-htv", async (long id, SignTransportInsDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.SignHTVAsync(id, tc.OrgId, dto.SignerName, dto.FilePath);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            htvSignStatus = payment.HTVSignStatus.ToString(),
            payment.HTVSignUser,
            payment.HTVSignDTime,
            status = payment.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Quyết toán chi trả cước vận tải & bảo hiểm qua UNC ngân hàng (Settled / Paid)
app.MapPost("/api/transport-insurance/{id:long}/settle", async (long id, SettleTransportInsDto? dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.SettlePaymentAsync(id, tc.OrgId, dto?.BankTxnRef, dto?.SettlerName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            status = payment.Status.ToString(),
            payment.BankTxnRef,
            payment.SettledBy,
            payment.SettledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Từ chối bảng kê kèm lý do (Pmt_TransportIns_Cancel / btnDeny_Click)
app.MapPost("/api/transport-insurance/{id:long}/reject", async (long id, RejectTransportInsDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.RejectPaymentAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            status = payment.Status.ToString(),
            payment.RejectReason,
            payment.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy bảng kê vận tải & bảo hiểm (Pmt_TransportIns_Cancel / btnDelete_Click)
app.MapPost("/api/transport-insurance/{id:long}/cancel", async (long id, CancelTransportInsDto? dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.CancelPaymentAsync(id, tc.OrgId, dto?.Reason);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            status = payment.Status.ToString(),
            payment.CancelledAt
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Cập nhật điều chỉnh chi phí vận tải & phạt chậm hàng loạt (Pmt_TransportIns_UpdateMulti)
app.MapPut("/api/transport-insurance/{id:long}/details", async (long id, UpdateTransportDetailsDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Danh sách cập nhật chi phí (Items) không được để trống." });

    try
    {
        var payment = await transService.UpdateDetailsAsync(id, tc.OrgId, dto.Items);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            payment.TotalVehicles,
            payment.TotalTransportCost,
            payment.TotalDelayPenalty,
            payment.TotalInsuranceCost,
            payment.TotalBeforeVAT,
            payment.AmountVAT,
            payment.TotalAmount,
            status = payment.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Bổ sung xe vào bảng kê hiện có (FrmTaoThanhToanVanTaiBaoHiem_AddCar)
app.MapPost("/api/transport-insurance/{id:long}/import-vehicles", async (long id, ImportTransportVehiclesDto dto, TransportInsPaymentService transService, ITenantContext tc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Danh sách xe bổ sung không được để trống." });

    try
    {
        var payment = await transService.ImportVehiclesAsync(id, tc.OrgId, dto.Items);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            payment.TotalVehicles,
            payment.TotalTransportCost,
            payment.TotalDelayPenalty,
            payment.TotalInsuranceCost,
            payment.TotalBeforeVAT,
            payment.AmountVAT,
            payment.TotalAmount,
            status = payment.Status.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Xóa 1 xe khỏi bảng kê vận tải & bảo hiểm
app.MapDelete("/api/transport-insurance/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, TransportInsPaymentService transService, ITenantContext tc) =>
{
    try
    {
        var payment = await transService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

        return Results.Ok(new
        {
            payment.Id,
            payment.TransportInsNo,
            payment.TotalVehicles,
            payment.TotalTransportCost,
            payment.TotalDelayPenalty,
            payment.TotalInsuranceCost,
            payment.TotalAmount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 14) Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí vận chuyển & bảo hiểm xe (CR_Pmt_TransportIns Advice)
app.MapGet("/api/transport-insurance/{id:long}/statement-advice", async (long id, TransportInsPaymentService transService, ITenantContext tc) =>
{
    var advice = await transService.GenerateStatementAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
    return Results.Ok(advice);
});

// 15) Báo cáo dashboard tổng hợp số liệu thanh toán vận tải & bảo hiểm
app.MapGet("/api/transport-insurance/summary", async (TransportInsPaymentService transService, ITenantContext tc) =>
{
    var summary = await transService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// =========================================================================
// QUẢN LÝ BẢNG KÊ THANH TOÁN CHI PHÍ LƯU KHO XE (BizHTC.Payment / 0.34.Contract)
// Tương ứng Pmt_PaymentStorage, Pmt_PaymentStorageDetail, FrmQuanLyThanhToanLuuKho, FrmSuaThanhToanLuuKho & CR_PAYMENT_STORAGE
// =========================================================================

// 1) Lập bảng kê chi phí lưu kho xe mới (Job_Pmt_PaymentStorage_Create)
app.MapPost("/api/payment-storage", async (CreatePaymentStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.CreatePaymentAsync(
            tc.OrgId,
            dto.PaymentStorageNo,
            dto.PmtMonth,
            dto.StorageOperatorCode,
            dto.StorageOperatorName,
            dto.VATRate ?? 10.0m,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Created($"/api/payment-storage/{payment.Id}", new
        {
            payment.Id,
            payment.PaymentStorageNo,
            payment.PmtMonth,
            payment.StorageOperatorCode,
            payment.StorageOperatorName,
            payment.TotalVehicles,
            payment.TotalCoatCost,
            payment.TotalStorageCost,
            payment.TotalAmount,
            payment.VATRate,
            payment.UnitPriceVAT,
            payment.AmountTotal,
            Status = payment.Status.ToString(),
            payment.CreatedAt
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

// 2) Danh sách tìm kiếm bảng kê thanh toán lưu kho (Pmt_PaymentStorage_Get)
app.MapGet("/api/payment-storage", async (string? pmtMonth, string? status, string? operatorCode, string? vin, PaymentStorageService storageService, ITenantContext tc) =>
{
    var list = await storageService.GetListAsync(tc.OrgId, pmtMonth, status, operatorCode, vin);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PaymentStorageNo,
        p.PmtMonth,
        p.StorageOperatorCode,
        p.StorageOperatorName,
        p.TotalVehicles,
        p.TotalCoatCost,
        p.TotalStorageCost,
        p.TotalAmount,
        p.VATRate,
        p.UnitPriceVAT,
        p.AmountTotal,
        Status = p.Status.ToString(),
        TCMSSignStatus = p.TCMSSignStatus.ToString(),
        p.TCMSSignUser,
        p.TCMSSignDTime,
        HTVSignStatus = p.HTVSignStatus.ToString(),
        p.HTVSignUser,
        p.HTVSignDTime,
        p.Appr1By,
        p.Appr1DTime,
        p.Appr2By,
        p.Appr2DTime,
        p.SettledBy,
        p.SettledAt,
        p.BankTxnRef,
        p.FilePath,
        p.Remark,
        p.CreatedBy,
        p.CreatedAt
    }));
});

// 3) Chi tiết 1 bảng kê kèm danh sách xe lưu kho (Pmt_PaymentStorageDetail_Get)
app.MapGet("/api/payment-storage/{id:long}", async (long id, PaymentStorageService storageService, ITenantContext tc) =>
{
    var payment = await storageService.GetByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });

    return Results.Ok(new
    {
        payment.Id,
        payment.PaymentStorageNo,
        payment.PmtMonth,
        payment.StorageOperatorCode,
        payment.StorageOperatorName,
        payment.TotalVehicles,
        payment.TotalCoatCost,
        payment.TotalStorageCost,
        payment.TotalAmount,
        payment.VATRate,
        payment.UnitPriceVAT,
        payment.AmountTotal,
        Status = payment.Status.ToString(),
        TCMSSignStatus = payment.TCMSSignStatus.ToString(),
        payment.TCMSSignUser,
        payment.TCMSSignDTime,
        HTVSignStatus = payment.HTVSignStatus.ToString(),
        payment.HTVSignUser,
        payment.HTVSignDTime,
        payment.Appr1By,
        payment.Appr1DTime,
        payment.Appr2By,
        payment.Appr2DTime,
        payment.SettledBy,
        payment.SettledAt,
        payment.BankTxnRef,
        payment.FilePath,
        payment.Remark,
        payment.RejectReason,
        payment.CancelledAt,
        payment.CreatedBy,
        payment.CreatedAt,
        Details = payment.Details.Select(d => new
        {
            d.Id,
            d.PaymentStorageId,
            d.PaymentStorageNo,
            d.VIN,
            d.CarId,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.SpecDescription,
            d.ColorExtNameVN,
            d.StorageCodeInit,
            d.StorageDate,
            d.ApprovedDate2,
            d.DeliveryOutDate,
            d.DealerCode,
            d.DealerName,
            d.InCostStorageDate,
            d.OutCostStorageDate,
            d.CostStorageMonth,
            d.LevelStorage,
            d.DailyStorageRate,
            d.CostCoat,
            d.CostStorage,
            d.TotalAmount,
            Status = d.Status.ToString(),
            d.Remark
        })
    });
});

// 4) HTV Duyệt sơ bộ cấp 1 (Pmt_PaymentStorage_Approve1)
app.MapPost("/api/payment-storage/{id:long}/approve-htv", async (long id, ApproveStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.Approve1HTVAsync(id, tc.OrgId, dto.ApproverName);
        return Results.Ok(new { message = $"HTV duyệt sơ bộ cấp 1 bảng kê #{id} thành công.", Status = payment.Status.ToString(), payment.Appr1By, payment.Appr1DTime });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) TCMS Thẩm định duyệt cấp 2 (Pmt_PaymentStorage_Approve2)
app.MapPost("/api/payment-storage/{id:long}/approve-tcms", async (long id, ApproveStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.Approve2TCMSAsync(id, tc.OrgId, dto.ApproverName);
        return Results.Ok(new { message = $"TCMS duyệt cấp 2 bảng kê #{id} thành công.", Status = payment.Status.ToString(), payment.Appr2By, payment.Appr2DTime });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ký số điện tử CA đại diện TCMS (Pmt_PaymentStorage_TCMSApproveAndSign)
app.MapPost("/api/payment-storage/{id:long}/sign-tcms", async (long id, SignStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.SignTCMSAsync(id, tc.OrgId, dto.SignerName, dto.FilePath);
        return Results.Ok(new { message = $"TCMS đã ký số điện tử bảng kê #{id} thành công.", TCMSSignStatus = payment.TCMSSignStatus.ToString(), payment.TCMSSignUser, payment.TCMSSignDTime, payment.FilePath });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ký số điện tử CA đại diện HTV (Pmt_PaymentStorage_HTVApproveAndSign)
app.MapPost("/api/payment-storage/{id:long}/sign-htv", async (long id, SignStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.SignHTVAsync(id, tc.OrgId, dto.SignerName, dto.FilePath);
        return Results.Ok(new { message = $"HTV đã ký số hoàn tất bảng kê #{id}.", Status = payment.Status.ToString(), HTVSignStatus = payment.HTVSignStatus.ToString(), payment.HTVSignUser, payment.HTVSignDTime, payment.FilePath });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Quyết toán chi trả qua UNC ngân hàng (Settled / Paid)
app.MapPost("/api/payment-storage/{id:long}/settle", async (long id, SettleStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.SettlePaymentAsync(id, tc.OrgId, dto.BankTxnRef, dto.SettlerName);
        return Results.Ok(new { message = $"Đã quyết toán chi trả thành công bảng kê #{id} qua UNC ngân hàng.", Status = payment.Status.ToString(), payment.SettledBy, payment.SettledAt, payment.BankTxnRef });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Từ chối bảng kê kèm lý do (Pmt_PaymentStorage_Cancel / Deny)
app.MapPost("/api/payment-storage/{id:long}/reject", async (long id, RejectStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.RejectAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        return Results.Ok(new { message = $"Đã từ chối bảng kê #{id}.", Status = payment.Status.ToString(), payment.RejectReason });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy bảng kê (Pmt_PaymentStorage_Cancel)
app.MapPost("/api/payment-storage/{id:long}/cancel", async (long id, CancelStorageDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.CancelAsync(id, tc.OrgId, dto.Reason);
        return Results.Ok(new { message = $"Đã hủy bảng kê #{id}.", Status = payment.Status.ToString(), payment.RejectReason });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Cập nhật điều chỉnh chi phí bạt & lưu kho xe hàng loạt (Pmt_PaymentStorage_UpdateMulti / FrmSuaThanhToanLuuKho)
app.MapPut("/api/payment-storage/{id:long}/details", async (long id, UpdateStorageDetailsDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.UpdateDetailsAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(new
        {
            message = $"Cập nhật điều chỉnh chi phí xe bảng kê #{id} thành công.",
            payment.TotalCoatCost,
            payment.TotalStorageCost,
            payment.TotalAmount,
            payment.UnitPriceVAT,
            payment.AmountTotal
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Bổ sung xe vào bảng kê hiện có
app.MapPost("/api/payment-storage/{id:long}/import-vehicles", async (long id, ImportStorageVehiclesDto dto, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.ImportVehiclesAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(new
        {
            message = $"Bổ sung {dto.Items.Count} xe vào bảng kê #{id} thành công.",
            payment.TotalVehicles,
            payment.TotalCoatCost,
            payment.TotalStorageCost,
            payment.TotalAmount,
            payment.UnitPriceVAT,
            payment.AmountTotal
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Xóa 1 xe khỏi bảng kê
app.MapDelete("/api/payment-storage/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        var payment = await storageService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(new
        {
            message = $"Đã xóa xe #{detailId} khỏi bảng kê #{id}.",
            payment.TotalVehicles,
            payment.TotalCoatCost,
            payment.TotalStorageCost,
            payment.TotalAmount,
            payment.UnitPriceVAT,
            payment.AmountTotal
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 14) Xóa hẳn bảng kê nháp/hủy (Pmt_PaymentStorage_Delete)
app.MapDelete("/api/payment-storage/{id:long}", async (long id, PaymentStorageService storageService, ITenantContext tc) =>
{
    try
    {
        await storageService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa bảng kê lưu kho #{id} thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 15) Sinh dữ liệu mẫu in Bảng kê quyết toán chi phí lưu kho xe (CR_PAYMENT_STORAGE Advice)
app.MapGet("/api/payment-storage/{id:long}/statement-advice", async (long id, PaymentStorageService storageService, ITenantContext tc) =>
{
    var advice = await storageService.GenerateStatementAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
    return Results.Ok(advice);
});

// 16) Báo cáo dashboard tổng hợp số liệu lưu kho
app.MapGet("/api/payment-storage/summary", async (PaymentStorageService storageService, ITenantContext tc) =>
{
    var summary = await storageService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ==========================================
// QUẢN LÝ CÔNG VĂN GIA HẠN THƯ BẢO LÃNH THANH TOÁN NGÂN HÀNG (Bank Guarantee Extension Dispatch - Pmt_GrtClaimExt / FrmQLCVanGiaHan_PhatHanhBL)
// ==========================================

// 1) Lập công văn đề nghị gia hạn bảo lãnh mới (Pmt_GrtClaimExt_Save / FrmTaoCVanGiaHan_PhatHanhBL)
app.MapPost("/api/guarantee-extensions", async (CreateGuaranteeExtensionDto dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.CreateDispatchAsync(
            tc.OrgId,
            dto.DispatchNo,
            dto.DealerCode,
            dto.DealerName,
            dto.BankCode,
            dto.BankName,
            dto.BankCodeMonitor,
            dto.FlagIsHTC ?? "1",
            dto.NumberOfDaysExt > 0 ? dto.NumberOfDaysExt : 30,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Created($"/api/guarantee-extensions/{dispatch.Id}", dispatch);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

// 2) Lấy danh sách công văn gia hạn kèm bộ lọc
app.MapGet("/api/guarantee-extensions", async (
    string? dealerCode,
    string? bankCode,
    string? status,
    string? flagIsHTC,
    string? vin,
    GuaranteeExtensionService extService,
    ITenantContext tc) =>
{
    var list = await extService.GetDispatchesAsync(tc.OrgId, dealerCode, bankCode, status, flagIsHTC, vin);
    return Results.Ok(list);
});

// 3) Lấy chi tiết 1 công văn gia hạn
app.MapGet("/api/guarantee-extensions/{id:long}", async (long id, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    var dispatch = await extService.GetDispatchByIdAsync(id, tc.OrgId);
    if (dispatch == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
    return Results.Ok(dispatch);
});

// 4) Tự động quét các bảo lãnh sắp đến hạn và sinh công văn đề nghị gia hạn (Pmt_GrtClaimExt_GenAuto_New20201210)
app.MapPost("/api/guarantee-extensions/auto-generate", async (AutoGenerateExtensionDto? dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    var withinDays = dto?.WithinDays > 0 ? dto.WithinDays : 15;
    var extDays = dto?.NumberOfDaysExt > 0 ? dto.NumberOfDaysExt : 30;
    var flagIsHTC = dto?.FlagIsHTC ?? "1";
    var remark = dto?.Remark ?? "Hệ thống tự động quét bảo lãnh sắp hết hạn sinh công văn";
    var createdBy = dto?.CreatedBy ?? "AutoScheduleService";

    var dispatches = await extService.AutoGenerateDispatchesAsync(tc.OrgId, withinDays, extDays, flagIsHTC, remark, createdBy);
    return Results.Ok(new
    {
        message = $"Đã quét và tự động sinh {dispatches.Count} công văn gia hạn bảo lãnh.",
        count = dispatches.Count,
        dispatches
    });
});

// 5) Ký số điện tử CA phát hành công văn (Pmt_GrtClaimExt_SignAndSendEmail / FrmQLCVanGiaHan_PhatHanhBL.btnKyDienTu)
app.MapPost("/api/guarantee-extensions/{id:long}/sign-ca", async (long id, SignExtensionCADto? dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.SignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        if (dispatch == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(dispatch);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ghi nhận ngân hàng đối tác chấp thuận gia hạn bảo lãnh
app.MapPost("/api/guarantee-extensions/{id:long}/bank-accept", async (long id, BankAcceptExtensionDto dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.BankAcceptAsync(id, tc.OrgId, dto.BankResponseRef, dto.Note);
        if (dispatch == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(dispatch);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ghi nhận ngân hàng từ chối gia hạn bảo lãnh
app.MapPost("/api/guarantee-extensions/{id:long}/bank-reject", async (long id, BankRejectExtensionDto dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.BankRejectAsync(id, tc.OrgId, dto.Reason);
        if (dispatch == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(dispatch);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Hủy công văn gia hạn (Pmt_GrtClaimExt_Cancel / FrmQLCVanGiaHan_PhatHanhBL.btnDelete)
app.MapPost("/api/guarantee-extensions/{id:long}/cancel", async (long id, CancelExtensionDto? dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.CancelDispatchAsync(id, tc.OrgId, dto?.Reason);
        if (dispatch == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(dispatch);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Bổ sung xe vào công văn Draft
app.MapPost("/api/guarantee-extensions/{id:long}/vehicles", async (long id, ImportExtensionVehiclesDto dto, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.AddVehiclesAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(dispatch);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Xóa 1 xe khỏi công văn Draft
app.MapDelete("/api/guarantee-extensions/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        var dispatch = await extService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(dispatch);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Xóa bản ghi công văn nháp
app.MapDelete("/api/guarantee-extensions/{id:long}", async (long id, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    try
    {
        await extService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa công văn #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Sinh công văn mẫu in gửi ngân hàng (CR_ClaimPM Advice)
app.MapGet("/api/guarantee-extensions/{id:long}/advice", async (long id, GuaranteeExtensionService extService, ITenantContext tc) =>
{
    var advice = await extService.GenerateDispatchAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
    return Results.Ok(advice);
});

// 13) Báo cáo dashboard tổng hợp công văn gia hạn bảo lãnh
app.MapGet("/api/guarantee-extensions/summary", async (GuaranteeExtensionService extService, ITenantContext tc) =>
{
    var summary = await extService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản Lý Hồ Sơ & Công Văn Đòi Tiền Bảo Lãnh Ngân Hàng (Bank Guarantee Default Claim / Pmt_GrtClaim) =====

// 1) Lấy danh sách hồ sơ công văn đòi bảo lãnh (Pmt_GrtClaimGet)
app.MapGet("/api/guarantee-claims", async (
    string? dealerCode,
    string? bankCode,
    string? status,
    string? flagIsHTC,
    string? vin,
    BankGuaranteeClaimService claimService,
    ITenantContext tc) =>
{
    var list = await claimService.GetClaimsAsync(tc.OrgId, dealerCode, bankCode, status, flagIsHTC, vin);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.ClaimNo,
        c.DealerCode,
        c.DealerName,
        c.BankCode,
        c.BankName,
        c.BankCodeMonitor,
        c.FlagIsHTC,
        c.TotalCarCount,
        c.TotalClaimAmount,
        c.SettledAmount,
        status = c.Status.ToString(),
        signCAStatus = c.SignCAStatus.ToString(),
        c.SignedBy,
        c.SignedAt,
        c.SentToBankAt,
        c.BankRefNo,
        c.SettledAt,
        c.SettledBy,
        c.BankTxnRef,
        c.BankRejectReason,
        c.CancelledAt,
        c.CancelReason,
        c.Remark,
        c.CreatedBy,
        c.CreatedAt
    }));
});

// 2) Báo cáo dashboard tổng hợp công văn đòi bảo lãnh
app.MapGet("/api/guarantee-claims/summary", async (BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    var summary = await claimService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Chi tiết 1 công văn đòi bảo lãnh kèm danh mục xe ô tô (Pmt_GrtClaimDetail)
app.MapGet("/api/guarantee-claims/{id:long}", async (long id, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    var claim = await claimService.GetClaimByIdAsync(id, tc.OrgId);
    if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn đòi bảo lãnh #{id}." });
    return Results.Ok(claim);
});

// 4) Sinh công văn mẫu in gửi ngân hàng kèm đọc số thành chữ tiếng Việt (CR_ClaimPM Advice)
app.MapGet("/api/guarantee-claims/{id:long}/advice", async (long id, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    var advice = await claimService.GenerateClaimAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
    return Results.Ok(advice);
});

// 5) Tự động quét bảo lãnh quá hạn đề xuất lập công văn (Auto-Scan)
app.MapPost("/api/guarantee-claims/auto-scan", async (AutoScanClaimRequestDto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    var minDays = dto?.MinOverdueDays ?? 1;
    var candidates = await claimService.AutoScanOverdueGuaranteesAsync(tc.OrgId, minDays);
    return Results.Ok(candidates);
});

// 6) Lập công văn đòi bảo lãnh mới (Pmt_GrtClaimCreate_Multi_New20190312 / FrmNewGrtClaim)
app.MapPost("/api/guarantee-claims", async (CreateGuaranteeClaimDto dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.CreateClaimAsync(
            tc.OrgId,
            dto.ClaimNo,
            dto.DealerCode,
            dto.DealerName,
            dto.BankCode,
            dto.BankName,
            dto.BankCodeMonitor,
            dto.FlagIsHTC ?? "1",
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Ok(claim);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Trình Ban Pháp chế & Rủi ro tín dụng thẩm định (Draft -> Submitted)
app.MapPost("/api/guarantee-claims/{id:long}/submit", async (long id, SubmitClaimDto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.SubmitForReviewAsync(id, tc.OrgId, dto?.SubmittedBy);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Ký số điện tử CA phát hành công văn (Submitted -> SignedCA)
app.MapPost("/api/guarantee-claims/{id:long}/sign-ca", async (long id, SignClaimCADto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.SignAndIssueCAAsync(id, tc.OrgId, dto?.SignedBy, dto?.CertThumbprint, dto?.FilePath);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Gửi công văn đòi nợ tới Ngân hàng bảo lãnh (SignedCA -> SentToBank)
app.MapPost("/api/guarantee-claims/{id:long}/send-to-bank", async (long id, SendClaimToBankDto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.SendToBankAsync(id, tc.OrgId, dto?.SentBy, dto?.BankRefNo);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Ngân hàng giải ngân bồi hoàn tất toán thành công (SentToBank -> Settled)
app.MapPost("/api/guarantee-claims/{id:long}/settle", async (long id, SettleClaimDto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var settledAmount = dto?.SettledAmount ?? 0;
        var claim = await claimService.SettleClaimAsync(id, tc.OrgId, settledAmount, dto?.BankTxnRef, dto?.SettlerName);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Ngân hàng từ chối chi trả bồi hoàn bảo lãnh (SentToBank -> BankRejected)
app.MapPost("/api/guarantee-claims/{id:long}/bank-reject", async (long id, BankRejectClaimDto dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.BankRejectAsync(id, tc.OrgId, dto.Reason, dto.RejectedBy);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Hủy công văn đòi nợ (Pmt_GrtClaim_Cancel / FrmNewGrtClaim.btnCancel)
app.MapPost("/api/guarantee-claims/{id:long}/cancel", async (long id, CancelClaimDto? dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.CancelClaimAsync(id, tc.OrgId, dto?.CancelReason, dto?.CancelledBy);
        if (claim == null) return Results.NotFound(new { error = $"Không tìm thấy công văn #{id}." });
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Bổ sung xe vào công văn nháp / đang trình duyệt (FrmNewGrtClaim.btnAddCar)
app.MapPost("/api/guarantee-claims/{id:long}/vehicles", async (long id, AddClaimVehicleDto dto, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.AddVehicleAsync(id, tc.OrgId, dto.Vehicle);
        return Results.Ok(claim);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 14) Xóa xe khỏi công văn nháp (FrmNewGrtClaim.btnRemoveCar)
app.MapDelete("/api/guarantee-claims/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        var claim = await claimService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(claim);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 15) Xóa công văn nháp (GrtClaimDelete_New20181115)
app.MapDelete("/api/guarantee-claims/{id:long}", async (long id, BankGuaranteeClaimService claimService, ITenantContext tc) =>
{
    try
    {
        await claimService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa công văn đòi nợ bảo lãnh #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
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
record CreatePaymentOrderDto(
    string? PaymentNo,
    PaymentOrderType? PaymentType,
    string? BankPaymentNo,
    DateTime? PaymentEndDate,
    string PartnerCode,
    string? PartnerName,
    string BankCodeSend,
    string? BankNameSend,
    string BankAccountSend,
    string BankCodeReceive,
    string? BankNameReceive,
    string BankAccountReceive,
    PaymentFundType? Funds,
    string? BankLending,
    decimal? InterestRate,
    int? LoanPeriodMonths,
    string? AccountingRecordNo,
    string? Remark,
    List<PaymentOrderItemInputDto> Items
);
record ApprovePaymentOrderDto(string? ApproverName);
record FinishPaymentOrderDto(DateTime? PaymentEndDate, string? BankPaymentNo, string? FinisherName);
record RejectPaymentOrderDto(string Reason, string? RejecterName);
record CancelPaymentOrderDto(string? Reason);
record UpdatePaymentRatesDto(List<long> PaymentIds, decimal InterestRate, int LoanPeriodMonths);
record CreateBankBillDto(
    string? BankBillMnNo,
    string BankCode,
    string? BankName,
    string PartnerCode,
    string? PartnerName,
    DateTime? BankBillDate,
    DateTime? BankBillPrintDate,
    string? Remark,
    List<BankBillItemInputDto> Items
);
record HandoverBankBillDto(string? HandedOverBy, string? Note);
record BankReceiveDto(DateTime? ReceiveDate, string? ReceivedBy, string? Note);
record SettleBankBillDto(string? SettlerName, string? Note);
record CancelBankBillDto(string? Reason);
record ImportVinsDto(List<BankBillItemInputDto> Items);
record CreatePaymentPDIDto(
    string? PmtPDINo,
    string? PmtMonth,
    string? ServiceUnitCode,
    string? ServiceUnitName,
    decimal? VATRate,
    string? Remark,
    List<PaymentPDIItemInputDto>? Items
);
record SignPDIDto(string? SignerName, string? FilePath);
record SettlePDIDto(string? BankTxnRef, string? PayerName);
record RejectPDIDto(string? Reason, string? RejecterName);
record CancelPDIDto(string? Reason);
record UpdatePDIDetailsDto(List<UpdatePDIDetailItemDto>? Items);
record CreateLatePaymentPenaltyDto(
    string? PenaltyRecordNo,
    string? SOCode,
    string? DealerCode,
    string? DealerName,
    string? ContractNo,
    DateTime? SOApprovedDate,
    decimal? PenaltyRateAnnual,
    string? Remark,
    string? CreatedBy,
    List<PenaltyItemInputDto>? Items
);
record ReviewPenaltyDto(long ProposedPenalizeActual, string? AdjustmentReason, string? ReviewerName);
record ApprovePenaltyDto(string? ApproverName);
record SettlePenaltyDto(string? PaymentProofRef, string? SettlerName);
record WaivePenaltyDto(string? WaiveReason, string? ApproverName);
record CancelPenaltyDto(string? Reason);
record UpdateActualPenaltyMultiDto(List<UpdateActualPenaltyItemDto>? Items, string? UpdatedBy);
record ImportPenaltyVehiclesDto(List<PenaltyItemInputDto>? Items);
record CreateTransportInsDto(
    string? TransportInsNo,
    string PmtMonth,
    string TransporterCode,
    string? TransporterName,
    string? InsuranceCompanyCode,
    string? InsuranceCompanyName,
    string? InsuranceContractNo,
    decimal? VATRate,
    string? Remark,
    string? CreatedBy,
    List<TransportInsItemInputDto> Items
);
record ApproveTransportInsDto(string? ApproverName);
record SignTransportInsDto(string? SignerName, string? FilePath);
record SettleTransportInsDto(string? BankTxnRef, string? SettlerName);
record RejectTransportInsDto(string Reason, string? RejecterName);
record CancelTransportInsDto(string? Reason);
record UpdateTransportDetailsDto(List<UpdateTransportDetailItemDto> Items);
record ImportTransportVehiclesDto(List<TransportInsItemInputDto> Items);

record CreatePaymentStorageDto(
    string? PaymentStorageNo,
    string PmtMonth,
    string? StorageOperatorCode,
    string? StorageOperatorName,
    decimal? VATRate,
    string? Remark,
    string? CreatedBy,
    List<PaymentStorageItemInputDto> Items
);
record ApproveStorageDto(string? ApproverName);
record SignStorageDto(string? SignerName, string? FilePath);
record SettleStorageDto(string? BankTxnRef, string? SettlerName);
record RejectStorageDto(string Reason, string? RejecterName);
record CancelStorageDto(string? Reason);
record UpdateStorageDetailsDto(List<UpdateStorageDetailItemDto> Items);
record ImportStorageVehiclesDto(List<PaymentStorageItemInputDto> Items);

record CreateGuaranteeExtensionDto(
    string? DispatchNo,
    string DealerCode,
    string? DealerName,
    string BankCode,
    string? BankName,
    string? BankCodeMonitor,
    string? FlagIsHTC,
    int NumberOfDaysExt,
    string? Remark,
    string? CreatedBy,
    List<GuaranteeExtensionItemInputDto> Items
);
record AutoGenerateExtensionDto(int WithinDays, int NumberOfDaysExt, string? FlagIsHTC, string? Remark, string? CreatedBy);
record SignExtensionCADto(string? SignerName, string? FilePath);
record BankAcceptExtensionDto(string BankResponseRef, string? Note);
record BankRejectExtensionDto(string Reason);
record CancelExtensionDto(string? Reason);
record ImportExtensionVehiclesDto(List<GuaranteeExtensionItemInputDto> Items);

record CreateGuaranteeClaimDto(
    string? ClaimNo,
    string DealerCode,
    string? DealerName,
    string BankCode,
    string? BankName,
    string? BankCodeMonitor,
    string? FlagIsHTC,
    string? Remark,
    string? CreatedBy,
    List<GuaranteeClaimItemInputDto> Items
);
record AutoScanClaimRequestDto(int? MinOverdueDays);
record SubmitClaimDto(string? SubmittedBy);
record SignClaimCADto(string? SignedBy, string? CertThumbprint, string? FilePath);
record SendClaimToBankDto(string? SentBy, string? BankRefNo);
record SettleClaimDto(long? SettledAmount, string? BankTxnRef, string? SettlerName);
record BankRejectClaimDto(string Reason, string? RejectedBy);
record CancelClaimDto(string? CancelReason, string? CancelledBy);
record AddClaimVehicleDto(GuaranteeClaimItemInputDto Vehicle);

