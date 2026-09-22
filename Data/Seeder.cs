using Microsoft.EntityFrameworkCore;
using MiniPay.Models;

namespace MiniPay.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        // Tự động bảo đảm các bảng mới tồn tại nếu database SQLite/Postgres đã được tạo từ các phiên trước
        if (db.Database.IsSqlite())
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""PayoutBatches"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PayoutBatches"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""BatchNo"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""SourceAccount"" TEXT NOT NULL,
                    ""SourceAccountName"" TEXT NULL,
                    ""BizResNumber"" TEXT NULL,
                    ""Remark"" TEXT NOT NULL,
                    ""TotalTrans"" INTEGER NOT NULL,
                    ""SuccessTrans"" INTEGER NOT NULL,
                    ""FailedTrans"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""SuccessAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""BankStatusCode"" TEXT NULL,
                    ""RefBankCode"" TEXT NULL,
                    ""BankRemark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CompletedAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PayoutDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PayoutDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""BatchId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""TransNo"" TEXT NOT NULL,
                    ""TransType"" INTEGER NOT NULL,
                    ""DisbursementType"" INTEGER NOT NULL,
                    ""RefNo"" TEXT NULL,
                    ""ReceivingUnit"" TEXT NOT NULL,
                    ""BankAccountReceive"" TEXT NOT NULL,
                    ""BankNameReceive"" TEXT NOT NULL,
                    ""ProvinceName"" TEXT NULL,
                    ""TransferAmount"" INTEGER NOT NULL,
                    ""TransferRemark"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""ErrorMessage"" TEXT NULL,
                    ""ExecutedAt"" TEXT NULL,
                    CONSTRAINT ""FK_PayoutDetails_PayoutBatches_BatchId"" FOREIGN KEY (""BatchId"") REFERENCES ""PayoutBatches"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PayoutBatches_OrgId_BatchNo"" ON ""PayoutBatches"" (""OrgId"", ""BatchNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PayoutDetails_BatchId"" ON ""PayoutDetails"" (""BatchId"");
                CREATE INDEX IF NOT EXISTS ""IX_PayoutDetails_OrgId_TransNo"" ON ""PayoutDetails"" (""OrgId"", ""TransNo"");

                CREATE TABLE IF NOT EXISTS ""DiscountRequests"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_DiscountRequests"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DiscountNo"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""ContractNo"" TEXT NULL,
                    ""TotalPaymentAmount"" INTEGER NOT NULL,
                    ""TotalDiscountAmount"" INTEGER NOT NULL,
                    ""NetPaymentAmount"" INTEGER NOT NULL,
                    ""DefaultAnnualRate"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""PartnerSignStatus"" INTEGER NOT NULL,
                    ""PartnerSignedBy"" TEXT NULL,
                    ""PartnerSignedAt"" TEXT NULL,
                    ""ApproverSignStatus"" INTEGER NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""Remark"" TEXT NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""DiscountDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_DiscountDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""RequestId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ItemRefNo"" TEXT NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""DueDate"" TEXT NOT NULL,
                    ""ActualPaymentDate"" TEXT NOT NULL,
                    ""EarlyDays"" INTEGER NOT NULL,
                    ""OriginalAmount"" INTEGER NOT NULL,
                    ""AnnualDiscountRate"" TEXT NOT NULL,
                    ""DiscountAmount"" INTEGER NOT NULL,
                    ""NetPayAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_DiscountDetails_DiscountRequests_RequestId"" FOREIGN KEY (""RequestId"") REFERENCES ""DiscountRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DiscountRequests_OrgId_DiscountNo"" ON ""DiscountRequests"" (""OrgId"", ""DiscountNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DiscountDetails_RequestId"" ON ""DiscountDetails"" (""RequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_DiscountDetails_OrgId_ItemRefNo"" ON ""DiscountDetails"" (""OrgId"", ""ItemRefNo"");
            ");
        }
        else if (db.Database.IsNpgsql())
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""PayoutBatches"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""BatchNo"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""SourceAccount"" text NOT NULL,
                    ""SourceAccountName"" text NULL,
                    ""BizResNumber"" text NULL,
                    ""Remark"" text NOT NULL,
                    ""TotalTrans"" integer NOT NULL,
                    ""SuccessTrans"" integer NOT NULL,
                    ""FailedTrans"" integer NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""SuccessAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""BankStatusCode"" text NULL,
                    ""RefBankCode"" text NULL,
                    ""BankRemark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""RejectReason"" text NULL,
                    ""CompletedAt"" timestamp without time zone NULL,
                    ""CancelledAt"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""PayoutDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""BatchId"" bigint NOT NULL REFERENCES ""PayoutBatches"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""TransNo"" text NOT NULL,
                    ""TransType"" integer NOT NULL,
                    ""DisbursementType"" integer NOT NULL,
                    ""RefNo"" text NULL,
                    ""ReceivingUnit"" text NOT NULL,
                    ""BankAccountReceive"" text NOT NULL,
                    ""BankNameReceive"" text NOT NULL,
                    ""ProvinceName"" text NULL,
                    ""TransferAmount"" bigint NOT NULL,
                    ""TransferRemark"" text NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""BankTxnRef"" text NULL,
                    ""ErrorMessage"" text NULL,
                    ""ExecutedAt"" timestamp without time zone NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PayoutBatches_OrgId_BatchNo"" ON ""PayoutBatches"" (""OrgId"", ""BatchNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PayoutDetails_BatchId"" ON ""PayoutDetails"" (""BatchId"");
                CREATE INDEX IF NOT EXISTS ""IX_PayoutDetails_OrgId_TransNo"" ON ""PayoutDetails"" (""OrgId"", ""TransNo"");

                CREATE TABLE IF NOT EXISTS ""DiscountRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DiscountNo"" text NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""ContractNo"" text NULL,
                    ""TotalPaymentAmount"" bigint NOT NULL,
                    ""TotalDiscountAmount"" bigint NOT NULL,
                    ""NetPaymentAmount"" bigint NOT NULL,
                    ""DefaultAnnualRate"" numeric NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""PartnerSignStatus"" integer NOT NULL,
                    ""PartnerSignedBy"" text NULL,
                    ""PartnerSignedAt"" timestamp without time zone NULL,
                    ""ApproverSignStatus"" integer NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""Remark"" text NOT NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""DiscountDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""RequestId"" bigint NOT NULL REFERENCES ""DiscountRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ItemRefNo"" text NOT NULL,
                    ""Description"" text NOT NULL,
                    ""DueDate"" timestamp without time zone NOT NULL,
                    ""ActualPaymentDate"" timestamp without time zone NOT NULL,
                    ""EarlyDays"" integer NOT NULL,
                    ""OriginalAmount"" bigint NOT NULL,
                    ""AnnualDiscountRate"" numeric NOT NULL,
                    ""DiscountAmount"" bigint NOT NULL,
                    ""NetPayAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DiscountRequests_OrgId_DiscountNo"" ON ""DiscountRequests"" (""OrgId"", ""DiscountNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DiscountDetails_RequestId"" ON ""DiscountDetails"" (""RequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_DiscountDetails_OrgId_ItemRefNo"" ON ""DiscountDetails"" (""OrgId"", ""ItemRefNo"");
            ");
        }

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Merchant", ApiKey = "demo-pay" });
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu giao dịch thanh toán để đối soát
        if (!await db.Payments.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            var p1 = new PaymentIntent
            {
                OrgId = TenantContext.DefaultOrgId,
                TxnRef = "TXN-REC-202501",
                OrderId = "ORD-REC-101",
                Amount = 1_500_000,
                OrderInfo = "Thanh toan hop dong HD001",
                Status = PayStatus.Paid,
                Provider = "vnpay",
                BankCode = "VCB",
                VnpTransactionNo = "VCB-TXN-88123",
                CreatedAt = DateTime.Now.AddDays(-2),
                PaidAt = DateTime.Now.AddDays(-1)
            };
            var p2 = new PaymentIntent
            {
                OrgId = TenantContext.DefaultOrgId,
                TxnRef = "TXN-REC-202502",
                OrderId = "ORD-REC-102",
                Amount = 3_200_000,
                OrderInfo = "Thanh toan bao hiem BH002",
                Status = PayStatus.Pending,
                Provider = "momo",
                CreatedAt = DateTime.Now.AddDays(-1)
            };
            var p3 = new PaymentIntent
            {
                OrgId = TenantContext.DefaultOrgId,
                TxnRef = "TXN-REC-202503",
                OrderId = "ORD-REC-103",
                Amount = 750_000,
                OrderInfo = "Thanh toan phi bao tri BT003",
                Status = PayStatus.Pending,
                Provider = "vnpay",
                CreatedAt = DateTime.Now.AddDays(-1)
            };
            db.Payments.AddRange(p1, p2, p3);
            await db.SaveChangesAsync();

            // Seed 1 đợt đối soát sao kê ngân hàng mẫu
            if (!await db.ReconcileBatches.AnyAsync(b => b.OrgId == TenantContext.DefaultOrgId))
            {
                var batch = new ReconcileBatch
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchCode = "REC-DEMO-20250514-01",
                    BankCode = "VCB",
                    AccountNo = "0011004123456",
                    StatementDate = DateTime.Today,
                    Note = "Sao kê tài khoản Vietcombank đợt 1",
                    TotalRecords = 3,
                    MatchedCount = 1,
                    MismatchedCount = 1,
                    UnmatchedCount = 1,
                    TotalAmount = 5_250_000,
                    MatchedAmount = 1_500_000,
                    Status = ReconcileStatus.Discrepant,
                    CreatedAt = DateTime.Now.AddHours(-3),
                    CompletedAt = DateTime.Now.AddHours(-1)
                };
                db.ReconcileBatches.Add(batch);
                await db.SaveChangesAsync();

                db.ReconcileDetails.AddRange(
                    new ReconcileDetail
                    {
                        BatchId = batch.Id,
                        OrgId = TenantContext.DefaultOrgId,
                        BankTxnNo = "VCB-TXN-88123",
                        TxnRef = "TXN-REC-202501",
                        TxnTime = DateTime.Now.AddDays(-1),
                        Amount = 1_500_000,
                        SenderAccount = "190200889988",
                        ReceiverAccount = "0011004123456",
                        Remark = "CT DEN ORD-REC-101 MA GD TXN-REC-202501",
                        MatchStatus = MatchStatus.Matched,
                        PaymentIntentId = p1.Id,
                        SystemAmount = 1_500_000,
                        DiscrepancyReason = "Khớp chuẩn mã giao dịch và số tiền.",
                        MatchedAt = DateTime.Now.AddHours(-1)
                    },
                    new ReconcileDetail
                    {
                        BatchId = batch.Id,
                        OrgId = TenantContext.DefaultOrgId,
                        BankTxnNo = "VCB-TXN-88124",
                        TxnRef = "TXN-REC-202502",
                        TxnTime = DateTime.Now.AddDays(-1),
                        Amount = 3_000_000, // Lệch: sao kê ghi 3.0M, hệ thống cần 3.2M
                        SenderAccount = "045100033221",
                        ReceiverAccount = "0011004123456",
                        Remark = "CT HD ORD-REC-102 TXN-REC-202502 NOP THIEU",
                        MatchStatus = MatchStatus.AmountMismatch,
                        PaymentIntentId = p2.Id,
                        SystemAmount = 3_200_000,
                        DiscrepancyReason = "Lệch số tiền: Sao kê 3,000,000 đ, Hệ thống 3,200,000 đ (chênh lệch: -200,000 đ)."
                    },
                    new ReconcileDetail
                    {
                        BatchId = batch.Id,
                        OrgId = TenantContext.DefaultOrgId,
                        BankTxnNo = "VCB-TXN-88125",
                        TxnRef = "TXN-REC-99999",
                        TxnTime = DateTime.Now.AddDays(-1),
                        Amount = 750_000,
                        SenderAccount = "007100112233",
                        ReceiverAccount = "0011004123456",
                        Remark = "Chuyen tien mua phu tung ngoai he thong",
                        MatchStatus = MatchStatus.NotFound,
                        DiscrepancyReason = "Không tìm thấy giao dịch tương ứng trong hệ thống."
                    }
                );
                await db.SaveChangesAsync();
            }
        }

        // Seed dữ liệu mẫu cho Lệnh chi chuyển tiền ngân hàng tự động (Banking Payout / RQ_BankingTransactions)
        if (!await db.PayoutBatches.AnyAsync(b => b.OrgId == TenantContext.DefaultOrgId))
        {
            var pBatch1 = new BankingPayoutBatch
            {
                OrgId = TenantContext.DefaultOrgId,
                BatchNo = "BTX-DEMO-20250514-01",
                BankCode = "CTG",
                SourceAccount = "112000345678",
                SourceAccountName = "CONG TY TNHH MINIPAY VIET NAM",
                BizResNumber = "QD-CHI-2025-089",
                Remark = "Lệnh chi tiền lô thanh toán NCC và hoàn tiền khách hàng đợt 1",
                TotalTrans = 3,
                SuccessTrans = 3,
                FailedTrans = 0,
                TotalAmount = 45_800_000,
                SuccessAmount = 45_800_000,
                Status = PayoutStatus.Completed,
                BankStatusCode = "00",
                RefBankCode = "VTB-BULK-20250514-991",
                BankRemark = "Giao dịch chuyển tiền ngân hàng thành công trọn gói.",
                CreatedBy = "KeToanVien",
                ApprovedBy = "KeToanTruong",
                CreatedAt = DateTime.Now.AddDays(-2),
                ApprovedAt = DateTime.Now.AddDays(-2).AddHours(1),
                CompletedAt = DateTime.Now.AddDays(-2).AddHours(2)
            };
            db.PayoutBatches.Add(pBatch1);
            await db.SaveChangesAsync();

            db.PayoutDetails.AddRange(
                new BankingPayoutDetail
                {
                    BatchId = pBatch1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = "BTX-DEMO-20250514-01-001",
                    TransType = PayoutTransType.Napas247,
                    DisbursementType = DisbursementType.Supplier,
                    RefNo = "PO-2025-012",
                    ReceivingUnit = "CONG TY CP THIET BI SO LINH KIEN",
                    BankAccountReceive = "0011000987654",
                    BankNameReceive = "VCB",
                    ProvinceName = "Hà Nội",
                    TransferAmount = 25_000_000,
                    TransferRemark = "Thanh toan tien hang PO-2025-012",
                    Status = PayoutItemStatus.Success,
                    BankTxnRef = "CTG-20250512-881923",
                    ExecutedAt = DateTime.Now.AddDays(-2).AddHours(2)
                },
                new BankingPayoutDetail
                {
                    BatchId = pBatch1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = "BTX-DEMO-20250514-01-002",
                    TransType = PayoutTransType.Napas247,
                    DisbursementType = DisbursementType.Refund,
                    RefNo = "ORD-REC-102",
                    ReceivingUnit = "NGUYEN VAN A",
                    BankAccountReceive = "1903456789012",
                    BankNameReceive = "TCB",
                    ProvinceName = "TP Hồ Chí Minh",
                    TransferAmount = 3_200_000,
                    TransferRemark = "Hoan tien huy don ORD-REC-102",
                    Status = PayoutItemStatus.Success,
                    BankTxnRef = "CTG-20250512-881924",
                    ExecutedAt = DateTime.Now.AddDays(-2).AddHours(2)
                },
                new BankingPayoutDetail
                {
                    BatchId = pBatch1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = "BTX-DEMO-20250514-01-003",
                    TransType = PayoutTransType.Napas247,
                    DisbursementType = DisbursementType.Commission,
                    RefNo = "CMS-DLR-008",
                    ReceivingUnit = "DAI LY HYUNDAI DONG DO",
                    BankAccountReceive = "0880199887766",
                    BankNameReceive = "MBB",
                    ProvinceName = "Hà Nội",
                    TransferAmount = 17_600_000,
                    TransferRemark = "Chi tra hoa hong ban hang thang 4 CMS-DLR-008",
                    Status = PayoutItemStatus.Success,
                    BankTxnRef = "CTG-20250512-881925",
                    ExecutedAt = DateTime.Now.AddDays(-2).AddHours(2)
                }
            );
            await db.SaveChangesAsync();

            var pBatch2 = new BankingPayoutBatch
            {
                OrgId = TenantContext.DefaultOrgId,
                BatchNo = "BTX-DEMO-20250514-02",
                BankCode = "MBB",
                SourceAccount = "088011223344",
                SourceAccountName = "CONG TY TNHH MINIPAY VIET NAM",
                BizResNumber = "QD-CHI-2025-095",
                Remark = "Lệnh chi trả tiền thuê mặt bằng và dịch vụ hạ tầng đợt 2",
                TotalTrans = 2,
                SuccessTrans = 0,
                FailedTrans = 0,
                TotalAmount = 18_500_000,
                SuccessAmount = 0,
                Status = PayoutStatus.PendingApproval,
                CreatedBy = "KeToanVien",
                CreatedAt = DateTime.Now.AddHours(-4)
            };
            db.PayoutBatches.Add(pBatch2);
            await db.SaveChangesAsync();

            db.PayoutDetails.AddRange(
                new BankingPayoutDetail
                {
                    BatchId = pBatch2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = "BTX-DEMO-20250514-02-001",
                    TransType = PayoutTransType.Napas247,
                    DisbursementType = DisbursementType.Supplier,
                    RefNo = "HD-THUE-MB01",
                    ReceivingUnit = "CONG TY TNHH QUAN LY TOA NHA CAPITAL",
                    BankAccountReceive = "0331000554433",
                    BankNameReceive = "VCB",
                    ProvinceName = "Hà Nội",
                    TransferAmount = 12_000_000,
                    TransferRemark = "Thanh toan tien thue van phong ky 5/2025",
                    Status = PayoutItemStatus.Pending
                },
                new BankingPayoutDetail
                {
                    BatchId = pBatch2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = "BTX-DEMO-20250514-02-002",
                    TransType = PayoutTransType.Napas247,
                    DisbursementType = DisbursementType.Other,
                    RefNo = "INV-AWS-889",
                    ReceivingUnit = "TRUNG TAM DU LIEU CLOUD FAST",
                    BankAccountReceive = "114002345678",
                    BankNameReceive = "CTG",
                    ProvinceName = "Đà Nẵng",
                    TransferAmount = 6_500_000,
                    TransferRemark = "Chi tra cuoc ha tang cloud Fast thang 5",
                    Status = PayoutItemStatus.Pending
                }
            );
            await db.SaveChangesAsync();
        }

        // Seed dữ liệu mẫu Chiết khấu thanh toán sớm (Payment Discount / Req_PaymentDiscount)
        if (!await db.DiscountRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            var dReq1 = new PaymentDiscountRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                DiscountNo = "DIS-DEMO-20250514-01",
                PartnerCode = "DLR-HYUNDAI-HADONG",
                PartnerName = "CÔNG TY CP LIÊN DOANH HYUNDAI HÀ ĐÔNG",
                ContractNo = "CTR-HTCV-2025-088",
                DefaultAnnualRate = 7.5m,
                TotalPaymentAmount = 3_300_000_000,
                TotalDiscountAmount = 20_437_500,
                NetPaymentAmount = 3_279_562_500,
                Status = DiscountRequestStatus.Settled,
                PartnerSignStatus = DiscountSignStatus.Signed,
                PartnerSignedBy = "GiamDoc_HyundaiHaDong",
                PartnerSignedAt = DateTime.Now.AddDays(-3).AddHours(2),
                ApproverSignStatus = DiscountSignStatus.Signed,
                ApprovedBy = "TruongPhongKeToan_HTC",
                ApprovedAt = DateTime.Now.AddDays(-2).AddHours(4),
                SettledBy = "GiamDocTaiChinh_HTC",
                SettledAt = DateTime.Now.AddDays(-1).AddHours(1),
                Remark = "Hồ sơ chiết khấu thanh toán sớm lô xe hợp đồng CTR-HTCV-2025-088",
                CreatedAt = DateTime.Now.AddDays(-3)
            };
            db.DiscountRequests.Add(dReq1);
            await db.SaveChangesAsync();

            db.DiscountDetails.AddRange(
                new PaymentDiscountDetail
                {
                    RequestId = dReq1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-04-01",
                    Description = "Thanh toán đợt 1 lô 02 xe Hyundai Santa Fe 2025",
                    DueDate = DateTime.Today.AddDays(25),
                    ActualPaymentDate = DateTime.Today.AddDays(-2),
                    EarlyDays = 27,
                    OriginalAmount = 1_500_000_000,
                    AnnualDiscountRate = 7.5m,
                    DiscountAmount = 8_437_500, // 1.5B * 7.5% * 27 / 360 = 8,437,500
                    NetPayAmount = 1_491_562_500,
                    Status = DiscountItemStatus.Active,
                    Note = "Đã đối chiếu khớp lệnh ủy nhiệm chi VCB"
                },
                new PaymentDiscountDetail
                {
                    RequestId = dReq1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-04-02",
                    Description = "Thanh toán đợt 2 lô 02 xe Hyundai Tucson 2025",
                    DueDate = DateTime.Today.AddDays(30),
                    ActualPaymentDate = DateTime.Today.AddDays(-2),
                    EarlyDays = 32,
                    OriginalAmount = 1_800_000_000,
                    AnnualDiscountRate = 7.5m,
                    DiscountAmount = 12_000_000, // 1.8B * 7.5% * 32 / 360 = 12,000,000
                    NetPayAmount = 1_788_000_000,
                    Status = DiscountItemStatus.Active,
                    Note = "Đã đối chiếu khớp lệnh ủy nhiệm chi VCB"
                }
            );
            await db.SaveChangesAsync();

            var dReq2 = new PaymentDiscountRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                DiscountNo = "DIS-DEMO-20250514-02",
                PartnerCode = "DLR-HYUNDAI-DONGDO",
                PartnerName = "ĐẠI LÝ Ô TÔ HYUNDAI ĐÔNG ĐÔ",
                ContractNo = "CTR-HTCV-2025-102",
                DefaultAnnualRate = 8.0m,
                TotalPaymentAmount = 1_850_000_000,
                TotalDiscountAmount = 9_000_000,
                NetPaymentAmount = 1_841_000_000,
                Status = DiscountRequestStatus.PartnerSigned,
                PartnerSignStatus = DiscountSignStatus.Signed,
                PartnerSignedBy = "KeToanTruong_DongDo",
                PartnerSignedAt = DateTime.Now.AddHours(-3),
                ApproverSignStatus = DiscountSignStatus.Pending,
                Remark = "Đề nghị duyệt chiết khấu trả sớm đơn hàng xe Stargazer & phụ tùng",
                CreatedAt = DateTime.Now.AddHours(-6)
            };
            db.DiscountRequests.Add(dReq2);
            await db.SaveChangesAsync();

            db.DiscountDetails.AddRange(
                new PaymentDiscountDetail
                {
                    RequestId = dReq2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-05-10",
                    Description = "Thanh toán lô phụ tùng & linh kiện chính hãng",
                    DueDate = DateTime.Today.AddDays(18),
                    ActualPaymentDate = DateTime.Today,
                    EarlyDays = 18,
                    OriginalAmount = 650_000_000,
                    AnnualDiscountRate = 8.0m,
                    DiscountAmount = 2_600_000, // 650M * 8% * 18 / 360 = 2,600,000
                    NetPayAmount = 647_400_000,
                    Status = DiscountItemStatus.Active,
                    Note = "Đề nghị cấn trừ hóa đơn VAT tiếp theo"
                },
                new PaymentDiscountDetail
                {
                    RequestId = dReq2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-05-11",
                    Description = "Thanh toán lô xe Hyundai Stargazer 2025 đợt 1",
                    DueDate = DateTime.Today.AddDays(24),
                    ActualPaymentDate = DateTime.Today,
                    EarlyDays = 24,
                    OriginalAmount = 1_200_000_000,
                    AnnualDiscountRate = 8.0m,
                    DiscountAmount = 6_400_000, // 1.2B * 8% * 24 / 360 = 6,400,000
                    NetPayAmount = 1_193_600_000,
                    Status = DiscountItemStatus.Active,
                    Note = "Đính kèm bản scan UNC ngân hàng MBBank"
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
