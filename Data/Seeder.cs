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

                CREATE TABLE IF NOT EXISTS ""PaymentGuarantees"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentGuarantees"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""GuaranteeNo"" TEXT NOT NULL,
                    ""BankGuaranteeNo"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""ContractNo"" TEXT NULL,
                    ""GuaranteeType"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""UtilizedAmount"" INTEGER NOT NULL,
                    ""RemainingAmount"" INTEGER NOT NULL,
                    ""DateOpen"" TEXT NOT NULL,
                    ""DateEnd"" TEXT NOT NULL,
                    ""DateExpired"" TEXT NOT NULL,
                    ""TermDays"" INTEGER NOT NULL,
                    ""TermWarningDays"" INTEGER NOT NULL,
                    ""FeePercent"" TEXT NOT NULL,
                    ""DateRecieveGrtRoot"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""RemarkReject"" TEXT NULL,
                    ""ClaimedAmount"" INTEGER NULL,
                    ""ClaimReason"" TEXT NULL,
                    ""ClaimedAt"" TEXT NULL,
                    ""ClaimedBy"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentGuaranteeDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentGuaranteeDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""GuaranteeId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ItemRefNo"" TEXT NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""OrderAmount"" INTEGER NOT NULL,
                    ""GuaranteeValue"" INTEGER NOT NULL,
                    ""GuaranteePercent"" TEXT NOT NULL,
                    ""DateStart"" TEXT NOT NULL,
                    ""DateEnd"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentGuaranteeDetails_PaymentGuarantees_GuaranteeId"" FOREIGN KEY (""GuaranteeId"") REFERENCES ""PaymentGuarantees"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGuarantees_OrgId_GuaranteeNo"" ON ""PaymentGuarantees"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_GuaranteeId"" ON ""PaymentGuaranteeDetails"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_OrgId_ItemRefNo"" ON ""PaymentGuaranteeDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""MortgageRequests"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_MortgageRequests"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ReqRMNo"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""CreditContractNo"" TEXT NULL,
                    ""MortgageDate"" TEXT NOT NULL,
                    ""TotalItems"" INTEGER NOT NULL,
                    ""ActiveItems"" INTEGER NOT NULL,
                    ""RedeemedItems"" INTEGER NOT NULL,
                    ""TotalCollateralValue"" INTEGER NOT NULL,
                    ""TotalLoanAmount"" INTEGER NOT NULL,
                    ""RemainingLoanAmount"" INTEGER NOT NULL,
                    ""InterestRate"" TEXT NOT NULL,
                    ""LoanPeriodDays"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""FinishedBy"" TEXT NULL,
                    ""FinishedAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""MortgageDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_MortgageDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""MortgageRequestId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ItemRefNo"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""CQNo"" TEXT NULL,
                    ""CONo"" TEXT NULL,
                    ""DeclarationNo"" TEXT NULL,
                    ""CODate"" TEXT NULL,
                    ""CollateralValue"" INTEGER NOT NULL,
                    ""LoanAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""ReqDMNo"" TEXT NULL,
                    ""RedeemedAt"" TEXT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_MortgageDetails_MortgageRequests_MortgageRequestId"" FOREIGN KEY (""MortgageRequestId"") REFERENCES ""MortgageRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MortgageRequests_OrgId_ReqRMNo"" ON ""MortgageRequests"" (""OrgId"", ""ReqRMNo"");
                CREATE INDEX IF NOT EXISTS ""IX_MortgageDetails_MortgageRequestId"" ON ""MortgageDetails"" (""MortgageRequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_MortgageDetails_OrgId_ItemRefNo"" ON ""MortgageDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""RedeemRequests"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_RedeemRequests"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ReqDMNo"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""RedeemDate"" TEXT NOT NULL,
                    ""TotalItems"" INTEGER NOT NULL,
                    ""ApprovedItems"" INTEGER NOT NULL,
                    ""TotalSettlementAmount"" INTEGER NOT NULL,
                    ""PaymentProofNo"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""RedeemDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_RedeemDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""RedeemRequestId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ItemRefNo"" TEXT NOT NULL,
                    ""ReqRMNo"" TEXT NOT NULL,
                    ""MortgageDetailId"" INTEGER NULL,
                    ""ModelCode"" TEXT NULL,
                    ""DealerCode"" TEXT NULL,
                    ""SettlementAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_RedeemDetails_RedeemRequests_RedeemRequestId"" FOREIGN KEY (""RedeemRequestId"") REFERENCES ""RedeemRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RedeemRequests_OrgId_ReqDMNo"" ON ""RedeemRequests"" (""OrgId"", ""ReqDMNo"");
                CREATE INDEX IF NOT EXISTS ""IX_RedeemDetails_RedeemRequestId"" ON ""RedeemDetails"" (""RedeemRequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_RedeemDetails_OrgId_ItemRefNo"" ON ""RedeemDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""PaymentOrders"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentOrders"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentNo"" TEXT NOT NULL,
                    ""PaymentType"" INTEGER NOT NULL,
                    ""BankPaymentNo"" TEXT NULL,
                    ""PaymentEndDate"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""BankCodeSend"" TEXT NOT NULL,
                    ""BankNameSend"" TEXT NULL,
                    ""BankAccountSend"" TEXT NOT NULL,
                    ""BankCodeReceive"" TEXT NOT NULL,
                    ""BankNameReceive"" TEXT NULL,
                    ""BankAccountReceive"" TEXT NOT NULL,
                    ""Funds"" INTEGER NOT NULL,
                    ""BankLending"" TEXT NULL,
                    ""InterestRate"" TEXT NOT NULL,
                    ""LoanPeriodMonths"" INTEGER NOT NULL,
                    ""AccountingRecordNo"" TEXT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""TotalAccumAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NOT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""FinishedBy"" TEXT NULL,
                    ""FinishedAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentOrderDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentOrderDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""PaymentOrderId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ItemRefNo"" TEXT NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NULL,
                    ""UnitPriceActual"" INTEGER NOT NULL,
                    ""AmountAccum"" INTEGER NOT NULL,
                    ""PercentAccum"" TEXT NOT NULL,
                    ""Amount"" INTEGER NOT NULL,
                    ""PercentCurrent"" TEXT NOT NULL,
                    ""AmountTotal"" INTEGER NOT NULL,
                    ""PercentTotal"" TEXT NOT NULL,
                    ""GuaranteeNo"" TEXT NULL,
                    ""BankGrtNo"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentOrderDetails_PaymentOrders_PaymentOrderId"" FOREIGN KEY (""PaymentOrderId"" ) REFERENCES ""PaymentOrders"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentOrders_OrgId_PaymentNo"" ON ""PaymentOrders"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentOrderDetails_PaymentOrderId"" ON ""PaymentOrderDetails"" (""PaymentOrderId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentOrderDetails_OrgId_ItemRefNo"" ON ""PaymentOrderDetails"" (""OrgId"", ""ItemRefNo"");
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

                CREATE TABLE IF NOT EXISTS ""PaymentGuarantees"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""GuaranteeNo"" text NOT NULL,
                    ""BankGuaranteeNo"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""ContractNo"" text NULL,
                    ""GuaranteeType"" integer NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""UtilizedAmount"" bigint NOT NULL,
                    ""RemainingAmount"" bigint NOT NULL,
                    ""DateOpen"" timestamp without time zone NOT NULL,
                    ""DateEnd"" timestamp without time zone NOT NULL,
                    ""DateExpired"" timestamp without time zone NOT NULL,
                    ""TermDays"" integer NOT NULL,
                    ""TermWarningDays"" integer NOT NULL,
                    ""FeePercent"" numeric NOT NULL,
                    ""DateRecieveGrtRoot"" timestamp without time zone NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""RemarkReject"" text NULL,
                    ""ClaimedAmount"" bigint NULL,
                    ""ClaimReason"" text NULL,
                    ""ClaimedAt"" timestamp without time zone NULL,
                    ""ClaimedBy"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledAt"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentGuaranteeDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""GuaranteeId"" bigint NOT NULL REFERENCES ""PaymentGuarantees"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ItemRefNo"" text NOT NULL,
                    ""Description"" text NOT NULL,
                    ""OrderAmount"" bigint NOT NULL,
                    ""GuaranteeValue"" bigint NOT NULL,
                    ""GuaranteePercent"" numeric NOT NULL,
                    ""DateStart"" timestamp without time zone NOT NULL,
                    ""DateEnd"" timestamp without time zone NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGuarantees_OrgId_GuaranteeNo"" ON ""PaymentGuarantees"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_GuaranteeId"" ON ""PaymentGuaranteeDetails"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_OrgId_ItemRefNo"" ON ""PaymentGuaranteeDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""MortgageRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ReqRMNo"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""CreditContractNo"" text NULL,
                    ""MortgageDate"" timestamp without time zone NOT NULL,
                    ""TotalItems"" integer NOT NULL,
                    ""ActiveItems"" integer NOT NULL,
                    ""RedeemedItems"" integer NOT NULL,
                    ""TotalCollateralValue"" bigint NOT NULL,
                    ""TotalLoanAmount"" bigint NOT NULL,
                    ""RemainingLoanAmount"" bigint NOT NULL,
                    ""InterestRate"" numeric NOT NULL,
                    ""LoanPeriodDays"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""FinishedBy"" text NULL,
                    ""FinishedAt"" timestamp without time zone NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE TABLE IF NOT EXISTS ""MortgageDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""MortgageRequestId"" bigint NOT NULL REFERENCES ""MortgageRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ItemRefNo"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""EngineNo"" text NULL,
                    ""CQNo"" text NULL,
                    ""CONo"" text NULL,
                    ""DeclarationNo"" text NULL,
                    ""CODate"" timestamp without time zone NULL,
                    ""CollateralValue"" bigint NOT NULL,
                    ""LoanAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""ReqDMNo"" text NULL,
                    ""RedeemedAt"" timestamp without time zone NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MortgageRequests_OrgId_ReqRMNo"" ON ""MortgageRequests"" (""OrgId"", ""ReqRMNo"");
                CREATE INDEX IF NOT EXISTS ""IX_MortgageDetails_MortgageRequestId"" ON ""MortgageDetails"" (""MortgageRequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_MortgageDetails_OrgId_ItemRefNo"" ON ""MortgageDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""RedeemRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ReqDMNo"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""RedeemDate"" timestamp without time zone NOT NULL,
                    ""TotalItems"" integer NOT NULL,
                    ""ApprovedItems"" integer NOT NULL,
                    ""TotalSettlementAmount"" bigint NOT NULL,
                    ""PaymentProofNo"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE TABLE IF NOT EXISTS ""RedeemDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""RedeemRequestId"" bigint NOT NULL REFERENCES ""RedeemRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ItemRefNo"" text NOT NULL,
                    ""ReqRMNo"" text NOT NULL,
                    ""MortgageDetailId"" bigint NULL,
                    ""ModelCode"" text NULL,
                    ""DealerCode"" text NULL,
                    ""SettlementAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RedeemRequests_OrgId_ReqDMNo"" ON ""RedeemRequests"" (""OrgId"", ""ReqDMNo"");
                CREATE INDEX IF NOT EXISTS ""IX_RedeemDetails_RedeemRequestId"" ON ""RedeemDetails"" (""RedeemRequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_RedeemDetails_OrgId_ItemRefNo"" ON ""RedeemDetails"" (""OrgId"", ""ItemRefNo"");

                CREATE TABLE IF NOT EXISTS ""PaymentOrders"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentNo"" text NOT NULL,
                    ""PaymentType"" integer NOT NULL,
                    ""BankPaymentNo"" text NULL,
                    ""PaymentEndDate"" timestamp without time zone NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""BankCodeSend"" text NOT NULL,
                    ""BankNameSend"" text NULL,
                    ""BankAccountSend"" text NOT NULL,
                    ""BankCodeReceive"" text NOT NULL,
                    ""BankNameReceive"" text NULL,
                    ""BankAccountReceive"" text NOT NULL,
                    ""Funds"" integer NOT NULL,
                    ""BankLending"" text NULL,
                    ""InterestRate"" numeric NOT NULL,
                    ""LoanPeriodMonths"" integer NOT NULL,
                    ""AccountingRecordNo"" text NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""TotalAccumAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NOT NULL,
                    ""RejectReason"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""FinishedBy"" text NULL,
                    ""FinishedAt"" timestamp without time zone NULL,
                    ""CancelledAt"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentOrderDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentOrderId"" bigint NOT NULL REFERENCES ""PaymentOrders"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ItemRefNo"" text NOT NULL,
                    ""Description"" text NOT NULL,
                    ""ModelCode"" text NULL,
                    ""UnitPriceActual"" bigint NOT NULL,
                    ""AmountAccum"" bigint NOT NULL,
                    ""PercentAccum"" numeric NOT NULL,
                    ""Amount"" bigint NOT NULL,
                    ""PercentCurrent"" numeric NOT NULL,
                    ""AmountTotal"" bigint NOT NULL,
                    ""PercentTotal"" numeric NOT NULL,
                    ""GuaranteeNo"" text NULL,
                    ""BankGrtNo"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentOrders_OrgId_PaymentNo"" ON ""PaymentOrders"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentOrderDetails_PaymentOrderId"" ON ""PaymentOrderDetails"" (""PaymentOrderId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentOrderDetails_OrgId_ItemRefNo"" ON ""PaymentOrderDetails"" (""OrgId"", ""ItemRefNo"");
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

        // Dữ liệu mẫu Thư bảo lãnh thanh toán ngân hàng (Pmt_Guarantee / BizHTC.Payment)
        if (!await db.Guarantees.AnyAsync(g => g.OrgId == TenantContext.DefaultOrgId))
        {
            var grt1 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250501-001",
                BankGuaranteeNo = "BL-VCB-2025/088",
                BankCode = "VCB",
                BankName = "Ngân hàng TMCP Ngoại thương Việt Nam (Vietcombank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Đại lý Ô tô Hyundai Thanh Xuân",
                ContractNo = "HD-HTCV-2025-088",
                GuaranteeType = GuaranteeType.Payment,
                TotalAmount = 2_500_000_000,
                UtilizedAmount = 2_150_000_000,
                RemainingAmount = 350_000_000,
                DateOpen = DateTime.Today.AddDays(-20),
                DateEnd = DateTime.Today.AddDays(70),
                DateExpired = DateTime.Today.AddDays(70),
                TermDays = 90,
                TermWarningDays = 15,
                FeePercent = 1.2m,
                DateRecieveGrtRoot = DateTime.Today.AddDays(-19),
                Status = GuaranteeStatus.Active,
                CreatedBy = "ChuyenVienKinhDoanh",
                ApprovedBy = "GiamDocTaiChinh",
                ApprovedAt = DateTime.Now.AddDays(-19),
                Remark = "Bảo lãnh thanh toán lô 03 xe thương mại & du lịch tháng 5/2025",
                CreatedAt = DateTime.Now.AddDays(-20)
            };
            db.Guarantees.Add(grt1);
            await db.SaveChangesAsync();

            db.GuaranteeDetails.AddRange(
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-GRAND-I10-01",
                    Description = "Xe Hyundai Grand i10 1.2 AT Hatchback",
                    OrderAmount = 435_000_000,
                    GuaranteeValue = 400_000_000,
                    GuaranteePercent = 91.95m,
                    DateStart = DateTime.Today.AddDays(-20),
                    DateEnd = DateTime.Today.AddDays(70),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Bảo lãnh hạn mức 92%"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-ACCENT-02",
                    Description = "Xe Hyundai Accent 1.5 AT Đặc biệt",
                    OrderAmount = 569_000_000,
                    GuaranteeValue = 550_000_000,
                    GuaranteePercent = 96.66m,
                    DateStart = DateTime.Today.AddDays(-20),
                    DateEnd = DateTime.Today.AddDays(70),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Bảo lãnh hạn mức 96.7%"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-TUCSON-03",
                    Description = "Xe Hyundai Tucson 2.0 Dầu Đặc biệt",
                    OrderAmount = 1_250_000_000,
                    GuaranteeValue = 1_200_000_000,
                    GuaranteePercent = 96.00m,
                    DateStart = DateTime.Today.AddDays(-20),
                    DateEnd = DateTime.Today.AddDays(70),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Bảo lãnh hạn mức 96%"
                }
            );
            await db.SaveChangesAsync();

            var grt2 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250510-002",
                BankGuaranteeNo = "BL-TCB-2025/112",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ thương Việt Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Đại lý Ô tô Hyundai Phạm Văn Đồng",
                ContractNo = "HD-HTCV-2025-112",
                GuaranteeType = GuaranteeType.DeferredPayment,
                TotalAmount = 1_850_000_000,
                UtilizedAmount = 1_850_000_000,
                RemainingAmount = 0,
                DateOpen = DateTime.Today.AddDays(-3),
                DateEnd = DateTime.Today.AddDays(57),
                DateExpired = DateTime.Today.AddDays(57),
                TermDays = 60,
                TermWarningDays = 15,
                FeePercent = 1.5m,
                Status = GuaranteeStatus.PendingApproval,
                CreatedBy = "ChuyenVienTinDung",
                Remark = "Hồ sơ bảo lãnh thanh toán trả chậm LC 60 ngày đợt 2",
                CreatedAt = DateTime.Now.AddDays(-3)
            };
            db.Guarantees.Add(grt2);
            await db.SaveChangesAsync();

            db.GuaranteeDetails.AddRange(
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-SANTAFE-01",
                    Description = "Xe Hyundai Santa Fe Calligraphy 2.5 Turbo",
                    OrderAmount = 1_365_000_000,
                    GuaranteeValue = 1_300_000_000,
                    GuaranteePercent = 95.24m,
                    DateStart = DateTime.Today.AddDays(-3),
                    DateEnd = DateTime.Today.AddDays(57),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Đang thẩm định hồ sơ gốc"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CUSTIN-02",
                    Description = "Xe Hyundai Custin 1.5T Đặc biệt",
                    OrderAmount = 850_000_000,
                    GuaranteeValue = 550_000_000,
                    GuaranteePercent = 64.71m,
                    DateStart = DateTime.Today.AddDays(-3),
                    DateEnd = DateTime.Today.AddDays(57),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Đang thẩm định hồ sơ gốc"
                }
            );
            await db.SaveChangesAsync();

            var grt3 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250415-003",
                BankGuaranteeNo = "BL-MBB-2025/045",
                BankCode = "MBB",
                BankName = "Ngân hàng TMCP Quân đội (MBBank)",
                PartnerCode = "DLR-HYUNDAI-HOANGVIET",
                PartnerName = "Đại lý Ô tô Hyundai Hoàng Việt",
                ContractNo = "HD-HTCV-2025-045",
                GuaranteeType = GuaranteeType.Payment,
                TotalAmount = 1_200_000_000,
                UtilizedAmount = 1_200_000_000,
                RemainingAmount = 0,
                DateOpen = DateTime.Today.AddDays(-40),
                DateEnd = DateTime.Today.AddDays(-5),
                DateExpired = DateTime.Today.AddDays(20),
                TermDays = 60,
                TermWarningDays = 15,
                FeePercent = 1.2m,
                DateRecieveGrtRoot = DateTime.Today.AddDays(-38),
                Status = GuaranteeStatus.Claimed,
                ClaimedAmount = 1_200_000_000,
                ClaimReason = "Đại lý quá hạn thanh toán 30 ngày theo thỏa thuận hợp đồng, kích hoạt đòi ngân hàng MBBank thực hiện nghĩa vụ bảo lãnh thanh toán.",
                ClaimedBy = "GiamDocQuanTriRuiRo",
                ClaimedAt = DateTime.Now.AddDays(-4),
                CreatedBy = "ChuyenVienKinhDoanh",
                ApprovedBy = "GiamDocTaiChinh",
                ApprovedAt = DateTime.Now.AddDays(-39),
                Remark = "Đã gửi công văn đòi bảo lãnh chính thức kèm hồ sơ gốc sang MBBank",
                CreatedAt = DateTime.Now.AddDays(-40)
            };
            db.Guarantees.Add(grt3);
            await db.SaveChangesAsync();

            db.GuaranteeDetails.AddRange(
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CRETA-01",
                    Description = "Xe Hyundai Creta 1.5L Cao cấp",
                    OrderAmount = 699_000_000,
                    GuaranteeValue = 600_000_000,
                    GuaranteePercent = 85.84m,
                    DateStart = DateTime.Today.AddDays(-40),
                    DateEnd = DateTime.Today.AddDays(-5),
                    Status = GuaranteeDetailStatus.Claimed,
                    Note = "Quá hạn thanh toán, chuyển sang đòi bảo lãnh"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-STARGAZER-02",
                    Description = "Xe Hyundai Stargazer X Cao cấp",
                    OrderAmount = 650_000_000,
                    GuaranteeValue = 600_000_000,
                    GuaranteePercent = 92.31m,
                    DateStart = DateTime.Today.AddDays(-40),
                    DateEnd = DateTime.Today.AddDays(-5),
                    Status = GuaranteeDetailStatus.Claimed,
                    Note = "Quá hạn thanh toán, chuyển sang đòi bảo lãnh"
                }
            );
            await db.SaveChangesAsync();

            var grt4 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250320-004",
                BankGuaranteeNo = "BL-CTG-2025/019",
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công thương Việt Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-GIAIPHONG",
                PartnerName = "Đại lý Ô tô Hyundai Giải Phóng",
                ContractNo = "HD-HTCV-2025-019",
                GuaranteeType = GuaranteeType.ContractPerformance,
                TotalAmount = 3_000_000_000,
                UtilizedAmount = 3_000_000_000,
                RemainingAmount = 0,
                DateOpen = DateTime.Today.AddDays(-60),
                DateEnd = DateTime.Today.AddDays(-10),
                DateExpired = DateTime.Today.AddDays(-5),
                TermDays = 55,
                TermWarningDays = 15,
                FeePercent = 1.0m,
                DateRecieveGrtRoot = DateTime.Today.AddDays(-58),
                Status = GuaranteeStatus.Settled,
                SettledBy = "KeToanTruong",
                SettledAt = DateTime.Now.AddDays(-7),
                CreatedBy = "ChuyenVienKinhDoanh",
                ApprovedBy = "GiamDocTaiChinh",
                ApprovedAt = DateTime.Now.AddDays(-59),
                Remark = "Đại lý đã thanh toán đủ 100% tiền mua hàng qua ủy nhiệm chi VietinBank, HTC đã phát hành biên bản giải tỏa bảo lãnh hoàn tất.",
                CreatedAt = DateTime.Now.AddDays(-60)
            };
            db.Guarantees.Add(grt4);
            await db.SaveChangesAsync();

            db.GuaranteeDetails.AddRange(
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-PALISADE-01",
                    Description = "Xe Hyundai Palisade Prestige 6 chỗ",
                    OrderAmount = 1_559_000_000,
                    GuaranteeValue = 1_500_000_000,
                    GuaranteePercent = 96.22m,
                    DateStart = DateTime.Today.AddDays(-60),
                    DateEnd = DateTime.Today.AddDays(-10),
                    Status = GuaranteeDetailStatus.Released,
                    Note = "Đã giải tỏa toàn bộ nghĩa vụ bảo lãnh"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-IONIQ5-02",
                    Description = "Xe Hyundai IONIQ 5 Exclusive EV",
                    OrderAmount = 1_500_000_000,
                    GuaranteeValue = 1_500_000_000,
                    GuaranteePercent = 100m,
                    DateStart = DateTime.Today.AddDays(-60),
                    DateEnd = DateTime.Today.AddDays(-10),
                    Status = GuaranteeDetailStatus.Released,
                    Note = "Đã giải tỏa toàn bộ nghĩa vụ bảo lãnh"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Hồ sơ Thế chấp & Giải chấp tài sản ngân hàng (RM_ReqMortgage & RD_ReqRedeem / BizHTC.GiaiChap)
        if (!await db.MortgageRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Hồ sơ thế chấp kho xe vay VietinBank (CTG) - Đã giải chấp 1 xe, còn 2 xe đang thế chấp
            var mReq1 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250501-001",
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công thương Việt Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Đại lý Ô tô Hyundai Thanh Xuân",
                CreditContractNo = "TD-CTG-2025/09",
                MortgageDate = DateTime.Today.AddDays(-20),
                TotalItems = 3,
                ActiveItems = 2,
                RedeemedItems = 1,
                TotalCollateralValue = 2_890_000_000,
                TotalLoanAmount = 2_020_000_000,
                RemainingLoanAmount = 1_070_000_000,
                InterestRate = 8.2m,
                LoanPeriodDays = 90,
                Status = MortgageStatus.Approved,
                CreatedBy = "ChuyenVienTinDung",
                ApprovedBy = "GiamDocTinDung_VietinBank",
                ApprovedAt = DateTime.Now.AddDays(-19),
                Remark = "Thế chấp bảo đảm lô xe du lịch Hyundai phục vụ giải ngân nguồn vốn lưu động",
                CreatedAt = DateTime.Now.AddDays(-20)
            };
            db.MortgageRequests.Add(mReq1);
            await db.SaveChangesAsync();

            var mDtl1_1 = new MortgageDetail
            {
                MortgageRequestId = mReq1.Id,
                OrgId = TenantContext.DefaultOrgId,
                ItemRefNo = "VIN-SANTAFE-2025-01",
                ModelCode = "SANTAFE-CAL",
                EngineNo = "ENG-SF25-99881",
                CQNo = "CQ-2025-88120",
                CONo = "CO-2025-11020",
                DeclarationNo = "TK-HQ-2025-01991",
                CODate = DateTime.Today.AddDays(-25),
                CollateralValue = 1_350_000_000,
                LoanAmount = 950_000_000,
                Status = MortgageDetailStatus.Redeemed,
                ApprovedBy = "GiamDocTinDung_VietinBank",
                ApprovedAt = DateTime.Now.AddDays(-19),
                ReqDMNo = "DM-DEMO-20250512-001",
                RedeemedAt = DateTime.Now.AddDays(-2),
                Note = "Đã hoàn tất giải chấp, bàn giao hồ sơ gốc cho đại lý xuất xưởng"
            };
            var mDtl1_2 = new MortgageDetail
            {
                MortgageRequestId = mReq1.Id,
                OrgId = TenantContext.DefaultOrgId,
                ItemRefNo = "VIN-TUCSON-2025-02",
                ModelCode = "TUCSON-2.0D",
                EngineNo = "ENG-TC25-77210",
                CQNo = "CQ-2025-88121",
                CONo = "CO-2025-11021",
                DeclarationNo = "TK-HQ-2025-01992",
                CODate = DateTime.Today.AddDays(-25),
                CollateralValue = 980_000_000,
                LoanAmount = 680_000_000,
                Status = MortgageDetailStatus.Approved,
                ApprovedBy = "GiamDocTinDung_VietinBank",
                ApprovedAt = DateTime.Now.AddDays(-19),
                Note = "Đang lưu kho phong tỏa thế chấp tại Chi nhánh Hoàn Kiếm"
            };
            var mDtl1_3 = new MortgageDetail
            {
                MortgageRequestId = mReq1.Id,
                OrgId = TenantContext.DefaultOrgId,
                ItemRefNo = "VIN-ACCENT-2025-03",
                ModelCode = "ACCENT-1.5AT",
                EngineNo = "ENG-AC25-33105",
                CQNo = "CQ-2025-88122",
                CONo = "CO-2025-11022",
                DeclarationNo = "TK-HQ-2025-01993",
                CODate = DateTime.Today.AddDays(-25),
                CollateralValue = 560_000_000,
                LoanAmount = 390_000_000,
                Status = MortgageDetailStatus.Approved,
                ApprovedBy = "GiamDocTinDung_VietinBank",
                ApprovedAt = DateTime.Now.AddDays(-19),
                Note = "Đang lưu kho phong tỏa thế chấp tại Chi nhánh Hoàn Kiếm"
            };
            db.MortgageDetails.AddRange(mDtl1_1, mDtl1_2, mDtl1_3);
            await db.SaveChangesAsync();

            // 2. Hồ sơ thế chấp MBBank (MBB) - 2 xe đang thế chấp
            var mReq2 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250510-002",
                BankCode = "MBB",
                BankName = "Ngân hàng TMCP Quân đội (MBBank)",
                PartnerCode = "DLR-HYUNDAI-DONGDO",
                PartnerName = "Đại lý Ô tô Hyundai Đông Đô",
                CreditContractNo = "TD-MBB-2025/14",
                MortgageDate = DateTime.Today.AddDays(-5),
                TotalItems = 2,
                ActiveItems = 2,
                RedeemedItems = 0,
                TotalCollateralValue = 1_370_000_000,
                TotalLoanAmount = 950_000_000,
                RemainingLoanAmount = 950_000_000,
                InterestRate = 8.5m,
                LoanPeriodDays = 60,
                Status = MortgageStatus.Approved,
                CreatedBy = "ChuyenVienTinDung",
                ApprovedBy = "QuanLyTinDung_MBBank",
                ApprovedAt = DateTime.Now.AddDays(-4),
                Remark = "Thế chấp kho xe bảo đảm khoản vay thanh toán nhà máy HTMV",
                CreatedAt = DateTime.Now.AddDays(-5)
            };
            db.MortgageRequests.Add(mReq2);
            await db.SaveChangesAsync();

            var mDtl2_1 = new MortgageDetail
            {
                MortgageRequestId = mReq2.Id,
                OrgId = TenantContext.DefaultOrgId,
                ItemRefNo = "VIN-CRETA-2025-01",
                ModelCode = "CRETA-1.5PRE",
                EngineNo = "ENG-CR25-55410",
                CQNo = "CQ-2025-99011",
                CONo = "CO-2025-22011",
                DeclarationNo = "TK-HQ-2025-03411",
                CODate = DateTime.Today.AddDays(-10),
                CollateralValue = 720_000_000,
                LoanAmount = 500_000_000,
                Status = MortgageDetailStatus.Approved,
                ApprovedBy = "QuanLyTinDung_MBBank",
                ApprovedAt = DateTime.Now.AddDays(-4),
                Note = "Giấy chứng nhận đăng kiểm CQ gốc gửi kho bảo mật MBBank"
            };
            var mDtl2_2 = new MortgageDetail
            {
                MortgageRequestId = mReq2.Id,
                OrgId = TenantContext.DefaultOrgId,
                ItemRefNo = "VIN-STARGAZER-2025-02",
                ModelCode = "STARGAZER-X",
                EngineNo = "ENG-SG25-66720",
                CQNo = "CQ-2025-99012",
                CONo = "CO-2025-22012",
                DeclarationNo = "TK-HQ-2025-03412",
                CODate = DateTime.Today.AddDays(-10),
                CollateralValue = 650_000_000,
                LoanAmount = 450_000_000,
                Status = MortgageDetailStatus.Approved,
                ApprovedBy = "QuanLyTinDung_MBBank",
                ApprovedAt = DateTime.Now.AddDays(-4),
                Note = "Giấy chứng nhận đăng kiểm CQ gốc gửi kho bảo mật MBBank"
            };
            db.MortgageDetails.AddRange(mDtl2_1, mDtl2_2);
            await db.SaveChangesAsync();

            // 3. Hồ sơ thế chấp Techcombank (TCB) - Mới lập, đang chờ phê duyệt
            var mReq3 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250514-003",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ thương Việt Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Đại lý Ô tô Hyundai Phạm Văn Đồng",
                CreditContractNo = "TD-TCB-2025/22",
                MortgageDate = DateTime.Today,
                TotalItems = 2,
                ActiveItems = 0,
                RedeemedItems = 0,
                TotalCollateralValue = 2_215_000_000,
                TotalLoanAmount = 1_550_000_000,
                RemainingLoanAmount = 1_550_000_000,
                InterestRate = 8.0m,
                LoanPeriodDays = 90,
                Status = MortgageStatus.PendingApproval,
                CreatedBy = "KeToanVayVon",
                Remark = "Hồ sơ thế chấp tài sản xe Custin & Palisade đang chờ thẩm định định giá tài sản",
                CreatedAt = DateTime.Now.AddHours(-2)
            };
            db.MortgageRequests.Add(mReq3);
            await db.SaveChangesAsync();

            db.MortgageDetails.AddRange(
                new MortgageDetail
                {
                    MortgageRequestId = mReq3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CUSTIN-2025-03",
                    ModelCode = "CUSTIN-1.5T",
                    EngineNo = "ENG-CU25-11230",
                    CQNo = "CQ-2025-99801",
                    CONo = "CO-2025-33001",
                    DeclarationNo = "TK-HQ-2025-04551",
                    CODate = DateTime.Today.AddDays(-5),
                    CollateralValue = 850_000_000,
                    LoanAmount = 600_000_000,
                    Status = MortgageDetailStatus.Pending,
                    Note = "Chờ cán bộ định giá TCB phê duyệt"
                },
                new MortgageDetail
                {
                    MortgageRequestId = mReq3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-PALISADE-2025-04",
                    ModelCode = "PALISADE-PRE",
                    EngineNo = "ENG-PA25-88902",
                    CQNo = "CQ-2025-99802",
                    CONo = "CO-2025-33002",
                    DeclarationNo = "TK-HQ-2025-04552",
                    CODate = DateTime.Today.AddDays(-5),
                    CollateralValue = 1_365_000_000,
                    LoanAmount = 950_000_000,
                    Status = MortgageDetailStatus.Pending,
                    Note = "Chờ cán bộ định giá TCB phê duyệt"
                }
            );
            await db.SaveChangesAsync();

            // 4. Hồ sơ đề nghị giải chấp đã hoàn tất cho xe SantaFe (VietinBank CTG)
            if (!await db.RedeemRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
            {
                var rReq1 = new RedeemRequest
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqDMNo = "DM-DEMO-20250512-001",
                    BankCode = "CTG",
                    BankName = "Ngân hàng TMCP Công thương Việt Nam (VietinBank)",
                    PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                    PartnerName = "Đại lý Ô tô Hyundai Thanh Xuân",
                    RedeemDate = DateTime.Today.AddDays(-2),
                    TotalItems = 1,
                    ApprovedItems = 1,
                    TotalSettlementAmount = 950_000_000,
                    PaymentProofNo = "UNC-CTG-20250512-88712",
                    Status = RedeemStatus.Completed,
                    CreatedBy = "KeToanThanhToan",
                    ApprovedBy = "TruongPhongGD_VietinBank",
                    ApprovedAt = DateTime.Now.AddDays(-2),
                    Remark = "Đại lý nộp đủ 950 triệu đồng tiền tất toán gốc vay xe SantaFe qua tài khoản CTG",
                    CreatedAt = DateTime.Now.AddDays(-3)
                };
                db.RedeemRequests.Add(rReq1);
                await db.SaveChangesAsync();

                db.RedeemDetails.Add(new RedeemDetail
                {
                    RedeemRequestId = rReq1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-SANTAFE-2025-01",
                    ReqRMNo = "RM-DEMO-20250501-001",
                    MortgageDetailId = mDtl1_1.Id,
                    ModelCode = "SANTAFE-CAL",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    SettlementAmount = 950_000_000,
                    Status = RedeemDetailStatus.Approved,
                    ApprovedBy = "TruongPhongGD_VietinBank",
                    ApprovedAt = DateTime.Now.AddDays(-2),
                    Note = "Đã thu nợ gốc thành công, phát hành giấy xóa thế chấp xe"
                });
                await db.SaveChangesAsync();
            }
        }

        // Dữ liệu mẫu Phiếu thanh toán & Ủy nhiệm chi ngân hàng (Pmt_Payment / BizHTC.Payment)
        if (!await db.PaymentOrders.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Phiếu thanh toán UNC ngân hàng (VCB) đã hoàn tất - Thanh toán tiền xe Santa Fe & Tucson
            var pm1 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250501-001",
                PaymentType = PaymentOrderType.UNC,
                BankPaymentNo = "UNC-VCB-20250501-8899",
                PaymentEndDate = DateTime.Today.AddDays(15),
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Đại lý Ô tô Hyundai Thanh Xuân",
                BankCodeSend = "VCB",
                BankNameSend = "Vietcombank - Chi nhánh Thăng Long",
                BankAccountSend = "0011009876543",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trụ sở chính",
                BankAccountReceive = "118000234567",
                Funds = PaymentFundType.OwnCapital,
                AccountingRecordNo = "PKT-202505-0145",
                TotalAmount = 1_800_000_000,
                TotalAccumAmount = 2_750_000_000,
                Status = PaymentOrderStatus.Finished,
                Remark = "Thanh toan tien mua xe Hyundai Santa Fe & Tucson theo don hang SO-2025-088",
                CreatedBy = "KeToanThanhToan",
                CreatedAt = DateTime.Now.AddDays(-10),
                ApprovedBy = "TruongPhongKeToan",
                ApprovedAt = DateTime.Now.AddDays(-9),
                FinishedBy = "KeToanNganHang",
                FinishedAt = DateTime.Now.AddDays(-8)
            };
            db.PaymentOrders.Add(pm1);
            await db.SaveChangesAsync();

            db.PaymentOrderDetails.AddRange(
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-SANTAFE-CAL-01",
                    Description = "Xe Hyundai Santa Fe Calligraphy 2.5T AWD 2025",
                    ModelCode = "SANTAFE-CAL",
                    UnitPriceActual = 1_369_000_000,
                    AmountAccum = 450_000_000, // Đã cọc trước 450M
                    PercentAccum = 32.87m,
                    Amount = 919_000_000,      // Thanh toán nốt 919M
                    PercentCurrent = 67.13m,
                    AmountTotal = 1_369_000_000,
                    PercentTotal = 100m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "Đã tất toán 100% giá trị xe, đủ điều kiện xuất hóa đơn VAT"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-TUCSON-TURBO-02",
                    Description = "Xe Hyundai Tucson 1.6T Turbo 2025",
                    ModelCode = "TUCSON-TURBO",
                    UnitPriceActual = 959_000_000,
                    AmountAccum = 500_000_000, // Đã cọc trước 500M
                    PercentAccum = 52.14m,
                    Amount = 459_000_000,      // Thanh toán tiếp 459M
                    PercentCurrent = 47.86m,
                    AmountTotal = 959_000_000,
                    PercentTotal = 100m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "Đã tất toán 100% giá trị xe, bàn giao hồ sơ gốc"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CRETA-PREM-03",
                    Description = "Xe Hyundai Creta 1.5L Cao cấp 2025",
                    ModelCode = "CRETA-PREM",
                    UnitPriceActual = 699_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 422_000_000,      // Thanh toán đợt 1
                    PercentCurrent = 60.37m,
                    AmountTotal = 422_000_000,
                    PercentTotal = 60.37m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "Thanh toán đợt 1 (60%), đợt 2 thanh toán khi nhận xe"
                }
            );
            await db.SaveChangesAsync();

            // 2. Phiếu thanh toán Bảo lãnh ngân hàng (MBB) - Đã phê duyệt, chờ ngân hàng hạch toán
            var pm2 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250510-002",
                PaymentType = PaymentOrderType.GuaranteePayment,
                BankPaymentNo = "BL-MBB-2025/045",
                PaymentEndDate = DateTime.Today.AddDays(25),
                PartnerCode = "DLR-HYUNDAI-HOANGVIET",
                PartnerName = "Đại lý Ô tô Hyundai Hoàng Việt",
                BankCodeSend = "MBB",
                BankNameSend = "Ngân hàng TMCP Quân đội (MBBank)",
                BankAccountSend = "0880123456789",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trụ sở chính",
                BankAccountReceive = "118000234567",
                Funds = PaymentFundType.CreditLine,
                AccountingRecordNo = "PKT-202505-0210",
                TotalAmount = 1_200_000_000,
                TotalAccumAmount = 1_200_000_000,
                Status = PaymentOrderStatus.Approved,
                Remark = "Thanh toán theo Thư bảo lãnh thanh toán MBBank số BL-MBB-2025/045",
                CreatedBy = "KeToanThanhToan",
                CreatedAt = DateTime.Now.AddDays(-4),
                ApprovedBy = "GiamDocTaiChinh",
                ApprovedAt = DateTime.Now.AddDays(-3)
            };
            db.PaymentOrders.Add(pm2);
            await db.SaveChangesAsync();

            db.PaymentOrderDetails.AddRange(
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CRETA-01",
                    Description = "Xe Hyundai Creta 1.5L Cao cấp",
                    ModelCode = "CRETA-PREM",
                    UnitPriceActual = 699_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 600_000_000,
                    PercentCurrent = 85.84m,
                    AmountTotal = 600_000_000,
                    PercentTotal = 85.84m,
                    GuaranteeNo = "GRT-20250501-001",
                    BankGrtNo = "BL-MBB-2025/045",
                    Status = PaymentOrderDetailStatus.Approved,
                    Note = "Bảo lãnh thanh toán phát hành qua MBBank"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-STARGAZER-02",
                    Description = "Xe Hyundai Stargazer X Cao cấp",
                    ModelCode = "STARGAZER-X",
                    UnitPriceActual = 650_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 600_000_000,
                    PercentCurrent = 92.31m,
                    AmountTotal = 600_000_000,
                    PercentTotal = 92.31m,
                    GuaranteeNo = "GRT-20250501-001",
                    BankGrtNo = "BL-MBB-2025/045",
                    Status = PaymentOrderDetailStatus.Approved,
                    Note = "Bảo lãnh thanh toán phát hành qua MBBank"
                }
            );
            await db.SaveChangesAsync();

            // 3. Phiếu thanh toán Nộp tiền cọc vay vốn ngân hàng (TCB) - Đang chờ phê duyệt
            var pm3 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250514-003",
                PaymentType = PaymentOrderType.Deposit,
                BankPaymentNo = "UNC-TCB-20250514-0091",
                PaymentEndDate = DateTime.Today.AddDays(30),
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Đại lý Ô tô Hyundai Phạm Văn Đồng",
                BankCodeSend = "TCB",
                BankNameSend = "Techcombank - Chi nhánh Hoàng Gia",
                BankAccountSend = "1903344556677",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trụ sở chính",
                BankAccountReceive = "118000234567",
                Funds = PaymentFundType.BankLoan,
                BankLending = "TCB",
                InterestRate = 7.8m,
                LoanPeriodMonths = 6,
                TotalAmount = 400_000_000,
                TotalAccumAmount = 400_000_000,
                Status = PaymentOrderStatus.PendingApproval,
                Remark = "Nộp cọc mua 02 xe Hyundai Custin & Palisade vay vốn tài trợ Techcombank",
                CreatedBy = "KeToanThanhToan",
                CreatedAt = DateTime.Now.AddHours(-3)
            };
            db.PaymentOrders.Add(pm3);
            await db.SaveChangesAsync();

            db.PaymentOrderDetails.AddRange(
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CUSTIN-2025-03",
                    Description = "Xe Hyundai Custin 1.5 Turbo Tiêu chuẩn",
                    ModelCode = "CUSTIN-1.5T",
                    UnitPriceActual = 850_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 170_000_000, // Cọc 20%
                    PercentCurrent = 20.0m,
                    AmountTotal = 170_000_000,
                    PercentTotal = 20.0m,
                    Status = PaymentOrderDetailStatus.Pending,
                    Note = "Cọc giữ chỗ 20% đơn hàng xe theo hợp đồng vay TCB"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-PALISADE-2025-04",
                    Description = "Xe Hyundai Palisade Prestige 6 chỗ",
                    ModelCode = "PALISADE-PRE",
                    UnitPriceActual = 1_365_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 230_000_000, // Cọc ~16.85%
                    PercentCurrent = 16.85m,
                    AmountTotal = 230_000_000,
                    PercentTotal = 16.85m,
                    Status = PaymentOrderDetailStatus.Pending,
                    Note = "Cọc giữ chỗ đơn hàng xe theo hợp đồng vay TCB"
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
