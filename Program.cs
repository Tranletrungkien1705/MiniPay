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
builder.Services.AddScoped<PaymentAVNService>();
builder.Services.AddScoped<PaymentGPSService>();
builder.Services.AddScoped<FinancialExpenseService>();
builder.Services.AddScoped<BankingDisbursementService>();
builder.Services.AddScoped<CancelBankMDService>();
builder.Services.AddScoped<InsurancePaymentService>();
builder.Services.AddScoped<SupplierPaymentService>();
builder.Services.AddScoped<CustomerPaymentService>();
builder.Services.AddScoped<ContractCancellationService>();
builder.Services.AddScoped<CarDocReqService>();
builder.Services.AddScoped<HTCInvoiceService>();
builder.Services.AddScoped<LetterOfCreditService>();
builder.Services.AddScoped<BankDealerService>();
builder.Services.AddScoped<AccountingVoucherService>();
builder.Services.AddScoped<BankStatementAutoApproveService>();
builder.Services.AddScoped<PaymentCalendarService>();
builder.Services.AddScoped<DealerContractService>();
builder.Services.AddScoped<StorageRearrangeService>();
builder.Services.AddScoped<TransportFeeVersionService>();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

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

// ===== Quản Lý Bảng Kê Thanh Toán Thiết Bị AVN Trên Xe Ô Tô (Pmt_PaymentAVN / FrmQuanLyThanhToanAVN) =====

// 1) Lấy danh sách bảng kê thanh toán AVN
app.MapGet("/api/payment-avn", async (
    string? pmtMonth,
    string? status,
    string? supplierCode,
    string? vin,
    PaymentAVNService avnService,
    ITenantContext tc) =>
{
    var list = await avnService.GetListAsync(tc.OrgId, pmtMonth, status, supplierCode, vin);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PaymentAVNNo,
        p.PmtMonth,
        p.SupplierCode,
        p.SupplierName,
        p.TotalVehicles,
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
    }));
});

// 2) Lấy chi tiết 1 bảng kê kèm danh sách xe
app.MapGet("/api/payment-avn/{id:long}", async (long id, PaymentAVNService avnService, ITenantContext tc) =>
{
    var payment = await avnService.GetByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê AVN #{id}." });
    return Results.Ok(payment);
});

