using Microsoft.EntityFrameworkCore;
using MiniPay.Models;
using MiniPay.Services;

namespace MiniPay.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        // Tá»± Ä‘á»™ng báº£o Ä‘áº£m cÃ¡c báº£ng má»›i tá»“n táº¡i náº¿u database SQLite/Postgres Ä‘Ã£ Ä‘Æ°á»£c táº¡o tá»« cÃ¡c phiÃªn trÆ°á»›c
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
                    ""DateWarning"" TEXT NULL,
                    ""DateExpired"" TEXT NULL,
                    ""FlagDtlDiscount"" TEXT NOT NULL DEFAULT '0',
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentGuaranteeDetails_PaymentGuarantees_GuaranteeId"" FOREIGN KEY (""GuaranteeId"") REFERENCES ""PaymentGuarantees"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGuarantees_OrgId_GuaranteeNo"" ON ""PaymentGuarantees"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_GuaranteeId"" ON ""PaymentGuaranteeDetails"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_OrgId_ItemRefNo"" ON ""PaymentGuaranteeDetails"" (""OrgId"", ""ItemRefNo"");
                -- SQLite khong ho tro ADD COLUMN IF NOT EXISTS. Cac cot DateWarning/DateExpired/
                -- FlagDtlDiscount da co san trong CREATE TABLE IF NOT EXISTS o tren (DB moi), nen
                -- ALTER vo dieu kien se bao 'duplicate column name'. Chi bo sung cot cho DB cu
                -- (tao tu phien truoc, thieu cot) bang cach kiem tra truoc khi them.

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

                CREATE TABLE IF NOT EXISTS ""BankBillMinutes"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_BankBillMinutes"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""BankBillMnNo"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""PartnerCode"" TEXT NOT NULL,
                    ""PartnerName"" TEXT NOT NULL,
                    ""BankBillDate"" TEXT NOT NULL,
                    ""BankBillPrintDate"" TEXT NULL,
                    ""BankBillReciveDate"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalClaimAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""HandedOverBy"" TEXT NULL,
                    ""HandedOverAt"" TEXT NULL,
                    ""BankReceivedBy"" TEXT NULL,
                    ""BankReceivedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""BankBillMinutesDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_BankBillMinutesDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""MinutesId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""CONo"" TEXT NULL,
                    ""CabinCONo"" TEXT NULL,
                    ""DeclarationNo"" TEXT NULL,
                    ""BankGuaranteeNo"" TEXT NULL,
                    ""HTCInvoiceNo"" TEXT NULL,
                    ""TCGInvoiceNo"" TEXT NULL,
                    ""TransportMinutesNo"" TEXT NULL,
                    ""ClaimAmount"" INTEGER NOT NULL,
                    ""GuaranteeDateStart"" TEXT NULL,
                    ""GuaranteeDateOpen"" TEXT NULL,
                    ""NumberOfDaysDeferred"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_BankBillMinutesDetails_BankBillMinutes_MinutesId"" FOREIGN KEY (""MinutesId"") REFERENCES ""BankBillMinutes"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_BankBillMinutes_OrgId_BankBillMnNo"" ON ""BankBillMinutes"" (""OrgId"", ""BankBillMnNo"");
                CREATE INDEX IF NOT EXISTS ""IX_BankBillMinutesDetails_MinutesId"" ON ""BankBillMinutesDetails"" (""MinutesId"");
                CREATE INDEX IF NOT EXISTS ""IX_BankBillMinutesDetails_OrgId_VIN"" ON ""BankBillMinutesDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentPDIs"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentPDIs"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PmtPDINo"" TEXT NOT NULL,
                    ""PmtMonth"" TEXT NOT NULL,
                    ""ServiceUnitCode"" TEXT NOT NULL,
                    ""ServiceUnitName"" TEXT NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalCostIn"" INTEGER NOT NULL,
                    ""TotalCostOut"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""VATRate"" TEXT NOT NULL,
                    ""AmountVAT"" INTEGER NOT NULL,
                    ""TotalAmountAfterVAT"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TCMSSignStatus"" INTEGER NOT NULL,
                    ""TCMSSignUser"" TEXT NULL,
                    ""TCMSSignDTime"" TEXT NULL,
                    ""HTVSignStatus"" INTEGER NOT NULL,
                    ""HTVSignUser"" TEXT NULL,
                    ""HTVSignDTime"" TEXT NULL,
                    ""Appr1By"" TEXT NULL,
                    ""Appr1DTime"" TEXT NULL,
                    ""Appr2By"" TEXT NULL,
                    ""Appr2DTime"" TEXT NULL,
                    ""PaidBy"" TEXT NULL,
                    ""PaidAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentPDIDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PaymentPDIDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""PaymentPDIId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorExtNameVN"" TEXT NULL,
                    ""StorageCodeInit"" TEXT NOT NULL,
                    ""StoreDate"" TEXT NULL,
                    ""DeliveryOutDate"" TEXT NULL,
                    ""DlvMnNo"" TEXT NULL,
                    ""DealerCode"" TEXT NULL,
                    ""CostInCheck"" INTEGER NOT NULL,
                    ""CostOutCheck"" INTEGER NOT NULL,
                    ""TotalCostCheck"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentPDIDetails_PaymentPDIs_PaymentPDIId"" FOREIGN KEY (""PaymentPDIId"") REFERENCES ""PaymentPDIs"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentPDIs_OrgId_PmtPDINo"" ON ""PaymentPDIs"" (""OrgId"", ""PmtPDINo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIs_OrgId_PmtMonth"" ON ""PaymentPDIs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIDetails_PaymentPDIId"" ON ""PaymentPDIDetails"" (""PaymentPDIId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIDetails_OrgId_VIN"" ON ""PaymentPDIDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""LatePaymentPenalties"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_LatePaymentPenalties"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PenaltyRecordNo"" TEXT NOT NULL,
                    ""SOCode"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NULL,
                    ""ContractNo"" TEXT NULL,
                    ""SOApprovedDate"" TEXT NULL,
                    ""TotalApprovedQuantity"" INTEGER NOT NULL,
                    ""TotalUnitPriceActual"" INTEGER NOT NULL,
                    ""MaxDelayDaysDeposit"" INTEGER NOT NULL,
                    ""MaxDelayDaysGrtOpen"" INTEGER NOT NULL,
                    ""MaxDelayDaysGrtPay"" INTEGER NOT NULL,
                    ""MaxDelayDays60Pmt"" INTEGER NOT NULL,
                    ""MaxDelayDaysRemain"" INTEGER NOT NULL,
                    ""TotalDatePenalty"" INTEGER NOT NULL,
                    ""PenaltyRateAnnual"" TEXT NOT NULL,
                    ""AmountPenaltySystem"" INTEGER NOT NULL,
                    ""PenalizeActual"" INTEGER NOT NULL,
                    ""WaivedAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""AdjustmentReason"" TEXT NULL,
                    ""PaymentProofRef"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""CalculatedAt"" TEXT NULL,
                    ""ReviewedBy"" TEXT NULL,
                    ""ReviewedAt"" TEXT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""LatePaymentPenaltyDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_LatePaymentPenaltyDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""PenaltyId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""UnitPriceActual"" INTEGER NOT NULL,
                    ""DepositDueDate"" TEXT NULL,
                    ""ActualDepositDate"" TEXT NULL,
                    ""GrtDueDate"" TEXT NULL,
                    ""ActualGrtDate"" TEXT NULL,
                    ""GrtPayDueDate"" TEXT NULL,
                    ""ActualGrtPayDate"" TEXT NULL,
                    ""Payment60DueDate"" TEXT NULL,
                    ""Actual60PayDate"" TEXT NULL,
                    ""PaymentRemainDueDate"" TEXT NULL,
                    ""ActualRemainPayDate"" TEXT NULL,
                    ""DelayDaysDeposit"" INTEGER NOT NULL,
                    ""DelayDaysGrtOpen"" INTEGER NOT NULL,
                    ""DelayDaysGrtPay"" INTEGER NOT NULL,
                    ""DelayDays60Pmt"" INTEGER NOT NULL,
                    ""DelayDaysRemain"" INTEGER NOT NULL,
                    ""MaxDelayDays"" INTEGER NOT NULL,
                    ""ItemPenaltyAmount"" INTEGER NOT NULL,
                    ""ActualItemPenalty"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_LatePaymentPenaltyDetails_LatePaymentPenalties_PenaltyId"" FOREIGN KEY (""PenaltyId"") REFERENCES ""LatePaymentPenalties"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_LatePaymentPenalties_OrgId_PenaltyRecordNo"" ON ""LatePaymentPenalties"" (""OrgId"", ""PenaltyRecordNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenalties_OrgId_SOCode"" ON ""LatePaymentPenalties"" (""OrgId"", ""SOCode"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenaltyDetails_PenaltyId"" ON ""LatePaymentPenaltyDetails"" (""PenaltyId"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenaltyDetails_OrgId_VIN"" ON ""LatePaymentPenaltyDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""TransportInsPayments"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""TransportInsNo"" TEXT NOT NULL,
                    ""PmtMonth"" TEXT NOT NULL,
                    ""TransporterCode"" TEXT NOT NULL,
                    ""TransporterName"" TEXT NOT NULL,
                    ""InsuranceCompanyCode"" TEXT NOT NULL,
                    ""InsuranceCompanyName"" TEXT NOT NULL,
                    ""InsuranceContractNo"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalTransportCost"" INTEGER NOT NULL,
                    ""TotalDelayPenalty"" INTEGER NOT NULL,
                    ""TotalInsuranceCost"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""TotalBeforeVAT"" INTEGER NOT NULL,
                    ""AmountVAT"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TCMSSignStatus"" INTEGER NOT NULL,
                    ""TCMSSignUser"" TEXT NULL,
                    ""TCMSSignDTime"" TEXT NULL,
                    ""HTVSignStatus"" INTEGER NOT NULL,
                    ""HTVSignUser"" TEXT NULL,
                    ""HTVSignDTime"" TEXT NULL,
                    ""Appr1By"" TEXT NULL,
                    ""Appr1DTime"" TEXT NULL,
                    ""Appr2By"" TEXT NULL,
                    ""Appr2DTime"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""TransportInsPaymentDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""TransportInsPaymentId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""FStorageCode"" TEXT NOT NULL,
                    ""FProvinceName"" TEXT NULL,
                    ""TStorageCode"" TEXT NOT NULL,
                    ""TProvinceName"" TEXT NULL,
                    ""TranspReqType"" TEXT NOT NULL,
                    ""DlvMnNo"" TEXT NULL,
                    ""DlvStartDate"" TEXT NULL,
                    ""ExpectedDays"" INTEGER NOT NULL,
                    ""ExpectedDlvEndDate"" TEXT NULL,
                    ""DlvEndDate"" TEXT NULL,
                    ""DelayDays"" INTEGER NOT NULL,
                    ""TFValReal"" INTEGER NOT NULL,
                    ""TPValReal"" INTEGER NOT NULL,
                    ""PriceCar"" INTEGER NOT NULL,
                    ""InsurancePercent"" REAL NOT NULL,
                    ""InsuranceCost"" INTEGER NOT NULL,
                    ""Val_Transport"" INTEGER NOT NULL,
                    ""StandardRemark"" TEXT NULL,
                    ""FProvinceRemark"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_TransportInsPaymentDetails_TransportInsPayments_TransportInsPaymentId"" FOREIGN KEY (""TransportInsPaymentId"") REFERENCES ""TransportInsPayments"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TransportInsPayments_OrgId_TransportInsNo"" ON ""TransportInsPayments"" (""OrgId"", ""TransportInsNo"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPayments_OrgId_PmtMonth"" ON ""TransportInsPayments"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPaymentDetails_TransportInsPaymentId"" ON ""TransportInsPaymentDetails"" (""TransportInsPaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPaymentDetails_OrgId_VIN"" ON ""TransportInsPaymentDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentStorages"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentStorageNo"" TEXT NOT NULL,
                    ""PmtMonth"" TEXT NOT NULL,
                    ""StorageOperatorCode"" TEXT NOT NULL,
                    ""StorageOperatorName"" TEXT NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalCoatCost"" INTEGER NOT NULL,
                    ""TotalStorageCost"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""UnitPriceVAT"" INTEGER NOT NULL,
                    ""AmountTotal"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TCMSSignStatus"" INTEGER NOT NULL,
                    ""TCMSSignUser"" TEXT NULL,
                    ""TCMSSignDTime"" TEXT NULL,
                    ""HTVSignStatus"" INTEGER NOT NULL,
                    ""HTVSignUser"" TEXT NULL,
                    ""HTVSignDTime"" TEXT NULL,
                    ""Appr1By"" TEXT NULL,
                    ""Appr1DTime"" TEXT NULL,
                    ""Appr2By"" TEXT NULL,
                    ""Appr2DTime"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentStorageDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentStorageId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentStorageNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorExtNameVN"" TEXT NULL,
                    ""StorageCodeInit"" TEXT NOT NULL,
                    ""StorageDate"" TEXT NOT NULL,
                    ""ApprovedDate2"" TEXT NULL,
                    ""DeliveryOutDate"" TEXT NULL,
                    ""DealerCode"" TEXT NULL,
                    ""DealerName"" TEXT NULL,
                    ""InCostStorageDate"" TEXT NOT NULL,
                    ""OutCostStorageDate"" TEXT NOT NULL,
                    ""CostStorageMonth"" INTEGER NOT NULL,
                    ""LevelStorage"" INTEGER NOT NULL,
                    ""DailyStorageRate"" INTEGER NOT NULL,
                    ""CostCoat"" INTEGER NOT NULL,
                    ""CostStorage"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentStorageDetails_PaymentStorages_PaymentStorageId"" FOREIGN KEY (""PaymentStorageId"") REFERENCES ""PaymentStorages"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentStorages_OrgId_PaymentStorageNo"" ON ""PaymentStorages"" (""OrgId"", ""PaymentStorageNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorages_OrgId_PmtMonth"" ON ""PaymentStorages"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorageDetails_PaymentStorageId"" ON ""PaymentStorageDetails"" (""PaymentStorageId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorageDetails_OrgId_VIN"" ON ""PaymentStorageDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeExtensionDispatches"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DispatchNo"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""BankCodeMonitor"" TEXT NULL,
                    ""FlagIsHTC"" TEXT NOT NULL,
                    ""NumberOfDaysExt"" INTEGER NOT NULL,
                    ""TotalCarCount"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""TotalCarsNotDelivered"" INTEGER NOT NULL,
                    ""TotalCarsDelivered"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""SignCAStatus"" INTEGER NOT NULL,
                    ""SignedBy"" TEXT NULL,
                    ""SignedAt"" TEXT NULL,
                    ""CertThumbprint"" TEXT NULL,
                    ""BankResponseRef"" TEXT NULL,
                    ""BankAcceptedAt"" TEXT NULL,
                    ""BankRejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeExtensionDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""DispatchId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""SOCode"" TEXT NULL,
                    ""DlrCtrNo"" TEXT NULL,
                    ""GuaranteeNo"" TEXT NULL,
                    ""BankGuaranteeNo"" TEXT NOT NULL,
                    ""GrtDateStart"" TEXT NOT NULL,
                    ""GrtDateExpired"" TEXT NOT NULL,
                    ""ExtendedDate"" TEXT NOT NULL,
                    ""GrtValue"" INTEGER NOT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""IsDelivered"" INTEGER NOT NULL,
                    ""DeliveryDate"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_GuaranteeExtensionDetails_GuaranteeExtensionDispatches_DispatchId"" FOREIGN KEY (""DispatchId"") REFERENCES ""GuaranteeExtensionDispatches"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDispatches_OrgId_DispatchNo"" ON ""GuaranteeExtensionDispatches"" (""OrgId"", ""DispatchNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDispatches_OrgId_BankCode"" ON ""GuaranteeExtensionDispatches"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDetails_DispatchId"" ON ""GuaranteeExtensionDetails"" (""DispatchId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDetails_OrgId_VIN"" ON ""GuaranteeExtensionDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeClaims"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ClaimNo"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""BankCodeMonitor"" TEXT NULL,
                    ""FlagIsHTC"" TEXT NOT NULL,
                    ""TotalCarCount"" INTEGER NOT NULL,
                    ""TotalClaimAmount"" INTEGER NOT NULL,
                    ""SettledAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""SignCAStatus"" INTEGER NOT NULL,
                    ""SignedBy"" TEXT NULL,
                    ""SignedAt"" TEXT NULL,
                    ""CertThumbprint"" TEXT NULL,
                    ""SentToBankAt"" TEXT NULL,
                    ""BankRefNo"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""BankRejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""CancelReason"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeClaimDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""ClaimId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""SOCode"" TEXT NULL,
                    ""ContractNo"" TEXT NULL,
                    ""GuaranteeNo"" TEXT NULL,
                    ""BankGuaranteeNo"" TEXT NOT NULL,
                    ""DateOpen"" TEXT NOT NULL,
                    ""DateExpired"" TEXT NOT NULL,
                    ""OverdueDays"" INTEGER NOT NULL,
                    ""UnitPriceActual"" INTEGER NOT NULL,
                    ""GrtValue"" INTEGER NOT NULL,
                    ""GrtPercent"" REAL NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_GuaranteeClaimDetails_GuaranteeClaims_ClaimId"" FOREIGN KEY (""ClaimId"") REFERENCES ""GuaranteeClaims"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_ClaimNo"" ON ""GuaranteeClaims"" (""OrgId"", ""ClaimNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_BankCode"" ON ""GuaranteeClaims"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_DealerCode"" ON ""GuaranteeClaims"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaimDetails_ClaimId"" ON ""GuaranteeClaimDetails"" (""ClaimId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaimDetails_OrgId_VIN"" ON ""GuaranteeClaimDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentAVNs"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentAVNNo"" TEXT NOT NULL,
                    ""PmtMonth"" TEXT NOT NULL,
                    ""SupplierCode"" TEXT NOT NULL,
                    ""SupplierName"" TEXT NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""AmountVAT"" INTEGER NOT NULL,
                    ""TotalAmountAfterVAT"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TCMSSignStatus"" INTEGER NOT NULL,
                    ""TCMSSignUser"" TEXT NULL,
                    ""TCMSSignDTime"" TEXT NULL,
                    ""HTVSignStatus"" INTEGER NOT NULL,
                    ""HTVSignUser"" TEXT NULL,
                    ""HTVSignDTime"" TEXT NULL,
                    ""Appr1By"" TEXT NULL,
                    ""Appr1DTime"" TEXT NULL,
                    ""Appr2By"" TEXT NULL,
                    ""Appr2DTime"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentAVNDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentAVNId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentAVNNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""AVNCode"" TEXT NOT NULL,
                    ""SerialNo"" TEXT NOT NULL,
                    ""UnitPriceAVN"" INTEGER NOT NULL,
                    ""AVNDate"" TEXT NULL,
                    ""InStorageDate"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentAVNDetails_PaymentAVNs_PaymentAVNId"" FOREIGN KEY (""PaymentAVNId"") REFERENCES ""PaymentAVNs"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentAVNs_OrgId_PaymentAVNNo"" ON ""PaymentAVNs"" (""OrgId"", ""PaymentAVNNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNs_OrgId_PmtMonth"" ON ""PaymentAVNs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNDetails_PaymentAVNId"" ON ""PaymentAVNDetails"" (""PaymentAVNId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNDetails_OrgId_VIN"" ON ""PaymentAVNDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentGPSs"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentGPSNo"" TEXT NOT NULL,
                    ""PmtMonth"" TEXT NOT NULL,
                    ""ContractNo"" TEXT NOT NULL,
                    ""ProviderCode"" TEXT NOT NULL,
                    ""ProviderName"" TEXT NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalPlanDays"" INTEGER NOT NULL,
                    ""TotalDeductDays"" INTEGER NOT NULL,
                    ""TotalActualDays"" INTEGER NOT NULL,
                    ""AmountTotal"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""UnitPriceVAT"" INTEGER NOT NULL,
                    ""TotalAmountVAT"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""HTVSignStatus"" INTEGER NOT NULL,
                    ""HTVSignUser"" TEXT NULL,
                    ""HTVSignDTime"" TEXT NULL,
                    ""TCMSSignStatus"" INTEGER NOT NULL,
                    ""TCMSSignUser"" TEXT NULL,
                    ""TCMSSignDTime"" TEXT NULL,
                    ""Appr1By"" TEXT NULL,
                    ""Appr1DTime"" TEXT NULL,
                    ""Appr2By"" TEXT NULL,
                    ""Appr2DTime"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""FilePath"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentGPSDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentGPSId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentGPSNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""CarID"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""GPSID"" TEXT NOT NULL,
                    ""ContractGPS"" TEXT NOT NULL,
                    ""GPSStartDate"" TEXT NULL,
                    ""RetailDate"" TEXT NULL,
                    ""CostGPSStartDate"" TEXT NOT NULL,
                    ""CostGPSEndDate"" TEXT NOT NULL,
                    ""PlanCostGPSDate"" INTEGER NOT NULL,
                    ""DeductDate"" INTEGER NOT NULL,
                    ""ActualCostGPSDate"" INTEGER NOT NULL,
                    ""PriceGPS"" INTEGER NOT NULL,
                    ""AmountGPS"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_PaymentGPSDetails_PaymentGPSs_PaymentGPSId"" FOREIGN KEY (""PaymentGPSId"") REFERENCES ""PaymentGPSs"" (""Id"") ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS ""UnitPriceGPSs"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ContractNo"" TEXT NOT NULL,
                    ""ProviderCode"" TEXT NOT NULL,
                    ""ProviderName"" TEXT NOT NULL,
                    ""DailyPrice"" INTEGER NOT NULL,
                    ""MonthlyRate"" INTEGER NOT NULL,
                    ""EffectiveStartDate"" TEXT NOT NULL,
                    ""IsActive"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGPSs_OrgId_PaymentGPSNo"" ON ""PaymentGPSs"" (""OrgId"", ""PaymentGPSNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSs_OrgId_PmtMonth"" ON ""PaymentGPSs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSDetails_PaymentGPSId"" ON ""PaymentGPSDetails"" (""PaymentGPSId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSDetails_OrgId_VIN"" ON ""PaymentGPSDetails"" (""OrgId"", ""VIN"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_UnitPriceGPSs_OrgId_ContractNo"" ON ""UnitPriceGPSs"" (""OrgId"", ""ContractNo"");

                CREATE TABLE IF NOT EXISTS ""FnExpStatements"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""CaNo"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""CAName"" TEXT NOT NULL,
                    ""TermFrom"" TEXT NOT NULL,
                    ""TermTo"" TEXT NOT NULL,
                    ""TermPrevFrom"" TEXT NOT NULL,
                    ""TermPrevTo"" TEXT NOT NULL,
                    ""FnExpPercent"" REAL NOT NULL,
                    ""PmtDsTCGPercent"" REAL NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalFnDepositAmount"" INTEGER NOT NULL,
                    ""TotalFnGrtAmount"" INTEGER NOT NULL,
                    ""TotalFnAmount"" INTEGER NOT NULL,
                    ""TotalPDAmount"" INTEGER NOT NULL,
                    ""TotalSettlementAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""DlrSignStatus"" INTEGER NOT NULL,
                    ""DlrSignUser"" TEXT NULL,
                    ""DlrSignDTime"" TEXT NULL,
                    ""HTCSignStatus"" INTEGER NOT NULL,
                    ""HTCSignUser"" TEXT NULL,
                    ""HTCSignDTime"" TEXT NULL,
                    ""DlrAppr1By"" TEXT NULL,
                    ""DlrAppr1DTime"" TEXT NULL,
                    ""HTCAppr1By"" TEXT NULL,
                    ""HTCAppr1DTime"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""CancelBy"" TEXT NULL,
                    ""CancelDTime"" TEXT NULL,
                    ""CancelReason"" TEXT NULL,
                    ""FilePathFnExp"" TEXT NULL,
                    ""FilePathPmtDc"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""FnExpDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""StatementId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CaNo"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""SOCode"" TEXT NULL,
                    ""AssemblyType"" INTEGER NOT NULL,
                    ""UnitPriceActual"" INTEGER NOT NULL,
                    ""SodApprovedDate"" TEXT NULL,
                    ""SodDepositDutyEndDate"" TEXT NULL,
                    ""TotalCompletedDate"" TEXT NULL,
                    ""DateStart"" TEXT NULL,
                    ""DateEnd"" TEXT NULL,
                    ""TermActual"" INTEGER NOT NULL,
                    ""FnDepositCountDate"" INTEGER NOT NULL,
                    ""FnDepositAmount"" INTEGER NOT NULL,
                    ""FnGrtCountDate"" INTEGER NOT NULL,
                    ""FnGrtAmount"" INTEGER NOT NULL,
                    ""FnTotalAmount"" INTEGER NOT NULL,
                    ""PDCountDate"" INTEGER NOT NULL,
                    ""PDAmount"" INTEGER NOT NULL,
                    ""CarTotalSettlement"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_FnExpDetails_FnExpStatements_StatementId"" FOREIGN KEY (""StatementId"") REFERENCES ""FnExpStatements"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_FnExpStatements_OrgId_CaNo"" ON ""FnExpStatements"" (""OrgId"", ""CaNo"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpStatements_OrgId_DealerCode"" ON ""FnExpStatements"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpDetails_StatementId"" ON ""FnExpDetails"" (""StatementId"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpDetails_OrgId_VIN"" ON ""FnExpDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""DisbursementRequests"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""TransNo"" TEXT NOT NULL,
                    ""TransType"" INTEGER NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""BizResNumber"" TEXT NOT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""PaymentAccount"" TEXT NULL,
                    ""PaymentBankName"" TEXT NULL,
                    ""ReceivingUnit"" TEXT NOT NULL,
                    ""ReceivingAccount"" TEXT NOT NULL,
                    ""ReceivingBank"" TEXT NOT NULL,
                    ""TotalCars"" INTEGER NOT NULL,
                    ""TotalContractAmount"" INTEGER NOT NULL,
                    ""TotalDisbursementAmount"" INTEGER NOT NULL,
                    ""ActualDisbursedAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""BankStatus"" INTEGER NOT NULL,
                    ""RefBankCode"" TEXT NULL,
                    ""BankRemark"" TEXT NULL,
                    ""LDNo"" TEXT NULL,
                    ""DisbursementDate"" TEXT NULL,
                    ""DisbursementTerm"" TEXT NULL,
                    ""DisbursementInterestRate"" REAL NOT NULL,
                    ""FirstInterestPmtDate"" TEXT NULL,
                    ""LoanLimit"" INTEGER NOT NULL,
                    ""MDNo"" TEXT NULL,
                    ""GrtAmount"" INTEGER NOT NULL,
                    ""GrtDateStart"" TEXT NULL,
                    ""GrtDateEnd"" TEXT NULL,
                    ""GrtTerm"" TEXT NULL,
                    ""GrtFee"" INTEGER NOT NULL,
                    ""LCNo"" TEXT NULL,
                    ""LCAmount"" INTEGER NOT NULL,
                    ""LCStartDate"" TEXT NULL,
                    ""LCEndDate"" TEXT NULL,
                    ""SentToBankAt"" TEXT NULL,
                    ""CompletedAt"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""CancelReason"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""DisbursementDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""RequestId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""TransNo"" TEXT NOT NULL,
                    ""DlrCtrNo"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NOT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""AssemblyType"" INTEGER NOT NULL,
                    ""ContractDate"" TEXT NULL,
                    ""PrincipalContractNo"" TEXT NULL,
                    ""PrincipalContractDate"" TEXT NULL,
                    ""DeliveryDate"" TEXT NULL,
                    ""Qty"" INTEGER NOT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""LtvRate"" REAL NOT NULL,
                    ""DisbursementAmount"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_DisbursementDetails_DisbursementRequests_RequestId"" FOREIGN KEY (""RequestId"") REFERENCES ""DisbursementRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS ""DisbursementFiles"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""RequestId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""TransNo"" TEXT NOT NULL,
                    ""DocType"" INTEGER NOT NULL,
                    ""FileName"" TEXT NOT NULL,
                    ""FilePath"" TEXT NULL,
                    ""SignStatus"" INTEGER NOT NULL,
                    ""SignedUser"" TEXT NULL,
                    ""SignedAt"" TEXT NULL,
                    ""CertSerialNumber"" TEXT NULL,
                    ""UploadDate"" TEXT NOT NULL,
                    CONSTRAINT ""FK_DisbursementFiles_DisbursementRequests_RequestId"" FOREIGN KEY (""RequestId"") REFERENCES ""DisbursementRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_TransNo"" ON ""DisbursementRequests"" (""OrgId"", ""TransNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_DealerCode"" ON ""DisbursementRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_BankCode"" ON ""DisbursementRequests"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementDetails_RequestId"" ON ""DisbursementDetails"" (""RequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementDetails_OrgId_DlrCtrNo"" ON ""DisbursementDetails"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementFiles_RequestId"" ON ""DisbursementFiles"" (""RequestId"");

                CREATE TABLE IF NOT EXISTS ""CancelBankMDRequests"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""CancelBankMDNo"" TEXT NOT NULL,
                    ""DlrCtrNo"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""BankCodeMD"" TEXT NOT NULL,
                    ""BankNameMD"" TEXT NOT NULL,
                    ""NewBankCodeMD"" TEXT NULL,
                    ""NewBankNameMD"" TEXT NULL,
                    ""GuaranteeType"" INTEGER NOT NULL,
                    ""ContractAmount"" INTEGER NOT NULL,
                    ""GuaranteeAmount"" INTEGER NOT NULL,
                    ""ReasonType"" INTEGER NOT NULL,
                    ""ReasonDescription"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""RemarkDlr"" TEXT NULL,
                    ""RemarkBank"" TEXT NULL,
                    ""RemarkHTC"" TEXT NULL,
                    ""ApproveBy"" TEXT NULL,
                    ""ApproveDateTime"" TEXT NULL,
                    ""FinishBy"" TEXT NULL,
                    ""FinishDTime"" TEXT NULL,
                    ""RejectBy"" TEXT NULL,
                    ""RejectDateTime"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelBy"" TEXT NULL,
                    ""CancelDateTime"" TEXT NULL,
                    ""CancelReason"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""CancelBankMDDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CancelBankMDId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CancelBankMDNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""SpecDescription"" TEXT NULL,
                    ""ColorExtNameVN"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""GuaranteeAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_CancelBankMDDetails_CancelBankMDRequests_CancelBankMDId"" FOREIGN KEY (""CancelBankMDId"") REFERENCES ""CancelBankMDRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_CancelBankMDNo"" ON ""CancelBankMDRequests"" (""OrgId"", ""CancelBankMDNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_DlrCtrNo"" ON ""CancelBankMDRequests"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_DealerCode"" ON ""CancelBankMDRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_BankCodeMD"" ON ""CancelBankMDRequests"" (""OrgId"", ""BankCodeMD"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_CancelBankMDId"" ON ""CancelBankMDDetails"" (""CancelBankMDId"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_OrgId_VIN"" ON ""CancelBankMDDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_OrgId_CancelBankMDNo"" ON ""CancelBankMDDetails"" (""OrgId"", ""CancelBankMDNo"");

                CREATE TABLE IF NOT EXISTS ""InsuranceClaimDebits"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""InsNo"" TEXT NOT NULL,
                    ""InsName"" TEXT NOT NULL,
                    ""RONo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""PlateNo"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""CustomerName"" TEXT NULL,
                    ""CustomerPhone"" TEXT NULL,
                    ""DebitDate"" TEXT NOT NULL,
                    ""DueDate"" TEXT NOT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""PaidAmount"" INTEGER NOT NULL,
                    ""RemainAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_DebitNo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_InsNo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""InsNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_RONo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_VIN"" ON ""InsuranceClaimDebits"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""InsurancePayments"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentNo"" TEXT NOT NULL,
                    ""InsNo"" TEXT NOT NULL,
                    ""InsName"" TEXT NOT NULL,
                    ""PayDate"" TEXT NOT NULL,
                    ""PayPersonName"" TEXT NOT NULL,
                    ""PayPersonIDCardNo"" TEXT NULL,
                    ""PayPersonPhone"" TEXT NULL,
                    ""PaymentAmount"" INTEGER NOT NULL,
                    ""PaymentMethod"" INTEGER NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""BankAccountNo"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""TotalAllocated"" INTEGER NOT NULL,
                    ""UnallocatedAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ConfirmedBy"" TEXT NULL,
                    ""ConfirmedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledBy"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""CancelReason"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsurancePayments_OrgId_PaymentNo"" ON ""InsurancePayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePayments_OrgId_InsNo"" ON ""InsurancePayments"" (""OrgId"", ""InsNo"");

                CREATE TABLE IF NOT EXISTS ""InsurancePaymentDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentId"" INTEGER NOT NULL,
                    ""DebitId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""RONo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""PlateNo"" TEXT NOT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""DebitAmountBefore"" INTEGER NOT NULL,
                    ""PaymentDetailAmount"" INTEGER NOT NULL,
                    ""DebitAmountLeft"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_InsurancePaymentDetails_InsurancePayments_PaymentId"" FOREIGN KEY (""PaymentId"") REFERENCES ""InsurancePayments"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_PaymentId"" ON ""InsurancePaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_DebitId"" ON ""InsurancePaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_OrgId_RONo"" ON ""InsurancePaymentDetails"" (""OrgId"", ""RONo"");

                CREATE TABLE IF NOT EXISTS ""SupplierDebits"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""SupplierCode"" TEXT NOT NULL,
                    ""SupplierName"" TEXT NOT NULL,
                    ""SupplierPhone"" TEXT NULL,
                    ""SupplierAddress"" TEXT NULL,
                    ""StockInNo"" TEXT NOT NULL,
                    ""StockInDate"" TEXT NULL,
                    ""OrderPartNo"" TEXT NULL,
                    ""Category"" TEXT NULL,
                    ""DebitDate"" TEXT NOT NULL,
                    ""DueDate"" TEXT NOT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""PaidAmount"" INTEGER NOT NULL,
                    ""RemainAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_DebitNo"" ON ""SupplierDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_SupplierCode"" ON ""SupplierDebits"" (""OrgId"", ""SupplierCode"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_StockInNo"" ON ""SupplierDebits"" (""OrgId"", ""StockInNo"");

                CREATE TABLE IF NOT EXISTS ""SupplierPayments"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentNo"" TEXT NOT NULL,
                    ""SupplierCode"" TEXT NOT NULL,
                    ""SupplierName"" TEXT NOT NULL,
                    ""PayDate"" TEXT NOT NULL,
                    ""PayPersonName"" TEXT NOT NULL,
                    ""PayPersonIDCardNo"" TEXT NULL,
                    ""PayPersonPhone"" TEXT NULL,
                    ""PaymentAmount"" INTEGER NOT NULL,
                    ""PaymentMethod"" INTEGER NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""BankAccountNo"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""TotalAllocated"" INTEGER NOT NULL,
                    ""UnallocatedAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ConfirmedBy"" TEXT NULL,
                    ""ConfirmedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledBy"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""CancelReason"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierPayments_OrgId_PaymentNo"" ON ""SupplierPayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPayments_OrgId_SupplierCode"" ON ""SupplierPayments"" (""OrgId"", ""SupplierCode"");

                CREATE TABLE IF NOT EXISTS ""SupplierPaymentDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentId"" INTEGER NOT NULL,
                    ""DebitId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""StockInNo"" TEXT NOT NULL,
                    ""OrderPartNo"" TEXT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""DebitAmountBefore"" INTEGER NOT NULL,
                    ""PaymentDetailAmount"" INTEGER NOT NULL,
                    ""DebitAmountLeft"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_SupplierPaymentDetails_SupplierPayments_PaymentId"" FOREIGN KEY (""PaymentId"") REFERENCES ""SupplierPayments"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_PaymentId"" ON ""SupplierPaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_DebitId"" ON ""SupplierPaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_OrgId_StockInNo"" ON ""SupplierPaymentDetails"" (""OrgId"", ""StockInNo"");

                CREATE TABLE IF NOT EXISTS ""CustomerDebits"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""CusId"" TEXT NOT NULL,
                    ""CusName"" TEXT NOT NULL,
                    ""Phone"" TEXT NULL,
                    ""Address"" TEXT NULL,
                    ""RONo"" TEXT NOT NULL,
                    ""RODate"" TEXT NULL,
                    ""PlateNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ServiceType"" TEXT NULL,
                    ""DebitDate"" TEXT NOT NULL,
                    ""DueDate"" TEXT NOT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""PaidAmount"" INTEGER NOT NULL,
                    ""RemainAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_DebitNo"" ON ""CustomerDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_CusId"" ON ""CustomerDebits"" (""OrgId"", ""CusId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_RONo"" ON ""CustomerDebits"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_PlateNo"" ON ""CustomerDebits"" (""OrgId"", ""PlateNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_VIN"" ON ""CustomerDebits"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""CustomerPayments"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentNo"" TEXT NOT NULL,
                    ""CusId"" TEXT NOT NULL,
                    ""CusName"" TEXT NOT NULL,
                    ""CusPhone"" TEXT NULL,
                    ""PlateNo"" TEXT NULL,
                    ""PayDate"" TEXT NOT NULL,
                    ""PayPersonName"" TEXT NOT NULL,
                    ""PayPersonIDCardNo"" TEXT NULL,
                    ""PayPersonPhone"" TEXT NULL,
                    ""PaymentAmount"" INTEGER NOT NULL,
                    ""PaymentMethod"" INTEGER NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""BankAccountNo"" TEXT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""TotalAllocated"" INTEGER NOT NULL,
                    ""UnallocatedAmount"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ConfirmedBy"" TEXT NULL,
                    ""ConfirmedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""CancelledBy"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL,
                    ""CancelReason"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerPayments_OrgId_PaymentNo"" ON ""CustomerPayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPayments_OrgId_CusId"" ON ""CustomerPayments"" (""OrgId"", ""CusId"");

                CREATE TABLE IF NOT EXISTS ""CustomerPaymentDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""PaymentId"" INTEGER NOT NULL,
                    ""DebitId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""DebitNo"" TEXT NOT NULL,
                    ""RONo"" TEXT NOT NULL,
                    ""PlateNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""DebitAmount"" INTEGER NOT NULL,
                    ""DebitAmountBefore"" INTEGER NOT NULL,
                    ""PaymentDetailAmount"" INTEGER NOT NULL,
                    ""DebitAmountLeft"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_CustomerPaymentDetails_CustomerPayments_PaymentId"" FOREIGN KEY (""PaymentId"") REFERENCES ""CustomerPayments"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_PaymentId"" ON ""CustomerPaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_DebitId"" ON ""CustomerPaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_OrgId_RONo"" ON ""CustomerPaymentDetails"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_OrgId_PlateNo"" ON ""CustomerPaymentDetails"" (""OrgId"", ""PlateNo"");

                CREATE TABLE IF NOT EXISTS ""ContractCancellations"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ContractCancelNo"" TEXT NOT NULL,
                    ""DlrContractNo"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""CancelDate"" TEXT NOT NULL,
                    ""SettlementType"" INTEGER NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""BankGuaranteeNo"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalContractAmount"" INTEGER NOT NULL,
                    ""TotalDepositPaid"" INTEGER NOT NULL,
                    ""TotalRefundAmount"" INTEGER NOT NULL,
                    ""TotalPenaltyAmount"" INTEGER NOT NULL,
                    ""TotalGuaranteeRelease"" INTEGER NOT NULL,
                    ""TransferContractNo"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""CancelReason"" TEXT NOT NULL,
                    ""BankTxnRef"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelReasonText"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_ContractCancelNo"" ON ""ContractCancellations"" (""OrgId"", ""ContractCancelNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_DlrContractNo"" ON ""ContractCancellations"" (""OrgId"", ""DlrContractNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_DealerCode"" ON ""ContractCancellations"" (""OrgId"", ""DealerCode"");

                CREATE TABLE IF NOT EXISTS ""ContractCancelDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CancellationId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""ContractCancelNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""ColorCode"" TEXT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""DepositPaid"" INTEGER NOT NULL,
                    ""RefundAmount"" INTEGER NOT NULL,
                    ""PenaltyAmount"" INTEGER NOT NULL,
                    ""GuaranteeAmount"" INTEGER NOT NULL,
                    ""TransferContractNo"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_ContractCancelDetails_ContractCancellations_CancellationId"" FOREIGN KEY (""CancellationId"") REFERENCES ""ContractCancellations"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_CancellationId"" ON ""ContractCancelDetails"" (""CancellationId"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_OrgId_VIN"" ON ""ContractCancelDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_OrgId_ContractCancelNo"" ON ""ContractCancelDetails"" (""OrgId"", ""ContractCancelNo"");

                CREATE TABLE IF NOT EXISTS ""CarDocRequests"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DRListCode"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""DealerCodeRecieve"" TEXT NULL,
                    ""DealerNameRecieve"" TEXT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""TypeCRR"" INTEGER NOT NULL,
                    ""LetterRepresentationNo"" TEXT NULL,
                    ""LetterRepresentationDate"" TEXT NULL,
                    ""RepresentativeName"" TEXT NULL,
                    ""RepresentativeIdCard"" TEXT NULL,
                    ""RepresentativePhone"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalCarAmount"" INTEGER NOT NULL,
                    ""TotalPaymentAmount"" INTEGER NOT NULL,
                    ""TotalGuaranteeAmount"" INTEGER NOT NULL,
                    ""AvgDutyCompletePercent"" REAL NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""ApprovedBy1"" TEXT NULL,
                    ""ApprovedDate1"" TEXT NULL,
                    ""BankApprovedBy"" TEXT NULL,
                    ""BankApprovedAt"" TEXT NULL,
                    ""ApprovedBy2"" TEXT NULL,
                    ""ApprovedDate2"" TEXT NULL,
                    ""HandoverDate"" TEXT NULL,
                    ""HandedOverBy"" TEXT NULL,
                    ""HandoverRecipient"" TEXT NULL,
                    ""ReturnDueDate"" TEXT NULL,
                    ""ActualReturnDate"" TEXT NULL,
                    ""ReturnedBy"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CancelReason"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_DRListCode"" ON ""CarDocRequests"" (""OrgId"", ""DRListCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_DealerCode"" ON ""CarDocRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_BankCode"" ON ""CarDocRequests"" (""OrgId"", ""BankCode"");

                CREATE TABLE IF NOT EXISTS ""CarDocRequestDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""DocReqId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""DRListCode"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""ColorNameVN"" TEXT NULL,
                    ""ContractNo"" TEXT NULL,
                    ""UnitPriceActual"" INTEGER NOT NULL,
                    ""PaymentPercent"" REAL NOT NULL,
                    ""DepositPercent"" REAL NOT NULL,
                    ""GuaranteePercent"" REAL NOT NULL,
                    ""DutyCompletePercent"" REAL NOT NULL,
                    ""BankGuaranteeNo"" TEXT NULL,
                    ""CONo"" TEXT NULL,
                    ""CQNo"" TEXT NULL,
                    ""CustomsDeclarationNo"" TEXT NULL,
                    ""HTCInvoiceNo"" TEXT NULL,
                    ""DocumentsGiven"" TEXT NOT NULL,
                    ""BankApprStatus"" INTEGER NOT NULL,
                    ""BankApprDTime"" TEXT NULL,
                    ""BankApprBy"" TEXT NULL,
                    ""BankApprNote"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""HandoverDate"" TEXT NULL,
                    ""ReturnDueDate"" TEXT NULL,
                    ""ActualReturnDate"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_CarDocRequestDetails_CarDocRequests_DocReqId"" FOREIGN KEY (""DocReqId"") REFERENCES ""CarDocRequests"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_DocReqId"" ON ""CarDocRequestDetails"" (""DocReqId"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_OrgId_VIN"" ON ""CarDocRequestDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_OrgId_DRListCode"" ON ""CarDocRequestDetails"" (""OrgId"", ""DRListCode"");

                CREATE TABLE IF NOT EXISTS ""HTCInvoices"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""HTCInvoiceCode"" TEXT NOT NULL,
                    ""HTCInvoiceNo"" TEXT NULL,
                    ""InvoiceSymbol"" TEXT NOT NULL,
                    ""InvoiceDate"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NOT NULL,
                    ""BuyerTaxCode"" TEXT NOT NULL,
                    ""BuyerAddress"" TEXT NOT NULL,
                    ""BuyerLegalRepresentative"" TEXT NULL,
                    ""PaymentMethod"" TEXT NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BankName"" TEXT NULL,
                    ""BankAccountNo"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""VATAmount"" INTEGER NOT NULL,
                    ""TotalPayment"" INTEGER NOT NULL,
                    ""PaidAmount"" INTEGER NOT NULL,
                    ""RemainAmount"" INTEGER NOT NULL,
                    ""OS_HDDT_InvoiceCode"" TEXT NULL,
                    ""SourceInvoiceCode"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Root_HTCInvoiceNo"" TEXT NULL,
                    ""Adj_DeleteReason"" TEXT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedDate"" TEXT NULL,
                    ""IssuedBy"" TEXT NULL,
                    ""IssuedDate"" TEXT NULL,
                    ""CreatedBy"" TEXT NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedBy"" TEXT NULL,
                    ""UpdatedAt"" TEXT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_HTCInvoiceCode"" ON ""HTCInvoices"" (""OrgId"", ""HTCInvoiceCode"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_HTCInvoiceNo"" ON ""HTCInvoices"" (""OrgId"", ""HTCInvoiceNo"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_DealerCode"" ON ""HTCInvoices"" (""OrgId"", ""DealerCode"");

                CREATE TABLE IF NOT EXISTS ""HTCInvoiceDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""InvoiceId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""HTCInvoiceCode"" TEXT NOT NULL,
                    ""ItemNo"" INTEGER NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NOT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""ColorVN"" TEXT NULL,
                    ""ProductionYear"" TEXT NOT NULL,
                    ""CabinCONo"" TEXT NULL,
                    ""CQNo"" TEXT NULL,
                    ""CustomsDeclarationNo"" TEXT NULL,
                    ""SOCode"" TEXT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""VATRate"" REAL NOT NULL,
                    ""VATAmount"" INTEGER NOT NULL,
                    ""TotalPrice"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_HTCInvoiceDetails_HTCInvoices_InvoiceId"" FOREIGN KEY (""InvoiceId"") REFERENCES ""HTCInvoices"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_InvoiceId"" ON ""HTCInvoiceDetails"" (""InvoiceId"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_OrgId_VIN"" ON ""HTCInvoiceDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_OrgId_HTCInvoiceCode"" ON ""HTCInvoiceDetails"" (""OrgId"", ""HTCInvoiceCode"");

                CREATE TABLE IF NOT EXISTS ""LettersOfCredit"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""LCNo"" TEXT NOT NULL,
                    ""ContractNo"" TEXT NOT NULL,
                    ""BankName"" TEXT NOT NULL,
                    ""BankCode"" TEXT NULL,
                    ""BeneficiaryName"" TEXT NULL,
                    ""BeneficiaryCountry"" TEXT NULL,
                    ""ApplicantName"" TEXT NULL,
                    ""LCType"" INTEGER NOT NULL,
                    ""Currency"" TEXT NOT NULL,
                    ""LCAmount"" TEXT NOT NULL,
                    ""ExchangeRate"" TEXT NOT NULL,
                    ""LCAmountVND"" INTEGER NOT NULL,
                    ""UtilizedAmount"" TEXT NOT NULL,
                    ""RemainingAmount"" TEXT NOT NULL,
                    ""DateOpen"" TEXT NOT NULL,
                    ""DateExpired"" TEXT NULL,
                    ""LatestShipmentDate"" TEXT NULL,
                    ""TermDays"" INTEGER NOT NULL,
                    ""PackingListNo"" TEXT NULL,
                    ""PaymentTerm"" TEXT NULL,
                    ""PortOfLoading"" TEXT NULL,
                    ""PortOfDischarge"" TEXT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""OpenedBy"" TEXT NULL,
                    ""OpenedAt"" TEXT NULL,
                    ""AmendedBy"" TEXT NULL,
                    ""AmendedAt"" TEXT NULL,
                    ""SettledBy"" TEXT NULL,
                    ""SettledAt"" TEXT NULL,
                    ""RejectReason"" TEXT NULL,
                    ""CreatedBy"" TEXT NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_LCNo"" ON ""LettersOfCredit"" (""OrgId"", ""LCNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_ContractNo"" ON ""LettersOfCredit"" (""OrgId"", ""ContractNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_BankCode"" ON ""LettersOfCredit"" (""OrgId"", ""BankCode"");

                CREATE TABLE IF NOT EXISTS ""LetterOfCreditDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""LCId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""LCNo"" TEXT NOT NULL,
                    ""ItemNo"" INTEGER NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NOT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""ColorCode"" TEXT NULL,
                    ""WorkOrderNo"" TEXT NULL,
                    ""PortCode"" TEXT NULL,
                    ""PlantCode"" TEXT NULL,
                    ""UnitPrice"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_LetterOfCreditDetails_LettersOfCredit_LCId"" FOREIGN KEY (""LCId"") REFERENCES ""LettersOfCredit"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_LCId"" ON ""LetterOfCreditDetails"" (""LCId"");
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_OrgId_VIN"" ON ""LetterOfCreditDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_OrgId_LCNo"" ON ""LetterOfCreditDetails"" (""OrgId"", ""LCNo"");
                CREATE TABLE IF NOT EXISTS ""BankDealers"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NULL,
                    ""BankCode"" TEXT NOT NULL,
                    ""BankName"" TEXT NULL,
                    ""CreditContractNo"" TEXT NULL,
                    ""CreditContractDate"" TEXT NULL,
                    ""CreditAmount"" TEXT NULL,
                    ""BankBranchCode"" TEXT NULL,
                    ""BankBranchName"" TEXT NULL,
                    ""FlagBankGrt"" INTEGER NOT NULL,
                    ""FlagBankPmt"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedBy"" TEXT NULL,
                    ""UpdatedAt"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_DealerCode_BankCode"" ON ""BankDealers"" (""OrgId"", ""DealerCode"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_DealerCode"" ON ""BankDealers"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_BankCode"" ON ""BankDealers"" (""OrgId"", ""BankCode"");
                CREATE TABLE IF NOT EXISTS ""AccountingVoucherUpdates"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""BatchNo"" TEXT NOT NULL,
                    ""Description"" TEXT NULL,
                    ""TotalItems"" INTEGER NOT NULL,
                    ""UpdatedItems"" INTEGER NOT NULL,
                    ""SkippedItems"" INTEGER NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""AppliedBy"" TEXT NULL,
                    ""AppliedAt"" TEXT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdates_OrgId_BatchNo"" ON ""AccountingVoucherUpdates"" (""OrgId"", ""BatchNo"");
                CREATE TABLE IF NOT EXISTS ""AccountingVoucherUpdateDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""AccountingVoucherUpdateId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""PaymentNo"" TEXT NOT NULL,
                    ""OldAccountingRecordNo"" TEXT NULL,
                    ""NewAccountingRecordNo"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""Note"" TEXT NULL,
                    CONSTRAINT ""FK_AccountingVoucherUpdateDetails_AccountingVoucherUpdates_AccountingVoucherUpdateId"" FOREIGN KEY (""AccountingVoucherUpdateId"") REFERENCES ""AccountingVoucherUpdates"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdateDetails_AccountingVoucherUpdateId"" ON ""AccountingVoucherUpdateDetails"" (""AccountingVoucherUpdateId"");
                CREATE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdateDetails_OrgId_PaymentNo"" ON ""AccountingVoucherUpdateDetails"" (""OrgId"", ""PaymentNo"");
                CREATE TABLE IF NOT EXISTS ""CalendarEntries"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""CalendarType"" TEXT NOT NULL,
                    ""Date"" TEXT NOT NULL,
                    ""StatusValue"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedBy"" TEXT NULL,
                    ""UpdatedAt"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CalendarEntries_OrgId_CalendarType_Date"" ON ""CalendarEntries"" (""OrgId"", ""CalendarType"", ""Date"");
                CREATE INDEX IF NOT EXISTS ""IX_CalendarEntries_OrgId_CalendarType"" ON ""CalendarEntries"" (""OrgId"", ""CalendarType"");
                CREATE TABLE IF NOT EXISTS ""DealerContracts"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""DlrCtrNo"" TEXT NOT NULL,
                    ""DCPType"" INTEGER NOT NULL,
                    ""DealerCode"" TEXT NOT NULL,
                    ""DealerName"" TEXT NULL,
                    ""BankCodeMD"" TEXT NULL,
                    ""BankNameMD"" TEXT NULL,
                    ""ContractDate"" TEXT NOT NULL,
                    ""TotalAmount"" INTEGER NOT NULL,
                    ""TotalVehicles"" INTEGER NOT NULL,
                    ""FilePath"" TEXT NULL,
                    ""FlagDlrCtrAdjust"" INTEGER NOT NULL,
                    ""DlrSignStatus"" INTEGER NOT NULL,
                    ""HTCSignStatus"" INTEGER NOT NULL,
                    ""DlrCtrStatus"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedBy"" TEXT NULL,
                    ""UpdatedAt"" TEXT NULL,
                    ""DlrApprovedBy"" TEXT NULL,
                    ""DlrApprovedAt"" TEXT NULL,
                    ""HTCApproved1By"" TEXT NULL,
                    ""HTCApproved1At"" TEXT NULL,
                    ""HTCApproved2By"" TEXT NULL,
                    ""HTCApproved2At"" TEXT NULL,
                    ""RejectedBy"" TEXT NULL,
                    ""RejectedAt"" TEXT NULL,
                    ""CancelledBy"" TEXT NULL,
                    ""CancelledAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""DealerContractDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""DealerContractId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""CarId"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""ModelCode"" TEXT NULL,
                    ""ModelName"" TEXT NULL,
                    ""SpecCode"" TEXT NULL,
                    ""ColorCode"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""OriginNo"" TEXT NULL,
                    ""ProductionYear"" INTEGER NOT NULL,
                    ""UnitPrice"" INTEGER NOT NULL,
                    ""DlrCtrStatusDtl"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_DealerContractDetails_DealerContracts_DealerContractId"" FOREIGN KEY (""DealerContractId"") REFERENCES ""DealerContracts"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_DlrCtrNo"" ON ""DealerContracts"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_DealerCode"" ON ""DealerContracts"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_BankCodeMD"" ON ""DealerContracts"" (""OrgId"", ""BankCodeMD"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_DealerContractId"" ON ""DealerContractDetails"" (""DealerContractId"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_OrgId_VIN"" ON ""DealerContractDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_OrgId_DlrCtrNo"" ON ""DealerContractDetails"" (""OrgId"", ""DlrCtrNo"");
                CREATE TABLE IF NOT EXISTS ""StorageRearranges"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""StorageRearrangeNo"" TEXT NOT NULL,
                    ""RearrangeStatus"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy1"" TEXT NULL,
                    ""ApprovedAt1"" TEXT NULL,
                    ""ApprovedBy2"" TEXT NULL,
                    ""ApprovedAt2"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""StorageRearrangeDetails"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""StorageRearrangeId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""StorageRearrangeNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""StorageCodeFrom"" TEXT NOT NULL,
                    ""StorageCodeTo"" TEXT NOT NULL,
                    ""ExpectedStartDate"" TEXT NOT NULL,
                    ""ExpectedEndDate"" TEXT NULL,
                    ""Remark"" TEXT NULL,
                    ""RearrangeDtlStatus"" INTEGER NOT NULL,
                    ""ConfirmBy"" TEXT NULL,
                    ""ConfirmDate"" TEXT NULL,
                    CONSTRAINT ""FK_StorageRearrangeDetails_StorageRearranges_StorageRearrangeId"" FOREIGN KEY (""StorageRearrangeId"") REFERENCES ""StorageRearranges"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StorageRearranges_OrgId_StorageRearrangeNo"" ON ""StorageRearranges"" (""OrgId"", ""StorageRearrangeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_StorageRearrangeId"" ON ""StorageRearrangeDetails"" (""StorageRearrangeId"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_OrgId_VIN"" ON ""StorageRearrangeDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_OrgId_StorageRearrangeNo"" ON ""StorageRearrangeDetails"" (""OrgId"", ""StorageRearrangeNo"");

                CREATE TABLE IF NOT EXISTS ""TransportFeeVersions"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""TFVCode"" TEXT NOT NULL,
                    ""Description"" TEXT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""CreatedDate"" TEXT NOT NULL,
                    ""AppliedDate"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedBy"" TEXT NULL,
                    ""UpdatedAt"" TEXT NULL,
                    ""Remark"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""TransportFeeRates"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""VersionId"" INTEGER NOT NULL,
                    ""OrgId"" TEXT NOT NULL,
                    ""TFVCode"" TEXT NOT NULL,
                    ""ProvinceCodeFrom"" TEXT NOT NULL,
                    ""ProvinceNameFrom"" TEXT NULL,
                    ""DistrictCodeFrom"" TEXT NOT NULL,
                    ""DistrictNameFrom"" TEXT NULL,
                    ""ProvinceCodeTo"" TEXT NOT NULL,
                    ""ProvinceNameTo"" TEXT NULL,
                    ""DistrictCodeTo"" TEXT NOT NULL,
                    ""DistrictNameTo"" TEXT NULL,
                    ""TransporterCode"" TEXT NOT NULL,
                    ""TransporterName"" TEXT NULL,
                    ""ModelCode"" TEXT NOT NULL,
                    ""ModelName"" TEXT NULL,
                    ""ValFee"" INTEGER NOT NULL,
                    ""ExpectedDays"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    CONSTRAINT ""FK_TransportFeeRates_TransportFeeVersions_VersionId"" FOREIGN KEY (""VersionId"") REFERENCES ""TransportFeeVersions"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TransportFeeVersions_OrgId_TFVCode"" ON ""TransportFeeVersions"" (""OrgId"", ""TFVCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_VersionId"" ON ""TransportFeeRates"" (""VersionId"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_TFVCode"" ON ""TransportFeeRates"" (""OrgId"", ""TFVCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_TransporterCode"" ON ""TransportFeeRates"" (""OrgId"", ""TransporterCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_Route"" ON ""TransportFeeRates"" (""OrgId"", ""ProvinceCodeFrom"", ""DistrictCodeFrom"", ""ProvinceCodeTo"", ""DistrictCodeTo"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeAttachFiles"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GuaranteeAttachFiles"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""GuaranteeId"" INTEGER NOT NULL,
                    ""GuaranteeNo"" TEXT NOT NULL,
                    ""FileIndex"" INTEGER NOT NULL,
                    ""GrtFilePath"" TEXT NOT NULL,
                    ""GrtFileName"" TEXT NOT NULL,
                    ""FileSizeInBytes"" INTEGER NOT NULL,
                    ""GrtFileRemark"" TEXT NULL,
                    ""LogLUBy"" TEXT NULL,
                    ""LogLUDateTime"" TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeAttachFileHis"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GuaranteeAttachFileHis"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""GuaranteeNo"" TEXT NOT NULL,
                    ""FileIndex"" INTEGER NOT NULL,
                    ""GrtFilePath"" TEXT NOT NULL,
                    ""GrtFileName"" TEXT NOT NULL,
                    ""FileSizeInBytes"" INTEGER NOT NULL,
                    ""GrtFileRemark"" TEXT NULL,
                    ""LogLUBy"" TEXT NULL,
                    ""LogLUDateTime"" TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_OrgId_GuaranteeNo_FileIndex"" ON ""GuaranteeAttachFiles"" (""OrgId"", ""GuaranteeNo"", ""FileIndex"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_GuaranteeId"" ON ""GuaranteeAttachFiles"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_OrgId_GuaranteeNo"" ON ""GuaranteeAttachFiles"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFileHis_OrgId_GuaranteeNo"" ON ""GuaranteeAttachFileHis"" (""OrgId"", ""GuaranteeNo"");
            ");
        }

        // Nâng cấp DB SQLite cũ: thêm cột mới nếu thực sự còn thiếu (không dùng ALTER vô điều kiện
        // như trước vì sẽ báo 'duplicate column name' trên DB tạo bằng CREATE TABLE ở trên).
        if (db.Database.IsSqlite())
        {
            await EnsureSqliteColumnAsync(db, "PaymentGuaranteeDetails", "DateWarning", "\"DateWarning\" TEXT NULL");
            await EnsureSqliteColumnAsync(db, "PaymentGuaranteeDetails", "DateExpired", "\"DateExpired\" TEXT NULL");
            await EnsureSqliteColumnAsync(db, "PaymentGuaranteeDetails", "FlagDtlDiscount", "\"FlagDtlDiscount\" TEXT NOT NULL DEFAULT '0'");
        }

        // Báº£ng Äá» nghá»‹ xuáº¥t hÃ³a Ä‘Æ¡n / giao há»“ sÆ¡ (RD_ReqInvoice) â€” táº¡o cho DB Ä‘Ã£ tá»“n táº¡i tá»« phiÃªn trÆ°á»›c
        if (db.Database.IsSqlite())
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ReqInvoices"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ReqInvoices"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ReqIVNo"" TEXT NOT NULL,
                    ""ReqIVStatus"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS ""ReqInvoiceDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ReqInvoiceDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""OrgId"" TEXT NOT NULL,
                    ""ReqInvoiceId"" INTEGER NOT NULL,
                    ""ReqIVNo"" TEXT NOT NULL,
                    ""VIN"" TEXT NOT NULL,
                    ""CarId"" TEXT NULL,
                    ""ModelCode"" TEXT NULL,
                    ""ModelName"" TEXT NULL,
                    ""ColorCode"" TEXT NULL,
                    ""ColorName"" TEXT NULL,
                    ""EngineNo"" TEXT NULL,
                    ""TypeRDReqIv"" INTEGER NOT NULL,
                    ""DealerCode"" TEXT NULL,
                    ""DealerName"" TEXT NULL,
                    ""MortageBankCode"" TEXT NULL,
                    ""GuaranteeNo"" TEXT NULL,
                    ""PGBankCode"" TEXT NULL,
                    ""PGBankCodeMonitor"" TEXT NULL,
                    ""PGDateExpired"" TEXT NULL,
                    ""HTCInvoiceNo"" TEXT NULL,
                    ""TCGInvoiceNo"" TEXT NULL,
                    ""DlrCtrNo"" TEXT NULL,
                    ""ProvinceName"" TEXT NULL,
                    ""RDReqIvDtlStatus"" INTEGER NOT NULL,
                    ""Remark"" TEXT NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedBy"" TEXT NULL,
                    ""ApprovedAt"" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ReqInvoices_OrgId_ReqIVNo"" ON ""ReqInvoices"" (""OrgId"", ""ReqIVNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_ReqInvoiceId"" ON ""ReqInvoiceDetails"" (""ReqInvoiceId"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_OrgId_VIN"" ON ""ReqInvoiceDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_OrgId_ReqIVNo"" ON ""ReqInvoiceDetails"" (""OrgId"", ""ReqIVNo"");
            ");
        }
        else if (db.Database.IsNpgsql())
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ReqInvoices"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ReqIVNo"" text NOT NULL,
                    ""ReqIVStatus"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp NULL
                );
                CREATE TABLE IF NOT EXISTS ""ReqInvoiceDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ReqInvoiceId"" bigint NOT NULL,
                    ""ReqIVNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NULL,
                    ""ModelName"" text NULL,
                    ""ColorCode"" text NULL,
                    ""ColorName"" text NULL,
                    ""EngineNo"" text NULL,
                    ""TypeRDReqIv"" integer NOT NULL,
                    ""DealerCode"" text NULL,
                    ""DealerName"" text NULL,
                    ""MortageBankCode"" text NULL,
                    ""GuaranteeNo"" text NULL,
                    ""PGBankCode"" text NULL,
                    ""PGBankCodeMonitor"" text NULL,
                    ""PGDateExpired"" timestamp NULL,
                    ""HTCInvoiceNo"" text NULL,
                    ""TCGInvoiceNo"" text NULL,
                    ""DlrCtrNo"" text NULL,
                    ""ProvinceName"" text NULL,
                    ""RDReqIvDtlStatus"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ReqInvoices_OrgId_ReqIVNo"" ON ""ReqInvoices"" (""OrgId"", ""ReqIVNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_ReqInvoiceId"" ON ""ReqInvoiceDetails"" (""ReqInvoiceId"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_OrgId_VIN"" ON ""ReqInvoiceDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_ReqInvoiceDetails_OrgId_ReqIVNo"" ON ""ReqInvoiceDetails"" (""OrgId"", ""ReqIVNo"");
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
                    ""DateWarning"" timestamp without time zone NULL,
                    ""DateExpired"" timestamp without time zone NULL,
                    ""FlagDtlDiscount"" text NOT NULL DEFAULT '0',
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGuarantees_OrgId_GuaranteeNo"" ON ""PaymentGuarantees"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_GuaranteeId"" ON ""PaymentGuaranteeDetails"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGuaranteeDetails_OrgId_ItemRefNo"" ON ""PaymentGuaranteeDetails"" (""OrgId"", ""ItemRefNo"");
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN IF NOT EXISTS ""DateWarning"" timestamp without time zone NULL;
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN IF NOT EXISTS ""DateExpired"" timestamp without time zone NULL;
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN IF NOT EXISTS ""FlagDtlDiscount"" text NOT NULL DEFAULT '0';

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

                CREATE TABLE IF NOT EXISTS ""BankBillMinutes"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""BankBillMnNo"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""PartnerCode"" text NOT NULL,
                    ""PartnerName"" text NOT NULL,
                    ""BankBillDate"" timestamp without time zone NOT NULL,
                    ""BankBillPrintDate"" timestamp without time zone NULL,
                    ""BankBillReciveDate"" timestamp without time zone NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalClaimAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""HandedOverBy"" text NULL,
                    ""HandedOverAt"" timestamp without time zone NULL,
                    ""BankReceivedBy"" text NULL,
                    ""BankReceivedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledAt"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""BankBillMinutesDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""MinutesId"" bigint NOT NULL REFERENCES ""BankBillMinutes"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""EngineNo"" text NULL,
                    ""CONo"" text NULL,
                    ""CabinCONo"" text NULL,
                    ""DeclarationNo"" text NULL,
                    ""BankGuaranteeNo"" text NULL,
                    ""HTCInvoiceNo"" text NULL,
                    ""TCGInvoiceNo"" text NULL,
                    ""TransportMinutesNo"" text NULL,
                    ""ClaimAmount"" bigint NOT NULL,
                    ""GuaranteeDateStart"" timestamp without time zone NULL,
                    ""GuaranteeDateOpen"" timestamp without time zone NULL,
                    ""NumberOfDaysDeferred"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_BankBillMinutes_OrgId_BankBillMnNo"" ON ""BankBillMinutes"" (""OrgId"", ""BankBillMnNo"");
                CREATE INDEX IF NOT EXISTS ""IX_BankBillMinutesDetails_MinutesId"" ON ""BankBillMinutesDetails"" (""MinutesId"");
                CREATE INDEX IF NOT EXISTS ""IX_BankBillMinutesDetails_OrgId_VIN"" ON ""BankBillMinutesDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentPDIs"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PmtPDINo"" text NOT NULL,
                    ""PmtMonth"" text NOT NULL,
                    ""ServiceUnitCode"" text NOT NULL,
                    ""ServiceUnitName"" text NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalCostIn"" bigint NOT NULL,
                    ""TotalCostOut"" bigint NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""AmountVAT"" bigint NOT NULL,
                    ""TotalAmountAfterVAT"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""TCMSSignStatus"" integer NOT NULL,
                    ""TCMSSignUser"" text NULL,
                    ""TCMSSignDTime"" timestamp without time zone NULL,
                    ""HTVSignStatus"" integer NOT NULL,
                    ""HTVSignUser"" text NULL,
                    ""HTVSignDTime"" timestamp without time zone NULL,
                    ""Appr1By"" text NULL,
                    ""Appr1DTime"" timestamp without time zone NULL,
                    ""Appr2By"" text NULL,
                    ""Appr2DTime"" timestamp without time zone NULL,
                    ""PaidBy"" text NULL,
                    ""PaidAt"" timestamp without time zone NULL,
                    ""BankTxnRef"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentPDIDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentPDIId"" bigint NOT NULL REFERENCES ""PaymentPDIs"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorExtNameVN"" text NULL,
                    ""StorageCodeInit"" text NOT NULL,
                    ""StoreDate"" timestamp without time zone NULL,
                    ""DeliveryOutDate"" timestamp without time zone NULL,
                    ""DlvMnNo"" text NULL,
                    ""DealerCode"" text NULL,
                    ""CostInCheck"" bigint NOT NULL,
                    ""CostOutCheck"" bigint NOT NULL,
                    ""TotalCostCheck"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentPDIs_OrgId_PmtPDINo"" ON ""PaymentPDIs"" (""OrgId"", ""PmtPDINo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIs_OrgId_PmtMonth"" ON ""PaymentPDIs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIDetails_PaymentPDIId"" ON ""PaymentPDIDetails"" (""PaymentPDIId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentPDIDetails_OrgId_VIN"" ON ""PaymentPDIDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""LatePaymentPenalties"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PenaltyRecordNo"" text NOT NULL,
                    ""SOCode"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NULL,
                    ""ContractNo"" text NULL,
                    ""SOApprovedDate"" timestamp without time zone NULL,
                    ""TotalApprovedQuantity"" integer NOT NULL,
                    ""TotalUnitPriceActual"" bigint NOT NULL,
                    ""MaxDelayDaysDeposit"" integer NOT NULL,
                    ""MaxDelayDaysGrtOpen"" integer NOT NULL,
                    ""MaxDelayDaysGrtPay"" integer NOT NULL,
                    ""MaxDelayDays60Pmt"" integer NOT NULL,
                    ""MaxDelayDaysRemain"" integer NOT NULL,
                    ""TotalDatePenalty"" integer NOT NULL,
                    ""PenaltyRateAnnual"" numeric NOT NULL,
                    ""AmountPenaltySystem"" bigint NOT NULL,
                    ""PenalizeActual"" bigint NOT NULL,
                    ""WaivedAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""AdjustmentReason"" text NULL,
                    ""PaymentProofRef"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""CalculatedAt"" timestamp without time zone NULL,
                    ""ReviewedBy"" text NULL,
                    ""ReviewedAt"" timestamp without time zone NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledAt"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""LatePaymentPenaltyDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PenaltyId"" bigint NOT NULL REFERENCES ""LatePaymentPenalties"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""CarId"" text NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""ColorName"" text NULL,
                    ""UnitPriceActual"" bigint NOT NULL,
                    ""DepositDueDate"" timestamp without time zone NULL,
                    ""ActualDepositDate"" timestamp without time zone NULL,
                    ""GrtDueDate"" timestamp without time zone NULL,
                    ""ActualGrtDate"" timestamp without time zone NULL,
                    ""GrtPayDueDate"" timestamp without time zone NULL,
                    ""ActualGrtPayDate"" timestamp without time zone NULL,
                    ""Payment60DueDate"" timestamp without time zone NULL,
                    ""Actual60PayDate"" timestamp without time zone NULL,
                    ""PaymentRemainDueDate"" timestamp without time zone NULL,
                    ""ActualRemainPayDate"" timestamp without time zone NULL,
                    ""DelayDaysDeposit"" integer NOT NULL,
                    ""DelayDaysGrtOpen"" integer NOT NULL,
                    ""DelayDaysGrtPay"" integer NOT NULL,
                    ""DelayDays60Pmt"" integer NOT NULL,
                    ""DelayDaysRemain"" integer NOT NULL,
                    ""MaxDelayDays"" integer NOT NULL,
                    ""ItemPenaltyAmount"" bigint NOT NULL,
                    ""ActualItemPenalty"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_LatePaymentPenalties_OrgId_PenaltyRecordNo"" ON ""LatePaymentPenalties"" (""OrgId"", ""PenaltyRecordNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenalties_OrgId_SOCode"" ON ""LatePaymentPenalties"" (""OrgId"", ""SOCode"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenaltyDetails_PenaltyId"" ON ""LatePaymentPenaltyDetails"" (""PenaltyId"");
                CREATE INDEX IF NOT EXISTS ""IX_LatePaymentPenaltyDetails_OrgId_VIN"" ON ""LatePaymentPenaltyDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""TransportInsPayments"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""TransportInsNo"" text NOT NULL,
                    ""PmtMonth"" text NOT NULL,
                    ""TransporterCode"" text NOT NULL,
                    ""TransporterName"" text NOT NULL,
                    ""InsuranceCompanyCode"" text NOT NULL,
                    ""InsuranceCompanyName"" text NOT NULL,
                    ""InsuranceContractNo"" text NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalTransportCost"" bigint NOT NULL,
                    ""TotalDelayPenalty"" bigint NOT NULL,
                    ""TotalInsuranceCost"" bigint NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""TotalBeforeVAT"" bigint NOT NULL,
                    ""AmountVAT"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""TCMSSignStatus"" integer NOT NULL,
                    ""TCMSSignUser"" text NULL,
                    ""TCMSSignDTime"" timestamp without time zone NULL,
                    ""HTVSignStatus"" integer NOT NULL,
                    ""HTVSignUser"" text NULL,
                    ""HTVSignDTime"" timestamp without time zone NULL,
                    ""Appr1By"" text NULL,
                    ""Appr1DTime"" timestamp without time zone NULL,
                    ""Appr2By"" text NULL,
                    ""Appr2DTime"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""BankTxnRef"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""TransportInsPaymentDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""TransportInsPaymentId"" bigint NOT NULL REFERENCES ""TransportInsPayments"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorName"" text NULL,
                    ""FStorageCode"" text NOT NULL,
                    ""FProvinceName"" text NULL,
                    ""TStorageCode"" text NOT NULL,
                    ""TProvinceName"" text NULL,
                    ""TranspReqType"" text NOT NULL,
                    ""DlvMnNo"" text NULL,
                    ""DlvStartDate"" timestamp without time zone NULL,
                    ""ExpectedDays"" integer NOT NULL,
                    ""ExpectedDlvEndDate"" timestamp without time zone NULL,
                    ""DlvEndDate"" timestamp without time zone NULL,
                    ""DelayDays"" integer NOT NULL,
                    ""TFValReal"" bigint NOT NULL,
                    ""TPValReal"" bigint NOT NULL,
                    ""PriceCar"" bigint NOT NULL,
                    ""InsurancePercent"" numeric NOT NULL,
                    ""InsuranceCost"" bigint NOT NULL,
                    ""Val_Transport"" bigint NOT NULL,
                    ""StandardRemark"" text NULL,
                    ""FProvinceRemark"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TransportInsPayments_OrgId_TransportInsNo"" ON ""TransportInsPayments"" (""OrgId"", ""TransportInsNo"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPayments_OrgId_PmtMonth"" ON ""TransportInsPayments"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPaymentDetails_TransportInsPaymentId"" ON ""TransportInsPaymentDetails"" (""TransportInsPaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportInsPaymentDetails_OrgId_VIN"" ON ""TransportInsPaymentDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentStorages"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentStorageNo"" text NOT NULL,
                    ""PmtMonth"" text NOT NULL,
                    ""StorageOperatorCode"" text NOT NULL,
                    ""StorageOperatorName"" text NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalCoatCost"" bigint NOT NULL,
                    ""TotalStorageCost"" bigint NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""UnitPriceVAT"" bigint NOT NULL,
                    ""AmountTotal"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""TCMSSignStatus"" integer NOT NULL,
                    ""TCMSSignUser"" text NULL,
                    ""TCMSSignDTime"" timestamp without time zone NULL,
                    ""HTVSignStatus"" integer NOT NULL,
                    ""HTVSignUser"" text NULL,
                    ""HTVSignDTime"" timestamp without time zone NULL,
                    ""Appr1By"" text NULL,
                    ""Appr1DTime"" timestamp without time zone NULL,
                    ""Appr2By"" text NULL,
                    ""Appr2DTime"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""BankTxnRef"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentStorageDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentStorageId"" bigint NOT NULL REFERENCES ""PaymentStorages"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentStorageNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorExtNameVN"" text NULL,
                    ""StorageCodeInit"" text NOT NULL,
                    ""StorageDate"" timestamp without time zone NOT NULL,
                    ""ApprovedDate2"" timestamp without time zone NULL,
                    ""DeliveryOutDate"" timestamp without time zone NULL,
                    ""DealerCode"" text NULL,
                    ""DealerName"" text NULL,
                    ""InCostStorageDate"" timestamp without time zone NOT NULL,
                    ""OutCostStorageDate"" timestamp without time zone NOT NULL,
                    ""CostStorageMonth"" integer NOT NULL,
                    ""LevelStorage"" integer NOT NULL,
                    ""DailyStorageRate"" bigint NOT NULL,
                    ""CostCoat"" bigint NOT NULL,
                    ""CostStorage"" bigint NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentStorages_OrgId_PaymentStorageNo"" ON ""PaymentStorages"" (""OrgId"", ""PaymentStorageNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorages_OrgId_PmtMonth"" ON ""PaymentStorages"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorageDetails_PaymentStorageId"" ON ""PaymentStorageDetails"" (""PaymentStorageId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentStorageDetails_OrgId_VIN"" ON ""PaymentStorageDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeExtensionDispatches"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DispatchNo"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""BankCodeMonitor"" text NULL,
                    ""FlagIsHTC"" text NOT NULL,
                    ""NumberOfDaysExt"" integer NOT NULL,
                    ""TotalCarCount"" integer NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""TotalCarsNotDelivered"" integer NOT NULL,
                    ""TotalCarsDelivered"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""SignCAStatus"" integer NOT NULL,
                    ""SignedBy"" text NULL,
                    ""SignedAt"" timestamp with time zone NULL,
                    ""CertThumbprint"" text NULL,
                    ""BankResponseRef"" text NULL,
                    ""BankAcceptedAt"" timestamp with time zone NULL,
                    ""BankRejectReason"" text NULL,
                    ""CancelledAt"" timestamp with time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeExtensionDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""DispatchId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""CarId"" text NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorName"" text NULL,
                    ""SOCode"" text NULL,
                    ""DlrCtrNo"" text NULL,
                    ""GuaranteeNo"" text NULL,
                    ""BankGuaranteeNo"" text NOT NULL,
                    ""GrtDateStart"" timestamp with time zone NOT NULL,
                    ""GrtDateExpired"" timestamp with time zone NOT NULL,
                    ""ExtendedDate"" timestamp with time zone NOT NULL,
                    ""GrtValue"" bigint NOT NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""IsDelivered"" boolean NOT NULL,
                    ""DeliveryDate"" timestamp with time zone NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_GuaranteeExtensionDetails_GuaranteeExtensionDispatches_DispatchId"" FOREIGN KEY (""DispatchId"") REFERENCES ""GuaranteeExtensionDispatches"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDispatches_OrgId_DispatchNo"" ON ""GuaranteeExtensionDispatches"" (""OrgId"", ""DispatchNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDispatches_OrgId_BankCode"" ON ""GuaranteeExtensionDispatches"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDetails_DispatchId"" ON ""GuaranteeExtensionDetails"" (""DispatchId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeExtensionDetails_OrgId_VIN"" ON ""GuaranteeExtensionDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeClaims"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ClaimNo"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""BankCodeMonitor"" text NULL,
                    ""FlagIsHTC"" text NOT NULL,
                    ""TotalCarCount"" integer NOT NULL,
                    ""TotalClaimAmount"" bigint NOT NULL,
                    ""SettledAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""SignCAStatus"" integer NOT NULL,
                    ""SignedBy"" text NULL,
                    ""SignedAt"" timestamp with time zone NULL,
                    ""CertThumbprint"" text NULL,
                    ""SentToBankAt"" timestamp with time zone NULL,
                    ""BankRefNo"" text NULL,
                    ""SettledAt"" timestamp with time zone NULL,
                    ""SettledBy"" text NULL,
                    ""BankTxnRef"" text NULL,
                    ""BankRejectReason"" text NULL,
                    ""CancelledAt"" timestamp with time zone NULL,
                    ""CancelReason"" text NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeClaimDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""ClaimId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""CarId"" text NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorName"" text NULL,
                    ""SOCode"" text NULL,
                    ""ContractNo"" text NULL,
                    ""GuaranteeNo"" text NULL,
                    ""BankGuaranteeNo"" text NOT NULL,
                    ""DateOpen"" timestamp with time zone NOT NULL,
                    ""DateExpired"" timestamp with time zone NOT NULL,
                    ""OverdueDays"" integer NOT NULL,
                    ""UnitPriceActual"" bigint NOT NULL,
                    ""GrtValue"" bigint NOT NULL,
                    ""GrtPercent"" numeric NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_GuaranteeClaimDetails_GuaranteeClaims_ClaimId"" FOREIGN KEY (""ClaimId"") REFERENCES ""GuaranteeClaims"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_ClaimNo"" ON ""GuaranteeClaims"" (""OrgId"", ""ClaimNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_BankCode"" ON ""GuaranteeClaims"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaims_OrgId_DealerCode"" ON ""GuaranteeClaims"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaimDetails_ClaimId"" ON ""GuaranteeClaimDetails"" (""ClaimId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeClaimDetails_OrgId_VIN"" ON ""GuaranteeClaimDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentAVNs"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentAVNNo"" text NOT NULL,
                    ""PmtMonth"" text NOT NULL,
                    ""SupplierCode"" text NOT NULL,
                    ""SupplierName"" text NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""AmountVAT"" bigint NOT NULL,
                    ""TotalAmountAfterVAT"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""TCMSSignStatus"" integer NOT NULL,
                    ""TCMSSignUser"" text NULL,
                    ""TCMSSignDTime"" timestamp with time zone NULL,
                    ""HTVSignStatus"" integer NOT NULL,
                    ""HTVSignUser"" text NULL,
                    ""HTVSignDTime"" timestamp with time zone NULL,
                    ""Appr1By"" text NULL,
                    ""Appr1DTime"" timestamp with time zone NULL,
                    ""Appr2By"" text NULL,
                    ""Appr2DTime"" timestamp with time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp with time zone NULL,
                    ""BankTxnRef"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp with time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentAVNDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentAVNId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentAVNNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""EngineNo"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorName"" text NULL,
                    ""AVNCode"" text NOT NULL,
                    ""SerialNo"" text NOT NULL,
                    ""UnitPriceAVN"" bigint NOT NULL,
                    ""AVNDate"" timestamp with time zone NULL,
                    ""InStorageDate"" timestamp with time zone NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_PaymentAVNDetails_PaymentAVNs_PaymentAVNId"" FOREIGN KEY (""PaymentAVNId"") REFERENCES ""PaymentAVNs"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentAVNs_OrgId_PaymentAVNNo"" ON ""PaymentAVNs"" (""OrgId"", ""PaymentAVNNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNs_OrgId_PmtMonth"" ON ""PaymentAVNs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNDetails_PaymentAVNId"" ON ""PaymentAVNDetails"" (""PaymentAVNId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentAVNDetails_OrgId_VIN"" ON ""PaymentAVNDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""PaymentGPSs"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentGPSNo"" text NOT NULL,
                    ""PmtMonth"" text NOT NULL,
                    ""ContractNo"" text NOT NULL,
                    ""ProviderCode"" text NOT NULL,
                    ""ProviderName"" text NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalPlanDays"" integer NOT NULL,
                    ""TotalDeductDays"" integer NOT NULL,
                    ""TotalActualDays"" integer NOT NULL,
                    ""AmountTotal"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""UnitPriceVAT"" bigint NOT NULL,
                    ""TotalAmountVAT"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""HTVSignStatus"" integer NOT NULL,
                    ""HTVSignUser"" text NULL,
                    ""HTVSignDTime"" timestamp with time zone NULL,
                    ""TCMSSignStatus"" integer NOT NULL,
                    ""TCMSSignUser"" text NULL,
                    ""TCMSSignDTime"" timestamp with time zone NULL,
                    ""Appr1By"" text NULL,
                    ""Appr1DTime"" timestamp with time zone NULL,
                    ""Appr2By"" text NULL,
                    ""Appr2DTime"" timestamp with time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp with time zone NULL,
                    ""BankTxnRef"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelledAt"" timestamp with time zone NULL,
                    ""FilePath"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""PaymentGPSDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentGPSId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentGPSNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""EngineNo"" text NULL,
                    ""CarID"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""GPSID"" text NOT NULL,
                    ""ContractGPS"" text NOT NULL,
                    ""GPSStartDate"" timestamp with time zone NULL,
                    ""RetailDate"" timestamp with time zone NULL,
                    ""CostGPSStartDate"" timestamp with time zone NOT NULL,
                    ""CostGPSEndDate"" timestamp with time zone NOT NULL,
                    ""PlanCostGPSDate"" integer NOT NULL,
                    ""DeductDate"" integer NOT NULL,
                    ""ActualCostGPSDate"" integer NOT NULL,
                    ""PriceGPS"" bigint NOT NULL,
                    ""AmountGPS"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_PaymentGPSDetails_PaymentGPSs_PaymentGPSId"" FOREIGN KEY (""PaymentGPSId"") REFERENCES ""PaymentGPSs"" (""Id"") ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS ""UnitPriceGPSs"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ContractNo"" text NOT NULL,
                    ""ProviderCode"" text NOT NULL,
                    ""ProviderName"" text NOT NULL,
                    ""DailyPrice"" bigint NOT NULL,
                    ""MonthlyRate"" bigint NOT NULL,
                    ""EffectiveStartDate"" timestamp with time zone NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PaymentGPSs_OrgId_PaymentGPSNo"" ON ""PaymentGPSs"" (""OrgId"", ""PaymentGPSNo"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSs_OrgId_PmtMonth"" ON ""PaymentGPSs"" (""OrgId"", ""PmtMonth"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSDetails_PaymentGPSId"" ON ""PaymentGPSDetails"" (""PaymentGPSId"");
                CREATE INDEX IF NOT EXISTS ""IX_PaymentGPSDetails_OrgId_VIN"" ON ""PaymentGPSDetails"" (""OrgId"", ""VIN"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_UnitPriceGPSs_OrgId_ContractNo"" ON ""UnitPriceGPSs"" (""OrgId"", ""ContractNo"");

                CREATE TABLE IF NOT EXISTS ""FnExpStatements"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""CaNo"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""CAName"" text NOT NULL,
                    ""TermFrom"" timestamp NOT NULL,
                    ""TermTo"" timestamp NOT NULL,
                    ""TermPrevFrom"" timestamp NOT NULL,
                    ""TermPrevTo"" timestamp NOT NULL,
                    ""FnExpPercent"" numeric NOT NULL,
                    ""PmtDsTCGPercent"" numeric NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalFnDepositAmount"" bigint NOT NULL,
                    ""TotalFnGrtAmount"" bigint NOT NULL,
                    ""TotalFnAmount"" bigint NOT NULL,
                    ""TotalPDAmount"" bigint NOT NULL,
                    ""TotalSettlementAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""DlrSignStatus"" integer NOT NULL,
                    ""DlrSignUser"" text NULL,
                    ""DlrSignDTime"" timestamp NULL,
                    ""HTCSignStatus"" integer NOT NULL,
                    ""HTCSignUser"" text NULL,
                    ""HTCSignDTime"" timestamp NULL,
                    ""DlrAppr1By"" text NULL,
                    ""DlrAppr1DTime"" timestamp NULL,
                    ""HTCAppr1By"" text NULL,
                    ""HTCAppr1DTime"" timestamp NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp NULL,
                    ""BankTxnRef"" text NULL,
                    ""CancelBy"" text NULL,
                    ""CancelDTime"" timestamp NULL,
                    ""CancelReason"" text NULL,
                    ""FilePathFnExp"" text NULL,
                    ""FilePathPmtDc"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""FnExpDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""StatementId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""CaNo"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorName"" text NULL,
                    ""SOCode"" text NULL,
                    ""AssemblyType"" integer NOT NULL,
                    ""UnitPriceActual"" bigint NOT NULL,
                    ""SodApprovedDate"" timestamp NULL,
                    ""SodDepositDutyEndDate"" timestamp NULL,
                    ""TotalCompletedDate"" timestamp NULL,
                    ""DateStart"" timestamp NULL,
                    ""DateEnd"" timestamp NULL,
                    ""TermActual"" integer NOT NULL,
                    ""FnDepositCountDate"" integer NOT NULL,
                    ""FnDepositAmount"" bigint NOT NULL,
                    ""FnGrtCountDate"" integer NOT NULL,
                    ""FnGrtAmount"" bigint NOT NULL,
                    ""FnTotalAmount"" bigint NOT NULL,
                    ""PDCountDate"" integer NOT NULL,
                    ""PDAmount"" bigint NOT NULL,
                    ""CarTotalSettlement"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_FnExpDetails_FnExpStatements_StatementId"" FOREIGN KEY (""StatementId"") REFERENCES ""FnExpStatements"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_FnExpStatements_OrgId_CaNo"" ON ""FnExpStatements"" (""OrgId"", ""CaNo"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpStatements_OrgId_DealerCode"" ON ""FnExpStatements"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpDetails_StatementId"" ON ""FnExpDetails"" (""StatementId"");
                CREATE INDEX IF NOT EXISTS ""IX_FnExpDetails_OrgId_VIN"" ON ""FnExpDetails"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""DisbursementRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""TransNo"" text NOT NULL,
                    ""TransType"" integer NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""BizResNumber"" text NOT NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""PaymentAccount"" text NULL,
                    ""PaymentBankName"" text NULL,
                    ""ReceivingUnit"" text NOT NULL,
                    ""ReceivingAccount"" text NOT NULL,
                    ""ReceivingBank"" text NOT NULL,
                    ""TotalCars"" integer NOT NULL,
                    ""TotalContractAmount"" bigint NOT NULL,
                    ""TotalDisbursementAmount"" bigint NOT NULL,
                    ""ActualDisbursedAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""BankStatus"" integer NOT NULL,
                    ""RefBankCode"" text NULL,
                    ""BankRemark"" text NULL,
                    ""LDNo"" text NULL,
                    ""DisbursementDate"" timestamp NULL,
                    ""DisbursementTerm"" text NULL,
                    ""DisbursementInterestRate"" numeric NOT NULL,
                    ""FirstInterestPmtDate"" timestamp NULL,
                    ""LoanLimit"" bigint NOT NULL,
                    ""MDNo"" text NULL,
                    ""GrtAmount"" bigint NOT NULL,
                    ""GrtDateStart"" timestamp NULL,
                    ""GrtDateEnd"" timestamp NULL,
                    ""GrtTerm"" text NULL,
                    ""GrtFee"" bigint NOT NULL,
                    ""LCNo"" text NULL,
                    ""LCAmount"" bigint NOT NULL,
                    ""LCStartDate"" timestamp NULL,
                    ""LCEndDate"" timestamp NULL,
                    ""SentToBankAt"" timestamp NULL,
                    ""CompletedAt"" timestamp NULL,
                    ""CancelledAt"" timestamp NULL,
                    ""CancelReason"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""DisbursementDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""RequestId"" bigint NOT NULL REFERENCES ""DisbursementRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""TransNo"" text NOT NULL,
                    ""DlrCtrNo"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NOT NULL,
                    ""SpecDescription"" text NULL,
                    ""AssemblyType"" integer NOT NULL,
                    ""ContractDate"" timestamp NULL,
                    ""PrincipalContractNo"" text NULL,
                    ""PrincipalContractDate"" timestamp NULL,
                    ""DeliveryDate"" timestamp NULL,
                    ""Qty"" integer NOT NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""LtvRate"" numeric NOT NULL,
                    ""DisbursementAmount"" bigint NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE TABLE IF NOT EXISTS ""DisbursementFiles"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""RequestId"" bigint NOT NULL REFERENCES ""DisbursementRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""TransNo"" text NOT NULL,
                    ""DocType"" integer NOT NULL,
                    ""FileName"" text NOT NULL,
                    ""FilePath"" text NULL,
                    ""SignStatus"" integer NOT NULL,
                    ""SignedUser"" text NULL,
                    ""SignedAt"" timestamp NULL,
                    ""CertSerialNumber"" text NULL,
                    ""UploadDate"" timestamp NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_TransNo"" ON ""DisbursementRequests"" (""OrgId"", ""TransNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_DealerCode"" ON ""DisbursementRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementRequests_OrgId_BankCode"" ON ""DisbursementRequests"" (""OrgId"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementDetails_RequestId"" ON ""DisbursementDetails"" (""RequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementDetails_OrgId_DlrCtrNo"" ON ""DisbursementDetails"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DisbursementFiles_RequestId"" ON ""DisbursementFiles"" (""RequestId"");

                CREATE TABLE IF NOT EXISTS ""CancelBankMDRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""CancelBankMDNo"" text NOT NULL,
                    ""DlrCtrNo"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""BankCodeMD"" text NOT NULL,
                    ""BankNameMD"" text NOT NULL,
                    ""NewBankCodeMD"" text NULL,
                    ""NewBankNameMD"" text NULL,
                    ""GuaranteeType"" integer NOT NULL,
                    ""ContractAmount"" bigint NOT NULL,
                    ""GuaranteeAmount"" bigint NOT NULL,
                    ""ReasonType"" integer NOT NULL,
                    ""ReasonDescription"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""RemarkDlr"" text NULL,
                    ""RemarkBank"" text NULL,
                    ""RemarkHTC"" text NULL,
                    ""ApproveBy"" text NULL,
                    ""ApproveDateTime"" timestamp NULL,
                    ""FinishBy"" text NULL,
                    ""FinishDTime"" timestamp NULL,
                    ""RejectBy"" text NULL,
                    ""RejectDateTime"" timestamp NULL,
                    ""RejectReason"" text NULL,
                    ""CancelBy"" text NULL,
                    ""CancelDateTime"" timestamp NULL,
                    ""CancelReason"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""CancelBankMDDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""CancelBankMDId"" bigint NOT NULL REFERENCES ""CancelBankMDRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""CancelBankMDNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""SpecDescription"" text NULL,
                    ""ColorExtNameVN"" text NULL,
                    ""EngineNo"" text NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""GuaranteeAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_CancelBankMDNo"" ON ""CancelBankMDRequests"" (""OrgId"", ""CancelBankMDNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_DlrCtrNo"" ON ""CancelBankMDRequests"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_DealerCode"" ON ""CancelBankMDRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDRequests_OrgId_BankCodeMD"" ON ""CancelBankMDRequests"" (""OrgId"", ""BankCodeMD"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_CancelBankMDId"" ON ""CancelBankMDDetails"" (""CancelBankMDId"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_OrgId_VIN"" ON ""CancelBankMDDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_CancelBankMDDetails_OrgId_CancelBankMDNo"" ON ""CancelBankMDDetails"" (""OrgId"", ""CancelBankMDNo"");

                CREATE TABLE IF NOT EXISTS ""InsuranceClaimDebits"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""InsNo"" text NOT NULL,
                    ""InsName"" text NOT NULL,
                    ""RONo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""PlateNo"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""CustomerName"" text NULL,
                    ""CustomerPhone"" text NULL,
                    ""DebitDate"" timestamp without time zone NOT NULL,
                    ""DueDate"" timestamp without time zone NOT NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""PaidAmount"" bigint NOT NULL,
                    ""RemainAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_DebitNo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_InsNo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""InsNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_RONo"" ON ""InsuranceClaimDebits"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsuranceClaimDebits_OrgId_VIN"" ON ""InsuranceClaimDebits"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""InsurancePayments"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentNo"" text NOT NULL,
                    ""InsNo"" text NOT NULL,
                    ""InsName"" text NOT NULL,
                    ""PayDate"" timestamp without time zone NOT NULL,
                    ""PayPersonName"" text NOT NULL,
                    ""PayPersonIDCardNo"" text NULL,
                    ""PayPersonPhone"" text NULL,
                    ""PaymentAmount"" bigint NOT NULL,
                    ""PaymentMethod"" integer NOT NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""BankAccountNo"" text NULL,
                    ""BankTxnRef"" text NULL,
                    ""TotalAllocated"" bigint NOT NULL,
                    ""UnallocatedAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ConfirmedBy"" text NULL,
                    ""ConfirmedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledBy"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""CancelReason"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsurancePayments_OrgId_PaymentNo"" ON ""InsurancePayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePayments_OrgId_InsNo"" ON ""InsurancePayments"" (""OrgId"", ""InsNo"");

                CREATE TABLE IF NOT EXISTS ""InsurancePaymentDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentId"" bigint NOT NULL REFERENCES ""InsurancePayments"" (""Id"") ON DELETE CASCADE,
                    ""DebitId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""RONo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""PlateNo"" text NOT NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""DebitAmountBefore"" bigint NOT NULL,
                    ""PaymentDetailAmount"" bigint NOT NULL,
                    ""DebitAmountLeft"" bigint NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_PaymentId"" ON ""InsurancePaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_DebitId"" ON ""InsurancePaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_InsurancePaymentDetails_OrgId_RONo"" ON ""InsurancePaymentDetails"" (""OrgId"", ""RONo"");

                CREATE TABLE IF NOT EXISTS ""SupplierDebits"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""SupplierCode"" text NOT NULL,
                    ""SupplierName"" text NOT NULL,
                    ""SupplierPhone"" text NULL,
                    ""SupplierAddress"" text NULL,
                    ""StockInNo"" text NOT NULL,
                    ""StockInDate"" timestamp without time zone NULL,
                    ""OrderPartNo"" text NULL,
                    ""Category"" text NULL,
                    ""DebitDate"" timestamp without time zone NOT NULL,
                    ""DueDate"" timestamp without time zone NOT NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""PaidAmount"" bigint NOT NULL,
                    ""RemainAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_DebitNo"" ON ""SupplierDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_SupplierCode"" ON ""SupplierDebits"" (""OrgId"", ""SupplierCode"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_StockInNo"" ON ""SupplierDebits"" (""OrgId"", ""StockInNo"");

                CREATE TABLE IF NOT EXISTS ""SupplierPayments"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentNo"" text NOT NULL,
                    ""SupplierCode"" text NOT NULL,
                    ""SupplierName"" text NOT NULL,
                    ""PayDate"" timestamp without time zone NOT NULL,
                    ""PayPersonName"" text NOT NULL,
                    ""PayPersonIDCardNo"" text NULL,
                    ""PayPersonPhone"" text NULL,
                    ""PaymentAmount"" bigint NOT NULL,
                    ""PaymentMethod"" integer NOT NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""BankAccountNo"" text NULL,
                    ""BankTxnRef"" text NULL,
                    ""TotalAllocated"" bigint NOT NULL,
                    ""UnallocatedAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ConfirmedBy"" text NULL,
                    ""ConfirmedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledBy"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""CancelReason"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierPayments_OrgId_PaymentNo"" ON ""SupplierPayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPayments_OrgId_SupplierCode"" ON ""SupplierPayments"" (""OrgId"", ""SupplierCode"");

                CREATE TABLE IF NOT EXISTS ""SupplierPaymentDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentId"" bigint NOT NULL REFERENCES ""SupplierPayments"" (""Id"") ON DELETE CASCADE,
                    ""DebitId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""StockInNo"" text NOT NULL,
                    ""OrderPartNo"" text NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""DebitAmountBefore"" bigint NOT NULL,
                    ""PaymentDetailAmount"" bigint NOT NULL,
                    ""DebitAmountLeft"" bigint NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_PaymentId"" ON ""SupplierPaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_DebitId"" ON ""SupplierPaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_OrgId_StockInNo"" ON ""SupplierPaymentDetails"" (""OrgId"", ""StockInNo"");

                CREATE TABLE IF NOT EXISTS ""CustomerDebits"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""CusId"" text NOT NULL,
                    ""CusName"" text NOT NULL,
                    ""Phone"" text NULL,
                    ""Address"" text NULL,
                    ""RONo"" text NOT NULL,
                    ""RODate"" timestamp without time zone NULL,
                    ""PlateNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ServiceType"" text NULL,
                    ""DebitDate"" timestamp without time zone NOT NULL,
                    ""DueDate"" timestamp without time zone NOT NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""PaidAmount"" bigint NOT NULL,
                    ""RemainAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_DebitNo"" ON ""CustomerDebits"" (""OrgId"", ""DebitNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_CusId"" ON ""CustomerDebits"" (""OrgId"", ""CusId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_RONo"" ON ""CustomerDebits"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_PlateNo"" ON ""CustomerDebits"" (""OrgId"", ""PlateNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerDebits_OrgId_VIN"" ON ""CustomerDebits"" (""OrgId"", ""VIN"");

                CREATE TABLE IF NOT EXISTS ""CustomerPayments"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentNo"" text NOT NULL,
                    ""CusId"" text NOT NULL,
                    ""CusName"" text NOT NULL,
                    ""CusPhone"" text NULL,
                    ""PlateNo"" text NULL,
                    ""PayDate"" timestamp without time zone NOT NULL,
                    ""PayPersonName"" text NOT NULL,
                    ""PayPersonIDCardNo"" text NULL,
                    ""PayPersonPhone"" text NULL,
                    ""PaymentAmount"" bigint NOT NULL,
                    ""PaymentMethod"" integer NOT NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""BankAccountNo"" text NULL,
                    ""BankTxnRef"" text NULL,
                    ""TotalAllocated"" bigint NOT NULL,
                    ""UnallocatedAmount"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ConfirmedBy"" text NULL,
                    ""ConfirmedAt"" timestamp without time zone NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""CancelledBy"" text NULL,
                    ""CancelledAt"" timestamp without time zone NULL,
                    ""CancelReason"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerPayments_OrgId_PaymentNo"" ON ""CustomerPayments"" (""OrgId"", ""PaymentNo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPayments_OrgId_CusId"" ON ""CustomerPayments"" (""OrgId"", ""CusId"");

                CREATE TABLE IF NOT EXISTS ""CustomerPaymentDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""PaymentId"" bigint NOT NULL REFERENCES ""CustomerPayments"" (""Id"") ON DELETE CASCADE,
                    ""DebitId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""DebitNo"" text NOT NULL,
                    ""RONo"" text NOT NULL,
                    ""PlateNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""DebitAmount"" bigint NOT NULL,
                    ""DebitAmountBefore"" bigint NOT NULL,
                    ""PaymentDetailAmount"" bigint NOT NULL,
                    ""DebitAmountLeft"" bigint NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_PaymentId"" ON ""CustomerPaymentDetails"" (""PaymentId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_DebitId"" ON ""CustomerPaymentDetails"" (""DebitId"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_OrgId_RONo"" ON ""CustomerPaymentDetails"" (""OrgId"", ""RONo"");
                CREATE INDEX IF NOT EXISTS ""IX_CustomerPaymentDetails_OrgId_PlateNo"" ON ""CustomerPaymentDetails"" (""OrgId"", ""PlateNo"");

                CREATE TABLE IF NOT EXISTS ""ContractCancellations"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""ContractCancelNo"" text NOT NULL,
                    ""DlrContractNo"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""CancelDate"" timestamp without time zone NOT NULL,
                    ""SettlementType"" integer NOT NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""BankGuaranteeNo"" text NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalContractAmount"" bigint NOT NULL,
                    ""TotalDepositPaid"" bigint NOT NULL,
                    ""TotalRefundAmount"" bigint NOT NULL,
                    ""TotalPenaltyAmount"" bigint NOT NULL,
                    ""TotalGuaranteeRelease"" bigint NOT NULL,
                    ""TransferContractNo"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""CancelReason"" text NOT NULL,
                    ""BankTxnRef"" text NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp without time zone NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedAt"" timestamp without time zone NULL,
                    ""RejectReason"" text NULL,
                    ""CancelReasonText"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_ContractCancelNo"" ON ""ContractCancellations"" (""OrgId"", ""ContractCancelNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_DlrContractNo"" ON ""ContractCancellations"" (""OrgId"", ""DlrContractNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancellations_OrgId_DealerCode"" ON ""ContractCancellations"" (""OrgId"", ""DealerCode"");

                CREATE TABLE IF NOT EXISTS ""ContractCancelDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""CancellationId"" bigint NOT NULL REFERENCES ""ContractCancellations"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""ContractCancelNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""ColorCode"" text NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""DepositPaid"" bigint NOT NULL,
                    ""RefundAmount"" bigint NOT NULL,
                    ""PenaltyAmount"" bigint NOT NULL,
                    ""GuaranteeAmount"" bigint NOT NULL,
                    ""TransferContractNo"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_CancellationId"" ON ""ContractCancelDetails"" (""CancellationId"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_OrgId_VIN"" ON ""ContractCancelDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_ContractCancelDetails_OrgId_ContractCancelNo"" ON ""ContractCancelDetails"" (""OrgId"", ""ContractCancelNo"");

                CREATE TABLE IF NOT EXISTS ""CarDocRequests"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DRListCode"" text NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""DealerCodeRecieve"" text NULL,
                    ""DealerNameRecieve"" text NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""TypeCRR"" integer NOT NULL,
                    ""LetterRepresentationNo"" text NULL,
                    ""LetterRepresentationDate"" timestamp with time zone NULL,
                    ""RepresentativeName"" text NULL,
                    ""RepresentativeIdCard"" text NULL,
                    ""RepresentativePhone"" text NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalCarAmount"" bigint NOT NULL,
                    ""TotalPaymentAmount"" bigint NOT NULL,
                    ""TotalGuaranteeAmount"" bigint NOT NULL,
                    ""AvgDutyCompletePercent"" numeric NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""ApprovedBy1"" text NULL,
                    ""ApprovedDate1"" timestamp with time zone NULL,
                    ""BankApprovedBy"" text NULL,
                    ""BankApprovedAt"" timestamp with time zone NULL,
                    ""ApprovedBy2"" text NULL,
                    ""ApprovedDate2"" timestamp with time zone NULL,
                    ""HandoverDate"" timestamp with time zone NULL,
                    ""HandedOverBy"" text NULL,
                    ""HandoverRecipient"" text NULL,
                    ""ReturnDueDate"" timestamp with time zone NULL,
                    ""ActualReturnDate"" timestamp with time zone NULL,
                    ""ReturnedBy"" text NULL,
                    ""RejectReason"" text NULL,
                    ""CancelReason"" text NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_DRListCode"" ON ""CarDocRequests"" (""OrgId"", ""DRListCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_DealerCode"" ON ""CarDocRequests"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequests_OrgId_BankCode"" ON ""CarDocRequests"" (""OrgId"", ""BankCode"");

                CREATE TABLE IF NOT EXISTS ""CarDocRequestDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""DocReqId"" bigint NOT NULL REFERENCES ""CarDocRequests"" (""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""DRListCode"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""CarId"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""EngineNo"" text NULL,
                    ""ColorNameVN"" text NULL,
                    ""ContractNo"" text NULL,
                    ""UnitPriceActual"" bigint NOT NULL,
                    ""PaymentPercent"" numeric NOT NULL,
                    ""DepositPercent"" numeric NOT NULL,
                    ""GuaranteePercent"" numeric NOT NULL,
                    ""DutyCompletePercent"" numeric NOT NULL,
                    ""BankGuaranteeNo"" text NULL,
                    ""CONo"" text NULL,
                    ""CQNo"" text NULL,
                    ""CustomsDeclarationNo"" text NULL,
                    ""HTCInvoiceNo"" text NULL,
                    ""DocumentsGiven"" text NOT NULL,
                    ""BankApprStatus"" integer NOT NULL,
                    ""BankApprDTime"" timestamp with time zone NULL,
                    ""BankApprBy"" text NULL,
                    ""BankApprNote"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""HandoverDate"" timestamp with time zone NULL,
                    ""ReturnDueDate"" timestamp with time zone NULL,
                    ""ActualReturnDate"" timestamp with time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_DocReqId"" ON ""CarDocRequestDetails"" (""DocReqId"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_OrgId_VIN"" ON ""CarDocRequestDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_CarDocRequestDetails_OrgId_DRListCode"" ON ""CarDocRequestDetails"" (""OrgId"", ""DRListCode"");

                CREATE TABLE IF NOT EXISTS ""HTCInvoices"" (
                    ""Id"" bigserial PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""HTCInvoiceCode"" text NOT NULL,
                    ""HTCInvoiceNo"" text NULL,
                    ""InvoiceSymbol"" text NOT NULL,
                    ""InvoiceDate"" timestamp with time zone NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NOT NULL,
                    ""BuyerTaxCode"" text NOT NULL,
                    ""BuyerAddress"" text NOT NULL,
                    ""BuyerLegalRepresentative"" text NULL,
                    ""PaymentMethod"" text NOT NULL,
                    ""BankCode"" text NULL,
                    ""BankName"" text NULL,
                    ""BankAccountNo"" text NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""VATAmount"" bigint NOT NULL,
                    ""TotalPayment"" bigint NOT NULL,
                    ""PaidAmount"" bigint NOT NULL,
                    ""RemainAmount"" bigint NOT NULL,
                    ""OS_HDDT_InvoiceCode"" text NULL,
                    ""SourceInvoiceCode"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Root_HTCInvoiceNo"" text NULL,
                    ""Adj_DeleteReason"" text NULL,
                    ""ApprovedBy"" text NULL,
                    ""ApprovedDate"" timestamp with time zone NULL,
                    ""IssuedBy"" text NULL,
                    ""IssuedDate"" timestamp with time zone NULL,
                    ""CreatedBy"" text NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" text NULL,
                    ""UpdatedAt"" timestamp with time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_HTCInvoiceCode"" ON ""HTCInvoices"" (""OrgId"", ""HTCInvoiceCode"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_HTCInvoiceNo"" ON ""HTCInvoices"" (""OrgId"", ""HTCInvoiceNo"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoices_OrgId_DealerCode"" ON ""HTCInvoices"" (""OrgId"", ""DealerCode"");

                CREATE TABLE IF NOT EXISTS ""HTCInvoiceDetails"" (
                    ""Id"" bigserial PRIMARY KEY,
                    ""InvoiceId"" bigint NOT NULL REFERENCES ""HTCInvoices""(""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""HTCInvoiceCode"" text NOT NULL,
                    ""ItemNo"" integer NOT NULL,
                    ""CarId"" text NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NOT NULL,
                    ""SpecCode"" text NULL,
                    ""EngineNo"" text NULL,
                    ""ColorVN"" text NULL,
                    ""ProductionYear"" text NOT NULL,
                    ""CabinCONo"" text NULL,
                    ""CQNo"" text NULL,
                    ""CustomsDeclarationNo"" text NULL,
                    ""SOCode"" text NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""VATRate"" numeric NOT NULL,
                    ""VATAmount"" bigint NOT NULL,
                    ""TotalPrice"" bigint NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_InvoiceId"" ON ""HTCInvoiceDetails"" (""InvoiceId"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_OrgId_VIN"" ON ""HTCInvoiceDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_HTCInvoiceDetails_OrgId_HTCInvoiceCode"" ON ""HTCInvoiceDetails"" (""OrgId"", ""HTCInvoiceCode"");

                CREATE TABLE IF NOT EXISTS ""LettersOfCredit"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""LCNo"" text NOT NULL,
                    ""ContractNo"" text NOT NULL,
                    ""BankName"" text NOT NULL,
                    ""BankCode"" text NULL,
                    ""BeneficiaryName"" text NULL,
                    ""BeneficiaryCountry"" text NULL,
                    ""ApplicantName"" text NULL,
                    ""LCType"" integer NOT NULL,
                    ""Currency"" text NOT NULL,
                    ""LCAmount"" numeric NOT NULL,
                    ""ExchangeRate"" numeric NOT NULL,
                    ""LCAmountVND"" bigint NOT NULL,
                    ""UtilizedAmount"" numeric NOT NULL,
                    ""RemainingAmount"" numeric NOT NULL,
                    ""DateOpen"" timestamp NOT NULL,
                    ""DateExpired"" timestamp NULL,
                    ""LatestShipmentDate"" timestamp NULL,
                    ""TermDays"" integer NOT NULL,
                    ""PackingListNo"" text NULL,
                    ""PaymentTerm"" text NULL,
                    ""PortOfLoading"" text NULL,
                    ""PortOfDischarge"" text NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""OpenedBy"" text NULL,
                    ""OpenedAt"" timestamp NULL,
                    ""AmendedBy"" text NULL,
                    ""AmendedAt"" timestamp NULL,
                    ""SettledBy"" text NULL,
                    ""SettledAt"" timestamp NULL,
                    ""RejectReason"" text NULL,
                    ""CreatedBy"" text NOT NULL,
                    ""CreatedAt"" timestamp NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_LCNo"" ON ""LettersOfCredit"" (""OrgId"", ""LCNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_ContractNo"" ON ""LettersOfCredit"" (""OrgId"", ""ContractNo"");
                CREATE INDEX IF NOT EXISTS ""IX_LettersOfCredit_OrgId_BankCode"" ON ""LettersOfCredit"" (""OrgId"", ""BankCode"");

                CREATE TABLE IF NOT EXISTS ""LetterOfCreditDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""LCId"" bigint NOT NULL REFERENCES ""LettersOfCredit""(""Id"") ON DELETE CASCADE,
                    ""OrgId"" uuid NOT NULL,
                    ""LCNo"" text NOT NULL,
                    ""ItemNo"" integer NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NOT NULL,
                    ""SpecCode"" text NULL,
                    ""EngineNo"" text NULL,
                    ""ColorCode"" text NULL,
                    ""WorkOrderNo"" text NULL,
                    ""PortCode"" text NULL,
                    ""PlantCode"" text NULL,
                    ""UnitPrice"" numeric NOT NULL,
                    ""Amount"" numeric NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL
                );
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_LCId"" ON ""LetterOfCreditDetails"" (""LCId"");
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_OrgId_VIN"" ON ""LetterOfCreditDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_LetterOfCreditDetails_OrgId_LCNo"" ON ""LetterOfCreditDetails"" (""OrgId"", ""LCNo"");
                CREATE TABLE IF NOT EXISTS ""BankDealers"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NULL,
                    ""BankCode"" text NOT NULL,
                    ""BankName"" text NULL,
                    ""CreditContractNo"" text NULL,
                    ""CreditContractDate"" timestamp with time zone NULL,
                    ""CreditAmount"" numeric NULL,
                    ""BankBranchCode"" text NULL,
                    ""BankBranchName"" text NULL,
                    ""FlagBankGrt"" boolean NOT NULL,
                    ""FlagBankPmt"" boolean NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" text NULL,
                    ""UpdatedAt"" timestamp with time zone NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_DealerCode_BankCode"" ON ""BankDealers"" (""OrgId"", ""DealerCode"", ""BankCode"");
                CREATE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_DealerCode"" ON ""BankDealers"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_BankDealers_OrgId_BankCode"" ON ""BankDealers"" (""OrgId"", ""BankCode"");
                CREATE TABLE IF NOT EXISTS ""AccountingVoucherUpdates"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""BatchNo"" text NOT NULL,
                    ""Description"" text NULL,
                    ""TotalItems"" integer NOT NULL,
                    ""UpdatedItems"" integer NOT NULL,
                    ""SkippedItems"" integer NOT NULL,
                    ""Status"" integer NOT NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""AppliedBy"" text NULL,
                    ""AppliedAt"" timestamp with time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdates_OrgId_BatchNo"" ON ""AccountingVoucherUpdates"" (""OrgId"", ""BatchNo"");
                CREATE TABLE IF NOT EXISTS ""AccountingVoucherUpdateDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""AccountingVoucherUpdateId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""PaymentNo"" text NOT NULL,
                    ""OldAccountingRecordNo"" text NULL,
                    ""NewAccountingRecordNo"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""Note"" text NULL,
                    CONSTRAINT ""FK_AccountingVoucherUpdateDetails_AccountingVoucherUpdates_AccountingVoucherUpdateId"" FOREIGN KEY (""AccountingVoucherUpdateId"") REFERENCES ""AccountingVoucherUpdates"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdateDetails_AccountingVoucherUpdateId"" ON ""AccountingVoucherUpdateDetails"" (""AccountingVoucherUpdateId"");
                CREATE INDEX IF NOT EXISTS ""IX_AccountingVoucherUpdateDetails_OrgId_PaymentNo"" ON ""AccountingVoucherUpdateDetails"" (""OrgId"", ""PaymentNo"");
                CREATE TABLE IF NOT EXISTS ""CalendarEntries"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""CalendarType"" text NOT NULL,
                    ""Date"" timestamp without time zone NOT NULL,
                    ""StatusValue"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" text NULL,
                    ""UpdatedAt"" timestamp with time zone NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CalendarEntries_OrgId_CalendarType_Date"" ON ""CalendarEntries"" (""OrgId"", ""CalendarType"", ""Date"");
                CREATE INDEX IF NOT EXISTS ""IX_CalendarEntries_OrgId_CalendarType"" ON ""CalendarEntries"" (""OrgId"", ""CalendarType"");
                CREATE TABLE IF NOT EXISTS ""DealerContracts"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""DlrCtrNo"" text NOT NULL,
                    ""DCPType"" integer NOT NULL,
                    ""DealerCode"" text NOT NULL,
                    ""DealerName"" text NULL,
                    ""BankCodeMD"" text NULL,
                    ""BankNameMD"" text NULL,
                    ""ContractDate"" timestamp without time zone NOT NULL,
                    ""TotalAmount"" bigint NOT NULL,
                    ""TotalVehicles"" integer NOT NULL,
                    ""FilePath"" text NULL,
                    ""FlagDlrCtrAdjust"" boolean NOT NULL,
                    ""DlrSignStatus"" integer NOT NULL,
                    ""HTCSignStatus"" integer NOT NULL,
                    ""DlrCtrStatus"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedBy"" text NULL,
                    ""UpdatedAt"" timestamp with time zone NULL,
                    ""DlrApprovedBy"" text NULL,
                    ""DlrApprovedAt"" timestamp with time zone NULL,
                    ""HTCApproved1By"" text NULL,
                    ""HTCApproved1At"" timestamp with time zone NULL,
                    ""HTCApproved2By"" text NULL,
                    ""HTCApproved2At"" timestamp with time zone NULL,
                    ""RejectedBy"" text NULL,
                    ""RejectedAt"" timestamp with time zone NULL,
                    ""CancelledBy"" text NULL,
                    ""CancelledAt"" timestamp with time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""DealerContractDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""DealerContractId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""CarId"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""ModelCode"" text NULL,
                    ""ModelName"" text NULL,
                    ""SpecCode"" text NULL,
                    ""ColorCode"" text NULL,
                    ""ColorName"" text NULL,
                    ""OriginNo"" text NULL,
                    ""ProductionYear"" integer NOT NULL,
                    ""UnitPrice"" bigint NOT NULL,
                    ""DlrCtrStatusDtl"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_DealerContractDetails_DealerContracts_DealerContractId"" FOREIGN KEY (""DealerContractId"") REFERENCES ""DealerContracts"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_DlrCtrNo"" ON ""DealerContracts"" (""OrgId"", ""DlrCtrNo"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_DealerCode"" ON ""DealerContracts"" (""OrgId"", ""DealerCode"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContracts_OrgId_BankCodeMD"" ON ""DealerContracts"" (""OrgId"", ""BankCodeMD"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_DealerContractId"" ON ""DealerContractDetails"" (""DealerContractId"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_OrgId_VIN"" ON ""DealerContractDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_DealerContractDetails_OrgId_DlrCtrNo"" ON ""DealerContractDetails"" (""OrgId"", ""DlrCtrNo"");
                CREATE TABLE IF NOT EXISTS ""StorageRearranges"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""StorageRearrangeNo"" text NOT NULL,
                    ""RearrangeStatus"" integer NOT NULL,
                    ""Remark"" text NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""ApprovedBy1"" text NULL,
                    ""ApprovedAt1"" timestamp without time zone NULL,
                    ""ApprovedBy2"" text NULL,
                    ""ApprovedAt2"" timestamp without time zone NULL
                );
                CREATE TABLE IF NOT EXISTS ""StorageRearrangeDetails"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""StorageRearrangeId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""StorageRearrangeNo"" text NOT NULL,
                    ""VIN"" text NOT NULL,
                    ""StorageCodeFrom"" text NOT NULL,
                    ""StorageCodeTo"" text NOT NULL,
                    ""ExpectedStartDate"" timestamp without time zone NOT NULL,
                    ""ExpectedEndDate"" timestamp without time zone NULL,
                    ""Remark"" text NULL,
                    ""RearrangeDtlStatus"" integer NOT NULL,
                    ""ConfirmBy"" text NULL,
                    ""ConfirmDate"" timestamp without time zone NULL,
                    CONSTRAINT ""FK_StorageRearrangeDetails_StorageRearranges_StorageRearrangeId"" FOREIGN KEY (""StorageRearrangeId"") REFERENCES ""StorageRearranges"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StorageRearranges_OrgId_StorageRearrangeNo"" ON ""StorageRearranges"" (""OrgId"", ""StorageRearrangeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_StorageRearrangeId"" ON ""StorageRearrangeDetails"" (""StorageRearrangeId"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_OrgId_VIN"" ON ""StorageRearrangeDetails"" (""OrgId"", ""VIN"");
                CREATE INDEX IF NOT EXISTS ""IX_StorageRearrangeDetails_OrgId_StorageRearrangeNo"" ON ""StorageRearrangeDetails"" (""OrgId"", ""StorageRearrangeNo"");

                CREATE TABLE IF NOT EXISTS ""TransportFeeVersions"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""TFVCode"" text NOT NULL,
                    ""Description"" text NULL,
                    ""Status"" integer NOT NULL,
                    ""CreatedDate"" timestamp without time zone NOT NULL,
                    ""AppliedDate"" timestamp without time zone NULL,
                    ""CreatedBy"" text NULL,
                    ""CreatedAt"" timestamp without time zone NOT NULL,
                    ""UpdatedBy"" text NULL,
                    ""UpdatedAt"" timestamp without time zone NULL,
                    ""Remark"" text NULL
                );
                CREATE TABLE IF NOT EXISTS ""TransportFeeRates"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""VersionId"" bigint NOT NULL,
                    ""OrgId"" uuid NOT NULL,
                    ""TFVCode"" text NOT NULL,
                    ""ProvinceCodeFrom"" text NOT NULL,
                    ""ProvinceNameFrom"" text NULL,
                    ""DistrictCodeFrom"" text NOT NULL,
                    ""DistrictNameFrom"" text NULL,
                    ""ProvinceCodeTo"" text NOT NULL,
                    ""ProvinceNameTo"" text NULL,
                    ""DistrictCodeTo"" text NOT NULL,
                    ""DistrictNameTo"" text NULL,
                    ""TransporterCode"" text NOT NULL,
                    ""TransporterName"" text NULL,
                    ""ModelCode"" text NOT NULL,
                    ""ModelName"" text NULL,
                    ""ValFee"" bigint NOT NULL,
                    ""ExpectedDays"" integer NOT NULL,
                    ""Remark"" text NULL,
                    CONSTRAINT ""FK_TransportFeeRates_TransportFeeVersions_VersionId"" FOREIGN KEY (""VersionId"") REFERENCES ""TransportFeeVersions"" (""Id"") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TransportFeeVersions_OrgId_TFVCode"" ON ""TransportFeeVersions"" (""OrgId"", ""TFVCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_VersionId"" ON ""TransportFeeRates"" (""VersionId"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_TFVCode"" ON ""TransportFeeRates"" (""OrgId"", ""TFVCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_TransporterCode"" ON ""TransportFeeRates"" (""OrgId"", ""TransporterCode"");
                CREATE INDEX IF NOT EXISTS ""IX_TransportFeeRates_OrgId_Route"" ON ""TransportFeeRates"" (""OrgId"", ""ProvinceCodeFrom"", ""DistrictCodeFrom"", ""ProvinceCodeTo"", ""DistrictCodeTo"");

                CREATE TABLE IF NOT EXISTS ""GuaranteeAttachFiles"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""GuaranteeId"" bigint NOT NULL,
                    ""GuaranteeNo"" text NOT NULL,
                    ""FileIndex"" integer NOT NULL,
                    ""GrtFilePath"" text NOT NULL,
                    ""GrtFileName"" text NOT NULL,
                    ""FileSizeInBytes"" bigint NOT NULL,
                    ""GrtFileRemark"" text NULL,
                    ""LogLUBy"" text NULL,
                    ""LogLUDateTime"" timestamp without time zone NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ""GuaranteeAttachFileHis"" (
                    ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""OrgId"" uuid NOT NULL,
                    ""GuaranteeNo"" text NOT NULL,
                    ""FileIndex"" integer NOT NULL,
                    ""GrtFilePath"" text NOT NULL,
                    ""GrtFileName"" text NOT NULL,
                    ""FileSizeInBytes"" bigint NOT NULL,
                    ""GrtFileRemark"" text NULL,
                    ""LogLUBy"" text NULL,
                    ""LogLUDateTime"" timestamp without time zone NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_OrgId_GuaranteeNo_FileIndex"" ON ""GuaranteeAttachFiles"" (""OrgId"", ""GuaranteeNo"", ""FileIndex"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_GuaranteeId"" ON ""GuaranteeAttachFiles"" (""GuaranteeId"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFiles_OrgId_GuaranteeNo"" ON ""GuaranteeAttachFiles"" (""OrgId"", ""GuaranteeNo"");
                CREATE INDEX IF NOT EXISTS ""IX_GuaranteeAttachFileHis_OrgId_GuaranteeNo"" ON ""GuaranteeAttachFileHis"" (""OrgId"", ""GuaranteeNo"");
            ");
        }

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Merchant", ApiKey = "demo-pay" });
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u giao dá»‹ch thanh toÃ¡n Ä‘á»ƒ Ä‘á»‘i soÃ¡t
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

            // Seed 1 Ä‘á»£t Ä‘á»‘i soÃ¡t sao kÃª ngÃ¢n hÃ ng máº«u
            if (!await db.ReconcileBatches.AnyAsync(b => b.OrgId == TenantContext.DefaultOrgId))
            {
                var batch = new ReconcileBatch
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchCode = "REC-DEMO-20250514-01",
                    BankCode = "VCB",
                    AccountNo = "0011004123456",
                    StatementDate = DateTime.Today,
                    Note = "Sao kÃª tÃ i khoáº£n Vietcombank Ä‘á»£t 1",
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
                        DiscrepancyReason = "Khá»›p chuáº©n mÃ£ giao dá»‹ch vÃ  sá»‘ tiá»n.",
                        MatchedAt = DateTime.Now.AddHours(-1)
                    },
                    new ReconcileDetail
                    {
                        BatchId = batch.Id,
                        OrgId = TenantContext.DefaultOrgId,
                        BankTxnNo = "VCB-TXN-88124",
                        TxnRef = "TXN-REC-202502",
                        TxnTime = DateTime.Now.AddDays(-1),
                        Amount = 3_000_000, // Lá»‡ch: sao kÃª ghi 3.0M, há»‡ thá»‘ng cáº§n 3.2M
                        SenderAccount = "045100033221",
                        ReceiverAccount = "0011004123456",
                        Remark = "CT HD ORD-REC-102 TXN-REC-202502 NOP THIEU",
                        MatchStatus = MatchStatus.AmountMismatch,
                        PaymentIntentId = p2.Id,
                        SystemAmount = 3_200_000,
                        DiscrepancyReason = "Lá»‡ch sá»‘ tiá»n: Sao kÃª 3,000,000 Ä‘, Há»‡ thá»‘ng 3,200,000 Ä‘ (chÃªnh lá»‡ch: -200,000 Ä‘)."
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
                        DiscrepancyReason = "KhÃ´ng tÃ¬m tháº¥y giao dá»‹ch tÆ°Æ¡ng á»©ng trong há»‡ thá»‘ng."
                    }
                );
                await db.SaveChangesAsync();
            }
        }

        // Seed dá»¯ liá»‡u máº«u cho Lá»‡nh chi chuyá»ƒn tiá»n ngÃ¢n hÃ ng tá»± Ä‘á»™ng (Banking Payout / RQ_BankingTransactions)
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
                Remark = "Lá»‡nh chi tiá»n lÃ´ thanh toÃ¡n NCC vÃ  hoÃ n tiá»n khÃ¡ch hÃ ng Ä‘á»£t 1",
                TotalTrans = 3,
                SuccessTrans = 3,
                FailedTrans = 0,
                TotalAmount = 45_800_000,
                SuccessAmount = 45_800_000,
                Status = PayoutStatus.Completed,
                BankStatusCode = "00",
                RefBankCode = "VTB-BULK-20250514-991",
                BankRemark = "Giao dá»‹ch chuyá»ƒn tiá»n ngÃ¢n hÃ ng thÃ nh cÃ´ng trá»n gÃ³i.",
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
                    ProvinceName = "HÃ  Ná»™i",
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
                    ProvinceName = "TP Há»“ ChÃ­ Minh",
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
                    ProvinceName = "HÃ  Ná»™i",
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
                Remark = "Lá»‡nh chi tráº£ tiá»n thuÃª máº·t báº±ng vÃ  dá»‹ch vá»¥ háº¡ táº§ng Ä‘á»£t 2",
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
                    ProvinceName = "HÃ  Ná»™i",
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
                    ProvinceName = "ÄÃ  Náºµng",
                    TransferAmount = 6_500_000,
                    TransferRemark = "Chi tra cuoc ha tang cloud Fast thang 5",
                    Status = PayoutItemStatus.Pending
                }
            );
            await db.SaveChangesAsync();
        }

        // Seed dá»¯ liá»‡u máº«u Chiáº¿t kháº¥u thanh toÃ¡n sá»›m (Payment Discount / Req_PaymentDiscount)
        if (!await db.DiscountRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            var dReq1 = new PaymentDiscountRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                DiscountNo = "DIS-DEMO-20250514-01",
                PartnerCode = "DLR-HYUNDAI-HADONG",
                PartnerName = "CÃ”NG TY CP LIÃŠN DOANH HYUNDAI HÃ€ ÄÃ”NG",
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
                Remark = "Há»“ sÆ¡ chiáº¿t kháº¥u thanh toÃ¡n sá»›m lÃ´ xe há»£p Ä‘á»“ng CTR-HTCV-2025-088",
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
                    Description = "Thanh toÃ¡n Ä‘á»£t 1 lÃ´ 02 xe Hyundai Santa Fe 2025",
                    DueDate = DateTime.Today.AddDays(25),
                    ActualPaymentDate = DateTime.Today.AddDays(-2),
                    EarlyDays = 27,
                    OriginalAmount = 1_500_000_000,
                    AnnualDiscountRate = 7.5m,
                    DiscountAmount = 8_437_500, // 1.5B * 7.5% * 27 / 360 = 8,437,500
                    NetPayAmount = 1_491_562_500,
                    Status = DiscountItemStatus.Active,
                    Note = "ÄÃ£ Ä‘á»‘i chiáº¿u khá»›p lá»‡nh á»§y nhiá»‡m chi VCB"
                },
                new PaymentDiscountDetail
                {
                    RequestId = dReq1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-04-02",
                    Description = "Thanh toÃ¡n Ä‘á»£t 2 lÃ´ 02 xe Hyundai Tucson 2025",
                    DueDate = DateTime.Today.AddDays(30),
                    ActualPaymentDate = DateTime.Today.AddDays(-2),
                    EarlyDays = 32,
                    OriginalAmount = 1_800_000_000,
                    AnnualDiscountRate = 7.5m,
                    DiscountAmount = 12_000_000, // 1.8B * 7.5% * 32 / 360 = 12,000,000
                    NetPayAmount = 1_788_000_000,
                    Status = DiscountItemStatus.Active,
                    Note = "ÄÃ£ Ä‘á»‘i chiáº¿u khá»›p lá»‡nh á»§y nhiá»‡m chi VCB"
                }
            );
            await db.SaveChangesAsync();

            var dReq2 = new PaymentDiscountRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                DiscountNo = "DIS-DEMO-20250514-02",
                PartnerCode = "DLR-HYUNDAI-DONGDO",
                PartnerName = "Äáº I LÃ Ã” TÃ” HYUNDAI ÄÃ”NG ÄÃ”",
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
                Remark = "Äá» nghá»‹ duyá»‡t chiáº¿t kháº¥u tráº£ sá»›m Ä‘Æ¡n hÃ ng xe Stargazer & phá»¥ tÃ¹ng",
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
                    Description = "Thanh toÃ¡n lÃ´ phá»¥ tÃ¹ng & linh kiá»‡n chÃ­nh hÃ£ng",
                    DueDate = DateTime.Today.AddDays(18),
                    ActualPaymentDate = DateTime.Today,
                    EarlyDays = 18,
                    OriginalAmount = 650_000_000,
                    AnnualDiscountRate = 8.0m,
                    DiscountAmount = 2_600_000, // 650M * 8% * 18 / 360 = 2,600,000
                    NetPayAmount = 647_400_000,
                    Status = DiscountItemStatus.Active,
                    Note = "Äá» nghá»‹ cáº¥n trá»« hÃ³a Ä‘Æ¡n VAT tiáº¿p theo"
                },
                new PaymentDiscountDetail
                {
                    RequestId = dReq2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "INV-2025-05-11",
                    Description = "Thanh toÃ¡n lÃ´ xe Hyundai Stargazer 2025 Ä‘á»£t 1",
                    DueDate = DateTime.Today.AddDays(24),
                    ActualPaymentDate = DateTime.Today,
                    EarlyDays = 24,
                    OriginalAmount = 1_200_000_000,
                    AnnualDiscountRate = 8.0m,
                    DiscountAmount = 6_400_000, // 1.2B * 8% * 24 / 360 = 6,400,000
                    NetPayAmount = 1_193_600_000,
                    Status = DiscountItemStatus.Active,
                    Note = "ÄÃ­nh kÃ¨m báº£n scan UNC ngÃ¢n hÃ ng MBBank"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u ThÆ° báº£o lÃ£nh thanh toÃ¡n ngÃ¢n hÃ ng (Pmt_Guarantee / BizHTC.Payment)
        if (!await db.Guarantees.AnyAsync(g => g.OrgId == TenantContext.DefaultOrgId))
        {
            var grt1 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250501-001",
                BankGuaranteeNo = "BL-VCB-2025/088",
                BankCode = "VCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ngoáº¡i thÆ°Æ¡ng Viá»‡t Nam (Vietcombank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Thanh XuÃ¢n",
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
                Remark = "Báº£o lÃ£nh thanh toÃ¡n lÃ´ 03 xe thÆ°Æ¡ng máº¡i & du lá»‹ch thÃ¡ng 5/2025",
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
                    DateWarning = DateTime.Today.AddDays(55),
                    DateExpired = DateTime.Today.AddDays(70),
                    FlagDtlDiscount = "0",
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Báº£o lÃ£nh háº¡n má»©c 92%"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-ACCENT-02",
                    Description = "Xe Hyundai Accent 1.5 AT Äáº·c biá»‡t",
                    OrderAmount = 569_000_000,
                    GuaranteeValue = 550_000_000,
                    GuaranteePercent = 96.66m,
                    DateStart = DateTime.Today.AddDays(-20),
                    DateEnd = DateTime.Today.AddDays(70),
                    DateWarning = DateTime.Today.AddDays(55),
                    DateExpired = DateTime.Today.AddDays(70),
                    FlagDtlDiscount = "0",
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Báº£o lÃ£nh háº¡n má»©c 96.7%"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-TUCSON-03",
                    Description = "Xe Hyundai Tucson 2.0 Dáº§u Äáº·c biá»‡t",
                    OrderAmount = 1_250_000_000,
                    GuaranteeValue = 1_200_000_000,
                    GuaranteePercent = 96.00m,
                    DateStart = DateTime.Today.AddDays(-20),
                    DateEnd = DateTime.Today.AddDays(70),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Báº£o lÃ£nh háº¡n má»©c 96%"
                }
            );
            await db.SaveChangesAsync();

            var grt2 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250510-002",
                BankGuaranteeNo = "BL-TCB-2025/112",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ thÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Pháº¡m VÄƒn Äá»“ng",
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
                Remark = "Há»“ sÆ¡ báº£o lÃ£nh thanh toÃ¡n tráº£ cháº­m LC 60 ngÃ y Ä‘á»£t 2",
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
                    Note = "Äang tháº©m Ä‘á»‹nh há»“ sÆ¡ gá»‘c"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CUSTIN-02",
                    Description = "Xe Hyundai Custin 1.5T Äáº·c biá»‡t",
                    OrderAmount = 850_000_000,
                    GuaranteeValue = 550_000_000,
                    GuaranteePercent = 64.71m,
                    DateStart = DateTime.Today.AddDays(-3),
                    DateEnd = DateTime.Today.AddDays(57),
                    Status = GuaranteeDetailStatus.Active,
                    Note = "Äang tháº©m Ä‘á»‹nh há»“ sÆ¡ gá»‘c"
                }
            );
            await db.SaveChangesAsync();

            var grt3 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250415-003",
                BankGuaranteeNo = "BL-MBB-2025/045",
                BankCode = "MBB",
                BankName = "NgÃ¢n hÃ ng TMCP QuÃ¢n Ä‘á»™i (MBBank)",
                PartnerCode = "DLR-HYUNDAI-HOANGVIET",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai HoÃ ng Viá»‡t",
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
                ClaimReason = "Äáº¡i lÃ½ quÃ¡ háº¡n thanh toÃ¡n 30 ngÃ y theo thá»a thuáº­n há»£p Ä‘á»“ng, kÃ­ch hoáº¡t Ä‘Ã²i ngÃ¢n hÃ ng MBBank thá»±c hiá»‡n nghÄ©a vá»¥ báº£o lÃ£nh thanh toÃ¡n.",
                ClaimedBy = "GiamDocQuanTriRuiRo",
                ClaimedAt = DateTime.Now.AddDays(-4),
                CreatedBy = "ChuyenVienKinhDoanh",
                ApprovedBy = "GiamDocTaiChinh",
                ApprovedAt = DateTime.Now.AddDays(-39),
                Remark = "ÄÃ£ gá»­i cÃ´ng vÄƒn Ä‘Ã²i báº£o lÃ£nh chÃ­nh thá»©c kÃ¨m há»“ sÆ¡ gá»‘c sang MBBank",
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
                    Description = "Xe Hyundai Creta 1.5L Cao cáº¥p",
                    OrderAmount = 699_000_000,
                    GuaranteeValue = 600_000_000,
                    GuaranteePercent = 85.84m,
                    DateStart = DateTime.Today.AddDays(-40),
                    DateEnd = DateTime.Today.AddDays(-5),
                    Status = GuaranteeDetailStatus.Claimed,
                    Note = "QuÃ¡ háº¡n thanh toÃ¡n, chuyá»ƒn sang Ä‘Ã²i báº£o lÃ£nh"
                },
                new PaymentGuaranteeDetail
                {
                    GuaranteeId = grt3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-STARGAZER-02",
                    Description = "Xe Hyundai Stargazer X Cao cáº¥p",
                    OrderAmount = 650_000_000,
                    GuaranteeValue = 600_000_000,
                    GuaranteePercent = 92.31m,
                    DateStart = DateTime.Today.AddDays(-40),
                    DateEnd = DateTime.Today.AddDays(-5),
                    Status = GuaranteeDetailStatus.Claimed,
                    Note = "QuÃ¡ háº¡n thanh toÃ¡n, chuyá»ƒn sang Ä‘Ã²i báº£o lÃ£nh"
                }
            );
            await db.SaveChangesAsync();

            var grt4 = new PaymentGuarantee
            {
                OrgId = TenantContext.DefaultOrgId,
                GuaranteeNo = "GRT-20250320-004",
                BankGuaranteeNo = "BL-CTG-2025/019",
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng thÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-GIAIPHONG",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Giáº£i PhÃ³ng",
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
                Remark = "Äáº¡i lÃ½ Ä‘Ã£ thanh toÃ¡n Ä‘á»§ 100% tiá»n mua hÃ ng qua á»§y nhiá»‡m chi VietinBank, HTC Ä‘Ã£ phÃ¡t hÃ nh biÃªn báº£n giáº£i tá»a báº£o lÃ£nh hoÃ n táº¥t.",
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
                    Description = "Xe Hyundai Palisade Prestige 6 chá»—",
                    OrderAmount = 1_559_000_000,
                    GuaranteeValue = 1_500_000_000,
                    GuaranteePercent = 96.22m,
                    DateStart = DateTime.Today.AddDays(-60),
                    DateEnd = DateTime.Today.AddDays(-10),
                    Status = GuaranteeDetailStatus.Released,
                    Note = "ÄÃ£ giáº£i tá»a toÃ n bá»™ nghÄ©a vá»¥ báº£o lÃ£nh"
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
                    Note = "ÄÃ£ giáº£i tá»a toÃ n bá»™ nghÄ©a vá»¥ báº£o lÃ£nh"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Há»“ sÆ¡ Tháº¿ cháº¥p & Giáº£i cháº¥p tÃ i sáº£n ngÃ¢n hÃ ng (RM_ReqMortgage & RD_ReqRedeem / BizHTC.GiaiChap)
        if (!await db.MortgageRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Há»“ sÆ¡ tháº¿ cháº¥p kho xe vay VietinBank (CTG) - ÄÃ£ giáº£i cháº¥p 1 xe, cÃ²n 2 xe Ä‘ang tháº¿ cháº¥p
            var mReq1 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250501-001",
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng thÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Thanh XuÃ¢n",
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
                Remark = "Tháº¿ cháº¥p báº£o Ä‘áº£m lÃ´ xe du lá»‹ch Hyundai phá»¥c vá»¥ giáº£i ngÃ¢n nguá»“n vá»‘n lÆ°u Ä‘á»™ng",
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
                Note = "ÄÃ£ hoÃ n táº¥t giáº£i cháº¥p, bÃ n giao há»“ sÆ¡ gá»‘c cho Ä‘áº¡i lÃ½ xuáº¥t xÆ°á»Ÿng"
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
                Note = "Äang lÆ°u kho phong tá»a tháº¿ cháº¥p táº¡i Chi nhÃ¡nh HoÃ n Kiáº¿m"
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
                Note = "Äang lÆ°u kho phong tá»a tháº¿ cháº¥p táº¡i Chi nhÃ¡nh HoÃ n Kiáº¿m"
            };
            db.MortgageDetails.AddRange(mDtl1_1, mDtl1_2, mDtl1_3);
            await db.SaveChangesAsync();

            // 2. Há»“ sÆ¡ tháº¿ cháº¥p MBBank (MBB) - 2 xe Ä‘ang tháº¿ cháº¥p
            var mReq2 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250510-002",
                BankCode = "MBB",
                BankName = "NgÃ¢n hÃ ng TMCP QuÃ¢n Ä‘á»™i (MBBank)",
                PartnerCode = "DLR-HYUNDAI-DONGDO",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai ÄÃ´ng ÄÃ´",
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
                Remark = "Tháº¿ cháº¥p kho xe báº£o Ä‘áº£m khoáº£n vay thanh toÃ¡n nhÃ  mÃ¡y HTMV",
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
                Note = "Giáº¥y chá»©ng nháº­n Ä‘Äƒng kiá»ƒm CQ gá»‘c gá»­i kho báº£o máº­t MBBank"
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
                Note = "Giáº¥y chá»©ng nháº­n Ä‘Äƒng kiá»ƒm CQ gá»‘c gá»­i kho báº£o máº­t MBBank"
            };
            db.MortgageDetails.AddRange(mDtl2_1, mDtl2_2);
            await db.SaveChangesAsync();

            // 3. Há»“ sÆ¡ tháº¿ cháº¥p Techcombank (TCB) - Má»›i láº­p, Ä‘ang chá» phÃª duyá»‡t
            var mReq3 = new MortgageRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                ReqRMNo = "RM-DEMO-20250514-003",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ thÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Pháº¡m VÄƒn Äá»“ng",
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
                Remark = "Há»“ sÆ¡ tháº¿ cháº¥p tÃ i sáº£n xe Custin & Palisade Ä‘ang chá» tháº©m Ä‘á»‹nh Ä‘á»‹nh giÃ¡ tÃ i sáº£n",
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
                    Note = "Chá» cÃ¡n bá»™ Ä‘á»‹nh giÃ¡ TCB phÃª duyá»‡t"
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
                    Note = "Chá» cÃ¡n bá»™ Ä‘á»‹nh giÃ¡ TCB phÃª duyá»‡t"
                }
            );
            await db.SaveChangesAsync();

            // 4. Há»“ sÆ¡ Ä‘á» nghá»‹ giáº£i cháº¥p Ä‘Ã£ hoÃ n táº¥t cho xe SantaFe (VietinBank CTG)
            if (!await db.RedeemRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
            {
                var rReq1 = new RedeemRequest
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqDMNo = "DM-DEMO-20250512-001",
                    BankCode = "CTG",
                    BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng thÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                    PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                    PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Thanh XuÃ¢n",
                    RedeemDate = DateTime.Today.AddDays(-2),
                    TotalItems = 1,
                    ApprovedItems = 1,
                    TotalSettlementAmount = 950_000_000,
                    PaymentProofNo = "UNC-CTG-20250512-88712",
                    Status = RedeemStatus.Completed,
                    CreatedBy = "KeToanThanhToan",
                    ApprovedBy = "TruongPhongGD_VietinBank",
                    ApprovedAt = DateTime.Now.AddDays(-2),
                    Remark = "Äáº¡i lÃ½ ná»™p Ä‘á»§ 950 triá»‡u Ä‘á»“ng tiá»n táº¥t toÃ¡n gá»‘c vay xe SantaFe qua tÃ i khoáº£n CTG",
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
                    Note = "ÄÃ£ thu ná»£ gá»‘c thÃ nh cÃ´ng, phÃ¡t hÃ nh giáº¥y xÃ³a tháº¿ cháº¥p xe"
                });
                await db.SaveChangesAsync();
            }
        }

        // Dá»¯ liá»‡u máº«u Phiáº¿u thanh toÃ¡n & á»¦y nhiá»‡m chi ngÃ¢n hÃ ng (Pmt_Payment / BizHTC.Payment)
        if (!await db.PaymentOrders.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Phiáº¿u thanh toÃ¡n UNC ngÃ¢n hÃ ng (VCB) Ä‘Ã£ hoÃ n táº¥t - Thanh toÃ¡n tiá»n xe Santa Fe & Tucson
            var pm1 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250501-001",
                PaymentType = PaymentOrderType.UNC,
                BankPaymentNo = "UNC-VCB-20250501-8899",
                PaymentEndDate = DateTime.Today.AddDays(15),
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Thanh XuÃ¢n",
                BankCodeSend = "VCB",
                BankNameSend = "Vietcombank - Chi nhÃ¡nh ThÄƒng Long",
                BankAccountSend = "0011009876543",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trá»¥ sá»Ÿ chÃ­nh",
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
                    AmountAccum = 450_000_000, // ÄÃ£ cá»c trÆ°á»›c 450M
                    PercentAccum = 32.87m,
                    Amount = 919_000_000,      // Thanh toÃ¡n ná»‘t 919M
                    PercentCurrent = 67.13m,
                    AmountTotal = 1_369_000_000,
                    PercentTotal = 100m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "ÄÃ£ táº¥t toÃ¡n 100% giÃ¡ trá»‹ xe, Ä‘á»§ Ä‘iá»u kiá»‡n xuáº¥t hÃ³a Ä‘Æ¡n VAT"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-TUCSON-TURBO-02",
                    Description = "Xe Hyundai Tucson 1.6T Turbo 2025",
                    ModelCode = "TUCSON-TURBO",
                    UnitPriceActual = 959_000_000,
                    AmountAccum = 500_000_000, // ÄÃ£ cá»c trÆ°á»›c 500M
                    PercentAccum = 52.14m,
                    Amount = 459_000_000,      // Thanh toÃ¡n tiáº¿p 459M
                    PercentCurrent = 47.86m,
                    AmountTotal = 959_000_000,
                    PercentTotal = 100m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "ÄÃ£ táº¥t toÃ¡n 100% giÃ¡ trá»‹ xe, bÃ n giao há»“ sÆ¡ gá»‘c"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-CRETA-PREM-03",
                    Description = "Xe Hyundai Creta 1.5L Cao cáº¥p 2025",
                    ModelCode = "CRETA-PREM",
                    UnitPriceActual = 699_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 422_000_000,      // Thanh toÃ¡n Ä‘á»£t 1
                    PercentCurrent = 60.37m,
                    AmountTotal = 422_000_000,
                    PercentTotal = 60.37m,
                    Status = PaymentOrderDetailStatus.Finished,
                    Note = "Thanh toÃ¡n Ä‘á»£t 1 (60%), Ä‘á»£t 2 thanh toÃ¡n khi nháº­n xe"
                }
            );
            await db.SaveChangesAsync();

            // 2. Phiáº¿u thanh toÃ¡n Báº£o lÃ£nh ngÃ¢n hÃ ng (MBB) - ÄÃ£ phÃª duyá»‡t, chá» ngÃ¢n hÃ ng háº¡ch toÃ¡n
            var pm2 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250510-002",
                PaymentType = PaymentOrderType.GuaranteePayment,
                BankPaymentNo = "BL-MBB-2025/045",
                PaymentEndDate = DateTime.Today.AddDays(25),
                PartnerCode = "DLR-HYUNDAI-HOANGVIET",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai HoÃ ng Viá»‡t",
                BankCodeSend = "MBB",
                BankNameSend = "NgÃ¢n hÃ ng TMCP QuÃ¢n Ä‘á»™i (MBBank)",
                BankAccountSend = "0880123456789",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trá»¥ sá»Ÿ chÃ­nh",
                BankAccountReceive = "118000234567",
                Funds = PaymentFundType.CreditLine,
                AccountingRecordNo = "PKT-202505-0210",
                TotalAmount = 1_200_000_000,
                TotalAccumAmount = 1_200_000_000,
                Status = PaymentOrderStatus.Approved,
                Remark = "Thanh toÃ¡n theo ThÆ° báº£o lÃ£nh thanh toÃ¡n MBBank sá»‘ BL-MBB-2025/045",
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
                    Description = "Xe Hyundai Creta 1.5L Cao cáº¥p",
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
                    Note = "Báº£o lÃ£nh thanh toÃ¡n phÃ¡t hÃ nh qua MBBank"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-STARGAZER-02",
                    Description = "Xe Hyundai Stargazer X Cao cáº¥p",
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
                    Note = "Báº£o lÃ£nh thanh toÃ¡n phÃ¡t hÃ nh qua MBBank"
                }
            );
            await db.SaveChangesAsync();

            // 3. Phiáº¿u thanh toÃ¡n Ná»™p tiá»n cá»c vay vá»‘n ngÃ¢n hÃ ng (TCB) - Äang chá» phÃª duyá»‡t
            var pm3 = new PaymentOrder
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-DEMO-20250514-003",
                PaymentType = PaymentOrderType.Deposit,
                BankPaymentNo = "UNC-TCB-20250514-0091",
                PaymentEndDate = DateTime.Today.AddDays(30),
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Pháº¡m VÄƒn Äá»“ng",
                BankCodeSend = "TCB",
                BankNameSend = "Techcombank - Chi nhÃ¡nh HoÃ ng Gia",
                BankAccountSend = "1903344556677",
                BankCodeReceive = "CTG",
                BankNameReceive = "VietinBank - Trá»¥ sá»Ÿ chÃ­nh",
                BankAccountReceive = "118000234567",
                Funds = PaymentFundType.BankLoan,
                BankLending = "TCB",
                InterestRate = 7.8m,
                LoanPeriodMonths = 6,
                TotalAmount = 400_000_000,
                TotalAccumAmount = 400_000_000,
                Status = PaymentOrderStatus.PendingApproval,
                Remark = "Ná»™p cá»c mua 02 xe Hyundai Custin & Palisade vay vá»‘n tÃ i trá»£ Techcombank",
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
                    Description = "Xe Hyundai Custin 1.5 Turbo TiÃªu chuáº©n",
                    ModelCode = "CUSTIN-1.5T",
                    UnitPriceActual = 850_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 170_000_000, // Cá»c 20%
                    PercentCurrent = 20.0m,
                    AmountTotal = 170_000_000,
                    PercentTotal = 20.0m,
                    Status = PaymentOrderDetailStatus.Pending,
                    Note = "Cá»c giá»¯ chá»— 20% Ä‘Æ¡n hÃ ng xe theo há»£p Ä‘á»“ng vay TCB"
                },
                new PaymentOrderDetail
                {
                    PaymentOrderId = pm3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ItemRefNo = "VIN-PALISADE-2025-04",
                    Description = "Xe Hyundai Palisade Prestige 6 chá»—",
                    ModelCode = "PALISADE-PRE",
                    UnitPriceActual = 1_365_000_000,
                    AmountAccum = 0,
                    PercentAccum = 0m,
                    Amount = 230_000_000, // Cá»c ~16.85%
                    PercentCurrent = 16.85m,
                    AmountTotal = 230_000_000,
                    PercentTotal = 16.85m,
                    Status = PaymentOrderDetailStatus.Pending,
                    Note = "Cá»c giá»¯ chá»— Ä‘Æ¡n hÃ ng xe theo há»£p Ä‘á»“ng vay TCB"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u BiÃªn báº£n bÃ n giao xe & chá»©ng tá»« theo Há»‘i phiáº¿u ngÃ¢n hÃ ng (Car_BankBillMinutes / BizHTC.Payment)
        if (!await db.BankBillMinutes.AnyAsync(b => b.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. BiÃªn báº£n bÃ n giao VPBank - NgÃ¢n hÃ ng Ä‘Ã£ tiáº¿p nháº­n Ä‘á»§ chá»©ng tá»« gá»‘c xe (BankReceived)
            var bb1 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250505-001",
                BankCode = "VPB",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                PartnerCode = "DLR-HYUNDAI-HADONG",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai HÃ  ÄÃ´ng",
                BankBillDate = DateTime.Today.AddDays(-10),
                BankBillPrintDate = DateTime.Today.AddDays(-10),
                BankBillReciveDate = DateTime.Today.AddDays(-6),
                TotalVehicles = 3,
                TotalClaimAmount = 2_750_000_000,
                Status = BankBillMinutesStatus.BankReceived,
                Remark = "BÃ n giao há»“ sÆ¡ gá»‘c xe kÃ¨m Há»‘i phiáº¿u tÃ i trá»£ vá»‘n Ä‘á»£t 1 thÃ¡ng 05/2025 qua VPBank ThÄƒng Long",
                CreatedBy = "ChuyenVienHopDong",
                CreatedAt = DateTime.Now.AddDays(-10),
                HandedOverBy = "NguyenVanGiao_Logistics",
                HandedOverAt = DateTime.Now.AddDays(-9),
                BankReceivedBy = "PhamThuTrang_GiaoDichVienVPB",
                BankReceivedAt = DateTime.Now.AddDays(-6)
            };
            db.BankBillMinutes.Add(bb1);
            await db.SaveChangesAsync();

            db.BankBillMinutesDetails.AddRange(
                new BankBillMinutesDetail
                {
                    MinutesId = bb1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100101",
                    ModelCode = "SANTAFE-CAL",
                    SpecCode = "SF-2.5T-AWD",
                    SpecDescription = "Hyundai Santa Fe Calligraphy 2.5 Turbo 6 chá»—",
                    EngineNo = "G4KP-088124",
                    CONo = "CO-TCG-2025-09112",
                    CabinCONo = "CQ-2025-08112",
                    DeclarationNo = "TK-HQ-2025-099120",
                    BankGuaranteeNo = "BL-VPB-2025/088",
                    HTCInvoiceNo = "HD-HTC-2025-0331",
                    TCGInvoiceNo = "HD-TCG-2025-0899",
                    TransportMinutesNo = "BBVC-2025-0551",
                    ClaimAmount = 1_350_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-10),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-10),
                    NumberOfDaysDeferred = 45,
                    Status = BankBillDetailStatus.BankVerified,
                    Note = "Báº£n gá»‘c CO xuáº¥t xÆ°á»Ÿng, Giáº¥y chá»©ng nháº­n cháº¥t lÆ°á»£ng CQ, Tá» khai HQ"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100102",
                    ModelCode = "TUCSON-TURBO",
                    SpecCode = "TUC-1.6T-HTRAC",
                    SpecDescription = "Hyundai Tucson 1.6 T-GDi Äáº·c biá»‡t HTRAC",
                    EngineNo = "G4FP-190334",
                    CONo = "CO-TCG-2025-09113",
                    CabinCONo = "CQ-2025-08113",
                    DeclarationNo = "TK-HQ-2025-099121",
                    BankGuaranteeNo = "BL-VPB-2025/088",
                    HTCInvoiceNo = "HD-HTC-2025-0332",
                    TCGInvoiceNo = "HD-TCG-2025-0900",
                    TransportMinutesNo = "BBVC-2025-0551",
                    ClaimAmount = 950_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-10),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-10),
                    NumberOfDaysDeferred = 45,
                    Status = BankBillDetailStatus.BankVerified,
                    Note = "Báº£n gá»‘c CO xuáº¥t xÆ°á»Ÿng, HÃ³a Ä‘Æ¡n VAT HTC báº£n gá»‘c"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100103",
                    ModelCode = "ACCENT-AT",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "Hyundai Accent 1.5 AT Báº£n Äáº·c biá»‡t",
                    EngineNo = "G4FL-445109",
                    CONo = "CO-TCG-2025-09114",
                    CabinCONo = "CQ-2025-08114",
                    DeclarationNo = "TK-HQ-2025-099122",
                    BankGuaranteeNo = "BL-VPB-2025/088",
                    HTCInvoiceNo = "HD-HTC-2025-0333",
                    TCGInvoiceNo = "HD-TCG-2025-0901",
                    TransportMinutesNo = "BBVC-2025-0552",
                    ClaimAmount = 450_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-10),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-10),
                    NumberOfDaysDeferred = 30,
                    Status = BankBillDetailStatus.BankVerified,
                    Note = "Äá»§ há»“ sÆ¡ gá»‘c há»£p lá»‡, ngÃ¢n hÃ ng Ä‘Ã£ kiá»ƒm tra vÃ  lÆ°u kho há»“ sÆ¡"
                }
            );
            await db.SaveChangesAsync();

            // 2. BiÃªn báº£n bÃ n giao VietinBank (CTG) - ÄÃ£ xuáº¥t trÃ¬nh bÃ n giao chá»©ng tá»« sang NgÃ¢n hÃ ng (Delivered)
            var bb2 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250508-002",
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng thÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Thanh XuÃ¢n",
                BankBillDate = DateTime.Today.AddDays(-4),
                BankBillPrintDate = DateTime.Today.AddDays(-4),
                TotalVehicles = 2,
                TotalClaimAmount = 1_200_000_000,
                Status = BankBillMinutesStatus.Delivered,
                Remark = "ÄÃ£ xuáº¥t trÃ¬nh bá»™ há»“ sÆ¡ xe vÃ  Há»‘i phiáº¿u sang VietinBank TrÃ ng An, Ä‘ang Ä‘á»£i chuyÃªn viÃªn kÃ½ nháº­n",
                CreatedBy = "ChuyenVienHopDong",
                CreatedAt = DateTime.Now.AddDays(-4),
                HandedOverBy = "TranMinhQuan_GiaoNhan",
                HandedOverAt = DateTime.Now.AddDays(-3)
            };
            db.BankBillMinutes.Add(bb2);
            await db.SaveChangesAsync();

            db.BankBillMinutesDetails.AddRange(
                new BankBillMinutesDetail
                {
                    MinutesId = bb2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100201",
                    ModelCode = "CRETA-PREM",
                    SpecCode = "CRT-1.5-PREM",
                    SpecDescription = "Hyundai Creta 1.5L XÄƒng Cao cáº¥p",
                    EngineNo = "G4FL-889012",
                    CONo = "CO-TCG-2025-10201",
                    CabinCONo = "CQ-2025-10201",
                    DeclarationNo = "TK-HQ-2025-104401",
                    BankGuaranteeNo = "BL-CTG-2025/110",
                    HTCInvoiceNo = "HD-HTC-2025-0450",
                    TCGInvoiceNo = "HD-TCG-2025-1102",
                    TransportMinutesNo = "BBVC-2025-0610",
                    ClaimAmount = 600_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-4),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-4),
                    NumberOfDaysDeferred = 30,
                    Status = BankBillDetailStatus.Delivered,
                    Note = "Báº£n gá»‘c CO vÃ  CQ xuáº¥t xÆ°á»Ÿng"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100202",
                    ModelCode = "STARGAZER-X",
                    SpecCode = "SGZ-X-PREM",
                    SpecDescription = "Hyundai Stargazer X 1.5L Cao cáº¥p",
                    EngineNo = "G4FL-776214",
                    CONo = "CO-TCG-2025-10202",
                    CabinCONo = "CQ-2025-10202",
                    DeclarationNo = "TK-HQ-2025-104402",
                    BankGuaranteeNo = "BL-CTG-2025/110",
                    HTCInvoiceNo = "HD-HTC-2025-0451",
                    TCGInvoiceNo = "HD-TCG-2025-1103",
                    TransportMinutesNo = "BBVC-2025-0610",
                    ClaimAmount = 600_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-4),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-4),
                    NumberOfDaysDeferred = 30,
                    Status = BankBillDetailStatus.Delivered,
                    Note = "Há»“ sÆ¡ bÃ n giao niÃªm phong tÃºi há»“ sÆ¡ xe"
                }
            );
            await db.SaveChangesAsync();

            // 3. BiÃªn báº£n bÃ n giao Techcombank (TCB) - ÄÃ£ thanh toÃ¡n quyáº¿t toÃ¡n há»‘i phiáº¿u hoÃ n táº¥t (Settled)
            var bb3 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250512-003",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai Pháº¡m VÄƒn Äá»“ng",
                BankBillDate = DateTime.Today.AddDays(-15),
                BankBillPrintDate = DateTime.Today.AddDays(-15),
                BankBillReciveDate = DateTime.Today.AddDays(-12),
                TotalVehicles = 2,
                TotalClaimAmount = 1_850_000_000,
                Status = BankBillMinutesStatus.Settled,
                Remark = "ÄÃ£ thu tiá»n thanh toÃ¡n Ä‘á»§ 1.85 tá»· Ä‘á»“ng qua há»‘i phiáº¿u Techcombank, hoÃ n táº¥t quyáº¿t toÃ¡n há»“ sÆ¡",
                CreatedBy = "ChuyenVienHopDong",
                CreatedAt = DateTime.Now.AddDays(-15),
                HandedOverBy = "LeVanToan_GiaoNhan",
                HandedOverAt = DateTime.Now.AddDays(-14),
                BankReceivedBy = "DoThiMai_TCB",
                BankReceivedAt = DateTime.Now.AddDays(-12),
                SettledBy = "TruongPhongKeToan",
                SettledAt = DateTime.Now.AddDays(-2)
            };
            db.BankBillMinutes.Add(bb3);
            await db.SaveChangesAsync();

            db.BankBillMinutesDetails.AddRange(
                new BankBillMinutesDetail
                {
                    MinutesId = bb3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100301",
                    ModelCode = "CUSTIN-1.5T",
                    SpecCode = "CST-1.5T-DB",
                    SpecDescription = "Hyundai Custin 1.5 Turbo Báº£n Äáº·c biá»‡t",
                    EngineNo = "G4FS-229045",
                    CONo = "CO-TCG-2025-11050",
                    CabinCONo = "CQ-2025-11050",
                    DeclarationNo = "TK-HQ-2025-112201",
                    BankGuaranteeNo = "BL-TCB-2025/112",
                    HTCInvoiceNo = "HD-HTC-2025-0512",
                    TCGInvoiceNo = "HD-TCG-2025-1215",
                    TransportMinutesNo = "BBVC-2025-0701",
                    ClaimAmount = 850_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-15),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-15),
                    NumberOfDaysDeferred = 60,
                    Status = BankBillDetailStatus.BankVerified,
                    Note = "ÄÃ£ quyáº¿t toÃ¡n thanh toÃ¡n"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100302",
                    ModelCode = "PALISADE-PRE",
                    SpecCode = "PLS-2.2D-PREM",
                    SpecDescription = "Hyundai Palisade 2.2 Diesel Prestige 6 chá»—",
                    EngineNo = "D4HB-667102",
                    CONo = "CO-TCG-2025-11051",
                    CabinCONo = "CQ-2025-11051",
                    DeclarationNo = "TK-HQ-2025-112202",
                    BankGuaranteeNo = "BL-TCB-2025/112",
                    HTCInvoiceNo = "HD-HTC-2025-0513",
                    TCGInvoiceNo = "HD-TCG-2025-1216",
                    TransportMinutesNo = "BBVC-2025-0701",
                    ClaimAmount = 1_000_000_000,
                    GuaranteeDateStart = DateTime.Today.AddDays(-15),
                    GuaranteeDateOpen = DateTime.Today.AddDays(-15),
                    NumberOfDaysDeferred = 60,
                    Status = BankBillDetailStatus.BankVerified,
                    Note = "ÄÃ£ quyáº¿t toÃ¡n thanh toÃ¡n"
                }
            );
            await db.SaveChangesAsync();

            // 4. BiÃªn báº£n bÃ n giao MBBank - Má»›i láº­p, Chá» bÃ n giao sang ngÃ¢n hÃ ng (PendingHandover)
            var bb4 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250514-004",
                BankCode = "MBB",
                BankName = "NgÃ¢n hÃ ng TMCP QuÃ¢n Äá»™i (MBBank)",
                PartnerCode = "DLR-HYUNDAI-SAIGON",
                PartnerName = "Äáº¡i lÃ½ Ã” tÃ´ Hyundai SÃ i GÃ²n",
                BankBillDate = DateTime.Today,
                BankBillPrintDate = DateTime.Today,
                TotalVehicles = 2,
                TotalClaimAmount = 1_680_000_000,
                Status = BankBillMinutesStatus.PendingHandover,
                Remark = "Äá»£t giao xe Tucson vÃ  Elantra báº£o lÃ£nh qua MBBank Chi nhÃ¡nh Sá»Ÿ Giao dá»‹ch",
                CreatedBy = "ChuyenVienHopDong",
                CreatedAt = DateTime.Now.AddHours(-2)
            };
            db.BankBillMinutes.Add(bb4);
            await db.SaveChangesAsync();

            db.BankBillMinutesDetails.AddRange(
                new BankBillMinutesDetail
                {
                    MinutesId = bb4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100401",
                    ModelCode = "TUCSON-TURBO",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Hyundai Tucson 1.6 T-GDi Turbo",
                    EngineNo = "G4FP-334109",
                    CONo = "CO-TCG-2025-12301",
                    CabinCONo = "CQ-2025-12301",
                    DeclarationNo = "TK-HQ-2025-125001",
                    BankGuaranteeNo = "BL-MBB-2025/095",
                    HTCInvoiceNo = "HD-HTC-2025-0601",
                    TCGInvoiceNo = "HD-TCG-2025-1350",
                    TransportMinutesNo = "BBVC-2025-0811",
                    ClaimAmount = 980_000_000,
                    GuaranteeDateStart = DateTime.Today,
                    GuaranteeDateOpen = DateTime.Today,
                    NumberOfDaysDeferred = 30,
                    Status = BankBillDetailStatus.Pending,
                    Note = "Chuáº©n bá»‹ niÃªm phong chá»©ng tá»« giao NgÃ¢n hÃ ng"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100402",
                    ModelCode = "ELANTRA-NLINE",
                    SpecCode = "ELN-1.6T-NL",
                    SpecDescription = "Hyundai Elantra N Line 1.6 T-GDi Thá»ƒ thao",
                    EngineNo = "G4FJ-551098",
                    CONo = "CO-TCG-2025-12302",
                    CabinCONo = "CQ-2025-12302",
                    DeclarationNo = "TK-HQ-2025-125002",
                    BankGuaranteeNo = "BL-MBB-2025/095",
                    HTCInvoiceNo = "HD-HTC-2025-0602",
                    TCGInvoiceNo = "HD-TCG-2025-1351",
                    TransportMinutesNo = "BBVC-2025-0811",
                    ClaimAmount = 700_000_000,
                    GuaranteeDateStart = DateTime.Today,
                    GuaranteeDateOpen = DateTime.Today,
                    NumberOfDaysDeferred = 30,
                    Status = BankBillDetailStatus.Pending,
                    Note = "Báº£n gá»‘c CO vÃ  CQ Ä‘áº§y Ä‘á»§"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Báº£ng kÃª thanh toÃ¡n chi phÃ­ kiá»ƒm tra xe PDI (Pmt_PaymentPDI / BizHTC.Payment)
        if (!await db.PaymentPDIs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Báº£ng kÃª thÃ¡ng 04/2025 - ÄÃ£ thanh toÃ¡n & Quyáº¿t toÃ¡n chi phÃ­ hoÃ n táº¥t (Paid)
            var pdi1 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202504-001",
                PmtMonth = "2025-04",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tÃ¢m Dá»‹ch vá»¥ Ká»¹ thuáº­t & PDI Ã” tÃ´ TCMS",
                TotalVehicles = 4,
                TotalCostIn = 600_000,   // 4 * 150k
                TotalCostOut = 800_000,  // 4 * 200k
                TotalAmount = 1_400_000,
                VATRate = 10.0m,
                AmountVAT = 140_000,
                TotalAmountAfterVAT = 1_540_000,
                Status = PaymentPDIStatus.Paid,
                TCMSSignStatus = PDISignStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Now.AddDays(-12),
                HTVSignStatus = PDISignStatus.Signed,
                HTVSignUser = "TranThiHong_TruongPhongKT_HTV",
                HTVSignDTime = DateTime.Now.AddDays(-10),
                Appr1By = "NguyenVanQuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-12),
                Appr2By = "TranThiHong_TruongPhongKT_HTV",
                Appr2DTime = DateTime.Now.AddDays(-10),
                PaidBy = "PhamVanThanh_KeToanNganHang",
                PaidAt = DateTime.Now.AddDays(-8),
                BankTxnRef = "UNC-VCB-PDI-202504-8891",
                FilePath = "/documents/pdi/PDI-202504-001-Signed.pdf",
                Remark = "Thanh toÃ¡n chi phÃ­ PDI xe nháº­p/xuáº¥t kho ká»³ thÃ¡ng 04/2025 táº¡i Kho NhÃ  mÃ¡y Ninh BÃ¬nh",
                CreatedBy = "LeMinhKhoa_KiemDinhVien",
                CreatedAt = DateTime.Now.AddDays(-15)
            };
            db.PaymentPDIs.Add(pdi1);
            await db.SaveChangesAsync();

            db.PaymentPDIDetails.AddRange(
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200101",
                    CarId = "CAR-SF-0101",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-AWD",
                    SpecDescription = "Báº£n 6 chá»— cao cáº¥p HTRAC AWD",
                    ColorExtNameVN = "Tráº¯ng ngá»c trai",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-30),
                    DeliveryOutDate = DateTime.Today.AddDays(-16),
                    DlvMnNo = "BBVC-2025-0401",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI nháº­p kho: 100% Ä‘áº¡t tiÃªu chuáº©n; PDI xuáº¥t kho: rá»­a sáº¡ch, náº¡p áº¯c quy, kiá»ƒm tra Ã¡p suáº¥t lá»‘p"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200102",
                    CarId = "CAR-TUC-0102",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Báº£n 1.6 Turbo XÄƒng HTRAC",
                    ColorExtNameVN = "Äen Ãnh kim",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-28),
                    DeliveryOutDate = DateTime.Today.AddDays(-15),
                    DlvMnNo = "BBVC-2025-0402",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI Ä‘áº§y Ä‘á»§ há»‡ thá»‘ng Ä‘iá»‡n, cáº£m biáº¿n vÃ  cÃ¢n báº±ng Ä‘iá»‡n tá»­ ESC"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200103",
                    CarId = "CAR-CRT-0103",
                    ModelCode = "CRETA-PRE",
                    ModelName = "Hyundai Creta 1.5 Cao cáº¥p",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Báº£n SmartSense cao cáº¥p",
                    ColorExtNameVN = "Äá» ÄÃ´",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-27),
                    DeliveryOutDate = DateTime.Today.AddDays(-14),
                    DlvMnNo = "BBVC-2025-0403",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI Ä‘áº¡t yÃªu cáº§u, kiá»ƒm tra Ä‘Ã¨n LED trÆ°á»›c sau hoÃ n háº£o"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200104",
                    CarId = "CAR-ACC-0104",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c biá»‡t",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sá»‘ tá»± Ä‘á»™ng báº£n Äáº·c biá»‡t 2025",
                    ColorExtNameVN = "Báº¡c Ãnh kim",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-25),
                    DeliveryOutDate = DateTime.Today.AddDays(-13),
                    DlvMnNo = "BBVC-2025-0404",
                    DealerCode = "DLR-HYUNDAI-SAIGON",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI Ä‘á»™ng cÆ¡ SmartStream G1.5 kiá»ƒm tra Ã¡p suáº¥t dáº§u vÃ  dung dá»‹ch lÃ m mÃ¡t"
                }
            );
            await db.SaveChangesAsync();

            // 2. Báº£ng kÃª thÃ¡ng 05/2025 Ä‘á»£t 1 - HTV ÄÃ£ phÃª duyá»‡t cáº¥p 2 (HTVApproved), Chá» káº¿ toÃ¡n chuyá»ƒn khoáº£n thanh toÃ¡n
            var pdi2 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-001",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tÃ¢m Dá»‹ch vá»¥ Ká»¹ thuáº­t & PDI Ã” tÃ´ TCMS",
                TotalVehicles = 3,
                TotalCostIn = 450_000,   // 3 * 150k
                TotalCostOut = 600_000,  // 3 * 200k
                TotalAmount = 1_050_000,
                VATRate = 10.0m,
                AmountVAT = 105_000,
                TotalAmountAfterVAT = 1_155_000,
                Status = PaymentPDIStatus.HTVApproved,
                TCMSSignStatus = PDISignStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Now.AddDays(-5),
                HTVSignStatus = PDISignStatus.Signed,
                HTVSignUser = "TranThiHong_TruongPhongKT_HTV",
                HTVSignDTime = DateTime.Now.AddDays(-3),
                Appr1By = "NguyenVanQuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-5),
                Appr2By = "TranThiHong_TruongPhongKT_HTV",
                Appr2DTime = DateTime.Now.AddDays(-3),
                FilePath = "/documents/pdi/PDI-202505-001-Approved.pdf",
                Remark = "Báº£ng kÃª Ä‘á»£t 1 thÃ¡ng 05/2025 xe Palisade, Custin, Stargazer X táº¡i Kho ÄÃ´ng Anh",
                CreatedBy = "LeMinhKhoa_KiemDinhVien",
                CreatedAt = DateTime.Now.AddDays(-6)
            };
            db.PaymentPDIs.Add(pdi2);
            await db.SaveChangesAsync();

            db.PaymentPDIDetails.AddRange(
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200201",
                    CarId = "CAR-PLS-0201",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PLS-2.2D-PRE",
                    SpecDescription = "Báº£n Prestige mÃ¡y Dáº§u 6 chá»—",
                    ColorExtNameVN = "Xanh BÃ³ng ÄÃªm",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-12),
                    DeliveryOutDate = DateTime.Today.AddDays(-5),
                    DlvMnNo = "BBVC-2025-0511",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI há»‡ thá»‘ng Ã¢m thanh Infinity 12 loa, kiá»ƒm tra radar SmartSense"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200202",
                    CarId = "CAR-CST-0202",
                    ModelCode = "CUSTIN-1.5T",
                    ModelName = "Hyundai Custin 1.5 Turbo Äáº·c biá»‡t",
                    SpecCode = "CST-1.5T-DB",
                    SpecDescription = "Báº£n MPV 7 chá»— cá»­a trÆ°á»£t Ä‘iá»‡n",
                    ColorExtNameVN = "Tráº¯ng ngá»c trai",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-11),
                    DeliveryOutDate = DateTime.Today.AddDays(-4),
                    DlvMnNo = "BBVC-2025-0512",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI gháº¿ cÆ¡ trÆ°á»Ÿng hÃ ng gháº¿ 2 chá»‰nh Ä‘iá»‡n, cá»­a giÃ³ Ä‘iá»u hÃ²a Ä‘á»™c láº­p"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200203",
                    CarId = "CAR-SGZ-0203",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao cáº¥p",
                    SpecCode = "SGZ-X-PREM",
                    SpecDescription = "Báº£n Crossover MPV 7 chá»—",
                    ColorExtNameVN = "XÃ¡m Kim loáº¡i",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-10),
                    DeliveryOutDate = DateTime.Today.AddDays(-4),
                    DlvMnNo = "BBVC-2025-0513",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI phanh tay Ä‘iá»‡n tá»­ EPB vÃ  Auto Hold hoáº¡t Ä‘á»™ng chuáº©n"
                }
            );
            await db.SaveChangesAsync();

            // 3. Báº£ng kÃª thÃ¡ng 05/2025 Ä‘á»£t 2 - TCMS ÄÃ£ duyá»‡t cáº¥p 1 (TCMSApproved), Äang chá» lÃ£nh Ä‘áº¡o HTV duyá»‡t cáº¥p 2
            var pdi3 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-002",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tÃ¢m Dá»‹ch vá»¥ Ká»¹ thuáº­t & PDI Ã” tÃ´ TCMS",
                TotalVehicles = 3,
                TotalCostIn = 450_000,
                TotalCostOut = 600_000,
                TotalAmount = 1_050_000,
                VATRate = 10.0m,
                AmountVAT = 105_000,
                TotalAmountAfterVAT = 1_155_000,
                Status = PaymentPDIStatus.TCMSApproved,
                TCMSSignStatus = PDISignStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Now.AddDays(-1),
                HTVSignStatus = PDISignStatus.Pending,
                Appr1By = "NguyenVanQuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-1),
                Remark = "Äá»£t giao xe Elantra N Line, Accent vÃ  Creta cho Ä‘áº¡i lÃ½ khu vá»±c Miá»n Báº¯c",
                CreatedBy = "VuThiThu_CanBoKeHoach",
                CreatedAt = DateTime.Now.AddDays(-2)
            };
            db.PaymentPDIs.Add(pdi3);
            await db.SaveChangesAsync();

            db.PaymentPDIDetails.AddRange(
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200301",
                    CarId = "CAR-ELN-0301",
                    ModelCode = "ELANTRA-NLINE",
                    ModelName = "Hyundai Elantra N Line 1.6 Turbo",
                    SpecCode = "ELN-1.6T-NL",
                    SpecDescription = "Báº£n thá»ƒ thao 204 mÃ£ lá»±c ly há»£p kÃ©p 7 cáº¥p",
                    ColorExtNameVN = "Äá» Thá»ƒ thao",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DlvMnNo = "BBVC-2025-0521",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI cháº¿ Ä‘á»™ lÃ¡i Sport+, cÃ¡nh giÃ³ thá»ƒ thao vÃ  á»‘ng xáº£ kÃ©p thá»ƒ thao"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200302",
                    CarId = "CAR-ACC-0302",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT TiÃªu chuáº©n",
                    SpecCode = "ACC-1.5L-TC",
                    SpecDescription = "Báº£n tá»± Ä‘á»™ng tiÃªu chuáº©n 2025",
                    ColorExtNameVN = "Tráº¯ng",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DlvMnNo = "BBVC-2025-0522",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI kiá»ƒm tra 4 lá»‘p, há»‡ thá»‘ng cÃ¢n báº±ng Ã¡p suáº¥t TPMS chuáº©n"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200303",
                    CarId = "CAR-CRT-0303",
                    ModelCode = "CRETA-PRE",
                    ModelName = "Hyundai Creta 1.5 Äáº·c biá»‡t",
                    SpecCode = "CRT-1.5L-DB",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng Äáº·c biá»‡t",
                    ColorExtNameVN = "Xanh DÆ°Æ¡ng",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-4),
                    DeliveryOutDate = DateTime.Today.AddDays(-1),
                    DlvMnNo = "BBVC-2025-0523",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI hoÃ n thiá»‡n tem nhÃ£n, sá»• hÆ°á»›ng dáº«n sá»­ dá»¥ng vÃ  phá»¥ kiá»‡n theo xe"
                }
            );
            await db.SaveChangesAsync();

            // 4. Báº£ng kÃª thÃ¡ng 05/2025 Ä‘á»£t 3 - Má»›i táº¡o nhÃ¡p (Draft), Äang chuáº©n bá»‹ kiá»ƒm Ä‘á»‹nh xe
            var pdi4 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-003",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tÃ¢m Dá»‹ch vá»¥ Ká»¹ thuáº­t & PDI Ã” tÃ´ TCMS",
                TotalVehicles = 2,
                TotalCostIn = 300_000,
                TotalCostOut = 400_000,
                TotalAmount = 700_000,
                VATRate = 10.0m,
                AmountVAT = 70_000,
                TotalAmountAfterVAT = 770_000,
                Status = PaymentPDIStatus.Draft,
                TCMSSignStatus = PDISignStatus.Pending,
                HTVSignStatus = PDISignStatus.Pending,
                Remark = "Äá»£t bÃ n giao xe Ä‘iá»‡n thuáº§n IONIQ 5 vÃ  SUV Ä‘Ã´ thá»‹ Venue vá»«a hoÃ n táº¥t kiá»ƒm tra kho",
                CreatedBy = "HoangVanBac_KTV",
                CreatedAt = DateTime.Now.AddHours(-3)
            };
            db.PaymentPDIs.Add(pdi4);
            await db.SaveChangesAsync();

            db.PaymentPDIDetails.AddRange(
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200401",
                    CarId = "CAR-ION-0401",
                    ModelCode = "IONIQ-5-PRE",
                    ModelName = "Hyundai IONIQ 5 Prestige EV",
                    SpecCode = "IQ5-72KWH-PRE",
                    SpecDescription = "Xe thuáº§n Ä‘iá»‡n Pin 72.6 kWh cÃ´ng nghá»‡ sáº¡c 800V siÃªu nhanh",
                    ColorExtNameVN = "VÃ ng Titan Má»",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-2),
                    DeliveryOutDate = DateTime.Today,
                    DlvMnNo = "BBVC-2025-0531",
                    DealerCode = "DLR-HYUNDAI-SAIGON",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Pending,
                    Remark = "PDI dung lÆ°á»£ng pin SOC 85%, cá»•ng sáº¡c CCS2 vÃ  bá»™ sáº¡c di Ä‘á»™ng 220V kÃ¨m xe"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200402",
                    CarId = "CAR-VNU-0402",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 Turbo Äáº·c biá»‡t",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "Báº£n SUV cá»¡ A Ä‘á»™ng cÆ¡ Turbo T-GDi",
                    ColorExtNameVN = "Xanh Ngá»c",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-2),
                    DeliveryOutDate = DateTime.Today,
                    DlvMnNo = "BBVC-2025-0532",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Pending,
                    Remark = "PDI kiá»ƒm tra Ä‘á»™ng cÆ¡ tÄƒng Ã¡p 1.0L, há»™p sá»‘ tá»± Ä‘á»™ng 7DCT mÆ°á»£t mÃ "
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 10. Seed Dá»¯ liá»‡u máº«u Há»“ sÆ¡ Quáº£n lÃ½ & TÃ­nh pháº¡t Cháº­m thanh toÃ¡n Xe (Late Payment Delay Penalty) =====
        if (!await db.LatePaymentPenalties.AnyAsync())
        {
            // 1. Há»“ sÆ¡ pháº¡t ÄÃƒ QUYáº¾T TOÃN THU PHáº T (Settled) - Äáº¡i lÃ½ Hyundai HÃ  ÄÃ´ng
            var pen1 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202504-001",
                SOCode = "SO-2025-04-HD01",
                DealerCode = "DLR-HYUNDAI-HADONG",
                DealerName = "Hyundai HÃ  ÄÃ´ng (á»¦y quyá»n phÃ¢n phá»‘i)",
                ContractNo = "HD-2025-0418-HTC",
                SOApprovedDate = DateTime.Today.AddDays(-65),
                TotalApprovedQuantity = 3,
                TotalUnitPriceActual = 3_250_000_000,
                MaxDelayDaysDeposit = 4,
                MaxDelayDaysGrtOpen = 7,
                MaxDelayDaysGrtPay = 12,
                MaxDelayDays60Pmt = 5,
                MaxDelayDaysRemain = 8,
                TotalDatePenalty = 12,
                PenaltyRateAnnual = 12.0m,
                AmountPenaltySystem = 12_821_918,
                PenalizeActual = 10_000_000,
                WaivedAmount = 2_821_918,
                Status = LatePaymentPenaltyStatus.Settled,
                Remark = "LÃ´ 3 xe SantaFe vÃ  Tucson giao Ä‘á»£t 1 thÃ¡ng 04/2025",
                AdjustmentReason = "Miá»…n giáº£m 2.821.918 Ä‘ do Ä‘áº¡i lÃ½ Ä‘á»‘i soÃ¡t bÃ¹ trá»« tÃ i khoáº£n báº£o lÃ£nh ngÃ¢n hÃ ng VPBank",
                PaymentProofRef = "UNC-VCB-7749102",
                CreatedBy = "VuThiThu_KeToanCongNo",
                CreatedAt = DateTime.Now.AddDays(-30),
                CalculatedAt = DateTime.Now.AddDays(-29),
                ReviewedBy = "NguyenThiMai_KTT",
                ReviewedAt = DateTime.Now.AddDays(-28),
                ApprovedBy = "LeHoangNam_GDTaiChinh_HTC",
                ApprovedAt = DateTime.Now.AddDays(-26),
                SettledBy = "TranMinhAnh_ThuQuy",
                SettledAt = DateTime.Now.AddDays(-25)
            };
            db.LatePaymentPenalties.Add(pen1);
            await db.SaveChangesAsync();

            db.LatePaymentPenaltyDetails.AddRange(
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-2025-01",
                    VIN = "KMHCT81EPHU300101",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai SantaFe Calligraphy 2.5 AWD",
                    ColorName = "Äen Huyá»n BÃ­",
                    UnitPriceActual = 1_369_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-60),
                    ActualDepositDate = DateTime.Today.AddDays(-56),
                    GrtDueDate = DateTime.Today.AddDays(-53),
                    ActualGrtDate = DateTime.Today.AddDays(-46),
                    GrtPayDueDate = DateTime.Today.AddDays(-38),
                    ActualGrtPayDate = DateTime.Today.AddDays(-26),
                    Payment60DueDate = DateTime.Today.AddDays(-45),
                    Actual60PayDate = DateTime.Today.AddDays(-40),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-30),
                    ActualRemainPayDate = DateTime.Today.AddDays(-22),
                    DelayDaysDeposit = 4,
                    DelayDaysGrtOpen = 7,
                    DelayDaysGrtPay = 12,
                    DelayDays60Pmt = 5,
                    DelayDaysRemain = 8,
                    MaxDelayDays = 12,
                    ItemPenaltyAmount = 5_401_644,
                    ActualItemPenalty = 4_212_000,
                    Status = LatePaymentPenaltyDetailStatus.Settled,
                    Note = "Trá»… má»‘c thanh toÃ¡n báº£o lÃ£nh 12 ngÃ y do ngÃ¢n hÃ ng chuyá»ƒn lá»‡nh muá»™n"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-2025-02",
                    VIN = "KMHCT81EPHU300102",
                    ModelCode = "SANTAFE-PRE",
                    ModelName = "Hyundai SantaFe Cao cáº¥p 2.5 H-Trac",
                    ColorName = "Tráº¯ng Tinh KhÃ´i",
                    UnitPriceActual = 1_269_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-60),
                    ActualDepositDate = DateTime.Today.AddDays(-57),
                    GrtDueDate = DateTime.Today.AddDays(-53),
                    ActualGrtDate = DateTime.Today.AddDays(-48),
                    GrtPayDueDate = DateTime.Today.AddDays(-38),
                    ActualGrtPayDate = DateTime.Today.AddDays(-28),
                    Payment60DueDate = DateTime.Today.AddDays(-45),
                    Actual60PayDate = DateTime.Today.AddDays(-42),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-30),
                    ActualRemainPayDate = DateTime.Today.AddDays(-25),
                    DelayDaysDeposit = 3,
                    DelayDaysGrtOpen = 5,
                    DelayDaysGrtPay = 10,
                    DelayDays60Pmt = 3,
                    DelayDaysRemain = 5,
                    MaxDelayDays = 10,
                    ItemPenaltyAmount = 4_172_055,
                    ActualItemPenalty = 3_254_000,
                    Status = LatePaymentPenaltyDetailStatus.Settled,
                    Note = "Trá»… háº¡n báº£o lÃ£nh ngÃ¢n hÃ ng 10 ngÃ y"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-2025-01",
                    VIN = "KMHCT81EPHU300103",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6T HTRAC Turbo",
                    ColorName = "Äá» Máº­n",
                    UnitPriceActual = 612_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-60),
                    ActualDepositDate = DateTime.Today.AddDays(-59),
                    GrtDueDate = DateTime.Today.AddDays(-53),
                    ActualGrtDate = DateTime.Today.AddDays(-50),
                    GrtPayDueDate = DateTime.Today.AddDays(-38),
                    ActualGrtPayDate = DateTime.Today.AddDays(-22),
                    Payment60DueDate = DateTime.Today.AddDays(-45),
                    Actual60PayDate = DateTime.Today.AddDays(-44),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-30),
                    ActualRemainPayDate = DateTime.Today.AddDays(-24),
                    DelayDaysDeposit = 1,
                    DelayDaysGrtOpen = 3,
                    DelayDaysGrtPay = 16,
                    DelayDays60Pmt = 1,
                    DelayDaysRemain = 6,
                    MaxDelayDays = 16,
                    ItemPenaltyAmount = 3_248_219,
                    ActualItemPenalty = 2_534_000,
                    Status = LatePaymentPenaltyDetailStatus.Settled,
                    Note = "Thanh toÃ¡n dá»©t Ä‘iá»ƒm khi nháº­n há»“ sÆ¡ gá»‘c"
                }
            );
            await db.SaveChangesAsync();

            // 2. Há»“ sÆ¡ pháº¡t ÄÃƒ PHÃŠ DUYá»†T CHá»T PHáº T (Approved) - Äáº¡i lÃ½ Hyundai SÃ i GÃ²n
            var pen2 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-001",
                SOCode = "SO-2025-05-SG02",
                DealerCode = "DLR-HYUNDAI-SAIGON",
                DealerName = "Hyundai SÃ i GÃ²n 1S (Äáº¡i lÃ½ Miá»n Nam)",
                ContractNo = "HD-2025-0502-HTC",
                SOApprovedDate = DateTime.Today.AddDays(-40),
                TotalApprovedQuantity = 2,
                TotalUnitPriceActual = 2_138_000_000,
                MaxDelayDaysDeposit = 5,
                MaxDelayDaysGrtOpen = 9,
                MaxDelayDaysGrtPay = 15,
                MaxDelayDays60Pmt = 6,
                MaxDelayDaysRemain = 11,
                TotalDatePenalty = 15,
                PenaltyRateAnnual = 12.0m,
                AmountPenaltySystem = 10_543_562,
                PenalizeActual = 8_500_000,
                WaivedAmount = 2_043_562,
                Status = LatePaymentPenaltyStatus.Approved,
                Remark = "ÄÆ¡n xe giao khu vá»±c phÃ­a Nam lÃ´ Ä‘áº§u thÃ¡ng 05/2025",
                AdjustmentReason = "Giáº£m 2.043.562 Ä‘ do thá»i gian váº­n chuyá»ƒn tÃ u há»a Báº¯c - Nam bá»‹ cháº­m tiáº¿n Ä‘á»™ giao nháº­n",
                PaymentProofRef = null,
                CreatedBy = "VuThiThu_KeToanCongNo",
                CreatedAt = DateTime.Now.AddDays(-14),
                CalculatedAt = DateTime.Now.AddDays(-13),
                ReviewedBy = "NguyenThiMai_KTT",
                ReviewedAt = DateTime.Now.AddDays(-10),
                ApprovedBy = "LeHoangNam_GDTaiChinh_HTC",
                ApprovedAt = DateTime.Now.AddDays(-5)
            };
            db.LatePaymentPenalties.Add(pen2);
            await db.SaveChangesAsync();

            db.LatePaymentPenaltyDetails.AddRange(
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CUS-2025-01",
                    VIN = "KMHCT81EPHU300201",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0L Turbo Cao Cáº¥p",
                    ColorName = "Báº¡c Ãnh Kim",
                    UnitPriceActual = 1_079_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-38),
                    ActualDepositDate = DateTime.Today.AddDays(-33),
                    GrtDueDate = DateTime.Today.AddDays(-30),
                    ActualGrtDate = DateTime.Today.AddDays(-21),
                    GrtPayDueDate = DateTime.Today.AddDays(-20),
                    ActualGrtPayDate = DateTime.Today.AddDays(-5),
                    Payment60DueDate = DateTime.Today.AddDays(-25),
                    Actual60PayDate = DateTime.Today.AddDays(-19),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-15),
                    ActualRemainPayDate = DateTime.Today.AddDays(-4),
                    DelayDaysDeposit = 5,
                    DelayDaysGrtOpen = 9,
                    DelayDaysGrtPay = 15,
                    DelayDays60Pmt = 6,
                    DelayDaysRemain = 11,
                    MaxDelayDays = 15,
                    ItemPenaltyAmount = 5_321_096,
                    ActualItemPenalty = 4_290_000,
                    Status = LatePaymentPenaltyDetailStatus.Approved,
                    Note = "Cháº­m thanh toÃ¡n báº£o lÃ£nh táº¡i MBBank"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-2025-03",
                    VIN = "KMHCT81EPHU300202",
                    ModelCode = "SANTAFE-EXT",
                    ModelName = "Hyundai SantaFe TiÃªu chuáº©n 2.5L",
                    ColorName = "Äen",
                    UnitPriceActual = 1_059_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-38),
                    ActualDepositDate = DateTime.Today.AddDays(-35),
                    GrtDueDate = DateTime.Today.AddDays(-30),
                    ActualGrtDate = DateTime.Today.AddDays(-24),
                    GrtPayDueDate = DateTime.Today.AddDays(-20),
                    ActualGrtPayDate = DateTime.Today.AddDays(-5),
                    Payment60DueDate = DateTime.Today.AddDays(-25),
                    Actual60PayDate = DateTime.Today.AddDays(-20),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-15),
                    ActualRemainPayDate = DateTime.Today.AddDays(-7),
                    DelayDaysDeposit = 3,
                    DelayDaysGrtOpen = 6,
                    DelayDaysGrtPay = 15,
                    DelayDays60Pmt = 5,
                    DelayDaysRemain = 8,
                    MaxDelayDays = 15,
                    ItemPenaltyAmount = 5_222_466,
                    ActualItemPenalty = 4_210_000,
                    Status = LatePaymentPenaltyDetailStatus.Approved,
                    Note = "Ban GiÃ¡m Ä‘á»‘c Ä‘Ã£ kÃ½ duyá»‡t chá»‘t sá»‘ tiá»n pháº¡t"
                }
            );
            await db.SaveChangesAsync();

            // 3. Há»“ sÆ¡ pháº¡t ÄANG THáº¨M Äá»ŠNH (Reviewed) - Äáº¡i lÃ½ Hyundai ÄÃ´ng Anh
            var pen3 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-002",
                SOCode = "SO-2025-05-DA03",
                DealerCode = "DLR-HYUNDAI-DONGANH",
                DealerName = "Hyundai ÄÃ´ng Anh (Chi nhÃ¡nh miá»n Báº¯c)",
                ContractNo = "HD-2025-0511-HTC",
                SOApprovedDate = DateTime.Today.AddDays(-25),
                TotalApprovedQuantity = 2,
                TotalUnitPriceActual = 1_538_000_000,
                MaxDelayDaysDeposit = 3,
                MaxDelayDaysGrtOpen = 5,
                MaxDelayDaysGrtPay = 8,
                MaxDelayDays60Pmt = 4,
                MaxDelayDaysRemain = 6,
                TotalDatePenalty = 8,
                PenaltyRateAnnual = 12.0m,
                AmountPenaltySystem = 4_045_151,
                PenalizeActual = 3_500_000,
                WaivedAmount = 545_151,
                Status = LatePaymentPenaltyStatus.Reviewed,
                Remark = "ÄÆ¡n xe Creta vÃ  Accent Ä‘á»£t giá»¯a thÃ¡ng 05",
                AdjustmentReason = "Káº¿ toÃ¡n tháº©m Ä‘á»‹nh Ä‘á» xuáº¥t má»©c pháº¡t 3.500.000 Ä‘ theo Ä‘á» xuáº¥t giáº£i trÃ¬nh cá»§a Ä‘áº¡i lÃ½",
                PaymentProofRef = null,
                CreatedBy = "VuThiThu_KeToanCongNo",
                CreatedAt = DateTime.Now.AddDays(-7),
                CalculatedAt = DateTime.Now.AddDays(-6),
                ReviewedBy = "NguyenThiMai_KTT",
                ReviewedAt = DateTime.Now.AddDays(-3)
            };
            db.LatePaymentPenalties.Add(pen3);
            await db.SaveChangesAsync();

            db.LatePaymentPenaltyDetails.AddRange(
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRT-2025-01",
                    VIN = "KMHCT81EPHU300301",
                    ModelCode = "CRETA-CAO-CAP",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    ColorName = "Äá» Máº­n",
                    UnitPriceActual = 740_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-22),
                    ActualDepositDate = DateTime.Today.AddDays(-19),
                    GrtDueDate = DateTime.Today.AddDays(-17),
                    ActualGrtDate = DateTime.Today.AddDays(-12),
                    GrtPayDueDate = DateTime.Today.AddDays(-10),
                    ActualGrtPayDate = DateTime.Today.AddDays(-2),
                    Payment60DueDate = DateTime.Today.AddDays(-15),
                    Actual60PayDate = DateTime.Today.AddDays(-11),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-8),
                    ActualRemainPayDate = DateTime.Today.AddDays(-2),
                    DelayDaysDeposit = 3,
                    DelayDaysGrtOpen = 5,
                    DelayDaysGrtPay = 8,
                    DelayDays60Pmt = 4,
                    DelayDaysRemain = 6,
                    MaxDelayDays = 8,
                    ItemPenaltyAmount = 1_946_301,
                    ActualItemPenalty = 1_684_000,
                    Status = LatePaymentPenaltyDetailStatus.Calculated,
                    Note = "Cháº­m tiáº¿n Ä‘á»™ thanh toÃ¡n tiá»n báº£o lÃ£nh 8 ngÃ y"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-2025-01",
                    VIN = "KMHCT81EPHU300302",
                    ModelCode = "ACCENT-AT-DACBIET",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    ColorName = "Tráº¯ng",
                    UnitPriceActual = 798_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-22),
                    ActualDepositDate = DateTime.Today.AddDays(-20),
                    GrtDueDate = DateTime.Today.AddDays(-17),
                    ActualGrtDate = DateTime.Today.AddDays(-13),
                    GrtPayDueDate = DateTime.Today.AddDays(-10),
                    ActualGrtPayDate = DateTime.Today.AddDays(-2),
                    Payment60DueDate = DateTime.Today.AddDays(-15),
                    Actual60PayDate = DateTime.Today.AddDays(-12),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-8),
                    ActualRemainPayDate = DateTime.Today.AddDays(-3),
                    DelayDaysDeposit = 2,
                    DelayDaysGrtOpen = 4,
                    DelayDaysGrtPay = 8,
                    DelayDays60Pmt = 3,
                    DelayDaysRemain = 5,
                    MaxDelayDays = 8,
                    ItemPenaltyAmount = 2_098_850,
                    ActualItemPenalty = 1_816_000,
                    Status = LatePaymentPenaltyDetailStatus.Calculated,
                    Note = "Chá» GiÃ¡m Ä‘á»‘c TÃ i chÃ­nh phÃª duyá»‡t má»©c pháº¡t cuá»‘i"
                }
            );
            await db.SaveChangesAsync();

            // 4. Há»“ sÆ¡ pháº¡t Vá»ªA TÃNH Tá»° Äá»˜NG (Calculated) - Äáº¡i lÃ½ Hyundai An KhÃ¡nh
            var pen4 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-003",
                SOCode = "SO-2025-05-AK04",
                DealerCode = "DLR-HYUNDAI-ANKHANH",
                DealerName = "Hyundai An KhÃ¡nh (Äáº¡i lÃ½ 3S)",
                ContractNo = "HD-2025-0518-HTC",
                SOApprovedDate = DateTime.Today.AddDays(-15),
                TotalApprovedQuantity = 1,
                TotalUnitPriceActual = 569_000_000,
                MaxDelayDaysDeposit = 2,
                MaxDelayDaysGrtOpen = 3,
                MaxDelayDaysGrtPay = 5,
                MaxDelayDays60Pmt = 2,
                MaxDelayDaysRemain = 4,
                TotalDatePenalty = 5,
                PenaltyRateAnnual = 12.0m,
                AmountPenaltySystem = 935_342,
                PenalizeActual = 935_342,
                WaivedAmount = 0,
                Status = LatePaymentPenaltyStatus.Calculated,
                Remark = "ÄÆ¡n bá»• sung xe Accent sá»‘ tá»± Ä‘á»™ng cho Ä‘áº¡i lÃ½ An KhÃ¡nh",
                AdjustmentReason = null,
                CreatedBy = "VuThiThu_KeToanCongNo",
                CreatedAt = DateTime.Now.AddDays(-2),
                CalculatedAt = DateTime.Now.AddDays(-1)
            };
            db.LatePaymentPenalties.Add(pen4);
            await db.SaveChangesAsync();

            db.LatePaymentPenaltyDetails.Add(
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-2025-02",
                    VIN = "KMHCT81EPHU300401",
                    ModelCode = "ACCENT-AT-TIEUCHUAN",
                    ModelName = "Hyundai Accent 1.5 AT TiÃªu Chuáº©n",
                    ColorName = "Báº¡c",
                    UnitPriceActual = 569_000_000,
                    DepositDueDate = DateTime.Today.AddDays(-12),
                    ActualDepositDate = DateTime.Today.AddDays(-10),
                    GrtDueDate = DateTime.Today.AddDays(-9),
                    ActualGrtDate = DateTime.Today.AddDays(-6),
                    GrtPayDueDate = DateTime.Today.AddDays(-5),
                    ActualGrtPayDate = DateTime.Today,
                    Payment60DueDate = DateTime.Today.AddDays(-7),
                    Actual60PayDate = DateTime.Today.AddDays(-5),
                    PaymentRemainDueDate = DateTime.Today.AddDays(-4),
                    ActualRemainPayDate = DateTime.Today,
                    DelayDaysDeposit = 2,
                    DelayDaysGrtOpen = 3,
                    DelayDaysGrtPay = 5,
                    DelayDays60Pmt = 2,
                    DelayDaysRemain = 4,
                    MaxDelayDays = 5,
                    ItemPenaltyAmount = 935_342,
                    ActualItemPenalty = 935_342,
                    Status = LatePaymentPenaltyDetailStatus.Calculated,
                    Note = "Há»“ sÆ¡ má»›i tÃ­nh toÃ¡n, Ä‘ang chuáº©n bá»‹ chuyá»ƒn tháº©m Ä‘á»‹nh"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 11. Seed Dá»¯ liá»‡u máº«u Báº£ng kÃª Thanh toÃ¡n Chi phÃ­ Váº­n táº£i & Báº£o hiá»ƒm Xe (Pmt_TransportIns) =====
        if (!await db.TransportInsPayments.AnyAsync())
        {
            // 1. Báº£ng kÃª ÄÃƒ QUYáº¾T TOÃN THANH TOÃN (Settled) - ÄÆ¡n vá»‹ váº­n táº£i NewWay (ThÃ¡ng 04/2025)
            // Tuyáº¿n Ninh BÃ¬nh -> HÃ  Ná»™i, 3 xe du lá»‹ch SantaFe, Tucson, Creta
            var tip1 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202504-001",
                PmtMonth = "2025-04",
                TransporterCode = "TRANS-NEWWAY",
                TransporterName = "CÃ´ng ty Cá»• pháº§n Váº­n táº£i NewWay",
                InsuranceCompanyCode = "PVI",
                InsuranceCompanyName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm Dáº§u khÃ­ Viá»‡t Nam (PVI)",
                InsuranceContractNo = "HD-BH-PVI-2025/VT09",
                TotalVehicles = 3,
                TotalTransportCost = 5_550_000,    // 3 xe * 1,850,000
                TotalDelayPenalty = 0,            // ÄÃºng háº¡n
                TotalInsuranceCost = 1_550_000,   // 650k + 500k + 400k
                TotalAmount = 7_100_000,          // TotalTransportCost + TotalInsuranceCost - TotalDelayPenalty
                VATRate = 10.0m,
                TotalBeforeVAT = 6_454_545,       // 7,100,000 / 1.1
                AmountVAT = 645_455,
                Status = TransportInsStatus.Settled,
                TCMSSignStatus = TransportSignCAStatus.Signed,
                TCMSSignUser = "DangVanHai_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Now.AddDays(-18),
                HTVSignStatus = TransportSignCAStatus.Signed,
                HTVSignUser = "NguyenThiMai_GDTaiChinh_HTV",
                HTVSignDTime = DateTime.Now.AddDays(-16),
                Appr1By = "DangVanHai_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-18),
                Appr2By = "NguyenThiMai_GDTaiChinh_HTV",
                Appr2DTime = DateTime.Now.AddDays(-16),
                SettledBy = "PhamVanThanh_KeToanThanhToan",
                SettledAt = DateTime.Now.AddDays(-14),
                BankTxnRef = "UNC-VCB-20250428-9901",
                Remark = "Quyáº¿t toÃ¡n thanh toÃ¡n cÆ°á»›c váº­n táº£i vÃ  phÃ­ báº£o hiá»ƒm hÃ ng hÃ³a xe Ä‘á»£t 1 thÃ¡ng 4 qua VCB",
                CreatedBy = "VuMinhTu_DieuVan",
                CreatedAt = DateTime.Now.AddDays(-25)
            };
            db.TransportInsPayments.Add(tip1);
            await db.SaveChangesAsync();

            db.TransportInsPaymentDetails.AddRange(
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500101",
                    CarId = "CAR-SANTAFE-0101",
                    ModelCode = "SANTAFE-CALLI",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL",
                    SpecDescription = "Báº£n cao cáº¥p 6 chá»— ngá»“i dáº«n Ä‘á»™ng HTRAC",
                    ColorName = "Äen Phantom",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "HÃ  Ná»™i",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-NW-2025-0401",
                    DlvStartDate = DateTime.Today.AddDays(-24),
                    ExpectedDays = 2,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-22),
                    DlvEndDate = DateTime.Today.AddDays(-22),
                    DelayDays = 0,
                    TFValReal = 1_850_000,
                    TPValReal = 0,
                    PriceCar = 1_300_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 650_000,
                    Val_Transport = 2_500_000,
                    Status = TransportInsDetailStatus.Settled,
                    Remark = "BÃ n giao an toÃ n táº¡i Ä‘áº¡i lÃ½ Hyundai Thanh XuÃ¢n"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500102",
                    CarId = "CAR-TUCSON-0102",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng tÄƒng Ã¡p HTRAC",
                    ColorName = "Tráº¯ng Tinh KhÃ´i",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "HÃ  Ná»™i",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-NW-2025-0401",
                    DlvStartDate = DateTime.Today.AddDays(-24),
                    ExpectedDays = 2,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-22),
                    DlvEndDate = DateTime.Today.AddDays(-22),
                    DelayDays = 0,
                    TFValReal = 1_850_000,
                    TPValReal = 0,
                    PriceCar = 1_000_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 500_000,
                    Val_Transport = 2_350_000,
                    Status = TransportInsDetailStatus.Settled,
                    Remark = "Äáº§y Ä‘á»§ biÃªn báº£n bÃ n giao kÃ¨m tem niÃªm phong"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500103",
                    CarId = "CAR-CRETA-0103",
                    ModelCode = "CRETA-PREMIUM",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng cao cáº¥p gÃ³i SmartSense",
                    ColorName = "Äá» Máº­n",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "HÃ  Ná»™i",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-NW-2025-0402",
                    DlvStartDate = DateTime.Today.AddDays(-23),
                    ExpectedDays = 2,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-21),
                    DlvEndDate = DateTime.Today.AddDays(-21),
                    DelayDays = 0,
                    TFValReal = 1_850_000,
                    TPValReal = 0,
                    PriceCar = 800_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 400_000,
                    Val_Transport = 2_250_000,
                    Status = TransportInsDetailStatus.Settled,
                    Remark = "Giao Ä‘á»§ phá»¥ kiá»‡n theo xe"
                }
            );
            await db.SaveChangesAsync();

            // 2. Báº£ng kÃª ÄÃƒ KÃ Sá» CA HOÃ€N Táº¤T (Signed) - NhÃ  xe Äáº¡t Äá»©c (ThÃ¡ng 05/2025 Ä‘á»£t 1)
            // Tuyáº¿n Ninh BÃ¬nh -> SÃ i GÃ²n, 2 xe lá»›n Palisade vÃ  Custin
            var tip2 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-001",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-DATDUC",
                TransporterName = "CÃ´ng ty TNHH Váº­n táº£i ThÆ°Æ¡ng máº¡i Äáº¡t Äá»©c",
                InsuranceCompanyCode = "BAOVIET",
                InsuranceCompanyName = "Tá»•ng CÃ´ng ty Báº£o hiá»ƒm Báº£o Viá»‡t",
                InsuranceContractNo = "HD-BH-BV-2025/SG12",
                TotalVehicles = 2,
                TotalTransportCost = 13_000_000,  // 2 xe * 6,500,000 (Báº¯c - Nam)
                TotalDelayPenalty = 0,
                TotalInsuranceCost = 1_225_000,   // 750k + 475k
                TotalAmount = 14_225_000,
                VATRate = 10.0m,
                TotalBeforeVAT = 12_931_818,      // 14,225,000 / 1.1
                AmountVAT = 1_293_182,
                Status = TransportInsStatus.Signed,
                TCMSSignStatus = TransportSignCAStatus.Signed,
                TCMSSignUser = "DangVanHai_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Now.AddDays(-5),
                HTVSignStatus = TransportSignCAStatus.Signed,
                HTVSignUser = "NguyenThiMai_GDTaiChinh_HTV",
                HTVSignDTime = DateTime.Now.AddDays(-4),
                Appr1By = "DangVanHai_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-6),
                Appr2By = "NguyenThiMai_GDTaiChinh_HTV",
                Appr2DTime = DateTime.Now.AddDays(-5),
                FilePath = "/certs/signatures/VTBH-202505-001-signed.pdf",
                Remark = "Äá»£t váº­n chuyá»ƒn Báº¯c - Nam xe phÃ¢n khÃºc cao cáº¥p Ä‘Ã£ kÃ½ sá»‘ CA hoÃ n táº¥t, chuáº©n bá»‹ phÃ¡t hÃ nh UNC",
                CreatedBy = "VuMinhTu_DieuVan",
                CreatedAt = DateTime.Now.AddDays(-10)
            };
            db.TransportInsPayments.Add(tip2);
            await db.SaveChangesAsync();

            db.TransportInsPaymentDetails.AddRange(
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500201",
                    CarId = "CAR-PALISADE-0201",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade Prestige 6S",
                    SpecCode = "PAL-3.8L-6S",
                    SpecDescription = "SUV Flagship 6 chá»— mÃ¡y dáº§u cao cáº¥p",
                    ColorName = "Xanh BÃ³ng ÄÃªm",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-SAIGON",
                    TProvinceName = "Há»“ ChÃ­ Minh",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-DD-2025-0501",
                    DlvStartDate = DateTime.Today.AddDays(-9),
                    ExpectedDays = 5,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-4),
                    DlvEndDate = DateTime.Today.AddDays(-4),
                    DelayDays = 0,
                    TFValReal = 6_500_000,
                    TPValReal = 0,
                    PriceCar = 1_500_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 750_000,
                    Val_Transport = 7_250_000,
                    Status = TransportInsDetailStatus.Approved,
                    Remark = "BÃ n giao Ä‘Ãºng tiáº¿n Ä‘á»™ cho Ä‘áº¡i lÃ½ Hyundai SÃ i GÃ²n 1S"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500202",
                    CarId = "CAR-CUSTIN-0202",
                    ModelCode = "CUSTIN-TURBO",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PRE",
                    SpecDescription = "MPV 7 chá»— gháº¿ thÆ°Æ¡ng gia tÃ­ch há»£p lÃ m mÃ¡t",
                    ColorName = "Báº¡c Ãnh Kim",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-SAIGON",
                    TProvinceName = "Há»“ ChÃ­ Minh",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-DD-2025-0501",
                    DlvStartDate = DateTime.Today.AddDays(-9),
                    ExpectedDays = 5,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-4),
                    DlvEndDate = DateTime.Today.AddDays(-4),
                    DelayDays = 0,
                    TFValReal = 6_500_000,
                    TPValReal = 0,
                    PriceCar = 950_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 475_000,
                    Val_Transport = 6_975_000,
                    Status = TransportInsDetailStatus.Approved,
                    Remark = "Xe kiá»ƒm tra ngoáº¡i tháº¥t nguyÃªn váº¹n, khÃ´ng tráº§y xÆ°á»›c"
                }
            );
            await db.SaveChangesAsync();

            // 3. Báº£ng kÃª TCMS ÄÃƒ DUYá»†T Cáº¤P 1 (TCMSApproved) - Váº­n táº£i Miá»n Báº¯c (ThÃ¡ng 05/2025 Ä‘á»£t 2)
            // Tuyáº¿n Ninh BÃ¬nh -> ÄÃ  Náºµng, 2 xe Accent vÃ  Stargazer (CÃ³ 1 xe trá»… háº¡n 1 ngÃ y bá»‹ pháº¡t)
            var tip3 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-002",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-MIENBAC",
                TransporterName = "CÃ´ng ty Váº­n táº£i & Tiáº¿p váº­n Miá»n Báº¯c",
                InsuranceCompanyCode = "MIC",
                InsuranceCompanyName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm QuÃ¢n Ä‘á»™i (MIC)",
                InsuranceContractNo = "HD-BH-MIC-2025/DN03",
                TotalVehicles = 2,
                TotalTransportCost = 7_600_000,   // 2 xe * 3,800,000 (Ninh BÃ¬nh - ÄÃ  Náºµng)
                TotalDelayPenalty = 200_000,      // Trá»… 1 ngÃ y pháº¡t 200k
                TotalInsuranceCost = 625_000,     // 325k + 300k
                TotalAmount = 8_025_000,          // 7,600,000 + 625,000 - 200,000
                VATRate = 10.0m,
                TotalBeforeVAT = 7_295_455,       // 8,025,000 / 1.1
                AmountVAT = 729_545,
                Status = TransportInsStatus.TCMSApproved,
                TCMSSignStatus = TransportSignCAStatus.Pending,
                HTVSignStatus = TransportSignCAStatus.Pending,
                Appr1By = "DangVanHai_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Now.AddDays(-2),
                Remark = "Äá»£t giao xe miá»n Trung cÃ³ sá»± cá»‘ mÆ°a lÅ© cháº­m 1 ngÃ y, TCMS Ä‘Ã£ duyá»‡t giáº£m trá»« pháº¡t cháº­m theo há»£p Ä‘á»“ng",
                CreatedBy = "VuMinhTu_DieuVan",
                CreatedAt = DateTime.Now.AddDays(-4)
            };
            db.TransportInsPayments.Add(tip3);
            await db.SaveChangesAsync();

            db.TransportInsPaymentDetails.AddRange(
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500301",
                    CarId = "CAR-ACCENT-0301",
                    ModelCode = "ACCENT-AT-DB",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan háº¡ng B tháº¿ há»‡ má»›i Ä‘á»™ng cÆ¡ Smartstream",
                    ColorName = "Äá» TÆ°Æ¡i",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-DANANG",
                    TProvinceName = "ÄÃ  Náºµng",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-MB-2025-0511",
                    DlvStartDate = DateTime.Today.AddDays(-5),
                    ExpectedDays = 3,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-2),
                    DlvEndDate = DateTime.Today.AddDays(-1), // Trá»… 1 ngÃ y
                    DelayDays = 1,
                    TFValReal = 3_800_000,
                    TPValReal = 200_000,
                    PriceCar = 650_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 325_000,
                    Val_Transport = 3_925_000,
                    Status = TransportInsDetailStatus.Approved,
                    StandardRemark = "MÆ°a lÅ© trÃªn Ä‘Ã¨o Háº£i VÃ¢n gÃ¢y Ã¹n táº¯c kÃ©o dÃ i 1 ngÃ y",
                    Remark = "Äáº¡i lÃ½ Ä‘Ã£ xÃ¡c nháº­n nháº­n xe Ä‘á»§ Ä‘iá»u kiá»‡n"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500302",
                    CarId = "CAR-STARGAZER-0302",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chá»— phong cÃ¡ch thá»ƒ thao",
                    ColorName = "Tráº¯ng Má»",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-DANANG",
                    TProvinceName = "ÄÃ  Náºµng",
                    TranspReqType = "STORAGEREARRANGE",
                    DlvMnNo = "BBVC-MB-2025-0512",
                    DlvStartDate = DateTime.Today.AddDays(-4),
                    ExpectedDays = 3,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-1),
                    DlvEndDate = DateTime.Today.AddDays(-1), // ÄÃºng háº¡n
                    DelayDays = 0,
                    TFValReal = 3_800_000,
                    TPValReal = 0,
                    PriceCar = 600_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 300_000,
                    Val_Transport = 4_100_000,
                    Status = TransportInsDetailStatus.Approved,
                    Remark = "Lá»‡nh Ä‘iá»u chuyá»ƒn kho bÃ£i phá»¥c vá»¥ trÆ°ng bÃ y triá»ƒn lÃ£m"
                }
            );
            await db.SaveChangesAsync();

            // 4. Báº£ng kÃª Má»šI Táº O NHÃP (Draft) - ÄÆ¡n vá»‹ váº­n táº£i SÃ i GÃ²n (ThÃ¡ng 05/2025 Ä‘á»£t 3)
            // Tuyáº¿n Ninh BÃ¬nh -> Cáº§n ThÆ¡, xe Ä‘iá»‡n thuáº§n IONIQ 5 vÃ  SUV Venue
            var tip4 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-003",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-SAIGON",
                TransporterName = "CÃ´ng ty Dá»‹ch vá»¥ Váº­n táº£i Ã” tÃ´ SÃ i GÃ²n",
                InsuranceCompanyCode = "PVI",
                InsuranceCompanyName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm Dáº§u khÃ­ Viá»‡t Nam (PVI)",
                InsuranceContractNo = "HD-BH-PVI-2025/CT01",
                TotalVehicles = 2,
                TotalTransportCost = 13_800_000, // Tuyáº¿n Miá»n TÃ¢y 6,900,000 * 2
                TotalDelayPenalty = 0,
                TotalInsuranceCost = 1_000_000,  // 700k + 300k
                TotalAmount = 14_800_000,
                VATRate = 10.0m,
                TotalBeforeVAT = 13_454_545,     // 14,800,000 / 1.1
                AmountVAT = 1_345_455,
                Status = TransportInsStatus.Draft,
                TCMSSignStatus = TransportSignCAStatus.Pending,
                HTVSignStatus = TransportSignCAStatus.Pending,
                Remark = "Äá»£t giao xe khu vá»±c TÃ¢y Nam Bá»™ má»›i láº­p báº£ng kÃª, chuáº©n bá»‹ gá»­i TCMS tháº©m Ä‘á»‹nh",
                CreatedBy = "VuMinhTu_DieuVan",
                CreatedAt = DateTime.Now.AddHours(-4)
            };
            db.TransportInsPayments.Add(tip4);
            await db.SaveChangesAsync();

            db.TransportInsPaymentDetails.AddRange(
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500401",
                    CarId = "CAR-IONIQ5-0401",
                    ModelCode = "IONIQ-5-PREM",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "Xe thuáº§n Ä‘iá»‡n Pin 72.6 kWh cÃ´ng nghá»‡ sáº¡c 800V siÃªu nhanh",
                    ColorName = "XÃ¡m Xi MÄƒng",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-CANTHO",
                    TProvinceName = "Cáº§n ThÆ¡",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-SG-2025-0520",
                    DlvStartDate = DateTime.Today.AddDays(-2),
                    ExpectedDays = 5,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(3),
                    DlvEndDate = DateTime.Today.AddDays(3),
                    DelayDays = 0,
                    TFValReal = 6_900_000,
                    TPValReal = 0,
                    PriceCar = 1_400_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 700_000,
                    Val_Transport = 7_600_000,
                    Status = TransportInsDetailStatus.Pending,
                    Remark = "Váº­n chuyá»ƒn báº±ng xe lá»“ng bá»c kÃ­n chuyÃªn dá»¥ng cho xe Ä‘iá»‡n"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500402",
                    CarId = "CAR-VENUE-0402",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Äáº·c Biá»‡t",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "SUV Ä‘Ã´ thá»‹ nhá» gá»n Ä‘á»™ng cÆ¡ Turbo há»™p sá»‘ ly há»£p kÃ©p 7DCT",
                    ColorName = "Äá» Máº­n",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh BÃ¬nh",
                    TStorageCode = "KHO-CANTHO",
                    TProvinceName = "Cáº§n ThÆ¡",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-SG-2025-0520",
                    DlvStartDate = DateTime.Today.AddDays(-2),
                    ExpectedDays = 5,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(3),
                    DlvEndDate = DateTime.Today.AddDays(3),
                    DelayDays = 0,
                    TFValReal = 6_900_000,
                    TPValReal = 0,
                    PriceCar = 600_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 300_000,
                    Val_Transport = 7_200_000,
                    Status = TransportInsDetailStatus.Pending,
                    Remark = "BÃ n giao kÃ¨m theo phá»¥ kiá»‡n tiÃªu chuáº©n theo xe"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Báº£ng kÃª thanh toÃ¡n chi phÃ­ lÆ°u kho xe Ã´ tÃ´ (Pmt_PaymentStorage & Pmt_PaymentStorageDetail)
        if (!await db.PaymentStorages.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Báº£ng kÃª ThÃ¡ng 04/2025 - ÄÃ£ quyáº¿t toÃ¡n chi tráº£ chuyá»ƒn khoáº£n UNC VietinBank (Settled)
            // Tá»•ng kho Ninh BÃ¬nh (KHO-NBD), 3 xe SantaFe, Tucson, Creta
            var ps1 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202504-001",
                PmtMonth = "2025-04",
                StorageOperatorCode = "KHO-NBD",
                StorageOperatorName = "Tá»•ng kho PhÃ¢n phá»‘i Ã” tÃ´ Hyundai Ninh BÃ¬nh",
                TotalVehicles = 3,
                TotalCoatCost = 150_000,      // 3 xe * 50k báº¡t che phá»§
                TotalStorageCost = 2_250_000,  // (30 + 30 + 30 ngÃ y) * 25k = 90 * 25k = 2,250,000
                TotalAmount = 2_400_000,       // TrÆ°á»›c VAT
                VATRate = 10.0m,
                UnitPriceVAT = 240_000,        // 10% VAT
                AmountTotal = 2_640_000,       // Sau VAT
                Status = PaymentStorageStatus.Settled,
                TCMSSignStatus = StorageSignCAStatus.Signed,
                TCMSSignUser = "TranQuocTuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-20),
                HTVSignStatus = StorageSignCAStatus.Signed,
                HTVSignUser = "NguyenVanHieu_TongGiamDoc_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-19),
                Appr1By = "LeHongPhong_TruongPhongKD_HTV",
                Appr1DTime = DateTime.Today.AddDays(-22),
                Appr2By = "TranQuocTuan_GDKyThuat_TCMS",
                Appr2DTime = DateTime.Today.AddDays(-21),
                SettledBy = "BuiThiThanh_KeToanTruong",
                SettledAt = DateTime.Today.AddDays(-18),
                BankTxnRef = "UNC-CTG-LK-20250422-8812",
                FilePath = "/documents/storage/LK-202504-001-Signed.pdf",
                Remark = "Thanh toÃ¡n dá»©t Ä‘iá»ƒm chi phÃ­ lÆ°u giá»¯ kho bÃ£i vÃ  báº¡t phá»§ xe thÃ¡ng 04/2025",
                CreatedBy = "PhamVanDuc_QuanLyKho",
                CreatedAt = DateTime.Today.AddDays(-25)
            };
            db.PaymentStorages.Add(ps1);
            await db.SaveChangesAsync();

            db.PaymentStorageDetails.AddRange(
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps1.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600101",
                    CarId = "CAR-SAN-6001",
                    ModelCode = "SANTAFE-CALLI",
                    ModelName = "Hyundai SantaFe 2.5 Turbo Calligraphy",
                    SpecCode = "SAN-2.5T-CAL",
                    SpecDescription = "SUV 7 chá»— mÃ¡y xÄƒng Turbo dáº«n Ä‘á»™ng 4 bÃ¡nh HTRAC",
                    ColorExtNameVN = "Tráº¯ng Tinh KhÃ´i",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-60),
                    ApprovedDate2 = DateTime.Today.AddDays(-28),
                    DeliveryOutDate = DateTime.Today.AddDays(-15),
                    DealerCode = "DLR-HYUNDAI-ANPHU",
                    DealerName = "Hyundai An PhÃº SÃ i GÃ²n",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000, // 30 * 25k
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Báº¡t bá»c kÃ­n chá»‘ng tia cá»±c tÃ­m tiÃªu chuáº©n HTC"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps1.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600102",
                    CarId = "CAR-TUC-6002",
                    ModelCode = "TUCSON-1.6T",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Äáº·c Biá»‡t",
                    SpecCode = "TUC-1.6T-DB",
                    SpecDescription = "Crossover 5 chá»— mÃ¡y Turbo há»™p sá»‘ ly há»£p kÃ©p 7 cáº¥p",
                    ColorExtNameVN = "Äen Sang Trá»ng",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-55),
                    ApprovedDate2 = DateTime.Today.AddDays(-26),
                    DeliveryOutDate = DateTime.Today.AddDays(-16),
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    DealerName = "Hyundai Thanh XuÃ¢n HÃ  Ná»™i",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000,
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "LÆ°u kho khu vá»±c cÃ³ mÃ¡i che phá»¥"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps1.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600103",
                    CarId = "CAR-CRT-6003",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5L Cao Cáº¥p",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "SUV Ä‘Ã´ thá»‹ nhá» gá»n gÃ³i cÃ´ng nghá»‡ an toÃ n SmartSense",
                    ColorExtNameVN = "Äá» Máº­n",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-50),
                    ApprovedDate2 = DateTime.Today.AddDays(-25),
                    DeliveryOutDate = DateTime.Today.AddDays(-18),
                    DealerCode = "DLR-HYUNDAI-DANANG",
                    DealerName = "Hyundai Cáº©m Lá»‡ ÄÃ  Náºµng",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000,
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Kiá»ƒm tra báº£o dÆ°á»¡ng lá»‘p Ä‘á»‹nh ká»³ trong thá»i gian lÆ°u bÃ£i"
                }
            );
            await db.SaveChangesAsync();

            // 2. Báº£ng kÃª ThÃ¡ng 05/2025 Äá»£t 1 - ÄÃ£ kÃ½ sá»‘ CA 2 cáº¥p hoÃ n táº¥t (Signed)
            // Tá»•ng kho ÄÃ´ng Anh (KHO-DA), 3 xe Palisade, Custin, Stargazer X
            var ps2 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-001",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-DA",
                StorageOperatorName = "Tá»•ng kho Ã” tÃ´ Hyundai Miá»n Báº¯c - ÄÃ´ng Anh",
                TotalVehicles = 3,
                TotalCoatCost = 150_000,       // 3 * 50k
                TotalStorageCost = 1_500_000,   // (20 + 20 + 20 ngÃ y) * 25k = 60 * 25k = 1,500,000
                TotalAmount = 1_650_000,
                VATRate = 10.0m,
                UnitPriceVAT = 165_000,
                AmountTotal = 1_815_000,
                Status = PaymentStorageStatus.Signed,
                TCMSSignStatus = StorageSignCAStatus.Signed,
                TCMSSignUser = "TranQuocTuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-4),
                HTVSignStatus = StorageSignCAStatus.Signed,
                HTVSignUser = "NguyenVanHieu_TongGiamDoc_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-3),
                Appr1By = "LeHongPhong_TruongPhongKD_HTV",
                Appr1DTime = DateTime.Today.AddDays(-6),
                Appr2By = "TranQuocTuan_GDKyThuat_TCMS",
                Appr2DTime = DateTime.Today.AddDays(-5),
                FilePath = "/documents/storage/LK-202505-001-Signed.pdf",
                Remark = "Báº£ng kÃª Ä‘á»£t 1 thÃ¡ng 05 xe lÆ°u giá»¯ kho bÃ£i ÄÃ´ng Anh chuáº©n bá»‹ giao Ä‘áº¡i lÃ½ phÃ­a Báº¯c",
                CreatedBy = "VuDinhTruong_ThuKhoDA",
                CreatedAt = DateTime.Today.AddDays(-7)
            };
            db.PaymentStorages.Add(ps2);
            await db.SaveChangesAsync();

            db.PaymentStorageDetails.AddRange(
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps2.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600201",
                    CarId = "CAR-PAL-6004",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PLS-2.2D-PRE",
                    SpecDescription = "SUV Flagship 7 chá»— Ä‘á»™ng cÆ¡ Diesel dáº«n Ä‘á»™ng HTRAC",
                    ColorExtNameVN = "Xanh BÃ³ng ÄÃªm",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-35),
                    ApprovedDate2 = DateTime.Today.AddDays(-10),
                    DeliveryOutDate = DateTime.Today.AddDays(-3),
                    DealerCode = "DLR-HYUNDAI-LEVANLUONG",
                    DealerName = "Hyundai LÃª VÄƒn LÆ°Æ¡ng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000, // 20 * 25k
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Xe bá»c báº¡t phá»§ chuyÃªn dá»¥ng báº£o vá»‡ lá»›p sÆ¡n bÃ³ng"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps2.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600202",
                    CarId = "CAR-CUS-6005",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CST-2.0T-PRE",
                    SpecDescription = "MPV cá»¡ trung cao cáº¥p cá»­a trÆ°á»£t Ä‘iá»‡n thÃ´ng minh",
                    ColorExtNameVN = "Tráº¯ng Tuyáº¿t",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-32),
                    ApprovedDate2 = DateTime.Today.AddDays(-8),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    DealerName = "Hyundai Pháº¡m VÄƒn Äá»“ng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000,
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Kiá»ƒm tra áº¯c quy vÃ  Ã¡p suáº¥t lá»‘p trÆ°á»›c khi xuáº¥t"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps2.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600203",
                    CarId = "CAR-SGZ-6006",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chá»— phong cÃ¡ch thá»ƒ thao gáº§m cao",
                    ColorExtNameVN = "Báº¡c Ãnh Kim",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-30),
                    ApprovedDate2 = DateTime.Today.AddDays(-9),
                    DeliveryOutDate = DateTime.Today.AddDays(-1),
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    DealerName = "Hyundai HÃ  ÄÃ´ng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000,
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "ÄÃ£ rá»­a sáº¡ch bá»¥i báº©n trÆ°á»›c bÃ n giao nhÃ  xe váº­n chuyá»ƒn"
                }
            );
            await db.SaveChangesAsync();

            // 3. Báº£ng kÃª ThÃ¡ng 05/2025 Äá»£t 2 - TCMS Tháº©m Ä‘á»‹nh duyá»‡t cáº¥p 2 (TCMSApproved)
            // Äang chá» lÃ£nh Ä‘áº¡o kÃ½ sá»‘ CA Ä‘iá»‡n tá»­. Tá»•ng kho ÄÃ  Náºµng (KHO-DN), 2 xe Accent, Venue
            var ps3 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-002",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-DN",
                StorageOperatorName = "Tá»•ng kho Trung chuyá»ƒn Ã” tÃ´ Hyundai Miá»n Trung - ÄÃ  Náºµng",
                TotalVehicles = 2,
                TotalCoatCost = 100_000,       // 2 * 50k
                TotalStorageCost = 600_000,     // (12 + 12 ngÃ y) * 25k = 24 * 25k = 600,000
                TotalAmount = 700_000,
                VATRate = 10.0m,
                UnitPriceVAT = 70_000,
                AmountTotal = 770_000,
                Status = PaymentStorageStatus.TCMSApproved,
                TCMSSignStatus = StorageSignCAStatus.Pending,
                HTVSignStatus = StorageSignCAStatus.Pending,
                Appr1By = "LeHongPhong_TruongPhongKD_HTV",
                Appr1DTime = DateTime.Today.AddDays(-3),
                Appr2By = "TranQuocTuan_GDKyThuat_TCMS",
                Appr2DTime = DateTime.Today.AddDays(-2),
                Remark = "Äá»£t 2 thÃ¡ng 05/2025 kho ÄÃ  Náºµng phÃ¢n bá»• cho cÃ¡c Ä‘áº¡i lÃ½ khu vá»±c Miá»n Trung",
                CreatedBy = "NguyenThiMai_ThuKhoDN",
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.PaymentStorages.Add(ps3);
            await db.SaveChangesAsync();

            db.PaymentStorageDetails.AddRange(
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps3.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600301",
                    CarId = "CAR-ACC-6007",
                    ModelCode = "ACCENT-AT-DB",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan phÃ¢n khÃºc B tháº¿ há»‡ má»›i Ä‘á»™ng cÆ¡ Smartstream",
                    ColorExtNameVN = "Tráº¯ng",
                    StorageCodeInit = "KHO-DN",
                    StorageDate = DateTime.Today.AddDays(-20),
                    ApprovedDate2 = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = null, // ChÆ°a xuáº¥t kho
                    DealerCode = "DLR-HYUNDAI-SONTRA",
                    DealerName = "Hyundai SÆ¡n TrÃ  ÄÃ  Náºµng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 12),
                    CostStorageMonth = 12,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 300_000, // 12 * 25k
                    TotalAmount = 350_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Báº¡t phá»§ che náº¯ng vÃ¹ng miá»n Trung"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps3.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600302",
                    CarId = "CAR-VNU-6008",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Äáº·c Biá»‡t",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "A-SUV phong cÃ¡ch tráº» trung thá»ƒ thao hiá»‡n Ä‘áº¡i",
                    ColorExtNameVN = "Äá» Máº­n",
                    StorageCodeInit = "KHO-DN",
                    StorageDate = DateTime.Today.AddDays(-18),
                    ApprovedDate2 = DateTime.Today.AddDays(-4),
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-QUANGNAM",
                    DealerName = "Hyundai Quáº£ng Nam",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 12),
                    CostStorageMonth = 12,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 300_000,
                    TotalAmount = 350_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "LÆ°u kho an toÃ n cÃ³ camera giÃ¡m sÃ¡t 24/7"
                }
            );
            await db.SaveChangesAsync();

            // 4. Báº£ng kÃª ThÃ¡ng 05/2025 Äá»£t 3 - Má»šI Táº O NHÃP (Draft)
            // Tá»•ng kho BÃ¬nh DÆ°Æ¡ng (KHO-BD), 2 xe Ä‘iá»‡n thuáº§n IONIQ 5 vÃ  SUV Tucson
            var ps4 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-003",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-BD",
                StorageOperatorName = "Tá»•ng kho LÆ°u chuyá»ƒn Ã” tÃ´ Hyundai Miá»n Nam - BÃ¬nh DÆ°Æ¡ng",
                TotalVehicles = 2,
                TotalCoatCost = 100_000,       // 2 * 50k
                TotalStorageCost = 450_000,     // (9 + 9 ngÃ y) * 25k = 18 * 25k = 450,000
                TotalAmount = 550_000,
                VATRate = 10.0m,
                UnitPriceVAT = 55_000,
                AmountTotal = 605_000,
                Status = PaymentStorageStatus.Draft,
                TCMSSignStatus = StorageSignCAStatus.Pending,
                HTVSignStatus = StorageSignCAStatus.Pending,
                Remark = "Äá»£t 3 thÃ¡ng 05/2025 kho BÃ¬nh DÆ°Æ¡ng Ä‘ang kiá»ƒm Ä‘áº¿m báº¡t che phá»§ vÃ  háº¡n xuáº¥t kho",
                CreatedBy = "VoVanNam_QuanLyKhoBD",
                CreatedAt = DateTime.Now.AddHours(-3)
            };
            db.PaymentStorages.Add(ps4);
            await db.SaveChangesAsync();

            db.PaymentStorageDetails.AddRange(
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps4.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600401",
                    CarId = "CAR-IQ5-6009",
                    ModelCode = "IONIQ-5-PREM",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "Xe thuáº§n Ä‘iá»‡n Pin 72.6 kWh sáº¡c siÃªu nhanh 800V",
                    ColorExtNameVN = "XÃ¡m Xi MÄƒng",
                    StorageCodeInit = "KHO-BD",
                    StorageDate = DateTime.Today.AddDays(-10),
                    ApprovedDate2 = null, // ChÆ°a duyá»‡t LXX
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-BINHDUONG",
                    DealerName = "Hyundai BÃ¬nh DÆ°Æ¡ng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 9),
                    CostStorageMonth = 9,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 225_000, // 9 * 25k
                    TotalAmount = 275_000,
                    Status = PaymentStorageDetailStatus.Pending,
                    Remark = "Khoang lÆ°u giá»¯ xe Ä‘iá»‡n cÃ¡ch ly nhiá»‡t Ä‘á»™ vÃ  kiá»ƒm tra Ä‘iá»‡n Ã¡p pin"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps4.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600402",
                    CarId = "CAR-TUC-6010",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Äáº·c Biá»‡t",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover 5 chá»— mÃ¡y dáº§u tiáº¿t kiá»‡m nhiÃªn liá»‡u há»™p sá»‘ 8 cáº¥p",
                    ColorExtNameVN = "NÃ¢u VÃ ng Ãnh Kim",
                    StorageCodeInit = "KHO-BD",
                    StorageDate = DateTime.Today.AddDays(-12),
                    ApprovedDate2 = null,
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-MIENTAY",
                    DealerName = "Hyundai Miá»n TÃ¢y SÃ i GÃ²n",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 9),
                    CostStorageMonth = 9,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 225_000,
                    TotalAmount = 275_000,
                    Status = PaymentStorageDetailStatus.Pending,
                    Remark = "Xe má»›i nháº­p kho Ä‘ang láº¯p báº¡t phá»§ chá»‘ng bá»¥i"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 12. Seed Dá»¯ liá»‡u máº«u CÃ´ng vÄƒn Äá» nghá»‹ Gia háº¡n ThÆ° Báº£o lÃ£nh NgÃ¢n hÃ ng (Pmt_GrtClaimExt) =====
        if (!await db.GuaranteeExtensionDispatches.AnyAsync(d => d.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. CÃ´ng vÄƒn ÄÃƒ ÄÆ¯á»¢C NGÃ‚N HÃ€NG CHáº¤P THUáº¬N GIA Háº N (BankAccepted) - VPBank & Hyundai HÃ  ÄÃ´ng
            var ged1 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202504-001",
                DealerCode = "DLR-HYUNDAI-HADONG",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai HÃ  ÄÃ´ng",
                BankCode = "VPB",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                BankCodeMonitor = "VPBANK_HO",
                FlagIsHTC = "1",
                NumberOfDaysExt = 30,
                TotalCarCount = 3,
                TotalAmount = 3_150_000_000,
                TotalCarsNotDelivered = 1,
                TotalCarsDelivered = 2,
                Status = GuaranteeExtensionStatus.BankAccepted,
                SignCAStatus = ExtensionSignCAStatus.Signed,
                SignedBy = "NguyenVanHieu_TongGiamDoc_HTV",
                SignedAt = DateTime.Today.AddDays(-22),
                CertThumbprint = "E4F890A1B2C3D4E5",
                BankResponseRef = "VPB-CV-2025/0428-BL",
                BankAcceptedAt = DateTime.Today.AddDays(-20),
                FilePath = "/documents/extension/CVGH-202504-001-Signed.pdf",
                Remark = "Äá» nghá»‹ gia háº¡n 30 ngÃ y do thá»§ tá»¥c Ä‘Äƒng kÃ½ xe theo há»£p Ä‘á»“ng dá»± Ã¡n cá»§a Ä‘áº¡i lÃ½ bá»‹ cháº­m",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-25)
            };
            db.GuaranteeExtensionDispatches.Add(ged1);
            await db.SaveChangesAsync();

            db.GuaranteeExtensionDetails.AddRange(
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-7001",
                    VIN = "KMHCT81EPHU700101",
                    ModelCode = "SANTAFE-CALLI",
                    ModelName = "Hyundai SantaFe 2.5 Turbo Calligraphy",
                    SpecCode = "SAN-2.5T-CAL",
                    SpecDescription = "SUV 7 chá»— mÃ¡y xÄƒng Turbo dáº«n Ä‘á»™ng HTRAC",
                    ColorName = "Äen Sang Trá»ng",
                    SOCode = "SO-2025-0401",
                    DlrCtrNo = "PLHD-HADONG-2025/04",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VPB-2025-0091",
                    GrtDateStart = DateTime.Today.AddDays(-60),
                    GrtDateExpired = DateTime.Today.AddDays(-15),
                    ExtendedDate = DateTime.Today.AddDays(15),
                    GrtValue = 1_250_000_000,
                    UnitPrice = 1_470_000_000,
                    IsDelivered = true,
                    DeliveryDate = DateTime.Today.AddDays(-25),
                    Status = GuaranteeExtensionDetailStatus.BankAccepted,
                    Remark = "ÄÃ£ giao xe cho Ä‘áº¡i lÃ½, Ä‘ang hoÃ n thiá»‡n Ä‘Äƒng kÃ½ lÆ°u hÃ nh"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-7002",
                    VIN = "KMHCT81EPHU700102",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Crossover 5 chá»— Turbo HTRAC",
                    ColorName = "Tráº¯ng Tinh KhÃ´i",
                    SOCode = "SO-2025-0401",
                    DlrCtrNo = "PLHD-HADONG-2025/04",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VPB-2025-0091",
                    GrtDateStart = DateTime.Today.AddDays(-60),
                    GrtDateExpired = DateTime.Today.AddDays(-15),
                    ExtendedDate = DateTime.Today.AddDays(15),
                    GrtValue = 950_000_000,
                    UnitPrice = 1_120_000_000,
                    IsDelivered = true,
                    DeliveryDate = DateTime.Today.AddDays(-25),
                    Status = GuaranteeExtensionDetailStatus.BankAccepted,
                    Remark = "ÄÃ£ giao xe trÆ°ng bÃ y showroom"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRT-7003",
                    VIN = "KMHCT81EPHU700103",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5L Cao Cáº¥p",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "SUV Ä‘Ã´ thá»‹ gÃ³i an toÃ n SmartSense",
                    ColorName = "Äá» Máº­n",
                    SOCode = "SO-2025-0401",
                    DlrCtrNo = "PLHD-HADONG-2025/04",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VPB-2025-0091",
                    GrtDateStart = DateTime.Today.AddDays(-60),
                    GrtDateExpired = DateTime.Today.AddDays(-15),
                    ExtendedDate = DateTime.Today.AddDays(15),
                    GrtValue = 950_000_000,
                    UnitPrice = 1_120_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.BankAccepted,
                    Remark = "Chá» bÃ n giao chá»©ng tá»« gá»‘c xe táº¡i ngÃ¢n hÃ ng"
                }
            );
            await db.SaveChangesAsync();

            // 2. CÃ´ng vÄƒn ÄÃƒ KÃ Sá» CA PHÃT HÃ€NH (Signed) - Äang gá»­i VietinBank chá» phÃºc Ä‘Ã¡p
            var ged2 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-001",
                DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                DealerName = "CÃ´ng ty CP Hyundai Pháº¡m VÄƒn Äá»“ng",
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                BankCodeMonitor = "CTG_HANOI",
                FlagIsHTC = "2", // HTV
                NumberOfDaysExt = 30,
                TotalCarCount = 3,
                TotalAmount = 3_600_000_000,
                TotalCarsNotDelivered = 2,
                TotalCarsDelivered = 1,
                Status = GuaranteeExtensionStatus.Signed,
                SignCAStatus = ExtensionSignCAStatus.Signed,
                SignedBy = "NguyenThiMai_GDTaiChinh_HTV",
                SignedAt = DateTime.Today.AddDays(-5),
                CertThumbprint = "C8A234B5D6E7F890",
                FilePath = "/documents/extension/CVGH-202505-001-Signed.pdf",
                Remark = "Äá» nghá»‹ VietinBank gia háº¡n báº£o lÃ£nh theo gÃ³i tÃ­n dá»¥ng liÃªn káº¿t phÃ¢n phá»‘i Ã´ tÃ´ HTV",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-8)
            };
            db.GuaranteeExtensionDispatches.Add(ged2);
            await db.SaveChangesAsync();

            db.GuaranteeExtensionDetails.AddRange(
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-PLS-7004",
                    VIN = "KMHCT81EPHU700201",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PLS-2.2D-PRE",
                    SpecDescription = "SUV Flagship 7 chá»— Diesel HTRAC",
                    ColorName = "Xanh BÃ³ng ÄÃªm",
                    SOCode = "SO-2025-0501",
                    DlrCtrNo = "PLHD-PVD-2025/05",
                    GuaranteeNo = "GRT-2025-0501",
                    BankGuaranteeNo = "BL-CTG-2025-0812",
                    GrtDateStart = DateTime.Today.AddDays(-45),
                    GrtDateExpired = DateTime.Today.AddDays(2),
                    ExtendedDate = DateTime.Today.AddDays(32),
                    GrtValue = 1_400_000_000,
                    UnitPrice = 1_650_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.Active,
                    Remark = "Xe Ä‘ang lÆ°u kho ÄÃ´ng Anh chá» xuáº¥t trÃ¬nh há»“ sÆ¡"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CUS-7005",
                    VIN = "KMHCT81EPHU700202",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CST-2.0T-PRE",
                    SpecDescription = "MPV cao cáº¥p cá»­a lÃ¹a Ä‘iá»‡n thÃ´ng minh",
                    ColorName = "Tráº¯ng Tuyáº¿t",
                    SOCode = "SO-2025-0501",
                    DlrCtrNo = "PLHD-PVD-2025/05",
                    GuaranteeNo = "GRT-2025-0501",
                    BankGuaranteeNo = "BL-CTG-2025-0812",
                    GrtDateStart = DateTime.Today.AddDays(-45),
                    GrtDateExpired = DateTime.Today.AddDays(2),
                    ExtendedDate = DateTime.Today.AddDays(32),
                    GrtValue = 1_100_000_000,
                    UnitPrice = 1_300_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.Active,
                    Remark = "Chá» Ä‘áº¡i lÃ½ táº¥t toÃ¡n Ä‘á»£t thanh toÃ¡n trÆ°á»›c"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-7006",
                    VIN = "KMHCT81EPHU700203",
                    ModelCode = "ACCENT-AT-DB",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan phÃ¢n khÃºc B Smartstream",
                    ColorName = "Báº¡c Ãnh Kim",
                    SOCode = "SO-2025-0501",
                    DlrCtrNo = "PLHD-PVD-2025/05",
                    GuaranteeNo = "GRT-2025-0501",
                    BankGuaranteeNo = "BL-CTG-2025-0812",
                    GrtDateStart = DateTime.Today.AddDays(-45),
                    GrtDateExpired = DateTime.Today.AddDays(2),
                    ExtendedDate = DateTime.Today.AddDays(32),
                    GrtValue = 1_100_000_000,
                    UnitPrice = 1_300_000_000,
                    IsDelivered = true,
                    DeliveryDate = DateTime.Today.AddDays(-10),
                    Status = GuaranteeExtensionDetailStatus.Active,
                    Remark = "ÄÃ£ giao xe vá» Ä‘áº¡i lÃ½"
                }
            );
            await db.SaveChangesAsync();

            // 3. CÃ´ng vÄƒn Má»šI Táº O Dá»° THáº¢O (Draft) - Techcombank & Hyundai An PhÃº SÃ i GÃ²n
            var ged3 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-002",
                DealerCode = "DLR-HYUNDAI-ANPHU",
                DealerName = "Hyundai An PhÃº SÃ i GÃ²n",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                BankCodeMonitor = "TCB_HO",
                FlagIsHTC = "1",
                NumberOfDaysExt = 15,
                TotalCarCount = 2,
                TotalAmount = 2_100_000_000,
                TotalCarsNotDelivered = 2,
                TotalCarsDelivered = 0,
                Status = GuaranteeExtensionStatus.Draft,
                SignCAStatus = ExtensionSignCAStatus.Pending,
                Remark = "Dá»± tháº£o cÃ´ng vÄƒn xin gia háº¡n báº£o lÃ£nh xe Ä‘iá»‡n IONIQ 5 vÃ  SUV Venue",
                CreatedBy = "LeHongPhong_TinDung",
                CreatedAt = DateTime.Now.AddHours(-6)
            };
            db.GuaranteeExtensionDispatches.Add(ged3);
            await db.SaveChangesAsync();

            db.GuaranteeExtensionDetails.AddRange(
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-IQ5-7007",
                    VIN = "KMHCT81EPHU700301",
                    ModelCode = "IONIQ-5-PREM",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "Xe thuáº§n Ä‘iá»‡n Pin 72.6 kWh sáº¡c 800V siÃªu nhanh",
                    ColorName = "XÃ¡m Xi MÄƒng",
                    SOCode = "SO-2025-0505",
                    DlrCtrNo = "PLHD-ANPHU-2025/05",
                    GuaranteeNo = "GRT-2025-0505",
                    BankGuaranteeNo = "BL-TCB-2025-0331",
                    GrtDateStart = DateTime.Today.AddDays(-25),
                    GrtDateExpired = DateTime.Today.AddDays(5),
                    ExtendedDate = DateTime.Today.AddDays(20),
                    GrtValue = 1_400_000_000,
                    UnitPrice = 1_650_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.Pending,
                    Remark = "Xe Ä‘ang chá» váº­n chuyá»ƒn vÃ o kho BÃ¬nh DÆ°Æ¡ng"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-VNU-7008",
                    VIN = "KMHCT81EPHU700302",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Äáº·c Biá»‡t",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "SUV Ä‘Ã´ thá»‹ nhá» gá»n 1.0 Turbo 7DCT",
                    ColorName = "Äá» Máº­n",
                    SOCode = "SO-2025-0505",
                    DlrCtrNo = "PLHD-ANPHU-2025/05",
                    GuaranteeNo = "GRT-2025-0505",
                    BankGuaranteeNo = "BL-TCB-2025-0331",
                    GrtDateStart = DateTime.Today.AddDays(-25),
                    GrtDateExpired = DateTime.Today.AddDays(5),
                    ExtendedDate = DateTime.Today.AddDays(20),
                    GrtValue = 700_000_000,
                    UnitPrice = 820_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.Pending,
                    Remark = "Chá» hoÃ n thiá»‡n thá»§ tá»¥c kÃ½ duyá»‡t cÃ´ng vÄƒn"
                }
            );
            await db.SaveChangesAsync();

            // 4. CÃ´ng vÄƒn NGÃ‚N HÃ€NG Tá»ª CHá»I GIA Háº N (BankRejected) - MBBank & Hyundai Cáº©m Lá»‡ ÄÃ  Náºµng
            var ged4 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-003",
                DealerCode = "DLR-HYUNDAI-DANANG",
                DealerName = "Hyundai Cáº©m Lá»‡ ÄÃ  Náºµng",
                BankCode = "MBB",
                BankName = "NgÃ¢n hÃ ng TMCP QuÃ¢n Äá»™i (MBBank)",
                BankCodeMonitor = "MBB_DANANG",
                FlagIsHTC = "1",
                NumberOfDaysExt = 45,
                TotalCarCount = 2,
                TotalAmount = 1_850_000_000,
                TotalCarsNotDelivered = 2,
                TotalCarsDelivered = 0,
                Status = GuaranteeExtensionStatus.BankRejected,
                SignCAStatus = ExtensionSignCAStatus.Signed,
                SignedBy = "NguyenVanHieu_TongGiamDoc_HTV",
                SignedAt = DateTime.Today.AddDays(-4),
                CertThumbprint = "D1E2F3A4B5C67890",
                BankRejectReason = "Äáº¡i lÃ½ Ä‘Ã£ vÆ°á»£t háº¡n má»©c cáº¥p tÃ­n dá»¥ng xoay vÃ²ng quÃ½ 2/2025 theo Há»£p Ä‘á»“ng háº¡n má»©c sá»‘ MB-HDTD-2024/099",
                FilePath = "/documents/extension/CVGH-202505-003-Signed.pdf",
                Remark = "Äá» nghá»‹ gia háº¡n 45 ngÃ y nhÆ°ng bá»‹ ngÃ¢n hÃ ng tá»« chá»‘i do cháº¡m tráº§n háº¡n má»©c tÃ­n dá»¥ng",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-6)
            };
            db.GuaranteeExtensionDispatches.Add(ged4);
            await db.SaveChangesAsync();

            db.GuaranteeExtensionDetails.AddRange(
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SGZ-7009",
                    VIN = "KMHCT81EPHU700401",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chá»— gáº§m cao",
                    ColorName = "Tráº¯ng Má»",
                    SOCode = "SO-2025-0508",
                    DlrCtrNo = "PLHD-DANANG-2025/05",
                    GuaranteeNo = "GRT-2025-0508",
                    BankGuaranteeNo = "BL-MBB-2025-0442",
                    GrtDateStart = DateTime.Today.AddDays(-40),
                    GrtDateExpired = DateTime.Today.AddDays(-1),
                    ExtendedDate = DateTime.Today.AddDays(44),
                    GrtValue = 750_000_000,
                    UnitPrice = 880_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.BankRejected,
                    Remark = "NgÃ¢n hÃ ng yÃªu cáº§u Ä‘áº¡i lÃ½ thanh toÃ¡n dá»©t Ä‘iá»ƒm hoáº·c bá»• sung tÃ i sáº£n báº£o Ä‘áº£m"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-7010",
                    VIN = "KMHCT81EPHU700402",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Äáº·c Biá»‡t",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover 5 chá»— mÃ¡y dáº§u tiáº¿t kiá»‡m nhiÃªn liá»‡u",
                    ColorName = "NÃ¢u VÃ ng Ãnh Kim",
                    SOCode = "SO-2025-0508",
                    DlrCtrNo = "PLHD-DANANG-2025/05",
                    GuaranteeNo = "GRT-2025-0508",
                    BankGuaranteeNo = "BL-MBB-2025-0442",
                    GrtDateStart = DateTime.Today.AddDays(-40),
                    GrtDateExpired = DateTime.Today.AddDays(-1),
                    ExtendedDate = DateTime.Today.AddDays(44),
                    GrtValue = 1_100_000_000,
                    UnitPrice = 1_300_000_000,
                    IsDelivered = false,
                    DeliveryDate = null,
                    Status = GuaranteeExtensionDetailStatus.BankRejected,
                    Remark = "Chuyá»ƒn phÆ°Æ¡ng Ã¡n yÃªu cáº§u thanh toÃ¡n chuyá»ƒn khoáº£n UNC trá»±c tiáº¿p"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 13. Seed Dá»¯ liá»‡u máº«u CÃ´ng vÄƒn ÄÃ²i Tiá»n Báº£o LÃ£nh NgÃ¢n HÃ ng (Pmt_GrtClaim / FrmMngGrtClaim) =====
        if (!await db.GuaranteeClaims.AnyAsync(c => c.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. CÃ´ng vÄƒn ÄÃƒ THU Há»’I Bá»’I HOÃ€N THÃ€NH CÃ”NG (Settled) - Vietcombank & Hyundai HÃ  ÄÃ´ng
            var gcd1 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-001",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai HÃ  ÄÃ´ng",
                BankCode = "VCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ngoáº¡i ThÆ°Æ¡ng Viá»‡t Nam (Vietcombank)",
                BankCodeMonitor = "VCB_HO",
                FlagIsHTC = "1",
                TotalCarCount = 3,
                TotalClaimAmount = 2_150_000_000,
                SettledAmount = 2_150_000_000,
                Status = GuaranteeClaimStatus.Settled,
                SignCAStatus = ClaimSignCAStatus.Signed,
                SignedBy = "LeNgocAnh_TongGiamDoc_HTC",
                SignedAt = DateTime.Today.AddDays(-12),
                CertThumbprint = "VCB789CA01",
                SentToBankAt = DateTime.Today.AddDays(-11),
                BankRefNo = "VCB-CLAIM-REC-20250503",
                SettledAt = DateTime.Today.AddDays(-8),
                SettledBy = "TranMinhThu_KTT_HTC",
                BankTxnRef = "FT2025050899881",
                FilePath = "/documents/claims/CVDBL-202505-001-Signed.pdf",
                Remark = "Äáº¡i lÃ½ quÃ¡ háº¡n thanh toÃ¡n tiá»n xe 26 ngÃ y theo Há»£p Ä‘á»“ng sá»‘ HD-2025-0101. Vietcombank Ä‘Ã£ trÃ­ch ná»£ bá»“i hoÃ n thÃ nh cÃ´ng 100% nghÄ©a vá»¥ báº£o lÃ£nh.",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-14)
            };
            db.GuaranteeClaims.Add(gcd1);
            await db.SaveChangesAsync();

            db.GuaranteeClaimDetails.AddRange(
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-8001",
                    VIN = "KMHCT81EPHU800101",
                    ModelCode = "SANTAFE-2.2D",
                    ModelName = "Hyundai Santa Fe 2.2 Diesel Cao Cáº¥p",
                    SpecCode = "SAN-2.2D-PRE",
                    SpecDescription = "SUV 7 chá»— mÃ¡y dáº§u tháº¿ há»‡ má»›i",
                    ColorName = "Äen Ãnh Kim",
                    SOCode = "SO-2025-0401",
                    ContractNo = "PLHD-HADONG-2025/01",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VCB-2025-0101",
                    DateOpen = DateTime.Today.AddDays(-60),
                    DateExpired = DateTime.Today.AddDays(-26),
                    OverdueDays = 26,
                    UnitPriceActual = 1_150_000_000,
                    GrtValue = 950_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Settled,
                    Remark = "Vietcombank Ä‘Ã£ trÃ­ch ná»£ tÃ i khoáº£n phong tá»a chi tráº£ bá»“i hoÃ n cho HTC"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-8002",
                    VIN = "KMHCT81EPHU800102",
                    ModelCode = "TUCSON-1.6T",
                    ModelName = "Hyundai Tucson 1.6 Turbo HTRAC",
                    SpecCode = "TUC-1.6T-DB",
                    SpecDescription = "Crossover 5 chá»— tÄƒng Ã¡p",
                    ColorName = "Tráº¯ng Tinh KhÃ´i",
                    SOCode = "SO-2025-0401",
                    ContractNo = "PLHD-HADONG-2025/01",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VCB-2025-0101",
                    DateOpen = DateTime.Today.AddDays(-60),
                    DateExpired = DateTime.Today.AddDays(-26),
                    OverdueDays = 26,
                    UnitPriceActual = 890_000_000,
                    GrtValue = 700_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Settled,
                    Remark = "Vietcombank Ä‘Ã£ trÃ­ch ná»£ tÃ i khoáº£n phong tá»a chi tráº£ bá»“i hoÃ n cho HTC"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8003",
                    VIN = "KMHCT81EPHU800103",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5L-PRE",
                    SpecDescription = "B-SUV Ä‘Ã´ thá»‹ Ä‘a dá»¥ng",
                    ColorName = "Äá» Máº­n",
                    SOCode = "SO-2025-0401",
                    ContractNo = "PLHD-HADONG-2025/01",
                    GuaranteeNo = "GRT-2025-0401",
                    BankGuaranteeNo = "BL-VCB-2025-0101",
                    DateOpen = DateTime.Today.AddDays(-60),
                    DateExpired = DateTime.Today.AddDays(-26),
                    OverdueDays = 26,
                    UnitPriceActual = 640_000_000,
                    GrtValue = 500_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Settled,
                    Remark = "Vietcombank Ä‘Ã£ trÃ­ch ná»£ tÃ i khoáº£n phong tá»a chi tráº£ bá»“i hoÃ n cho HTC"
                }
            );
            await db.SaveChangesAsync();

            // 2. CÃ´ng vÄƒn ÄANG Gá»¬I NGÃ‚N HÃ€NG ÄÃ’I TIá»€N (SentToBank) - Techcombank & Hyundai Pháº¡m HÃ¹ng
            var gcd2 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-002",
                DealerCode = "DLR-PHAMHUNG",
                DealerName = "Hyundai Pháº¡m HÃ¹ng",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                BankCodeMonitor = "TCB_HO",
                FlagIsHTC = "1",
                TotalCarCount = 2,
                TotalClaimAmount = 1_580_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.SentToBank,
                SignCAStatus = ClaimSignCAStatus.Signed,
                SignedBy = "LeNgocAnh_TongGiamDoc_HTC",
                SignedAt = DateTime.Today.AddDays(-3),
                CertThumbprint = "TCB456CA02",
                SentToBankAt = DateTime.Today.AddDays(-2),
                BankRefNo = "TCB-CLAIM-REC-20250512",
                FilePath = "/documents/claims/CVDBL-202505-002-Signed.pdf",
                Remark = "Äáº¡i lÃ½ quÃ¡ háº¡n cam káº¿t thanh toÃ¡n xe 18 ngÃ y. Techcombank Ä‘ang tiáº¿n hÃ nh phong tá»a tÃ i khoáº£n vÃ  chuáº©n bá»‹ trÃ­ch tiá»n bá»“i hoÃ n.",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.GuaranteeClaims.Add(gcd2);
            await db.SaveChangesAsync();

            db.GuaranteeClaimDetails.AddRange(
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CUS-8004",
                    VIN = "KMHCT81EPHU800201",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 Turbo Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PRE",
                    SpecDescription = "MPV 7 chá»— thÆ°Æ¡ng gia cá»­a lÃ¹a Ä‘iá»‡n",
                    ColorName = "Tráº¯ng Ngá»c Trai",
                    SOCode = "SO-2025-0402",
                    ContractNo = "PLHD-PHAMHUNG-2025/02",
                    GuaranteeNo = "GRT-2025-0402",
                    BankGuaranteeNo = "BL-TCB-2025-0210",
                    DateOpen = DateTime.Today.AddDays(-50),
                    DateExpired = DateTime.Today.AddDays(-18),
                    OverdueDays = 18,
                    UnitPriceActual = 1_180_000_000,
                    GrtValue = 980_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Claimed,
                    Remark = "Äang chá» Techcombank thá»±c hiá»‡n nghÄ©a vá»¥ báº£o lÃ£nh thanh toÃ¡n"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8005",
                    VIN = "KMHCT81EPHU800202",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Äáº·c Biá»‡t",
                    SpecCode = "CRE-1.5L-DB",
                    SpecDescription = "B-SUV Ä‘Ã´ thá»‹",
                    ColorName = "XÃ¡m Titan",
                    SOCode = "SO-2025-0402",
                    ContractNo = "PLHD-PHAMHUNG-2025/02",
                    GuaranteeNo = "GRT-2025-0402",
                    BankGuaranteeNo = "BL-TCB-2025-0210",
                    DateOpen = DateTime.Today.AddDays(-50),
                    DateExpired = DateTime.Today.AddDays(-18),
                    OverdueDays = 18,
                    UnitPriceActual = 720_000_000,
                    GrtValue = 600_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Claimed,
                    Remark = "Äang chá» Techcombank thá»±c hiá»‡n nghÄ©a vá»¥ báº£o lÃ£nh thanh toÃ¡n"
                }
            );
            await db.SaveChangesAsync();

            // 3. CÃ´ng vÄƒn ÄÃƒ TRÃŒNH THáº¨M Äá»ŠNH PHÃP CHáº¾ & Rá»¦I RO (Submitted) - MBBank & Hyundai SÃ i GÃ²n
            var gcd3 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-003",
                DealerCode = "DLR-SAIGON",
                DealerName = "Hyundai SÃ i GÃ²n 1S",
                BankCode = "MBB",
                BankName = "NgÃ¢n hÃ ng TMCP QuÃ¢n Äá»™i (MBBank)",
                BankCodeMonitor = "MBB_SAIGON",
                FlagIsHTC = "2",
                TotalCarCount = 2,
                TotalClaimAmount = 880_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Submitted,
                SignCAStatus = ClaimSignCAStatus.Pending,
                Remark = "Äáº¡i lÃ½ quÃ¡ háº¡n cam káº¿t báº£o lÃ£nh 14 ngÃ y. Há»“ sÆ¡ Ä‘ang Ä‘Æ°á»£c Ban PhÃ¡p cháº¿ & Rá»§i ro tÃ­n dá»¥ng HTV tháº©m Ä‘á»‹nh trÆ°á»›c khi kÃ½ sá»‘ CA.",
                CreatedBy = "NguyenVanHieu_TinDungHTV",
                CreatedAt = DateTime.Today.AddDays(-2)
            };
            db.GuaranteeClaims.Add(gcd3);
            await db.SaveChangesAsync();

            db.GuaranteeClaimDetails.AddRange(
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-8006",
                    VIN = "KMHCT81EPHU800301",
                    ModelCode = "ACCENT-1.5AT",
                    ModelName = "All New Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "Sedan háº¡ng B tháº¿ há»‡ má»›i",
                    ColorName = "Tráº¯ng",
                    SOCode = "SO-2025-0403",
                    ContractNo = "PLHD-SAIGON-2025/03",
                    GuaranteeNo = "GRT-2025-0403",
                    BankGuaranteeNo = "BL-MBB-2025-0315",
                    DateOpen = DateTime.Today.AddDays(-45),
                    DateExpired = DateTime.Today.AddDays(-14),
                    OverdueDays = 14,
                    UnitPriceActual = 510_000_000,
                    GrtValue = 430_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Pending,
                    Remark = "Chá» hoÃ n thiá»‡n thá»§ tá»¥c tháº©m Ä‘á»‹nh phÃ¡p cháº¿ Ä‘á»ƒ kÃ½ sá»‘ CA"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SGZ-8007",
                    VIN = "KMHCT81EPHU800302",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X TiÃªu Chuáº©n",
                    SpecCode = "SGZ-1.5L-STD",
                    SpecDescription = "MPV 7 chá»— gáº§m cao",
                    ColorName = "Báº¡c",
                    SOCode = "SO-2025-0403",
                    ContractNo = "PLHD-SAIGON-2025/03",
                    GuaranteeNo = "GRT-2025-0403",
                    BankGuaranteeNo = "BL-MBB-2025-0315",
                    DateOpen = DateTime.Today.AddDays(-45),
                    DateExpired = DateTime.Today.AddDays(-14),
                    OverdueDays = 14,
                    UnitPriceActual = 550_000_000,
                    GrtValue = 450_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Pending,
                    Remark = "Chá» hoÃ n thiá»‡n thá»§ tá»¥c tháº©m Ä‘á»‹nh phÃ¡p cháº¿ Ä‘á»ƒ kÃ½ sá»‘ CA"
                }
            );
            await db.SaveChangesAsync();

            // 4. CÃ´ng vÄƒn Má»šI Táº O Dá»° THáº¢O (Draft) - VietinBank & Hyundai ÄÃ´ng Anh
            var gcd4 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-004",
                DealerCode = "DLR-DONGANH",
                DealerName = "Hyundai ÄÃ´ng Anh",
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                BankCodeMonitor = "CTG_DONGANH",
                FlagIsHTC = "1",
                TotalCarCount = 2,
                TotalClaimAmount = 2_450_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Draft,
                SignCAStatus = ClaimSignCAStatus.Pending,
                Remark = "Há»“ sÆ¡ dá»± tháº£o Ä‘Ã²i ná»£ xe Palisade & Tucson quÃ¡ háº¡n 8 ngÃ y. Äang kiá»ƒm tra bá»• sung Ä‘á»‘i chiáº¿u biÃªn báº£n bÃ n giao xe theo há»‘i phiáº¿u.",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.GuaranteeClaims.Add(gcd4);
            await db.SaveChangesAsync();

            db.GuaranteeClaimDetails.AddRange(
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-PAL-8008",
                    VIN = "KMHCT81EPHU800401",
                    ModelCode = "PALISADE-2.2D",
                    ModelName = "Hyundai Palisade 2.2 Diesel Prestige 6 Chá»—",
                    SpecCode = "PAL-2.2D-PRE6",
                    SpecDescription = "SUV Ä‘áº§u báº£ng sang trá»ng",
                    ColorName = "Xanh BÃ³ng ÄÃªm",
                    SOCode = "SO-2025-0501",
                    ContractNo = "PLHD-DONGANH-2025/05",
                    GuaranteeNo = "GRT-2025-0501",
                    BankGuaranteeNo = "BL-CTG-2025-0419",
                    DateOpen = DateTime.Today.AddDays(-38),
                    DateExpired = DateTime.Today.AddDays(-8),
                    OverdueDays = 8,
                    UnitPriceActual = 1_850_000_000,
                    GrtValue = 1_550_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Pending,
                    Remark = "Há»“ sÆ¡ dá»± tháº£o Ä‘ang rÃ  soÃ¡t phá»¥ lá»¥c há»£p Ä‘á»“ng"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-8009",
                    VIN = "KMHCT81EPHU800402",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Äáº·c Biá»‡t",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover mÃ¡y dáº§u tiáº¿t kiá»‡m",
                    ColorName = "Äá» Máº­n",
                    SOCode = "SO-2025-0501",
                    ContractNo = "PLHD-DONGANH-2025/05",
                    GuaranteeNo = "GRT-2025-0501",
                    BankGuaranteeNo = "BL-CTG-2025-0419",
                    DateOpen = DateTime.Today.AddDays(-38),
                    DateExpired = DateTime.Today.AddDays(-8),
                    OverdueDays = 8,
                    UnitPriceActual = 1_080_000_000,
                    GrtValue = 900_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Pending,
                    Remark = "Há»“ sÆ¡ dá»± tháº£o Ä‘ang rÃ  soÃ¡t phá»¥ lá»¥c há»£p Ä‘á»“ng"
                }
            );
            await db.SaveChangesAsync();

            // 5. CÃ´ng vÄƒn ÄÃƒ Há»¦Y DO Äáº I LÃ Ná»˜P TIá»€N TRá»°C TIáº¾P (Cancelled) - VPBank & Hyundai An KhÃ¡nh
            var gcd5 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-005",
                DealerCode = "DLR-ANKHANH",
                DealerName = "Hyundai An KhÃ¡nh",
                BankCode = "VPB",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                BankCodeMonitor = "VPB_HO",
                FlagIsHTC = "1",
                TotalCarCount = 2,
                TotalClaimAmount = 1_700_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Cancelled,
                SignCAStatus = ClaimSignCAStatus.Pending,
                CancelledAt = DateTime.Today.AddDays(-1),
                CancelReason = "Äáº¡i lÃ½ Ä‘Ã£ thanh toÃ¡n chuyá»ƒn khoáº£n dá»©t Ä‘iá»ƒm 100% tiá»n xe qua UNC Vietcombank nÃªn HTC há»§y cÃ´ng vÄƒn Ä‘Ã²i ná»£ báº£o lÃ£nh.",
                Remark = "Äáº¡i lÃ½ Ä‘Ã£ ná»™p Ä‘á»§ tiá»n kháº¯c phá»¥c vi pháº¡m trÆ°á»›c khi gá»­i cÃ´ng vÄƒn chÃ­nh thá»©c tá»›i VPBank.",
                CreatedBy = "VuMinhTu_TinDung",
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.GuaranteeClaims.Add(gcd5);
            await db.SaveChangesAsync();

            db.GuaranteeClaimDetails.AddRange(
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-8010",
                    VIN = "KMHCT81EPHU800501",
                    ModelCode = "SANTAFE-2.5X",
                    ModelName = "Hyundai Santa Fe 2.5 XÄƒng Cao Cáº¥p",
                    SpecCode = "SAN-2.5X-PRE",
                    SpecDescription = "SUV 7 chá»— mÃ¡y xÄƒng",
                    ColorName = "Tráº¯ng Tinh KhÃ´i",
                    SOCode = "SO-2025-0450",
                    ContractNo = "PLHD-ANKHANH-2025/04",
                    GuaranteeNo = "GRT-2025-0450",
                    BankGuaranteeNo = "BL-VPB-2025-0550",
                    DateOpen = DateTime.Today.AddDays(-40),
                    DateExpired = DateTime.Today.AddDays(-10),
                    OverdueDays = 10,
                    UnitPriceActual = 1_180_000_000,
                    GrtValue = 950_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Cancelled,
                    Remark = "Äáº¡i lÃ½ ná»™p tiá»n trá»±c tiáº¿p vÃ o tÃ i khoáº£n HTC, há»§y há»“ sÆ¡ Ä‘Ã²i ná»£"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8011",
                    VIN = "KMHCT81EPHU800502",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5L-PRE",
                    SpecDescription = "B-SUV Ä‘Ã´ thá»‹",
                    ColorName = "Äá» Máº­n",
                    SOCode = "SO-2025-0450",
                    ContractNo = "PLHD-ANKHANH-2025/04",
                    GuaranteeNo = "GRT-2025-0450",
                    BankGuaranteeNo = "BL-VPB-2025-0550",
                    DateOpen = DateTime.Today.AddDays(-40),
                    DateExpired = DateTime.Today.AddDays(-10),
                    OverdueDays = 10,
                    UnitPriceActual = 900_000_000,
                    GrtValue = 750_000_000,
                    GrtPercent = 100.0,
                    Status = GuaranteeClaimDetailStatus.Cancelled,
                    Remark = "Äáº¡i lÃ½ ná»™p tiá»n trá»±c tiáº¿p vÃ o tÃ i khoáº£n HTC, há»§y há»“ sÆ¡ Ä‘Ã²i ná»£"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Báº£ng kÃª thanh toÃ¡n chi phÃ­ thiáº¿t bá»‹ AVN trÃªn xe Ã´ tÃ´ (Pmt_PaymentAVN & Pmt_PaymentAVNDetail / BizHTC.Payment)
        if (!await db.PaymentAVNs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Báº£ng kÃª ThÃ¡ng 04/2025 - ÄÃƒ QUYáº¾T TOÃN CHUYá»‚N TIá»€N UNC VIETCOMBANK (Settled)
            // Cung cáº¥p bá»Ÿi Mobis Auto Parts VN, 4 xe SantaFe, Tucson, Creta, Accent
            var avn1 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202504-001",
                PmtMonth = "2025-04",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                TotalVehicles = 4,
                TotalAmount = 41_500_000,          // 12.5M + 11.0M + 9.5M + 8.5M = 41,500,000
                VATRate = 10.0m,
                AmountVAT = 4_150_000,             // 10% VAT
                TotalAmountAfterVAT = 45_650_000,  // Sau VAT
                Status = PaymentAVNStatus.Settled,
                TCMSSignStatus = AVNSignCAStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-18),
                HTVSignStatus = AVNSignCAStatus.Signed,
                HTVSignUser = "TranThiHong_TruongPhongKT_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-17),
                Appr1By = "NguyenVanQuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Today.AddDays(-20),
                Appr2By = "TranThiHong_TruongPhongKT_HTV",
                Appr2DTime = DateTime.Today.AddDays(-19),
                SettledBy = "PhamVanThanh_KeToanNganHang",
                SettledAt = DateTime.Today.AddDays(-15),
                BankTxnRef = "UNC-VCB-AVN-202504-8901",
                FilePath = "/documents/avn/AVN-202504-001-Signed.pdf",
                Remark = "Quyáº¿t toÃ¡n thanh toÃ¡n chi phÃ­ mÃ n hÃ¬nh AVN dáº«n Ä‘Æ°á»ng tÃ­ch há»£p báº£n Ä‘á»“ Viá»‡t Nam ká»³ thÃ¡ng 04/2025",
                CreatedBy = "LeMinhKhoa_KyThuatVien",
                CreatedAt = DateTime.Today.AddDays(-22)
            };
            db.PaymentAVNs.Add(avn1);
            await db.SaveChangesAsync();

            db.PaymentAVNDetails.AddRange(
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900101",
                    EngineNo = "G4KP-991201",
                    ModelCode = "SANTAFE",
                    ModelName = "Hyundai Santa Fe 2.5 XÄƒng Cao Cáº¥p",
                    SpecCode = "SF-2.5G-PREM",
                    SpecDescription = "MÃ n hÃ¬nh cong Ä‘Ã´i Panoramic 12.3 inch tÃ­ch há»£p báº£n Ä‘á»“ vá»‡ tinh",
                    ColorName = "Tráº¯ng Ngá»c Trai",
                    AVNCode = "AVN-STF-GEN5-PREM",
                    SerialNo = "AVN-SN-STF-990101",
                    UnitPriceAVN = 12_500_000,
                    AVNDate = DateTime.Today.AddDays(-32),
                    InStorageDate = DateTime.Today.AddDays(-28),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Kiá»ƒm tra káº¿t ná»‘i GPS vÃ  Apple CarPlay/Android Auto khÃ´ng dÃ¢y hoáº¡t Ä‘á»™ng tá»‘t"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900102",
                    EngineNo = "G4FJ-882302",
                    ModelCode = "TUCSON",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-HTRAC",
                    SpecDescription = "MÃ n hÃ¬nh giáº£i trÃ­ trung tÃ¢m 10.25 inch cáº£m á»©ng Ä‘a Ä‘iá»ƒm",
                    ColorName = "Äen Ãnh Kim",
                    AVNCode = "AVN-TUC-GEN5-1025",
                    SerialNo = "AVN-SN-TUC-882302",
                    UnitPriceAVN = 11_000_000,
                    AVNDate = DateTime.Today.AddDays(-30),
                    InStorageDate = DateTime.Today.AddDays(-27),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "CÃ i Ä‘áº·t báº£n Ä‘á»“ Ä‘á»‹nh vá»‹ Viá»‡t Nam V9 má»›i nháº¥t"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900103",
                    EngineNo = "G4FL-773403",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRT-1.5L-PREM",
                    SpecDescription = "MÃ n hÃ¬nh 10.25 inch Ã¢m thanh 8 loa Bose",
                    ColorName = "Äá» Máº­n",
                    AVNCode = "AVN-CRT-1025-NAV",
                    SerialNo = "AVN-SN-CRT-773403",
                    UnitPriceAVN = 9_500_000,
                    AVNDate = DateTime.Today.AddDays(-29),
                    InStorageDate = DateTime.Today.AddDays(-25),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Ã‚m thanh Bose vÃ  camera lÃ¹i hiá»ƒn thá»‹ sáº¯c nÃ©t"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900104",
                    EngineNo = "G4LC-664504",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "MÃ n hÃ¬nh liá»n khá»‘i 8 inch há»— trá»£ Ä‘á»‹nh vá»‹ báº£n Ä‘á»“ tiáº¿ng Viá»‡t",
                    ColorName = "Báº¡c",
                    AVNCode = "AVN-ACC-GEN5-8IN",
                    SerialNo = "AVN-SN-ACC-664504",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-28),
                    InStorageDate = DateTime.Today.AddDays(-24),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Láº¯p Ä‘áº·t Ä‘áº¡t chuáº©n QA xÆ°á»Ÿng láº¯p rÃ¡p hoÃ n thiá»‡n HTMV"
                }
            );
            await db.SaveChangesAsync();

            // 2. Báº£ng kÃª ThÃ¡ng 05/2025 - ÄÃƒ KÃ Sá» CA HOÃ€N Táº¤T (Signed)
            // Äá»‘i tÃ¡c Motrex Auto Electronics, 3 xe cao cáº¥p Palisade, Custin, Ioniq 5
            var avn2 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-001",
                PmtMonth = "2025-05",
                SupplierCode = "MOTREX-VN",
                SupplierName = "CÃ´ng ty TNHH Äiá»‡n tá»­ Motrex Viá»‡t Nam",
                TotalVehicles = 3,
                TotalAmount = 48_500_000,          // 16.5M (Palisade) + 14.0M (Custin) + 18.0M (Ioniq 5) = 48,500,000
                VATRate = 10.0m,
                AmountVAT = 4_850_000,
                TotalAmountAfterVAT = 53_350_000,
                Status = PaymentAVNStatus.Signed,
                TCMSSignStatus = AVNSignCAStatus.Signed,
                TCMSSignUser = "TranQuocTuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-4),
                HTVSignStatus = AVNSignCAStatus.Signed,
                HTVSignUser = "NguyenVanHieu_TongGiamDoc_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-3),
                Appr1By = "TranQuocTuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Today.AddDays(-6),
                Appr2By = "LeHongPhong_TruongPhongKD_HTV",
                Appr2DTime = DateTime.Today.AddDays(-5),
                FilePath = "/documents/avn/AVN-202505-001-Signed.pdf",
                Remark = "Báº£ng kÃª mÃ n hÃ¬nh giáº£i trÃ­ tháº¿ há»‡ má»›i cho cÃ¡c máº«u xe SUV Ä‘áº§u báº£ng vÃ  xe Ä‘iá»‡n",
                CreatedBy = "VuMinhTu_LinhKien",
                CreatedAt = DateTime.Today.AddDays(-8)
            };
            db.PaymentAVNs.Add(avn2);
            await db.SaveChangesAsync();

            db.PaymentAVNDetails.AddRange(
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn2.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900201",
                    EngineNo = "D4HB-110201",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade 2.2 Diesel Prestige",
                    SpecCode = "PAL-2.2D-PRE6",
                    SpecDescription = "MÃ n hÃ¬nh giáº£i trÃ­ kÃ©p 12.3 inch Ã¢m thanh vÃ²m Infinity 12 loa",
                    ColorName = "Xanh BÃ³ng ÄÃªm",
                    AVNCode = "AVN-PAL-DUAL-12.3IN",
                    SerialNo = "AVN-SN-PAL-110201",
                    UnitPriceAVN = 16_500_000,
                    AVNDate = DateTime.Today.AddDays(-12),
                    InStorageDate = DateTime.Today.AddDays(-9),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Há»‡ thá»‘ng hiá»ƒn thá»‹ camera 360 toÃ n cáº£nh SVM vÃ  giÃ¡m sÃ¡t Ä‘iá»ƒm mÃ¹ BVM"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn2.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900202",
                    EngineNo = "G4NN-220202",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CST-2.0T-PREM",
                    SpecDescription = "MÃ n hÃ¬nh dá»c 10.4 inch giao diá»‡n tiáº¿ng Viá»‡t Ä‘iá»u khiá»ƒn tiá»‡n nghi hÃ ng gháº¿ 2",
                    ColorName = "Tráº¯ng",
                    AVNCode = "AVN-CST-PORTRAIT-104",
                    SerialNo = "AVN-SN-CST-220202",
                    UnitPriceAVN = 14_000_000,
                    AVNDate = DateTime.Today.AddDays(-11),
                    InStorageDate = DateTime.Today.AddDays(-8),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Äiá»u khiá»ƒn Ä‘iá»u hÃ²a vÃ  cháº¿ Ä‘á»™ gháº¿ thÆ°Æ¡ng gia tÃ­ch há»£p mÃ n hÃ¬nh"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn2.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900203",
                    EngineNo = "EM07-330203",
                    ModelCode = "IONIQ-5",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "MÃ n hÃ¬nh Ä‘Ã´i thÃ´ng tin & giáº£i trÃ­ kÃ©p 12.3 inch hiá»ƒn thá»‹ tráº¡ng thÃ¡i sáº¡c Pin EV",
                    ColorName = "XÃ¡m Xi MÄƒng",
                    AVNCode = "AVN-IQ5-EV-DUAL-12.3",
                    SerialNo = "AVN-SN-IQ5-330203",
                    UnitPriceAVN = 18_000_000,
                    AVNDate = DateTime.Today.AddDays(-10),
                    InStorageDate = DateTime.Today.AddDays(-7),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Äá»‹nh vá»‹ báº£n Ä‘á»“ tráº¡m sáº¡c Ä‘iá»‡n toÃ n quá»‘c vÃ  tá»‘i Æ°u hÃ nh trÃ¬nh EV"
                }
            );
            await db.SaveChangesAsync();

            // 3. Báº£ng kÃª ThÃ¡ng 05/2025 - HTV ÄÃƒ PHÃŠ DUYá»†T Cáº¤P 2 (HTVApproved)
            // ÄÃ£ Ä‘Æ°á»£c TCMS kÃ½ CA, Ä‘ang chá» lÃ£nh Ä‘áº¡o HTV kÃ½ sá»‘ hoÃ n táº¥t
            var avn3 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-002",
                PmtMonth = "2025-05",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                TotalVehicles = 3,
                TotalAmount = 29_100_000,          // 9.8M (Elantra) + 8.8M (Stargazer) + 10.5M (Elantra N-Line) = 29,100,000
                VATRate = 10.0m,
                AmountVAT = 2_910_000,
                TotalAmountAfterVAT = 32_010_000,
                Status = PaymentAVNStatus.HTVApproved,
                TCMSSignStatus = AVNSignCAStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-2),
                HTVSignStatus = AVNSignCAStatus.Pending,
                Appr1By = "NguyenVanQuan_GDKyThuat_TCMS",
                Appr1DTime = DateTime.Today.AddDays(-4),
                Appr2By = "TranThiHong_TruongPhongKT_HTV",
                Appr2DTime = DateTime.Today.AddDays(-3),
                Remark = "Báº£ng kÃª Ä‘á»£t 2 thÃ¡ng 05/2025: LÃ´ xe Elantra & Stargazer Ä‘Ã£ kiá»ƒm Ä‘á»‹nh PDI Ä‘áº¡t chuáº©n",
                CreatedBy = "LeMinhKhoa_KyThuatVien",
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.PaymentAVNs.Add(avn3);
            await db.SaveChangesAsync();

            db.PaymentAVNDetails.AddRange(
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn3.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900301",
                    EngineNo = "G4NL-440301",
                    ModelCode = "ELANTRA",
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cáº¥p",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "MÃ n hÃ¬nh thÃ´ng tin giáº£i trÃ­ 10.25 inch liá»n khá»‘i",
                    ColorName = "Xanh DÆ°Æ¡ng",
                    AVNCode = "AVN-ELN-1025-WIDESCREEN",
                    SerialNo = "AVN-SN-ELN-440301",
                    UnitPriceAVN = 9_800_000,
                    AVNDate = DateTime.Today.AddDays(-7),
                    InStorageDate = DateTime.Today.AddDays(-5),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Káº¿t ná»‘i Bluetooth Ä‘a thiáº¿t bá»‹ vÃ  nháº­n diá»‡n giá»ng nÃ³i"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn3.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900302",
                    EngineNo = "G4FL-550302",
                    ModelCode = "STARGAZER",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p",
                    SpecCode = "SGZ-1.5L-PREM",
                    SpecDescription = "MÃ n hÃ¬nh cáº£m á»©ng 10.25 inch Ã¢m thanh 6 loa",
                    ColorName = "Tráº¯ng Má»",
                    AVNCode = "AVN-SGZ-1025-NAV",
                    SerialNo = "AVN-SN-SGZ-550302",
                    UnitPriceAVN = 8_800_000,
                    AVNDate = DateTime.Today.AddDays(-6),
                    InStorageDate = DateTime.Today.AddDays(-4),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Giao diá»‡n thÃ¢n thiá»‡n há»— trá»£ kiá»ƒm soÃ¡t sáº¡c khÃ´ng dÃ¢y Qi"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn3.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900303",
                    EngineNo = "G4FJ-660303",
                    ModelCode = "ELANTRA-NLINE",
                    ModelName = "Hyundai Elantra N Line 1.6 Turbo",
                    SpecCode = "ELN-1.6T-NL",
                    SpecDescription = "MÃ n hÃ¬nh thá»ƒ thao giao diá»‡n N-Mode Ä‘o lÆ°á»ng lá»±c G vÃ  gia tá»‘c",
                    ColorName = "Äá» N Line",
                    AVNCode = "AVN-ELN-NLINE-SPORT",
                    SerialNo = "AVN-SN-ELN-660303",
                    UnitPriceAVN = 10_500_000,
                    AVNDate = DateTime.Today.AddDays(-5),
                    InStorageDate = DateTime.Today.AddDays(-3),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Äo lÆ°á»ng thá»i gian vÃ²ng cháº¡y Lap timer vÃ  Ã¡p suáº¥t lá»‘p TPMS tÃ­ch há»£p"
                }
            );
            await db.SaveChangesAsync();

            // 4. Báº£ng kÃª ThÃ¡ng 05/2025 - Má»šI Táº O Dá»° THáº¢O (Draft)
            // LÃ´ xe má»›i láº¯p rÃ¡p xong táº¡i phÃ¢n xÆ°á»Ÿng Ninh BÃ¬nh, 2 xe SantaFe Calligraphy & Tucson Turbo
            var avn4 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-003",
                PmtMonth = "2025-05",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                TotalVehicles = 2,
                TotalAmount = 25_500_000,          // 14.0M (SantaFe Calligraphy) + 11.5M (Tucson Turbo) = 25,500,000
                VATRate = 10.0m,
                AmountVAT = 2_550_000,
                TotalAmountAfterVAT = 28_050_000,
                Status = PaymentAVNStatus.Draft,
                TCMSSignStatus = AVNSignCAStatus.Pending,
                HTVSignStatus = AVNSignCAStatus.Pending,
                Remark = "Dá»± tháº£o báº£ng kÃª thanh toÃ¡n mÃ n hÃ¬nh AVN lÃ´ xuáº¥t xÆ°á»Ÿng tuáº§n thá»© 3 thÃ¡ng 05/2025",
                CreatedBy = "LeMinhKhoa_KyThuatVien",
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.PaymentAVNs.Add(avn4);
            await db.SaveChangesAsync();

            db.PaymentAVNDetails.AddRange(
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn4.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900401",
                    EngineNo = "G4KP-770401",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "MÃ n hÃ¬nh cong liá»n khá»‘i 12.3 inch tháº¿ há»‡ ccNC má»›i nháº¥t",
                    ColorName = "Äá»“ng Ãnh Kim",
                    AVNCode = "AVN-STF-CAL-12.3IN",
                    SerialNo = "AVN-SN-STF-770401",
                    UnitPriceAVN = 14_000_000,
                    AVNDate = DateTime.Today.AddDays(-2),
                    InStorageDate = DateTime.Today.AddDays(-1),
                    Status = PaymentAVNDetailStatus.Pending,
                    Remark = "Cáº­p nháº­t pháº§n má»m há»‡ Ä‘iá»u hÃ nh ccNC há»— trá»£ káº¿t ná»‘i khÃ´ng dÃ¢y kÃ©p"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn4.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900402",
                    EngineNo = "G4FJ-880402",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "MÃ n hÃ¬nh Ä‘Ã´i 12.3 inch tÃ­ch há»£p Ä‘iá»u khiá»ƒn cáº£m á»©ng",
                    ColorName = "Tráº¯ng",
                    AVNCode = "AVN-TUC-TURBO-1025",
                    SerialNo = "AVN-SN-TUC-880402",
                    UnitPriceAVN = 11_500_000,
                    AVNDate = DateTime.Today.AddDays(-2),
                    InStorageDate = DateTime.Today.AddDays(-1),
                    Status = PaymentAVNDetailStatus.Pending,
                    Remark = "Há»‡ thá»‘ng báº£n Ä‘á»“ 3D offline há»— trá»£ cáº£nh bÃ¡o tá»‘c Ä‘á»™"
                }
            );
            await db.SaveChangesAsync();

            // 5. Báº£ng kÃª ThÃ¡ng 03/2025 - ÄÃƒ Há»¦Y (Cancelled)
            // Há»§y do Ä‘á»‘i tÃ¡c giao nháº§m lÃ´ mÃ n hÃ¬nh mÃ£ cÅ©, Ä‘Ã£ láº­p báº£ng kÃª thay tháº¿
            var avn5 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202503-099",
                PmtMonth = "2025-03",
                SupplierCode = "VIETTEL-AVN",
                SupplierName = "Tá»•ng CÃ´ng ty Giáº£i phÃ¡p Doanh nghiá»‡p Viettel (VTS)",
                TotalVehicles = 2,
                TotalAmount = 17_000_000,
                VATRate = 10.0m,
                AmountVAT = 1_700_000,
                TotalAmountAfterVAT = 18_700_000,
                Status = PaymentAVNStatus.Cancelled,
                TCMSSignStatus = AVNSignCAStatus.Pending,
                HTVSignStatus = AVNSignCAStatus.Pending,
                CancelledAt = DateTime.Today.AddDays(-40),
                RejectReason = "Há»§y báº£ng kÃª do Ä‘á»‘i tÃ¡c cáº¥p sai lÃ´ mÃ£ linh kiá»‡n AVN-ACC-GEN4 thay vÃ¬ GEN5. ÄÃ£ láº­p báº£ng kÃª bÃ¹ trong thÃ¡ng 04.",
                Remark = "Báº£ng kÃª bá»‹ há»§y trÆ°á»›c khi trÃ¬nh duyá»‡t cáº¥p 1",
                CreatedBy = "VuMinhTu_LinhKien",
                CreatedAt = DateTime.Today.AddDays(-45)
            };
            db.PaymentAVNs.Add(avn5);
            await db.SaveChangesAsync();

            db.PaymentAVNDetails.AddRange(
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn5.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900501",
                    EngineNo = "G4LC-990501",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT",
                    SpecCode = "ACC-1.5AT-STD",
                    SpecDescription = "MÃ n hÃ¬nh AVN máº«u cÅ©",
                    ColorName = "Äen",
                    AVNCode = "AVN-ACC-GEN4-7IN",
                    SerialNo = "AVN-SN-OLD-990501",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-44),
                    InStorageDate = DateTime.Today.AddDays(-42),
                    Status = PaymentAVNDetailStatus.Cancelled,
                    Remark = "Há»§y dÃ²ng xe do khÃ´ng khá»›p chá»©ng tá»« nháº­p linh kiá»‡n"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn5.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900502",
                    EngineNo = "G4LC-990502",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT",
                    SpecCode = "ACC-1.5AT-STD",
                    SpecDescription = "MÃ n hÃ¬nh AVN máº«u cÅ©",
                    ColorName = "Tráº¯ng",
                    AVNCode = "AVN-ACC-GEN4-7IN",
                    SerialNo = "AVN-SN-OLD-990502",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-44),
                    InStorageDate = DateTime.Today.AddDays(-42),
                    Status = PaymentAVNDetailStatus.Cancelled,
                    Remark = "Há»§y dÃ²ng xe do khÃ´ng khá»›p chá»©ng tá»« nháº­p linh kiá»‡n"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Báº£ng Ä‘Æ¡n giÃ¡ Ä‘á»‹nh má»©c thuÃª bao dá»‹ch vá»¥ GPS (Mst_UnitPriceGPS / BizHTC.Payment)
        if (!await db.UnitPriceGPSs.AnyAsync(u => u.OrgId == TenantContext.DefaultOrgId))
        {
            db.UnitPriceGPSs.AddRange(
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-VIETTEL-2024",
                    ProviderCode = "VIETTEL",
                    ProviderName = "Tá»•ng CÃ´ng ty Viá»…n thÃ´ng Viettel (Viettel Telecom)",
                    DailyPrice = 2_500,
                    MonthlyRate = 75_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Há»£p Ä‘á»“ng khung cung cáº¥p thiáº¿t bá»‹ vÃ  dá»‹ch vá»¥ Ä‘á»‹nh vá»‹ GPS Viettel 4G toÃ n quá»‘c"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-VNPT-TRACK",
                    ProviderCode = "VNPT",
                    ProviderName = "Tá»•ng CÃ´ng ty Dá»‹ch vá»¥ Viá»…n thÃ´ng VNPT (VNPT VinaPhone)",
                    DailyPrice = 2_400,
                    MonthlyRate = 72_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Há»£p Ä‘á»“ng thuÃª bao Ä‘á»‹nh vá»‹ vá»‡ tinh VNPT Smart Motor & Fleet"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-BINHANH",
                    ProviderCode = "BAGPS",
                    ProviderName = "CÃ´ng ty TNHH PhÃ¡t triá»ƒn CÃ´ng nghá»‡ Äiá»‡n tá»­ BÃ¬nh Anh (BA GPS)",
                    DailyPrice = 2_300,
                    MonthlyRate = 69_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Há»£p Ä‘á»“ng thiáº¿t bá»‹ giÃ¡m sÃ¡t hÃ nh trÃ¬nh há»£p chuáº©n BA-GPS"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-BKAV-SMART",
                    ProviderCode = "BKAV",
                    ProviderName = "Táº­p Ä‘oÃ n CÃ´ng nghá»‡ BKAV - Smart Vehicle IoT Solutions",
                    DailyPrice = 2_600,
                    MonthlyRate = 78_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Há»£p Ä‘á»“ng giÃ¡m sÃ¡t thÃ´ng minh IoT vÃ  an ninh dá»¯ liá»‡u vá»‹ trÃ­ xe BKAV"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Báº£ng kÃª thanh toÃ¡n chi phÃ­ Ä‘á»‹nh vá»‹ GPS trÃªn xe Ã´ tÃ´ (Pmt_PaymentGPS & Pmt_PaymentGPSDetail / BizHTC.Payment)
        if (!await db.PaymentGPSs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Báº£ng kÃª ThÃ¡ng 04/2025 - ÄÃƒ QUYáº¾T TOÃN THANH TOÃN (Settled)
            var gps1 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202504-001",
                PmtMonth = "2025-04",
                ContractNo = "HD-GPS-VIETTEL-2024",
                ProviderCode = "VIETTEL",
                ProviderName = "Tá»•ng CÃ´ng ty Viá»…n thÃ´ng Viettel (Viettel Telecom)",
                TotalVehicles = 4,
                TotalPlanDays = 107,
                TotalDeductDays = 3,
                TotalActualDays = 104,
                AmountTotal = 260_000,
                VATRate = 10.0m,
                UnitPriceVAT = 26_000,
                TotalAmountVAT = 286_000,
                Status = PaymentGPSStatus.Settled,
                HTVSignStatus = GPSSignCAStatus.Signed,
                HTVSignUser = "TranVanHung_GiamDocVanTai_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-28),
                TCMSSignStatus = GPSSignCAStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-29),
                Appr1By = "TranVanHung_GiamDocVanTai_HTV",
                Appr1DTime = DateTime.Today.AddDays(-30),
                Appr2By = "DangThanhTung_KeToanTruong_TCMS",
                Appr2DTime = DateTime.Today.AddDays(-29),
                SettledBy = "PhamThiMai_KeToanThanhToan_HTV",
                SettledAt = DateTime.Today.AddDays(-25),
                BankTxnRef = "UNC-GPS-20250428-1122",
                FilePath = "CR_Pmt_PaymentGPS_202504_001_Signed.pdf",
                Remark = "Quyáº¿t toÃ¡n thanh toÃ¡n phÃ­ dá»‹ch vá»¥ Ä‘á»‹nh vá»‹ GPS Viettel thÃ¡ng 04/2025 cho lÃ´ xe kho Ninh BÃ¬nh",
                CreatedBy = "VuMinhTu_QuanLyXe",
                CreatedAt = DateTime.Today.AddDays(-31)
            };
            db.PaymentGPSs.Add(gps1);
            await db.SaveChangesAsync();

            db.PaymentGPSDetails.AddRange(
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps1.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950101",
                    EngineNo = "G4KP-110101",
                    CarID = "CAR-NB-101",
                    ModelCode = "SANTAFE",
                    ModelName = "Hyundai Santa Fe 2.5 HTRAC",
                    SpecCode = "SF-2.5-GAS",
                    SpecDescription = "Báº£n xÄƒng cao cáº¥p",
                    GPSID = "GPS-VTT-950101",
                    ContractGPS = gps1.ContractNo,
                    GPSStartDate = new DateTime(2025, 3, 20),
                    CostGPSStartDate = new DateTime(2025, 4, 1),
                    CostGPSEndDate = new DateTime(2025, 4, 30),
                    PlanCostGPSDate = 30,
                    DeductDate = 0,
                    ActualCostGPSDate = 30,
                    PriceGPS = 2_500,
                    AmountGPS = 75_000,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "Thiáº¿t bá»‹ phÃ¡t sÃ³ng Ä‘á»‹nh vá»‹ GPS 4G chuáº©n xÃ¡c liÃªn tá»¥c 24/7"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps1.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950102",
                    EngineNo = "G4FJ-110102",
                    CarID = "CAR-NB-102",
                    ModelCode = "TUCSON",
                    ModelName = "Hyundai Tucson 2.0 MPI",
                    SpecCode = "TUC-2.0-AT",
                    SpecDescription = "Báº£n Ä‘áº·c biá»‡t mÃ¡y xÄƒng",
                    GPSID = "GPS-VTT-950102",
                    ContractGPS = gps1.ContractNo,
                    GPSStartDate = new DateTime(2025, 3, 22),
                    CostGPSStartDate = new DateTime(2025, 4, 1),
                    CostGPSEndDate = new DateTime(2025, 4, 30),
                    PlanCostGPSDate = 30,
                    DeductDate = 2,
                    ActualCostGPSDate = 28,
                    PriceGPS = 2_500,
                    AmountGPS = 70_000,
                    Status = PaymentGPSDetailStatus.Adjusted,
                    Remark = "Kháº¥u trá»« 2 ngÃ y xe vÃ o xÆ°á»Ÿng hiá»‡u chá»‰nh Äƒng-ten ngÆ°ng phÃ¡t sÃ³ng"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps1.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950103",
                    EngineNo = "G4NL-110103",
                    CarID = "CAR-NB-103",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5L Smart Stream",
                    SpecCode = "CRT-1.5L-PREM",
                    SpecDescription = "Báº£n cao cáº¥p 2 tÃ´ng mÃ u",
                    GPSID = "GPS-VTT-950103",
                    ContractGPS = gps1.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 5),
                    CostGPSStartDate = new DateTime(2025, 4, 5),
                    CostGPSEndDate = new DateTime(2025, 4, 30),
                    PlanCostGPSDate = 26,
                    DeductDate = 0,
                    ActualCostGPSDate = 26,
                    PriceGPS = 2_500,
                    AmountGPS = 65_000,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "KÃ­ch hoáº¡t giÃ¡m sÃ¡t Ä‘á»‹nh vá»‹ tá»« ngÃ y hoÃ n táº¥t kiá»ƒm tra xuáº¥t xÆ°á»Ÿng PDI"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps1.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950104",
                    EngineNo = "G4LC-110104",
                    CarID = "CAR-NB-104",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT",
                    SpecCode = "ACC-1.5AT-PREM",
                    SpecDescription = "Báº£n Ä‘áº·c biá»‡t",
                    GPSID = "GPS-VTT-950104",
                    ContractGPS = gps1.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 10),
                    CostGPSStartDate = new DateTime(2025, 4, 10),
                    CostGPSEndDate = new DateTime(2025, 4, 30),
                    PlanCostGPSDate = 21,
                    DeductDate = 1,
                    ActualCostGPSDate = 20,
                    PriceGPS = 2_500,
                    AmountGPS = 50_000,
                    Status = PaymentGPSDetailStatus.Adjusted,
                    Remark = "Kháº¥u trá»« 1 ngÃ y kiá»ƒm tra ká»¹ thuáº­t Ä‘á»‹nh ká»³ táº¡i bÃ£i"
                }
            );
            await db.SaveChangesAsync();

            // 2. Báº£ng kÃª ThÃ¡ng 05/2025 - ÄÃƒ KÃ Sá» CA HOÃ€N Táº¤T (Signed)
            var gps2 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-001",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-VNPT-TRACK",
                ProviderCode = "VNPT",
                ProviderName = "Tá»•ng CÃ´ng ty Dá»‹ch vá»¥ Viá»…n thÃ´ng VNPT (VNPT VinaPhone)",
                TotalVehicles = 3,
                TotalPlanDays = 93,
                TotalDeductDays = 1,
                TotalActualDays = 92,
                AmountTotal = 220_800,
                VATRate = 10.0m,
                UnitPriceVAT = 22_080,
                TotalAmountVAT = 242_880,
                Status = PaymentGPSStatus.Signed,
                HTVSignStatus = GPSSignCAStatus.Signed,
                HTVSignUser = "TranVanHung_GiamDocVanTai_HTV",
                HTVSignDTime = DateTime.Today.AddDays(-3),
                TCMSSignStatus = GPSSignCAStatus.Signed,
                TCMSSignUser = "NguyenVanQuan_GDKyThuat_TCMS",
                TCMSSignDTime = DateTime.Today.AddDays(-4),
                Appr1By = "TranVanHung_GiamDocVanTai_HTV",
                Appr1DTime = DateTime.Today.AddDays(-5),
                Appr2By = "DangThanhTung_KeToanTruong_TCMS",
                Appr2DTime = DateTime.Today.AddDays(-4),
                FilePath = "CR_Pmt_PaymentGPS_202505_001_Signed.pdf",
                Remark = "Báº£ng kÃª giÃ¡m sÃ¡t GPS xe nháº­p kháº©u phÃ¢n phá»‘i Ä‘á»£t 1 thÃ¡ng 05/2025 - Chá» káº¿ toÃ¡n chuyá»ƒn khoáº£n UNC",
                CreatedBy = "VuMinhTu_QuanLyXe",
                CreatedAt = DateTime.Today.AddDays(-6)
            };
            db.PaymentGPSs.Add(gps2);
            await db.SaveChangesAsync();

            db.PaymentGPSDetails.AddRange(
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps2.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950201",
                    EngineNo = "G6DU-220201",
                    CarID = "CAR-HP-201",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade 2.2 Diesel HTRAC",
                    SpecCode = "PAL-2.2D-PREM",
                    SpecDescription = "Báº£n 6 chá»— ngá»“i thÆ°Æ¡ng gia",
                    GPSID = "GPS-VNP-950201",
                    ContractGPS = gps2.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 25),
                    CostGPSStartDate = new DateTime(2025, 5, 1),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 0,
                    ActualCostGPSDate = 31,
                    PriceGPS = 2_400,
                    AmountGPS = 74_400,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "GiÃ¡m sÃ¡t vá»‹ trÃ­ lÆ°u kho táº¡i tá»•ng kho Hiá»‡p PhÆ°á»›c NhÃ  BÃ¨"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps2.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950202",
                    EngineNo = "G4KN-220202",
                    CarID = "CAR-HP-202",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CST-2.0T-PREM",
                    SpecDescription = "Báº£n cao cáº¥p cá»­a lÃ¹a Ä‘iá»‡n kÃ©p",
                    GPSID = "GPS-VNP-950202",
                    ContractGPS = gps2.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 26),
                    CostGPSStartDate = new DateTime(2025, 5, 1),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 1,
                    ActualCostGPSDate = 30,
                    PriceGPS = 2_400,
                    AmountGPS = 72_000,
                    Status = PaymentGPSDetailStatus.Adjusted,
                    Remark = "Kháº¥u trá»« 1 ngÃ y kiá»ƒm Ä‘á»‹nh mÃ´i trÆ°á»ng khÃ­ tháº£i xe"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps2.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950203",
                    EngineNo = "EM07-220203",
                    CarID = "CAR-HP-203",
                    ModelCode = "IONIQ-5",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "Xe Ä‘iá»‡n Pin 72.6 kWh cÃ´ng nghá»‡ E-GMP",
                    GPSID = "GPS-VNP-950203",
                    ContractGPS = gps2.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 28),
                    CostGPSStartDate = new DateTime(2025, 5, 1),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 0,
                    ActualCostGPSDate = 31,
                    PriceGPS = 2_400,
                    AmountGPS = 74_400,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "GiÃ¡m sÃ¡t tráº¡ng thÃ¡i sáº¡c Pin vÃ  vá»‹ trÃ­ xe an toÃ n 24/7"
                }
            );
            await db.SaveChangesAsync();

            // 3. Báº£ng kÃª ThÃ¡ng 05/2025 - HTV ÄÃƒ PHÃŠ DUYá»†T Cáº¤P 1 (HTVApproved)
            var gps3 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-002",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-BINHANH",
                ProviderCode = "BAGPS",
                ProviderName = "CÃ´ng ty TNHH PhÃ¡t triá»ƒn CÃ´ng nghá»‡ Äiá»‡n tá»­ BÃ¬nh Anh (BA GPS)",
                TotalVehicles = 3,
                TotalPlanDays = 80,
                TotalDeductDays = 2,
                TotalActualDays = 78,
                AmountTotal = 179_400,
                VATRate = 10.0m,
                UnitPriceVAT = 17_940,
                TotalAmountVAT = 197_340,
                Status = PaymentGPSStatus.HTVApproved,
                HTVSignStatus = GPSSignCAStatus.Pending,
                TCMSSignStatus = GPSSignCAStatus.Pending,
                Appr1By = "TranVanHung_GiamDocVanTai_HTV",
                Appr1DTime = DateTime.Today.AddDays(-2),
                Remark = "Báº£ng kÃª Ä‘á»£t 2 thÃ¡ng 05/2025: Thiáº¿t bá»‹ BA-GPS trÃªn dÃ²ng xe sedan & MPV, chá» TCMS duyá»‡t A2",
                CreatedBy = "VuMinhTu_QuanLyXe",
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.PaymentGPSs.Add(gps3);
            await db.SaveChangesAsync();

            db.PaymentGPSDetails.AddRange(
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps3.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950301",
                    EngineNo = "G4NL-330301",
                    CarID = "CAR-DA-301",
                    ModelCode = "ELANTRA",
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cáº¥p",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "Báº£n cao cáº¥p tháº¿ há»‡ 7",
                    GPSID = "GPS-BA-950301",
                    ContractGPS = gps3.ContractNo,
                    GPSStartDate = new DateTime(2025, 4, 30),
                    CostGPSStartDate = new DateTime(2025, 5, 1),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 0,
                    ActualCostGPSDate = 31,
                    PriceGPS = 2_300,
                    AmountGPS = 71_300,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "Káº¿t ná»‘i há»™p Ä‘en BA-GPS Ä‘áº¡t chuáº©n ká»¹ thuáº­t QCVN 31:2014"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps3.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950302",
                    EngineNo = "G4FL-330302",
                    CarID = "CAR-DA-302",
                    ModelCode = "STARGAZER",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p",
                    SpecCode = "SGZ-1.5L-PREM",
                    SpecDescription = "Báº£n MPV phong cÃ¡ch SUV",
                    GPSID = "GPS-BA-950302",
                    ContractGPS = gps3.ContractNo,
                    GPSStartDate = new DateTime(2025, 5, 4),
                    CostGPSStartDate = new DateTime(2025, 5, 5),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 27,
                    DeductDate = 0,
                    ActualCostGPSDate = 27,
                    PriceGPS = 2_300,
                    AmountGPS = 62_100,
                    Status = PaymentGPSDetailStatus.Approved,
                    Remark = "Truyá»n dá»¯ liá»‡u tá»‘c Ä‘á»™ vÃ  Ä‘á»‹nh vá»‹ mÃ¡y chá»§ á»•n Ä‘á»‹nh"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps3.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950303",
                    EngineNo = "G4FJ-330303",
                    CarID = "CAR-DA-303",
                    ModelCode = "ELANTRA-NLINE",
                    ModelName = "Hyundai Elantra N Line 1.6 Turbo",
                    SpecCode = "ELN-1.6T-NL",
                    SpecDescription = "Báº£n thá»ƒ thao Turbo",
                    GPSID = "GPS-BA-950303",
                    ContractGPS = gps3.ContractNo,
                    GPSStartDate = new DateTime(2025, 5, 9),
                    CostGPSStartDate = new DateTime(2025, 5, 10),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 22,
                    DeductDate = 2,
                    ActualCostGPSDate = 20,
                    PriceGPS = 2_300,
                    AmountGPS = 46_000,
                    Status = PaymentGPSDetailStatus.Adjusted,
                    Remark = "Kháº¥u trá»« 2 ngÃ y xe trÆ°ng bÃ y sá»± kiá»‡n ná»™i bá»™ khÃ´ng di chuyá»ƒn"
                }
            );
            await db.SaveChangesAsync();

            // 4. Báº£ng kÃª ThÃ¡ng 05/2025 - Dá»° THáº¢O (Draft)
            var gps4 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-003",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-VIETTEL-2024",
                ProviderCode = "VIETTEL",
                ProviderName = "Tá»•ng CÃ´ng ty Viá»…n thÃ´ng Viettel (Viettel Telecom)",
                TotalVehicles = 2,
                TotalPlanDays = 34,
                TotalDeductDays = 0,
                TotalActualDays = 34,
                AmountTotal = 85_000,
                VATRate = 10.0m,
                UnitPriceVAT = 8_500,
                TotalAmountVAT = 93_500,
                Status = PaymentGPSStatus.Draft,
                HTVSignStatus = GPSSignCAStatus.Pending,
                TCMSSignStatus = GPSSignCAStatus.Pending,
                Remark = "Dá»± tháº£o báº£ng kÃª thanh toÃ¡n GPS Ä‘á»£t 3 thÃ¡ng 05/2025 - Má»›i kÃ­ch hoáº¡t SIM vÃ  thiáº¿t bá»‹",
                CreatedBy = "VuMinhTu_QuanLyXe",
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.PaymentGPSs.Add(gps4);
            await db.SaveChangesAsync();

            db.PaymentGPSDetails.AddRange(
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps4.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950401",
                    EngineNo = "G4KP-440401",
                    CarID = "CAR-NB-401",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "Báº£n cao cáº¥p nháº¥t 6 chá»— mÃ¢m 21 inch",
                    GPSID = "GPS-VTT-950401",
                    ContractGPS = gps4.ContractNo,
                    GPSStartDate = new DateTime(2025, 5, 14),
                    CostGPSStartDate = new DateTime(2025, 5, 15),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 17,
                    DeductDate = 0,
                    ActualCostGPSDate = 17,
                    PriceGPS = 2_500,
                    AmountGPS = 42_500,
                    Status = PaymentGPSDetailStatus.Pending,
                    Remark = "Äá»‹nh vá»‹ 4G káº¿t ná»‘i há»‡ thá»‘ng viá»…n thÃ´ng thÃ´ng minh Bluelink"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps4.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950402",
                    EngineNo = "G4FJ-440402",
                    CarID = "CAR-NB-402",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng tÄƒng Ã¡p HTRAC",
                    GPSID = "GPS-VTT-950402",
                    ContractGPS = gps4.ContractNo,
                    GPSStartDate = new DateTime(2025, 5, 14),
                    CostGPSStartDate = new DateTime(2025, 5, 15),
                    CostGPSEndDate = new DateTime(2025, 5, 31),
                    PlanCostGPSDate = 17,
                    DeductDate = 0,
                    ActualCostGPSDate = 17,
                    PriceGPS = 2_500,
                    AmountGPS = 42_500,
                    Status = PaymentGPSDetailStatus.Pending,
                    Remark = "CÃ i Ä‘áº·t hÃ ng rÃ o Ä‘iá»‡n tá»­ cáº£nh bÃ¡o rá»i kho tá»•ng Ninh BÃ¬nh"
                }
            );
            await db.SaveChangesAsync();

            // 5. Báº£ng kÃª ThÃ¡ng 03/2025 - ÄÃƒ Há»¦Y (Cancelled)
            var gps5 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202503-099",
                PmtMonth = "2025-03",
                ContractNo = "HD-GPS-BKAV-SMART",
                ProviderCode = "BKAV",
                ProviderName = "Táº­p Ä‘oÃ n CÃ´ng nghá»‡ BKAV - Smart Vehicle IoT Solutions",
                TotalVehicles = 2,
                TotalPlanDays = 62,
                TotalDeductDays = 0,
                TotalActualDays = 62,
                AmountTotal = 161_200,
                VATRate = 10.0m,
                UnitPriceVAT = 16_120,
                TotalAmountVAT = 177_320,
                Status = PaymentGPSStatus.Cancelled,
                HTVSignStatus = GPSSignCAStatus.Pending,
                TCMSSignStatus = GPSSignCAStatus.Pending,
                CancelledAt = DateTime.Today.AddDays(-40),
                RejectReason = "Há»§y báº£ng kÃª do thay Ä‘á»•i phá»¥ lá»¥c sá»‘ 02 Ä‘iá»u chá»‰nh Ä‘Æ¡n giÃ¡ sang nhÃ  máº¡ng Viettel",
                Remark = "Báº£ng kÃª thá»­ nghiá»‡m thiáº¿t bá»‹ BKAV bá»‹ há»§y",
                CreatedBy = "VuMinhTu_QuanLyXe",
                CreatedAt = DateTime.Today.AddDays(-45)
            };
            db.PaymentGPSs.Add(gps5);
            await db.SaveChangesAsync();

            db.PaymentGPSDetails.AddRange(
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps5.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950501",
                    EngineNo = "G4LC-550501",
                    CarID = "CAR-BK-501",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT",
                    SpecCode = "ACC-1.5AT-STD",
                    SpecDescription = "Báº£n tiÃªu chuáº©n",
                    GPSID = "GPS-BKAV-550501",
                    ContractGPS = gps5.ContractNo,
                    GPSStartDate = new DateTime(2025, 3, 1),
                    CostGPSStartDate = new DateTime(2025, 3, 1),
                    CostGPSEndDate = new DateTime(2025, 3, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 0,
                    ActualCostGPSDate = 31,
                    PriceGPS = 2_600,
                    AmountGPS = 80_600,
                    Status = PaymentGPSDetailStatus.Cancelled,
                    Remark = "Há»§y do thay Ä‘á»•i phÆ°Æ¡ng Ã¡n triá»ƒn khai thiáº¿t bá»‹ thá»­ nghiá»‡m"
                },
                new PaymentGPSDetail
                {
                    PaymentGPSId = gps5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentGPSNo = gps5.PaymentGPSNo,
                    VIN = "KMHCT81EPHU950502",
                    EngineNo = "G4LC-550502",
                    CarID = "CAR-BK-502",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT",
                    SpecCode = "ACC-1.5AT-STD",
                    SpecDescription = "Báº£n tiÃªu chuáº©n",
                    GPSID = "GPS-BKAV-550502",
                    ContractGPS = gps5.ContractNo,
                    GPSStartDate = new DateTime(2025, 3, 1),
                    CostGPSStartDate = new DateTime(2025, 3, 1),
                    CostGPSEndDate = new DateTime(2025, 3, 31),
                    PlanCostGPSDate = 31,
                    DeductDate = 0,
                    ActualCostGPSDate = 31,
                    PriceGPS = 2_600,
                    AmountGPS = 80_600,
                    Status = PaymentGPSDetailStatus.Cancelled,
                    Remark = "Há»§y do thay Ä‘á»•i phÆ°Æ¡ng Ã¡n triá»ƒn khai thiáº¿t bá»‹ thá»­ nghiá»‡m"
                }
            );
            await db.SaveChangesAsync();
        }

        // 16. Dá»¯ liá»‡u máº«u Báº£ng tÃ­nh Chi phÃ­ TÃ i chÃ­nh (CPTC) & Chiáº¿t kháº¥u Thanh toÃ¡n TCG (CKTT) cho Äáº¡i lÃ½ (DMS40_FnExp_Calc_FnExp_PmDc)
        if (!await db.FnExpStatements.AnyAsync(s => s.OrgId == TenantContext.DefaultOrgId))
        {
            // Statement 1: ÄÃƒ QUYáº¾T TOÃN (Settled) - Hyundai HÃ  ÄÃ´ng (Ká»³ 04/2025)
            var fn1 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202504-HADONG-01",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai HÃ  ÄÃ´ng",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT TCG thÃ¡ng 04/2025 - Hyundai HÃ  ÄÃ´ng",
                TermFrom = new DateTime(2025, 4, 1),
                TermTo = new DateTime(2025, 4, 30),
                TermPrevFrom = new DateTime(2025, 3, 1),
                TermPrevTo = new DateTime(2025, 3, 31),
                FnExpPercent = 8.5m,
                PmtDsTCGPercent = 1.2m,
                TotalVehicles = 3,
                TotalFnDepositAmount = 6_160_552,
                TotalFnGrtAmount = 19_991_351,
                TotalFnAmount = 26_151_903,
                TotalPDAmount = 1_614_365,
                TotalSettlementAmount = 27_766_268,
                Status = FinancialExpenseStatus.Settled,
                DlrSignStatus = FnExpSignCAStatus.Signed,
                DlrSignUser = "NguyenVanThanh_GD_DaiLyHaDong",
                DlrSignDTime = new DateTime(2025, 5, 3, 10, 30, 0),
                HTCSignStatus = FnExpSignCAStatus.Signed,
                HTCSignUser = "LeQuangHuy_PhoTGDTaiChinh_HTC",
                HTCSignDTime = new DateTime(2025, 5, 5, 14, 15, 0),
                DlrAppr1By = "TranThiLan_KeToanTruongHaDong",
                DlrAppr1DTime = new DateTime(2025, 5, 2, 9, 0, 0),
                HTCAppr1By = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                HTCAppr1DTime = new DateTime(2025, 5, 4, 16, 20, 0),
                SettledBy = "PhamThiMai_KeToanThanhToan",
                SettledAt = new DateTime(2025, 5, 6, 11, 45, 0),
                BankTxnRef = "UNC-FNEXP-202504-HADONG-01",
                FilePathFnExp = "/docs/ca/CAN-202504-HADONG-01-FNEXP.pdf",
                FilePathPmtDc = "/docs/ca/CAN-202504-HADONG-01-PMTDC.pdf",
                Remark = "Quyáº¿t toÃ¡n hoÃ n táº¥t chuyá»ƒn tiá»n há»— trá»£ lÃ£i suáº¥t vÃ  chiáº¿t kháº¥u TCG ká»³ 04/2025 qua Vietcombank",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 5, 1, 8, 30, 0)
            };
            db.FnExpStatements.Add(fn1);
            await db.SaveChangesAsync();

            db.FnExpDetails.AddRange(
                new FinancialExpenseDetail
                {
                    StatementId = fn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn1.CaNo,
                    CarId = "CAR-FN-01",
                    VIN = "KMHCT81EPHU880101",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "Báº£n cao cáº¥p 6 chá»— HTRAC",
                    ColorName = "Tráº¯ng ngá»c trai",
                    SOCode = "SO-202504-HD01",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 1_365_000_000,
                    SodApprovedDate = new DateTime(2025, 3, 20),
                    SodDepositDutyEndDate = new DateTime(2025, 3, 25),
                    TotalCompletedDate = new DateTime(2025, 4, 20),
                    DateStart = new DateTime(2025, 3, 28),
                    DateEnd = new DateTime(2025, 4, 30),
                    TermActual = 33,
                    FnDepositCountDate = 28,
                    FnDepositAmount = 1_353_625,
                    FnGrtCountDate = 25,
                    FnGrtAmount = 6_848_698,
                    FnTotalAmount = 8_202_323,
                    PDCountDate = 10,
                    PDAmount = 386_750,
                    CarTotalSettlement = 8_589_073,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Äáº¡i lÃ½ ná»™p cá»c vÃ  thanh toÃ¡n trÆ°á»›c háº¡n báº£o lÃ£nh 10 ngÃ y"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn1.CaNo,
                    CarId = "CAR-FN-02",
                    VIN = "KMHCT81EPHU880102",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng tÄƒng Ã¡p HTRAC",
                    ColorName = "Äen huyá»n bÃ­",
                    SOCode = "SO-202504-HD02",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 989_000_000,
                    SodApprovedDate = new DateTime(2025, 3, 22),
                    SodDepositDutyEndDate = new DateTime(2025, 3, 27),
                    TotalCompletedDate = new DateTime(2025, 4, 17),
                    DateStart = new DateTime(2025, 3, 30),
                    DateEnd = new DateTime(2025, 4, 30),
                    TermActual = 31,
                    FnDepositCountDate = 25,
                    FnDepositAmount = 875_677,
                    FnGrtCountDate = 20,
                    FnGrtAmount = 3_969_736,
                    FnTotalAmount = 4_845_413,
                    PDCountDate = 13,
                    PDAmount = 364_282,
                    CarTotalSettlement = 5_209_695,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "HÆ°á»Ÿng chiáº¿t kháº¥u thanh toÃ¡n sá»›m 13 ngÃ y"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn1.CaNo,
                    CarId = "CAR-FN-03",
                    VIN = "KMHCT81EPHU880107",
                    ModelCode = "STARIA-LOUNGE",
                    ModelName = "Hyundai Staria Lounge 9 Seats CBU",
                    SpecCode = "STA-2.2D-CBU9",
                    SpecDescription = "Báº£n nháº­p kháº©u nguyÃªn chiáº¿c CBU",
                    ColorName = "Äen ngá»c bÃ­ch",
                    SOCode = "SO-202504-HD03",
                    AssemblyType = VehicleAssemblyType.CBU,
                    UnitPriceActual = 1_850_000_000,
                    SodApprovedDate = new DateTime(2025, 3, 15),
                    SodDepositDutyEndDate = new DateTime(2025, 3, 22),
                    TotalCompletedDate = new DateTime(2025, 4, 10),
                    DateStart = new DateTime(2025, 3, 25),
                    DateEnd = new DateTime(2025, 4, 30),
                    TermActual = 36,
                    FnDepositCountDate = 30,
                    FnDepositAmount = 3_931_250,
                    FnGrtCountDate = 30,
                    FnGrtAmount = 9_172_917,
                    FnTotalAmount = 13_104_167,
                    PDCountDate = 20,
                    PDAmount = 863_333,
                    CarTotalSettlement = 13_967_500,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Xe CBU Ã¡p dá»¥ng tá»· lá»‡ cá»c 30% vÃ  báº£o lÃ£nh 70%, chiáº¿t kháº¥u sá»›m 20 ngÃ y"
                }
            );
            await db.SaveChangesAsync();

            // Statement 2: ÄÃƒ KÃ Sá» CA HTC (HTCSigned) - Hyundai ÄÃ´ng Anh (Ká»³ 05/2025)
            var fn2 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-DONGANH-01",
                DealerCode = "DLR-DONGANH",
                DealerName = "Hyundai ÄÃ´ng Anh",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT TCG thÃ¡ng 05/2025 - Hyundai ÄÃ´ng Anh",
                TermFrom = new DateTime(2025, 5, 1),
                TermTo = new DateTime(2025, 5, 31),
                TermPrevFrom = new DateTime(2025, 4, 1),
                TermPrevTo = new DateTime(2025, 4, 30),
                FnExpPercent = 8.5m,
                PmtDsTCGPercent = 1.5m,
                TotalVehicles = 2,
                TotalFnDepositAmount = 3_834_427,
                TotalFnGrtAmount = 14_910_792,
                TotalFnAmount = 18_745_219,
                TotalPDAmount = 1_657_669,
                TotalSettlementAmount = 20_402_888,
                Status = FinancialExpenseStatus.HTCSigned,
                DlrSignStatus = FnExpSignCAStatus.Signed,
                DlrSignUser = "VuDinhTuan_GD_DaiLyDongAnh",
                DlrSignDTime = new DateTime(2025, 6, 2, 11, 20, 0),
                HTCSignStatus = FnExpSignCAStatus.Signed,
                HTCSignUser = "LeQuangHuy_PhoTGDTaiChinh_HTC",
                HTCSignDTime = new DateTime(2025, 6, 4, 15, 30, 0),
                DlrAppr1By = "TranDinhQuy_KeToanDongAnh",
                DlrAppr1DTime = new DateTime(2025, 6, 1, 14, 0, 0),
                HTCAppr1By = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                HTCAppr1DTime = new DateTime(2025, 6, 3, 10, 0, 0),
                FilePathFnExp = "/docs/ca/CAN-202505-DONGANH-01-FNEXP.pdf",
                FilePathPmtDc = "/docs/ca/CAN-202505-DONGANH-01-PMTDC.pdf",
                Remark = "ÄÃ£ hoÃ n táº¥t kÃ½ sá»‘ CA 2 cáº¥p, káº¿ toÃ¡n chuáº©n bá»‹ láº­p UNC thanh toÃ¡n qua BIDV",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 6, 1, 9, 0, 0)
            };
            db.FnExpStatements.Add(fn2);
            await db.SaveChangesAsync();

            db.FnExpDetails.AddRange(
                new FinancialExpenseDetail
                {
                    StatementId = fn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn2.CaNo,
                    CarId = "CAR-FN-04",
                    VIN = "KMHCT81EPHU880103",
                    ModelCode = "PALISADE-PREM",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PAL-2.2D-PRE7",
                    SpecDescription = "Báº£n SUV 7 chá»— mÃ¡y dáº§u",
                    ColorName = "Xanh lá»¥c báº£o",
                    SOCode = "SO-202505-DA01",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 1_589_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 10),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 15),
                    TotalCompletedDate = new DateTime(2025, 5, 22),
                    DateStart = new DateTime(2025, 4, 20),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 41,
                    FnDepositCountDate = 30,
                    FnDepositAmount = 1_688_313,
                    FnGrtCountDate = 31,
                    FnGrtAmount = 9_908_647,
                    FnTotalAmount = 11_596_960,
                    PDCountDate = 9,
                    PDAmount = 506_494,
                    CarTotalSettlement = 12_103_454,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Xe dÃ²ng cao cáº¥p Palisade, chiáº¿t kháº¥u sá»›m 9 ngÃ y"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn2.CaNo,
                    CarId = "CAR-FN-05",
                    VIN = "KMHCT81EPHU880105",
                    ModelCode = "CUSTIN-TURBO",
                    ModelName = "Hyundai Custin 2.0T Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Báº£n MPV cá»­a trÆ°á»£t Ä‘iá»‡n 7 chá»—",
                    ColorName = "Tráº¯ng tuyáº¿t",
                    SOCode = "SO-202505-DA02",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 974_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 22),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 27),
                    TotalCompletedDate = new DateTime(2025, 5, 19),
                    DateStart = new DateTime(2025, 5, 2),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 29,
                    FnDepositCountDate = 22,
                    FnDepositAmount = 2_146_114,
                    FnGrtCountDate = 17,
                    FnGrtAmount = 5_002_145,
                    FnTotalAmount = 7_148_259,
                    PDCountDate = 12,
                    PDAmount = 1_151_175,
                    CarTotalSettlement = 8_299_434,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "DÃ²ng MPV gia Ä‘Ã¬nh Custin tháº¿ há»‡ má»›i"
                }
            );
            await db.SaveChangesAsync();

            // Statement 3: Äáº I LÃ ÄÃƒ KÃ Sá» CA (DlrSigned) - Hyundai SÃ i GÃ²n 1S (Ká»³ 05/2025)
            var fn3 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-SAIGON-01",
                DealerCode = "DLR-SAIGON",
                DealerName = "Hyundai SÃ i GÃ²n 1S",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT TCG thÃ¡ng 05/2025 - Hyundai SÃ i GÃ²n 1S",
                TermFrom = new DateTime(2025, 5, 1),
                TermTo = new DateTime(2025, 5, 31),
                TermPrevFrom = new DateTime(2025, 4, 1),
                TermPrevTo = new DateTime(2025, 4, 30),
                FnExpPercent = 8.8m,
                PmtDsTCGPercent = 1.2m,
                TotalVehicles = 2,
                TotalFnDepositAmount = 2_150_230,
                TotalFnGrtAmount = 9_384_488,
                TotalFnAmount = 11_534_718,
                TotalPDAmount = 451_290,
                TotalSettlementAmount = 11_986_008,
                Status = FinancialExpenseStatus.DlrSigned,
                DlrSignStatus = FnExpSignCAStatus.Signed,
                DlrSignUser = "PhamMinhDuc_GD_DaiLySaiGon",
                DlrSignDTime = new DateTime(2025, 6, 3, 16, 45, 0),
                HTCSignStatus = FnExpSignCAStatus.Pending,
                DlrAppr1By = "NguyenThiMai_KeToanSaiGon",
                DlrAppr1DTime = new DateTime(2025, 6, 2, 10, 15, 0),
                FilePathFnExp = "/docs/ca/CAN-202505-SAIGON-01-FNEXP.pdf",
                Remark = "Äáº¡i lÃ½ Ä‘Ã£ kÃ½ sá»‘ CA xÃ¡c nháº­n sá»‘ liá»‡u, chá» chuyÃªn viÃªn QLPP HTC tháº©m Ä‘á»‹nh A1",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 6, 1, 10, 30, 0)
            };
            db.FnExpStatements.Add(fn3);
            await db.SaveChangesAsync();

            db.FnExpDetails.AddRange(
                new FinancialExpenseDetail
                {
                    StatementId = fn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn3.CaNo,
                    CarId = "CAR-FN-06",
                    VIN = "KMHCT81EPHU880104",
                    ModelCode = "IONIQ5-PREM",
                    ModelName = "Hyundai Ioniq 5 Prestige EV",
                    SpecCode = "IQ5-EV-PREM",
                    SpecDescription = "Báº£n xe Ä‘iá»‡n thÃ´ng minh E-GMP",
                    ColorName = "Báº¡c Ã¡nh kim",
                    SOCode = "SO-202505-SG01",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 1_450_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 20),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 25),
                    TotalCompletedDate = new DateTime(2025, 5, 25),
                    DateStart = new DateTime(2025, 4, 30),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 31,
                    FnDepositCountDate = 26,
                    FnDepositAmount = 1_382_367,
                    FnGrtCountDate = 25,
                    FnGrtAmount = 7_534_375,
                    FnTotalAmount = 8_916_742,
                    PDCountDate = 6,
                    PDAmount = 246_500,
                    CarTotalSettlement = 9_163_242,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "DÃ²ng xe thuáº§n Ä‘iá»‡n EV há»— trá»£ lÃ£i suáº¥t kÃ­ch cáº§u xanh"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn3.CaNo,
                    CarId = "CAR-FN-07",
                    VIN = "KMHCT81EPHU880106",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Báº£n SUV cá»¡ B SmartSense",
                    ColorName = "Äá» máº­n",
                    SOCode = "SO-202505-SG02",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 699_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 25),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 30),
                    TotalCompletedDate = new DateTime(2025, 5, 26),
                    DateStart = new DateTime(2025, 5, 5),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 26,
                    FnDepositCountDate = 20,
                    FnDepositAmount = 767_863,
                    FnGrtCountDate = 21,
                    FnGrtAmount = 1_850_113,
                    FnTotalAmount = 2_617_976,
                    PDCountDate = 5,
                    PDAmount = 204_790,
                    CarTotalSettlement = 2_822_766,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "SUV Creta phiÃªn báº£n cao cáº¥p SmartSense"
                }
            );
            await db.SaveChangesAsync();

            // Statement 4: HTC ÄÃƒ THáº¨M Äá»ŠNH DUYá»†T Cáº¤P 1 (HTCApproved1) - Hyundai Pháº¡m VÄƒn Äá»“ng (Ká»³ 05/2025)
            var fn4 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-PVD-01",
                DealerCode = "DLR-PHAMVANDONG",
                DealerName = "Hyundai Pháº¡m VÄƒn Äá»“ng",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT TCG thÃ¡ng 05/2025 - Hyundai Pháº¡m VÄƒn Äá»“ng",
                TermFrom = new DateTime(2025, 5, 1),
                TermTo = new DateTime(2025, 5, 31),
                TermPrevFrom = new DateTime(2025, 4, 1),
                TermPrevTo = new DateTime(2025, 4, 30),
                FnExpPercent = 8.5m,
                PmtDsTCGPercent = 1.2m,
                TotalVehicles = 2,
                TotalFnDepositAmount = 1_473_806,
                TotalFnGrtAmount = 5_929_625,
                TotalFnAmount = 7_403_431,
                TotalPDAmount = 569_580,
                TotalSettlementAmount = 7_973_011,
                Status = FinancialExpenseStatus.HTCApproved1,
                DlrSignStatus = FnExpSignCAStatus.Signed,
                DlrSignUser = "HoangVanCuong_GD_DaiLyPVD",
                DlrSignDTime = new DateTime(2025, 6, 2, 9, 30, 0),
                HTCSignStatus = FnExpSignCAStatus.Pending,
                DlrAppr1By = "DoThiHoa_KeToanPVD",
                DlrAppr1DTime = new DateTime(2025, 6, 1, 15, 0, 0),
                HTCAppr1By = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                HTCAppr1DTime = new DateTime(2025, 6, 4, 9, 0, 0),
                Remark = "ChuyÃªn viÃªn HTC Ä‘Ã£ tháº©m Ä‘á»‹nh khá»›p há»“ sÆ¡ báº£o lÃ£nh VietinBank, chá» LÃ£nh Ä‘áº¡o HTC kÃ½ sá»‘ CA",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 6, 1, 11, 0, 0)
            };
            db.FnExpStatements.Add(fn4);
            await db.SaveChangesAsync();

            db.FnExpDetails.AddRange(
                new FinancialExpenseDetail
                {
                    StatementId = fn4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn4.CaNo,
                    CarId = "CAR-FN-08",
                    VIN = "KMHCT81EPHU880108",
                    ModelCode = "ELANTRA-PREM",
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cáº¥p",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "Báº£n sedan thá»ƒ thao phÃ¢n khÃºc C",
                    ColorName = "Äá» Ä‘Ã´",
                    SOCode = "SO-202505-PVD01",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 729_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 12),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 18),
                    TotalCompletedDate = new DateTime(2025, 5, 23),
                    DateStart = new DateTime(2025, 4, 25),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 36,
                    FnDepositCountDate = 24,
                    FnDepositAmount = 619_650,
                    FnGrtCountDate = 20,
                    FnGrtAmount = 2_926_288,
                    FnTotalAmount = 3_545_938,
                    PDCountDate = 8,
                    PDAmount = 165_240,
                    CarTotalSettlement = 3_711_178,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "DÃ²ng sedan Elantra thanh toÃ¡n sá»›m 8 ngÃ y"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn4.CaNo,
                    CarId = "CAR-FN-09",
                    VIN = "KMHCT81EPHU880109",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5AT-SPEC",
                    SpecDescription = "Báº£n sedan B bÃ¡n cháº¡y",
                    ColorName = "Báº¡c",
                    SOCode = "SO-202505-PVD02",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 569_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 15),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 20),
                    TotalCompletedDate = new DateTime(2025, 5, 17),
                    DateStart = new DateTime(2025, 4, 28),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 33,
                    FnDepositCountDate = 26,
                    FnDepositAmount = 854_156,
                    FnGrtCountDate = 17,
                    FnGrtAmount = 3_003_337,
                    FnTotalAmount = 3_857_493,
                    PDCountDate = 14,
                    PDAmount = 404_340,
                    CarTotalSettlement = 4_261_833,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Accent thanh toÃ¡n dá»©t Ä‘iá»ƒm trÆ°á»›c háº¡n 14 ngÃ y"
                }
            );
            await db.SaveChangesAsync();

            // Statement 5: Dá»° THáº¢O Má»šI (Draft) - Hyundai ÄÃ  Náºµng (Ká»³ 05/2025)
            var fn5 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-DANANG-01",
                DealerCode = "DLR-DANANG",
                DealerName = "Hyundai SÃ´ng HÃ n - ÄÃ  Náºµng",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT TCG thÃ¡ng 05/2025 - Hyundai SÃ´ng HÃ n",
                TermFrom = new DateTime(2025, 5, 1),
                TermTo = new DateTime(2025, 5, 31),
                TermPrevFrom = new DateTime(2025, 4, 1),
                TermPrevTo = new DateTime(2025, 4, 30),
                FnExpPercent = 8.5m,
                PmtDsTCGPercent = 1.0m,
                TotalVehicles = 2,
                TotalFnDepositAmount = 1_047_656,
                TotalFnGrtAmount = 4_106_849,
                TotalFnAmount = 5_154_505,
                TotalPDAmount = 266_550,
                TotalSettlementAmount = 5_421_055,
                Status = FinancialExpenseStatus.Draft,
                DlrSignStatus = FnExpSignCAStatus.Pending,
                HTCSignStatus = FnExpSignCAStatus.Pending,
                Remark = "Dá»± tháº£o báº£ng tÃ­nh chi phÃ­ tÃ i chÃ­nh má»›i láº­p, Ä‘ang gá»­i káº¿ toÃ¡n Ä‘áº¡i lÃ½ tháº©m Ä‘á»‹nh",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = DateTime.Now.AddDays(-1)
            };
            db.FnExpStatements.Add(fn5);
            await db.SaveChangesAsync();

            db.FnExpDetails.AddRange(
                new FinancialExpenseDetail
                {
                    StatementId = fn5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn5.CaNo,
                    CarId = "CAR-FN-10",
                    VIN = "KMHCT81EPHU880110",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Báº£n SUV cá»¡ B SmartSense",
                    ColorName = "Tráº¯ng",
                    SOCode = "SO-202505-DN01",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 699_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 25),
                    SodDepositDutyEndDate = new DateTime(2025, 4, 30),
                    TotalCompletedDate = new DateTime(2025, 5, 26),
                    DateStart = new DateTime(2025, 5, 5),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 26,
                    FnDepositCountDate = 20,
                    FnDepositAmount = 742_688,
                    FnGrtCountDate = 18,
                    FnGrtAmount = 2_525_139,
                    FnTotalAmount = 3_267_827,
                    PDCountDate = 5,
                    PDAmount = 137_535,
                    CarTotalSettlement = 3_405_362,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Dá»± tháº£o dÃ²ng xe Creta"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn5.CaNo,
                    CarId = "CAR-FN-11",
                    VIN = "KMHCT81EPHU880111",
                    ModelCode = "ACCENT-STD",
                    ModelName = "Hyundai Accent 1.5 MT TiÃªu Chuáº©n",
                    SpecCode = "ACC-1.5MT-STD",
                    SpecDescription = "Báº£n sá»‘ sÃ n tiáº¿t kiá»‡m",
                    ColorName = "Äen",
                    SOCode = "SO-202505-DN02",
                    AssemblyType = VehicleAssemblyType.CKD,
                    UnitPriceActual = 439_000_000,
                    SodApprovedDate = new DateTime(2025, 4, 28),
                    SodDepositDutyEndDate = new DateTime(2025, 5, 3),
                    TotalCompletedDate = new DateTime(2025, 5, 21),
                    DateStart = new DateTime(2025, 5, 8),
                    DateEnd = new DateTime(2025, 5, 31),
                    TermActual = 23,
                    FnDepositCountDate = 17,
                    FnDepositAmount = 304_968,
                    FnGrtCountDate = 17,
                    FnGrtAmount = 1_581_710,
                    FnTotalAmount = 1_886_678,
                    PDCountDate = 10,
                    PDAmount = 129_015,
                    CarTotalSettlement = 2_015_693,
                    Status = FinancialExpenseDetailStatus.Active,
                    Remark = "Dá»± tháº£o dÃ²ng xe Accent"
                }
            );
            await db.SaveChangesAsync();

            // Statement 6: ÄÃƒ Há»¦Y (Cancelled) - Hyundai HÃ  ÄÃ´ng (Ká»³ 03/2025)
            var fn6 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202503-HADONG-09",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai HÃ  ÄÃ´ng",
                CAName = "Báº£ng tÃ­nh CPTC & CKTT thÃ¡ng 03/2025 (Há»§y lÃ m láº¡i)",
                TermFrom = new DateTime(2025, 3, 1),
                TermTo = new DateTime(2025, 3, 31),
                TermPrevFrom = new DateTime(2025, 2, 1),
                TermPrevTo = new DateTime(2025, 2, 28),
                FnExpPercent = 8.5m,
                PmtDsTCGPercent = 1.2m,
                TotalVehicles = 1,
                TotalFnDepositAmount = 1_000_000,
                TotalFnGrtAmount = 4_000_000,
                TotalFnAmount = 5_000_000,
                TotalPDAmount = 300_000,
                TotalSettlementAmount = 5_300_000,
                Status = FinancialExpenseStatus.Cancelled,
                DlrSignStatus = FnExpSignCAStatus.Pending,
                HTCSignStatus = FnExpSignCAStatus.Pending,
                CancelBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CancelDTime = new DateTime(2025, 4, 5, 11, 0, 0),
                CancelReason = "Há»§y do Ä‘áº¡i lÃ½ Ä‘iá»u chá»‰nh phá»¥ lá»¥c thay Ä‘á»•i ngÃ y ná»™p cá»c Ä‘Æ¡n hÃ ng SO",
                Remark = "Báº£ng tÃ­nh bá»‹ há»§y lÃ m láº¡i Ä‘á»£t khÃ¡c",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 4, 2, 14, 0, 0)
            };
            db.FnExpStatements.Add(fn6);
            await db.SaveChangesAsync();
        }

        // 17. Dá»¯ liá»‡u máº«u Há»“ sÆ¡ Äá» nghá»‹ Giao dá»‹ch NgÃ¢n hÃ ng & TÃ i trá»£ Vá»‘n Vay / Báº£o lÃ£nh Äáº¡i lÃ½ (RQ_BankingTransactions)
        if (!await db.DisbursementRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Há»“ sÆ¡ GNTT VPBank ÄÃƒ HOÃ€N Táº¤T GIáº¢I NGÃ‚N (Completed / Disbursed) - Hyundai Giáº£i PhÃ³ng
            var req1 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-001",
                TransType = BankingTransType.GNTT,
                DealerCode = "HYUNDAI-GP",
                DealerName = "Hyundai Giáº£i PhÃ³ng",
                BizResNumber = "0102839102-001",
                BankCode = "VPBANK",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                PaymentAccount = "108899882991",
                PaymentBankName = "VPBank - CN ThÄƒng Long",
                ReceivingUnit = "CÃ”NG TY Cá»” PHáº¦N LIÃŠN DOANH Ã” TÃ” HYUNDAI THÃ€NH CÃ”NG VIá»†T NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Äá»‘ng Äa",
                TotalCars = 9,
                TotalContractAmount = 10_781_000_000,
                TotalDisbursementAmount = 8_822_600_000,
                ActualDisbursedAmount = 8_822_600_000,
                Status = BankingTransStatus.Completed,
                BankStatus = BankingTransBankStatus.Disbursed,
                RefBankCode = "VPB-LN-20250510-001",
                BankRemark = "Há»™i Ä‘á»“ng tÃ­n dá»¥ng VPBank Ä‘Ã£ phÃª duyá»‡t vÃ  giáº£i ngÃ¢n thÃ nh cÃ´ng 8.822.600.000 VND theo Kháº¿ Æ°á»›c nháº­n ná»£ LD-VPB-20250510-001.",
                LDNo = "LD-VPB-20250510-001",
                DisbursementDate = DateTime.Today.AddDays(-5),
                DisbursementTerm = "03 thÃ¡ng",
                DisbursementInterestRate = 8.5m,
                FirstInterestPmtDate = DateTime.Today.AddDays(25),
                LoanLimit = 20_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-7),
                CompletedAt = DateTime.Today.AddDays(-5),
                Remark = "Äá» nghá»‹ giáº£i ngÃ¢n vá»‘n lÆ°u Ä‘á»™ng mua xe lÃ´ thÃ¡ng 05/2025 theo HÄNT-2025/HTC-GP",
                CreatedBy = "VuMinhTu_TinDungGP",
                CreatedAt = DateTime.Today.AddDays(-8)
            };
            db.DisbursementRequests.Add(req1);
            await db.SaveChangesAsync();

            db.DisbursementDetails.AddRange(
                new BankingDisbursementDetail
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DlrCtrNo = "PLHD-2025-05/GP01",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "Báº£n cao cáº¥p 6 chá»— HTRAC",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-10),
                    PrincipalContractNo = "HDNT-2025/HTC-GP",
                    PrincipalContractDate = new DateTime(2025, 1, 10),
                    DeliveryDate = DateTime.Today.AddDays(5),
                    Qty = 5,
                    UnitPrice = 1_365_000_000,
                    TotalAmount = 6_825_000_000,
                    LtvRate = 80.0m,
                    DisbursementAmount = 5_460_000_000,
                    Remark = "LÃ´ 5 xe Santa Fe giao tuáº§n 2 thÃ¡ng 5"
                },
                new BankingDisbursementDetail
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DlrCtrNo = "PLHD-2025-05/GP02",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo",
                    SpecCode = "TUC-1.6T-PREM",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng tÄƒng Ã¡p HTRAC",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-9),
                    PrincipalContractNo = "HDNT-2025/HTC-GP",
                    PrincipalContractDate = new DateTime(2025, 1, 10),
                    DeliveryDate = DateTime.Today.AddDays(7),
                    Qty = 4,
                    UnitPrice = 989_000_000,
                    TotalAmount = 3_956_000_000,
                    LtvRate = 85.0m,
                    DisbursementAmount = 3_362_600_000,
                    Remark = "LÃ´ 4 xe Tucson Turbo phá»¥c vá»¥ trÆ°ng bÃ y & bÃ¡n láº»"
                }
            );

            db.DisbursementFiles.AddRange(
                new BankingDisbursementFile
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DocType = BankFileDocumentType.DeNghiVay,
                    FileName = $"DeNghiGiaoDich_{req1.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req1.TransNo}/DeNghiGiaoDich_{req1.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Nguyen Van Thang - Tong Giam Doc GP",
                    SignedAt = DateTime.Today.AddDays(-7),
                    CertSerialNumber = "54018899AACC4520",
                    UploadDate = DateTime.Today.AddDays(-8)
                },
                new BankingDisbursementFile
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DocType = BankFileDocumentType.PhuLucHopDong,
                    FileName = "PhuLucHopDong_PLHD-2025-05-GP01.pdf",
                    FilePath = $"/storage/banking/{req1.TransNo}/PhuLucHopDong_PLHD-2025-05-GP01.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Nguyen Van Thang - Tong Giam Doc GP",
                    SignedAt = DateTime.Today.AddDays(-7),
                    CertSerialNumber = "54018899AACC4520",
                    UploadDate = DateTime.Today.AddDays(-8)
                },
                new BankingDisbursementFile
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DocType = BankFileDocumentType.DangKyKinhDoanh,
                    FileName = "GPKD_BCTC_HYUNDAI-GP.pdf",
                    FilePath = $"/storage/banking/{req1.TransNo}/GPKD_BCTC_HYUNDAI-GP.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Nguyen Van Thang - Tong Giam Doc GP",
                    SignedAt = DateTime.Today.AddDays(-7),
                    CertSerialNumber = "54018899AACC4520",
                    UploadDate = DateTime.Today.AddDays(-8)
                },
                new BankingDisbursementFile
                {
                    RequestId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req1.TransNo,
                    DocType = BankFileDocumentType.CamKetTraNo,
                    FileName = $"CamKetTraNo_{req1.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req1.TransNo}/CamKetTraNo_{req1.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Nguyen Van Thang - Tong Giam Doc GP",
                    SignedAt = DateTime.Today.AddDays(-7),
                    CertSerialNumber = "54018899AACC4520",
                    UploadDate = DateTime.Today.AddDays(-8)
                }
            );
            await db.SaveChangesAsync();

            // 2. Há»“ sÆ¡ PhÃ¡t hÃ nh ThÆ° báº£o lÃ£nh má»Ÿ L/C (PhatHanhBLLC) VietinBank ÄANG CHá»œ KÃ Sá» CA (Processing / RequireSignCA) - Hyundai Thanh XuÃ¢n
            var req2 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-002",
                TransType = BankingTransType.PhatHanhBLLC,
                DealerCode = "HYUNDAI-TX",
                DealerName = "Hyundai Thanh XuÃ¢n",
                BizResNumber = "0103774819-002",
                BankCode = "VIETINBANK",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                PaymentAccount = "118000992817",
                PaymentBankName = "VietinBank - CN Thanh XuÃ¢n",
                ReceivingUnit = "CÃ”NG TY Cá»” PHáº¦N LIÃŠN DOANH Ã” TÃ” HYUNDAI THÃ€NH CÃ”NG VIá»†T NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Äá»‘ng Äa",
                TotalCars = 5,
                TotalContractAmount = 7_528_000_000,
                TotalDisbursementAmount = 5_804_900_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Processing,
                BankStatus = BankingTransBankStatus.RequireSignCA,
                RefBankCode = "CTG-LN-20250512-002",
                BankRemark = "Há»“ sÆ¡ tÃ­n dá»¥ng Ä‘Ã£ Ä‘Æ°á»£c phÃª duyá»‡t sÆ¡ bá»™. Äá» nghá»‹ Ä‘áº¡i lÃ½ kÃ½ sá»‘ CA trÃªn ThÆ° cam káº¿t tráº£ ná»£ Ä‘á»ƒ Há»™i sá»Ÿ phÃ¡t hÃ nh thÆ° báº£o lÃ£nh.",
                DisbursementTerm = "45 ngÃ y",
                DisbursementInterestRate = 0m,
                GrtTerm = "45 ngÃ y",
                GrtFee = 11_609_800,
                LoanLimit = 15_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-3),
                Remark = "Äá» nghá»‹ phÃ¡t hÃ nh ThÆ° báº£o lÃ£nh thanh toÃ¡n má»Ÿ L/C mua lÃ´ xe Ä‘iá»‡n Ioniq 5 vÃ  Palisade",
                CreatedBy = "TranThiLan_KeToanTX",
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.DisbursementRequests.Add(req2);
            await db.SaveChangesAsync();

            db.DisbursementDetails.AddRange(
                new BankingDisbursementDetail
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DlrCtrNo = "PLHD-2025-05/TX01",
                    ModelCode = "IONIQ5-PREM",
                    ModelName = "Hyundai Ioniq 5 Prestige EV",
                    SpecCode = "IQ5-EV-PREM",
                    SpecDescription = "Báº£n xe Ä‘iá»‡n thÃ´ng minh E-GMP",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-5),
                    PrincipalContractNo = "HDNT-2025/HTC-TX",
                    PrincipalContractDate = new DateTime(2025, 1, 15),
                    DeliveryDate = DateTime.Today.AddDays(15),
                    Qty = 3,
                    UnitPrice = 1_450_000_000,
                    TotalAmount = 4_350_000_000,
                    LtvRate = 75.0m,
                    DisbursementAmount = 3_262_500_000,
                    Remark = "3 xe Ä‘iá»‡n Ioniq 5 cao cáº¥p"
                },
                new BankingDisbursementDetail
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DlrCtrNo = "PLHD-2025-05/TX02",
                    ModelCode = "PALISADE-PREM",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PAL-2.2D-PREM",
                    SpecDescription = "Báº£n SUV cá»¡ lá»›n mÃ¡y dáº§u 7 chá»—",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-4),
                    PrincipalContractNo = "HDNT-2025/HTC-TX",
                    PrincipalContractDate = new DateTime(2025, 1, 15),
                    DeliveryDate = DateTime.Today.AddDays(18),
                    Qty = 2,
                    UnitPrice = 1_589_000_000,
                    TotalAmount = 3_178_000_000,
                    LtvRate = 80.0m,
                    DisbursementAmount = 2_542_400_000,
                    Remark = "2 xe Palisade SUV Flagship"
                }
            );

            db.DisbursementFiles.AddRange(
                new BankingDisbursementFile
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DocType = BankFileDocumentType.DeNghiVay,
                    FileName = $"DeNghiPhatHanhBL_{req2.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req2.TransNo}/DeNghiPhatHanhBL_{req2.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Le Hoang Nam - Giam Doc TX",
                    SignedAt = DateTime.Today.AddDays(-3),
                    CertSerialNumber = "48197722EE990111",
                    UploadDate = DateTime.Today.AddDays(-4)
                },
                new BankingDisbursementFile
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DocType = BankFileDocumentType.PhuLucHopDong,
                    FileName = "PhuLucHopDong_PLHD-2025-05-TX01.pdf",
                    FilePath = $"/storage/banking/{req2.TransNo}/PhuLucHopDong_PLHD-2025-05-TX01.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Le Hoang Nam - Giam Doc TX",
                    SignedAt = DateTime.Today.AddDays(-3),
                    CertSerialNumber = "48197722EE990111",
                    UploadDate = DateTime.Today.AddDays(-4)
                },
                new BankingDisbursementFile
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DocType = BankFileDocumentType.DangKyKinhDoanh,
                    FileName = "GPKD_BCTC_HYUNDAI-TX.pdf",
                    FilePath = $"/storage/banking/{req2.TransNo}/GPKD_BCTC_HYUNDAI-TX.pdf",
                    SignStatus = BankFileSignStatus.Pending,
                    UploadDate = DateTime.Today.AddDays(-4)
                },
                new BankingDisbursementFile
                {
                    RequestId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req2.TransNo,
                    DocType = BankFileDocumentType.CamKetTraNo,
                    FileName = $"CamKetTraNo_{req2.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req2.TransNo}/CamKetTraNo_{req2.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Pending,
                    UploadDate = DateTime.Today.AddDays(-4)
                }
            );
            await db.SaveChangesAsync();

            // 3. Há»“ sÆ¡ GNTT VIB ÄANG THáº¨M Äá»ŠNH (Processing / Reviewing) - Hyundai Pháº¡m VÄƒn Äá»“ng
            var req3 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-003",
                TransType = BankingTransType.GNTT,
                DealerCode = "HYUNDAI-PVD",
                DealerName = "Hyundai Pháº¡m VÄƒn Äá»“ng",
                BizResNumber = "0104992817-003",
                BankCode = "VIB",
                BankName = "NgÃ¢n hÃ ng TMCP Quá»‘c táº¿ Viá»‡t Nam (VIB)",
                PaymentAccount = "025704068899",
                PaymentBankName = "VIB - CN Cáº§u Giáº¥y",
                ReceivingUnit = "CÃ”NG TY Cá»” PHáº¦N LIÃŠN DOANH Ã” TÃ” HYUNDAI THÃ€NH CÃ”NG VIá»†T NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Äá»‘ng Äa",
                TotalCars = 6,
                TotalContractAmount = 5_844_000_000,
                TotalDisbursementAmount = 4_675_200_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Processing,
                BankStatus = BankingTransBankStatus.Reviewing,
                RefBankCode = "VIB-LN-20250514-003",
                BankRemark = "CÃ¡n bá»™ tÃ­n dá»¥ng VIB Ä‘ang tháº©m Ä‘á»‹nh thá»±c Ä‘á»‹a kho bÃ£i vÃ  Ä‘á»‘i chiáº¿u háº¡n má»©c báº£o Ä‘áº£m theo há»£p Ä‘á»“ng nguyÃªn táº¯c.",
                DisbursementTerm = "03 thÃ¡ng",
                DisbursementInterestRate = 8.8m,
                LoanLimit = 12_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-2),
                Remark = "Äá» nghá»‹ giáº£i ngÃ¢n vá»‘n vay mua lÃ´ 6 xe MPV Custin",
                CreatedBy = "PhamVanDuc_KeToanPVD",
                CreatedAt = DateTime.Today.AddDays(-3)
            };
            db.DisbursementRequests.Add(req3);
            await db.SaveChangesAsync();

            db.DisbursementDetails.Add(
                new BankingDisbursementDetail
                {
                    RequestId = req3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req3.TransNo,
                    DlrCtrNo = "PLHD-2025-05/PVD01",
                    ModelCode = "CUSTIN-TURBO",
                    ModelName = "Hyundai Custin 2.0T Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Báº£n MPV cá»­a trÆ°á»£t Ä‘iá»‡n 7 chá»—",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-3),
                    PrincipalContractNo = "HDNT-2025/HTC-PVD",
                    PrincipalContractDate = new DateTime(2025, 1, 5),
                    DeliveryDate = DateTime.Today.AddDays(12),
                    Qty = 6,
                    UnitPrice = 974_000_000,
                    TotalAmount = 5_844_000_000,
                    LtvRate = 80.0m,
                    DisbursementAmount = 4_675_200_000,
                    Remark = "6 xe Custin 2.0T mÃ u tráº¯ng tuyáº¿t"
                }
            );

            db.DisbursementFiles.AddRange(
                new BankingDisbursementFile
                {
                    RequestId = req3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req3.TransNo,
                    DocType = BankFileDocumentType.DeNghiVay,
                    FileName = $"DeNghiGiaoDich_{req3.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req3.TransNo}/DeNghiGiaoDich_{req3.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Pham Van Hai - Tong Giam Doc PVD",
                    SignedAt = DateTime.Today.AddDays(-2),
                    CertSerialNumber = "99281740BB221877",
                    UploadDate = DateTime.Today.AddDays(-3)
                },
                new BankingDisbursementFile
                {
                    RequestId = req3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req3.TransNo,
                    DocType = BankFileDocumentType.PhuLucHopDong,
                    FileName = "PhuLucHopDong_PLHD-2025-05-PVD01.pdf",
                    FilePath = $"/storage/banking/{req3.TransNo}/PhuLucHopDong_PLHD-2025-05-PVD01.pdf",
                    SignStatus = BankFileSignStatus.Signed,
                    SignedUser = "Pham Van Hai - Tong Giam Doc PVD",
                    SignedAt = DateTime.Today.AddDays(-2),
                    CertSerialNumber = "99281740BB221877",
                    UploadDate = DateTime.Today.AddDays(-3)
                }
            );
            await db.SaveChangesAsync();

            // 4. Há»“ sÆ¡ Dá»° THáº¢O Má»šI (Draft / Pending) - Techcombank & Hyundai SÃ i GÃ²n
            var req4 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-004",
                TransType = BankingTransType.GNTTLC,
                DealerCode = "HYUNDAI-SG",
                DealerName = "Hyundai SÃ i GÃ²n 1S",
                BizResNumber = "0309118274-001",
                BankCode = "TECHCOMBANK",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                PaymentAccount = "19028819283011",
                PaymentBankName = "Techcombank - CN SÃ i GÃ²n",
                ReceivingUnit = "CÃ”NG TY Cá»” PHáº¦N LIÃŠN DOANH Ã” TÃ” HYUNDAI THÃ€NH CÃ”NG VIá»†T NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Äá»‘ng Äa",
                TotalCars = 4,
                TotalContractAmount = 2_796_000_000,
                TotalDisbursementAmount = 2_236_800_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Draft,
                BankStatus = BankingTransBankStatus.Pending,
                DisbursementTerm = "03 thÃ¡ng",
                DisbursementInterestRate = 8.2m,
                LoanLimit = 18_000_000_000,
                Remark = "Dá»± tháº£o Ä‘á» nghá»‹ tÃ i trá»£ giáº£i ngÃ¢n theo L/C cho 4 xe Creta Cao Cáº¥p",
                CreatedBy = "VoThiMai_KeToanSG",
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.DisbursementRequests.Add(req4);
            await db.SaveChangesAsync();

            db.DisbursementDetails.Add(
                new BankingDisbursementDetail
                {
                    RequestId = req4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req4.TransNo,
                    DlrCtrNo = "PLHD-2025-05/SG01",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Báº£n SUV cá»¡ B SmartSense",
                    AssemblyType = VehicleAssemblyType.CKD,
                    ContractDate = DateTime.Today.AddDays(-2),
                    PrincipalContractNo = "HDNT-2025/HTC-SG",
                    PrincipalContractDate = new DateTime(2025, 1, 8),
                    DeliveryDate = DateTime.Today.AddDays(16),
                    Qty = 4,
                    UnitPrice = 699_000_000,
                    TotalAmount = 2_796_000_000,
                    LtvRate = 80.0m,
                    DisbursementAmount = 2_236_800_000,
                    Remark = "4 xe Creta SUV B phÃ¢n phá»‘i thá»‹ trÆ°á»ng phÃ­a Nam"
                }
            );

            db.DisbursementFiles.Add(
                new BankingDisbursementFile
                {
                    RequestId = req4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    TransNo = req4.TransNo,
                    DocType = BankFileDocumentType.DeNghiVay,
                    FileName = $"DeNghiGiaoDich_{req4.TransNo}.pdf",
                    FilePath = $"/storage/banking/{req4.TransNo}/DeNghiGiaoDich_{req4.TransNo}.pdf",
                    SignStatus = BankFileSignStatus.Pending,
                    UploadDate = DateTime.Today.AddDays(-1)
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Há»“ sÆ¡ Äá» nghá»‹ Há»§y GÃ¡n NgÃ¢n HÃ ng Báº£o LÃ£nh Há»£p Äá»“ng Xe Ã” TÃ´ (DMS40_DlrCtr_CancelBankMD / BizHTC.Payment)
        if (!await db.CancelBankMDRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Há»“ sÆ¡ 01: HTC ÄÃƒ DUYá»†T HOÃ€N Táº¤T (Finished) - Hyundai Thanh XuÃ¢n Ä‘á»•i báº£o lÃ£nh VPBank sang VietinBank
            var can1 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-001",
                DlrCtrNo = "PLHD-2025-05/TX01",
                DealerCode = "HYUNDAI-TX",
                DealerName = "Hyundai Thanh XuÃ¢n",
                BankCodeMD = "VPB",
                BankNameMD = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                NewBankCodeMD = "CTG",
                NewBankNameMD = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 3_178_000_000,
                GuaranteeAmount = 2_700_000_000,
                ReasonType = CancelBankMDReasonType.ChangeBank,
                ReasonDescription = "Äáº¡i lÃ½ chuyá»ƒn sang VietinBank tÃ i trá»£ do Ä‘Æ°á»£c cáº¥p háº¡n má»©c tÃ­n dá»¥ng vÃ  lÃ£i suáº¥t Æ°u Ä‘Ã£i tá»‘t hÆ¡n",
                Status = CancelBankMDStatus.Finished,
                TotalVehicles = 2,
                RemarkDlr = "KÃ­nh Ä‘á» nghá»‹ Ban LÃ£nh Ä‘áº¡o HTC vÃ  NgÃ¢n hÃ ng VPBank cháº¥p thuáº­n há»§y cam káº¿t báº£o lÃ£nh cÅ© Ä‘á»ƒ gÃ¡n VietinBank",
                RemarkBank = "VPBank xÃ¡c nháº­n chÆ°a phÃ¡t hÃ nh thÆ° báº£o lÃ£nh chÃ­nh thá»©c vÃ  Ä‘á»“ng Ã½ giáº£i phÃ³ng nghÄ©a vá»¥ báº£o lÃ£nh cho há»£p Ä‘á»“ng PLHD-2025-05/TX01",
                RemarkHTC = "HTC phÃª duyá»‡t hoÃ n táº¥t há»§y gÃ¡n báº£o lÃ£nh VPBank. ÄÃ£ cáº­p nháº­t BankCodeMD = NULL vÃ  cho phÃ©p Ä‘áº¡i lÃ½ Ä‘Äƒng kÃ½ báº£o lÃ£nh VietinBank",
                ApproveBy = "TranQuocTuan_GiamDocTinDung_VPBank",
                ApproveDateTime = DateTime.Today.AddDays(-3),
                FinishBy = "LeQuangHuy_PhoTGDTaiChinh_HTC",
                FinishDTime = DateTime.Today.AddDays(-2),
                CreatedBy = "VuThanhNam_KeToanTX",
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.CancelBankMDRequests.Add(can1);
            await db.SaveChangesAsync();

            db.CancelBankMDDetails.AddRange(
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can1.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770101",
                    CarId = "CAR-SF-0101",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "Báº£n cao cáº¥p 6 chá»— HTRAC",
                    ColorExtNameVN = "Tráº¯ng ngá»c trai",
                    EngineNo = "ENG-SF-2025-01",
                    UnitPrice = 1_589_000_000,
                    GuaranteeAmount = 1_350_000_000,
                    Status = CancelBankMDDetailStatus.Finished,
                    Remark = "ÄÃ£ hoÃ n táº¥t há»§y gÃ¡n báº£o lÃ£nh VPBank"
                },
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can1.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770102",
                    CarId = "CAR-SF-0102",
                    ModelCode = "SANTAFE-CAL",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5T",
                    SpecCode = "SF-2.5T-CAL6",
                    SpecDescription = "Báº£n cao cáº¥p 6 chá»— HTRAC",
                    ColorExtNameVN = "Äen Midnight",
                    EngineNo = "ENG-SF-2025-02",
                    UnitPrice = 1_589_000_000,
                    GuaranteeAmount = 1_350_000_000,
                    Status = CancelBankMDDetailStatus.Finished,
                    Remark = "ÄÃ£ hoÃ n táº¥t há»§y gÃ¡n báº£o lÃ£nh VPBank"
                }
            );

            // 2. Há»“ sÆ¡ 02: NGÃ‚N HÃ€NG ÄÃƒ DUYá»†T (Approved) - Hyundai HÃ  ÄÃ´ng chuyá»ƒn sang thanh toÃ¡n vá»‘n tá»± cÃ³
            var can2 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-002",
                DlrCtrNo = "PLHD-2025-05/HD02",
                DealerCode = "HYUNDAI-HD",
                DealerName = "Hyundai HÃ  ÄÃ´ng",
                BankCodeMD = "CTG",
                BankNameMD = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 2_447_000_000,
                GuaranteeAmount = 2_000_000_000,
                ReasonType = CancelBankMDReasonType.SwitchToOwnCapital,
                ReasonDescription = "Äáº¡i lÃ½ chuyá»ƒn Ä‘á»•i sang thanh toÃ¡n báº±ng vá»‘n tá»± cÃ³ qua UNC ngÃ¢n hÃ ng Ä‘á»ƒ tiáº¿t kiá»‡m chi phÃ­ phÃ­ phÃ¡t hÃ nh báº£o lÃ£nh",
                Status = CancelBankMDStatus.Approved,
                TotalVehicles = 3,
                RemarkDlr = "Äáº¡i lÃ½ Ä‘Ã£ chuáº©n bá»‹ Ä‘á»§ nguá»“n tiá»n vá»‘n tá»± cÃ³ táº¡i tÃ i khoáº£n BIDV, Ä‘á» nghá»‹ há»§y báº£o lÃ£nh VietinBank",
                RemarkBank = "VietinBank Chi nhÃ¡nh HÃ  TÃ¢y cháº¥p thuáº­n há»§y chá»‰ Ä‘á»‹nh báº£o lÃ£nh cho há»£p Ä‘á»“ng PLHD-2025-05/HD02 theo Ä‘á» nghá»‹ cá»§a khÃ¡ch hÃ ng",
                ApproveBy = "NguyenVanBinh_TruongPhongKHDN_VietinBank",
                ApproveDateTime = DateTime.Today.AddDays(-1),
                CreatedBy = "NguyenThiMai_KeToanHD",
                CreatedAt = DateTime.Today.AddDays(-3)
            };
            db.CancelBankMDRequests.Add(can2);
            await db.SaveChangesAsync();

            db.CancelBankMDDetails.AddRange(
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can2.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770201",
                    CarId = "CAR-CRT-0201",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Báº£n SUV Ä‘Ã´ thá»‹ SmartSense",
                    ColorExtNameVN = "Äá» Máº­n",
                    EngineNo = "ENG-CR-2025-11",
                    UnitPrice = 699_000_000,
                    GuaranteeAmount = 570_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "NgÃ¢n hÃ ng VietinBank Ä‘Ã£ duyá»‡t cháº¥p thuáº­n há»§y"
                },
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can2.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770202",
                    CarId = "CAR-CST-0202",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Báº£n MPV cá»­a lÃ¹a Ä‘iá»‡n kÃ©p",
                    ColorExtNameVN = "Tráº¯ng",
                    EngineNo = "ENG-CS-2025-22",
                    UnitPrice = 974_000_000,
                    GuaranteeAmount = 800_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "NgÃ¢n hÃ ng VietinBank Ä‘Ã£ duyá»‡t cháº¥p thuáº­n há»§y"
                },
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can2.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770203",
                    CarId = "CAR-TUC-0203",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6 T-GDi HTRAC",
                    SpecCode = "TUC-1.6T-TUR",
                    SpecDescription = "Báº£n mÃ¡y xÄƒng tÄƒng Ã¡p thá»ƒ thao",
                    ColorExtNameVN = "Xanh DÆ°Æ¡ng",
                    EngineNo = "ENG-TC-2025-33",
                    UnitPrice = 774_000_000,
                    GuaranteeAmount = 630_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "NgÃ¢n hÃ ng VietinBank Ä‘Ã£ duyá»‡t cháº¥p thuáº­n há»§y"
                }
            );

            // 3. Há»“ sÆ¡ 03: Má»šI Láº¬P CHá»œ Xá»¬ LÃ (Pending) - Hyundai ÄÃ´ng Anh Ä‘á» nghá»‹ Ä‘á»•i sang Techcombank
            var can3 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-003",
                DlrCtrNo = "PLHD-2025-05/DA03",
                DealerCode = "HYUNDAI-DA",
                DealerName = "Hyundai ÄÃ´ng Anh",
                BankCodeMD = "MBB",
                BankNameMD = "NgÃ¢n hÃ ng TMCP QuÃ¢n Äá»™i (MBBank)",
                NewBankCodeMD = "TCB",
                NewBankNameMD = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 3_118_000_000,
                GuaranteeAmount = 2_600_000_000,
                ReasonType = CancelBankMDReasonType.ChangeBank,
                ReasonDescription = "Äáº¡i lÃ½ chuyá»ƒn báº£o lÃ£nh sang Techcombank Ä‘á»ƒ hÆ°á»Ÿng chÃ­nh sÃ¡ch Æ°u Ä‘Ã£i háº¡n má»©c vÃ  thá»i gian tráº£ cháº­m dÃ i hÆ¡n",
                Status = CancelBankMDStatus.Pending,
                TotalVehicles = 2,
                RemarkDlr = "KÃ­nh chuyá»ƒn Há»™i sá»Ÿ MBBank xem xÃ©t cháº¥p thuáº­n há»§y chá»‰ Ä‘á»‹nh báº£o lÃ£nh Ä‘á»ƒ Ä‘áº¡i lÃ½ gÃ¡n Techcombank",
                CreatedBy = "VuDinhTruong_KeToanDA",
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.CancelBankMDRequests.Add(can3);
            await db.SaveChangesAsync();

            db.CancelBankMDDetails.AddRange(
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can3.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770301",
                    CarId = "CAR-PLS-0301",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PLS-2.2D-PRE",
                    SpecDescription = "Báº£n SUV cá»¡ lá»›n 7 chá»— mÃ¡y dáº§u",
                    ColorExtNameVN = "Xanh BÃ³ng ÄÃªm",
                    EngineNo = "ENG-PL-2025-41",
                    UnitPrice = 1_559_000_000,
                    GuaranteeAmount = 1_300_000_000,
                    Status = CancelBankMDDetailStatus.Pending,
                    Remark = "Chá» MBBank tháº©m Ä‘á»‹nh cháº¥p thuáº­n"
                },
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can3.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770302",
                    CarId = "CAR-PLS-0302",
                    ModelCode = "PALISADE-PRE",
                    ModelName = "Hyundai Palisade 2.2D Prestige",
                    SpecCode = "PLS-2.2D-PRE",
                    SpecDescription = "Báº£n SUV cá»¡ lá»›n 7 chá»— mÃ¡y dáº§u",
                    ColorExtNameVN = "Äen",
                    EngineNo = "ENG-PL-2025-42",
                    UnitPrice = 1_559_000_000,
                    GuaranteeAmount = 1_300_000_000,
                    Status = CancelBankMDDetailStatus.Pending,
                    Remark = "Chá» MBBank tháº©m Ä‘á»‹nh cháº¥p thuáº­n"
                }
            );

            // 4. Há»“ sÆ¡ 04: Tá»ª CHá»I (Rejected) - Hyundai Pháº¡m VÄƒn Äá»“ng
            var can4 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-004",
                DlrCtrNo = "PLHD-2025-05/PVD04",
                DealerCode = "HYUNDAI-PVD",
                DealerName = "Hyundai Pháº¡m VÄƒn Äá»“ng",
                BankCodeMD = "VCB",
                BankNameMD = "NgÃ¢n hÃ ng TMCP Ngoáº¡i ThÆ°Æ¡ng Viá»‡t Nam (Vietcombank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 1_450_000_000,
                GuaranteeAmount = 1_450_000_000,
                ReasonType = CancelBankMDReasonType.ContractRestructuring,
                ReasonDescription = "Äáº¡i lÃ½ Ä‘á» nghá»‹ há»§y báº£o lÃ£nh Ä‘á»ƒ Ä‘iá»u chá»‰nh cÆ¡ cáº¥u phÃ¢n bá»• dÃ²ng xe",
                Status = CancelBankMDStatus.Rejected,
                TotalVehicles = 1,
                RemarkDlr = "KÃ­nh Ä‘á» nghá»‹ HTC xem xÃ©t há»§y báº£o lÃ£nh xe Ä‘iá»‡n Ioniq 5 Ä‘á»ƒ cÆ¡ cáº¥u láº¡i há»£p Ä‘á»“ng",
                RejectBy = "NguyenDucThanh_GiamDocPhapChe_HTC",
                RejectDateTime = DateTime.Today.AddDays(-2),
                RejectReason = "Xe Ä‘Ã£ hoÃ n táº¥t thá»§ tá»¥c xuáº¥t kho vÃ  Ä‘ang trÃªn Ä‘Æ°á»ng váº­n chuyá»ƒn giao Ä‘áº¡i lÃ½ theo biÃªn báº£n BBBG, khÃ´ng thá»ƒ há»§y gÃ¡n ngÃ¢n hÃ ng báº£o lÃ£nh.",
                RemarkHTC = "[HTC tá»« chá»‘i] Xe Ä‘Ã£ hoÃ n táº¥t thá»§ tá»¥c xuáº¥t kho vÃ  Ä‘ang trÃªn Ä‘Æ°á»ng váº­n chuyá»ƒn giao Ä‘áº¡i lÃ½ theo biÃªn báº£n BBBG, khÃ´ng thá»ƒ há»§y gÃ¡n ngÃ¢n hÃ ng báº£o lÃ£nh.",
                CreatedBy = "PhamVanDuc_KeToanPVD",
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.CancelBankMDRequests.Add(can4);
            await db.SaveChangesAsync();

            db.CancelBankMDDetails.Add(
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can4.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770401",
                    CarId = "CAR-IQ5-0401",
                    ModelCode = "IONIQ-5",
                    ModelName = "Hyundai IONIQ 5 Exclusive EV",
                    SpecCode = "IQ5-72KWH-EXC",
                    SpecDescription = "Xe Ä‘iá»‡n cÃ´ng nghá»‡ sáº¡c siÃªu nhanh E-GMP",
                    ColorExtNameVN = "Báº¡c NhÃ¡m Gravity Gold",
                    EngineNo = "EM07-2025-99",
                    UnitPrice = 1_450_000_000,
                    GuaranteeAmount = 1_450_000_000,
                    Status = CancelBankMDDetailStatus.Cancelled,
                    Remark = "Tá»« chá»‘i do xe Ä‘Ã£ váº­n chuyá»ƒn Ä‘i"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 19. Seed Dá»¯ liá»‡u máº«u Thu Tiá»n Thanh ToÃ¡n & Quyáº¿t ToÃ¡n Bá»“i ThÆ°á»ng Báº£o Hiá»ƒm Xe Ã” TÃ´ (Insurance Claim Payment) =====
        if (!await db.InsuranceClaimDebits.AnyAsync())
        {
            var d1 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-001",
                InsNo = "INS-PTI",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm BÆ°u Ä‘iá»‡n (PTI)",
                RONo = "RO-2025-0501",
                VIN = "KMHCT81EPHU990101",
                PlateNo = "30H-889.99",
                ModelCode = "SANTAFE",
                ModelName = "Hyundai Santa Fe 2.5 HTRAC",
                CustomerName = "Tráº§n ÄÃ¬nh Trá»ng",
                CustomerPhone = "0912345678",
                DebitDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(10),
                DebitAmount = 28_500_000,
                PaidAmount = 28_500_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "SÆ¡n sáº¥y cáº£n trÆ°á»›c vÃ  thay cá»¥m Ä‘Ã¨n pha LED bÃªn lÃ¡i sau va quáº¹t",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-20)
            };

            var d2 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-002",
                InsNo = "INS-PTI",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm BÆ°u Ä‘iá»‡n (PTI)",
                RONo = "RO-2025-0502",
                VIN = "KMHCT81EPHU990102",
                PlateNo = "30G-668.88",
                ModelCode = "TUCSON",
                ModelName = "Hyundai Tucson 2.0 AT",
                CustomerName = "Nguyá»…n Thá»‹ Mai",
                CustomerPhone = "0987654321",
                DebitDate = DateTime.Today.AddDays(-18),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 16_200_000,
                PaidAmount = 16_200_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "GÃ² náº¯n tai xe bÃªn phá»¥ vÃ  sÆ¡n phá»§ bÃ³ng 2 lá»›p",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-18)
            };

            var d3 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-003",
                InsNo = "INS-PTI",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm BÆ°u Ä‘iá»‡n (PTI)",
                RONo = "RO-2025-0503",
                VIN = "KMHCT81EPHU990103",
                PlateNo = "29A-995.12",
                ModelCode = "CRETA",
                ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                CustomerName = "LÃª HoÃ ng Long",
                CustomerPhone = "0903112233",
                DebitDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 19_800_000,
                PaidAmount = 10_300_000,
                RemainAmount = 9_500_000,
                Status = InsuranceDebitStatus.PartiallyPaid,
                Note = "Thay kÃ­nh cháº¯n giÃ³ trÆ°á»›c chÃ­nh hÃ£ng vÃ  sÆ¡n sÆ°á»n xe",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-15)
            };

            var d4 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-004",
                InsNo = "INS-PTI",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm BÆ°u Ä‘iá»‡n (PTI)",
                RONo = "RO-2025-0504",
                VIN = "KMHCT81EPHU990104",
                PlateNo = "30F-334.56",
                ModelCode = "ACCENT",
                ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                CustomerName = "Äáº·ng Quang Huy",
                CustomerPhone = "0945678901",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 7_500_000,
                PaidAmount = 0,
                RemainAmount = 7_500_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "SÆ¡n phá»¥c há»“i tráº§y xÆ°á»›c náº¯p capo vÃ  cáº£n sau",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var d5 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-005",
                InsNo = "INS-BV",
                InsName = "Tá»•ng CÃ´ng ty Báº£o hiá»ƒm Báº£o Viá»‡t",
                RONo = "RO-2025-0505",
                VIN = "KMHCT81EPHU990201",
                PlateNo = "30E-778.90",
                ModelCode = "SANTAFE",
                ModelName = "Hyundai Santa Fe 2.5 HTRAC",
                CustomerName = "Pháº¡m Há»“ng QuÃ¢n",
                CustomerPhone = "0934567890",
                DebitDate = DateTime.Today.AddDays(-12),
                DueDate = DateTime.Today.AddDays(18),
                DebitAmount = 34_000_000,
                PaidAmount = 34_000_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Thay tháº¿ thÆ°á»›c lÃ¡i Ä‘iá»‡n tá»­ vÃ  báº£o dÆ°á»¡ng phá»¥c há»“i giáº£m xÃ³c trÆ°á»›c",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-12)
            };

            var d6 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-006",
                InsNo = "INS-BV",
                InsName = "Tá»•ng CÃ´ng ty Báº£o hiá»ƒm Báº£o Viá»‡t",
                RONo = "RO-2025-0506",
                VIN = "KMHCT81EPHU990202",
                PlateNo = "30K-112.33",
                ModelCode = "PALISADE",
                ModelName = "Hyundai Palisade 2.2D Prestige",
                CustomerName = "HoÃ ng Thu Trang",
                CustomerPhone = "0967890123",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(27),
                DebitAmount = 42_500_000,
                PaidAmount = 0,
                RemainAmount = 42_500_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Phá»¥c há»“i cÃ¡nh cá»­a sau bÃªn phá»¥ vÃ  sÆ¡n phá»§ bÃ³ng cá»‘p Ä‘iá»‡n",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var d7 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-007",
                InsNo = "INS-PJICO",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm Petrolimex (PJICO)",
                RONo = "RO-2025-0507",
                VIN = "KMHCT81EPHU990301",
                PlateNo = "29B-445.67",
                ModelCode = "CRETA",
                ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                CustomerName = "Äá»— Äá»©c ToÃ n",
                CustomerPhone = "0978901234",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 21_000_000,
                PaidAmount = 21_000_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Sá»­a chá»¯a há»‡ thá»‘ng giáº£i nhiá»‡t kÃ©t nÆ°á»›c vÃ  cáº£n trÆ°á»›c",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var d8 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-008",
                InsNo = "INS-PJICO",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm Petrolimex (PJICO)",
                RONo = "RO-2025-0508",
                VIN = "KMHCT81EPHU990302",
                PlateNo = "30A-556.78",
                ModelCode = "ELANTRA",
                ModelName = "Hyundai Elantra N-Line 1.6 Turbo",
                CustomerName = "BÃ¹i Anh Tuáº¥n",
                CustomerPhone = "0923456789",
                DebitDate = DateTime.Today.AddDays(-7),
                DueDate = DateTime.Today.AddDays(23),
                DebitAmount = 12_800_000,
                PaidAmount = 6_000_000,
                RemainAmount = 6_800_000,
                Status = InsuranceDebitStatus.PartiallyPaid,
                Note = "SÆ¡n sáº¥y hÃ´ng trÃ¡i vÃ  cá»­a trÆ°á»›c",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-7)
            };

            var d9 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-009",
                InsNo = "INS-PVI",
                InsName = "Tá»•ng CÃ´ng ty Báº£o hiá»ƒm PVI (Dáº§u khÃ­)",
                RONo = "RO-2025-0509",
                VIN = "KMHCT81EPHU990401",
                PlateNo = "30H-223.45",
                ModelCode = "TUCSON",
                ModelName = "Hyundai Tucson 2.0 AT",
                CustomerName = "NgÃ´ Minh Khang",
                CustomerPhone = "0918765432",
                DebitDate = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(28),
                DebitAmount = 38_000_000,
                PaidAmount = 0,
                RemainAmount = 38_000_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Báº£o dÆ°á»¡ng Ä‘áº¡i tu gáº§m vÃ  sÆ¡n ba-Ä‘á»-sá»‘c sau",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-2)
            };

            var d10 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-010",
                InsNo = "INS-MIC",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm QuÃ¢n Ä‘á»™i (MIC)",
                RONo = "RO-2025-0510",
                VIN = "KMHCT81EPHU990501",
                PlateNo = "29D-889.01",
                ModelCode = "ACCENT",
                ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                CustomerName = "VÅ© ÄÃ¬nh Nam",
                CustomerPhone = "0909876543",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(29),
                DebitAmount = 9_600_000,
                PaidAmount = 0,
                RemainAmount = 9_600_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Thay cá»¥m gÆ°Æ¡ng chiáº¿u háº­u cÃ³ camera 360 vÃ  sÆ¡n cÃ¡nh cá»­a",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            db.InsuranceClaimDebits.AddRange(d1, d2, d3, d4, d5, d6, d7, d8, d9, d10);
            await db.SaveChangesAsync();

            // Phiáº¿u thu 1: PTI thanh toÃ¡n gá»™p 55,000,000 VND (chuyá»ƒn khoáº£n VietinBank)
            var p1 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-001",
                InsNo = "INS-PTI",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm BÆ°u Ä‘iá»‡n (PTI)",
                PayDate = DateTime.Today.AddDays(-8),
                PayPersonName = "Nguyá»…n VÄƒn Tuáº¥n (GiÃ¡m Ä‘á»‹nh viÃªn PTI)",
                PayPersonIDCardNo = "001089012345",
                PayPersonPhone = "0911223344",
                PaymentAmount = 55_000_000,
                PaymentMethod = InsurancePaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - CN Äá»‘ng Äa",
                BankAccountNo = "113000088999",
                BankTxnRef = "FT250508001882",
                TotalAllocated = 55_000_000,
                UnallocatedAmount = 0,
                Status = InsurancePaymentStatus.Settled,
                Note = "PTI thanh toÃ¡n bá»“i thÆ°á»ng Ä‘á»£t 1 thÃ¡ng 05/2025 cho 3 xe",
                CreatedBy = "NguyenThiThu_KeToanThuNgan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today.AddDays(-8),
                SettledBy = "TranDinhTuan_KeToanTruong",
                SettledAt = DateTime.Today.AddDays(-8),
                CreatedAt = DateTime.Today.AddDays(-8)
            };
            db.InsurancePayments.Add(p1);
            await db.SaveChangesAsync();

            db.InsurancePaymentDetails.AddRange(
                new InsurancePaymentDetail
                {
                    PaymentId = p1.Id,
                    DebitId = d1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d1.DebitNo,
                    RONo = d1.RONo,
                    VIN = d1.VIN,
                    PlateNo = d1.PlateNo,
                    DebitAmount = 28_500_000,
                    DebitAmountBefore = 28_500_000,
                    PaymentDetailAmount = 28_500_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ lá»‡nh RO"
                },
                new InsurancePaymentDetail
                {
                    PaymentId = p1.Id,
                    DebitId = d2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d2.DebitNo,
                    RONo = d2.RONo,
                    VIN = d2.VIN,
                    PlateNo = d2.PlateNo,
                    DebitAmount = 16_200_000,
                    DebitAmountBefore = 16_200_000,
                    PaymentDetailAmount = 16_200_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ lá»‡nh RO"
                },
                new InsurancePaymentDetail
                {
                    PaymentId = p1.Id,
                    DebitId = d3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d3.DebitNo,
                    RONo = d3.RONo,
                    VIN = d3.VIN,
                    PlateNo = d3.PlateNo,
                    DebitAmount = 19_800_000,
                    DebitAmountBefore = 19_800_000,
                    PaymentDetailAmount = 10_300_000,
                    DebitAmountLeft = 9_500_000,
                    Remark = "Thanh toÃ¡n má»™t pháº§n ná»£ RO"
                }
            );

            // Phiáº¿u thu 2: Báº£o Viá»‡t thanh toÃ¡n 34,000,000 VND qua Cá»•ng VNPay QR
            var p2 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-002",
                InsNo = "INS-BV",
                InsName = "Tá»•ng CÃ´ng ty Báº£o hiá»ƒm Báº£o Viá»‡t",
                PayDate = DateTime.Today.AddDays(-4),
                PayPersonName = "LÃª Thá»‹ Há»“ng (Káº¿ toÃ¡n Bá»“i thÆ°á»ng Báº£o Viá»‡t)",
                PayPersonIDCardNo = "001192034567",
                PayPersonPhone = "0988776655",
                PaymentAmount = 34_000_000,
                PaymentMethod = InsurancePaymentMethod.VnPay,
                BankCode = "VCB",
                BankName = "Vietcombank",
                BankAccountNo = "0011004123456",
                BankTxnRef = "VNPAY-QR-20250512-8871",
                TotalAllocated = 34_000_000,
                UnallocatedAmount = 0,
                Status = InsurancePaymentStatus.Settled,
                Note = "Báº£o Viá»‡t chuyá»ƒn khoáº£n quÃ©t mÃ£ QR VNPay thanh toÃ¡n lá»‡nh RO-2025-0505",
                CreatedBy = "NguyenThiThu_KeToanThuNgan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today.AddDays(-4),
                SettledBy = "TranDinhTuan_KeToanTruong",
                SettledAt = DateTime.Today.AddDays(-4),
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.InsurancePayments.Add(p2);
            await db.SaveChangesAsync();

            db.InsurancePaymentDetails.Add(
                new InsurancePaymentDetail
                {
                    PaymentId = p2.Id,
                    DebitId = d5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d5.DebitNo,
                    RONo = d5.RONo,
                    VIN = d5.VIN,
                    PlateNo = d5.PlateNo,
                    DebitAmount = 34_000_000,
                    DebitAmountBefore = 34_000_000,
                    PaymentDetailAmount = 34_000_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ lá»‡nh RO qua cá»•ng VNPay"
                }
            );

            // Phiáº¿u thu 3: PJICO thanh toÃ¡n 27,000,000 VND (chuyá»ƒn khoáº£n MBBank)
            var p3 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-003",
                InsNo = "INS-PJICO",
                InsName = "Tá»•ng CÃ´ng ty Cá»• pháº§n Báº£o hiá»ƒm Petrolimex (PJICO)",
                PayDate = DateTime.Today.AddDays(-1),
                PayPersonName = "HoÃ ng Trá»ng NghÄ©a (GiÃ¡m Ä‘á»‹nh viÃªn PJICO)",
                PayPersonIDCardNo = "001095067890",
                PayPersonPhone = "0933445566",
                PaymentAmount = 27_000_000,
                PaymentMethod = InsurancePaymentMethod.BankTransfer,
                BankCode = "MBB",
                BankName = "MBBank - CN Sá»Ÿ Giao Dá»‹ch",
                BankAccountNo = "0880112345678",
                BankTxnRef = "MB-TRANS-9908123",
                TotalAllocated = 27_000_000,
                UnallocatedAmount = 0,
                Status = InsurancePaymentStatus.Confirmed,
                Note = "PJICO chuyá»ƒn tiá»n thanh toÃ¡n theo thá»a thuáº­n bá»“i thÆ°á»ng xe Hyundai",
                CreatedBy = "NguyenThiThu_KeToanThuNgan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.Today.AddDays(-1)
            };
            db.InsurancePayments.Add(p3);
            await db.SaveChangesAsync();

            db.InsurancePaymentDetails.AddRange(
                new InsurancePaymentDetail
                {
                    PaymentId = p3.Id,
                    DebitId = d7.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d7.DebitNo,
                    RONo = d7.RONo,
                    VIN = d7.VIN,
                    PlateNo = d7.PlateNo,
                    DebitAmount = 21_000_000,
                    DebitAmountBefore = 21_000_000,
                    PaymentDetailAmount = 21_000_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ lá»‡nh RO"
                },
                new InsurancePaymentDetail
                {
                    PaymentId = p3.Id,
                    DebitId = d8.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = d8.DebitNo,
                    RONo = d8.RONo,
                    VIN = d8.VIN,
                    PlateNo = d8.PlateNo,
                    DebitAmount = 12_800_000,
                    DebitAmountBefore = 12_800_000,
                    PaymentDetailAmount = 6_000_000,
                    DebitAmountLeft = 6_800_000,
                    Remark = "Thanh toÃ¡n má»™t pháº§n ná»£ RO"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 20. Seed Dá»¯ liá»‡u máº«u CÃ´ng Ná»£ & Thanh ToÃ¡n Quyáº¿t ToÃ¡n NhÃ  Cung Cáº¥p Phá»¥ TÃ¹ng (Supplier Payment) =====
        if (!await db.SupplierDebits.AnyAsync())
        {
            var sd1 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-001",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "LÃ´ E3, KCN ThÄƒng Long, Huyá»‡n ÄÃ´ng Anh, HÃ  Ná»™i",
                StockInNo = "PNK-2025-0501",
                StockInDate = DateTime.Today.AddDays(-25),
                OrderPartNo = "PO-2025-0480",
                Category = "Linh kiá»‡n gáº§m mÃ¡y & Lá»c dáº§u nhá»›t Santa Fe/Tucson",
                DebitDate = DateTime.Today.AddDays(-25),
                DueDate = DateTime.Today.AddDays(5),
                DebitAmount = 35_000_000,
                PaidAmount = 35_000_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "Nháº­p kho 100 bá»™ lá»c dáº§u, 50 bá»™ lá»c giÃ³ Ä‘á»™ng cÆ¡ vÃ  cáº£m biáº¿n khÃ­ náº¡p",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-25)
            };

            var sd2 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-002",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "LÃ´ E3, KCN ThÄƒng Long, Huyá»‡n ÄÃ´ng Anh, HÃ  Ná»™i",
                StockInNo = "PNK-2025-0502",
                StockInDate = DateTime.Today.AddDays(-22),
                OrderPartNo = "PO-2025-0485",
                Category = "MÃ¡ phanh Ä‘Ä©a trÆ°á»›c sau & Cá»¥m moay-Æ¡ Creta/Accent",
                DebitDate = DateTime.Today.AddDays(-22),
                DueDate = DateTime.Today.AddDays(8),
                DebitAmount = 22_500_000,
                PaidAmount = 22_500_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "LÃ´ mÃ¡ phanh Ä‘Ä©a gá»‘m chÃ­nh hÃ£ng nháº­p kháº©u Mobis HÃ n Quá»‘c",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-22)
            };

            var sd3 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-003",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "LÃ´ E3, KCN ThÄƒng Long, Huyá»‡n ÄÃ´ng Anh, HÃ  Ná»™i",
                StockInNo = "PNK-2025-0503",
                StockInDate = DateTime.Today.AddDays(-15),
                OrderPartNo = "PO-2025-0492",
                Category = "ÄÃ¨n pha Full LED & GÆ°Æ¡ng chiáº¿u háº­u gáº­p Ä‘iá»‡n Palisade",
                DebitDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 48_000_000,
                PaidAmount = 7_500_000,
                RemainAmount = 40_500_000,
                Status = SupplierDebitStatus.PartiallyPaid,
                Note = "Cá»¥m Ä‘Ã¨n pha thÃ­ch á»©ng thÃ´ng minh IFS vÃ  máº·t gÆ°Æ¡ng sáº¥y Ä‘iá»‡n",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-15)
            };

            var sd4 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-004",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "LÃ´ E3, KCN ThÄƒng Long, Huyá»‡n ÄÃ´ng Anh, HÃ  Ná»™i",
                StockInNo = "PNK-2025-0504",
                StockInDate = DateTime.Today.AddDays(-3),
                OrderPartNo = "PO-2025-0510",
                Category = "CÃ ng chá»¯ A & Giáº£m xÃ³c hÆ¡i Ä‘iá»‡n tá»­ Custin",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(27),
                DebitAmount = 31_000_000,
                PaidAmount = 0,
                RemainAmount = 31_000_000,
                Status = SupplierDebitStatus.Pending,
                Note = "LÃ´ giáº£m xÃ³c trÆ°á»›c sau cho xe Custin 2.0 Turbo",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var sd5 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-005",
                SupplierCode = "BOSCH-VN",
                SupplierName = "CÃ´ng ty TNHH Robert Bosch Viá»‡t Nam",
                SupplierPhone = "028-6258-3690",
                SupplierAddress = "Táº§ng 14, TÃ²a nhÃ  Deutsches Haus, 33 LÃª Duáº©n, Quáº­n 1, TP. HCM",
                StockInNo = "PNK-2025-0505",
                StockInDate = DateTime.Today.AddDays(-18),
                OrderPartNo = "PO-2025-0488",
                Category = "Cáº§n gáº¡t mÆ°a Aerotwin & Bugi Iridium cao cáº¥p",
                DebitDate = DateTime.Today.AddDays(-18),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 18_500_000,
                PaidAmount = 18_500_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "Gáº¡t mÆ°a silicon Ä‘a nÄƒng vÃ  120 chiáº¿c bugi Ä‘Ã¡nh lá»­a Bosch kÃ©p",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-18)
            };

            var sd6 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-006",
                SupplierCode = "BOSCH-VN",
                SupplierName = "CÃ´ng ty TNHH Robert Bosch Viá»‡t Nam",
                SupplierPhone = "028-6258-3690",
                SupplierAddress = "Táº§ng 14, TÃ²a nhÃ  Deutsches Haus, 33 LÃª Duáº©n, Quáº­n 1, TP. HCM",
                StockInNo = "PNK-2025-0506",
                StockInDate = DateTime.Today.AddDays(-5),
                OrderPartNo = "PO-2025-0505",
                Category = "Cáº£m biáº¿n Ã¡p suáº¥t lá»‘p TPMS & CÃ²i sÃªn Ã´ tÃ´ 12V",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 14_200_000,
                PaidAmount = 0,
                RemainAmount = 14_200_000,
                Status = SupplierDebitStatus.Pending,
                Note = "Cáº£m biáº¿n van trong tÃ­ch há»£p Bluetooth vÃ  cÃ²i sÃªn Ã¢m lÆ°á»£ng cao",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var sd7 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-007",
                SupplierCode = "CASTROL-VN",
                SupplierName = "CÃ´ng ty TNHH Castrol BP Petco Viá»‡t Nam",
                SupplierPhone = "028-3821-9153",
                SupplierAddress = "Láº§u 9, TÃ²a nhÃ  Times Square, 22-36 Nguyá»…n Huá»‡, Quáº­n 1, TP. HCM",
                StockInNo = "PNK-2025-0507",
                StockInDate = DateTime.Today.AddDays(-10),
                OrderPartNo = "PO-2025-0498",
                Category = "Dáº§u nhá»›t tá»•ng há»£p Castrol Magnatec & Edge 5W-30",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 32_000_000,
                PaidAmount = 0,
                RemainAmount = 32_000_000,
                Status = SupplierDebitStatus.Pending,
                Note = "40 thÃ¹ng dáº§u Castrol Magnatec Stop-Start 5W-30 vÃ  dáº§u cáº§u há»™p sá»‘ ATF",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var sd8 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-008",
                SupplierCode = "MICHELIN-VN",
                SupplierName = "CÃ´ng ty TNHH Michelin Viá»‡t Nam",
                SupplierPhone = "028-3824-3456",
                SupplierAddress = "Táº§ng 10, TÃ²a nhÃ  Empress Tower, 138-142 Hai BÃ  TrÆ°ng, Quáº­n 1, TP. HCM",
                StockInNo = "PNK-2025-0508",
                StockInDate = DateTime.Today.AddDays(-7),
                OrderPartNo = "PO-2025-0502",
                Category = "Lá»‘p xe Ã´ tÃ´ Michelin Primacy 4 & Pilot Sport SUV",
                DebitDate = DateTime.Today.AddDays(-7),
                DueDate = DateTime.Today.AddDays(23),
                DebitAmount = 45_600_000,
                PaidAmount = 0,
                RemainAmount = 45_600_000,
                Status = SupplierDebitStatus.Pending,
                Note = "24 quáº£ lá»‘p kÃ­ch thÆ°á»›c 235/55R19 cho Santa Fe vÃ  235/60R18 cho Tucson",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-7)
            };

            var sd9 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-009",
                SupplierCode = "3M-VN",
                SupplierName = "CÃ´ng ty TNHH 3M Viá»‡t Nam",
                SupplierPhone = "028-5416-0429",
                SupplierAddress = "Táº§ng 20, TÃ²a nhÃ  Mapletree Business Centre, 1060 Nguyá»…n VÄƒn Linh, Quáº­n 7, TP. HCM",
                StockInNo = "PNK-2025-0509",
                StockInDate = DateTime.Today.AddDays(-4),
                OrderPartNo = "PO-2025-0508",
                Category = "Phim cÃ¡ch nhiá»‡t Crystalline & HÃ³a cháº¥t phá»§ gáº§m ceramic",
                DebitDate = DateTime.Today.AddDays(-4),
                DueDate = DateTime.Today.AddDays(26),
                DebitAmount = 26_800_000,
                PaidAmount = 0,
                RemainAmount = 26_800_000,
                Status = SupplierDebitStatus.Pending,
                Note = "5 cuá»™n phim cÃ¡ch nhiá»‡t quang há»c 3M Crystalline vÃ  30 chai xá»‹t gáº§m chá»‘ng rá»‰",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-4)
            };

            var sd10 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-010",
                SupplierCode = "PPG-VN",
                SupplierName = "CÃ´ng ty TNHH SÆ¡n PPG Viá»‡t Nam",
                SupplierPhone = "0274-375-7260",
                SupplierAddress = "ÄÆ°á»ng sá»‘ 6, KCN Viá»‡t HÆ°Æ¡ng 1, Thá»‹ xÃ£ Thuáº­n An, BÃ¬nh DÆ°Æ¡ng",
                StockInNo = "PNK-2025-0510",
                StockInDate = DateTime.Today.AddDays(-1),
                OrderPartNo = "PO-2025-0515",
                Category = "SÆ¡n Ã´ tÃ´ cÃ´ng nghiá»‡p Deltron gá»‘c nÆ°á»›c & Dáº§u bÃ³ng 2K",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(29),
                DebitAmount = 17_500_000,
                PaidAmount = 0,
                RemainAmount = 17_500_000,
                Status = SupplierDebitStatus.Pending,
                Note = "SÆ¡n pha mÃ u vi tÃ­nh chuáº©n code mÃ u Hyundai Tráº¯ng Tuyáº¿t, Äen Ngá»c, Äá» Máº­n",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            db.SupplierDebits.AddRange(sd1, sd2, sd3, sd4, sd5, sd6, sd7, sd8, sd9, sd10);
            await db.SaveChangesAsync();

            // Phiáº¿u chi 1: Thanh toÃ¡n cho Mobis 65,000,000 VND (chuyá»ƒn khoáº£n VietinBank CTG)
            // PhÃ¢n bá»•: 35M cho PNK 1 (táº¥t toÃ¡n), 22.5M cho PNK 2 (táº¥t toÃ¡n), 7.5M cho PNK 3 (tráº£ má»™t pháº§n)
            var sp1 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-001",
                SupplierCode = "MOBIS-VN",
                SupplierName = "CÃ´ng ty TNHH Mobis Auto Parts Viá»‡t Nam",
                PayDate = DateTime.Today.AddDays(-10),
                PayPersonName = "Nguyá»…n VÄƒn HÃ¹ng (Káº¿ toÃ¡n cÃ´ng ná»£ Mobis)",
                PayPersonIDCardNo = "001088019283",
                PayPersonPhone = "024-3768-9988",
                PaymentAmount = 65_000_000,
                PaymentMethod = SupplierPaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - CN Äá»‘ng Äa",
                BankAccountNo = "113000088999",
                BankTxnRef = "UNC-CTG-202505-00981",
                TotalAllocated = 65_000_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Settled,
                Note = "Thanh toÃ¡n cÃ´ng ná»£ Ä‘á»£t 1 thÃ¡ng 05/2025 cho 3 phiáº¿u nháº­p kho phá»¥ tÃ¹ng chÃ­nh hÃ£ng",
                CreatedBy = "NguyenThiThu_KeToanThanhToan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today.AddDays(-10),
                SettledBy = "NguyenThiThu_KeToanThanhToan",
                SettledAt = DateTime.Today.AddDays(-10),
                CreatedAt = DateTime.Today.AddDays(-10)
            };
            db.SupplierPayments.Add(sp1);
            await db.SaveChangesAsync();

            db.SupplierPaymentDetails.AddRange(
                new SupplierPaymentDetail
                {
                    PaymentId = sp1.Id,
                    DebitId = sd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = sd1.DebitNo,
                    StockInNo = sd1.StockInNo,
                    OrderPartNo = sd1.OrderPartNo,
                    DebitAmount = 35_000_000,
                    DebitAmountBefore = 35_000_000,
                    PaymentDetailAmount = 35_000_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ phiáº¿u nháº­p kho"
                },
                new SupplierPaymentDetail
                {
                    PaymentId = sp1.Id,
                    DebitId = sd2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = sd2.DebitNo,
                    StockInNo = sd2.StockInNo,
                    OrderPartNo = sd2.OrderPartNo,
                    DebitAmount = 22_500_000,
                    DebitAmountBefore = 22_500_000,
                    PaymentDetailAmount = 22_500_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ phiáº¿u nháº­p kho"
                },
                new SupplierPaymentDetail
                {
                    PaymentId = sp1.Id,
                    DebitId = sd3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = sd3.DebitNo,
                    StockInNo = sd3.StockInNo,
                    OrderPartNo = sd3.OrderPartNo,
                    DebitAmount = 48_000_000,
                    DebitAmountBefore = 48_000_000,
                    PaymentDetailAmount = 7_500_000,
                    DebitAmountLeft = 40_500_000,
                    Remark = "Thanh toÃ¡n má»™t pháº§n ná»£ phiáº¿u nháº­p kho"
                }
            );

            // Phiáº¿u chi 2: Thanh toÃ¡n cho Bosch 18,500,000 VND (cá»•ng VNPay QR B2B)
            var sp2 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-002",
                SupplierCode = "BOSCH-VN",
                SupplierName = "CÃ´ng ty TNHH Robert Bosch Viá»‡t Nam",
                PayDate = DateTime.Today.AddDays(-6),
                PayPersonName = "Tráº§n Minh Äá»©c (Äáº¡i diá»‡n kinh doanh Bosch)",
                PayPersonIDCardNo = "079090012345",
                PayPersonPhone = "028-6258-3690",
                PaymentAmount = 18_500_000,
                PaymentMethod = SupplierPaymentMethod.VnPay,
                BankCode = "VCB",
                BankName = "Vietcombank - CN TÃ¢n Äá»‹nh",
                BankAccountNo = "0071008899221",
                BankTxnRef = "VNPAY-B2B-8839102",
                TotalAllocated = 18_500_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Settled,
                Note = "Thanh toÃ¡n qua cá»•ng VNPay QR hÃ³a Ä‘Æ¡n gáº¡t mÆ°a vÃ  bugi Bosch",
                CreatedBy = "NguyenThiThu_KeToanThanhToan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today.AddDays(-6),
                SettledBy = "NguyenThiThu_KeToanThanhToan",
                SettledAt = DateTime.Today.AddDays(-6),
                CreatedAt = DateTime.Today.AddDays(-6)
            };
            db.SupplierPayments.Add(sp2);
            await db.SaveChangesAsync();

            db.SupplierPaymentDetails.Add(
                new SupplierPaymentDetail
                {
                    PaymentId = sp2.Id,
                    DebitId = sd5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = sd5.DebitNo,
                    StockInNo = sd5.StockInNo,
                    OrderPartNo = sd5.OrderPartNo,
                    DebitAmount = 18_500_000,
                    DebitAmountBefore = 18_500_000,
                    PaymentDetailAmount = 18_500_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ phiáº¿u nháº­p kho"
                }
            );

            // Phiáº¿u chi 3: ÄÃ£ láº­p & Káº¿ toÃ¡n trÆ°á»Ÿng tháº©m Ä‘á»‹nh duyá»‡t chi cho Castrol 32,000,000 VND (chá» xuáº¥t tiá»n UNC Techcombank)
            var sp3 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-003",
                SupplierCode = "CASTROL-VN",
                SupplierName = "CÃ´ng ty TNHH Castrol BP Petco Viá»‡t Nam",
                PayDate = DateTime.Today,
                PayPersonName = "LÃª Quá»‘c Báº£o (Thá»§ quá»¹ Ä‘á»‘i tÃ¡c Castrol)",
                PayPersonIDCardNo = "079085023456",
                PayPersonPhone = "028-3821-9153",
                PaymentAmount = 32_000_000,
                PaymentMethod = SupplierPaymentMethod.BankTransfer,
                BankCode = "TCB",
                BankName = "Techcombank - Há»™i Sá»Ÿ ChÃ­nh",
                BankAccountNo = "19028833990011",
                BankTxnRef = "UNC-TCB-202505-00512",
                TotalAllocated = 32_000_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Confirmed,
                Note = "Káº¿ toÃ¡n trÆ°á»Ÿng Ä‘Ã£ duyá»‡t lá»‡nh chi thanh toÃ¡n tiá»n dáº§u nhá»n Castrol, chuáº©n bá»‹ Ä‘áº©y UNC",
                CreatedBy = "NguyenThiThu_KeToanThanhToan",
                ConfirmedBy = "TranDinhTuan_KeToanTruong",
                ConfirmedAt = DateTime.Today,
                CreatedAt = DateTime.Today
            };
            db.SupplierPayments.Add(sp3);
            await db.SaveChangesAsync();

            db.SupplierPaymentDetails.Add(
                new SupplierPaymentDetail
                {
                    PaymentId = sp3.Id,
                    DebitId = sd7.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = sd7.DebitNo,
                    StockInNo = sd7.StockInNo,
                    OrderPartNo = sd7.OrderPartNo,
                    DebitAmount = 32_000_000,
                    DebitAmountBefore = 32_000_000,
                    PaymentDetailAmount = 32_000_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ phiáº¿u nháº­p kho"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 21. Seed Dá»¯ liá»‡u máº«u CÃ´ng Ná»£ & Thu Tiá»n Quyáº¿t ToÃ¡n KhÃ¡ch HÃ ng Dá»‹ch Vá»¥ Sá»­a Chá»¯a (Customer Payment) =====
        if (!await db.CustomerDebits.AnyAsync())
        {
            var cd1 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-001",
                CusId = "CUS-001",
                CusName = "Nguyá»…n VÄƒn HÃ¹ng",
                Phone = "0912-345-678",
                Address = "Sá»‘ 15 Phá»‘ Duy TÃ¢n, Cáº§u Giáº¥y, HÃ  Ná»™i",
                RONo = "RO-202505-0101",
                RODate = DateTime.Today.AddDays(-14),
                PlateNo = "30H-889.92",
                VIN = "KMHSH81WPPU102911",
                ModelCode = "SANTAFE",
                ServiceType = "Báº£o dÆ°á»¡ng Ä‘á»‹nh ká»³ cáº¥p lá»›n 40.000km",
                DebitDate = DateTime.Today.AddDays(-14),
                DueDate = DateTime.Today.AddDays(1),
                DebitAmount = 8_500_000,
                PaidAmount = 8_500_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "Báº£o dÆ°á»¡ng cáº¥p lá»›n 4 váº¡n: thay dáº§u Ä‘á»™ng cÆ¡, lá»c dáº§u, lá»c giÃ³, dáº§u phanh vÃ  bugi",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-14)
            };

            var cd2 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-002",
                CusId = "CUS-001",
                CusName = "Nguyá»…n VÄƒn HÃ¹ng",
                Phone = "0912-345-678",
                Address = "Sá»‘ 15 Phá»‘ Duy TÃ¢n, Cáº§u Giáº¥y, HÃ  Ná»™i",
                RONo = "RO-202505-0205",
                RODate = DateTime.Today.AddDays(-10),
                PlateNo = "30H-889.92",
                VIN = "KMHSH81WPPU102911",
                ModelCode = "SANTAFE",
                ServiceType = "Sá»­a chá»¯a há»‡ thá»‘ng phanh",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(5),
                DebitAmount = 4_200_000,
                PaidAmount = 4_200_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "Thay mÃ¡ phanh Ä‘Ä©a gá»‘m trÆ°á»›c sau vÃ  lÃ¡ng Ä‘Ä©a phanh vi tÃ­nh",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var cd3 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-003",
                CusId = "CUS-002",
                CusName = "CÃ´ng ty CP Taxi Mai Linh Báº¯c Trung Bá»™",
                Phone = "024-3833-3333",
                Address = "Khu Ä‘Ã´ thá»‹ Trung HÃ²a - NhÃ¢n ChÃ­nh, Thanh XuÃ¢n, HÃ  Ná»™i",
                RONo = "RO-202505-0112",
                RODate = DateTime.Today.AddDays(-8),
                PlateNo = "29E-023.45",
                VIN = "KMHDH41CPPU298102",
                ModelCode = "ACCENT",
                ServiceType = "Äáº¡i tu há»™p sá»‘ & CÃ´n ly há»£p",
                DebitDate = DateTime.Today.AddDays(-8),
                DueDate = DateTime.Today.AddDays(7),
                DebitAmount = 28_000_000,
                PaidAmount = 15_000_000,
                RemainAmount = 13_000_000,
                Status = CustomerDebitStatus.PartiallyPaid,
                Note = "Háº¡ há»™p sá»‘ thay bÃ n Ã©p, lÃ¡ cÃ´n, bi tÃª vÃ  bÃ¡nh Ä‘Ã  xe há»£p Ä‘á»“ng taxi",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-8)
            };

            var cd4 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-004",
                CusId = "CUS-002",
                CusName = "CÃ´ng ty CP Taxi Mai Linh Báº¯c Trung Bá»™",
                Phone = "024-3833-3333",
                Address = "Khu Ä‘Ã´ thá»‹ Trung HÃ²a - NhÃ¢n ChÃ­nh, Thanh XuÃ¢n, HÃ  Ná»™i",
                RONo = "RO-202505-0118",
                RODate = DateTime.Today.AddDays(-6),
                PlateNo = "29E-044.88",
                VIN = "KMHDH41CPPU301984",
                ModelCode = "ACCENT",
                ServiceType = "Báº£o dÆ°á»¡ng 80.000km & Thay lá»‘p",
                DebitDate = DateTime.Today.AddDays(-6),
                DueDate = DateTime.Today.AddDays(9),
                DebitAmount = 16_500_000,
                PaidAmount = 0,
                RemainAmount = 16_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Báº£o dÆ°á»¡ng tá»•ng thá»ƒ vÃ  thay 4 quáº£ lá»‘p Michelin 185/65R15 xe taxi",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-6)
            };

            var cd5 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-005",
                CusId = "CUS-003",
                CusName = "Tráº§n Thá»‹ Mai PhÆ°Æ¡ng",
                Phone = "0988-123-456",
                Address = "Biá»‡t thá»± Vinhome Riverside, Long BiÃªn, HÃ  Ná»™i",
                RONo = "RO-202505-0125",
                RODate = DateTime.Today.AddDays(-5),
                PlateNo = "30K-128.66",
                VIN = "KMHTG81BAPU504123",
                ModelCode = "CRETA",
                ServiceType = "SÆ¡n gÃ² phá»¥c há»“i thÃ¢n vá»",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(10),
                DebitAmount = 6_800_000,
                PaidAmount = 6_800_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "GÃ² cáº£n trÆ°á»›c, sÆ¡n háº¥p buá»“ng sáº¥y ba Ä‘á» sá»‘c vÃ  tai xe bÃªn phá»¥",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var cd6 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-006",
                CusId = "CUS-004",
                CusName = "LÃª HoÃ ng Nam",
                Phone = "0904-567-890",
                Address = "TÃ²a nhÃ  Keangnam Landmark 72, Pháº¡m HÃ¹ng, Nam Tá»« LiÃªm, HÃ  Ná»™i",
                RONo = "RO-202505-0130",
                RODate = DateTime.Today.AddDays(-4),
                PlateNo = "30F-998.12",
                VIN = "KMHT381CMPU612844",
                ModelCode = "TUCSON",
                ServiceType = "Sá»­a chá»¯a Ä‘iá»u hÃ²a & Há»‡ thá»‘ng lÃ m mÃ¡t",
                DebitDate = DateTime.Today.AddDays(-4),
                DueDate = DateTime.Today.AddDays(11),
                DebitAmount = 14_500_000,
                PaidAmount = 0,
                RemainAmount = 14_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Thay lá»‘c Ä‘iá»u hÃ²a Hanon, vá»‡ sinh dÃ n láº¡nh vÃ  náº¡p ga R134a chuáº©n",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-4)
            };

            var cd7 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-007",
                CusId = "CUS-005",
                CusName = "CÃ´ng ty Váº­n táº£i & Du lá»‹ch An PhÃº",
                Phone = "024-3999-8888",
                Address = "Sá»‘ 48 HoÃ ng Quá»‘c Viá»‡t, Cáº§u Giáº¥y, HÃ  Ná»™i",
                RONo = "RO-202505-0142",
                RODate = DateTime.Today.AddDays(-3),
                PlateNo = "29B-512.34",
                VIN = "KMHTS81DAPU718290",
                ModelCode = "STARGAZER",
                ServiceType = "Báº£o dÆ°á»¡ng gáº§m & Há»‡ thá»‘ng treo",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 12_000_000,
                PaidAmount = 0,
                RemainAmount = 12_000_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Thay bá»™ giáº£m cháº¥n trÆ°á»›c sau, cao su cÃ¢n báº±ng vÃ  rotuyn lÃ¡i",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var cd8 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-008",
                CusId = "CUS-006",
                CusName = "VÅ© Minh TrÃ­",
                Phone = "0915-888-999",
                Address = "Chung cÆ° Mandarin Garden, HoÃ ng Minh GiÃ¡m, Cáº§u Giáº¥y, HÃ  Ná»™i",
                RONo = "RO-202505-0155",
                RODate = DateTime.Today.AddDays(-2),
                PlateNo = "30G-678.90",
                VIN = "KMHC881EAPU823901",
                ModelCode = "CUSTIN",
                ServiceType = "ChÄƒm sÃ³c & Phá»¥ kiá»‡n cao cáº¥p",
                DebitDate = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(13),
                DebitAmount = 18_200_000,
                PaidAmount = 0,
                RemainAmount = 18_200_000,
                Status = CustomerDebitStatus.Pending,
                Note = "DÃ¡n phim cÃ¡ch nhiá»‡t quang há»c 3M Crystalline vÃ  phá»§ gáº§m chá»‘ng rá»‰ cao cáº¥p",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-2)
            };

            var cd9 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-009",
                CusId = "CUS-007",
                CusName = "HoÃ ng Äá»©c Máº¡nh",
                Phone = "0936-777-222",
                Address = "Khu Ä‘Ã´ thá»‹ Ecopark, VÄƒn Giang, HÆ°ng YÃªn",
                RONo = "RO-202505-0160",
                RODate = DateTime.Today.AddDays(-1),
                PlateNo = "30H-456.78",
                VIN = "KMHEE81EAPU901844",
                ModelCode = "IONIQ5",
                ServiceType = "Báº£o dÆ°á»¡ng chuyÃªn biá»‡t xe Ä‘iá»‡n EV",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(14),
                DebitAmount = 5_500_000,
                PaidAmount = 0,
                RemainAmount = 5_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Kiá»ƒm tra há»‡ thá»‘ng quáº£n lÃ½ pin BMS, cáº­p nháº­t ECU vÃ  kiá»ƒm tra phanh há»“i nÄƒng lÆ°á»£ng",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            var cd10 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-010",
                CusId = "CUS-008",
                CusName = "Pháº¡m Thu HÆ°Æ¡ng",
                Phone = "0979-333-555",
                Address = "Sá»‘ 88 Phá»‘ Huáº¿, Hai BÃ  TrÆ°ng, HÃ  Ná»™i",
                RODate = DateTime.Today,
                PlateNo = "30A-789.01",
                VIN = "KMHDH41DPPU410294",
                ModelCode = "ELANTRA",
                RONo = "RO-202505-0175",
                ServiceType = "CÄƒn chá»‰nh thÆ°á»›c lÃ¡i & Lá»‘p xe",
                DebitDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 7_200_000,
                PaidAmount = 0,
                RemainAmount = 7_200_000,
                Status = CustomerDebitStatus.Pending,
                Note = "CÃ¢n chá»‰nh gÃ³c Ä‘áº·t bÃ¡nh xe 3D vÃ  thay 2 quáº£ lá»‘p trÆ°á»›c Bridgestone 205/55R16",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today
            };

            db.CustomerDebits.AddRange(cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10);
            await db.SaveChangesAsync();

            // Phiáº¿u thu 1: Táº¥t toÃ¡n 2 lá»‡nh sá»­a chá»¯a cho anh Nguyá»…n VÄƒn HÃ¹ng qua Chuyá»ƒn khoáº£n VietQR
            var cp1 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-001",
                CusId = "CUS-001",
                CusName = "Nguyá»…n VÄƒn HÃ¹ng",
                CusPhone = "0912-345-678",
                PlateNo = "30H-889.92",
                PayDate = DateTime.Today.AddDays(-9),
                PayPersonName = "Nguyá»…n VÄƒn HÃ¹ng",
                PayPersonIDCardNo = "001085002931",
                PayPersonPhone = "0912-345-678",
                PaymentAmount = 12_700_000,
                PaymentMethod = CustomerPaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - Chi nhÃ¡nh Nam ThÄƒng Long",
                BankAccountNo = "113000889988",
                BankTxnRef = "FT251299834211",
                TotalAllocated = 12_700_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Settled,
                Note = "Thu tiá»n thanh toÃ¡n dá»‹ch vá»¥ báº£o dÆ°á»¡ng vÃ  mÃ¡ phanh qua quÃ©t mÃ£ VietQR",
                CreatedBy = "LeThiHuyen_ThuNgan",
                ConfirmedBy = "LeThiHuyen_ThuNgan",
                ConfirmedAt = DateTime.Today.AddDays(-9),
                SettledBy = "TranDinhTuan_KeToanTruong",
                SettledAt = DateTime.Today.AddDays(-9),
                CreatedAt = DateTime.Today.AddDays(-9)
            };
            db.CustomerPayments.Add(cp1);
            await db.SaveChangesAsync();

            db.CustomerPaymentDetails.AddRange(
                new CustomerPaymentDetail
                {
                    PaymentId = cp1.Id,
                    DebitId = cd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = cd1.DebitNo,
                    RONo = cd1.RONo,
                    PlateNo = cd1.PlateNo,
                    VIN = cd1.VIN,
                    DebitAmount = 8_500_000,
                    DebitAmountBefore = 8_500_000,
                    PaymentDetailAmount = 8_500_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ lá»‡nh sá»­a chá»¯a RO-202505-0101"
                },
                new CustomerPaymentDetail
                {
                    PaymentId = cp1.Id,
                    DebitId = cd2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = cd2.DebitNo,
                    RONo = cd2.RONo,
                    PlateNo = cd2.PlateNo,
                    VIN = cd2.VIN,
                    DebitAmount = 4_200_000,
                    DebitAmountBefore = 4_200_000,
                    PaymentDetailAmount = 4_200_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ lá»‡nh sá»­a chá»¯a RO-202505-0205"
                }
            );

            // Phiáº¿u thu 2: Táº¥t toÃ¡n chi phÃ­ sÆ¡n gÃ² cho chá»‹ Tráº§n Thá»‹ Mai PhÆ°Æ¡ng qua VNPay QR
            var cp2 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-002",
                CusId = "CUS-003",
                CusName = "Tráº§n Thá»‹ Mai PhÆ°Æ¡ng",
                CusPhone = "0988-123-456",
                PlateNo = "30K-128.66",
                PayDate = DateTime.Today.AddDays(-5),
                PayPersonName = "Tráº§n Thá»‹ Mai PhÆ°Æ¡ng",
                PayPersonIDCardNo = "001192004821",
                PayPersonPhone = "0988-123-456",
                PaymentAmount = 6_800_000,
                PaymentMethod = CustomerPaymentMethod.VnPay,
                BankCode = "VNPAY",
                BankName = "Cá»•ng thanh toÃ¡n VNPay QR Pos",
                BankTxnRef = "VNPAY-POS-202505-8831",
                TotalAllocated = 6_800_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Settled,
                Note = "KhÃ¡ch hÃ ng thanh toÃ¡n qua cá»•ng VNPay QR táº¡i quáº§y thu ngÃ¢n dá»‹ch vá»¥",
                CreatedBy = "LeThiHuyen_ThuNgan",
                ConfirmedBy = "LeThiHuyen_ThuNgan",
                ConfirmedAt = DateTime.Today.AddDays(-5),
                SettledBy = "TranDinhTuan_KeToanTruong",
                SettledAt = DateTime.Today.AddDays(-5),
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.CustomerPayments.Add(cp2);
            await db.SaveChangesAsync();

            db.CustomerPaymentDetails.Add(
                new CustomerPaymentDetail
                {
                    PaymentId = cp2.Id,
                    DebitId = cd5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = cd5.DebitNo,
                    RONo = cd5.RONo,
                    PlateNo = cd5.PlateNo,
                    VIN = cd5.VIN,
                    DebitAmount = 6_800_000,
                    DebitAmountBefore = 6_800_000,
                    PaymentDetailAmount = 6_800_000,
                    DebitAmountLeft = 0,
                    Remark = "Táº¥t toÃ¡n toÃ n bá»™ ná»£ sÆ¡n cáº£n trÆ°á»›c lá»‡nh RO-202505-0125"
                }
            );

            // Phiáº¿u thu 3: Thanh toÃ¡n má»™t pháº§n cho CÃ´ng ty Taxi Mai Linh Báº¯c Trung Bá»™ qua Techcombank UNC
            var cp3 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-003",
                CusId = "CUS-002",
                CusName = "CÃ´ng ty CP Taxi Mai Linh Báº¯c Trung Bá»™",
                CusPhone = "024-3833-3333",
                PlateNo = "29E-023.45",
                PayDate = DateTime.Today,
                PayPersonName = "HoÃ ng Kim Tuáº¥n (Káº¿ toÃ¡n Ä‘á»™i xe Mai Linh)",
                PayPersonIDCardNo = "036087001294",
                PayPersonPhone = "0903-222-111",
                PaymentAmount = 15_000_000,
                PaymentMethod = CustomerPaymentMethod.BankTransfer,
                BankCode = "TCB",
                BankName = "Techcombank - Há»™i Sá»Ÿ",
                BankAccountNo = "19028833990011",
                BankTxnRef = "UNC-TCB-CUS-202505-092",
                TotalAllocated = 15_000_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Confirmed,
                Note = "Thu ngÃ¢n xÃ¡c nháº­n tiá»n ná»•i vÃ o tÃ i khoáº£n Techcombank, trá»« ná»£ Ä‘á»£t 1 lá»‡nh Ä‘áº¡i tu há»™p sá»‘ RO-202505-0112",
                CreatedBy = "LeThiHuyen_ThuNgan",
                ConfirmedBy = "LeThiHuyen_ThuNgan",
                ConfirmedAt = DateTime.Today,
                CreatedAt = DateTime.Today
            };
            db.CustomerPayments.Add(cp3);
            await db.SaveChangesAsync();

            db.CustomerPaymentDetails.Add(
                new CustomerPaymentDetail
                {
                    PaymentId = cp3.Id,
                    DebitId = cd3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DebitNo = cd3.DebitNo,
                    RONo = cd3.RONo,
                    PlateNo = cd3.PlateNo,
                    VIN = cd3.VIN,
                    DebitAmount = 28_000_000,
                    DebitAmountBefore = 28_000_000,
                    PaymentDetailAmount = 15_000_000,
                    DebitAmountLeft = 13_000_000,
                    Remark = "Trá»« ná»£ Ä‘á»£t 1 (15 triá»‡u / 28 triá»‡u), cÃ²n ná»£ 13 triá»‡u"
                }
            );

            await db.SaveChangesAsync();
        }

        // Seed dá»¯ liá»‡u máº«u cho Quáº£n lÃ½ BiÃªn Báº£n Thá»a Thuáº­n Há»§y Há»£p Äá»“ng & Quyáº¿t ToÃ¡n NghÄ©a Vá»¥ TÃ i ChÃ­nh (BizHTC.Contract / DMS40.Contract)
        if (!await db.ContractCancellations.AnyAsync())
        {
            // 1. BiÃªn báº£n hoÃ n táº¥t quyáº¿t toÃ¡n hoÃ n cá»c 100% qua UNC VietinBank (Settled)
            var cc1 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-001",
                DlrContractNo = "HDMB-2025-HN01",
                DealerCode = "VN001",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai HÃ  Ná»™i",
                CancelDate = DateTime.Today.AddDays(-10),
                SettlementType = ContractCancelSettlementType.RefundDeposit,
                BankCode = "CTG",
                BankName = "NgÃ¢n hÃ ng TMCP CÃ´ng ThÆ°Æ¡ng Viá»‡t Nam (VietinBank)",
                BankGuaranteeNo = null,
                TransferContractNo = null,
                TotalVehicles = 2,
                TotalContractAmount = 2_190_000_000,
                TotalDepositPaid = 328_500_000,
                TotalRefundAmount = 328_500_000,
                TotalPenaltyAmount = 0,
                TotalGuaranteeRelease = 0,
                Status = ContractCancelStatus.Settled,
                CancelReason = "HÃ£ng Ä‘iá»u chá»‰nh káº¿ hoáº¡ch sáº£n xuáº¥t linh kiá»‡n toÃ n cáº§u, khÃ´ng ká»‹p giao xe Ä‘Ãºng tiáº¿n Ä‘á»™ cam káº¿t. Hai bÃªn thá»‘ng nháº¥t thá»a thuáº­n há»§y phá»¥ lá»¥c há»£p Ä‘á»“ng vÃ  HTC hoÃ n láº¡i 100% tiá»n cá»c cho Ä‘áº¡i lÃ½.",
                BankTxnRef = "UNC-CTG-REF-202505-0019",
                SettledBy = "NguyenVanQuyet_KeToanThanhToan",
                SettledAt = DateTime.Today.AddDays(-8),
                ApprovedBy = "TranMinhDuc_PhoTongGiamDocKinhDoanh",
                ApprovedAt = DateTime.Today.AddDays(-9),
                Remark = "ÄÃ£ xuáº¥t lá»‡nh chi UNC chuyá»ƒn khoáº£n VietinBank tÃ i khoáº£n thá»¥ hÆ°á»Ÿng cá»§a Ä‘áº¡i lÃ½ thÃ nh cÃ´ng.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-10)
            };
            db.ContractCancellations.Add(cc1);
            await db.SaveChangesAsync();

            db.ContractCancelDetails.AddRange(
                new ContractCancelDetail
                {
                    CancellationId = cc1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc1.ContractCancelNo,
                    VIN = "KMHSH81DPNU100881",
                    CarId = "CAR-SANTAFE-0881",
                    ModelCode = "SANTAFE",
                    ModelName = "Hyundai Santa Fe 2.5 AWD Cao Cáº¥p",
                    SpecCode = "SF-2.5-PRE",
                    ColorCode = "TRANG",
                    UnitPrice = 1_450_000_000,
                    DepositPaid = 217_500_000,
                    RefundAmount = 217_500_000,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Settled,
                    Remark = "HoÃ n cá»c 15% do Ä‘á»©t gÃ£y chuá»—i cung á»©ng linh kiá»‡n radar"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc1.ContractCancelNo,
                    VIN = "KMHCT81EPNU100882",
                    CarId = "CAR-CRETA-0882",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CR-1.5-PRE",
                    ColorCode = "DO",
                    UnitPrice = 740_000_000,
                    DepositPaid = 111_000_000,
                    RefundAmount = 111_000_000,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Settled,
                    Remark = "HoÃ n cá»c 15% thá»a thuáº­n há»§y Ä‘Æ¡n hÃ ng Ä‘áº¡i lÃ½"
                }
            );

            // 2. BiÃªn báº£n pháº¡t cá»c do Ä‘áº¡i lÃ½ cháº­m ná»™p tiá»n thanh toÃ¡n (Approved, chá» quyáº¿t toÃ¡n)
            var cc2 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-002",
                DlrContractNo = "HDMB-2025-VH03",
                DealerCode = "VN040",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai Viá»‡t HÃ n (SÃ i GÃ²n)",
                CancelDate = DateTime.Today.AddDays(-4),
                SettlementType = ContractCancelSettlementType.ForfeitDeposit,
                BankCode = null,
                BankName = null,
                BankGuaranteeNo = null,
                TransferContractNo = null,
                TotalVehicles = 2,
                TotalContractAmount = 1_580_000_000,
                TotalDepositPaid = 237_000_000,
                TotalRefundAmount = 0,
                TotalPenaltyAmount = 237_000_000,
                TotalGuaranteeRelease = 0,
                Status = ContractCancelStatus.Approved,
                CancelReason = "Äáº¡i lÃ½ vi pháº¡m nghiÃªm trá»ng cam káº¿t tiáº¿n Ä‘á»™ thanh toÃ¡n xe (quÃ¡ háº¡n 45 ngÃ y ká»ƒ tá»« ngÃ y thÃ´ng bÃ¡o giao xe). CÄƒn cá»© Äiá»u 8 Há»£p Ä‘á»“ng phÃ¢n phá»‘i Ä‘áº¡i lÃ½, HTC quyáº¿t Ä‘á»‹nh há»§y há»£p Ä‘á»“ng vÃ  tá»‹ch thu toÃ n bá»™ tiá»n cá»c ná»™p pháº¡t.",
                BankTxnRef = null,
                ApprovedBy = "PhamQuangHai_GiamDocTaiChinh",
                ApprovedAt = DateTime.Today.AddDays(-2),
                Remark = "LÃ£nh Ä‘áº¡o Ä‘Ã£ kÃ½ duyá»‡t pháº¡t cá»c, chuyá»ƒn phÃ²ng Káº¿ toÃ¡n háº¡ch toÃ¡n sung doanh thu pháº¡t vi pháº¡m há»£p Ä‘á»“ng.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-4)
            };
            db.ContractCancellations.Add(cc2);
            await db.SaveChangesAsync();

            db.ContractCancelDetails.AddRange(
                new ContractCancelDetail
                {
                    CancellationId = cc2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc2.ContractCancelNo,
                    VIN = "KMHTU81DPNU200115",
                    CarId = "CAR-TUCSON-0115",
                    ModelCode = "TUCSON",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Turbo AWD",
                    SpecCode = "TUC-1.6T-PRE",
                    ColorCode = "DEN",
                    UnitPrice = 1_020_000_000,
                    DepositPaid = 153_000_000,
                    RefundAmount = 0,
                    PenaltyAmount = 153_000_000,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Approved,
                    Remark = "Pháº¡t cá»c 15% vi pháº¡m thá»i háº¡n ná»™p tiá»n thanh toÃ¡n dá»©t Ä‘iá»ƒm"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc2.ContractCancelNo,
                    VIN = "KMHAC81EPNU200116",
                    CarId = "CAR-ACCENT-0116",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5-DB",
                    ColorCode = "BAC",
                    UnitPrice = 560_000_000,
                    DepositPaid = 84_000_000,
                    RefundAmount = 0,
                    PenaltyAmount = 84_000_000,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Approved,
                    Remark = "Pháº¡t cá»c 15% vi pháº¡m nháº­n xe theo thÃ´ng bÃ¡o sá»‘ TB-GIAOXE-8839"
                }
            );

            // 3. BiÃªn báº£n giáº£i phÃ³ng báº£o lÃ£nh ngÃ¢n hÃ ng VPBank (Submitted, Ä‘ang trÃ¬nh duyá»‡t)
            var cc3 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-003",
                DlrContractNo = "HDMB-2025-DA08",
                DealerCode = "VN005",
                DealerName = "CÃ´ng ty CP ThÆ°Æ¡ng máº¡i Dá»‹ch vá»¥ Hyundai ÄÃ´ng Anh",
                CancelDate = DateTime.Today.AddDays(-2),
                SettlementType = ContractCancelSettlementType.ReleaseGuarantee,
                BankCode = "VPB",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank)",
                BankGuaranteeNo = "BLTT-VPB-2025-9982",
                TransferContractNo = null,
                TotalVehicles = 2,
                TotalContractAmount = 3_150_000_000,
                TotalDepositPaid = 472_500_000,
                TotalRefundAmount = 0,
                TotalPenaltyAmount = 0,
                TotalGuaranteeRelease = 2_677_500_000,
                Status = ContractCancelStatus.Submitted,
                CancelReason = "KhÃ¡ch hÃ ng mua lÃ´ doanh nghiá»‡p cá»§a Ä‘áº¡i lÃ½ thay Ä‘á»•i nhu cáº§u sang xe thÆ°Æ¡ng máº¡i táº£i náº·ng. Äáº¡i lÃ½ Ä‘á» nghá»‹ há»§y giao xe phá»¥ lá»¥c HDMB-2025-DA08 vÃ  Ä‘á» nghá»‹ HTC phÃ¡t hÃ nh vÄƒn báº£n giáº£i tá»a ThÆ° báº£o lÃ£nh sá»‘ BLTT-VPB-2025-9982 Ä‘á»ƒ hoÃ n tráº£ háº¡n má»©c tÃ­n dá»¥ng cho Ä‘áº¡i lÃ½.",
                BankTxnRef = null,
                Remark = "Há»“ sÆ¡ Ä‘áº§y Ä‘á»§ cÃ´ng vÄƒn giáº£i trÃ¬nh cá»§a Ä‘áº¡i lÃ½ vÃ  biÃªn báº£n lÃ m viá»‡c giá»¯a 2 bÃªn. ÄÃ£ trÃ¬nh Ban GiÃ¡m Ä‘á»‘c tháº©m Ä‘á»‹nh.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-2)
            };
            db.ContractCancellations.Add(cc3);
            await db.SaveChangesAsync();

            db.ContractCancelDetails.AddRange(
                new ContractCancelDetail
                {
                    CancellationId = cc3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc3.ContractCancelNo,
                    VIN = "KMHPA81DPNU300551",
                    CarId = "CAR-PALISADE-0551",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade 2.2D Exclusive AWD",
                    SpecCode = "PAL-2.2-EXC",
                    ColorCode = "XANH",
                    UnitPrice = 1_870_000_000,
                    DepositPaid = 280_500_000,
                    RefundAmount = 0,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 1_589_500_000,
                    Status = ContractCancelDetailStatus.Pending,
                    Remark = "Giáº£i phÃ³ng nghÄ©a vá»¥ báº£o lÃ£nh 85% giÃ¡ trá»‹ xe táº¡i VPBank"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc3.ContractCancelNo,
                    VIN = "KMHCU81EPNU300552",
                    CarId = "CAR-CUSTIN-0552",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cáº¥p",
                    SpecCode = "CUS-2.0T-PRE",
                    ColorCode = "TRANG",
                    UnitPrice = 1_280_000_000,
                    DepositPaid = 192_000_000,
                    RefundAmount = 0,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 1_088_000_000,
                    Status = ContractCancelDetailStatus.Pending,
                    Remark = "Giáº£i phÃ³ng nghÄ©a vá»¥ báº£o lÃ£nh 85% giÃ¡ trá»‹ xe táº¡i VPBank"
                }
            );

            // 4. BiÃªn báº£n chuyá»ƒn tiá»n cá»c sang phá»¥ lá»¥c há»£p Ä‘á»“ng xe khÃ¡c (Draft)
            var cc4 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-004",
                DlrContractNo = "HDMB-2025-HP05",
                DealerCode = "VN018",
                DealerName = "CÃ´ng ty TNHH Hyundai Háº£i PhÃ²ng",
                CancelDate = DateTime.Today,
                SettlementType = ContractCancelSettlementType.TransferDeposit,
                BankCode = null,
                BankName = null,
                BankGuaranteeNo = null,
                TransferContractNo = "HDMB-2025-HP12",
                TotalVehicles = 1,
                TotalContractAmount = 1_450_000_000,
                TotalDepositPaid = 217_500_000,
                TotalRefundAmount = 0,
                TotalPenaltyAmount = 0,
                TotalGuaranteeRelease = 0,
                Status = ContractCancelStatus.Draft,
                CancelReason = "Äáº¡i lÃ½ vÃ  khÃ¡ch hÃ ng thá»a thuáº­n chuyá»ƒn Ä‘á»•i sang Ä‘áº·t mua dÃ²ng xe thuáº§n Ä‘iá»‡n IONIQ 5 tháº¿ há»‡ má»›i theo phá»¥ lá»¥c há»£p Ä‘á»“ng HDMB-2025-HP12, xin chuyá»ƒn toÃ n bá»™ 217,500,000 VND tiá»n cá»c Ä‘Ã£ ná»™p sang há»£p Ä‘á»“ng má»›i.",
                Remark = "Dá»± tháº£o biÃªn báº£n thá»a thuáº­n chuyá»ƒn cá»c, Ä‘ang hoÃ n thiá»‡n há»“ sÆ¡ Ä‘á»‘i chiáº¿u phiáº¿u thu tiá»n cá»c ban Ä‘áº§u.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today
            };
            db.ContractCancellations.Add(cc4);
            await db.SaveChangesAsync();

            db.ContractCancelDetails.Add(
                new ContractCancelDetail
                {
                    CancellationId = cc4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc4.ContractCancelNo,
                    VIN = "KMHIO81DPNU400991",
                    CarId = "CAR-IONIQ5-0991",
                    ModelCode = "IONIQ5",
                    ModelName = "Hyundai Ioniq 5 Prestige EV",
                    SpecCode = "IO5-EV-PRE",
                    ColorCode = "XAM",
                    UnitPrice = 1_450_000_000,
                    DepositPaid = 217_500_000,
                    RefundAmount = 0,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 0,
                    TransferContractNo = "HDMB-2025-HP12",
                    Status = ContractCancelDetailStatus.Pending,
                    Remark = "Chuyá»ƒn tiá»n cá»c sang há»£p Ä‘á»“ng má»›i HDMB-2025-HP12"
                }
            );

            await db.SaveChangesAsync();
        }

        // Seed dá»¯ liá»‡u máº«u cho Quáº£n lÃ½ Há»“ sÆ¡ Äá» nghá»‹ MÆ°á»£n / BÃ n Giao Chá»©ng Tá»« Gá»‘c Xe Ã” TÃ´ & XÃ¡c Nháº­n NgÃ¢n HÃ ng (BizHTC.Car.Profile.cs / Car_DocReqList / FrmMngDocReq)
        if (!await db.CarDocRequests.AnyAsync())
        {
            // Há»“ sÆ¡ 1: BÃ n giao chá»©ng tá»« xe hoÃ n táº¥t thanh toÃ¡n (TypeCRR = Normal, Status = HandedOver)
            var req1 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-001",
                DealerCode = "DLR-PVD",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai Pháº¡m VÄƒn Äá»“ng",
                BankCode = null,
                BankName = null,
                TypeCRR = CarDocReqType.Normal,
                LetterRepresentationNo = "GGT-2025/05/PVD-01",
                LetterRepresentationDate = DateTime.Today.AddDays(-5),
                RepresentativeName = "Nguyá»…n VÄƒn HÆ°ng",
                RepresentativeIdCard = "001092004812",
                RepresentativePhone = "0912345678",
                TotalVehicles = 2,
                TotalCarAmount = 2_150_000_000,
                TotalPaymentAmount = 2_150_000_000,
                TotalGuaranteeAmount = 0,
                AvgDutyCompletePercent = 100.0m,
                Status = CarDocReqStatus.HandedOver,
                ApprovedBy1 = "TrinhVanBinh_ChuyenVienCongNo",
                ApprovedDate1 = DateTime.Today.AddDays(-4),
                ApprovedBy2 = "PhamQuangHuy_PhoTGDKD",
                ApprovedDate2 = DateTime.Today.AddDays(-3),
                HandoverDate = DateTime.Today.AddDays(-2),
                HandedOverBy = "LeVanThang_ThuKhoChungTu",
                HandoverRecipient = "Nguyá»…n VÄƒn HÆ°ng",
                Remark = "Äáº¡i lÃ½ Ä‘Ã£ hoÃ n táº¥t thanh toÃ¡n 100% tiá»n mua 02 xe Santa Fe qua UNC ngÃ¢n hÃ ng VietinBank. Há»“ sÆ¡ gá»‘c CO/CQ Ä‘Ã£ xuáº¥t giao Ä‘á»§ cho Ä‘áº¡i diá»‡n Ä‘áº¡i lÃ½ nháº­n lÃ m thá»§ tá»¥c bÃ n giao khÃ¡ch.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-5)
            };
            db.CarDocRequests.Add(req1);
            await db.SaveChangesAsync();

            db.CarDocRequestDetails.AddRange(
                new CarDocReqDetail
                {
                    DocReqId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req1.DRListCode,
                    VIN = "KMHEE81WBPU018821",
                    CarId = "CAR-STA-018821",
                    ModelCode = "SANTAFE",
                    ModelName = "Hyundai Santa Fe Calligraphy 2.5 AWD",
                    SpecCode = "STA-2.5-CAL",
                    EngineNo = "G4KP-PU018821",
                    ColorNameVN = "Tráº¯ng Tinh KhÃ´i",
                    ContractNo = "HDMB-2025-PVD01",
                    UnitPriceActual = 1_280_000_000,
                    PaymentPercent = 100.0m,
                    DepositPercent = 20.0m,
                    GuaranteePercent = 0m,
                    DutyCompletePercent = 100.0m,
                    CONo = "CO-2025-STA-8821",
                    CQNo = "CQ-2025-STA-8821",
                    HTCInvoiceNo = "HD-2025-001289",
                    DocumentsGiven = "Báº£n gá»‘c CO, Báº£n sao CQ, Giáº¥y kiá»ƒm Ä‘á»‹nh xuáº¥t xÆ°á»Ÿng, HÃ³a Ä‘Æ¡n VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req1.HandoverDate,
                    Remark = "ÄÃ£ xuáº¥t giao há»“ sÆ¡ gá»‘c hoÃ n táº¥t"
                },
                new CarDocReqDetail
                {
                    DocReqId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req1.DRListCode,
                    VIN = "KMHCT81DAPU029944",
                    CarId = "CAR-CRE-029944",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5-PRE",
                    EngineNo = "G4FL-PU029944",
                    ColorNameVN = "Äá» Quyáº¿n RÅ©",
                    ContractNo = "HDMB-2025-PVD01",
                    UnitPriceActual = 870_000_000,
                    PaymentPercent = 100.0m,
                    DepositPercent = 20.0m,
                    GuaranteePercent = 0m,
                    DutyCompletePercent = 100.0m,
                    CONo = "CO-2025-CRE-9944",
                    CQNo = "CQ-2025-CRE-9944",
                    HTCInvoiceNo = "HD-2025-001290",
                    DocumentsGiven = "Báº£n gá»‘c CO, Báº£n sao CQ, HÃ³a Ä‘Æ¡n VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req1.HandoverDate,
                    Remark = "ÄÃ£ xuáº¥t giao há»“ sÆ¡ gá»‘c hoÃ n táº¥t"
                }
            );

            // Há»“ sÆ¡ 2: BÃ n giao theo ThÆ° báº£o lÃ£nh ngÃ¢n hÃ ng tÃ i trá»£ Techcombank (TypeCRR = Special, Status = Approved2)
            var req2 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-002",
                DealerCode = "DLR-DA",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai ÄÃ´ng Anh",
                BankCode = "TCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ká»¹ ThÆ°Æ¡ng Viá»‡t Nam (Techcombank) - CN ThÄƒng Long",
                TypeCRR = CarDocReqType.Special,
                LetterRepresentationNo = "GGT-2025/05/DA-08",
                LetterRepresentationDate = DateTime.Today.AddDays(-3),
                RepresentativeName = "Tráº§n Háº£i ÄÄƒng",
                RepresentativeIdCard = "001088009123",
                RepresentativePhone = "0988776655",
                TotalVehicles = 2,
                TotalCarAmount = 1_870_000_000,
                TotalPaymentAmount = 561_000_000,
                TotalGuaranteeAmount = 1_309_000_000,
                AvgDutyCompletePercent = 100.0m,
                Status = CarDocReqStatus.Approved2,
                ApprovedBy1 = "TrinhVanBinh_ChuyenVienCongNo",
                ApprovedDate1 = DateTime.Today.AddDays(-2),
                BankApprovedBy = "NguyenPhuongThao_CBTD_TCB",
                BankApprovedAt = DateTime.Today.AddDays(-1),
                ApprovedBy2 = "PhamQuangHuy_PhoTGDKD",
                ApprovedDate2 = DateTime.Today,
                Remark = "Äáº¡i lÃ½ Ä‘Ã£ thanh toÃ¡n 30% tiá»n cá»c vÃ  cÃ³ ThÆ° báº£o lÃ£nh Techcombank sá»‘ BL-2025-TCB-8821 báº£o lÃ£nh 70% giÃ¡ trá»‹ há»£p Ä‘á»“ng. NgÃ¢n hÃ ng Ä‘Ã£ tháº©m Ä‘á»‹nh vÃ  xÃ¡c nháº­n cháº¥p thuáº­n giáº£i phÃ³ng há»“ sÆ¡ xe. ÄÃ£ cÃ³ lá»‡nh xuáº¥t kÃ©t A2.",
                CreatedBy = "NguyenVanTam_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-3)
            };
            db.CarDocRequests.Add(req2);
            await db.SaveChangesAsync();

            db.CarDocRequestDetails.AddRange(
                new CarDocReqDetail
                {
                    DocReqId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req2.DRListCode,
                    VIN = "KMHJT81WAPU044120",
                    CarId = "CAR-TUC-044120",
                    ModelCode = "TUCSON",
                    ModelName = "Hyundai Tucson 1.6T HTRAC Turbo",
                    SpecCode = "TUC-1.6T-TURBO",
                    EngineNo = "G4FP-PU044120",
                    ColorNameVN = "Äen Sang Trá»ng",
                    ContractNo = "HDMB-2025-TC01",
                    UnitPriceActual = 1_040_000_000,
                    PaymentPercent = 30.0m,
                    DepositPercent = 30.0m,
                    GuaranteePercent = 70.0m,
                    DutyCompletePercent = 100.0m,
                    BankGuaranteeNo = "BL-2025-TCB-8821",
                    CONo = "CO-2025-TUC-4120",
                    CQNo = "CQ-2025-TUC-4120",
                    HTCInvoiceNo = "HD-2025-001305",
                    DocumentsGiven = "Báº£n gá»‘c CO, Báº£n sao CQ, Tá» khai HQ, HÃ³a Ä‘Æ¡n VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "NguyenPhuongThao_CBTD_TCB",
                    BankApprDTime = DateTime.Today.AddDays(-1),
                    BankApprNote = "Techcombank ThÄƒng Long xÃ¡c nháº­n báº£o lÃ£nh há»£p lá»‡ cho sá»‘ khung KMHJT81WAPU044120",
                    Status = CarDocReqDetailStatus.Approved2,
                    Remark = "Chá» thá»§ kho xuáº¥t giao chá»©ng tá»« cho cÃ¡n bá»™ ngÃ¢n hÃ ng kÃ½ nháº­n"
                },
                new CarDocReqDetail
                {
                    DocReqId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req2.DRListCode,
                    VIN = "KMHAC81CBPU077112",
                    CarId = "CAR-ACC-077112",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai All-New Accent 1.5 AT Äáº·c Biá»‡t",
                    SpecCode = "ACC-1.5-AT-DB",
                    EngineNo = "G4FA-PU077112",
                    ColorNameVN = "Báº¡c Ãnh Kim",
                    ContractNo = "HDMB-2025-TC01",
                    UnitPriceActual = 830_000_000,
                    PaymentPercent = 30.0m,
                    DepositPercent = 30.0m,
                    GuaranteePercent = 70.0m,
                    DutyCompletePercent = 100.0m,
                    BankGuaranteeNo = "BL-2025-TCB-8821",
                    CONo = "CO-2025-ACC-7112",
                    CQNo = "CQ-2025-ACC-7112",
                    HTCInvoiceNo = "HD-2025-001306",
                    DocumentsGiven = "Báº£n gá»‘c CO, Báº£n sao CQ, HÃ³a Ä‘Æ¡n VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "NguyenPhuongThao_CBTD_TCB",
                    BankApprDTime = DateTime.Today.AddDays(-1),
                    BankApprNote = "Techcombank ThÄƒng Long xÃ¡c nháº­n báº£o lÃ£nh há»£p lá»‡ cho sá»‘ khung KMHAC81CBPU077112",
                    Status = CarDocReqDetailStatus.Approved2,
                    Remark = "Chá» thá»§ kho xuáº¥t giao chá»©ng tá»« cho cÃ¡n bá»™ ngÃ¢n hÃ ng kÃ½ nháº­n"
                }
            );

            // Há»“ sÆ¡ 3: Äáº¡i lÃ½ mÆ°á»£n há»“ sÆ¡ gá»‘c lÃ m thá»§ tá»¥c Ä‘Äƒng kÃ½ xe trÆ°á»›c cho khÃ¡ch (TypeCRR = Dealer, Status = HandedOver kÃ¨m háº¡n tráº£ kÃ©t)
            var req3 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-003",
                DealerCode = "DLR-SG",
                DealerName = "CÃ´ng ty CP Hyundai SÃ i GÃ²n PhÃ¢n Phá»‘i",
                BankCode = "VCB",
                BankName = "NgÃ¢n hÃ ng TMCP Ngoáº¡i ThÆ°Æ¡ng Viá»‡t Nam (Vietcombank) - CN Ká»³ Äá»“ng",
                TypeCRR = CarDocReqType.Dealer,
                LetterRepresentationNo = "GGT-2025/05/SG-14",
                LetterRepresentationDate = DateTime.Today.AddDays(-7),
                RepresentativeName = "HoÃ ng Minh TrÃ­",
                RepresentativeIdCard = "079090001245",
                RepresentativePhone = "0903334455",
                TotalVehicles = 1,
                TotalCarAmount = 1_580_000_000,
                TotalPaymentAmount = 474_000_000,
                TotalGuaranteeAmount = 1_106_000_000,
                AvgDutyCompletePercent = 100.0m,
                Status = CarDocReqStatus.HandedOver,
                ApprovedBy1 = "TrinhVanBinh_ChuyenVienCongNo",
                ApprovedDate1 = DateTime.Today.AddDays(-6),
                BankApprovedBy = "VoThiKimOanh_CBTD_VCB",
                BankApprovedAt = DateTime.Today.AddDays(-5),
                ApprovedBy2 = "PhamQuangHuy_PhoTGDKD",
                ApprovedDate2 = DateTime.Today.AddDays(-4),
                HandoverDate = DateTime.Today.AddDays(-3),
                HandedOverBy = "LeVanThang_ThuKhoChungTu",
                HandoverRecipient = "HoÃ ng Minh TrÃ­",
                ReturnDueDate = DateTime.Today.AddDays(7), // Háº¡n tráº£ há»“ sÆ¡ gá»‘c trong vÃ²ng 10 ngÃ y
                Remark = "Äáº¡i lÃ½ mÆ°á»£n báº£n gá»‘c CO xe Palisade Ä‘á»ƒ hoÃ n táº¥t thá»§ tá»¥c Ä‘Äƒng kÃ½ xe vÃ  Ä‘Äƒng kiá»ƒm biá»ƒn sá»‘ cho khÃ¡ch hÃ ng VIP cá»§a Vietcombank. Cam káº¿t hoÃ n tráº£ báº£n gá»‘c CO vá» kÃ©t trÆ°á»›c ngÃ y quy Ä‘á»‹nh.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today.AddDays(-7)
            };
            db.CarDocRequests.Add(req3);
            await db.SaveChangesAsync();

            db.CarDocRequestDetails.Add(
                new CarDocReqDetail
                {
                    DocReqId = req3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req3.DRListCode,
                    VIN = "KMHPL81CBPU066911",
                    CarId = "CAR-PAL-066911",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade Calligraphy 2.2D AWD",
                    SpecCode = "PAL-2.2D-CAL",
                    EngineNo = "D4HB-PU066911",
                    ColorNameVN = "Tráº¯ng Ngá»c Trai",
                    ContractNo = "HDMB-2025-SG04",
                    UnitPriceActual = 1_580_000_000,
                    PaymentPercent = 30.0m,
                    DepositPercent = 30.0m,
                    GuaranteePercent = 70.0m,
                    DutyCompletePercent = 100.0m,
                    BankGuaranteeNo = "BL-2025-VCB-9901",
                    CONo = "CO-2025-PAL-6911",
                    CQNo = "CQ-2025-PAL-6911",
                    HTCInvoiceNo = "HD-2025-001318",
                    DocumentsGiven = "Báº£n gá»‘c CO mÆ°á»£n táº¡m, Báº£n sao CQ cÃ´ng chá»©ng",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "VoThiKimOanh_CBTD_VCB",
                    BankApprDTime = DateTime.Today.AddDays(-5),
                    BankApprNote = "Vietcombank Ká»³ Äá»“ng Ä‘á»“ng Ã½ cho Ä‘áº¡i lÃ½ mÆ°á»£n CO gá»‘c Ä‘Äƒng kÃ½ báº¥m biá»ƒn xe cho KH",
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req3.HandoverDate,
                    ReturnDueDate = req3.ReturnDueDate,
                    Remark = "Äang mÆ°á»£n há»“ sÆ¡ gá»‘c lÃ m thá»§ tá»¥c Ä‘Äƒng kÃ½ xe, háº¡n tráº£ kÃ©t ngÃ y " + req3.ReturnDueDate?.ToString("dd/MM/yyyy")
                }
            );

            // Há»“ sÆ¡ 4: Äá» nghá»‹ má»›i táº¡o dá»± tháº£o (TypeCRR = Normal, Status = Draft)
            var req4 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-004",
                DealerCode = "DLR-LB",
                DealerName = "CÃ´ng ty CP Ã” tÃ´ Hyundai Long BiÃªn",
                BankCode = "VPB",
                BankName = "NgÃ¢n hÃ ng TMCP Viá»‡t Nam Thá»‹nh VÆ°á»£ng (VPBank) - Há»™i Sá»Ÿ",
                TypeCRR = CarDocReqType.Special,
                LetterRepresentationNo = "GGT-2025/05/LB-03",
                LetterRepresentationDate = DateTime.Today,
                RepresentativeName = "ÄoÃ n VÄƒn HÃ¹ng",
                RepresentativeIdCard = "001094002871",
                RepresentativePhone = "0944556677",
                TotalVehicles = 1,
                TotalCarAmount = 1_250_000_000,
                TotalPaymentAmount = 250_000_000,
                TotalGuaranteeAmount = 1_000_000_000,
                AvgDutyCompletePercent = 100.0m,
                Status = CarDocReqStatus.Draft,
                Remark = "Dá»± tháº£o há»“ sÆ¡ Ä‘á» nghá»‹ giao há»“ sÆ¡ gá»‘c xe Hyundai Custin theo ThÆ° báº£o lÃ£nh VPBank, Ä‘ang chá» hoÃ n thiá»‡n sao y cÃ´ng chá»©ng giáº¥y giá»›i thiá»‡u.",
                CreatedBy = "VuThiThao_ChuyenVienQuanLyDaiLy",
                CreatedAt = DateTime.Today
            };
            db.CarDocRequests.Add(req4);
            await db.SaveChangesAsync();

            db.CarDocRequestDetails.Add(
                new CarDocReqDetail
                {
                    DocReqId = req4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req4.DRListCode,
                    VIN = "KMHCU81NBPU055209",
                    CarId = "CAR-CUS-055209",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0T Cao Cáº¥p MPV",
                    SpecCode = "CUS-2.0T-PRE",
                    EngineNo = "G4NN-PU055209",
                    ColorNameVN = "Xanh Lá»¥c Báº£o",
                    ContractNo = "HDMB-2025-LB09",
                    UnitPriceActual = 1_250_000_000,
                    PaymentPercent = 20.0m,
                    DepositPercent = 20.0m,
                    GuaranteePercent = 80.0m,
                    DutyCompletePercent = 100.0m,
                    BankGuaranteeNo = "BL-2025-VPB-7712",
                    CONo = "CO-2025-CUS-5209",
                    CQNo = "CQ-2025-CUS-5209",
                    HTCInvoiceNo = "HD-2025-001322",
                    DocumentsGiven = "Báº£n gá»‘c CO, Báº£n sao CQ, HÃ³a Ä‘Æ¡n VAT",
                    BankApprStatus = BankDocApprStatus.Pending,
                    Status = CarDocReqDetailStatus.Pending,
                    Remark = "Dá»± tháº£o Ä‘á» nghá»‹, chuáº©n bá»‹ trÃ¬nh chuyÃªn viÃªn cÃ´ng ná»£ HTC tháº©m Ä‘á»‹nh A1"
                }
            );

            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u HÃ³a Ä‘Æ¡n GTGT (VAT E-Invoice) bÃ¡n buÃ´n xe Ã´ tÃ´ (VAT_HTCInvoice / PrintVAT)
        if (!await db.HTCInvoices.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            // HÃ³a Ä‘Æ¡n 1: ÄÃ£ phÃ¡t hÃ nh, ÄÃ£ thanh toÃ¡n 1 pháº§n (PartiallyPaid)
            var inv1 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-001",
                HTCInvoiceNo = "0012891",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-6),
                DealerCode = "DL-HYUNDAI-HADONG",
                DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai HÃ  ÄÃ´ng",
                BuyerTaxCode = "0105882910",
                BuyerAddress = "Tá»• 17 PhÆ°á»ng YÃªn NghÄ©a, Quáº­n HÃ  ÄÃ´ng, TP. HÃ  Ná»™i",
                BuyerLegalRepresentative = "Nguyá»…n VÄƒn Tuáº¥n - Tá»•ng GiÃ¡m Äá»‘c",
                PaymentMethod = "CK",
                BankCode = "CTG",
                BankName = "VietinBank - CN ÄÃ´ ThÃ nh",
                BankAccountNo = "118000293849",
                TotalVehicles = 2,
                TotalAmount = 2_228_000_000,
                VATRate = 10.0m,
                VATAmount = 222_800_000,
                TotalPayment = 2_450_800_000,
                PaidAmount = 1_500_000_000,
                RemainAmount = 950_800_000,
                OS_HDDT_InvoiceCode = "EINV-202505-HTC-9B2144F1",
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.PartiallyPaid,
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-6).AddHours(2),
                IssuedBy = "GiamDocTaiChinh_HTC",
                IssuedDate = DateTime.Today.AddDays(-6).AddHours(4),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-6),
                Remark = "TT 1,500,000,000 Ä‘ qua UNC MBB-8921 ngÃ y 10/05/2025"
            };
            db.HTCInvoices.Add(inv1);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.AddRange(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv1.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-SAN-881023",
                    VIN = "KMHEN41LBCU881023",
                    ModelCode = "SANTAFE",
                    ModelName = "Hyundai Santa Fe 2.5 H-Trac XÄƒng Cao Cáº¥p",
                    SpecCode = "SAN-2.5-GAS-PRE",
                    EngineNo = "G4KM-CU881023",
                    ColorVN = "Äen Sang Trá»ng",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-SAN-1023",
                    CQNo = "CQ-2025-SAN-1023",
                    SOCode = "SO-202505-HD01",
                    UnitPrice = 1_269_000_000,
                    VATRate = 10.0m,
                    VATAmount = 126_900_000,
                    TotalPrice = 1_395_900_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "Xe giao Ä‘á»£t 1 thÃ¡ng 5/2025 theo HÄ Ä‘áº¡i lÃ½"
                },
                new HTCInvoiceDetail
                {
                    InvoiceId = inv1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv1.HTCInvoiceCode,
                    ItemNo = 2,
                    CarId = "CAR-TUC-881024",
                    VIN = "KMHEN41LBCU881024",
                    ModelCode = "TUCSON",
                    ModelName = "Hyundai Tucson 2.0 Dáº§u Äáº·c Biá»‡t",
                    SpecCode = "TUC-2.0D-SPE",
                    EngineNo = "D4HD-CU881024",
                    ColorVN = "Tráº¯ng Tinh KhÃ´i",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-TUC-1024",
                    CQNo = "CQ-2025-TUC-1024",
                    SOCode = "SO-202505-HD02",
                    UnitPrice = 959_000_000,
                    VATRate = 10.0m,
                    VATAmount = 95_900_000,
                    TotalPrice = 1_054_900_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "Xe giao Ä‘á»£t 1 thÃ¡ng 5/2025 theo HÄ Ä‘áº¡i lÃ½"
                }
            );
            await db.SaveChangesAsync();

            // HÃ³a Ä‘Æ¡n 2: ÄÃ£ phÃ¡t hÃ nh, ÄÃ£ quyáº¿t toÃ¡n 100% (Settled)
            var inv2 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-002",
                HTCInvoiceNo = "0012892",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-5),
                DealerCode = "DL-HYUNDAI-SAIGON",
                DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai SÃ i GÃ²n",
                BuyerTaxCode = "0304991823",
                BuyerAddress = "Sá»‘ 70 LÆ°Æ¡ng Äá»‹nh Cá»§a, PhÆ°á»ng An PhÃº, TP. Thá»§ Äá»©c, TP. Há»“ ChÃ­ Minh",
                BuyerLegalRepresentative = "LÃª HoÃ ng Nam - GiÃ¡m Äá»‘c",
                PaymentMethod = "CK",
                BankCode = "VCB",
                BankName = "Vietcombank - CN TP.HCM",
                BankAccountNo = "0071000881923",
                TotalVehicles = 3,
                TotalAmount = 1_878_000_000,
                VATRate = 10.0m,
                VATAmount = 187_800_000,
                TotalPayment = 2_065_800_000,
                PaidAmount = 2_065_800_000,
                RemainAmount = 0,
                OS_HDDT_InvoiceCode = "EINV-202505-HTC-4C8821EE",
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.Settled,
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-5).AddHours(1),
                IssuedBy = "GiamDocTaiChinh_HTC",
                IssuedDate = DateTime.Today.AddDays(-5).AddHours(3),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-5),
                Remark = "TT 2,065,800,000 Ä‘ qua UNC VCB-3498 ngÃ y 08/05/2025"
            };
            db.HTCInvoices.Add(inv2);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.AddRange(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv2.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-CRE-102941",
                    VIN = "KMHEC41ABNU102941",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cáº¥p",
                    SpecCode = "CRE-1.5-PRE",
                    EngineNo = "G4FL-NU102941",
                    ColorVN = "Äá» Máº­n",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-CRE-2941",
                    CQNo = "CQ-2025-CRE-2941",
                    SOCode = "SO-202505-SG01",
                    UnitPrice = 699_000_000,
                    VATRate = 10.0m,
                    VATAmount = 69_900_000,
                    TotalPrice = 768_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                },
                new HTCInvoiceDetail
                {
                    InvoiceId = inv2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv2.HTCInvoiceCode,
                    ItemNo = 2,
                    CarId = "CAR-CRE-102942",
                    VIN = "KMHEC41ABNU102942",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Äáº·c Biá»‡t",
                    SpecCode = "CRE-1.5-SPE",
                    EngineNo = "G4FL-NU102942",
                    ColorVN = "Tráº¯ng Tuyáº¿t",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-CRE-2942",
                    CQNo = "CQ-2025-CRE-2942",
                    SOCode = "SO-202505-SG02",
                    UnitPrice = 650_000_000,
                    VATRate = 10.0m,
                    VATAmount = 65_000_000,
                    TotalPrice = 715_000_000,
                    Status = HTCInvoiceDetailStatus.Active
                },
                new HTCInvoiceDetail
                {
                    InvoiceId = inv2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv2.HTCInvoiceCode,
                    ItemNo = 3,
                    CarId = "CAR-ACC-332190",
                    VIN = "KMHBA41BBNU332190",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT Äáº·c Biá»‡t All New",
                    SpecCode = "ACC-1.5-AT-SPE",
                    EngineNo = "G4LC-NU332190",
                    ColorVN = "Báº¡c Ãnh Kim",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-ACC-2190",
                    CQNo = "CQ-2025-ACC-2190",
                    SOCode = "SO-202505-SG03",
                    UnitPrice = 529_000_000,
                    VATRate = 10.0m,
                    VATAmount = 52_900_000,
                    TotalPrice = 581_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                }
            );
            await db.SaveChangesAsync();

            // HÃ³a Ä‘Æ¡n 3: ÄÃ£ phÃ¡t hÃ nh, ChÆ°a thanh toÃ¡n (Issued)
            var inv3 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-003",
                HTCInvoiceNo = "0012893",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-3),
                DealerCode = "DL-HYUNDAI-DONGDO",
                DealerName = "CÃ´ng ty Cá»• pháº§n Hyundai ÄÃ´ng ÄÃ´",
                BuyerTaxCode = "0106771890",
                BuyerAddress = "Sá»‘ 98 Phá»‘ PhÃ¹ng HÆ°ng, Quáº­n HoÃ n Kiáº¿m, TP. HÃ  Ná»™i",
                BuyerLegalRepresentative = "Tráº§n Máº¡nh CÆ°á»ng - Chá»§ Tá»‹ch HÄQT",
                PaymentMethod = "CK",
                BankCode = "MBB",
                BankName = "MBBank - CN Sá»Ÿ Giao Dá»‹ch",
                BankAccountNo = "0581100982734",
                TotalVehicles = 2,
                TotalAmount = 3_058_000_000,
                VATRate = 10.0m,
                VATAmount = 305_800_000,
                TotalPayment = 3_363_800_000,
                PaidAmount = 0,
                RemainAmount = 3_363_800_000,
                OS_HDDT_InvoiceCode = "EINV-202505-HTC-8A7129CC",
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.Issued,
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-3).AddHours(2),
                IssuedBy = "GiamDocTaiChinh_HTC",
                IssuedDate = DateTime.Today.AddDays(-3).AddHours(4),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-3),
                Remark = "ÄÃ£ gá»­i hÃ³a Ä‘Æ¡n Ä‘iá»‡n tá»­ cho Ä‘áº¡i lÃ½, chá» káº¿ toÃ¡n Ä‘áº¡i lÃ½ á»§y nhiá»‡m chi"
            };
            db.HTCInvoices.Add(inv3);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.AddRange(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv3.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-PAL-091822",
                    VIN = "KMHPL41ABRU091822",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade 2.2D Prestige 6 chá»—",
                    SpecCode = "PAL-2.2D-PRE-6S",
                    EngineNo = "D4HB-RU091822",
                    ColorVN = "Xanh BÃ³ng ÄÃªm",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-PAL-1822",
                    CQNo = "CQ-2025-PAL-1822",
                    SOCode = "SO-202505-DD01",
                    UnitPrice = 1_589_000_000,
                    VATRate = 10.0m,
                    VATAmount = 158_900_000,
                    TotalPrice = 1_747_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                },
                new HTCInvoiceDetail
                {
                    InvoiceId = inv3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv3.HTCInvoiceCode,
                    ItemNo = 2,
                    CarId = "CAR-PAL-091823",
                    VIN = "KMHPL41ABRU091823",
                    ModelCode = "PALISADE",
                    ModelName = "Hyundai Palisade 2.2D Exclusive 7 chá»—",
                    SpecCode = "PAL-2.2D-EXC-7S",
                    EngineNo = "D4HB-RU091823",
                    ColorVN = "Tráº¯ng Ngá»c Trai",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-PAL-1823",
                    CQNo = "CQ-2025-PAL-1823",
                    SOCode = "SO-202505-DD02",
                    UnitPrice = 1_469_000_000,
                    VATRate = 10.0m,
                    VATAmount = 146_900_000,
                    TotalPrice = 1_615_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                }
            );
            await db.SaveChangesAsync();

            // HÃ³a Ä‘Æ¡n 4: ÄÃ£ duyá»‡t, Chá» phÃ¡t hÃ nh (Approved)
            var inv4 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-004",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-1),
                DealerCode = "DL-HYUNDAI-DANANG",
                DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai ÄÃ  Náºµng",
                BuyerTaxCode = "0401882716",
                BuyerAddress = "Sá»‘ 86 Duy TÃ¢n, PhÆ°á»ng HÃ²a Thuáº­n TÃ¢y, Quáº­n Háº£i ChÃ¢u, TP. ÄÃ  Náºµng",
                BuyerLegalRepresentative = "VÃµ VÄƒn HÃ¹ng - GiÃ¡m Äá»‘c",
                PaymentMethod = "CK",
                BankCode = "VPB",
                BankName = "VPBank - CN ÄÃ  Náºµng",
                BankAccountNo = "192837465012",
                TotalVehicles = 2,
                TotalAmount = 1_828_000_000,
                VATRate = 10.0m,
                VATAmount = 182_800_000,
                TotalPayment = 2_010_800_000,
                PaidAmount = 0,
                RemainAmount = 2_010_800_000,
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.Approved,
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-1).AddHours(3),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-1),
                Remark = "Káº¿ toÃ¡n trÆ°á»Ÿng Ä‘Ã£ duyá»‡t, sáºµn sÃ ng kÃ½ sá»‘ CA phÃ¡t hÃ nh"
            };
            db.HTCInvoices.Add(inv4);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.AddRange(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv4.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-CUS-055209",
                    VIN = "KMHCU81NBPU055209",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0T Cao Cáº¥p MPV",
                    SpecCode = "CUS-2.0T-PRE",
                    EngineNo = "G4NN-PU055209",
                    ColorVN = "Xanh Lá»¥c Báº£o",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-CUS-5209",
                    CQNo = "CQ-2025-CUS-5209",
                    SOCode = "SO-202505-DN01",
                    UnitPrice = 1_029_000_000,
                    VATRate = 10.0m,
                    VATAmount = 102_900_000,
                    TotalPrice = 1_131_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                },
                new HTCInvoiceDetail
                {
                    InvoiceId = inv4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv4.HTCInvoiceCode,
                    ItemNo = 2,
                    CarId = "CAR-ELA-033100",
                    VIN = "KMHEL41ABPU033100",
                    ModelCode = "ELANTRA",
                    ModelName = "Hyundai Elantra N-Line 1.6 Turbo",
                    SpecCode = "ELA-1.6T-NLINE",
                    EngineNo = "G4FP-PU033100",
                    ColorVN = "XÃ¡m Xi MÄƒng",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-ELA-3100",
                    CQNo = "CQ-2025-ELA-3100",
                    SOCode = "SO-202505-DN02",
                    UnitPrice = 799_000_000,
                    VATRate = 10.0m,
                    VATAmount = 79_900_000,
                    TotalPrice = 878_900_000,
                    Status = HTCInvoiceDetailStatus.Active
                }
            );
            await db.SaveChangesAsync();

            // HÃ³a Ä‘Æ¡n 5: Dá»± tháº£o (Draft)
            var inv5 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-005",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today,
                DealerCode = "DL-HYUNDAI-CANTHO",
                DealerName = "CÃ´ng ty TNHH MTV Hyundai Cáº§n ThÆ¡",
                BuyerTaxCode = "1801293810",
                BuyerAddress = "Quá»‘c Lá»™ 91B, PhÆ°á»ng An KhÃ¡nh, Quáº­n Ninh Kiá»u, TP. Cáº§n ThÆ¡",
                BuyerLegalRepresentative = "Pháº¡m Thá»‹ Mai - GiÃ¡m Äá»‘c",
                PaymentMethod = "CK",
                BankCode = "TCB",
                BankName = "Techcombank - CN Cáº§n ThÆ¡",
                BankAccountNo = "19034455667788",
                TotalVehicles = 1,
                TotalAmount = 1_450_000_000,
                VATRate = 10.0m,
                VATAmount = 145_000_000,
                TotalPayment = 1_595_000_000,
                PaidAmount = 0,
                RemainAmount = 1_595_000_000,
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.Draft,
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today,
                Remark = "Dá»± tháº£o hÃ³a Ä‘Æ¡n xe Ä‘iá»‡n Ioniq 5 phÃ¢n bá»• vá» Ä‘áº¡i lÃ½ Cáº§n ThÆ¡"
            };
            db.HTCInvoices.Add(inv5);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.Add(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv5.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-IQ5-012899",
                    VIN = "KMHIQ51EBPU012899",
                    ModelCode = "IONIQ5",
                    ModelName = "Hyundai Ioniq 5 Prestige Äiá»‡n AWD",
                    SpecCode = "IQ5-PRE-AWD",
                    EngineNo = "EM17-PU012899",
                    ColorVN = "VÃ ng CÃ¡t Gravity Gold",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-IQ5-2899",
                    CQNo = "CQ-2025-IQ5-2899",
                    SOCode = "SO-202505-CT01",
                    UnitPrice = 1_450_000_000,
                    VATRate = 10.0m,
                    VATAmount = 145_000_000,
                    TotalPrice = 1_595_000_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "DÃ²ng xe thuáº§n Ä‘iá»‡n EV cao cáº¥p phÃ¢n phá»‘i Ä‘áº¡i lÃ½"
                }
            );
            await db.SaveChangesAsync();

            // HÃ³a Ä‘Æ¡n 6: Thu há»“i / Há»§y (Cancelled)
            var inv6 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-006",
                HTCInvoiceNo = "0012890",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-10),
                DealerCode = "DL-HYUNDAI-LONGBIEN",
                DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai Long BiÃªn",
                BuyerTaxCode = "0107883921",
                BuyerAddress = "Sá»‘ 3-5 Nguyá»…n VÄƒn Linh, PhÆ°á»ng Gia Thá»¥y, Quáº­n Long BiÃªn, TP. HÃ  Ná»™i",
                PaymentMethod = "CK",
                BankCode = "CTG",
                BankName = "VietinBank - CN ÄÃ´ng HÃ  Ná»™i",
                BankAccountNo = "119000188293",
                TotalVehicles = 1,
                TotalAmount = 599_000_000,
                VATRate = 10.0m,
                VATAmount = 59_900_000,
                TotalPayment = 658_900_000,
                PaidAmount = 0,
                RemainAmount = 658_900_000,
                OS_HDDT_InvoiceCode = "EINV-202505-HTC-1109AACC",
                SourceInvoiceCode = HTCInvoiceSource.Root,
                Status = HTCInvoiceStatus.Cancelled,
                Adj_DeleteReason = "BB sá»‘ 14/BBTH-2025: KhÃ¡ch hÃ ng Ä‘á»•i sang há»£p Ä‘á»“ng xe Santa Fe má»›i, thu há»“i hÃ³a Ä‘Æ¡n nháº§m phiÃªn báº£n.",
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-10).AddHours(2),
                IssuedBy = "GiamDocTaiChinh_HTC",
                IssuedDate = DateTime.Today.AddDays(-10).AddHours(4),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-10),
                Remark = "ÄÃ£ láº­p biÃªn báº£n thu há»“i hÃ³a Ä‘Æ¡n theo quy Ä‘á»‹nh cá»§a Cá»¥c Thuáº¿"
            };
            db.HTCInvoices.Add(inv6);
            await db.SaveChangesAsync();

            db.HTCInvoiceDetails.Add(
                new HTCInvoiceDetail
                {
                    InvoiceId = inv6.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    HTCInvoiceCode = inv6.HTCInvoiceCode,
                    ItemNo = 1,
                    CarId = "CAR-STG-011999",
                    VIN = "KMHST41ABPU011999",
                    ModelCode = "STARGAZER",
                    ModelName = "Hyundai Stargazer X Cao Cáº¥p 1.5 AT",
                    SpecCode = "STG-X-PRE",
                    EngineNo = "G4FL-PU011999",
                    ColorVN = "Tráº¯ng Má»",
                    ProductionYear = "2025",
                    SOCode = "SO-202505-LB01",
                    UnitPrice = 599_000_000,
                    VATRate = 10.0m,
                    VATAmount = 59_900_000,
                    TotalPrice = 658_900_000,
                    Status = HTCInvoiceDetailStatus.Cancelled,
                    Remark = "Xe thu há»“i do Ä‘á»•i sang há»£p Ä‘á»“ng xe Santa Fe"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Danh má»¥c NgÃ¢n hÃ ng - Äáº¡i lÃ½ (Mst_BankDealer / FrmDealerBank)
        if (!await db.BankDealers.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.BankDealers.AddRange(
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai Long BiÃªn",
                    BankCode = "VCB",
                    BankName = "Vietcombank - CN Sá»Ÿ Giao Dá»‹ch",
                    CreditContractNo = "HÄTD-VCB-2025-001",
                    CreditContractDate = DateTime.Today.AddMonths(-6),
                    CreditAmount = 50_000_000_000,
                    BankBranchCode = "VCB-SGD",
                    BankBranchName = "Vietcombank - Chi nhÃ¡nh Sá»Ÿ Giao Dá»‹ch",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Háº¡n má»©c tÃ­n dá»¥ng báº£o lÃ£nh + thanh toÃ¡n xe nháº­p kháº©u",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-6)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai Long BiÃªn",
                    BankCode = "TCB",
                    BankName = "Techcombank - CN Long BiÃªn",
                    CreditContractNo = "HÄTD-TCB-2025-014",
                    CreditContractDate = DateTime.Today.AddMonths(-4),
                    CreditAmount = 30_000_000_000,
                    BankBranchCode = "TCB-LB",
                    BankBranchName = "Techcombank - Chi nhÃ¡nh Long BiÃªn",
                    FlagBankGrt = true,
                    FlagBankPmt = false,
                    Status = BankDealerStatus.Active,
                    Remark = "Chá»‰ dÃ¹ng cho báº£o lÃ£nh má»Ÿ L/C nháº­p kháº©u",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-4)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-DANANG",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai ÄÃ  Náºµng",
                    BankCode = "VPB",
                    BankName = "VPBank - CN ÄÃ  Náºµng",
                    CreditContractNo = "HÄTD-VPB-2025-007",
                    CreditContractDate = DateTime.Today.AddMonths(-3),
                    CreditAmount = 20_000_000_000,
                    BankBranchCode = "VPB-DN",
                    BankBranchName = "VPBank - Chi nhÃ¡nh ÄÃ  Náºµng",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Háº¡n má»©c tÃ­n dá»¥ng báº£o lÃ£nh + thanh toÃ¡n",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-3)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-CANTHO",
                    DealerName = "CÃ´ng ty TNHH MTV Hyundai Cáº§n ThÆ¡",
                    BankCode = "CTG",
                    BankName = "VietinBank - CN Cáº§n ThÆ¡",
                    CreditContractNo = "HÄTD-CTG-2025-021",
                    CreditContractDate = DateTime.Today.AddMonths(-2),
                    CreditAmount = 15_000_000_000,
                    BankBranchCode = "CTG-CT",
                    BankBranchName = "VietinBank - Chi nhÃ¡nh Cáº§n ThÆ¡",
                    FlagBankGrt = false,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Chá»‰ dÃ¹ng cho thanh toÃ¡n UNC, khÃ´ng dÃ¹ng báº£o lÃ£nh",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-2)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-HAIPHONG",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai Háº£i PhÃ²ng",
                    BankCode = "MBB",
                    BankName = "MBBank - CN Háº£i PhÃ²ng",
                    CreditContractNo = "HÄTD-MBB-2024-088",
                    CreditContractDate = DateTime.Today.AddMonths(-10),
                    CreditAmount = 10_000_000_000,
                    BankBranchCode = "MBB-HP",
                    BankBranchName = "MBBank - Chi nhÃ¡nh Háº£i PhÃ²ng",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Inactive,
                    Remark = "Há»£p Ä‘á»“ng tÃ­n dá»¥ng Ä‘Ã£ táº¥t toÃ¡n, táº¡m ngÆ°ng sá»­ dá»¥ng",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-10),
                    UpdatedBy = "Admin_HTC",
                    UpdatedAt = DateTime.Today.AddMonths(-1)
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Cáº­p nháº­t Chá»©ng tá»« Káº¿ toÃ¡n (FrmUpdateChungTuKT / Pmt_Payment_UpdateFinancial)
        if (!await db.AccountingVoucherUpdates.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.AccountingVoucherUpdates.AddRange(
                new AccountingVoucherUpdate
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchNo = "CTKT-202505-001",
                    Description = "Cáº­p nháº­t sá»‘ chá»©ng tá»« káº¿ toÃ¡n Ä‘á»£t 1 thÃ¡ng 05/2025",
                    TotalItems = 2,
                    UpdatedItems = 1,
                    SkippedItems = 1,
                    Status = AccountingVoucherUpdateStatus.Applied,
                    CreatedBy = "KeToanTongHop",
                    CreatedAt = DateTime.Today.AddDays(-6),
                    AppliedBy = "KeToanTruong",
                    AppliedAt = DateTime.Today.AddDays(-5),
                    Remark = "Ghi sá»• chá»©ng tá»« cho cÃ¡c phiáº¿u thanh toÃ¡n Ä‘Ã£ hoÃ n táº¥t",
                    Details =
                    {
                        new AccountingVoucherUpdateDetail
                        {
                            OrgId = TenantContext.DefaultOrgId,
                            PaymentNo = "PM-DEMO-20250501-001",
                            OldAccountingRecordNo = "PKT-202505-0145",
                            NewAccountingRecordNo = "PKT-202505-0301",
                            Status = AccountingVoucherUpdateDetailStatus.Updated,
                            Note = "ÄÃ£ cáº­p nháº­t sá»‘ chá»©ng tá»« káº¿ toÃ¡n."
                        },
                        new AccountingVoucherUpdateDetail
                        {
                            OrgId = TenantContext.DefaultOrgId,
                            PaymentNo = "PM-DEMO-20250510-002",
                            NewAccountingRecordNo = "PKT-202505-0302",
                            Status = AccountingVoucherUpdateDetailStatus.Skipped,
                            Note = "Phiáº¿u chÆ°a hoÃ n táº¥t ghi sá»• (tráº¡ng thÃ¡i hiá»‡n táº¡i: Approved)."
                        }
                    }
                },
                new AccountingVoucherUpdate
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchNo = "CTKT-202505-002",
                    Description = "Cáº­p nháº­t sá»‘ chá»©ng tá»« káº¿ toÃ¡n Ä‘á»£t 2 thÃ¡ng 05/2025 (chá» Ã¡p dá»¥ng)",
                    TotalItems = 1,
                    UpdatedItems = 0,
                    SkippedItems = 0,
                    Status = AccountingVoucherUpdateStatus.Draft,
                    CreatedBy = "KeToanTongHop",
                    CreatedAt = DateTime.Today.AddDays(-1),
                    Remark = "Chá» káº¿ toÃ¡n trÆ°á»Ÿng Ã¡p dá»¥ng ghi sá»•",
                    Details =
                    {
                        new AccountingVoucherUpdateDetail
                        {
                            OrgId = TenantContext.DefaultOrgId,
                            PaymentNo = "PM-DEMO-20250501-001",
                            NewAccountingRecordNo = "PKT-202505-0310",
                            Status = AccountingVoucherUpdateDetailStatus.Pending
                        }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Lá»‹ch LÃ m Viá»‡c & Háº¡n Ná»™p Cá»c (Mst_Calendar / Mst_Calendar_ResetYear)
        // Sinh lá»‹ch lÃ m viá»‡c cho nÄƒm hiá»‡n táº¡i: Thá»© 2..Thá»© 6 lÃ m viá»‡c, Thá»© 7 & Chá»§ nháº­t nghá»‰.
        if (!await db.CalendarEntries.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            int seedYear = DateTime.Now.Year;
            var firstDay = new DateTime(seedYear, 1, 1);
            var nextYearFirstDay = new DateTime(seedYear + 1, 1, 1);
            var entries = new List<CalendarEntry>();
            for (var scan = firstDay; scan < nextYearFirstDay; scan = scan.AddDays(1))
            {
                bool isWeekend = scan.DayOfWeek == DayOfWeek.Saturday || scan.DayOfWeek == DayOfWeek.Sunday;
                entries.Add(new CalendarEntry
                {
                    OrgId = TenantContext.DefaultOrgId,
                    CalendarType = PaymentCalendarService.WorkingDayType,
                    Date = scan,
                    StatusValue = isWeekend ? CalendarDayStatus.Holiday : CalendarDayStatus.WorkingDay,
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Now
                });
            }
            db.CalendarEntries.AddRange(entries);
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Há»£p Ä‘á»“ng Mua bÃ¡n Xe Äáº¡i lÃ½ (DMS40_CT_DealerContract / FrmMngDC)
        if (!await db.DealerContracts.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.DealerContracts.AddRange(
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-001",
                    DCPType = DlrCtrPaymentType.BankGuarantee,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai Long BiÃªn",
                    BankCodeMD = "VCB",
                    BankNameMD = "Vietcombank - CN Sá»Ÿ Giao Dá»‹ch",
                    ContractDate = DateTime.Today.AddDays(-20),
                    TotalVehicles = 3,
                    TotalAmount = 2_400_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Approved,
                    HTCSignStatus = HTCSignStatus.Approved2,
                    DlrCtrStatus = DlrCtrStatus.Signed,
                    Remark = "Há»£p Ä‘á»“ng bÃ¡n buÃ´n xe Santa Fe & Tucson, báº£o lÃ£nh VCB",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-20),
                    DlrApprovedBy = "GiamDoc_DaiLy_LB",
                    DlrApprovedAt = DateTime.Today.AddDays(-18),
                    HTCApproved1By = "KeToanTruong_HTC",
                    HTCApproved1At = DateTime.Today.AddDays(-16),
                    HTCApproved2By = "BanGiamDoc_HTC",
                    HTCApproved2At = DateTime.Today.AddDays(-15),
                    Details =
                    {
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-SF-001", VIN = "KMHST81D8PU123001", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", SpecCode = "SF-2025-PRE", ColorCode = "WHT", ColorName = "Tráº¯ng", OriginNo = "CO-2025-001", ProductionYear = 2025, UnitPrice = 1_200_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-TU-002", VIN = "KMHJ3813DPU123002", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", SpecCode = "TU-2025-STD", ColorCode = "BLK", ColorName = "Äen", OriginNo = "CO-2025-002", ProductionYear = 2025, UnitPrice = 800_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-TU-003", VIN = "KMHJ3813DPU123003", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", SpecCode = "TU-2025-STD", ColorCode = "SLV", ColorName = "Báº¡c", OriginNo = "CO-2025-003", ProductionYear = 2025, UnitPrice = 400_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed }
                    }
                },
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-002",
                    DCPType = DlrCtrPaymentType.LetterOfCredit,
                    DealerCode = "DL-HYUNDAI-DANANG",
                    DealerName = "CÃ´ng ty Cá»• pháº§n Ã” tÃ´ Hyundai ÄÃ  Náºµng",
                    BankCodeMD = "TCB",
                    BankNameMD = "Techcombank - CN ÄÃ  Náºµng",
                    ContractDate = DateTime.Today.AddDays(-10),
                    TotalVehicles = 2,
                    TotalAmount = 1_500_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Approved,
                    HTCSignStatus = HTCSignStatus.Approved1,
                    DlrCtrStatus = DlrCtrStatus.NotSign,
                    Remark = "Há»£p Ä‘á»“ng xe nháº­p kháº©u má»Ÿ L/C qua Techcombank, chá» HTC duyá»‡t cáº¥p 2",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-10),
                    DlrApprovedBy = "GiamDoc_DaiLy_DN",
                    DlrApprovedAt = DateTime.Today.AddDays(-8),
                    HTCApproved1By = "KeToanTruong_HTC",
                    HTCApproved1At = DateTime.Today.AddDays(-6),
                    Details =
                    {
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-002", CarId = "CAR-PA-004", VIN = "KMHST81D8PU123004", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", SpecCode = "PA-2025-PRE", ColorCode = "GRY", ColorName = "XÃ¡m", OriginNo = "CO-2025-004", ProductionYear = 2025, UnitPrice = 1_000_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-002", CarId = "CAR-PA-005", VIN = "KMHST81D8PU123005", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", SpecCode = "PA-2025-STD", ColorCode = "WHT", ColorName = "Tráº¯ng", OriginNo = "CO-2025-005", ProductionYear = 2025, UnitPrice = 500_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed }
                    }
                },
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-003",
                    DCPType = DlrCtrPaymentType.OwnCapital,
                    DealerCode = "DL-HYUNDAI-CANTHO",
                    DealerName = "CÃ´ng ty TNHH MTV Hyundai Cáº§n ThÆ¡",
                    BankCodeMD = null,
                    BankNameMD = null,
                    ContractDate = DateTime.Today.AddDays(-3),
                    TotalVehicles = 2,
                    TotalAmount = 900_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Pending,
                    HTCSignStatus = HTCSignStatus.Pending,
                    DlrCtrStatus = DlrCtrStatus.NotSign,
                    Remark = "Há»£p Ä‘á»“ng thanh toÃ¡n vá»‘n tá»± cÃ³, chá» Ä‘áº¡i lÃ½ kÃ½",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-3),
                    Details =
                    {
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-003", CarId = "CAR-AC-006", VIN = "KMHCT41D8PU123006", ModelCode = "ACCENT", ModelName = "Hyundai Accent", SpecCode = "AC-2025-STD", ColorCode = "RED", ColorName = "Äá»", OriginNo = "CO-2025-006", ProductionYear = 2025, UnitPrice = 500_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.NotSign },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-003", CarId = "CAR-CR-007", VIN = "KMHCT41D8PU123007", ModelCode = "CRETA", ModelName = "Hyundai Creta", SpecCode = "CR-2025-STD", ColorCode = "BLU", ColorName = "Xanh", OriginNo = "CO-2025-007", ProductionYear = 2025, UnitPrice = 400_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.NotSign }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u YÃªu cáº§u Äiá»u chuyá»ƒn Váº­n táº£i Kho (Sto_StorageRearrange / FrmMngSC)
        if (!await db.StorageRearranges.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.StorageRearranges.AddRange(
                new StorageRearrange
                {
                    OrgId = TenantContext.DefaultOrgId,
                    StorageRearrangeNo = "SC-202505-001",
                    RearrangeStatus = StorageRearrangeStatus.Approved2,
                    Remark = "Äiá»u chuyá»ƒn xe Santa Fe & Tucson tá»« kho Ninh BÃ¬nh vá» kho ÄÃ´ng Anh",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-12),
                    ApprovedBy1 = "TruongKho_HTC",
                    ApprovedAt1 = DateTime.Today.AddDays(-11),
                    ApprovedBy2 = "BanGiamDoc_HTC",
                    ApprovedAt2 = DateTime.Today.AddDays(-10),
                    Details =
                    {
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-001", VIN = "KMHST81D8PU123001", StorageCodeFrom = "KHO-NBD", StorageCodeTo = "KHO-DA", ExpectedStartDate = DateTime.Today.AddDays(-9), ExpectedEndDate = DateTime.Today.AddDays(-7), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved2, Remark = "Xe Æ°u tiÃªn giao Ä‘áº¡i lÃ½ Long BiÃªn" },
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-001", VIN = "KMHJ3813DPU123002", StorageCodeFrom = "KHO-NBD", StorageCodeTo = "KHO-DA", ExpectedStartDate = DateTime.Today.AddDays(-9), ExpectedEndDate = DateTime.Today.AddDays(-7), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved2 }
                    }
                },
                new StorageRearrange
                {
                    OrgId = TenantContext.DefaultOrgId,
                    StorageRearrangeNo = "SC-202505-002",
                    RearrangeStatus = StorageRearrangeStatus.Approved1,
                    Remark = "Äiá»u chuyá»ƒn xe Palisade tá»« kho HÆ°ng YÃªn vá» kho Hiá»‡p PhÆ°á»›c SG, chá» duyá»‡t cáº¥p 2",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-5),
                    ApprovedBy1 = "TruongKho_HTC",
                    ApprovedAt1 = DateTime.Today.AddDays(-4),
                    Details =
                    {
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-002", VIN = "KMHST81D8PU123004", StorageCodeFrom = "KHO-HY", StorageCodeTo = "KHO-SG", ExpectedStartDate = DateTime.Today.AddDays(-2), ExpectedEndDate = DateTime.Today.AddDays(2), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved1 },
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-002", VIN = "KMHST81D8PU123005", StorageCodeFrom = "KHO-HY", StorageCodeTo = "KHO-SG", ExpectedStartDate = DateTime.Today.AddDays(-2), ExpectedEndDate = DateTime.Today.AddDays(2), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved1 }
                    }
                },
                new StorageRearrange
                {
                    OrgId = TenantContext.DefaultOrgId,
                    StorageRearrangeNo = "SC-202505-003",
                    RearrangeStatus = StorageRearrangeStatus.Pending,
                    Remark = "Äiá»u chuyá»ƒn xe Accent & Creta tá»« kho ÄÃ´ng Anh vá» kho Ninh BÃ¬nh, chá» duyá»‡t cáº¥p 1",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-1),
                    Details =
                    {
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-003", VIN = "KMHCT41D8PU123006", StorageCodeFrom = "KHO-DA", StorageCodeTo = "KHO-NBD", ExpectedStartDate = DateTime.Today.AddDays(1), ExpectedEndDate = DateTime.Today.AddDays(3), RearrangeDtlStatus = StorageRearrangeDetailStatus.Pending },
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-003", VIN = "KMHCT41D8PU123007", StorageCodeFrom = "KHO-DA", StorageCodeTo = "KHO-NBD", ExpectedStartDate = DateTime.Today.AddDays(1), ExpectedEndDate = DateTime.Today.AddDays(3), RearrangeDtlStatus = StorageRearrangeDetailStatus.Pending }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u PhiÃªn báº£n CÆ°á»›c phÃ­ Váº­n táº£i (Mst_TranspFeeVer / Mst_TranspFee)
        if (!await db.TransportFeeVersions.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.TransportFeeVersions.AddRange(
                new TransportFeeVersion
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TFVCode = "TFV-2025-01",
                    Description = "Báº£ng cÆ°á»›c phÃ­ váº­n táº£i Ã¡p dá»¥ng tá»« 01/2025",
                    Status = TransportFeeVersionStatus.Active,
                    CreatedDate = DateTime.Today.AddDays(-60),
                    AppliedDate = DateTime.Today.AddDays(-55),
                    Remark = "CÆ°á»›c váº­n táº£i tuyáº¿n Báº¯c - Nam, nhÃ  váº­n táº£i NewWay & Äáº¡t Äá»©c",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-60),
                    Rates =
                    {
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Há»“ ChÃ­ Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiá»‡p PhÆ°á»›c", TransporterCode = "NEWWAY", TransporterName = "CÃ´ng ty Váº­n táº£i NewWay", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 18_000_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Há»“ ChÃ­ Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiá»‡p PhÆ°á»›c", TransporterCode = "NEWWAY", TransporterName = "CÃ´ng ty Váº­n táº£i NewWay", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", ValFee = 16_000_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "DN", ProvinceNameTo = "ÄÃ  Náºµng", DistrictCodeTo = "DN-HC", DistrictNameTo = "HÃ²a CÆ°á»ng", TransporterCode = "DATDUC", TransporterName = "CÃ´ng ty Váº­n táº£i Äáº¡t Äá»©c", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 12_000_000, ExpectedDays = 3 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "DN", ProvinceNameTo = "ÄÃ  Náºµng", DistrictCodeTo = "DN-HC", DistrictNameTo = "HÃ²a CÆ°á»ng", TransporterCode = "DATDUC", TransporterName = "CÃ´ng ty Váº­n táº£i Äáº¡t Äá»©c", ModelCode = "CRETA", ModelName = "Hyundai Creta", ValFee = 9_000_000, ExpectedDays = 3 }
                    }
                },
                new TransportFeeVersion
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TFVCode = "TFV-2025-02",
                    Description = "Báº£ng cÆ°á»›c phÃ­ váº­n táº£i Ä‘iá»u chá»‰nh quÃ½ 2/2025 (chá» Ã¡p dá»¥ng)",
                    Status = TransportFeeVersionStatus.Draft,
                    CreatedDate = DateTime.Today.AddDays(-3),
                    Remark = "Äiá»u chá»‰nh tÄƒng cÆ°á»›c tuyáº¿n HÃ  Ná»™i - TP.HCM do giÃ¡ nhiÃªn liá»‡u",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-3),
                    Rates =
                    {
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-02", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Há»“ ChÃ­ Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiá»‡p PhÆ°á»›c", TransporterCode = "NEWWAY", TransporterName = "CÃ´ng ty Váº­n táº£i NewWay", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 19_500_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-02", ProvinceCodeFrom = "HN", ProvinceNameFrom = "HÃ  Ná»™i", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "ÄÃ´ng Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Há»“ ChÃ­ Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiá»‡p PhÆ°á»›c", TransporterCode = "NEWWAY", TransporterName = "CÃ´ng ty Váº­n táº£i NewWay", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ValFee = 21_000_000, ExpectedDays = 5 }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u HÃ³a Ä‘Æ¡n GTGT NhÃ  mÃ¡y sáº£n xuáº¥t TCG (VAT_TCGInvoice / FrmMngTCGInvoice)
        if (!await db.TCGInvoices.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.TCGInvoices.AddRange(
                new TCGInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TCGInvoiceCode = "TCG-202505-001",
                    SourceInvoiceCode = TCGInvoiceSource.Invoice,
                    InvoiceIDType = "TCG",
                    InvoiceIDCode = "1C25TAA",
                    VatTCGStatus = TCGInvoiceStatus.Finished,
                    TCGInvoiceNo = "0000001",
                    TCGInvoiceDate = DateTime.Today.AddDays(-20),
                    VAT = "10",
                    FlagisHTC = "1",
                    Remark = "HÃ³a Ä‘Æ¡n GTGT nhÃ  mÃ¡y TCG xuáº¥t cho HTC lÃ´ xe Santa Fe & Tucson nháº­p kho",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-22),
                    ApprovedBy = "KeToanTruong_HTC",
                    ApprovedAt = DateTime.Today.AddDays(-20),
                    Details =
                    {
                        new TCGInvoiceDetail { OrgId = TenantContext.DefaultOrgId, TCGInvoiceCode = "TCG-202505-001", VIN = "KMHST81D8PU123001", TCGUnitPrice = 1_100_000_000, TCGVAT = 10, TInvoicePrice = 1_000_000_000, BrandName = "HYUNDAI", CarType = "SANTAFE", ProductionMonth = "2025-03", TCGStatusDetail = TCGInvoiceDetailStatus.Finished },
                        new TCGInvoiceDetail { OrgId = TenantContext.DefaultOrgId, TCGInvoiceCode = "TCG-202505-001", VIN = "KMHJ3813DPU123002", TCGUnitPrice = 880_000_000, TCGVAT = 10, TInvoicePrice = 800_000_000, BrandName = "HYUNDAI", CarType = "TUCSON", ProductionMonth = "2025-03", TCGStatusDetail = TCGInvoiceDetailStatus.Finished }
                    }
                },
                new TCGInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TCGInvoiceCode = "TCG-202505-002",
                    SourceInvoiceCode = TCGInvoiceSource.Invoice,
                    InvoiceIDType = "TCG",
                    InvoiceIDCode = "1C25TAA",
                    VatTCGStatus = TCGInvoiceStatus.Pending,
                    TCGInvoiceNo = null,
                    TCGInvoiceDate = null,
                    VAT = "10",
                    FlagisHTC = "1",
                    Remark = "HÃ³a Ä‘Æ¡n GTGT nhÃ  mÃ¡y TCG lÃ´ xe Palisade nháº­p kháº©u CBU, chá» duyá»‡t",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-3),
                    Details =
                    {
                        new TCGInvoiceDetail { OrgId = TenantContext.DefaultOrgId, TCGInvoiceCode = "TCG-202505-002", VIN = "KMHST81D8PU123004", TCGUnitPrice = 1_650_000_000, TCGVAT = 10, TInvoicePrice = 1_500_000_000, BrandName = "HYUNDAI", CarType = "PALISADE", CustomsClearanceDate = DateTime.Today.AddDays(-10), ProductionMonth = "2025-02", TCGStatusDetail = TCGInvoiceDetailStatus.Pending },
                        new TCGInvoiceDetail { OrgId = TenantContext.DefaultOrgId, TCGInvoiceCode = "TCG-202505-002", VIN = "KMHST81D8PU123005", TCGUnitPrice = 1_320_000_000, TCGVAT = 10, TInvoicePrice = 1_200_000_000, BrandName = "HYUNDAI", CarType = "PALISADE", CustomsClearanceDate = DateTime.Today.AddDays(-10), ProductionMonth = "2025-02", TCGStatusDetail = TCGInvoiceDetailStatus.Pending }
                    }
                },
                new TCGInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TCGInvoiceCode = "TCG-202505-003",
                    SourceInvoiceCode = TCGInvoiceSource.Invoice,
                    InvoiceIDType = "TCG",
                    InvoiceIDCode = "1C25TAA",
                    VatTCGStatus = TCGInvoiceStatus.Cancelled,
                    TCGInvoiceNo = "0000002",
                    TCGInvoiceDate = DateTime.Today.AddDays(-15),
                    VAT = "10",
                    FlagisHTC = "1",
                    Remark = "HÃ³a Ä‘Æ¡n GTGT nhÃ  mÃ¡y TCG lÃ´ xe Accent bá»‹ há»§y do sai thÃ´ng tin",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-18),
                    ApprovedBy = "KeToanTruong_HTC",
                    ApprovedAt = DateTime.Today.AddDays(-15),
                    CancelledBy = "KeToanTruong_HTC",
                    CancelledAt = DateTime.Today.AddDays(-12),
                    Details =
                    {
                        new TCGInvoiceDetail { OrgId = TenantContext.DefaultOrgId, TCGInvoiceCode = "TCG-202505-003", VIN = "KMHCT41D8PU123006", TCGUnitPrice = 550_000_000, TCGVAT = 10, TInvoicePrice = 500_000_000, BrandName = "HYUNDAI", CarType = "ACCENT", ProductionMonth = "2025-01", TCGStatusDetail = TCGInvoiceDetailStatus.Cancelled }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u File / Chá»©ng tá»« ÄÃ­nh kÃ¨m ThÆ° báº£o lÃ£nh (Pmt_GuaranteeAttachFile / Pmt_GuaranteeAttachFileHis)
        if (!await db.GuaranteeAttachFiles.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            var grtVcb = await db.Guarantees.FirstOrDefaultAsync(g => g.OrgId == TenantContext.DefaultOrgId && g.GuaranteeNo == "GRT-20250501-001");
            var grtTcb = await db.Guarantees.FirstOrDefaultAsync(g => g.OrgId == TenantContext.DefaultOrgId && g.GuaranteeNo == "GRT-20250510-002");

            if (grtVcb != null)
            {
                db.GuaranteeAttachFiles.AddRange(
                    new GuaranteeAttachFile
                    {
                        OrgId = TenantContext.DefaultOrgId,
                        GuaranteeId = grtVcb.Id,
                        GuaranteeNo = grtVcb.GuaranteeNo,
                        FileIndex = 1,
                        GrtFilePath = "UploadedFiles/PmtGrtAttachFile/DLR-HYUNDAI-THANHXUAN-VCB-BL-VCB-2025-088-01.PDF",
                        GrtFileName = "DLR-HYUNDAI-THANHXUAN-VCB-BL-VCB-2025/088-01.PDF",
                        FileSizeInBytes = 1_248_576,
                        GrtFileRemark = "Báº£n scan ThÆ° báº£o lÃ£nh gá»‘c do Vietcombank phÃ¡t hÃ nh",
                        LogLUBy = "ChuyenVienKinhDoanh",
                        LogLUDateTime = DateTime.Now.AddDays(-19)
                    },
                    new GuaranteeAttachFile
                    {
                        OrgId = TenantContext.DefaultOrgId,
                        GuaranteeId = grtVcb.Id,
                        GuaranteeNo = grtVcb.GuaranteeNo,
                        FileIndex = 2,
                        GrtFilePath = "UploadedFiles/PmtGrtAttachFile/DLR-HYUNDAI-THANHXUAN-VCB-BL-VCB-2025-088-02.PDF",
                        GrtFileName = "DLR-HYUNDAI-THANHXUAN-VCB-BL-VCB-2025/088-02.PDF",
                        FileSizeInBytes = 512_000,
                        GrtFileRemark = "CÃ´ng vÄƒn xÃ¡c nháº­n háº¡n má»©c báº£o lÃ£nh cá»§a ngÃ¢n hÃ ng",
                        LogLUBy = "ChuyenVienKinhDoanh",
                        LogLUDateTime = DateTime.Now.AddDays(-18)
                    }
                );
            }

            if (grtTcb != null)
            {
                db.GuaranteeAttachFiles.Add(
                    new GuaranteeAttachFile
                    {
                        OrgId = TenantContext.DefaultOrgId,
                        GuaranteeId = grtTcb.Id,
                        GuaranteeNo = grtTcb.GuaranteeNo,
                        FileIndex = 1,
                        GrtFilePath = "UploadedFiles/PmtGrtAttachFile/DLR-HYUNDAI-HADONG-TCB-BL-TCB-2025-112-01.PDF",
                        GrtFileName = "DLR-HYUNDAI-HADONG-TCB-BL-TCB-2025/112-01.PDF",
                        FileSizeInBytes = 2_048_000,
                        GrtFileRemark = "Báº£n scan ThÆ° báº£o lÃ£nh Techcombank",
                        LogLUBy = "ChuyenVienKinhDoanh",
                        LogLUDateTime = DateTime.Now.AddDays(-10)
                    }
                );
            }

            await db.SaveChangesAsync();

            // Lá»‹ch sá»­ lÆ°u file (Pmt_GuaranteeAttachFileHis) â€” ghi láº¡i tráº¡ng thÃ¡i file hiá»‡n hÃ nh
            var seededFiles = await db.GuaranteeAttachFiles
                .Where(x => x.OrgId == TenantContext.DefaultOrgId)
                .ToListAsync();
            foreach (var f in seededFiles)
            {
                db.GuaranteeAttachFileHis.Add(new GuaranteeAttachFileHis
                {
                    OrgId = f.OrgId,
                    GuaranteeNo = f.GuaranteeNo,
                    FileIndex = f.FileIndex,
                    GrtFilePath = f.GrtFilePath,
                    GrtFileName = f.GrtFileName,
                    FileSizeInBytes = f.FileSizeInBytes,
                    GrtFileRemark = f.GrtFileRemark,
                    LogLUBy = f.LogLUBy,
                    LogLUDateTime = f.LogLUDateTime
                });
            }
            await db.SaveChangesAsync();
        }

        // Dá»¯ liá»‡u máº«u Äá» nghá»‹ xuáº¥t hÃ³a Ä‘Æ¡n / giao há»“ sÆ¡ (RD_ReqInvoice / RD_ReqInvoiceDtl)
        if (!await db.ReqInvoices.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.ReqInvoices.AddRange(
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-001",
                    ReqIVStatus = ReqIVStatus.Approved,
                    Remark = "Äá» nghá»‹ giao há»“ sÆ¡ (hÃ³a Ä‘Æ¡n GTGT + chá»©ng tá»« gá»‘c) cho Ä‘áº¡i lÃ½ Hyundai Thanh XuÃ¢n",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-15),
                    ApprovedBy = "KeToan_HTC",
                    ApprovedAt = DateTime.Today.AddDays(-14),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-001", VIN = "KMHST81D8PU123001", CarId = "CAR-SF-001", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ColorCode = "WHT", ColorName = "Tráº¯ng", EngineNo = "D4HB-123001", TypeRDReqIv = RDInvoiceType.Dealer, DealerCode = "DLR-HYUNDAI-THANHXUAN", DealerName = "CÃ´ng ty TNHH Hyundai Thanh XuÃ¢n", HTCInvoiceNo = "0000123", DlrCtrNo = "PLHD-202505-001", ProvinceName = "HÃ  Ná»™i", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-15), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-14) },
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-001", VIN = "KMHJ3813DPU123002", CarId = "CAR-TU-002", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", ColorCode = "BLK", ColorName = "Äen", EngineNo = "G4NA-123002", TypeRDReqIv = RDInvoiceType.Dealer, DealerCode = "DLR-HYUNDAI-THANHXUAN", DealerName = "CÃ´ng ty TNHH Hyundai Thanh XuÃ¢n", HTCInvoiceNo = "0000123", DlrCtrNo = "PLHD-202505-001", ProvinceName = "HÃ  Ná»™i", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-15), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-14) }
                    }
                },
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-002",
                    ReqIVStatus = ReqIVStatus.Pending,
                    Remark = "Äá» nghá»‹ giao há»“ sÆ¡ cho ngÃ¢n hÃ ng báº£o lÃ£nh Vietcombank (chá» duyá»‡t chi tiáº¿t)",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-5),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-002", VIN = "KMHST81D8PU123004", CarId = "CAR-PA-004", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ColorCode = "GRY", ColorName = "XÃ¡m", EngineNo = "D4HB-123004", TypeRDReqIv = RDInvoiceType.BankBL, DealerCode = "DLR-HYUNDAI-HADONG", DealerName = "CÃ´ng ty TNHH Hyundai HÃ  ÄÃ´ng", MortageBankCode = "VCB", GuaranteeNo = "GRT-20250501-001", PGBankCode = "VCB", PGBankCodeMonitor = "VCB", PGDateExpired = DateTime.Today.AddDays(60), DlrCtrNo = "PLHD-202505-002", ProvinceName = "HÃ  Ná»™i", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-5), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-4) },
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-002", VIN = "KMHST81D8PU123005", CarId = "CAR-PA-005", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ColorCode = "WHT", ColorName = "Tráº¯ng", EngineNo = "D4HB-123005", TypeRDReqIv = RDInvoiceType.BankBL, DealerCode = "DLR-HYUNDAI-HADONG", DealerName = "CÃ´ng ty TNHH Hyundai HÃ  ÄÃ´ng", MortageBankCode = "VCB", GuaranteeNo = "GRT-20250501-001", PGBankCode = "VCB", PGBankCodeMonitor = "VCB", PGDateExpired = DateTime.Today.AddDays(60), DlrCtrNo = "PLHD-202505-002", ProvinceName = "HÃ  Ná»™i", RDReqIvDtlStatus = RDReqIvDtlStatus.Pending, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-5) }
                    }
                },
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-003",
                    ReqIVStatus = ReqIVStatus.Pending,
                    Remark = "Äá» nghá»‹ giao há»“ sÆ¡ cho ngÃ¢n hÃ ng L/C Techcombank",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-2),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-003", VIN = "KMHCT41D8PU123006", CarId = "CAR-AC-006", ModelCode = "ACCENT", ModelName = "Hyundai Accent", ColorCode = "RED", ColorName = "Äá»", EngineNo = "G4LC-123006", TypeRDReqIv = RDInvoiceType.BankLC, DealerCode = "DLR-HYUNDAI-CANTHO", DealerName = "CÃ´ng ty TNHH MTV Hyundai Cáº§n ThÆ¡", MortageBankCode = "TCB", GuaranteeNo = "GRT-20250510-002", PGBankCode = "TCB", PGBankCodeMonitor = "TCB", PGDateExpired = DateTime.Today.AddDays(45), DlrCtrNo = "PLHD-202505-003", ProvinceName = "Cáº§n ThÆ¡", RDReqIvDtlStatus = RDReqIvDtlStatus.Pending, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-2) }
                    }
                }
            );
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Thêm cột cho bảng SQLite nếu cột thực sự chưa tồn tại (SQLite không hỗ trợ ADD COLUMN IF NOT EXISTS).
    /// Tránh lỗi "duplicate column name" khi bảng đã có cột (tạo bằng CREATE TABLE ở trên).
    /// </summary>
    private static async Task EnsureSqliteColumnAsync(AppDbContext db, string table, string column, string columnDef)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $"PRAGMA table_info(\"{table}\");";
            var exists = false;
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    if (string.Equals(name, column, System.StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }
            if (!exists)
            {
                await using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN {columnDef};";
                await alter.ExecuteNonQueryAsync();
            }
        }
    }
}