// 3) Lập bảng kê thanh toán AVN mới
app.MapPost("/api/payment-avn", async (CreatePaymentAVNDto dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.CreatePaymentAsync(
            tc.OrgId,
            dto.PaymentAVNNo,
            dto.PmtMonth,
            dto.SupplierCode,
            dto.SupplierName,
            dto.VATRate,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Ok(payment);
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

// 4) Duyệt thẩm định cấp 1 TCMS (A1)
app.MapPost("/api/payment-avn/{id:long}/tcms-approve", async (long id, ApprovePaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.TCMSApproveAsync(id, tc.OrgId, dto?.ApproverName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Duyệt thẩm định cấp 2 HTV (A2)
app.MapPost("/api/payment-avn/{id:long}/htv-approve", async (long id, ApprovePaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.HTVApproveAsync(id, tc.OrgId, dto?.ApproverName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ký số điện tử CA TCMS
app.MapPost("/api/payment-avn/{id:long}/tcms-sign", async (long id, SignPaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.TCMSSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ký số điện tử CA HTV hoàn tất (Signed)
app.MapPost("/api/payment-avn/{id:long}/htv-sign", async (long id, SignPaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.HTVSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Kế toán quyết toán chi trả chuyển khoản UNC ngân hàng (Settled)
app.MapPost("/api/payment-avn/{id:long}/settle", async (long id, SettlePaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.SettlePaymentAsync(id, tc.OrgId, dto?.BankTxnRef, dto?.SettledBy);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Từ chối bảng kê
app.MapPost("/api/payment-avn/{id:long}/reject", async (long id, RejectPaymentAVNDto dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.RejectPaymentAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy bảng kê
app.MapPost("/api/payment-avn/{id:long}/cancel", async (long id, CancelPaymentAVNDto? dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.CancelPaymentAsync(id, tc.OrgId, dto?.Reason);
        if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Bổ sung xe vào bảng kê nháp
app.MapPost("/api/payment-avn/{id:long}/vehicles", async (long id, ImportPaymentAVNVehiclesDto dto, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.AddVehiclesAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(payment);
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

// 12) Xóa xe khỏi bảng kê nháp
app.MapDelete("/api/payment-avn/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        var payment = await avnService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(payment);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Xóa toàn bộ bảng kê nháp
app.MapDelete("/api/payment-avn/{id:long}", async (long id, PaymentAVNService avnService, ITenantContext tc) =>
{
    try
    {
        await avnService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa bảng kê AVN #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 14) Sinh dữ liệu mẫu in Bảng kê quyết toán AVN (CR_PAYMENT_AVN Advice)
app.MapGet("/api/payment-avn/{id:long}/advice", async (long id, PaymentAVNService avnService, ITenantContext tc) =>
{
    var advice = await avnService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
    return Results.Ok(advice);
});

// 15) Thống kê tổng hợp dashboard
app.MapGet("/api/payment-avn/summary", async (PaymentAVNService avnService, ITenantContext tc) =>
{
    var summary = await avnService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 16) Bảng giá tham chiếu AVN theo model xe
app.MapGet("/api/payment-avn/prices", () =>
{
    var prices = PaymentAVNService.DefaultAVNPrices.Select(kv => new
    {
        modelCode = kv.Key,
        avnCode = kv.Value.AvnCode,
        unitPrice = kv.Value.Price
    });
    return Results.Ok(prices);
});

// ===== Quản Lý Bảng Kê Thanh Toán Chi Phí Định Vị GPS Trên Xe Ô Tô (Pmt_PaymentGPS / FrmQuanLyThanhToanGPS) =====

// 1) Lấy danh sách bảng kê thanh toán GPS
app.MapGet("/api/payment-gps", async (
    string? pmtMonth,
    string? status,
    string? providerCode,
    string? vin,
    PaymentGPSService gpsService,
    ITenantContext tc) =>
{
    var list = await gpsService.GetListAsync(tc.OrgId, pmtMonth, status, providerCode, vin);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PaymentGPSNo,
        p.PmtMonth,
        p.ContractNo,
        p.ProviderCode,
        p.ProviderName,
        p.TotalVehicles,
        p.TotalPlanDays,
        p.TotalDeductDays,
        p.TotalActualDays,
        p.AmountTotal,
        p.VATRate,
        p.UnitPriceVAT,
        p.TotalAmountVAT,
        status = p.Status.ToString(),
        htvSignStatus = p.HTVSignStatus.ToString(),
        p.HTVSignUser,
        p.HTVSignDTime,
        tcmsSignStatus = p.TCMSSignStatus.ToString(),
        p.TCMSSignUser,
        p.TCMSSignDTime,
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
    }));
});

// 2) Lấy chi tiết 1 bảng kê kèm danh sách xe
app.MapGet("/api/payment-gps/{id:long}", async (long id, PaymentGPSService gpsService, ITenantContext tc) =>
{
    var payment = await gpsService.GetByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê GPS #{id}." });
    return Results.Ok(payment);
});

// 3) Lập bảng kê thanh toán GPS mới
app.MapPost("/api/payment-gps", async (CreatePaymentGPSDto dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var created = await gpsService.CreatePaymentAsync(
            tc.OrgId,
            dto.PaymentGPSNo,
            dto.PmtMonth,
            dto.ContractNo,
            dto.ProviderCode,
            dto.ProviderName,
            dto.VATRate,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Created($"/api/payment-gps/{created.Id}", created);
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

// 4) Duyệt thẩm định cấp 1 HTV (A1)
app.MapPost("/api/payment-gps/{id:long}/htv-approve", async (long id, ApprovePaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.HTVApproveAsync(id, tc.OrgId, dto?.ApproverName);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Duyệt thẩm định cấp 2 TCMS (A2)
app.MapPost("/api/payment-gps/{id:long}/tcms-approve", async (long id, ApprovePaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.TCMSApproveAsync(id, tc.OrgId, dto?.ApproverName);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ký số điện tử CA HTV
app.MapPost("/api/payment-gps/{id:long}/htv-sign", async (long id, SignPaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.HTVSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Ký số điện tử CA TCMS
app.MapPost("/api/payment-gps/{id:long}/tcms-sign", async (long id, SignPaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.TCMSSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Kế toán quyết toán chi trả chuyển khoản UNC ngân hàng (Settled)
app.MapPost("/api/payment-gps/{id:long}/settle", async (long id, SettlePaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.SettlePaymentAsync(id, tc.OrgId, dto?.BankTxnRef, dto?.SettledBy);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Từ chối bảng kê
app.MapPost("/api/payment-gps/{id:long}/reject", async (long id, RejectPaymentGPSDto dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.RejectPaymentAsync(id, tc.OrgId, dto.Reason, dto.RejecterName);
        return Results.Ok(updated);
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

// 10) Hủy bảng kê
app.MapPost("/api/payment-gps/{id:long}/cancel", async (long id, CancelPaymentGPSDto? dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.CancelPaymentAsync(id, tc.OrgId, dto?.Reason);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Bổ sung xe vào bảng kê nháp
app.MapPost("/api/payment-gps/{id:long}/vehicles", async (long id, ImportPaymentGPSVehiclesDto dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.AddVehiclesAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(updated);
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

// 12) Xóa xe khỏi bảng kê nháp
app.MapDelete("/api/payment-gps/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Cập nhật chi tiết xe & tái tính toán bảng kê
app.MapPut("/api/payment-gps/{id:long}/details", async (long id, UpdatePaymentGPSDetailsRequestDto dto, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        var updated = await gpsService.UpdateDetailsAsync(id, tc.OrgId, dto.Items);
        return Results.Ok(updated);
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

// 14) Xóa toàn bộ bảng kê nháp
app.MapDelete("/api/payment-gps/{id:long}", async (long id, PaymentGPSService gpsService, ITenantContext tc) =>
{
    try
    {
        await gpsService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa bảng kê GPS #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 15) Sinh dữ liệu mẫu in Bảng kê quyết toán GPS (CR_Pmt_PaymentGPS Advice)
app.MapGet("/api/payment-gps/{id:long}/advice", async (long id, PaymentGPSService gpsService, ITenantContext tc) =>
{
    var advice = await gpsService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng kê #{id}." });
    return Results.Ok(advice);
});

// 16) Thống kê tổng hợp dashboard
app.MapGet("/api/payment-gps/summary", async (PaymentGPSService gpsService, ITenantContext tc) =>
{
    var summary = await gpsService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 17) Bảng đơn giá tham chiếu dịch vụ định vị GPS theo nhà cung cấp
app.MapGet("/api/payment-gps/providers", () =>
{
    var providers = PaymentGPSService.DefaultGPSProviders.Select(kv => new
    {
        providerCode = kv.Key,
        providerName = kv.Value.ProviderName,
        contractNo = kv.Value.ContractNo,
        dailyPrice = kv.Value.DailyPrice,
        monthlyRate = kv.Value.MonthlyRate
    });
    return Results.Ok(providers);
});

// =========================================================================
// QUẢN LÝ BẢNG TÍNH HỖ TRỢ CHI PHÍ TÀI CHÍNH (CPTC) & CHIẾT KHẤU THANH TOÁN TCG (CKTT)
// Tương ứng module DMS40_FnExp_Calc_FnExp_PmDc trong BizHTC.Payment / 0.41.CalcFnExp & FrmDMS40_2019_MngDMS40_FnExp_Calc_FnExp_PmDc
// =========================================================================

// 1) Danh sách bảng tính CPTC & CKTT TCG
app.MapGet("/api/fn-exp", async (FinancialExpenseService fnService, ITenantContext tc, string? dealerCode, string? status, DateTime? fromDate, DateTime? toDate) =>
{
    var list = await fnService.GetStatementsAsync(tc.OrgId, dealerCode, status, fromDate, toDate);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.CaNo,
        s.DealerCode,
        s.DealerName,
        s.CAName,
        TermFrom = s.TermFrom.ToString("yyyy-MM-dd"),
        TermTo = s.TermTo.ToString("yyyy-MM-dd"),
        TermPrevFrom = s.TermPrevFrom.ToString("yyyy-MM-dd"),
        TermPrevTo = s.TermPrevTo.ToString("yyyy-MM-dd"),
        s.FnExpPercent,
        s.PmtDsTCGPercent,
        s.TotalVehicles,
        s.TotalFnDepositAmount,
        s.TotalFnGrtAmount,
        s.TotalFnAmount,
        s.TotalPDAmount,
        s.TotalSettlementAmount,
        Status = s.Status.ToString(),
        DlrSignStatus = s.DlrSignStatus.ToString(),
        s.DlrSignUser,
        s.DlrSignDTime,
        HTCSignStatus = s.HTCSignStatus.ToString(),
        s.HTCSignUser,
        s.HTCSignDTime,
        s.DlrAppr1By,
        s.DlrAppr1DTime,
        s.HTCAppr1By,
        s.HTCAppr1DTime,
        s.SettledBy,
        s.SettledAt,
        s.BankTxnRef,
        s.FilePathFnExp,
        s.FilePathPmtDc,
        s.Remark,
        s.CreatedBy,
        s.CreatedAt
    }));
});

// 2) Báo cáo thống kê Dashboard KPI CPTC & CKTT
app.MapGet("/api/fn-exp/summary", async (FinancialExpenseService fnService, ITenantContext tc) =>
{
    var summary = await fnService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Danh sách xe ứng viên đủ điều kiện tính CPTC & CKTT
app.MapGet("/api/fn-exp/candidates", (FinancialExpenseService fnService, string? dealerCode) =>
{
    var candidates = fnService.GetCandidateVehicles(dealerCode);
    return Results.Ok(candidates);
});

// 4) Xem trước tính toán CPTC & CKTT (Preview Calculation)
app.MapPost("/api/fn-exp/preview", (PreviewFinancialExpenseDto dto, FinancialExpenseService fnService) =>
{
    try
    {
        var preview = fnService.PreviewCalculation(dto.FnExpPercent, dto.PmtDsTCGPercent, dto.Items ?? []);
        return Results.Ok(preview);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Lập bảng tính CPTC & CKTT TCG mới
app.MapPost("/api/fn-exp", async (CreateFinancialExpenseStatementDto dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var statement = await fnService.CreateStatementAsync(
            tc.OrgId,
            dto.CaNo,
            dto.DealerCode,
            dto.DealerName,
            dto.CAName,
            dto.TermFrom,
            dto.TermTo,
            dto.TermPrevFrom,
            dto.TermPrevTo,
            dto.FnExpPercent ?? 8.5m,
            dto.PmtDsTCGPercent ?? 1.2m,
            dto.Remark,
            dto.CreatedBy,
            dto.Items
        );
        return Results.Ok(statement);
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

// 6) Chi tiết 1 bảng tính kèm danh sách xe
app.MapGet("/api/fn-exp/{id:long}", async (long id, FinancialExpenseService fnService, ITenantContext tc) =>
{
    var statement = await fnService.GetStatementByIdAsync(id, tc.OrgId);
    if (statement == null) return Results.NotFound(new { error = $"Không tìm thấy bảng tính #{id}." });

    return Results.Ok(new
    {
        statement.Id,
        statement.CaNo,
        statement.DealerCode,
        statement.DealerName,
        statement.CAName,
        TermFrom = statement.TermFrom.ToString("yyyy-MM-dd"),
        TermTo = statement.TermTo.ToString("yyyy-MM-dd"),
        TermPrevFrom = statement.TermPrevFrom.ToString("yyyy-MM-dd"),
        TermPrevTo = statement.TermPrevTo.ToString("yyyy-MM-dd"),
        statement.FnExpPercent,
        statement.PmtDsTCGPercent,
        statement.TotalVehicles,
        statement.TotalFnDepositAmount,
        statement.TotalFnGrtAmount,
        statement.TotalFnAmount,
        statement.TotalPDAmount,
        statement.TotalSettlementAmount,
        Status = statement.Status.ToString(),
        DlrSignStatus = statement.DlrSignStatus.ToString(),
        statement.DlrSignUser,
        statement.DlrSignDTime,
        HTCSignStatus = statement.HTCSignStatus.ToString(),
        statement.HTCSignUser,
        statement.HTCSignDTime,
        statement.DlrAppr1By,
        statement.DlrAppr1DTime,
        statement.HTCAppr1By,
        statement.HTCAppr1DTime,
        statement.SettledBy,
        statement.SettledAt,
        statement.BankTxnRef,
        statement.CancelBy,
        statement.CancelDTime,
        statement.CancelReason,
        statement.FilePathFnExp,
        statement.FilePathPmtDc,
        statement.Remark,
        statement.CreatedBy,
        statement.CreatedAt,
        Details = statement.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.StatementId,
            d.CaNo,
            d.CarId,
            d.VIN,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.SpecDescription,
            d.ColorName,
            d.SOCode,
            AssemblyType = d.AssemblyType.ToString(),
            d.UnitPriceActual,
            SodApprovedDate = d.SodApprovedDate?.ToString("yyyy-MM-dd"),
            SodDepositDutyEndDate = d.SodDepositDutyEndDate?.ToString("yyyy-MM-dd"),
            TotalCompletedDate = d.TotalCompletedDate?.ToString("yyyy-MM-dd"),
            DateStart = d.DateStart?.ToString("yyyy-MM-dd"),
            DateEnd = d.DateEnd?.ToString("yyyy-MM-dd"),
            d.TermActual,
            d.FnDepositCountDate,
            d.FnDepositAmount,
            d.FnGrtCountDate,
            d.FnGrtAmount,
            d.FnTotalAmount,
            d.PDCountDate,
            d.PDAmount,
            d.CarTotalSettlement,
            Status = d.Status.ToString(),
            d.Remark
        })
    });
});

// 7) Đại lý duyệt thẩm định cấp 1 (DlrApprove1)
app.MapPost("/api/fn-exp/{id:long}/dlr-approve1", async (long id, ApproveFnExpDto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.DlrApprove1Async(id, tc.OrgId, dto?.ApproverName);
        return Results.Ok(new { message = $"Đại lý duyệt cấp 1 bảng tính #{id} thành công.", Status = s.Status.ToString(), s.DlrAppr1By, s.DlrAppr1DTime });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Đại lý ký số điện tử CA cấp 2 (DlrSignCA)
app.MapPost("/api/fn-exp/{id:long}/dlr-sign", async (long id, SignFnExpCADto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.DlrSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        return Results.Ok(new { message = $"Đại lý ký số CA bảng tính #{id} thành công.", Status = s.Status.ToString(), DlrSignStatus = s.DlrSignStatus.ToString(), s.DlrSignUser, s.DlrSignDTime });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) HTC Chuyên viên tài chính thẩm định duyệt cấp 1 (HTCApprove1)
app.MapPost("/api/fn-exp/{id:long}/htc-approve1", async (long id, ApproveFnExpDto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.HTCApprove1Async(id, tc.OrgId, dto?.ApproverName);
        return Results.Ok(new { message = $"HTC thẩm định duyệt cấp 1 bảng tính #{id} thành công.", Status = s.Status.ToString(), s.HTCAppr1By, s.HTCAppr1DTime });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) HTC Lãnh đạo ký số điện tử CA cấp 2 (HTCSignCA)
app.MapPost("/api/fn-exp/{id:long}/htc-sign", async (long id, SignFnExpCADto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.HTCSignCAAsync(id, tc.OrgId, dto?.SignerName, dto?.FilePath);
        return Results.Ok(new { message = $"HTC phê duyệt ký số CA bảng tính #{id} thành công.", Status = s.Status.ToString(), HTCSignStatus = s.HTCSignStatus.ToString(), s.HTCSignUser, s.HTCSignDTime });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Kế toán quyết toán chi trả chuyển khoản UNC ngân hàng (Settled)
app.MapPost("/api/fn-exp/{id:long}/settle", async (long id, SettleFnExpDto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.SettleAsync(id, tc.OrgId, dto?.BankTxnRef, dto?.SettledBy);
        return Results.Ok(new { message = $"Quyết toán chuyển tiền bảng tính #{id} thành công.", Status = s.Status.ToString(), s.BankTxnRef, s.SettledBy, s.SettledAt, s.TotalSettlementAmount });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Hủy bảng tính (Cancel)
app.MapPost("/api/fn-exp/{id:long}/cancel", async (long id, CancelFnExpDto? dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.CancelAsync(id, tc.OrgId, dto?.Reason, dto?.CancelledBy);
        return Results.Ok(new { message = $"Đã hủy bảng tính #{id}.", Status = s.Status.ToString(), s.CancelReason, s.CancelBy, s.CancelDTime });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Cập nhật chi tiết xe & tự động tái tính toán bảng kê
app.MapPut("/api/fn-exp/{id:long}/details", async (long id, UpdateFnExpDetailsRequestDto dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.UpdateDetailsAsync(id, tc.OrgId, dto.Items ?? []);
        return Results.Ok(s);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 14) Thêm xe vào bảng tính nháp
app.MapPost("/api/fn-exp/{id:long}/vehicles", async (long id, ImportFnExpVehiclesDto dto, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.AddVehiclesAsync(id, tc.OrgId, dto.Items ?? []);
        return Results.Ok(s);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 15) Xóa xe khỏi bảng tính nháp
app.MapDelete("/api/fn-exp/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        var s = await fnService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(s);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 16) Xóa toàn bộ bảng tính nháp
app.MapDelete("/api/fn-exp/{id:long}", async (long id, FinancialExpenseService fnService, ITenantContext tc) =>
{
    try
    {
        await fnService.DeleteDraftAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã xóa bảng tính #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 17) Sinh dữ liệu mẫu in Bảng kê Quyết toán CPTC & CKTT gửi Đại lý (CR Advice)
app.MapGet("/api/fn-exp/{id:long}/advice", async (long id, FinancialExpenseService fnService, ITenantContext tc) =>
{
    var advice = await fnService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy bảng tính #{id}." });
    return Results.Ok(advice);
});

// ===== Quản lý Hồ sơ Đề nghị Giao dịch Ngân hàng & Tài trợ Vốn Vay / Bảo lãnh Đại lý (Dealer Banking Disbursement - BizHTC.Payment) =====

// 1) Lập đề nghị giao dịch ngân hàng mới (RQ_BankingTransactions_Save / FrmDeNghiGDNganHang)
app.MapPost("/api/disbursement/requests", async (CreateDisbursementRequestDto dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.CreateRequestAsync(tc.OrgId, dto);
        return Results.Created($"/api/disbursement/requests/{req.Id}", req);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Tìm kiếm danh sách đề nghị giao dịch ngân hàng (RQ_BankingTransactions_Get / FrmQL_DeNghiGDNganHang)
app.MapGet("/api/disbursement/requests", async (
    string? dealerCode,
    string? bankCode,
    string? transType,
    string? status,
    string? bankStatus,
    DateTime? fromDate,
    DateTime? toDate,
    BankingDisbursementService disbService,
    ITenantContext tc) =>
{
    BankingTransType? tType = null;
    if (!string.IsNullOrWhiteSpace(transType) && Enum.TryParse<BankingTransType>(transType, true, out var parsedType))
        tType = parsedType;

    BankingTransStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BankingTransStatus>(status, true, out var parsedSt))
        st = parsedSt;

    BankingTransBankStatus? bSt = null;
    if (!string.IsNullOrWhiteSpace(bankStatus) && Enum.TryParse<BankingTransBankStatus>(bankStatus, true, out var parsedBSt))
        bSt = parsedBSt;

    var list = await disbService.GetRequestsAsync(tc.OrgId, dealerCode, bankCode, tType, st, bSt, fromDate, toDate);
    return Results.Ok(list);
});

// 3) Chi tiết 1 đề nghị giao dịch ngân hàng kèm danh mục xe và file chứng từ
app.MapGet("/api/disbursement/requests/{id:long}", async (long id, BankingDisbursementService disbService, ITenantContext tc) =>
{
    var req = await disbService.GetRequestByIdAsync(id, tc.OrgId);
    if (req == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị #{id}." });
    return Results.Ok(req);
});

// 4) Đẩy đề nghị sang cổng e-Banking ngân hàng kết nối (RQ_BankingTransactions_PushBank)
app.MapPost("/api/disbursement/requests/{id:long}/push-bank", async (long id, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.PushToBankAsync(id, tc.OrgId);
        return Results.Ok(new { message = $"Đã đẩy hồ sơ #{req.TransNo} sang cổng e-Banking {req.BankName} thành công.", req.RefBankCode, req.Status, req.BankStatus, req.BankRemark });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) Ngân hàng tiếp nhận thẩm định hồ sơ tín dụng
app.MapPost("/api/disbursement/requests/{id:long}/bank-review", async (long id, BankReviewDto? dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.ReviewByBankAsync(id, tc.OrgId, dto);
        return Results.Ok(new { message = $"Ngân hàng {req.BankName} đã tiếp nhận thẩm định hồ sơ #{req.TransNo}.", req.Status, req.BankStatus, req.BankRemark });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ngân hàng yêu cầu bổ sung tài liệu file hoặc hồ sơ pháp lý (Approve1/2/3)
app.MapPost("/api/disbursement/requests/{id:long}/request-docs", async (long id, RequestMoreDocsDto dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.RequestMoreDocsAsync(id, tc.OrgId, dto);
        return Results.Ok(new { message = $"Ngân hàng yêu cầu bổ sung hồ sơ cho đề nghị #{req.TransNo}.", req.BankStatus, req.BankRemark });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Đại lý ký số điện tử CA trên file chứng từ tài chính gửi ngân hàng
app.MapPost("/api/disbursement/files/{fileId:long}/sign", async (long fileId, SignBankFileDto dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var file = await disbService.SignBankFileAsync(fileId, tc.OrgId, dto);
        return Results.Ok(new { message = $"Ký số CA tài liệu '{file.FileName}' thành công.", file.SignStatus, file.SignedUser, file.CertSerialNumber, file.SignedAt });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Ngân hàng phê duyệt cấp tín dụng & hoàn tất giải ngân chuyển tiền (Finish / Disbursed)
app.MapPost("/api/disbursement/requests/{id:long}/disburse", async (long id, ApproveDisburseDto dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.ApproveAndDisburseAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Ngân hàng đã phê duyệt & hoàn tất giải ngân {req.ActualDisbursedAmount:N0} VND cho đề nghị #{req.TransNo}.",
            req.Status,
            req.BankStatus,
            req.ActualDisbursedAmount,
            req.LDNo,
            req.MDNo,
            req.LCNo,
            DisbursementDate = req.DisbursementDate?.ToString("yyyy-MM-dd"),
            req.BankRemark
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Ngân hàng từ chối cấp tín dụng (Rejected)
app.MapPost("/api/disbursement/requests/{id:long}/reject", async (long id, RejectDisbursementDto dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.RejectByBankAsync(id, tc.OrgId, dto);
        return Results.Ok(new { message = $"Ngân hàng đã từ chối hồ sơ đề nghị #{req.TransNo}.", req.BankStatus, req.BankRemark });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Đại lý hủy đề nghị khi chưa giải ngân
app.MapPost("/api/disbursement/requests/{id:long}/cancel", async (long id, RejectDisbursementDto? dto, BankingDisbursementService disbService, ITenantContext tc) =>
{
    try
    {
        var req = await disbService.CancelRequestAsync(id, tc.OrgId, dto?.Reason);
        return Results.Ok(new { message = $"Đã hủy đề nghị #{req.TransNo} thành công.", req.Status, req.CancelReason, req.CancelledAt });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Sinh dữ liệu mẫu in Giấy đề nghị giao dịch ngân hàng & cam kết tín dụng (Disbursement Advice)
app.MapGet("/api/disbursement/requests/{id:long}/advice", async (long id, BankingDisbursementService disbService, ITenantContext tc) =>
{
    var advice = await disbService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị #{id}." });
    return Results.Ok(advice);
});

// 12) Thống kê tổng hợp số liệu tín dụng đại lý
app.MapGet("/api/disbursement/summary", async (BankingDisbursementService disbService, ITenantContext tc) =>
{
    var summary = await disbService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 13) Lấy danh sách hợp đồng mẫu ứng viên để lập nhanh đề nghị
app.MapGet("/api/disbursement/candidate-contracts", (BankingDisbursementService disbService, string? dealerCode) =>
{
    var candidates = disbService.GetCandidateContracts(dealerCode);
    return Results.Ok(candidates);
});

// ===== Quản lý Hồ sơ Đề nghị Hủy Gán Ngân Hàng Bảo Lãnh Cho Hợp Đồng Xe (Cancel Bank MD - DMS40_DlrCtr_CancelBankMD) =====

// 1) Lập đề nghị hủy gán ngân hàng bảo lãnh mới (DMS40_DlrCtr_CancelBankMD_Save)
app.MapPost("/api/cancel-bank-md", async (CreateCancelBankMDDto dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    if (string.IsNullOrWhiteSpace(dto.DlrCtrNo))
        return Results.BadRequest(new { error = "Số phụ lục hợp đồng đại lý (DlrCtrNo) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.DealerCode))
        return Results.BadRequest(new { error = "Mã đại lý (DealerCode) không được để trống." });
    if (string.IsNullOrWhiteSpace(dto.BankCodeMD))
        return Results.BadRequest(new { error = "Mã ngân hàng bảo lãnh cần hủy (BankCodeMD) không được để trống." });

    try
    {
        var req = await cancelService.CreateRequestAsync(tc.OrgId, dto, dto.CreatedBy ?? "DealerCreditOfficer");
        return Results.Ok(new
        {
            req.Id,
            req.CancelBankMDNo,
            req.DlrCtrNo,
            req.DealerCode,
            req.DealerName,
            req.BankCodeMD,
            req.BankNameMD,
            req.NewBankCodeMD,
            req.NewBankNameMD,
            GuaranteeType = req.GuaranteeType.ToString(),
            ReasonType = req.ReasonType.ToString(),
            req.ReasonDescription,
            req.ContractAmount,
            req.GuaranteeAmount,
            req.TotalVehicles,
            Status = req.Status.ToString(),
            req.RemarkDlr,
            req.CreatedBy,
            req.CreatedAt,
            Details = req.Details.Select(d => new
            {
                d.Id,
                d.VIN,
                d.CarId,
                d.ModelCode,
                d.ModelName,
                d.SpecCode,
                d.SpecDescription,
                d.ColorExtNameVN,
                d.EngineNo,
                d.UnitPrice,
                d.GuaranteeAmount,
                Status = d.Status.ToString(),
                d.Remark
            })
        });
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

// 2) Danh sách hồ sơ đề nghị hủy gán ngân hàng bảo lãnh (DMS40_DlrCtr_CancelBankMD_GetX)
app.MapGet("/api/cancel-bank-md", async (
    string? dealerCode,
    string? bankCode,
    string? status,
    string? reasonType,
    DateTime? fromDate,
    DateTime? toDate,
    CancelBankMDService cancelService,
    ITenantContext tc) =>
{
    CancelBankMDStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CancelBankMDStatus>(status, true, out var parsedSt))
        st = parsedSt;

    CancelBankMDReasonType? rt = null;
    if (!string.IsNullOrWhiteSpace(reasonType) && Enum.TryParse<CancelBankMDReasonType>(reasonType, true, out var parsedRt))
        rt = parsedRt;

    var list = await cancelService.GetRequestsAsync(tc.OrgId, dealerCode, bankCode, st, rt, fromDate, toDate);
    return Results.Ok(list.Select(r => new
    {
        r.Id,
        r.CancelBankMDNo,
        r.DlrCtrNo,
        r.DealerCode,
        r.DealerName,
        r.BankCodeMD,
        r.BankNameMD,
        r.NewBankCodeMD,
        r.NewBankNameMD,
        GuaranteeType = r.GuaranteeType.ToString(),
        ReasonType = r.ReasonType.ToString(),
        r.ReasonDescription,
        r.ContractAmount,
        r.GuaranteeAmount,
        r.TotalVehicles,
        Status = r.Status.ToString(),
        r.RemarkDlr,
        r.RemarkBank,
        r.RemarkHTC,
        r.ApproveBy,
        r.ApproveDateTime,
        r.FinishBy,
        r.FinishDTime,
        r.RejectBy,
        r.RejectDateTime,
        r.RejectReason,
        r.CancelBy,
        r.CancelDateTime,
        r.CancelReason,
        r.CreatedBy,
        r.CreatedAt,
        VehicleCount = r.Details.Count
    }));
});

// 3) Chi tiết 1 hồ sơ đề nghị hủy gán ngân hàng bảo lãnh kèm danh sách xe
app.MapGet("/api/cancel-bank-md/{id:long}", async (long id, CancelBankMDService cancelService, ITenantContext tc) =>
{
    var req = await cancelService.GetRequestByIdAsync(id, tc.OrgId);
    if (req == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ đề nghị #{id}." });

    return Results.Ok(new
    {
        req.Id,
        req.CancelBankMDNo,
        req.DlrCtrNo,
        req.DealerCode,
        req.DealerName,
        req.BankCodeMD,
        req.BankNameMD,
        req.NewBankCodeMD,
        req.NewBankNameMD,
        GuaranteeType = req.GuaranteeType.ToString(),
        ReasonType = req.ReasonType.ToString(),
        req.ReasonDescription,
        req.ContractAmount,
        req.GuaranteeAmount,
        req.TotalVehicles,
        Status = req.Status.ToString(),
        req.RemarkDlr,
        req.RemarkBank,
        req.RemarkHTC,
        req.ApproveBy,
        req.ApproveDateTime,
        req.FinishBy,
        req.FinishDTime,
        req.RejectBy,
        req.RejectDateTime,
        req.RejectReason,
        req.CancelBy,
        req.CancelDateTime,
        req.CancelReason,
        req.CreatedBy,
        req.CreatedAt,
        Details = req.Details.Select(d => new
        {
            d.Id,
            d.CancelBankMDId,
            d.CancelBankMDNo,
            d.VIN,
            d.CarId,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.SpecDescription,
            d.ColorExtNameVN,
            d.EngineNo,
            d.UnitPrice,
            d.GuaranteeAmount,
            Status = d.Status.ToString(),
            d.Remark
        })
    });
});

// 4) Ngân hàng phát hành bảo lãnh phê duyệt chấp thuận hủy (DMS40_DlrCtr_CancelBankMD_Approve)
app.MapPost("/api/cancel-bank-md/{id:long}/approve-bank", async (long id, ApproveCancelBankMDBankDto? dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.ApproveByBankAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Ngân hàng {req.BankNameMD} đã phê duyệt chấp thuận hủy bảo lãnh cho hợp đồng {req.DlrCtrNo}.",
            Status = req.Status.ToString(),
            req.ApproveBy,
            req.ApproveDateTime,
            req.RemarkBank
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5) HTC thẩm định và duyệt hoàn tất hủy gán ngân hàng bảo lãnh (DMS40_DlrCtr_CancelBankMD_Finish)
app.MapPost("/api/cancel-bank-md/{id:long}/finish-htc", async (long id, FinishCancelBankMDHTCDto? dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.FinishByHTCAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"HTC đã duyệt hoàn tất hủy gán bảo lãnh cho hợp đồng {req.DlrCtrNo}. Đã gỡ bỏ BankCodeMD trên hợp đồng.",
            Status = req.Status.ToString(),
            req.FinishBy,
            req.FinishDTime,
            req.RemarkHTC
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Ngân hàng từ chối đề nghị hủy gán bảo lãnh
app.MapPost("/api/cancel-bank-md/{id:long}/reject-bank", async (long id, RejectCancelBankMDDto dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.RejectByBankAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Ngân hàng {req.BankNameMD} đã từ chối đề nghị #{req.CancelBankMDNo}.",
            Status = req.Status.ToString(),
            req.RejectBy,
            req.RejectDateTime,
            req.RejectReason,
            req.RemarkBank
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) HTC từ chối đề nghị hủy gán bảo lãnh (DMS40_DlrCtr_CancelBankMD_Reject)
app.MapPost("/api/cancel-bank-md/{id:long}/reject-htc", async (long id, RejectCancelBankMDDto dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.RejectByHTCAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"HTC đã từ chối đề nghị #{req.CancelBankMDNo}.",
            Status = req.Status.ToString(),
            req.RejectBy,
            req.RejectDateTime,
            req.RejectReason,
            req.RemarkHTC
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Đại lý hủy đề nghị khi chưa giải quyết xong (DMS40_DlrCtr_CancelBankMD_Cancel)
app.MapPost("/api/cancel-bank-md/{id:long}/cancel", async (long id, CancelBankMDUserCancelDto? dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.CancelRequestAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã hủy đề nghị #{req.CancelBankMDNo} thành công.",
            Status = req.Status.ToString(),
            req.CancelBy,
            req.CancelDateTime,
            req.CancelReason
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Bổ sung xe vào đề nghị đang chờ duyệt
app.MapPost("/api/cancel-bank-md/{id:long}/vehicles", async (long id, CancelBankMDItemInputDto dto, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.AddVehicleAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã bổ sung xe VIN '{dto.VIN}' vào đề nghị #{req.CancelBankMDNo}.",
            req.TotalVehicles,
            req.ContractAmount,
            req.GuaranteeAmount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Xóa xe khỏi đề nghị đang chờ duyệt
app.MapDelete("/api/cancel-bank-md/{id:long}/vehicles/{detailId:long}", async (long id, long detailId, CancelBankMDService cancelService, ITenantContext tc) =>
{
    try
    {
        var req = await cancelService.RemoveVehicleAsync(id, detailId, tc.OrgId);
        return Results.Ok(new
        {
            message = $"Đã xóa xe #{detailId} khỏi đề nghị #{req.CancelBankMDNo}.",
            req.TotalVehicles,
            req.ContractAmount,
            req.GuaranteeAmount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Sinh dữ liệu mẫu in Thỏa thuận 3 bên Hủy Cam kết Bảo lãnh Ngân hàng
app.MapGet("/api/cancel-bank-md/{id:long}/advice", async (long id, CancelBankMDService cancelService, ITenantContext tc) =>
{
    var advice = await cancelService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy đề nghị #{id}." });
    return Results.Ok(advice);
});

// 12) Dashboard KPI tổng hợp đề nghị hủy gán ngân hàng bảo lãnh
app.MapGet("/api/cancel-bank-md/summary", async (CancelBankMDService cancelService, ITenantContext tc) =>
{
    var summary = await cancelService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 13) Lấy danh sách hợp đồng mẫu ứng viên đang gán bảo lãnh
app.MapGet("/api/cancel-bank-md/candidates", (CancelBankMDService cancelService, string? dealerCode) =>
{
    var candidates = cancelService.GetCandidateContracts(dealerCode);
    return Results.Ok(candidates);
});

// ===== Quản lý Thu Tiền Thanh Toán & Quyết Toán Bồi Thường Bảo Hiểm Xe Ô Tô (Insurance Claim Payment & Settlement - BizCarSv.Debit.cs / FrmInsPaymentCreate) =====

// 1) Tạo hồ sơ công nợ bồi thường bảo hiểm mới theo Lệnh sửa chữa RO
app.MapPost("/api/insurance-payments/debits", async (CreateInsuranceDebitDto dto, InsurancePaymentService insService, ITenantContext tc) =>
{
    try
    {
        var debit = await insService.CreateDebitAsync(tc.OrgId, dto);
        return Results.Created($"/api/insurance-payments/debits/{debit.Id}", debit);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 2) Tìm kiếm danh sách công nợ bồi thường bảo hiểm
app.MapGet("/api/insurance-payments/debits", async (
    string? insNo,
    string? status,
    string? keyword,
    DateTime? fromDate,
    DateTime? toDate,
    InsurancePaymentService insService,
    ITenantContext tc) =>
{
    InsuranceDebitStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InsuranceDebitStatus>(status, true, out var parsedSt))
        st = parsedSt;

    var list = await insService.GetDebitsAsync(tc.OrgId, insNo, st, keyword, fromDate, toDate);
    return Results.Ok(list);
});

// 3) Chi tiết 1 hồ sơ công nợ bồi thường bảo hiểm
app.MapGet("/api/insurance-payments/debits/{id:long}", async (long id, InsurancePaymentService insService, ITenantContext tc) =>
{
    var debit = await insService.GetDebitByIdAsync(id, tc.OrgId);
    if (debit == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ nợ #{id}." });
    return Results.Ok(debit);
});

// 4) Lấy danh sách hồ sơ còn nợ của một công ty bảo hiểm (để phân bổ thanh toán)
app.MapGet("/api/insurance-payments/debits/eligible/{insNo}", async (string insNo, InsurancePaymentService insService, ITenantContext tc) =>
{
    var list = await insService.GetEligibleDebitsAsync(tc.OrgId, insNo);
    return Results.Ok(list);
});

// 5) Lập phiếu thu thanh toán bảo hiểm mới (Tự động phân bổ FIFO trừ nợ RO)
app.MapPost("/api/insurance-payments", async (CreateInsurancePaymentDto dto, InsurancePaymentService insService, ITenantContext tc) =>
{
    try
    {
        var payment = await insService.CreatePaymentAsync(tc.OrgId, dto);
        return Results.Created($"/api/insurance-payments/{payment.Id}", payment);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Tìm kiếm danh sách phiếu thu thanh toán bảo hiểm
app.MapGet("/api/insurance-payments", async (
    string? insNo,
    string? status,
    string? method,
    DateTime? fromDate,
    DateTime? toDate,
    InsurancePaymentService insService,
    ITenantContext tc) =>
{
    InsurancePaymentStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InsurancePaymentStatus>(status, true, out var parsedSt))
        st = parsedSt;

    InsurancePaymentMethod? m = null;
    if (!string.IsNullOrWhiteSpace(method) && Enum.TryParse<InsurancePaymentMethod>(method, true, out var parsedM))
        m = parsedM;

    var list = await insService.GetPaymentsAsync(tc.OrgId, insNo, st, m, fromDate, toDate);
    return Results.Ok(list);
});

// 7) Chi tiết 1 phiếu thu thanh toán bảo hiểm kèm danh sách phân bổ RO
app.MapGet("/api/insurance-payments/{id:long}", async (long id, InsurancePaymentService insService, ITenantContext tc) =>
{
    var payment = await insService.GetPaymentByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thu #{id}." });
    return Results.Ok(payment);
});

// 8) Kế toán xác nhận phiếu thu thanh toán bảo hiểm (Draft -> Confirmed)
app.MapPost("/api/insurance-payments/{id:long}/confirm", async (long id, ConfirmInsurancePaymentDto? dto, InsurancePaymentService insService, ITenantContext tc) =>
{
    try
    {
        var payment = await insService.ConfirmPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã xác nhận phiếu thu #{payment.PaymentNo} thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.ConfirmedBy,
            payment.ConfirmedAt
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Hoàn tất quyết toán công nợ và chốt sổ kế toán (Confirmed -> Settled)
app.MapPost("/api/insurance-payments/{id:long}/settle", async (long id, SettleInsurancePaymentDto? dto, InsurancePaymentService insService, ITenantContext tc) =>
{
    try
    {
        var payment = await insService.SettlePaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã quyết toán hoàn tất phiếu thu #{payment.PaymentNo}.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.SettledBy,
            payment.SettledAt
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy phiếu thu thanh toán bảo hiểm và HOÀN TÁC (ROLLBACK) nợ trên các hồ sơ RO
app.MapPost("/api/insurance-payments/{id:long}/cancel", async (long id, CancelInsurancePaymentDto dto, InsurancePaymentService insService, ITenantContext tc) =>
{
    try
    {
        var payment = await insService.CancelPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã hủy phiếu thu #{payment.PaymentNo} và hoàn tác nợ trên các lệnh RO thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.CancelledBy,
            payment.CancelledAt,
            payment.CancelReason
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Sinh dữ liệu mẫu in Giấy Báo Thu Tiền Quyết Toán Bảo Hiểm (Advice)
app.MapGet("/api/insurance-payments/{id:long}/advice", async (long id, InsurancePaymentService insService, ITenantContext tc) =>
{
    var advice = await insService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thu #{id}." });
    return Results.Ok(advice);
});

// 12) Dashboard KPI tổng hợp công nợ và quyết toán bảo hiểm
app.MapGet("/api/insurance-payments/summary", async (InsurancePaymentService insService, ITenantContext tc) =>
{
    var summary = await insService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản Lý Công Nợ & Thanh Toán Quyết Toán Cho Nhà Cung Cấp Phụ Tùng / Dịch Vụ Xe Ô Tô (Supplier Debit & Payment / BizCarSv.Debit.cs) =====

// 1) Lập hồ sơ công nợ nhà cung cấp mới theo phiếu nhập kho phụ tùng (Ser_SupplierDebit / FrmSuplierDebitCreate)
app.MapPost("/api/supplier-payments/debits", async (CreateSupplierDebitDto dto, SupplierPaymentService suppService, ITenantContext tc) =>
{
    try
    {
        var debit = await suppService.CreateDebitAsync(tc.OrgId, dto);
        return Results.Created($"/api/supplier-payments/debits/{debit.Id}", debit);
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

// 2) Tìm kiếm danh sách công nợ phải trả nhà cung cấp (SerSupplierDebitDetailGet / FrmSupplierDebitSearch)
app.MapGet("/api/supplier-payments/debits", async (
    string? supplierCode,
    string? status,
    string? keyword,
    DateTime? fromDate,
    DateTime? toDate,
    SupplierPaymentService suppService,
    ITenantContext tc) =>
{
    SupplierDebitStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SupplierDebitStatus>(status, true, out var parsedSt))
        st = parsedSt;

    var list = await suppService.GetDebitsAsync(tc.OrgId, supplierCode, st, keyword, fromDate, toDate);
    return Results.Ok(list);
});

// 3) Chi tiết 1 hồ sơ công nợ nhà cung cấp
app.MapGet("/api/supplier-payments/debits/{id:long}", async (long id, SupplierPaymentService suppService, ITenantContext tc) =>
{
    var debit = await suppService.GetDebitByIdAsync(id, tc.OrgId);
    if (debit == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ nợ #{id}." });
    return Results.Ok(debit);
});

// 4) Lấy danh sách phiếu nhập kho còn nợ của một nhà cung cấp (để phân bổ thanh toán)
app.MapGet("/api/supplier-payments/debits/eligible/{supplierCode}", async (string supplierCode, SupplierPaymentService suppService, ITenantContext tc) =>
{
    var list = await suppService.GetEligibleDebitsAsync(tc.OrgId, supplierCode);
    return Results.Ok(list);
});

// 5) Lập phiếu chi thanh toán nhà cung cấp mới (Tự động phân bổ FIFO trừ nợ phiếu nhập kho - SerPaymentCreate)
app.MapPost("/api/supplier-payments", async (CreateSupplierPaymentDto dto, SupplierPaymentService suppService, ITenantContext tc) =>
{
    try
    {
        var payment = await suppService.CreatePaymentAsync(tc.OrgId, dto);
        return Results.Created($"/api/supplier-payments/{payment.Id}", payment);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Tìm kiếm danh sách phiếu chi thanh toán nhà cung cấp (FrmSupplierPaymentCreate)
app.MapGet("/api/supplier-payments", async (
    string? supplierCode,
    string? status,
    string? method,
    DateTime? fromDate,
    DateTime? toDate,
    SupplierPaymentService suppService,
    ITenantContext tc) =>
{
    SupplierPaymentStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SupplierPaymentStatus>(status, true, out var parsedSt))
        st = parsedSt;

    SupplierPaymentMethod? m = null;
    if (!string.IsNullOrWhiteSpace(method) && Enum.TryParse<SupplierPaymentMethod>(method, true, out var parsedM))
        m = parsedM;

    var list = await suppService.GetPaymentsAsync(tc.OrgId, supplierCode, st, m, fromDate, toDate);
    return Results.Ok(list);
});

// 7) Chi tiết 1 phiếu chi thanh toán nhà cung cấp kèm danh sách phân bổ phiếu nhập kho
app.MapGet("/api/supplier-payments/{id:long}", async (long id, SupplierPaymentService suppService, ITenantContext tc) =>
{
    var payment = await suppService.GetPaymentByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu chi #{id}." });
    return Results.Ok(payment);
});

// 8) Kế toán trưởng thẩm định xác nhận phiếu chi nhà cung cấp (Draft -> Confirmed)
app.MapPost("/api/supplier-payments/{id:long}/confirm", async (long id, ConfirmSupplierPaymentDto? dto, SupplierPaymentService suppService, ITenantContext tc) =>
{
    try
    {
        var payment = await suppService.ConfirmPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã xác nhận phiếu chi #{payment.PaymentNo} thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.ConfirmedBy,
            payment.ConfirmedAt
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Hoàn tất xuất quỹ / chuyển khoản ngân hàng UNC và chốt sổ kế toán (Confirmed -> Settled)
app.MapPost("/api/supplier-payments/{id:long}/settle", async (long id, SettleSupplierPaymentDto? dto, SupplierPaymentService suppService, ITenantContext tc) =>
{
    try
    {
        var payment = await suppService.SettlePaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã quyết toán hoàn tất phiếu chi #{payment.PaymentNo}.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.SettledBy,
            payment.SettledAt,
            payment.BankTxnRef
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy phiếu chi thanh toán nhà cung cấp và HOÀN TÁC (ROLLBACK) nợ trên các phiếu nhập kho
app.MapPost("/api/supplier-payments/{id:long}/cancel", async (long id, CancelSupplierPaymentDto dto, SupplierPaymentService suppService, ITenantContext tc) =>
{
    try
    {
        var payment = await suppService.CancelPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã hủy phiếu chi #{payment.PaymentNo} và hoàn tác nợ trên các phiếu nhập kho thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.CancelledBy,
            payment.CancelledAt,
            payment.CancelReason
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Sinh dữ liệu mẫu in Phiếu Chi Thanh Toán Tiền Nhà Cung Cấp Phụ Tùng (Advice - SerPaymentPaperRpt)
app.MapGet("/api/supplier-payments/{id:long}/advice", async (long id, SupplierPaymentService suppService, ITenantContext tc) =>
{
    var advice = await suppService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu chi #{id}." });
    return Results.Ok(advice);
});

// 12) Dashboard KPI tổng hợp công nợ và quyết toán nhà cung cấp
app.MapGet("/api/supplier-payments/summary", async (SupplierPaymentService suppService, ITenantContext tc) =>
{
    var summary = await suppService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== QUẢN LÝ CÔNG NỢ & THU TIỀN THANH TOÁN KHÁCH HÀNG DỊCH VỤ SỬA CHỮA (Customer Service Debit & Settlement Payment) =====
// Tương ứng BizCarSv.Debit.cs (SerCusDebitCreate, SerCusDebitSearch, SerCusDebitDetailGet, SerPaymentCreate, SerPaymentGet, SerPaymentDelete, SerPaymentPaperRpt)
// và FrmCusDebitCreate, FrmCusDebitSearch, FrmCusPaymentCreate trong TERP.HTCServiceClient/Views/Debit hệ nguồn HTC 2010.

// 1) Lập hồ sơ công nợ dịch vụ sửa chữa mới theo Lệnh RO
app.MapPost("/api/customer-payments/debits", async (CreateCustomerDebitDto dto, CustomerPaymentService cusService, ITenantContext tc) =>
{
    try
    {
        var debit = await cusService.CreateDebitAsync(tc.OrgId, dto);
        return Results.Created($"/api/customer-payments/debits/{debit.Id}", debit);
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

// 2) Tìm kiếm danh sách công nợ dịch vụ khách hàng (SerCusDebitSearch)
app.MapGet("/api/customer-payments/debits", async (
    string? cusId,
    string? status,
    string? keyword,
    DateTime? fromDate,
    DateTime? toDate,
    CustomerPaymentService cusService,
    ITenantContext tc) =>
{
    CustomerDebitStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomerDebitStatus>(status, true, out var parsedSt))
        st = parsedSt;

    var list = await cusService.GetDebitsAsync(tc.OrgId, cusId, st, keyword, fromDate, toDate);
    return Results.Ok(list);
});

// 3) Chi tiết 1 hồ sơ công nợ dịch vụ sửa chữa (SerCusDebitDetailGet)
app.MapGet("/api/customer-payments/debits/{id:long}", async (long id, CustomerPaymentService cusService, ITenantContext tc) =>
{
    var debit = await cusService.GetDebitByIdAsync(id, tc.OrgId);
    if (debit == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ công nợ dịch vụ #{id}." });
    return Results.Ok(debit);
});

// 4) Lấy danh sách các lệnh RO còn nợ của một khách hàng (để lập phiếu thu & phân bổ)
app.MapGet("/api/customer-payments/debits/eligible/{cusId}", async (string cusId, CustomerPaymentService cusService, ITenantContext tc) =>
{
    var list = await cusService.GetEligibleDebitsAsync(tc.OrgId, cusId);
    return Results.Ok(list);
});

// 5) Lập phiếu thu tiền thanh toán dịch vụ khách hàng mới (Tự động phân bổ FIFO trừ nợ RO)
app.MapPost("/api/customer-payments", async (CreateCustomerPaymentDto dto, CustomerPaymentService cusService, ITenantContext tc) =>
{
    try
    {
        var payment = await cusService.CreatePaymentAsync(tc.OrgId, dto);
        return Results.Created($"/api/customer-payments/{payment.Id}", payment);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Tìm kiếm danh sách phiếu thu tiền dịch vụ của khách hàng (SerPaymentGet)
app.MapGet("/api/customer-payments", async (
    string? cusId,
    string? status,
    string? method,
    DateTime? fromDate,
    DateTime? toDate,
    CustomerPaymentService cusService,
    ITenantContext tc) =>
{
    CustomerPaymentStatus? st = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomerPaymentStatus>(status, true, out var parsedSt))
        st = parsedSt;

    CustomerPaymentMethod? m = null;
    if (!string.IsNullOrWhiteSpace(method) && Enum.TryParse<CustomerPaymentMethod>(method, true, out var parsedM))
        m = parsedM;

    var list = await cusService.GetPaymentsAsync(tc.OrgId, cusId, st, m, fromDate, toDate);
    return Results.Ok(list);
});

// 7) Chi tiết 1 phiếu thu thanh toán dịch vụ kèm danh sách phân bổ RO (SerPaymentGet)
app.MapGet("/api/customer-payments/{id:long}", async (long id, CustomerPaymentService cusService, ITenantContext tc) =>
{
    var payment = await cusService.GetPaymentByIdAsync(id, tc.OrgId);
    if (payment == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thu #{id}." });
    return Results.Ok(payment);
});

// 8) Thu ngân / Kế toán xác nhận phiếu thu tiền dịch vụ (Draft -> Confirmed)
app.MapPost("/api/customer-payments/{id:long}/confirm", async (long id, ConfirmCustomerPaymentDto? dto, CustomerPaymentService cusService, ITenantContext tc) =>
{
    try
    {
        var payment = await cusService.ConfirmPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã xác nhận phiếu thu #{payment.PaymentNo} thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.ConfirmedBy,
            payment.ConfirmedAt
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Quyết toán chốt ca kế toán dịch vụ vào sổ quỹ (Confirmed -> Settled)
app.MapPost("/api/customer-payments/{id:long}/settle", async (long id, SettleCustomerPaymentDto? dto, CustomerPaymentService cusService, ITenantContext tc) =>
{
    try
    {
        var payment = await cusService.SettlePaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã quyết toán hoàn tất phiếu thu #{payment.PaymentNo} vào sổ quỹ.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.SettledBy,
            payment.SettledAt,
            payment.BankTxnRef
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Hủy phiếu thu tiền dịch vụ và HOÀN TÁC ROLLBACK nợ trên các lệnh RO (SerPaymentDelete)
app.MapPost("/api/customer-payments/{id:long}/cancel", async (long id, CancelCustomerPaymentDto dto, CustomerPaymentService cusService, ITenantContext tc) =>
{
    try
    {
        var payment = await cusService.CancelPaymentAsync(id, tc.OrgId, dto);
        return Results.Ok(new
        {
            message = $"Đã hủy phiếu thu #{payment.PaymentNo} và hoàn tác nợ trên các lệnh sửa chữa RO thành công.",
            payment.Id,
            payment.PaymentNo,
            Status = payment.Status.ToString(),
            payment.CancelledBy,
            payment.CancelledAt,
            payment.CancelReason
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Sinh dữ liệu mẫu in Phiếu Thu Tiền Quyết Toán Dịch Vụ Sửa Chữa (SerPaymentPaperRpt / FrmDebitShow Advice)
app.MapGet("/api/customer-payments/{id:long}/advice", async (long id, CustomerPaymentService cusService, ITenantContext tc) =>
{
    var advice = await cusService.GenerateAdviceAsync(id, tc.OrgId);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy phiếu thu #{id}." });
    return Results.Ok(advice);
});

// 12) Dashboard KPI tổng hợp công nợ và quyết toán thu tiền dịch vụ khách hàng
app.MapGet("/api/customer-payments/summary", async (CustomerPaymentService cusService, ITenantContext tc) =>
{
    var summary = await cusService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ==========================================
// Nghiệp Vụ Quản Lý Thỏa Thuận Hủy Hợp Đồng Mua Bán Xe & Quyết Toán Nghĩa Vụ Tài Chính (BizHTC.Contract / DMS40.Contract)
// Dlr_ContractCancel, Dlr_ContractCancelDtl, Dlr_ContractCancelCar, FrmDMS40_DlrCtr_CancelMinutes
// ==========================================

// 1) Lấy danh sách biên bản thỏa thuận hủy hợp đồng
app.MapGet("/api/contract-cancels", async (
    string? status,
    string? dealer,
    string? settlementType,
    string? query,
    ContractCancellationService cancelService,
    ITenantContext tc) =>
{
    var list = await cancelService.GetListAsync(tc.OrgId, status, dealer, settlementType, query);
    return Results.Ok(list);
});

// 2) Báo cáo KPI tổng hợp số liệu biên bản thỏa thuận hủy & quyết toán tài chính
app.MapGet("/api/contract-cancels/summary", async (ContractCancellationService cancelService, ITenantContext tc) =>
{
    var summary = await cancelService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Lấy chi tiết biên bản thỏa thuận hủy theo ID
app.MapGet("/api/contract-cancels/{id:long}", async (long id, ContractCancellationService cancelService, ITenantContext tc) =>
{
    var item = await cancelService.GetByIdAsync(tc.OrgId, id);
    if (item == null) return Results.NotFound(new { error = $"Không tìm thấy biên bản thỏa thuận hủy #{id}." });
    return Results.Ok(item);
});

// 4) Lấy dữ liệu mẫu in Biên Bản Thỏa Thuận Hủy Hợp Đồng & Thanh Quyết Toán Nghĩa Vụ Tài Chính (kèm đọc số tiền thành chữ)
app.MapGet("/api/contract-cancels/{id:long}/advice", async (long id, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var advice = await cancelService.GenerateAdviceAsync(tc.OrgId, id);
        return Results.Ok(advice);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 5) Tạo mới biên bản thỏa thuận hủy hợp đồng & phương án quyết toán (Dlr_ContractCancel_Save)
app.MapPost("/api/contract-cancels", async (CreateContractCancelDto dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.CreateAsync(tc.OrgId, dto);
        return Results.Ok(entity);
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

// 6) Cập nhật biên bản thỏa thuận hủy ở trạng thái Draft
app.MapPut("/api/contract-cancels/{id:long}", async (long id, UpdateContractCancelDto dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.UpdateAsync(tc.OrgId, id, dto);
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 7) Thêm xe vào biên bản thỏa thuận hủy Draft
app.MapPost("/api/contract-cancels/{id:long}/items", async (long id, ContractCancelItemInputDto itemDto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.AddDetailAsync(tc.OrgId, id, itemDto);
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Xóa xe khỏi biên bản thỏa thuận hủy Draft
app.MapDelete("/api/contract-cancels/{id:long}/items/{detailId:long}", async (long id, long detailId, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.RemoveDetailAsync(tc.OrgId, id, detailId);
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Xóa biên bản thỏa thuận hủy Draft
app.MapDelete("/api/contract-cancels/{id:long}", async (long id, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        await cancelService.DeleteDraftAsync(tc.OrgId, id);
        return Results.Ok(new { message = $"Đã xóa biên bản thỏa thuận hủy #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Trình thẩm định phương án tài chính hủy hợp đồng (Draft -> Submitted)
app.MapPost("/api/contract-cancels/{id:long}/submit", async (long id, SubmitContractCancelDto? dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.SubmitAsync(tc.OrgId, id, dto ?? new SubmitContractCancelDto());
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Lãnh đạo HTC phê duyệt thỏa thuận hủy xe (Dlr_ContractCancel_ApproveMulti)
app.MapPost("/api/contract-cancels/{id:long}/approve", async (long id, ApproveContractCancelDto? dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.ApproveAsync(tc.OrgId, id, dto ?? new ApproveContractCancelDto());
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Kế toán hoàn tất thanh quyết toán tài chính: Chi trả UNC hoàn cọc / phạt cọc / giải phóng bảo lãnh
app.MapPost("/api/contract-cancels/{id:long}/settle", async (long id, SettleContractCancelDto? dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.SettleAsync(tc.OrgId, id, dto ?? new SettleContractCancelDto());
        return Results.Ok(entity);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Từ chối đề nghị thỏa thuận hủy hợp đồng
app.MapPost("/api/contract-cancels/{id:long}/reject", async (long id, RejectContractCancelDto dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.RejectAsync(tc.OrgId, id, dto);
        return Results.Ok(entity);
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

// 14) Hủy biên bản thỏa thuận hủy hợp đồng (Dlr_ContractCancel_CancelMulti)
app.MapPost("/api/contract-cancels/{id:long}/cancel", async (long id, CancelContractCancelDto dto, ContractCancellationService cancelService, ITenantContext tc) =>
{
    try
    {
        var entity = await cancelService.CancelAsync(tc.OrgId, id, dto);
        return Results.Ok(entity);
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

// ==========================================
// QUẢN LÝ HỒ SƠ ĐỀ NGHỊ MƯỢN / BÀN GIAO CHỨNG TỪ GỐC XE Ô TÔ & XÁC NHẬN NGÂN HÀNG (CarDocReq / FrmMngDocReq / FrmNewDocReq)
// ==========================================

// 1) Lấy danh sách hồ sơ đề nghị giao chứng từ gốc kèm bộ lọc
app.MapGet("/api/car-doc-requests", async (
    string? dealer,
    string? bank,
    string? status,
    string? typeCRR,
    string? query,
    CarDocReqService docReqService,
    ITenantContext tc) =>
{
    var list = await docReqService.GetListAsync(tc.OrgId, dealer, bank, status, typeCRR, query);
    return Results.Ok(list);
});

// 2) Lấy chi tiết hồ sơ đề nghị theo ID
app.MapGet("/api/car-doc-requests/{id:long}", async (long id, CarDocReqService docReqService, ITenantContext tc) =>
{
    var item = await docReqService.GetByIdAsync(tc.OrgId, id);
    if (item == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ đề nghị #{id}." });
    return Results.Ok(item);
});

// 3) Lấy chi tiết hồ sơ đề nghị theo Mã đề nghị (DRListCode)
app.MapGet("/api/car-doc-requests/code/{code}", async (string code, CarDocReqService docReqService, ITenantContext tc) =>
{
    var item = await docReqService.GetByCodeAsync(tc.OrgId, code);
    if (item == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ đề nghị '{code}'." });
    return Results.Ok(item);
});

// 4) Lập hồ sơ đề nghị giao chứng từ gốc mới (CarDocReqCreateHTC / CarDocReqCreateDealer)
app.MapPost("/api/car-doc-requests", async (CreateCarDocReqDto dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/car-doc-requests/{item.Id}", item);
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

// 5) Cập nhật hồ sơ đề nghị ở trạng thái Draft
app.MapPut("/api/car-doc-requests/{id:long}", async (long id, UpdateCarDocReqDto dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.UpdateAsync(tc.OrgId, id, dto);
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Thêm xe vào hồ sơ đề nghị Draft
app.MapPost("/api/car-doc-requests/{id:long}/items", async (long id, CarDocReqItemInputDto itemDto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.AddDetailAsync(tc.OrgId, id, itemDto);
        return Results.Ok(item);
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

// 7) Xóa xe khỏi hồ sơ đề nghị Draft
app.MapDelete("/api/car-doc-requests/{id:long}/items/{detailId:long}", async (long id, long detailId, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.RemoveDetailAsync(tc.OrgId, id, detailId);
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8) Xóa hồ sơ đề nghị Draft
app.MapDelete("/api/car-doc-requests/{id:long}", async (long id, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        await docReqService.DeleteDraftAsync(tc.OrgId, id);
        return Results.Ok(new { message = $"Đã xóa hồ sơ đề nghị #{id} thành công." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9) Trình thẩm định hồ sơ đề nghị (Draft -> Pending)
app.MapPost("/api/car-doc-requests/{id:long}/submit", async (long id, SubmitCarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.SubmitAsync(tc.OrgId, id, dto ?? new SubmitCarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10) Chuyên viên kế toán công nợ HTC thẩm định & duyệt cấp 1 (CarDocReqApprove1 / A1)
app.MapPost("/api/car-doc-requests/{id:long}/approve1", async (long id, Approve1CarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.Approve1Async(tc.OrgId, id, dto ?? new Approve1CarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 11) Ngân hàng tài trợ thẩm định & xác nhận chấp thuận bảo lãnh / giải phóng chứng từ xe
app.MapPost("/api/car-doc-requests/{id:long}/bank-approve", async (long id, BankApproveCarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.BankApproveAsync(tc.OrgId, id, dto ?? new BankApproveCarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 12) Lãnh đạo HTC duyệt cấp 2 lệnh xuất két hồ sơ gốc bàn giao (CarDocReqDtlApprove2 / A2)
app.MapPost("/api/car-doc-requests/{id:long}/approve2", async (long id, Approve2CarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.Approve2Async(tc.OrgId, id, dto ?? new Approve2CarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 13) Tự động quét và phê duyệt cấp 2 (AutoApprove2) cho các đề nghị đạt tỷ lệ nghĩa vụ
app.MapPost("/api/car-doc-requests/auto-approve2", async (AutoApprove2CarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    var list = await docReqService.AutoApprove2Async(tc.OrgId, dto ?? new AutoApprove2CarDocReqDto());
    return Results.Ok(new
    {
        message = $"Đã tự động duyệt cấp 2 cho {list.Count} hồ sơ đề nghị đạt điều kiện nghĩa vụ.",
        count = list.Count,
        items = list
    });
});

// 14) Thủ kho xuất giao hồ sơ gốc cho đại diện đại lý / cán bộ ngân hàng ký nhận BBBG (Handover)
app.MapPost("/api/car-doc-requests/{id:long}/handover", async (long id, HandoverCarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.HandoverAsync(tc.OrgId, id, dto ?? new HandoverCarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 15) Đại lý hoàn trả hồ sơ gốc về két bảo quản (với trường hợp mượn Dealer) (Return)
app.MapPost("/api/car-doc-requests/{id:long}/return", async (long id, ReturnCarDocReqDto? dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.ReturnAsync(tc.OrgId, id, dto ?? new ReturnCarDocReqDto());
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 16) Quay lui trạng thái xuất két A2 khi cần điều chỉnh (RevertA2)
app.MapPost("/api/car-doc-requests/{id:long}/revert-a2", async (long id, RevertA2CarDocReqDto dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.RevertA2Async(tc.OrgId, id, dto);
        return Results.Ok(item);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 17) Từ chối hồ sơ đề nghị (CarDocReqDtlReject)
app.MapPost("/api/car-doc-requests/{id:long}/reject", async (long id, RejectCarDocReqDto dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.RejectAsync(tc.OrgId, id, dto);
        return Results.Ok(item);
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

// 18) Hủy hồ sơ đề nghị (CarDocReqListCancel / CarDocReqDtlCancel)
app.MapPost("/api/car-doc-requests/{id:long}/cancel", async (long id, CancelCarDocReqDto dto, CarDocReqService docReqService, ITenantContext tc) =>
{
    try
    {
        var item = await docReqService.CancelAsync(tc.OrgId, id, dto);
        return Results.Ok(item);
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

// 19) Sinh dữ liệu mẫu in Biên bản bàn giao chứng từ gốc CRCarDocReq (Advice)
app.MapGet("/api/car-doc-requests/{id:long}/advice", async (long id, CarDocReqService docReqService, ITenantContext tc) =>
{
    var advice = await docReqService.GenerateAdviceAsync(tc.OrgId, id);
    if (advice == null) return Results.NotFound(new { error = $"Không tìm thấy hồ sơ đề nghị #{id}." });
    return Results.Ok(advice);
});

// 20) Thống kê tổng hợp báo cáo dashboard đề nghị giao chứng từ
app.MapGet("/api/car-doc-requests/summary", async (CarDocReqService docReqService, ITenantContext tc) =>
{
    var summary = await docReqService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// ===== Quản lý Bảng kê Hóa đơn Giá trị Gia tăng (VAT E-Invoice) Bán Buôn Xe Ô Tô Cho Đại Lý (VAT_HTCInvoice / PrintVATService) =====

// 1) Lấy danh sách hóa đơn GTGT bán buôn xe kèm bộ lọc
app.MapGet("/api/htc-invoices", async (
    string? status,
    string? dealer,
    string? bank,
    string? source,
    string? query,
    DateTime? fromDate,
    DateTime? toDate,
    HTCInvoiceService invoiceService,
    ITenantContext tc) =>
{
    var list = await invoiceService.GetListAsync(tc.OrgId, status, dealer, bank, source, query, fromDate, toDate);
    return Results.Ok(list);
});

// 2) Báo cáo dashboard tổng hợp số liệu hóa đơn bán buôn
app.MapGet("/api/htc-invoices/summary", async (HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    var summary = await invoiceService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Chi tiết 1 hóa đơn GTGT theo ID
app.MapGet("/api/htc-invoices/{id:long}", async (long id, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    var invoice = await invoiceService.GetByIdAsync(tc.OrgId, id);
    if (invoice == null) return Results.NotFound(new { error = $"Không tìm thấy hóa đơn #{id}." });
    return Results.Ok(invoice);
});

// 4) Chi tiết 1 hóa đơn GTGT theo mã hóa đơn hệ thống
app.MapGet("/api/htc-invoices/by-code/{code}", async (string code, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    var invoice = await invoiceService.GetByCodeAsync(tc.OrgId, code);
    if (invoice == null) return Results.NotFound(new { error = $"Không tìm thấy hóa đơn mã {code}." });
    return Results.Ok(invoice);
});

// 5) Lập hóa đơn GTGT bán buôn xe mới (Draft)
app.MapPost("/api/htc-invoices", async (CreateHTCInvoiceDto dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.CreateInvoiceAsync(tc.OrgId, dto);
        return Results.Created($"/api/htc-invoices/{invoice.Id}", invoice);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6) Bổ sung xe vào hóa đơn Draft
app.MapPost("/api/htc-invoices/{id:long}/cars", async (long id, AddCarToHTCInvoiceDto dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.AddCarToInvoiceAsync(tc.OrgId, id, dto);
        return Results.Ok(invoice);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 7) Xóa xe khỏi hóa đơn Draft
app.MapDelete("/api/htc-invoices/{id:long}/cars/{detailId:long}", async (long id, long detailId, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.RemoveCarFromInvoiceAsync(tc.OrgId, id, detailId);
        return Results.Ok(invoice);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 8) Kế toán trưởng thẩm định & phê duyệt hóa đơn (Draft -> Approved)
app.MapPost("/api/htc-invoices/{id:long}/approve", async (long id, ApproveHTCInvoiceDto? dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.ApproveInvoiceAsync(tc.OrgId, id, dto ?? new ApproveHTCInvoiceDto());
        return Results.Ok(invoice);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 9) Phát hành hóa đơn GTGT điện tử chính thức & ký số CA (Approved -> Issued)
app.MapPost("/api/htc-invoices/{id:long}/issue", async (long id, IssueHTCInvoiceDto? dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.IssueInvoiceAsync(tc.OrgId, id, dto ?? new IssueHTCInvoiceDto());
        return Results.Ok(invoice);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 10) Ghi nhận thanh toán tiền từ đại lý qua UNC ngân hàng
app.MapPost("/api/htc-invoices/{id:long}/record-payment", async (long id, RecordHTCInvoicePaymentDto dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.RecordPaymentAsync(tc.OrgId, id, dto);
        return Results.Ok(invoice);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 11) Thu hồi / Hủy hóa đơn GTGT theo thỏa thuận 2 bên (FrmThuHoiHDHTC)
app.MapPost("/api/htc-invoices/{id:long}/revoke", async (long id, RevokeHTCInvoiceDto dto, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var invoice = await invoiceService.RevokeInvoiceAsync(tc.OrgId, id, dto);
        return Results.Ok(invoice);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 12) Sinh dữ liệu mẫu in Hóa Đơn Giá Trị Gia Tăng Điện Tử (Advice TT78/2021/TT-BTC)
app.MapGet("/api/htc-invoices/{id:long}/advice", async (long id, HTCInvoiceService invoiceService, ITenantContext tc) =>
{
    try
    {
        var advice = await invoiceService.GenerateInvoiceAdviceAsync(tc.OrgId, id);
        return Results.Ok(advice);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// ===== Quản lý Thư Tín Dụng Nhập Khẩu (Letter of Credit - LC) thanh toán hợp đồng ngoại (CT_LC / FrmMngLC / FrmNewLC) =====

// 1) Lấy danh sách LC kèm bộ lọc
app.MapGet("/api/letters-of-credit", async (
    string? status,
    string? bank,
    string? contractNo,
    string? query,
    DateTime? fromDate,
    DateTime? toDate,
    LetterOfCreditService lcService,
    ITenantContext tc) =>
{
    var list = await lcService.GetListAsync(tc.OrgId, status, bank, contractNo, query, fromDate, toDate);
    return Results.Ok(list);
});

// 2) Báo cáo dashboard tổng hợp số liệu LC
app.MapGet("/api/letters-of-credit/summary", async (LetterOfCreditService lcService, ITenantContext tc) =>
{
    var summary = await lcService.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Chi tiết 1 LC theo ID
app.MapGet("/api/letters-of-credit/{id:long}", async (long id, LetterOfCreditService lcService, ITenantContext tc) =>
{
    var lc = await lcService.GetByIdAsync(tc.OrgId, id);
    if (lc == null) return Results.NotFound(new { error = $"Không tìm thấy LC #{id}." });
    return Results.Ok(lc);
});

// 4) Chi tiết 1 LC theo số LC
app.MapGet("/api/letters-of-credit/by-no/{lcNo}", async (string lcNo, LetterOfCreditService lcService, ITenantContext tc) =>
{
    var lc = await lcService.GetByNoAsync(tc.OrgId, lcNo);
    if (lc == null) return Results.NotFound(new { error = $"Không tìm thấy LC số {lcNo}." });
    return Results.Ok(lc);
});

// 5) Mở (lập) LC mới cho hợp đồng ngoại (ContractLCCreate / FrmNewLC)
app.MapPost("/api/letters-of-credit", async (CreateLetterOfCreditDto dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/letters-of-credit/{lc.Id}", lc);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 6) Bổ sung xe nhập khẩu vào LC Draft
app.MapPost("/api/letters-of-credit/{id:long}/cars", async (long id, AddCarToLetterOfCreditDto dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.AddCarAsync(tc.OrgId, id, dto);
        return Results.Ok(lc);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 7) Xóa xe khỏi LC Draft
app.MapDelete("/api/letters-of-credit/{id:long}/cars/{detailId:long}", async (long id, long detailId, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.RemoveCarAsync(tc.OrgId, id, detailId);
        return Results.Ok(lc);
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 8) Trình ngân hàng phát hành LC (Draft -> Opened)
app.MapPost("/api/letters-of-credit/{id:long}/open", async (long id, OpenLetterOfCreditDto? dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.OpenAsync(tc.OrgId, id, dto ?? new OpenLetterOfCreditDto());
        return Results.Ok(lc);
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 9) Tu chỉnh điều khoản LC (Opened -> Amended)
app.MapPost("/api/letters-of-credit/{id:long}/amend", async (long id, AmendLetterOfCreditDto dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.AmendAsync(tc.OrgId, id, dto);
        return Results.Ok(lc);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 10) Xuất trình bộ chứng từ sử dụng LC (Opened/Amended -> Utilized)
app.MapPost("/api/letters-of-credit/{id:long}/present", async (long id, PresentLetterOfCreditDto? dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.PresentAsync(tc.OrgId, id, dto ?? new PresentLetterOfCreditDto());
        return Results.Ok(lc);
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 11) Tất toán LC (Utilized -> Settled)
app.MapPost("/api/letters-of-credit/{id:long}/settle", async (long id, SettleLetterOfCreditDto? dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.SettleAsync(tc.OrgId, id, dto ?? new SettleLetterOfCreditDto());
        return Results.Ok(lc);
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 12) Hủy LC
app.MapPost("/api/letters-of-credit/{id:long}/cancel", async (long id, CancelLetterOfCreditDto dto, LetterOfCreditService lcService, ITenantContext tc) =>
{
    try
    {
        var lc = await lcService.CancelAsync(tc.OrgId, id, dto);
        return Results.Ok(lc);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// ===== Quản lý Danh mục Ngân hàng - Đại lý (Bank-Dealer authorization master data) =====
// Tương ứng Mst_BankDealer trong hệ nguồn 2010.HTC (Biz.HTC.WH.cs) và màn hình FrmDealerBank.

// 1) Lấy danh sách Ngân hàng - Đại lý kèm bộ lọc (Mst_BankDealer_Get)
app.MapGet("/api/bank-dealers", async (
    string? dealerCode,
    string? bankCode,
    string? status,
    bool? flagBankGrt,
    bool? flagBankPmt,
    string? query,
    BankDealerService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, dealerCode, bankCode, status, flagBankGrt, flagBankPmt, query);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.DealerCode,
        x.DealerName,
        x.BankCode,
        x.BankName,
        x.CreditContractNo,
        x.CreditContractDate,
        x.CreditAmount,
        x.BankBranchCode,
        x.BankBranchName,
        x.FlagBankGrt,
        x.FlagBankPmt,
        status = x.Status.ToString(),
        x.Remark,
        x.CreatedBy,
        x.CreatedAt,
        x.UpdatedBy,
        x.UpdatedAt
    }));
});

// 2) Báo cáo dashboard tổng hợp danh mục Ngân hàng - Đại lý
app.MapGet("/api/bank-dealers/summary", async (BankDealerService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Chi tiết 1 dòng Ngân hàng - Đại lý theo ID
app.MapGet("/api/bank-dealers/{id:long}", async (long id, BankDealerService svc, ITenantContext tc) =>
{
    var entity = await svc.GetByIdAsync(tc.OrgId, id);
    if (entity == null) return Results.NotFound(new { error = $"Không tìm thấy dòng Ngân hàng - Đại lý #{id}." });
    return Results.Ok(entity);
});

// 4) Tạo mới 1 dòng Ngân hàng - Đại lý (Mst_BankDealer_Create)
app.MapPost("/api/bank-dealers", async (CreateBankDealerDto dto, BankDealerService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/bank-dealers/{entity.Id}", entity);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 5) Cập nhật 1 dòng Ngân hàng - Đại lý (Mst_BankDealer_Update)
app.MapPut("/api/bank-dealers/{id:long}", async (long id, UpdateBankDealerDto dto, BankDealerService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.UpdateAsync(tc.OrgId, id, dto);
        return Results.Ok(entity);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// 6) Xóa 1 dòng Ngân hàng - Đại lý (Mst_BankDealer_Delete)
app.MapDelete("/api/bank-dealers/{id:long}", async (long id, BankDealerService svc, ITenantContext tc) =>
{
    var ok = await svc.DeleteAsync(tc.OrgId, id);
    if (!ok) return Results.NotFound(new { error = $"Không tìm thấy dòng Ngân hàng - Đại lý #{id}." });
    return Results.Ok(new { deleted = true, id });
});

// ===== Cập nhật Chứng từ Kế toán (Accounting Voucher / Accounting Record No bulk update) =====
// Tương ứng FrmUpdateChungTuKT + SalesService.UpdateCTKT + Pmt_Payment_UpdateFinancial trong hệ nguồn 2010.HTC.

// 1) Lấy danh sách lô cập nhật chứng từ kế toán kèm bộ lọc
app.MapGet("/api/accounting-vouchers", async (
    string? status,
    string? query,
    DateTime? fromDate,
    DateTime? toDate,
    AccountingVoucherService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, status, query, fromDate, toDate);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.BatchNo,
        x.Description,
        x.TotalItems,
        x.UpdatedItems,
        x.SkippedItems,
        status = x.Status.ToString(),
        x.CreatedBy,
        x.CreatedAt,
        x.AppliedBy,
        x.AppliedAt,
        x.Remark,
        details = x.Details.Select(d => new
        {
            d.Id,
            d.PaymentNo,
            d.OldAccountingRecordNo,
            d.NewAccountingRecordNo,
            status = d.Status.ToString(),
            d.Note
        })
    }));
});

// 2) Báo cáo dashboard tổng hợp các lô cập nhật chứng từ kế toán
app.MapGet("/api/accounting-vouchers/summary", async (AccountingVoucherService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 3) Chi tiết 1 lô cập nhật chứng từ kế toán theo ID
app.MapGet("/api/accounting-vouchers/{id:long}", async (long id, AccountingVoucherService svc, ITenantContext tc) =>
{
    var entity = await svc.GetByIdAsync(tc.OrgId, id);
    if (entity == null) return Results.NotFound(new { error = $"Không tìm thấy lô cập nhật chứng từ #{id}." });
    return Results.Ok(entity);
});

// 4) Lập lô cập nhật chứng từ kế toán mới (FrmUpdateChungTuKT)
app.MapPost("/api/accounting-vouchers", async (CreateAccountingVoucherUpdateDto dto, AccountingVoucherService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/accounting-vouchers/{entity.Id}", entity);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 5) Áp dụng lô cập nhật chứng từ kế toán (Pmt_Payment_UpdateFinancial)
app.MapPost("/api/accounting-vouchers/{id:long}/apply", async (long id, ApplyAccountingVoucherUpdateDto? dto, AccountingVoucherService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.ApplyAsync(tc.OrgId, id, dto);
        return Results.Ok(entity);
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 6) Hủy lô cập nhật chứng từ kế toán
app.MapPost("/api/accounting-vouchers/{id:long}/cancel", async (long id, AccountingVoucherService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CancelAsync(tc.OrgId, id);
        return Results.Ok(entity);
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// ===== Lịch Làm Việc & Hạn Nộp Cọc (Payment / Deposit Duty Calendar - Mst_Calendar) =====
// Tương ứng Mst_Calendar trong hệ nguồn 2010.HTC (BizHTC.MasterData.cs) và
// mySql_GetClauseSelect_Mst_Calendar_GetForDayT (BizHTC.Common.cs).

// 1) Lấy danh sách ngày trong lịch làm việc kèm bộ lọc (Mst_Calendar_Get)
app.MapGet("/api/calendar", async (
    string? calendarType,
    DateTime? fromDate,
    DateTime? toDate,
    string? statusValue,
    PaymentCalendarService svc,
    ITenantContext tc) =>
{
    CalendarDayStatus? st = null;
    if (!string.IsNullOrWhiteSpace(statusValue) && Enum.TryParse<CalendarDayStatus>(statusValue, true, out var parsed))
        st = parsed;

    var list = await svc.GetListAsync(tc.OrgId, calendarType, fromDate, toDate, st);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.CalendarType,
        x.Date,
        statusValue = x.StatusValue.ToString(),
        x.Remark,
        x.CreatedBy,
        x.CreatedAt,
        x.UpdatedBy,
        x.UpdatedAt
    }));
});

// 2) Báo cáo dashboard tổng hợp lịch làm việc
app.MapGet("/api/calendar/summary", async (int? year, PaymentCalendarService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId, year);
    return Results.Ok(summary);
});

// 3) Tính hạn nộp cọc cho các ngày làm việc kể từ ngày mốc (Mst_Calendar_GetForDepositDuty)
app.MapGet("/api/calendar/deposit-duty", async (DateTime fromDate, int? dayT, PaymentCalendarService svc, ITenantContext tc) =>
{
    var result = await svc.GetForDepositDutyAsync(tc.OrgId, fromDate, dayT);
    return Results.Ok(result);
});

// 4) Sinh lại lịch 1 năm theo trạng thái mặc định từng thứ (Mst_Calendar_ResetYear)
app.MapPost("/api/calendar/reset-year", async (ResetCalendarYearDto dto, PaymentCalendarService svc, ITenantContext tc) =>
{
    try
    {
        int count = await svc.ResetYearAsync(tc.OrgId, dto);
        return Results.Ok(new { year = dto.Year, generatedDays = count });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 5) Đổi trạng thái 1 ngày trong lịch (Mst_Calendar_UpdateStatusValue)
app.MapPost("/api/calendar/update-status", async (UpdateCalendarDayDto dto, PaymentCalendarService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.UpdateStatusValueAsync(tc.OrgId, dto);
        return Results.Ok(new
        {
            entity.Id,
            entity.CalendarType,
            entity.Date,
            statusValue = entity.StatusValue.ToString(),
            entity.Remark,
            entity.UpdatedBy,
            entity.UpdatedAt
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
});

// ===== Duyệt tự động Thanh toán theo Sổ phụ Ngân hàng (Bank Statement Auto-Approve / TCF) =====
// Tương ứng FrmMngPM_ApproveAuto + FrmMngPM.btnAutoAppA/btnAutoAppF + LayTTSoPhu
// + SalesService.OS_DMS_TCF_WA_Bank_BankStatementDtl_Get / PaymentPaymentApprove_Approve /
// PaymentPaymentConfirm_MultiAndpushTCF trong hệ nguồn 2010.HTC.

// 1) Chạy duyệt tự động thanh toán theo sổ phụ ngân hàng (Duyệt tự động A/F)
app.MapPost("/api/auto-approve/run", async (RunAutoApproveDto dto, BankStatementAutoApproveService svc, ITenantContext tc) =>
{
    try
    {
        var batch = await svc.RunAsync(tc.OrgId, dto);
        return Results.Ok(new
        {
            batch.Id,
            batch.BatchNo,
            mode = batch.Mode.ToString(),
            channel = batch.Channel.ToString(),
            batch.BankCode,
            batch.AccountNo,
            batch.StatementFrom,
            batch.StatementTo,
            batch.TotalRecords,
            batch.MatchedCount,
            batch.ApprovedCount,
            batch.SkippedCount,
            batch.TotalAmount,
            batch.MatchedAmount,
            status = batch.Status.ToString(),
            batch.CreatedBy,
            batch.CreatedAt,
            batch.CompletedAt,
            batch.Remark,
            details = batch.Details.Select(d => new
            {
                d.Id,
                d.BankTxnNo,
                d.TxnTime,
                d.Amount,
                d.SenderAccount,
                d.ReceiverAccount,
                d.Remark,
                d.PaymentNo,
                d.DealerCode,
                d.AccountingRecordNo,
                d.TcfAutoId,
                d.TcfBsInputNo,
                d.TcfMaGiaoDich,
                matchStatus = d.MatchStatus.ToString(),
                d.DiscrepancyReason,
                d.MatchedAt
            })
        });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 2) Lấy danh sách các lô duyệt tự động kèm bộ lọc
app.MapGet("/api/auto-approve/batches", async (
    string? status,
    string? mode,
    string? bankCode,
    DateTime? fromDate,
    DateTime? toDate,
    BankStatementAutoApproveService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, status, mode, bankCode, fromDate, toDate);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.BatchNo,
        mode = x.Mode.ToString(),
        channel = x.Channel.ToString(),
        x.BankCode,
        x.AccountNo,
        x.StatementFrom,
        x.StatementTo,
        x.TotalRecords,
        x.MatchedCount,
        x.ApprovedCount,
        x.SkippedCount,
        x.TotalAmount,
        x.MatchedAmount,
        status = x.Status.ToString(),
        x.CreatedBy,
        x.CreatedAt,
        x.CompletedAt,
        x.Remark,
        details = x.Details.Select(d => new
        {
            d.Id,
            d.BankTxnNo,
            d.TxnTime,
            d.Amount,
            d.SenderAccount,
            d.ReceiverAccount,
            d.Remark,
            d.PaymentNo,
            d.DealerCode,
            d.AccountingRecordNo,
            d.TcfAutoId,
            d.TcfBsInputNo,
            d.TcfMaGiaoDich,
            matchStatus = d.MatchStatus.ToString(),
            d.DiscrepancyReason,
            d.MatchedAt
        })
    }));
});

// 3) Báo cáo dashboard tổng hợp các lô duyệt tự động
app.MapGet("/api/auto-approve/summary", async (BankStatementAutoApproveService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 4) Chi tiết 1 lô duyệt tự động theo ID
app.MapGet("/api/auto-approve/batches/{id:long}", async (long id, BankStatementAutoApproveService svc, ITenantContext tc) =>
{
    var batch = await svc.GetByIdAsync(tc.OrgId, id);
    if (batch == null) return Results.NotFound(new { error = $"Không tìm thấy lô duyệt tự động #{id}." });
    return Results.Ok(batch);
});

// 5) Hủy lô duyệt tự động (chỉ khi còn Draft)
app.MapPost("/api/auto-approve/batches/{id:long}/cancel", async (long id, BankStatementAutoApproveService svc, ITenantContext tc) =>
{
    try
    {
        var batch = await svc.CancelAsync(tc.OrgId, id);
        return Results.Ok(new { batch.Id, batch.BatchNo, status = batch.Status.ToString() });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// ===== Quản lý Hợp đồng Mua bán Xe Đại lý (Dealer Sales Contract - DMS40_CT_DealerContract) =====

// 1) Lập hợp đồng đại lý mới (DMS40_CT_DealerContract_Save)
app.MapPost("/api/dealer-contracts", async (CreateDealerContractDto dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/dealer-contracts/{entity.Id}", new
        {
            entity.Id,
            entity.DlrCtrNo,
            dcpType = entity.DCPType.ToString(),
            entity.DealerCode,
            entity.DealerName,
            entity.BankCodeMD,
            entity.BankNameMD,
            entity.ContractDate,
            entity.TotalVehicles,
            entity.TotalAmount,
            dlrSignStatus = entity.DlrSignStatus.ToString(),
            htcSignStatus = entity.HTCSignStatus.ToString(),
            status = entity.DlrCtrStatus.ToString(),
            entity.Remark,
            entity.CreatedBy,
            entity.CreatedAt,
            details = entity.Details.Select(d => new
            {
                d.Id,
                d.CarId,
                d.VIN,
                d.ModelCode,
                d.ModelName,
                d.SpecCode,
                d.ColorCode,
                d.ColorName,
                d.OriginNo,
                d.ProductionYear,
                d.UnitPrice,
                status = d.DlrCtrStatusDtl.ToString(),
                d.Remark
            })
        });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 2) Danh sách hợp đồng đại lý (DMS40_CT_DealerContract_Get)
app.MapGet("/api/dealer-contracts", async (
    string? dlrCtrNo,
    string? dealerCode,
    string? bankCodeMD,
    string? dcpType,
    string? dlrSignStatus,
    string? htcSignStatus,
    string? status,
    string? query,
    DateTime? fromDate,
    DateTime? toDate,
    DealerContractService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, dlrCtrNo, dealerCode, bankCodeMD, dcpType, dlrSignStatus, htcSignStatus, status, query, fromDate, toDate);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.DlrCtrNo,
        dcpType = x.DCPType.ToString(),
        x.DealerCode,
        x.DealerName,
        x.BankCodeMD,
        x.BankNameMD,
        x.ContractDate,
        x.TotalVehicles,
        x.TotalAmount,
        x.FlagDlrCtrAdjust,
        dlrSignStatus = x.DlrSignStatus.ToString(),
        htcSignStatus = x.HTCSignStatus.ToString(),
        status = x.DlrCtrStatus.ToString(),
        x.Remark,
        x.CreatedBy,
        x.CreatedAt,
        x.DlrApprovedBy,
        x.DlrApprovedAt,
        x.HTCApproved1By,
        x.HTCApproved1At,
        x.HTCApproved2By,
        x.HTCApproved2At,
        x.RejectedBy,
        x.RejectedAt,
        x.CancelledAt
    }));
});

// 3) Báo cáo dashboard tổng hợp hợp đồng đại lý
app.MapGet("/api/dealer-contracts/summary", async (DealerContractService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 4) Chi tiết 1 hợp đồng đại lý kèm danh sách xe (DMS40_CT_DealerContract_Get)
app.MapGet("/api/dealer-contracts/{id:long}", async (long id, DealerContractService svc, ITenantContext tc) =>
{
    var entity = await svc.GetByIdAsync(tc.OrgId, id);
    if (entity == null) return Results.NotFound(new { error = $"Không tìm thấy hợp đồng đại lý #{id}." });
    return Results.Ok(new
    {
        entity.Id,
        entity.DlrCtrNo,
        dcpType = entity.DCPType.ToString(),
        entity.DealerCode,
        entity.DealerName,
        entity.BankCodeMD,
        entity.BankNameMD,
        entity.ContractDate,
        entity.TotalVehicles,
        entity.TotalAmount,
        entity.FilePath,
        entity.FlagDlrCtrAdjust,
        dlrSignStatus = entity.DlrSignStatus.ToString(),
        htcSignStatus = entity.HTCSignStatus.ToString(),
        status = entity.DlrCtrStatus.ToString(),
        entity.Remark,
        entity.CreatedBy,
        entity.CreatedAt,
        entity.UpdatedBy,
        entity.UpdatedAt,
        entity.DlrApprovedBy,
        entity.DlrApprovedAt,
        entity.HTCApproved1By,
        entity.HTCApproved1At,
        entity.HTCApproved2By,
        entity.HTCApproved2At,
        entity.RejectedBy,
        entity.RejectedAt,
        entity.CancelledBy,
        entity.CancelledAt,
        details = entity.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.CarId,
            d.VIN,
            d.ModelCode,
            d.ModelName,
            d.SpecCode,
            d.ColorCode,
            d.ColorName,
            d.OriginNo,
            d.ProductionYear,
            d.UnitPrice,
            status = d.DlrCtrStatusDtl.ToString(),
            d.Remark
        })
    });
});

// 5) Đại lý ký hợp đồng (DMS40_CT_DealerContract_DlrApprove)
app.MapPost("/api/dealer-contracts/{id:long}/dlr-approve", async (long id, ApproveDealerContractDto? dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.DlrApproveAsync(tc.OrgId, id, dto ?? new ApproveDealerContractDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.DlrCtrNo,
            dlrSignStatus = entity.DlrSignStatus.ToString(),
            status = entity.DlrCtrStatus.ToString(),
            entity.DlrApprovedBy,
            entity.DlrApprovedAt
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 6) HTC duyệt cấp 1 (DMS40_CT_DealerContract_HTCApprove1)
app.MapPost("/api/dealer-contracts/{id:long}/htc-approve1", async (long id, ApproveDealerContractDto? dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.HTCApprove1Async(tc.OrgId, id, dto ?? new ApproveDealerContractDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.DlrCtrNo,
            htcSignStatus = entity.HTCSignStatus.ToString(),
            status = entity.DlrCtrStatus.ToString(),
            entity.HTCApproved1By,
            entity.HTCApproved1At
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 7) HTC duyệt cấp 2 — hoàn tất ký hợp đồng (DMS40_CT_DealerContract_HTCApprove2)
app.MapPost("/api/dealer-contracts/{id:long}/htc-approve2", async (long id, ApproveDealerContractDto? dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.HTCApprove2Async(tc.OrgId, id, dto ?? new ApproveDealerContractDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.DlrCtrNo,
            htcSignStatus = entity.HTCSignStatus.ToString(),
            status = entity.DlrCtrStatus.ToString(),
            entity.HTCApproved2By,
            entity.HTCApproved2At
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 8) HTC từ chối hợp đồng (DMS40_CT_DealerContract_HTCReject)
app.MapPost("/api/dealer-contracts/{id:long}/htc-reject", async (long id, RejectDealerContractDto dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.HTCRejectAsync(tc.OrgId, id, dto);
        return Results.Ok(new
        {
            entity.Id,
            entity.DlrCtrNo,
            htcSignStatus = entity.HTCSignStatus.ToString(),
            entity.RejectedBy,
            entity.RejectedAt,
            entity.Remark
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 9) Đại lý hủy hợp đồng (DMS40_CT_DealerContract_DlrCancel)
app.MapPost("/api/dealer-contracts/{id:long}/dlr-cancel", async (long id, CancelDealerContractDto? dto, DealerContractService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.DlrCancelAsync(tc.OrgId, id, dto ?? new CancelDealerContractDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.DlrCtrNo,
            dlrSignStatus = entity.DlrSignStatus.ToString(),
            status = entity.DlrCtrStatus.ToString(),
            entity.CancelledBy,
            entity.CancelledAt
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// ===== Yêu cầu Điều chuyển Vận tải Kho (Storage Rearrange Transport Request - Sto_StorageRearrange / FrmMngSC) =====

// 1) Lập lệnh điều chuyển kho mới (StorageStorageRearrangeCreate)
app.MapPost("/api/storage-rearranges", async (CreateStorageRearrangeDto dto, StorageRearrangeService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/storage-rearranges/{entity.Id}", new
        {
            entity.Id,
            entity.StorageRearrangeNo,
            status = entity.RearrangeStatus.ToString(),
            entity.Remark,
            entity.CreatedBy,
            entity.CreatedAt,
            totalVehicles = entity.Details.Count,
            details = entity.Details.Select(d => new
            {
                d.Id,
                d.VIN,
                d.StorageCodeFrom,
                d.StorageCodeTo,
                d.ExpectedStartDate,
                d.ExpectedEndDate,
                status = d.RearrangeDtlStatus.ToString(),
                d.Remark
            })
        });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 2) Danh sách lệnh điều chuyển kho (StorageStorageRearrangeGet)
app.MapGet("/api/storage-rearranges", async (
    string? storageRearrangeNo,
    string? status,
    string? storageCodeTo,
    string? query,
    DateTime? fromDate,
    DateTime? toDate,
    StorageRearrangeService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, storageRearrangeNo, status, storageCodeTo, query, fromDate, toDate);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.StorageRearrangeNo,
        status = x.RearrangeStatus.ToString(),
        x.Remark,
        x.CreatedBy,
        x.CreatedAt,
        x.ApprovedBy1,
        x.ApprovedAt1,
        x.ApprovedBy2,
        x.ApprovedAt2,
        totalVehicles = x.Details.Count
    }));
});

// 3) Báo cáo dashboard tổng hợp lệnh điều chuyển kho
app.MapGet("/api/storage-rearranges/summary", async (StorageRearrangeService svc, ITenantContext tc) =>
{
    var summary = await svc.GetSummaryAsync(tc.OrgId);
    return Results.Ok(summary);
});

// 4) Chi tiết 1 lệnh điều chuyển kho kèm danh sách VIN (StorageStorageRearrangeGet)
app.MapGet("/api/storage-rearranges/{id:long}", async (long id, StorageRearrangeService svc, ITenantContext tc) =>
{
    var entity = await svc.GetByIdAsync(tc.OrgId, id);
    if (entity == null) return Results.NotFound(new { error = $"Không tìm thấy lệnh điều chuyển #{id}." });
    return Results.Ok(new
    {
        entity.Id,
        entity.StorageRearrangeNo,
        status = entity.RearrangeStatus.ToString(),
        entity.Remark,
        entity.CreatedBy,
        entity.CreatedAt,
        entity.ApprovedBy1,
        entity.ApprovedAt1,
        entity.ApprovedBy2,
        entity.ApprovedAt2,
        totalVehicles = entity.Details.Count,
        details = entity.Details.OrderBy(d => d.Id).Select(d => new
        {
            d.Id,
            d.VIN,
            d.StorageCodeFrom,
            d.StorageCodeTo,
            d.ExpectedStartDate,
            d.ExpectedEndDate,
            status = d.RearrangeDtlStatus.ToString(),
            d.ConfirmBy,
            d.ConfirmDate,
            d.Remark
        })
    });
});

// 5) Duyệt cấp 1 lệnh điều chuyển (StorageStorageRearrangeApprove1)
app.MapPost("/api/storage-rearranges/{id:long}/approve1", async (long id, ApproveStorageRearrangeDto? dto, StorageRearrangeService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.Approve1Async(tc.OrgId, id, dto ?? new ApproveStorageRearrangeDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.StorageRearrangeNo,
            status = entity.RearrangeStatus.ToString(),
            entity.ApprovedBy1,
            entity.ApprovedAt1
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 6) Duyệt cấp 2 — hoàn tất lệnh điều chuyển (StorageStorageRearrangeApprove2)
app.MapPost("/api/storage-rearranges/{id:long}/approve2", async (long id, ApproveStorageRearrangeDto? dto, StorageRearrangeService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.Approve2Async(tc.OrgId, id, dto ?? new ApproveStorageRearrangeDto());
        return Results.Ok(new
        {
            entity.Id,
            entity.StorageRearrangeNo,
            status = entity.RearrangeStatus.ToString(),
            entity.ApprovedBy2,
            entity.ApprovedAt2
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 7) Từ chối lệnh điều chuyển (StorageStorageRearrangeApprove1/2 với FlagUnapprove=Active)
app.MapPost("/api/storage-rearranges/{id:long}/reject", async (long id, RejectStorageRearrangeDto dto, StorageRearrangeService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.RejectAsync(tc.OrgId, id, dto);
        return Results.Ok(new
        {
            entity.Id,
            entity.StorageRearrangeNo,
            status = entity.RearrangeStatus.ToString(),
            entity.Remark
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 8) Cập nhật ngày giao xe dự kiến / ghi chú 1 dòng VIN (StorageStorageRearrangeDetailUpdate)
app.MapPost("/api/storage-rearranges/{id:long}/update-detail", async (long id, UpdateStorageRearrangeDetailDto dto, StorageRearrangeService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.UpdateDetailAsync(tc.OrgId, id, dto);
        return Results.Ok(new
        {
            entity.Id,
            entity.StorageRearrangeNo,
            status = entity.RearrangeStatus.ToString(),
            details = entity.Details.OrderBy(d => d.Id).Select(d => new
            {
                d.Id,
                d.VIN,
                d.StorageCodeFrom,
                d.StorageCodeTo,
                d.ExpectedStartDate,
                d.ExpectedEndDate,
                status = d.RearrangeDtlStatus.ToString(),
                d.Remark
            })
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// ===== Phiên bản Cước phí Vận tải (Transport Fee Version - Mst_TranspFeeVer / Mst_TranspFee) =====

// 1) Lập phiên bản cước phí vận tải mới (Mst_TranspFeeVerGetCreate)
app.MapPost("/api/transport-fee-versions", async (CreateTransportFeeVersionDto dto, TransportFeeVersionService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.CreateAsync(tc.OrgId, dto);
        return Results.Created($"/api/transport-fee-versions/{entity.Id}", new
        {
            entity.Id,
            entity.TFVCode,
            entity.Description,
            status = entity.Status.ToString(),
            entity.CreatedDate,
            entity.Remark,
            entity.CreatedBy,
            entity.CreatedAt,
            rateCount = entity.Rates.Count,
            totalFeeValue = entity.Rates.Sum(r => r.ValFee)
        });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 2) Danh sách phiên bản cước phí vận tải (Mst_TranspFeeVerGet)
app.MapGet("/api/transport-fee-versions", async (
    string? tfvCode,
    string? status,
    string? transporterCode,
    string? modelCode,
    string? query,
    TransportFeeVersionService svc,
    ITenantContext tc) =>
{
    var list = await svc.GetListAsync(tc.OrgId, tfvCode, status, transporterCode, modelCode, query);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.TFVCode,
        x.Description,
        status = x.Status.ToString(),
        x.CreatedDate,
        x.AppliedDate,
        x.Remark,
        x.CreatedBy,
        x.CreatedAt,
        rateCount = x.Rates.Count,
        totalFeeValue = x.Rates.Sum(r => r.ValFee)
    }));
});

// 3) Báo cáo dashboard tổng hợp phiên bản cước phí vận tải
app.MapGet("/api/transport-fee-versions/summary", async (TransportFeeVersionService svc, ITenantContext tc) =>
{
    return Results.Ok(await svc.GetSummaryAsync(tc.OrgId));
});

// 4) Chi tiết 1 phiên bản cước phí kèm danh sách dòng cước (Mst_TranspFeeVerGet + Mst_TranspFee)
app.MapGet("/api/transport-fee-versions/{id:long}", async (long id, TransportFeeVersionService svc, ITenantContext tc) =>
{
    var entity = await svc.GetByIdAsync(tc.OrgId, id);
    if (entity == null) return Results.NotFound(new { error = $"Không tìm thấy phiên bản cước phí #{id}." });
    return Results.Ok(new
    {
        entity.Id,
        entity.TFVCode,
        entity.Description,
        status = entity.Status.ToString(),
        entity.CreatedDate,
        entity.AppliedDate,
        entity.Remark,
        entity.CreatedBy,
        entity.CreatedAt,
        entity.UpdatedBy,
        entity.UpdatedAt,
        rateCount = entity.Rates.Count,
        totalFeeValue = entity.Rates.Sum(r => r.ValFee),
        rates = entity.Rates.OrderBy(r => r.ProvinceCodeFrom).ThenBy(r => r.TransporterCode).ThenBy(r => r.ModelCode).Select(r => new
        {
            r.Id,
            r.ProvinceCodeFrom,
            r.ProvinceNameFrom,
            r.DistrictCodeFrom,
            r.DistrictNameFrom,
            r.ProvinceCodeTo,
            r.ProvinceNameTo,
            r.DistrictCodeTo,
            r.DistrictNameTo,
            r.TransporterCode,
            r.TransporterName,
            r.ModelCode,
            r.ModelName,
            r.ValFee,
            r.ExpectedDays,
            r.Remark
        })
    });
});

// 5) Cập nhật mô tả / ghi chú phiên bản cước (chỉ khi Draft)
app.MapPut("/api/transport-fee-versions/{id:long}", async (long id, UpdateTransportFeeVersionDto dto, TransportFeeVersionService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.UpdateAsync(tc.OrgId, id, dto);
        return Results.Ok(new
        {
            entity.Id,
            entity.TFVCode,
            entity.Description,
            status = entity.Status.ToString(),
            entity.Remark,
            entity.UpdatedBy,
            entity.UpdatedAt
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 6) Áp dụng phiên bản cước (Draft -> Active)
app.MapPost("/api/transport-fee-versions/{id:long}/apply", async (long id, ApplyTransportFeeVersionDto? dto, TransportFeeVersionService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.ApplyAsync(tc.OrgId, id, dto?.AppliedBy);
        return Results.Ok(new
        {
            entity.Id,
            entity.TFVCode,
            status = entity.Status.ToString(),
            entity.AppliedDate,
            entity.UpdatedBy
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 7) Ngừng áp dụng phiên bản cước (Active -> Inactive)
app.MapPost("/api/transport-fee-versions/{id:long}/deactivate", async (long id, ApplyTransportFeeVersionDto? dto, TransportFeeVersionService svc, ITenantContext tc) =>
{
    try
    {
        var entity = await svc.DeactivateAsync(tc.OrgId, id, dto?.AppliedBy);
        return Results.Ok(new
        {
            entity.Id,
            entity.TFVCode,
            status = entity.Status.ToString(),
            entity.UpdatedBy,
            entity.UpdatedAt
        });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
});

// 8) Xóa phiên bản cước phí (Mst_TranspFeeVerDel)
app.MapDelete("/api/transport-fee-versions/{id:long}", async (long id, TransportFeeVersionService svc, ITenantContext tc) =>
{
    try
    {
        bool ok = await svc.DeleteAsync(tc.OrgId, id);
        if (!ok) return Results.NotFound(new { error = $"Không tìm thấy phiên bản cước phí #{id}." });
        return Results.Ok(new { deleted = true, id });
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
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

record CreatePaymentAVNDto(
    string? PaymentAVNNo,
    string PmtMonth,
    string? SupplierCode,
    string? SupplierName,
    decimal? VATRate,
    string? Remark,
    string? CreatedBy,
    List<PaymentAVNItemInputDto> Items
);
record ApprovePaymentAVNDto(string? ApproverName);
record SignPaymentAVNDto(string? SignerName, string? FilePath);
record SettlePaymentAVNDto(string? BankTxnRef, string? SettledBy);
record RejectPaymentAVNDto(string Reason, string? RejecterName);
record CancelPaymentAVNDto(string? Reason);
record ImportPaymentAVNVehiclesDto(List<PaymentAVNItemInputDto> Items);

record CreatePaymentGPSDto(
    string? PaymentGPSNo,
    string PmtMonth,
    string? ContractNo,
    string? ProviderCode,
    string? ProviderName,
    decimal? VATRate,
    string? Remark,
    string? CreatedBy,
    List<PaymentGPSItemInputDto> Items
);
record ApprovePaymentGPSDto(string? ApproverName);
record SignPaymentGPSDto(string? SignerName, string? FilePath);
record SettlePaymentGPSDto(string? BankTxnRef, string? SettledBy);
record RejectPaymentGPSDto(string Reason, string? RejecterName);
record CancelPaymentGPSDto(string? Reason);
record ImportPaymentGPSVehiclesDto(List<PaymentGPSItemInputDto> Items);
record UpdatePaymentGPSDetailsRequestDto(List<UpdatePaymentGPSDetailItemDto> Items);

record CreateFinancialExpenseStatementDto(
    string? CaNo,
    string DealerCode,
    string? DealerName,
    string? CAName,
    DateTime TermFrom,
    DateTime TermTo,
    DateTime TermPrevFrom,
    DateTime TermPrevTo,
    decimal? FnExpPercent,
    decimal? PmtDsTCGPercent,
    string? Remark,
    string? CreatedBy,
    List<FinancialExpenseItemInputDto> Items
);
record PreviewFinancialExpenseDto(
    decimal FnExpPercent,
    decimal PmtDsTCGPercent,
    List<FinancialExpenseItemInputDto>? Items
);
record ApproveFnExpDto(string? ApproverName);
record SignFnExpCADto(string? SignerName, string? FilePath);
record SettleFnExpDto(string? BankTxnRef, string? SettledBy);
record CancelFnExpDto(string? Reason, string? CancelledBy);
record UpdateFnExpDetailsRequestDto(List<UpdateFnExpDetailItemDto>? Items);
record ImportFnExpVehiclesDto(List<FinancialExpenseItemInputDto>? Items);

record ApplyTransportFeeVersionDto(string? AppliedBy);

