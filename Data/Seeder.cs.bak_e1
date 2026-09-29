using Microsoft.EntityFrameworkCore;
using MiniPay.Models;
using MiniPay.Services;

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
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN ""DateWarning"" TEXT NULL;
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN ""DateExpired"" TEXT NULL;
                ALTER TABLE ""PaymentGuaranteeDetails"" ADD COLUMN ""FlagDtlDiscount"" TEXT NOT NULL DEFAULT '0';

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

        // Bảng Đề nghị xuất hóa đơn / giao hồ sơ (RD_ReqInvoice) — tạo cho DB đã tồn tại từ phiên trước
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
                    DateWarning = DateTime.Today.AddDays(55),
                    DateExpired = DateTime.Today.AddDays(70),
                    FlagDtlDiscount = "0",
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
                    DateWarning = DateTime.Today.AddDays(55),
                    DateExpired = DateTime.Today.AddDays(70),
                    FlagDtlDiscount = "0",
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

        // Dữ liệu mẫu Biên bản bàn giao xe & chứng từ theo Hối phiếu ngân hàng (Car_BankBillMinutes / BizHTC.Payment)
        if (!await db.BankBillMinutes.AnyAsync(b => b.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Biên bản bàn giao VPBank - Ngân hàng đã tiếp nhận đủ chứng từ gốc xe (BankReceived)
            var bb1 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250505-001",
                BankCode = "VPB",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                PartnerCode = "DLR-HYUNDAI-HADONG",
                PartnerName = "Đại lý Ô tô Hyundai Hà Đông",
                BankBillDate = DateTime.Today.AddDays(-10),
                BankBillPrintDate = DateTime.Today.AddDays(-10),
                BankBillReciveDate = DateTime.Today.AddDays(-6),
                TotalVehicles = 3,
                TotalClaimAmount = 2_750_000_000,
                Status = BankBillMinutesStatus.BankReceived,
                Remark = "Bàn giao hồ sơ gốc xe kèm Hối phiếu tài trợ vốn đợt 1 tháng 05/2025 qua VPBank Thăng Long",
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
                    SpecDescription = "Hyundai Santa Fe Calligraphy 2.5 Turbo 6 chỗ",
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
                    Note = "Bản gốc CO xuất xưởng, Giấy chứng nhận chất lượng CQ, Tờ khai HQ"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100102",
                    ModelCode = "TUCSON-TURBO",
                    SpecCode = "TUC-1.6T-HTRAC",
                    SpecDescription = "Hyundai Tucson 1.6 T-GDi Đặc biệt HTRAC",
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
                    Note = "Bản gốc CO xuất xưởng, Hóa đơn VAT HTC bản gốc"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100103",
                    ModelCode = "ACCENT-AT",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "Hyundai Accent 1.5 AT Bản Đặc biệt",
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
                    Note = "Đủ hồ sơ gốc hợp lệ, ngân hàng đã kiểm tra và lưu kho hồ sơ"
                }
            );
            await db.SaveChangesAsync();

            // 2. Biên bản bàn giao VietinBank (CTG) - Đã xuất trình bàn giao chứng từ sang Ngân hàng (Delivered)
            var bb2 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250508-002",
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công thương Việt Nam (VietinBank)",
                PartnerCode = "DLR-HYUNDAI-THANHXUAN",
                PartnerName = "Đại lý Ô tô Hyundai Thanh Xuân",
                BankBillDate = DateTime.Today.AddDays(-4),
                BankBillPrintDate = DateTime.Today.AddDays(-4),
                TotalVehicles = 2,
                TotalClaimAmount = 1_200_000_000,
                Status = BankBillMinutesStatus.Delivered,
                Remark = "Đã xuất trình bộ hồ sơ xe và Hối phiếu sang VietinBank Tràng An, đang đợi chuyên viên ký nhận",
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
                    SpecDescription = "Hyundai Creta 1.5L Xăng Cao cấp",
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
                    Note = "Bản gốc CO và CQ xuất xưởng"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100202",
                    ModelCode = "STARGAZER-X",
                    SpecCode = "SGZ-X-PREM",
                    SpecDescription = "Hyundai Stargazer X 1.5L Cao cấp",
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
                    Note = "Hồ sơ bàn giao niêm phong túi hồ sơ xe"
                }
            );
            await db.SaveChangesAsync();

            // 3. Biên bản bàn giao Techcombank (TCB) - Đã thanh toán quyết toán hối phiếu hoàn tất (Settled)
            var bb3 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250512-003",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
                PartnerCode = "DLR-HYUNDAI-PHAMVANVO",
                PartnerName = "Đại lý Ô tô Hyundai Phạm Văn Đồng",
                BankBillDate = DateTime.Today.AddDays(-15),
                BankBillPrintDate = DateTime.Today.AddDays(-15),
                BankBillReciveDate = DateTime.Today.AddDays(-12),
                TotalVehicles = 2,
                TotalClaimAmount = 1_850_000_000,
                Status = BankBillMinutesStatus.Settled,
                Remark = "Đã thu tiền thanh toán đủ 1.85 tỷ đồng qua hối phiếu Techcombank, hoàn tất quyết toán hồ sơ",
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
                    SpecDescription = "Hyundai Custin 1.5 Turbo Bản Đặc biệt",
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
                    Note = "Đã quyết toán thanh toán"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100302",
                    ModelCode = "PALISADE-PRE",
                    SpecCode = "PLS-2.2D-PREM",
                    SpecDescription = "Hyundai Palisade 2.2 Diesel Prestige 6 chỗ",
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
                    Note = "Đã quyết toán thanh toán"
                }
            );
            await db.SaveChangesAsync();

            // 4. Biên bản bàn giao MBBank - Mới lập, Chờ bàn giao sang ngân hàng (PendingHandover)
            var bb4 = new BankBillMinutes
            {
                OrgId = TenantContext.DefaultOrgId,
                BankBillMnNo = "BBBG-DEMO-20250514-004",
                BankCode = "MBB",
                BankName = "Ngân hàng TMCP Quân Đội (MBBank)",
                PartnerCode = "DLR-HYUNDAI-SAIGON",
                PartnerName = "Đại lý Ô tô Hyundai Sài Gòn",
                BankBillDate = DateTime.Today,
                BankBillPrintDate = DateTime.Today,
                TotalVehicles = 2,
                TotalClaimAmount = 1_680_000_000,
                Status = BankBillMinutesStatus.PendingHandover,
                Remark = "Đợt giao xe Tucson và Elantra bảo lãnh qua MBBank Chi nhánh Sở Giao dịch",
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
                    Note = "Chuẩn bị niêm phong chứng từ giao Ngân hàng"
                },
                new BankBillMinutesDetail
                {
                    MinutesId = bb4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU100402",
                    ModelCode = "ELANTRA-NLINE",
                    SpecCode = "ELN-1.6T-NL",
                    SpecDescription = "Hyundai Elantra N Line 1.6 T-GDi Thể thao",
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
                    Note = "Bản gốc CO và CQ đầy đủ"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Bảng kê thanh toán chi phí kiểm tra xe PDI (Pmt_PaymentPDI / BizHTC.Payment)
        if (!await db.PaymentPDIs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Bảng kê tháng 04/2025 - Đã thanh toán & Quyết toán chi phí hoàn tất (Paid)
            var pdi1 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202504-001",
                PmtMonth = "2025-04",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tâm Dịch vụ Kỹ thuật & PDI Ô tô TCMS",
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
                Remark = "Thanh toán chi phí PDI xe nhập/xuất kho kỳ tháng 04/2025 tại Kho Nhà máy Ninh Bình",
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
                    SpecDescription = "Bản 6 chỗ cao cấp HTRAC AWD",
                    ColorExtNameVN = "Trắng ngọc trai",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-30),
                    DeliveryOutDate = DateTime.Today.AddDays(-16),
                    DlvMnNo = "BBVC-2025-0401",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI nhập kho: 100% đạt tiêu chuẩn; PDI xuất kho: rửa sạch, nạp ắc quy, kiểm tra áp suất lốp"
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
                    SpecDescription = "Bản 1.6 Turbo Xăng HTRAC",
                    ColorExtNameVN = "Đen Ánh kim",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-28),
                    DeliveryOutDate = DateTime.Today.AddDays(-15),
                    DlvMnNo = "BBVC-2025-0402",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI đầy đủ hệ thống điện, cảm biến và cân bằng điện tử ESC"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200103",
                    CarId = "CAR-CRT-0103",
                    ModelCode = "CRETA-PRE",
                    ModelName = "Hyundai Creta 1.5 Cao cấp",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Bản SmartSense cao cấp",
                    ColorExtNameVN = "Đỏ Đô",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-27),
                    DeliveryOutDate = DateTime.Today.AddDays(-14),
                    DlvMnNo = "BBVC-2025-0403",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI đạt yêu cầu, kiểm tra đèn LED trước sau hoàn hảo"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200104",
                    CarId = "CAR-ACC-0104",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT Đặc biệt",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Số tự động bản Đặc biệt 2025",
                    ColorExtNameVN = "Bạc Ánh kim",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-25),
                    DeliveryOutDate = DateTime.Today.AddDays(-13),
                    DlvMnNo = "BBVC-2025-0404",
                    DealerCode = "DLR-HYUNDAI-SAIGON",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI động cơ SmartStream G1.5 kiểm tra áp suất dầu và dung dịch làm mát"
                }
            );
            await db.SaveChangesAsync();

            // 2. Bảng kê tháng 05/2025 đợt 1 - HTV Đã phê duyệt cấp 2 (HTVApproved), Chờ kế toán chuyển khoản thanh toán
            var pdi2 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-001",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tâm Dịch vụ Kỹ thuật & PDI Ô tô TCMS",
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
                Remark = "Bảng kê đợt 1 tháng 05/2025 xe Palisade, Custin, Stargazer X tại Kho Đông Anh",
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
                    SpecDescription = "Bản Prestige máy Dầu 6 chỗ",
                    ColorExtNameVN = "Xanh Bóng Đêm",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-12),
                    DeliveryOutDate = DateTime.Today.AddDays(-5),
                    DlvMnNo = "BBVC-2025-0511",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI hệ thống âm thanh Infinity 12 loa, kiểm tra radar SmartSense"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200202",
                    CarId = "CAR-CST-0202",
                    ModelCode = "CUSTIN-1.5T",
                    ModelName = "Hyundai Custin 1.5 Turbo Đặc biệt",
                    SpecCode = "CST-1.5T-DB",
                    SpecDescription = "Bản MPV 7 chỗ cửa trượt điện",
                    ColorExtNameVN = "Trắng ngọc trai",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-11),
                    DeliveryOutDate = DateTime.Today.AddDays(-4),
                    DlvMnNo = "BBVC-2025-0512",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI ghế cơ trưởng hàng ghế 2 chỉnh điện, cửa gió điều hòa độc lập"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200203",
                    CarId = "CAR-SGZ-0203",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao cấp",
                    SpecCode = "SGZ-X-PREM",
                    SpecDescription = "Bản Crossover MPV 7 chỗ",
                    ColorExtNameVN = "Xám Kim loại",
                    StorageCodeInit = "KHO-DONGANH",
                    StoreDate = DateTime.Today.AddDays(-10),
                    DeliveryOutDate = DateTime.Today.AddDays(-4),
                    DlvMnNo = "BBVC-2025-0513",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI phanh tay điện tử EPB và Auto Hold hoạt động chuẩn"
                }
            );
            await db.SaveChangesAsync();

            // 3. Bảng kê tháng 05/2025 đợt 2 - TCMS Đã duyệt cấp 1 (TCMSApproved), Đang chờ lãnh đạo HTV duyệt cấp 2
            var pdi3 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-002",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tâm Dịch vụ Kỹ thuật & PDI Ô tô TCMS",
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
                Remark = "Đợt giao xe Elantra N Line, Accent và Creta cho đại lý khu vực Miền Bắc",
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
                    SpecDescription = "Bản thể thao 204 mã lực ly hợp kép 7 cấp",
                    ColorExtNameVN = "Đỏ Thể thao",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DlvMnNo = "BBVC-2025-0521",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI chế độ lái Sport+, cánh gió thể thao và ống xả kép thể thao"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200302",
                    CarId = "CAR-ACC-0302",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT Tiêu chuẩn",
                    SpecCode = "ACC-1.5L-TC",
                    SpecDescription = "Bản tự động tiêu chuẩn 2025",
                    ColorExtNameVN = "Trắng",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DlvMnNo = "BBVC-2025-0522",
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI kiểm tra 4 lốp, hệ thống cân bằng áp suất TPMS chuẩn"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200303",
                    CarId = "CAR-CRT-0303",
                    ModelCode = "CRETA-PRE",
                    ModelName = "Hyundai Creta 1.5 Đặc biệt",
                    SpecCode = "CRT-1.5L-DB",
                    SpecDescription = "Bản máy xăng Đặc biệt",
                    ColorExtNameVN = "Xanh Dương",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-4),
                    DeliveryOutDate = DateTime.Today.AddDays(-1),
                    DlvMnNo = "BBVC-2025-0523",
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Approved,
                    Remark = "PDI hoàn thiện tem nhãn, sổ hướng dẫn sử dụng và phụ kiện theo xe"
                }
            );
            await db.SaveChangesAsync();

            // 4. Bảng kê tháng 05/2025 đợt 3 - Mới tạo nháp (Draft), Đang chuẩn bị kiểm định xe
            var pdi4 = new PaymentPDI
            {
                OrgId = TenantContext.DefaultOrgId,
                PmtPDINo = "PDI-202505-003",
                PmtMonth = "2025-05",
                ServiceUnitCode = "TCMS",
                ServiceUnitName = "Trung tâm Dịch vụ Kỹ thuật & PDI Ô tô TCMS",
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
                Remark = "Đợt bàn giao xe điện thuần IONIQ 5 và SUV đô thị Venue vừa hoàn tất kiểm tra kho",
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
                    SpecDescription = "Xe thuần điện Pin 72.6 kWh công nghệ sạc 800V siêu nhanh",
                    ColorExtNameVN = "Vàng Titan Mờ",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-2),
                    DeliveryOutDate = DateTime.Today,
                    DlvMnNo = "BBVC-2025-0531",
                    DealerCode = "DLR-HYUNDAI-SAIGON",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Pending,
                    Remark = "PDI dung lượng pin SOC 85%, cổng sạc CCS2 và bộ sạc di động 220V kèm xe"
                },
                new PaymentPDIDetail
                {
                    PaymentPDIId = pdi4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU200402",
                    CarId = "CAR-VNU-0402",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 Turbo Đặc biệt",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "Bản SUV cỡ A động cơ Turbo T-GDi",
                    ColorExtNameVN = "Xanh Ngọc",
                    StorageCodeInit = "KHO-NINHBINH",
                    StoreDate = DateTime.Today.AddDays(-2),
                    DeliveryOutDate = DateTime.Today,
                    DlvMnNo = "BBVC-2025-0532",
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    CostInCheck = 150_000,
                    CostOutCheck = 200_000,
                    TotalCostCheck = 350_000,
                    Status = PaymentPDIDetailStatus.Pending,
                    Remark = "PDI kiểm tra động cơ tăng áp 1.0L, hộp số tự động 7DCT mượt mà"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 10. Seed Dữ liệu mẫu Hồ sơ Quản lý & Tính phạt Chậm thanh toán Xe (Late Payment Delay Penalty) =====
        if (!await db.LatePaymentPenalties.AnyAsync())
        {
            // 1. Hồ sơ phạt ĐÃ QUYẾT TOÁN THU PHẠT (Settled) - Đại lý Hyundai Hà Đông
            var pen1 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202504-001",
                SOCode = "SO-2025-04-HD01",
                DealerCode = "DLR-HYUNDAI-HADONG",
                DealerName = "Hyundai Hà Đông (Ủy quyền phân phối)",
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
                Remark = "Lô 3 xe SantaFe và Tucson giao đợt 1 tháng 04/2025",
                AdjustmentReason = "Miễn giảm 2.821.918 đ do đại lý đối soát bù trừ tài khoản bảo lãnh ngân hàng VPBank",
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
                    ColorName = "Đen Huyền Bí",
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
                    Note = "Trễ mốc thanh toán bảo lãnh 12 ngày do ngân hàng chuyển lệnh muộn"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-2025-02",
                    VIN = "KMHCT81EPHU300102",
                    ModelCode = "SANTAFE-PRE",
                    ModelName = "Hyundai SantaFe Cao cấp 2.5 H-Trac",
                    ColorName = "Trắng Tinh Khôi",
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
                    Note = "Trễ hạn bảo lãnh ngân hàng 10 ngày"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-2025-01",
                    VIN = "KMHCT81EPHU300103",
                    ModelCode = "TUCSON-TURBO",
                    ModelName = "Hyundai Tucson 1.6T HTRAC Turbo",
                    ColorName = "Đỏ Mận",
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
                    Note = "Thanh toán dứt điểm khi nhận hồ sơ gốc"
                }
            );
            await db.SaveChangesAsync();

            // 2. Hồ sơ phạt ĐÃ PHÊ DUYỆT CHỐT PHẠT (Approved) - Đại lý Hyundai Sài Gòn
            var pen2 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-001",
                SOCode = "SO-2025-05-SG02",
                DealerCode = "DLR-HYUNDAI-SAIGON",
                DealerName = "Hyundai Sài Gòn 1S (Đại lý Miền Nam)",
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
                Remark = "Đơn xe giao khu vực phía Nam lô đầu tháng 05/2025",
                AdjustmentReason = "Giảm 2.043.562 đ do thời gian vận chuyển tàu hỏa Bắc - Nam bị chậm tiến độ giao nhận",
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
                    ModelName = "Hyundai Custin 2.0L Turbo Cao Cấp",
                    ColorName = "Bạc Ánh Kim",
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
                    Note = "Chậm thanh toán bảo lãnh tại MBBank"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SAN-2025-03",
                    VIN = "KMHCT81EPHU300202",
                    ModelCode = "SANTAFE-EXT",
                    ModelName = "Hyundai SantaFe Tiêu chuẩn 2.5L",
                    ColorName = "Đen",
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
                    Note = "Ban Giám đốc đã ký duyệt chốt số tiền phạt"
                }
            );
            await db.SaveChangesAsync();

            // 3. Hồ sơ phạt ĐANG THẨM ĐỊNH (Reviewed) - Đại lý Hyundai Đông Anh
            var pen3 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-002",
                SOCode = "SO-2025-05-DA03",
                DealerCode = "DLR-HYUNDAI-DONGANH",
                DealerName = "Hyundai Đông Anh (Chi nhánh miền Bắc)",
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
                Remark = "Đơn xe Creta và Accent đợt giữa tháng 05",
                AdjustmentReason = "Kế toán thẩm định đề xuất mức phạt 3.500.000 đ theo đề xuất giải trình của đại lý",
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
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    ColorName = "Đỏ Mận",
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
                    Note = "Chậm tiến độ thanh toán tiền bảo lãnh 8 ngày"
                },
                new LatePaymentPenaltyDetail
                {
                    PenaltyId = pen3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-2025-01",
                    VIN = "KMHCT81EPHU300302",
                    ModelCode = "ACCENT-AT-DACBIET",
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    ColorName = "Trắng",
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
                    Note = "Chờ Giám đốc Tài chính phê duyệt mức phạt cuối"
                }
            );
            await db.SaveChangesAsync();

            // 4. Hồ sơ phạt VỪA TÍNH TỰ ĐỘNG (Calculated) - Đại lý Hyundai An Khánh
            var pen4 = new LatePaymentPenalty
            {
                OrgId = TenantContext.DefaultOrgId,
                PenaltyRecordNo = "PEN-202505-003",
                SOCode = "SO-2025-05-AK04",
                DealerCode = "DLR-HYUNDAI-ANKHANH",
                DealerName = "Hyundai An Khánh (Đại lý 3S)",
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
                Remark = "Đơn bổ sung xe Accent số tự động cho đại lý An Khánh",
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
                    ModelName = "Hyundai Accent 1.5 AT Tiêu Chuẩn",
                    ColorName = "Bạc",
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
                    Note = "Hồ sơ mới tính toán, đang chuẩn bị chuyển thẩm định"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 11. Seed Dữ liệu mẫu Bảng kê Thanh toán Chi phí Vận tải & Bảo hiểm Xe (Pmt_TransportIns) =====
        if (!await db.TransportInsPayments.AnyAsync())
        {
            // 1. Bảng kê ĐÃ QUYẾT TOÁN THANH TOÁN (Settled) - Đơn vị vận tải NewWay (Tháng 04/2025)
            // Tuyến Ninh Bình -> Hà Nội, 3 xe du lịch SantaFe, Tucson, Creta
            var tip1 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202504-001",
                PmtMonth = "2025-04",
                TransporterCode = "TRANS-NEWWAY",
                TransporterName = "Công ty Cổ phần Vận tải NewWay",
                InsuranceCompanyCode = "PVI",
                InsuranceCompanyName = "Tổng Công ty Cổ phần Bảo hiểm Dầu khí Việt Nam (PVI)",
                InsuranceContractNo = "HD-BH-PVI-2025/VT09",
                TotalVehicles = 3,
                TotalTransportCost = 5_550_000,    // 3 xe * 1,850,000
                TotalDelayPenalty = 0,            // Đúng hạn
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
                Remark = "Quyết toán thanh toán cước vận tải và phí bảo hiểm hàng hóa xe đợt 1 tháng 4 qua VCB",
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
                    SpecDescription = "Bản cao cấp 6 chỗ ngồi dẫn động HTRAC",
                    ColorName = "Đen Phantom",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "Hà Nội",
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
                    Remark = "Bàn giao an toàn tại đại lý Hyundai Thanh Xuân"
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
                    SpecDescription = "Bản máy xăng tăng áp HTRAC",
                    ColorName = "Trắng Tinh Khôi",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "Hà Nội",
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
                    Remark = "Đầy đủ biên bản bàn giao kèm tem niêm phong"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500103",
                    CarId = "CAR-CRETA-0103",
                    ModelCode = "CRETA-PREMIUM",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Bản máy xăng cao cấp gói SmartSense",
                    ColorName = "Đỏ Mận",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-THANHXUAN",
                    TProvinceName = "Hà Nội",
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
                    Remark = "Giao đủ phụ kiện theo xe"
                }
            );
            await db.SaveChangesAsync();

            // 2. Bảng kê ĐÃ KÝ SỐ CA HOÀN TẤT (Signed) - Nhà xe Đạt Đức (Tháng 05/2025 đợt 1)
            // Tuyến Ninh Bình -> Sài Gòn, 2 xe lớn Palisade và Custin
            var tip2 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-001",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-DATDUC",
                TransporterName = "Công ty TNHH Vận tải Thương mại Đạt Đức",
                InsuranceCompanyCode = "BAOVIET",
                InsuranceCompanyName = "Tổng Công ty Bảo hiểm Bảo Việt",
                InsuranceContractNo = "HD-BH-BV-2025/SG12",
                TotalVehicles = 2,
                TotalTransportCost = 13_000_000,  // 2 xe * 6,500,000 (Bắc - Nam)
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
                Remark = "Đợt vận chuyển Bắc - Nam xe phân khúc cao cấp đã ký số CA hoàn tất, chuẩn bị phát hành UNC",
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
                    SpecDescription = "SUV Flagship 6 chỗ máy dầu cao cấp",
                    ColorName = "Xanh Bóng Đêm",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-SAIGON",
                    TProvinceName = "Hồ Chí Minh",
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
                    Remark = "Bàn giao đúng tiến độ cho đại lý Hyundai Sài Gòn 1S"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500202",
                    CarId = "CAR-CUSTIN-0202",
                    ModelCode = "CUSTIN-TURBO",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CUS-2.0T-PRE",
                    SpecDescription = "MPV 7 chỗ ghế thương gia tích hợp làm mát",
                    ColorName = "Bạc Ánh Kim",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-SAIGON",
                    TProvinceName = "Hồ Chí Minh",
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
                    Remark = "Xe kiểm tra ngoại thất nguyên vẹn, không trầy xước"
                }
            );
            await db.SaveChangesAsync();

            // 3. Bảng kê TCMS ĐÃ DUYỆT CẤP 1 (TCMSApproved) - Vận tải Miền Bắc (Tháng 05/2025 đợt 2)
            // Tuyến Ninh Bình -> Đà Nẵng, 2 xe Accent và Stargazer (Có 1 xe trễ hạn 1 ngày bị phạt)
            var tip3 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-002",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-MIENBAC",
                TransporterName = "Công ty Vận tải & Tiếp vận Miền Bắc",
                InsuranceCompanyCode = "MIC",
                InsuranceCompanyName = "Tổng Công ty Cổ phần Bảo hiểm Quân đội (MIC)",
                InsuranceContractNo = "HD-BH-MIC-2025/DN03",
                TotalVehicles = 2,
                TotalTransportCost = 7_600_000,   // 2 xe * 3,800,000 (Ninh Bình - Đà Nẵng)
                TotalDelayPenalty = 200_000,      // Trễ 1 ngày phạt 200k
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
                Remark = "Đợt giao xe miền Trung có sự cố mưa lũ chậm 1 ngày, TCMS đã duyệt giảm trừ phạt chậm theo hợp đồng",
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
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan hạng B thế hệ mới động cơ Smartstream",
                    ColorName = "Đỏ Tươi",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-DANANG",
                    TProvinceName = "Đà Nẵng",
                    TranspReqType = "CARTRANSPORT",
                    DlvMnNo = "BBVC-MB-2025-0511",
                    DlvStartDate = DateTime.Today.AddDays(-5),
                    ExpectedDays = 3,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-2),
                    DlvEndDate = DateTime.Today.AddDays(-1), // Trễ 1 ngày
                    DelayDays = 1,
                    TFValReal = 3_800_000,
                    TPValReal = 200_000,
                    PriceCar = 650_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 325_000,
                    Val_Transport = 3_925_000,
                    Status = TransportInsDetailStatus.Approved,
                    StandardRemark = "Mưa lũ trên đèo Hải Vân gây ùn tắc kéo dài 1 ngày",
                    Remark = "Đại lý đã xác nhận nhận xe đủ điều kiện"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500302",
                    CarId = "CAR-STARGAZER-0302",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao Cấp",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chỗ phong cách thể thao",
                    ColorName = "Trắng Mờ",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-DANANG",
                    TProvinceName = "Đà Nẵng",
                    TranspReqType = "STORAGEREARRANGE",
                    DlvMnNo = "BBVC-MB-2025-0512",
                    DlvStartDate = DateTime.Today.AddDays(-4),
                    ExpectedDays = 3,
                    ExpectedDlvEndDate = DateTime.Today.AddDays(-1),
                    DlvEndDate = DateTime.Today.AddDays(-1), // Đúng hạn
                    DelayDays = 0,
                    TFValReal = 3_800_000,
                    TPValReal = 0,
                    PriceCar = 600_000_000,
                    InsurancePercent = 0.05m,
                    InsuranceCost = 300_000,
                    Val_Transport = 4_100_000,
                    Status = TransportInsDetailStatus.Approved,
                    Remark = "Lệnh điều chuyển kho bãi phục vụ trưng bày triển lãm"
                }
            );
            await db.SaveChangesAsync();

            // 4. Bảng kê MỚI TẠO NHÁP (Draft) - Đơn vị vận tải Sài Gòn (Tháng 05/2025 đợt 3)
            // Tuyến Ninh Bình -> Cần Thơ, xe điện thuần IONIQ 5 và SUV Venue
            var tip4 = new TransportInsPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                TransportInsNo = "VTBH-202505-003",
                PmtMonth = "2025-05",
                TransporterCode = "TRANS-SAIGON",
                TransporterName = "Công ty Dịch vụ Vận tải Ô tô Sài Gòn",
                InsuranceCompanyCode = "PVI",
                InsuranceCompanyName = "Tổng Công ty Cổ phần Bảo hiểm Dầu khí Việt Nam (PVI)",
                InsuranceContractNo = "HD-BH-PVI-2025/CT01",
                TotalVehicles = 2,
                TotalTransportCost = 13_800_000, // Tuyến Miền Tây 6,900,000 * 2
                TotalDelayPenalty = 0,
                TotalInsuranceCost = 1_000_000,  // 700k + 300k
                TotalAmount = 14_800_000,
                VATRate = 10.0m,
                TotalBeforeVAT = 13_454_545,     // 14,800,000 / 1.1
                AmountVAT = 1_345_455,
                Status = TransportInsStatus.Draft,
                TCMSSignStatus = TransportSignCAStatus.Pending,
                HTVSignStatus = TransportSignCAStatus.Pending,
                Remark = "Đợt giao xe khu vực Tây Nam Bộ mới lập bảng kê, chuẩn bị gửi TCMS thẩm định",
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
                    SpecDescription = "Xe thuần điện Pin 72.6 kWh công nghệ sạc 800V siêu nhanh",
                    ColorName = "Xám Xi Măng",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-CANTHO",
                    TProvinceName = "Cần Thơ",
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
                    Remark = "Vận chuyển bằng xe lồng bọc kín chuyên dụng cho xe điện"
                },
                new TransportInsPaymentDetail
                {
                    TransportInsPaymentId = tip4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    VIN = "KMHCT81EPHU500402",
                    CarId = "CAR-VENUE-0402",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Đặc Biệt",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "SUV đô thị nhỏ gọn động cơ Turbo hộp số ly hợp kép 7DCT",
                    ColorName = "Đỏ Mận",
                    FStorageCode = "KHO-NINHBINH",
                    FProvinceName = "Ninh Bình",
                    TStorageCode = "KHO-CANTHO",
                    TProvinceName = "Cần Thơ",
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
                    Remark = "Bàn giao kèm theo phụ kiện tiêu chuẩn theo xe"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Bảng kê thanh toán chi phí lưu kho xe ô tô (Pmt_PaymentStorage & Pmt_PaymentStorageDetail)
        if (!await db.PaymentStorages.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Bảng kê Tháng 04/2025 - Đã quyết toán chi trả chuyển khoản UNC VietinBank (Settled)
            // Tổng kho Ninh Bình (KHO-NBD), 3 xe SantaFe, Tucson, Creta
            var ps1 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202504-001",
                PmtMonth = "2025-04",
                StorageOperatorCode = "KHO-NBD",
                StorageOperatorName = "Tổng kho Phân phối Ô tô Hyundai Ninh Bình",
                TotalVehicles = 3,
                TotalCoatCost = 150_000,      // 3 xe * 50k bạt che phủ
                TotalStorageCost = 2_250_000,  // (30 + 30 + 30 ngày) * 25k = 90 * 25k = 2,250,000
                TotalAmount = 2_400_000,       // Trước VAT
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
                Remark = "Thanh toán dứt điểm chi phí lưu giữ kho bãi và bạt phủ xe tháng 04/2025",
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
                    SpecDescription = "SUV 7 chỗ máy xăng Turbo dẫn động 4 bánh HTRAC",
                    ColorExtNameVN = "Trắng Tinh Khôi",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-60),
                    ApprovedDate2 = DateTime.Today.AddDays(-28),
                    DeliveryOutDate = DateTime.Today.AddDays(-15),
                    DealerCode = "DLR-HYUNDAI-ANPHU",
                    DealerName = "Hyundai An Phú Sài Gòn",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000, // 30 * 25k
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Bạt bọc kín chống tia cực tím tiêu chuẩn HTC"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps1.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600102",
                    CarId = "CAR-TUC-6002",
                    ModelCode = "TUCSON-1.6T",
                    ModelName = "Hyundai Tucson 1.6 T-GDi Đặc Biệt",
                    SpecCode = "TUC-1.6T-DB",
                    SpecDescription = "Crossover 5 chỗ máy Turbo hộp số ly hợp kép 7 cấp",
                    ColorExtNameVN = "Đen Sang Trọng",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-55),
                    ApprovedDate2 = DateTime.Today.AddDays(-26),
                    DeliveryOutDate = DateTime.Today.AddDays(-16),
                    DealerCode = "DLR-HYUNDAI-THANHXUAN",
                    DealerName = "Hyundai Thanh Xuân Hà Nội",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000,
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Lưu kho khu vực có mái che phụ"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps1.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600103",
                    CarId = "CAR-CRT-6003",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5L Cao Cấp",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "SUV đô thị nhỏ gọn gói công nghệ an toàn SmartSense",
                    ColorExtNameVN = "Đỏ Mận",
                    StorageCodeInit = "KHO-NBD",
                    StorageDate = DateTime.Today.AddDays(-50),
                    ApprovedDate2 = DateTime.Today.AddDays(-25),
                    DeliveryOutDate = DateTime.Today.AddDays(-18),
                    DealerCode = "DLR-HYUNDAI-DANANG",
                    DealerName = "Hyundai Cẩm Lệ Đà Nẵng",
                    InCostStorageDate = new DateTime(2025, 4, 1),
                    OutCostStorageDate = new DateTime(2025, 4, 30),
                    CostStorageMonth = 30,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 750_000,
                    TotalAmount = 800_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Kiểm tra bảo dưỡng lốp định kỳ trong thời gian lưu bãi"
                }
            );
            await db.SaveChangesAsync();

            // 2. Bảng kê Tháng 05/2025 Đợt 1 - Đã ký số CA 2 cấp hoàn tất (Signed)
            // Tổng kho Đông Anh (KHO-DA), 3 xe Palisade, Custin, Stargazer X
            var ps2 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-001",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-DA",
                StorageOperatorName = "Tổng kho Ô tô Hyundai Miền Bắc - Đông Anh",
                TotalVehicles = 3,
                TotalCoatCost = 150_000,       // 3 * 50k
                TotalStorageCost = 1_500_000,   // (20 + 20 + 20 ngày) * 25k = 60 * 25k = 1,500,000
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
                Remark = "Bảng kê đợt 1 tháng 05 xe lưu giữ kho bãi Đông Anh chuẩn bị giao đại lý phía Bắc",
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
                    SpecDescription = "SUV Flagship 7 chỗ động cơ Diesel dẫn động HTRAC",
                    ColorExtNameVN = "Xanh Bóng Đêm",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-35),
                    ApprovedDate2 = DateTime.Today.AddDays(-10),
                    DeliveryOutDate = DateTime.Today.AddDays(-3),
                    DealerCode = "DLR-HYUNDAI-LEVANLUONG",
                    DealerName = "Hyundai Lê Văn Lương",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000, // 20 * 25k
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Xe bọc bạt phủ chuyên dụng bảo vệ lớp sơn bóng"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps2.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600202",
                    CarId = "CAR-CUS-6005",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CST-2.0T-PRE",
                    SpecDescription = "MPV cỡ trung cao cấp cửa trượt điện thông minh",
                    ColorExtNameVN = "Trắng Tuyết",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-32),
                    ApprovedDate2 = DateTime.Today.AddDays(-8),
                    DeliveryOutDate = DateTime.Today.AddDays(-2),
                    DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                    DealerName = "Hyundai Phạm Văn Đồng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000,
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Kiểm tra ắc quy và áp suất lốp trước khi xuất"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps2.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600203",
                    CarId = "CAR-SGZ-6006",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Cao Cấp",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chỗ phong cách thể thao gầm cao",
                    ColorExtNameVN = "Bạc Ánh Kim",
                    StorageCodeInit = "KHO-DA",
                    StorageDate = DateTime.Today.AddDays(-30),
                    ApprovedDate2 = DateTime.Today.AddDays(-9),
                    DeliveryOutDate = DateTime.Today.AddDays(-1),
                    DealerCode = "DLR-HYUNDAI-HADONG",
                    DealerName = "Hyundai Hà Đông",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 20),
                    CostStorageMonth = 20,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 500_000,
                    TotalAmount = 550_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Đã rửa sạch bụi bẩn trước bàn giao nhà xe vận chuyển"
                }
            );
            await db.SaveChangesAsync();

            // 3. Bảng kê Tháng 05/2025 Đợt 2 - TCMS Thẩm định duyệt cấp 2 (TCMSApproved)
            // Đang chờ lãnh đạo ký số CA điện tử. Tổng kho Đà Nẵng (KHO-DN), 2 xe Accent, Venue
            var ps3 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-002",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-DN",
                StorageOperatorName = "Tổng kho Trung chuyển Ô tô Hyundai Miền Trung - Đà Nẵng",
                TotalVehicles = 2,
                TotalCoatCost = 100_000,       // 2 * 50k
                TotalStorageCost = 600_000,     // (12 + 12 ngày) * 25k = 24 * 25k = 600,000
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
                Remark = "Đợt 2 tháng 05/2025 kho Đà Nẵng phân bổ cho các đại lý khu vực Miền Trung",
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
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan phân khúc B thế hệ mới động cơ Smartstream",
                    ColorExtNameVN = "Trắng",
                    StorageCodeInit = "KHO-DN",
                    StorageDate = DateTime.Today.AddDays(-20),
                    ApprovedDate2 = DateTime.Today.AddDays(-5),
                    DeliveryOutDate = null, // Chưa xuất kho
                    DealerCode = "DLR-HYUNDAI-SONTRA",
                    DealerName = "Hyundai Sơn Trà Đà Nẵng",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 12),
                    CostStorageMonth = 12,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 300_000, // 12 * 25k
                    TotalAmount = 350_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Bạt phủ che nắng vùng miền Trung"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps3.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600302",
                    CarId = "CAR-VNU-6008",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Đặc Biệt",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "A-SUV phong cách trẻ trung thể thao hiện đại",
                    ColorExtNameVN = "Đỏ Mận",
                    StorageCodeInit = "KHO-DN",
                    StorageDate = DateTime.Today.AddDays(-18),
                    ApprovedDate2 = DateTime.Today.AddDays(-4),
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-QUANGNAM",
                    DealerName = "Hyundai Quảng Nam",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 12),
                    CostStorageMonth = 12,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 300_000,
                    TotalAmount = 350_000,
                    Status = PaymentStorageDetailStatus.Approved,
                    Remark = "Lưu kho an toàn có camera giám sát 24/7"
                }
            );
            await db.SaveChangesAsync();

            // 4. Bảng kê Tháng 05/2025 Đợt 3 - MỚI TẠO NHÁP (Draft)
            // Tổng kho Bình Dương (KHO-BD), 2 xe điện thuần IONIQ 5 và SUV Tucson
            var ps4 = new PaymentStorage
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentStorageNo = "LK-202505-003",
                PmtMonth = "2025-05",
                StorageOperatorCode = "KHO-BD",
                StorageOperatorName = "Tổng kho Lưu chuyển Ô tô Hyundai Miền Nam - Bình Dương",
                TotalVehicles = 2,
                TotalCoatCost = 100_000,       // 2 * 50k
                TotalStorageCost = 450_000,     // (9 + 9 ngày) * 25k = 18 * 25k = 450,000
                TotalAmount = 550_000,
                VATRate = 10.0m,
                UnitPriceVAT = 55_000,
                AmountTotal = 605_000,
                Status = PaymentStorageStatus.Draft,
                TCMSSignStatus = StorageSignCAStatus.Pending,
                HTVSignStatus = StorageSignCAStatus.Pending,
                Remark = "Đợt 3 tháng 05/2025 kho Bình Dương đang kiểm đếm bạt che phủ và hạn xuất kho",
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
                    SpecDescription = "Xe thuần điện Pin 72.6 kWh sạc siêu nhanh 800V",
                    ColorExtNameVN = "Xám Xi Măng",
                    StorageCodeInit = "KHO-BD",
                    StorageDate = DateTime.Today.AddDays(-10),
                    ApprovedDate2 = null, // Chưa duyệt LXX
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-BINHDUONG",
                    DealerName = "Hyundai Bình Dương",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 9),
                    CostStorageMonth = 9,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 225_000, // 9 * 25k
                    TotalAmount = 275_000,
                    Status = PaymentStorageDetailStatus.Pending,
                    Remark = "Khoang lưu giữ xe điện cách ly nhiệt độ và kiểm tra điện áp pin"
                },
                new PaymentStorageDetail
                {
                    PaymentStorageId = ps4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentStorageNo = ps4.PaymentStorageNo,
                    VIN = "KMHCT81EPHU600402",
                    CarId = "CAR-TUC-6010",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Đặc Biệt",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover 5 chỗ máy dầu tiết kiệm nhiên liệu hộp số 8 cấp",
                    ColorExtNameVN = "Nâu Vàng Ánh Kim",
                    StorageCodeInit = "KHO-BD",
                    StorageDate = DateTime.Today.AddDays(-12),
                    ApprovedDate2 = null,
                    DeliveryOutDate = null,
                    DealerCode = "DLR-HYUNDAI-MIENTAY",
                    DealerName = "Hyundai Miền Tây Sài Gòn",
                    InCostStorageDate = new DateTime(2025, 5, 1),
                    OutCostStorageDate = new DateTime(2025, 5, 9),
                    CostStorageMonth = 9,
                    LevelStorage = 15,
                    DailyStorageRate = 25_000,
                    CostCoat = 50_000,
                    CostStorage = 225_000,
                    TotalAmount = 275_000,
                    Status = PaymentStorageDetailStatus.Pending,
                    Remark = "Xe mới nhập kho đang lắp bạt phủ chống bụi"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 12. Seed Dữ liệu mẫu Công văn Đề nghị Gia hạn Thư Bảo lãnh Ngân hàng (Pmt_GrtClaimExt) =====
        if (!await db.GuaranteeExtensionDispatches.AnyAsync(d => d.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Công văn ĐÃ ĐƯỢC NGÂN HÀNG CHẤP THUẬN GIA HẠN (BankAccepted) - VPBank & Hyundai Hà Đông
            var ged1 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202504-001",
                DealerCode = "DLR-HYUNDAI-HADONG",
                DealerName = "Công ty CP Ô tô Hyundai Hà Đông",
                BankCode = "VPB",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
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
                Remark = "Đề nghị gia hạn 30 ngày do thủ tục đăng ký xe theo hợp đồng dự án của đại lý bị chậm",
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
                    SpecDescription = "SUV 7 chỗ máy xăng Turbo dẫn động HTRAC",
                    ColorName = "Đen Sang Trọng",
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
                    Remark = "Đã giao xe cho đại lý, đang hoàn thiện đăng ký lưu hành"
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
                    SpecDescription = "Crossover 5 chỗ Turbo HTRAC",
                    ColorName = "Trắng Tinh Khôi",
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
                    Remark = "Đã giao xe trưng bày showroom"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRT-7003",
                    VIN = "KMHCT81EPHU700103",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5L Cao Cấp",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "SUV đô thị gói an toàn SmartSense",
                    ColorName = "Đỏ Mận",
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
                    Remark = "Chờ bàn giao chứng từ gốc xe tại ngân hàng"
                }
            );
            await db.SaveChangesAsync();

            // 2. Công văn ĐÃ KÝ SỐ CA PHÁT HÀNH (Signed) - Đang gửi VietinBank chờ phúc đáp
            var ged2 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-001",
                DealerCode = "DLR-HYUNDAI-PHAMVANVO",
                DealerName = "Công ty CP Hyundai Phạm Văn Đồng",
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
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
                Remark = "Đề nghị VietinBank gia hạn bảo lãnh theo gói tín dụng liên kết phân phối ô tô HTV",
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
                    SpecDescription = "SUV Flagship 7 chỗ Diesel HTRAC",
                    ColorName = "Xanh Bóng Đêm",
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
                    Remark = "Xe đang lưu kho Đông Anh chờ xuất trình hồ sơ"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CUS-7005",
                    VIN = "KMHCT81EPHU700202",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CST-2.0T-PRE",
                    SpecDescription = "MPV cao cấp cửa lùa điện thông minh",
                    ColorName = "Trắng Tuyết",
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
                    Remark = "Chờ đại lý tất toán đợt thanh toán trước"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-ACC-7006",
                    VIN = "KMHCT81EPHU700203",
                    ModelCode = "ACCENT-AT-DB",
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5L-DB",
                    SpecDescription = "Sedan phân khúc B Smartstream",
                    ColorName = "Bạc Ánh Kim",
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
                    Remark = "Đã giao xe về đại lý"
                }
            );
            await db.SaveChangesAsync();

            // 3. Công văn MỚI TẠO DỰ THẢO (Draft) - Techcombank & Hyundai An Phú Sài Gòn
            var ged3 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-002",
                DealerCode = "DLR-HYUNDAI-ANPHU",
                DealerName = "Hyundai An Phú Sài Gòn",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
                BankCodeMonitor = "TCB_HO",
                FlagIsHTC = "1",
                NumberOfDaysExt = 15,
                TotalCarCount = 2,
                TotalAmount = 2_100_000_000,
                TotalCarsNotDelivered = 2,
                TotalCarsDelivered = 0,
                Status = GuaranteeExtensionStatus.Draft,
                SignCAStatus = ExtensionSignCAStatus.Pending,
                Remark = "Dự thảo công văn xin gia hạn bảo lãnh xe điện IONIQ 5 và SUV Venue",
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
                    SpecDescription = "Xe thuần điện Pin 72.6 kWh sạc 800V siêu nhanh",
                    ColorName = "Xám Xi Măng",
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
                    Remark = "Xe đang chờ vận chuyển vào kho Bình Dương"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-VNU-7008",
                    VIN = "KMHCT81EPHU700302",
                    ModelCode = "VENUE-TURBO",
                    ModelName = "Hyundai Venue 1.0 T-GDi Đặc Biệt",
                    SpecCode = "VNU-1.0T-DB",
                    SpecDescription = "SUV đô thị nhỏ gọn 1.0 Turbo 7DCT",
                    ColorName = "Đỏ Mận",
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
                    Remark = "Chờ hoàn thiện thủ tục ký duyệt công văn"
                }
            );
            await db.SaveChangesAsync();

            // 4. Công văn NGÂN HÀNG TỪ CHỐI GIA HẠN (BankRejected) - MBBank & Hyundai Cẩm Lệ Đà Nẵng
            var ged4 = new GuaranteeExtensionDispatch
            {
                OrgId = TenantContext.DefaultOrgId,
                DispatchNo = "CVGH-202505-003",
                DealerCode = "DLR-HYUNDAI-DANANG",
                DealerName = "Hyundai Cẩm Lệ Đà Nẵng",
                BankCode = "MBB",
                BankName = "Ngân hàng TMCP Quân Đội (MBBank)",
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
                BankRejectReason = "Đại lý đã vượt hạn mức cấp tín dụng xoay vòng quý 2/2025 theo Hợp đồng hạn mức số MB-HDTD-2024/099",
                FilePath = "/documents/extension/CVGH-202505-003-Signed.pdf",
                Remark = "Đề nghị gia hạn 45 ngày nhưng bị ngân hàng từ chối do chạm trần hạn mức tín dụng",
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
                    ModelName = "Hyundai Stargazer X Cao Cấp",
                    SpecCode = "SGZ-1.5L-PRE",
                    SpecDescription = "Crossover MPV 7 chỗ gầm cao",
                    ColorName = "Trắng Mờ",
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
                    Remark = "Ngân hàng yêu cầu đại lý thanh toán dứt điểm hoặc bổ sung tài sản bảo đảm"
                },
                new GuaranteeExtensionDetail
                {
                    DispatchId = ged4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-7010",
                    VIN = "KMHCT81EPHU700402",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Đặc Biệt",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover 5 chỗ máy dầu tiết kiệm nhiên liệu",
                    ColorName = "Nâu Vàng Ánh Kim",
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
                    Remark = "Chuyển phương án yêu cầu thanh toán chuyển khoản UNC trực tiếp"
                }
            );
            await db.SaveChangesAsync();
        }

        // ===== 13. Seed Dữ liệu mẫu Công văn Đòi Tiền Bảo Lãnh Ngân Hàng (Pmt_GrtClaim / FrmMngGrtClaim) =====
        if (!await db.GuaranteeClaims.AnyAsync(c => c.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Công văn ĐÃ THU HỒI BỒI HOÀN THÀNH CÔNG (Settled) - Vietcombank & Hyundai Hà Đông
            var gcd1 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-001",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                BankCode = "VCB",
                BankName = "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
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
                Remark = "Đại lý quá hạn thanh toán tiền xe 26 ngày theo Hợp đồng số HD-2025-0101. Vietcombank đã trích nợ bồi hoàn thành công 100% nghĩa vụ bảo lãnh.",
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
                    ModelName = "Hyundai Santa Fe 2.2 Diesel Cao Cấp",
                    SpecCode = "SAN-2.2D-PRE",
                    SpecDescription = "SUV 7 chỗ máy dầu thế hệ mới",
                    ColorName = "Đen Ánh Kim",
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
                    Remark = "Vietcombank đã trích nợ tài khoản phong tỏa chi trả bồi hoàn cho HTC"
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
                    SpecDescription = "Crossover 5 chỗ tăng áp",
                    ColorName = "Trắng Tinh Khôi",
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
                    Remark = "Vietcombank đã trích nợ tài khoản phong tỏa chi trả bồi hoàn cho HTC"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8003",
                    VIN = "KMHCT81EPHU800103",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5L-PRE",
                    SpecDescription = "B-SUV đô thị đa dụng",
                    ColorName = "Đỏ Mận",
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
                    Remark = "Vietcombank đã trích nợ tài khoản phong tỏa chi trả bồi hoàn cho HTC"
                }
            );
            await db.SaveChangesAsync();

            // 2. Công văn ĐANG GỬI NGÂN HÀNG ĐÒI TIỀN (SentToBank) - Techcombank & Hyundai Phạm Hùng
            var gcd2 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-002",
                DealerCode = "DLR-PHAMHUNG",
                DealerName = "Hyundai Phạm Hùng",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
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
                Remark = "Đại lý quá hạn cam kết thanh toán xe 18 ngày. Techcombank đang tiến hành phong tỏa tài khoản và chuẩn bị trích tiền bồi hoàn.",
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
                    ModelName = "Hyundai Custin 2.0 Turbo Cao Cấp",
                    SpecCode = "CUS-2.0T-PRE",
                    SpecDescription = "MPV 7 chỗ thương gia cửa lùa điện",
                    ColorName = "Trắng Ngọc Trai",
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
                    Remark = "Đang chờ Techcombank thực hiện nghĩa vụ bảo lãnh thanh toán"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8005",
                    VIN = "KMHCT81EPHU800202",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Đặc Biệt",
                    SpecCode = "CRE-1.5L-DB",
                    SpecDescription = "B-SUV đô thị",
                    ColorName = "Xám Titan",
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
                    Remark = "Đang chờ Techcombank thực hiện nghĩa vụ bảo lãnh thanh toán"
                }
            );
            await db.SaveChangesAsync();

            // 3. Công văn ĐÃ TRÌNH THẨM ĐỊNH PHÁP CHẾ & RỦI RO (Submitted) - MBBank & Hyundai Sài Gòn
            var gcd3 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-003",
                DealerCode = "DLR-SAIGON",
                DealerName = "Hyundai Sài Gòn 1S",
                BankCode = "MBB",
                BankName = "Ngân hàng TMCP Quân Đội (MBBank)",
                BankCodeMonitor = "MBB_SAIGON",
                FlagIsHTC = "2",
                TotalCarCount = 2,
                TotalClaimAmount = 880_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Submitted,
                SignCAStatus = ClaimSignCAStatus.Pending,
                Remark = "Đại lý quá hạn cam kết bảo lãnh 14 ngày. Hồ sơ đang được Ban Pháp chế & Rủi ro tín dụng HTV thẩm định trước khi ký số CA.",
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
                    ModelName = "All New Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "Sedan hạng B thế hệ mới",
                    ColorName = "Trắng",
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
                    Remark = "Chờ hoàn thiện thủ tục thẩm định pháp chế để ký số CA"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-SGZ-8007",
                    VIN = "KMHCT81EPHU800302",
                    ModelCode = "STARGAZER-X",
                    ModelName = "Hyundai Stargazer X Tiêu Chuẩn",
                    SpecCode = "SGZ-1.5L-STD",
                    SpecDescription = "MPV 7 chỗ gầm cao",
                    ColorName = "Bạc",
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
                    Remark = "Chờ hoàn thiện thủ tục thẩm định pháp chế để ký số CA"
                }
            );
            await db.SaveChangesAsync();

            // 4. Công văn MỚI TẠO DỰ THẢO (Draft) - VietinBank & Hyundai Đông Anh
            var gcd4 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-004",
                DealerCode = "DLR-DONGANH",
                DealerName = "Hyundai Đông Anh",
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                BankCodeMonitor = "CTG_DONGANH",
                FlagIsHTC = "1",
                TotalCarCount = 2,
                TotalClaimAmount = 2_450_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Draft,
                SignCAStatus = ClaimSignCAStatus.Pending,
                Remark = "Hồ sơ dự thảo đòi nợ xe Palisade & Tucson quá hạn 8 ngày. Đang kiểm tra bổ sung đối chiếu biên bản bàn giao xe theo hối phiếu.",
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
                    ModelName = "Hyundai Palisade 2.2 Diesel Prestige 6 Chỗ",
                    SpecCode = "PAL-2.2D-PRE6",
                    SpecDescription = "SUV đầu bảng sang trọng",
                    ColorName = "Xanh Bóng Đêm",
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
                    Remark = "Hồ sơ dự thảo đang rà soát phụ lục hợp đồng"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-TUC-8009",
                    VIN = "KMHCT81EPHU800402",
                    ModelCode = "TUCSON-2.0D",
                    ModelName = "Hyundai Tucson 2.0 Diesel Đặc Biệt",
                    SpecCode = "TUC-2.0D-DB",
                    SpecDescription = "Crossover máy dầu tiết kiệm",
                    ColorName = "Đỏ Mận",
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
                    Remark = "Hồ sơ dự thảo đang rà soát phụ lục hợp đồng"
                }
            );
            await db.SaveChangesAsync();

            // 5. Công văn ĐÃ HỦY DO ĐẠI LÝ NỘP TIỀN TRỰC TIẾP (Cancelled) - VPBank & Hyundai An Khánh
            var gcd5 = new BankGuaranteeClaim
            {
                OrgId = TenantContext.DefaultOrgId,
                ClaimNo = "CVDBL-202505-005",
                DealerCode = "DLR-ANKHANH",
                DealerName = "Hyundai An Khánh",
                BankCode = "VPB",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                BankCodeMonitor = "VPB_HO",
                FlagIsHTC = "1",
                TotalCarCount = 2,
                TotalClaimAmount = 1_700_000_000,
                SettledAmount = 0,
                Status = GuaranteeClaimStatus.Cancelled,
                SignCAStatus = ClaimSignCAStatus.Pending,
                CancelledAt = DateTime.Today.AddDays(-1),
                CancelReason = "Đại lý đã thanh toán chuyển khoản dứt điểm 100% tiền xe qua UNC Vietcombank nên HTC hủy công văn đòi nợ bảo lãnh.",
                Remark = "Đại lý đã nộp đủ tiền khắc phục vi phạm trước khi gửi công văn chính thức tới VPBank.",
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
                    ModelName = "Hyundai Santa Fe 2.5 Xăng Cao Cấp",
                    SpecCode = "SAN-2.5X-PRE",
                    SpecDescription = "SUV 7 chỗ máy xăng",
                    ColorName = "Trắng Tinh Khôi",
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
                    Remark = "Đại lý nộp tiền trực tiếp vào tài khoản HTC, hủy hồ sơ đòi nợ"
                },
                new BankGuaranteeClaimDetail
                {
                    ClaimId = gcd5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CarId = "CAR-CRE-8011",
                    VIN = "KMHCT81EPHU800502",
                    ModelCode = "CRETA-1.5L",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5L-PRE",
                    SpecDescription = "B-SUV đô thị",
                    ColorName = "Đỏ Mận",
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
                    Remark = "Đại lý nộp tiền trực tiếp vào tài khoản HTC, hủy hồ sơ đòi nợ"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Bảng kê thanh toán chi phí thiết bị AVN trên xe ô tô (Pmt_PaymentAVN & Pmt_PaymentAVNDetail / BizHTC.Payment)
        if (!await db.PaymentAVNs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Bảng kê Tháng 04/2025 - ĐÃ QUYẾT TOÁN CHUYỂN TIỀN UNC VIETCOMBANK (Settled)
            // Cung cấp bởi Mobis Auto Parts VN, 4 xe SantaFe, Tucson, Creta, Accent
            var avn1 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202504-001",
                PmtMonth = "2025-04",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
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
                Remark = "Quyết toán thanh toán chi phí màn hình AVN dẫn đường tích hợp bản đồ Việt Nam kỳ tháng 04/2025",
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
                    ModelName = "Hyundai Santa Fe 2.5 Xăng Cao Cấp",
                    SpecCode = "SF-2.5G-PREM",
                    SpecDescription = "Màn hình cong đôi Panoramic 12.3 inch tích hợp bản đồ vệ tinh",
                    ColorName = "Trắng Ngọc Trai",
                    AVNCode = "AVN-STF-GEN5-PREM",
                    SerialNo = "AVN-SN-STF-990101",
                    UnitPriceAVN = 12_500_000,
                    AVNDate = DateTime.Today.AddDays(-32),
                    InStorageDate = DateTime.Today.AddDays(-28),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Kiểm tra kết nối GPS và Apple CarPlay/Android Auto không dây hoạt động tốt"
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
                    SpecDescription = "Màn hình giải trí trung tâm 10.25 inch cảm ứng đa điểm",
                    ColorName = "Đen Ánh Kim",
                    AVNCode = "AVN-TUC-GEN5-1025",
                    SerialNo = "AVN-SN-TUC-882302",
                    UnitPriceAVN = 11_000_000,
                    AVNDate = DateTime.Today.AddDays(-30),
                    InStorageDate = DateTime.Today.AddDays(-27),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Cài đặt bản đồ định vị Việt Nam V9 mới nhất"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900103",
                    EngineNo = "G4FL-773403",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRT-1.5L-PREM",
                    SpecDescription = "Màn hình 10.25 inch âm thanh 8 loa Bose",
                    ColorName = "Đỏ Mận",
                    AVNCode = "AVN-CRT-1025-NAV",
                    SerialNo = "AVN-SN-CRT-773403",
                    UnitPriceAVN = 9_500_000,
                    AVNDate = DateTime.Today.AddDays(-29),
                    InStorageDate = DateTime.Today.AddDays(-25),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Âm thanh Bose và camera lùi hiển thị sắc nét"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn1.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900104",
                    EngineNo = "G4LC-664504",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5AT-DB",
                    SpecDescription = "Màn hình liền khối 8 inch hỗ trợ định vị bản đồ tiếng Việt",
                    ColorName = "Bạc",
                    AVNCode = "AVN-ACC-GEN5-8IN",
                    SerialNo = "AVN-SN-ACC-664504",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-28),
                    InStorageDate = DateTime.Today.AddDays(-24),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Lắp đặt đạt chuẩn QA xưởng lắp ráp hoàn thiện HTMV"
                }
            );
            await db.SaveChangesAsync();

            // 2. Bảng kê Tháng 05/2025 - ĐÃ KÝ SỐ CA HOÀN TẤT (Signed)
            // Đối tác Motrex Auto Electronics, 3 xe cao cấp Palisade, Custin, Ioniq 5
            var avn2 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-001",
                PmtMonth = "2025-05",
                SupplierCode = "MOTREX-VN",
                SupplierName = "Công ty TNHH Điện tử Motrex Việt Nam",
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
                Remark = "Bảng kê màn hình giải trí thế hệ mới cho các mẫu xe SUV đầu bảng và xe điện",
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
                    SpecDescription = "Màn hình giải trí kép 12.3 inch âm thanh vòm Infinity 12 loa",
                    ColorName = "Xanh Bóng Đêm",
                    AVNCode = "AVN-PAL-DUAL-12.3IN",
                    SerialNo = "AVN-SN-PAL-110201",
                    UnitPriceAVN = 16_500_000,
                    AVNDate = DateTime.Today.AddDays(-12),
                    InStorageDate = DateTime.Today.AddDays(-9),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Hệ thống hiển thị camera 360 toàn cảnh SVM và giám sát điểm mù BVM"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn2.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900202",
                    EngineNo = "G4NN-220202",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CST-2.0T-PREM",
                    SpecDescription = "Màn hình dọc 10.4 inch giao diện tiếng Việt điều khiển tiện nghi hàng ghế 2",
                    ColorName = "Trắng",
                    AVNCode = "AVN-CST-PORTRAIT-104",
                    SerialNo = "AVN-SN-CST-220202",
                    UnitPriceAVN = 14_000_000,
                    AVNDate = DateTime.Today.AddDays(-11),
                    InStorageDate = DateTime.Today.AddDays(-8),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Điều khiển điều hòa và chế độ ghế thương gia tích hợp màn hình"
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
                    SpecDescription = "Màn hình đôi thông tin & giải trí kép 12.3 inch hiển thị trạng thái sạc Pin EV",
                    ColorName = "Xám Xi Măng",
                    AVNCode = "AVN-IQ5-EV-DUAL-12.3",
                    SerialNo = "AVN-SN-IQ5-330203",
                    UnitPriceAVN = 18_000_000,
                    AVNDate = DateTime.Today.AddDays(-10),
                    InStorageDate = DateTime.Today.AddDays(-7),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Định vị bản đồ trạm sạc điện toàn quốc và tối ưu hành trình EV"
                }
            );
            await db.SaveChangesAsync();

            // 3. Bảng kê Tháng 05/2025 - HTV ĐÃ PHÊ DUYỆT CẤP 2 (HTVApproved)
            // Đã được TCMS ký CA, đang chờ lãnh đạo HTV ký số hoàn tất
            var avn3 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-002",
                PmtMonth = "2025-05",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
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
                Remark = "Bảng kê đợt 2 tháng 05/2025: Lô xe Elantra & Stargazer đã kiểm định PDI đạt chuẩn",
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
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cấp",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "Màn hình thông tin giải trí 10.25 inch liền khối",
                    ColorName = "Xanh Dương",
                    AVNCode = "AVN-ELN-1025-WIDESCREEN",
                    SerialNo = "AVN-SN-ELN-440301",
                    UnitPriceAVN = 9_800_000,
                    AVNDate = DateTime.Today.AddDays(-7),
                    InStorageDate = DateTime.Today.AddDays(-5),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Kết nối Bluetooth đa thiết bị và nhận diện giọng nói"
                },
                new PaymentAVNDetail
                {
                    PaymentAVNId = avn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    PaymentAVNNo = avn3.PaymentAVNNo,
                    VIN = "KMHCT81EPHU900302",
                    EngineNo = "G4FL-550302",
                    ModelCode = "STARGAZER",
                    ModelName = "Hyundai Stargazer X Cao Cấp",
                    SpecCode = "SGZ-1.5L-PREM",
                    SpecDescription = "Màn hình cảm ứng 10.25 inch âm thanh 6 loa",
                    ColorName = "Trắng Mờ",
                    AVNCode = "AVN-SGZ-1025-NAV",
                    SerialNo = "AVN-SN-SGZ-550302",
                    UnitPriceAVN = 8_800_000,
                    AVNDate = DateTime.Today.AddDays(-6),
                    InStorageDate = DateTime.Today.AddDays(-4),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Giao diện thân thiện hỗ trợ kiểm soát sạc không dây Qi"
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
                    SpecDescription = "Màn hình thể thao giao diện N-Mode đo lường lực G và gia tốc",
                    ColorName = "Đỏ N Line",
                    AVNCode = "AVN-ELN-NLINE-SPORT",
                    SerialNo = "AVN-SN-ELN-660303",
                    UnitPriceAVN = 10_500_000,
                    AVNDate = DateTime.Today.AddDays(-5),
                    InStorageDate = DateTime.Today.AddDays(-3),
                    Status = PaymentAVNDetailStatus.Approved,
                    Remark = "Đo lường thời gian vòng chạy Lap timer và áp suất lốp TPMS tích hợp"
                }
            );
            await db.SaveChangesAsync();

            // 4. Bảng kê Tháng 05/2025 - MỚI TẠO DỰ THẢO (Draft)
            // Lô xe mới lắp ráp xong tại phân xưởng Ninh Bình, 2 xe SantaFe Calligraphy & Tucson Turbo
            var avn4 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202505-003",
                PmtMonth = "2025-05",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                TotalVehicles = 2,
                TotalAmount = 25_500_000,          // 14.0M (SantaFe Calligraphy) + 11.5M (Tucson Turbo) = 25,500,000
                VATRate = 10.0m,
                AmountVAT = 2_550_000,
                TotalAmountAfterVAT = 28_050_000,
                Status = PaymentAVNStatus.Draft,
                TCMSSignStatus = AVNSignCAStatus.Pending,
                HTVSignStatus = AVNSignCAStatus.Pending,
                Remark = "Dự thảo bảng kê thanh toán màn hình AVN lô xuất xưởng tuần thứ 3 tháng 05/2025",
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
                    SpecDescription = "Màn hình cong liền khối 12.3 inch thế hệ ccNC mới nhất",
                    ColorName = "Đồng Ánh Kim",
                    AVNCode = "AVN-STF-CAL-12.3IN",
                    SerialNo = "AVN-SN-STF-770401",
                    UnitPriceAVN = 14_000_000,
                    AVNDate = DateTime.Today.AddDays(-2),
                    InStorageDate = DateTime.Today.AddDays(-1),
                    Status = PaymentAVNDetailStatus.Pending,
                    Remark = "Cập nhật phần mềm hệ điều hành ccNC hỗ trợ kết nối không dây kép"
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
                    SpecDescription = "Màn hình đôi 12.3 inch tích hợp điều khiển cảm ứng",
                    ColorName = "Trắng",
                    AVNCode = "AVN-TUC-TURBO-1025",
                    SerialNo = "AVN-SN-TUC-880402",
                    UnitPriceAVN = 11_500_000,
                    AVNDate = DateTime.Today.AddDays(-2),
                    InStorageDate = DateTime.Today.AddDays(-1),
                    Status = PaymentAVNDetailStatus.Pending,
                    Remark = "Hệ thống bản đồ 3D offline hỗ trợ cảnh báo tốc độ"
                }
            );
            await db.SaveChangesAsync();

            // 5. Bảng kê Tháng 03/2025 - ĐÃ HỦY (Cancelled)
            // Hủy do đối tác giao nhầm lô màn hình mã cũ, đã lập bảng kê thay thế
            var avn5 = new PaymentAVN
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentAVNNo = "AVN-202503-099",
                PmtMonth = "2025-03",
                SupplierCode = "VIETTEL-AVN",
                SupplierName = "Tổng Công ty Giải pháp Doanh nghiệp Viettel (VTS)",
                TotalVehicles = 2,
                TotalAmount = 17_000_000,
                VATRate = 10.0m,
                AmountVAT = 1_700_000,
                TotalAmountAfterVAT = 18_700_000,
                Status = PaymentAVNStatus.Cancelled,
                TCMSSignStatus = AVNSignCAStatus.Pending,
                HTVSignStatus = AVNSignCAStatus.Pending,
                CancelledAt = DateTime.Today.AddDays(-40),
                RejectReason = "Hủy bảng kê do đối tác cấp sai lô mã linh kiện AVN-ACC-GEN4 thay vì GEN5. Đã lập bảng kê bù trong tháng 04.",
                Remark = "Bảng kê bị hủy trước khi trình duyệt cấp 1",
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
                    SpecDescription = "Màn hình AVN mẫu cũ",
                    ColorName = "Đen",
                    AVNCode = "AVN-ACC-GEN4-7IN",
                    SerialNo = "AVN-SN-OLD-990501",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-44),
                    InStorageDate = DateTime.Today.AddDays(-42),
                    Status = PaymentAVNDetailStatus.Cancelled,
                    Remark = "Hủy dòng xe do không khớp chứng từ nhập linh kiện"
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
                    SpecDescription = "Màn hình AVN mẫu cũ",
                    ColorName = "Trắng",
                    AVNCode = "AVN-ACC-GEN4-7IN",
                    SerialNo = "AVN-SN-OLD-990502",
                    UnitPriceAVN = 8_500_000,
                    AVNDate = DateTime.Today.AddDays(-44),
                    InStorageDate = DateTime.Today.AddDays(-42),
                    Status = PaymentAVNDetailStatus.Cancelled,
                    Remark = "Hủy dòng xe do không khớp chứng từ nhập linh kiện"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Bảng đơn giá định mức thuê bao dịch vụ GPS (Mst_UnitPriceGPS / BizHTC.Payment)
        if (!await db.UnitPriceGPSs.AnyAsync(u => u.OrgId == TenantContext.DefaultOrgId))
        {
            db.UnitPriceGPSs.AddRange(
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-VIETTEL-2024",
                    ProviderCode = "VIETTEL",
                    ProviderName = "Tổng Công ty Viễn thông Viettel (Viettel Telecom)",
                    DailyPrice = 2_500,
                    MonthlyRate = 75_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Hợp đồng khung cung cấp thiết bị và dịch vụ định vị GPS Viettel 4G toàn quốc"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-VNPT-TRACK",
                    ProviderCode = "VNPT",
                    ProviderName = "Tổng Công ty Dịch vụ Viễn thông VNPT (VNPT VinaPhone)",
                    DailyPrice = 2_400,
                    MonthlyRate = 72_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Hợp đồng thuê bao định vị vệ tinh VNPT Smart Motor & Fleet"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-BINHANH",
                    ProviderCode = "BAGPS",
                    ProviderName = "Công ty TNHH Phát triển Công nghệ Điện tử Bình Anh (BA GPS)",
                    DailyPrice = 2_300,
                    MonthlyRate = 69_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Hợp đồng thiết bị giám sát hành trình hợp chuẩn BA-GPS"
                },
                new UnitPriceGPS
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ContractNo = "HD-GPS-BKAV-SMART",
                    ProviderCode = "BKAV",
                    ProviderName = "Tập đoàn Công nghệ BKAV - Smart Vehicle IoT Solutions",
                    DailyPrice = 2_600,
                    MonthlyRate = 78_000,
                    EffectiveStartDate = new DateTime(2024, 1, 1),
                    IsActive = true,
                    Remark = "Hợp đồng giám sát thông minh IoT và an ninh dữ liệu vị trí xe BKAV"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Bảng kê thanh toán chi phí định vị GPS trên xe ô tô (Pmt_PaymentGPS & Pmt_PaymentGPSDetail / BizHTC.Payment)
        if (!await db.PaymentGPSs.AnyAsync(p => p.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Bảng kê Tháng 04/2025 - ĐÃ QUYẾT TOÁN THANH TOÁN (Settled)
            var gps1 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202504-001",
                PmtMonth = "2025-04",
                ContractNo = "HD-GPS-VIETTEL-2024",
                ProviderCode = "VIETTEL",
                ProviderName = "Tổng Công ty Viễn thông Viettel (Viettel Telecom)",
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
                Remark = "Quyết toán thanh toán phí dịch vụ định vị GPS Viettel tháng 04/2025 cho lô xe kho Ninh Bình",
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
                    SpecDescription = "Bản xăng cao cấp",
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
                    Remark = "Thiết bị phát sóng định vị GPS 4G chuẩn xác liên tục 24/7"
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
                    SpecDescription = "Bản đặc biệt máy xăng",
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
                    Remark = "Khấu trừ 2 ngày xe vào xưởng hiệu chỉnh ăng-ten ngưng phát sóng"
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
                    SpecDescription = "Bản cao cấp 2 tông màu",
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
                    Remark = "Kích hoạt giám sát định vị từ ngày hoàn tất kiểm tra xuất xưởng PDI"
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
                    SpecDescription = "Bản đặc biệt",
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
                    Remark = "Khấu trừ 1 ngày kiểm tra kỹ thuật định kỳ tại bãi"
                }
            );
            await db.SaveChangesAsync();

            // 2. Bảng kê Tháng 05/2025 - ĐÃ KÝ SỐ CA HOÀN TẤT (Signed)
            var gps2 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-001",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-VNPT-TRACK",
                ProviderCode = "VNPT",
                ProviderName = "Tổng Công ty Dịch vụ Viễn thông VNPT (VNPT VinaPhone)",
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
                Remark = "Bảng kê giám sát GPS xe nhập khẩu phân phối đợt 1 tháng 05/2025 - Chờ kế toán chuyển khoản UNC",
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
                    SpecDescription = "Bản 6 chỗ ngồi thương gia",
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
                    Remark = "Giám sát vị trí lưu kho tại tổng kho Hiệp Phước Nhà Bè"
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
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CST-2.0T-PREM",
                    SpecDescription = "Bản cao cấp cửa lùa điện kép",
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
                    Remark = "Khấu trừ 1 ngày kiểm định môi trường khí thải xe"
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
                    SpecDescription = "Xe điện Pin 72.6 kWh công nghệ E-GMP",
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
                    Remark = "Giám sát trạng thái sạc Pin và vị trí xe an toàn 24/7"
                }
            );
            await db.SaveChangesAsync();

            // 3. Bảng kê Tháng 05/2025 - HTV ĐÃ PHÊ DUYỆT CẤP 1 (HTVApproved)
            var gps3 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-002",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-BINHANH",
                ProviderCode = "BAGPS",
                ProviderName = "Công ty TNHH Phát triển Công nghệ Điện tử Bình Anh (BA GPS)",
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
                Remark = "Bảng kê đợt 2 tháng 05/2025: Thiết bị BA-GPS trên dòng xe sedan & MPV, chờ TCMS duyệt A2",
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
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cấp",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "Bản cao cấp thế hệ 7",
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
                    Remark = "Kết nối hộp đen BA-GPS đạt chuẩn kỹ thuật QCVN 31:2014"
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
                    ModelName = "Hyundai Stargazer X Cao Cấp",
                    SpecCode = "SGZ-1.5L-PREM",
                    SpecDescription = "Bản MPV phong cách SUV",
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
                    Remark = "Truyền dữ liệu tốc độ và định vị máy chủ ổn định"
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
                    SpecDescription = "Bản thể thao Turbo",
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
                    Remark = "Khấu trừ 2 ngày xe trưng bày sự kiện nội bộ không di chuyển"
                }
            );
            await db.SaveChangesAsync();

            // 4. Bảng kê Tháng 05/2025 - DỰ THẢO (Draft)
            var gps4 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202505-003",
                PmtMonth = "2025-05",
                ContractNo = "HD-GPS-VIETTEL-2024",
                ProviderCode = "VIETTEL",
                ProviderName = "Tổng Công ty Viễn thông Viettel (Viettel Telecom)",
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
                Remark = "Dự thảo bảng kê thanh toán GPS đợt 3 tháng 05/2025 - Mới kích hoạt SIM và thiết bị",
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
                    SpecDescription = "Bản cao cấp nhất 6 chỗ mâm 21 inch",
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
                    Remark = "Định vị 4G kết nối hệ thống viễn thông thông minh Bluelink"
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
                    SpecDescription = "Bản máy xăng tăng áp HTRAC",
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
                    Remark = "Cài đặt hàng rào điện tử cảnh báo rời kho tổng Ninh Bình"
                }
            );
            await db.SaveChangesAsync();

            // 5. Bảng kê Tháng 03/2025 - ĐÃ HỦY (Cancelled)
            var gps5 = new PaymentGPS
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentGPSNo = "GPS-202503-099",
                PmtMonth = "2025-03",
                ContractNo = "HD-GPS-BKAV-SMART",
                ProviderCode = "BKAV",
                ProviderName = "Tập đoàn Công nghệ BKAV - Smart Vehicle IoT Solutions",
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
                RejectReason = "Hủy bảng kê do thay đổi phụ lục số 02 điều chỉnh đơn giá sang nhà mạng Viettel",
                Remark = "Bảng kê thử nghiệm thiết bị BKAV bị hủy",
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
                    SpecDescription = "Bản tiêu chuẩn",
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
                    Remark = "Hủy do thay đổi phương án triển khai thiết bị thử nghiệm"
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
                    SpecDescription = "Bản tiêu chuẩn",
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
                    Remark = "Hủy do thay đổi phương án triển khai thiết bị thử nghiệm"
                }
            );
            await db.SaveChangesAsync();
        }

        // 16. Dữ liệu mẫu Bảng tính Chi phí Tài chính (CPTC) & Chiết khấu Thanh toán TCG (CKTT) cho Đại lý (DMS40_FnExp_Calc_FnExp_PmDc)
        if (!await db.FnExpStatements.AnyAsync(s => s.OrgId == TenantContext.DefaultOrgId))
        {
            // Statement 1: ĐÃ QUYẾT TOÁN (Settled) - Hyundai Hà Đông (Kỳ 04/2025)
            var fn1 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202504-HADONG-01",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                CAName = "Bảng tính CPTC & CKTT TCG tháng 04/2025 - Hyundai Hà Đông",
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
                Remark = "Quyết toán hoàn tất chuyển tiền hỗ trợ lãi suất và chiết khấu TCG kỳ 04/2025 qua Vietcombank",
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
                    SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                    ColorName = "Trắng ngọc trai",
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
                    Remark = "Đại lý nộp cọc và thanh toán trước hạn bảo lãnh 10 ngày"
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
                    SpecDescription = "Bản máy xăng tăng áp HTRAC",
                    ColorName = "Đen huyền bí",
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
                    Remark = "Hưởng chiết khấu thanh toán sớm 13 ngày"
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
                    SpecDescription = "Bản nhập khẩu nguyên chiếc CBU",
                    ColorName = "Đen ngọc bích",
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
                    Remark = "Xe CBU áp dụng tỷ lệ cọc 30% và bảo lãnh 70%, chiết khấu sớm 20 ngày"
                }
            );
            await db.SaveChangesAsync();

            // Statement 2: ĐÃ KÝ SỐ CA HTC (HTCSigned) - Hyundai Đông Anh (Kỳ 05/2025)
            var fn2 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-DONGANH-01",
                DealerCode = "DLR-DONGANH",
                DealerName = "Hyundai Đông Anh",
                CAName = "Bảng tính CPTC & CKTT TCG tháng 05/2025 - Hyundai Đông Anh",
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
                Remark = "Đã hoàn tất ký số CA 2 cấp, kế toán chuẩn bị lập UNC thanh toán qua BIDV",
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
                    SpecDescription = "Bản SUV 7 chỗ máy dầu",
                    ColorName = "Xanh lục bảo",
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
                    Remark = "Xe dòng cao cấp Palisade, chiết khấu sớm 9 ngày"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn2.CaNo,
                    CarId = "CAR-FN-05",
                    VIN = "KMHCT81EPHU880105",
                    ModelCode = "CUSTIN-TURBO",
                    ModelName = "Hyundai Custin 2.0T Cao Cấp",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Bản MPV cửa trượt điện 7 chỗ",
                    ColorName = "Trắng tuyết",
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
                    Remark = "Dòng MPV gia đình Custin thế hệ mới"
                }
            );
            await db.SaveChangesAsync();

            // Statement 3: ĐẠI LÝ ĐÃ KÝ SỐ CA (DlrSigned) - Hyundai Sài Gòn 1S (Kỳ 05/2025)
            var fn3 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-SAIGON-01",
                DealerCode = "DLR-SAIGON",
                DealerName = "Hyundai Sài Gòn 1S",
                CAName = "Bảng tính CPTC & CKTT TCG tháng 05/2025 - Hyundai Sài Gòn 1S",
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
                Remark = "Đại lý đã ký số CA xác nhận số liệu, chờ chuyên viên QLPP HTC thẩm định A1",
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
                    SpecDescription = "Bản xe điện thông minh E-GMP",
                    ColorName = "Bạc ánh kim",
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
                    Remark = "Dòng xe thuần điện EV hỗ trợ lãi suất kích cầu xanh"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn3.CaNo,
                    CarId = "CAR-FN-07",
                    VIN = "KMHCT81EPHU880106",
                    ModelCode = "CRETA-PREM",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Bản SUV cỡ B SmartSense",
                    ColorName = "Đỏ mận",
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
                    Remark = "SUV Creta phiên bản cao cấp SmartSense"
                }
            );
            await db.SaveChangesAsync();

            // Statement 4: HTC ĐÃ THẨM ĐỊNH DUYỆT CẤP 1 (HTCApproved1) - Hyundai Phạm Văn Đồng (Kỳ 05/2025)
            var fn4 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-PVD-01",
                DealerCode = "DLR-PHAMVANDONG",
                DealerName = "Hyundai Phạm Văn Đồng",
                CAName = "Bảng tính CPTC & CKTT TCG tháng 05/2025 - Hyundai Phạm Văn Đồng",
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
                Remark = "Chuyên viên HTC đã thẩm định khớp hồ sơ bảo lãnh VietinBank, chờ Lãnh đạo HTC ký số CA",
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
                    ModelName = "Hyundai Elantra 2.0 AT Cao Cấp",
                    SpecCode = "ELN-2.0AT-PREM",
                    SpecDescription = "Bản sedan thể thao phân khúc C",
                    ColorName = "Đỏ đô",
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
                    Remark = "Dòng sedan Elantra thanh toán sớm 8 ngày"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn4.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn4.CaNo,
                    CarId = "CAR-FN-09",
                    VIN = "KMHCT81EPHU880109",
                    ModelCode = "ACCENT-AT",
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5AT-SPEC",
                    SpecDescription = "Bản sedan B bán chạy",
                    ColorName = "Bạc",
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
                    Remark = "Accent thanh toán dứt điểm trước hạn 14 ngày"
                }
            );
            await db.SaveChangesAsync();

            // Statement 5: DỰ THẢO MỚI (Draft) - Hyundai Đà Nẵng (Kỳ 05/2025)
            var fn5 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202505-DANANG-01",
                DealerCode = "DLR-DANANG",
                DealerName = "Hyundai Sông Hàn - Đà Nẵng",
                CAName = "Bảng tính CPTC & CKTT TCG tháng 05/2025 - Hyundai Sông Hàn",
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
                Remark = "Dự thảo bảng tính chi phí tài chính mới lập, đang gửi kế toán đại lý thẩm định",
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
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Bản SUV cỡ B SmartSense",
                    ColorName = "Trắng",
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
                    Remark = "Dự thảo dòng xe Creta"
                },
                new FinancialExpenseDetail
                {
                    StatementId = fn5.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CaNo = fn5.CaNo,
                    CarId = "CAR-FN-11",
                    VIN = "KMHCT81EPHU880111",
                    ModelCode = "ACCENT-STD",
                    ModelName = "Hyundai Accent 1.5 MT Tiêu Chuẩn",
                    SpecCode = "ACC-1.5MT-STD",
                    SpecDescription = "Bản số sàn tiết kiệm",
                    ColorName = "Đen",
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
                    Remark = "Dự thảo dòng xe Accent"
                }
            );
            await db.SaveChangesAsync();

            // Statement 6: ĐÃ HỦY (Cancelled) - Hyundai Hà Đông (Kỳ 03/2025)
            var fn6 = new FinancialExpenseStatement
            {
                OrgId = TenantContext.DefaultOrgId,
                CaNo = "CAN-202503-HADONG-09",
                DealerCode = "DLR-HADONG",
                DealerName = "Hyundai Hà Đông",
                CAName = "Bảng tính CPTC & CKTT tháng 03/2025 (Hủy làm lại)",
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
                CancelReason = "Hủy do đại lý điều chỉnh phụ lục thay đổi ngày nộp cọc đơn hàng SO",
                Remark = "Bảng tính bị hủy làm lại đợt khác",
                CreatedBy = "NguyenQuocTuan_ChuyenVienTaiChinhHTC",
                CreatedAt = new DateTime(2025, 4, 2, 14, 0, 0)
            };
            db.FnExpStatements.Add(fn6);
            await db.SaveChangesAsync();
        }

        // 17. Dữ liệu mẫu Hồ sơ Đề nghị Giao dịch Ngân hàng & Tài trợ Vốn Vay / Bảo lãnh Đại lý (RQ_BankingTransactions)
        if (!await db.DisbursementRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Hồ sơ GNTT VPBank ĐÃ HOÀN TẤT GIẢI NGÂN (Completed / Disbursed) - Hyundai Giải Phóng
            var req1 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-001",
                TransType = BankingTransType.GNTT,
                DealerCode = "HYUNDAI-GP",
                DealerName = "Hyundai Giải Phóng",
                BizResNumber = "0102839102-001",
                BankCode = "VPBANK",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                PaymentAccount = "108899882991",
                PaymentBankName = "VPBank - CN Thăng Long",
                ReceivingUnit = "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Đống Đa",
                TotalCars = 9,
                TotalContractAmount = 10_781_000_000,
                TotalDisbursementAmount = 8_822_600_000,
                ActualDisbursedAmount = 8_822_600_000,
                Status = BankingTransStatus.Completed,
                BankStatus = BankingTransBankStatus.Disbursed,
                RefBankCode = "VPB-LN-20250510-001",
                BankRemark = "Hội đồng tín dụng VPBank đã phê duyệt và giải ngân thành công 8.822.600.000 VND theo Khế ước nhận nợ LD-VPB-20250510-001.",
                LDNo = "LD-VPB-20250510-001",
                DisbursementDate = DateTime.Today.AddDays(-5),
                DisbursementTerm = "03 tháng",
                DisbursementInterestRate = 8.5m,
                FirstInterestPmtDate = DateTime.Today.AddDays(25),
                LoanLimit = 20_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-7),
                CompletedAt = DateTime.Today.AddDays(-5),
                Remark = "Đề nghị giải ngân vốn lưu động mua xe lô tháng 05/2025 theo HĐNT-2025/HTC-GP",
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
                    SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
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
                    Remark = "Lô 5 xe Santa Fe giao tuần 2 tháng 5"
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
                    SpecDescription = "Bản máy xăng tăng áp HTRAC",
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
                    Remark = "Lô 4 xe Tucson Turbo phục vụ trưng bày & bán lẻ"
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

            // 2. Hồ sơ Phát hành Thư bảo lãnh mở L/C (PhatHanhBLLC) VietinBank ĐANG CHỜ KÝ SỐ CA (Processing / RequireSignCA) - Hyundai Thanh Xuân
            var req2 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-002",
                TransType = BankingTransType.PhatHanhBLLC,
                DealerCode = "HYUNDAI-TX",
                DealerName = "Hyundai Thanh Xuân",
                BizResNumber = "0103774819-002",
                BankCode = "VIETINBANK",
                BankName = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                PaymentAccount = "118000992817",
                PaymentBankName = "VietinBank - CN Thanh Xuân",
                ReceivingUnit = "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Đống Đa",
                TotalCars = 5,
                TotalContractAmount = 7_528_000_000,
                TotalDisbursementAmount = 5_804_900_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Processing,
                BankStatus = BankingTransBankStatus.RequireSignCA,
                RefBankCode = "CTG-LN-20250512-002",
                BankRemark = "Hồ sơ tín dụng đã được phê duyệt sơ bộ. Đề nghị đại lý ký số CA trên Thư cam kết trả nợ để Hội sở phát hành thư bảo lãnh.",
                DisbursementTerm = "45 ngày",
                DisbursementInterestRate = 0m,
                GrtTerm = "45 ngày",
                GrtFee = 11_609_800,
                LoanLimit = 15_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-3),
                Remark = "Đề nghị phát hành Thư bảo lãnh thanh toán mở L/C mua lô xe điện Ioniq 5 và Palisade",
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
                    SpecDescription = "Bản xe điện thông minh E-GMP",
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
                    Remark = "3 xe điện Ioniq 5 cao cấp"
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
                    SpecDescription = "Bản SUV cỡ lớn máy dầu 7 chỗ",
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

            // 3. Hồ sơ GNTT VIB ĐANG THẨM ĐỊNH (Processing / Reviewing) - Hyundai Phạm Văn Đồng
            var req3 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-003",
                TransType = BankingTransType.GNTT,
                DealerCode = "HYUNDAI-PVD",
                DealerName = "Hyundai Phạm Văn Đồng",
                BizResNumber = "0104992817-003",
                BankCode = "VIB",
                BankName = "Ngân hàng TMCP Quốc tế Việt Nam (VIB)",
                PaymentAccount = "025704068899",
                PaymentBankName = "VIB - CN Cầu Giấy",
                ReceivingUnit = "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Đống Đa",
                TotalCars = 6,
                TotalContractAmount = 5_844_000_000,
                TotalDisbursementAmount = 4_675_200_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Processing,
                BankStatus = BankingTransBankStatus.Reviewing,
                RefBankCode = "VIB-LN-20250514-003",
                BankRemark = "Cán bộ tín dụng VIB đang thẩm định thực địa kho bãi và đối chiếu hạn mức bảo đảm theo hợp đồng nguyên tắc.",
                DisbursementTerm = "03 tháng",
                DisbursementInterestRate = 8.8m,
                LoanLimit = 12_000_000_000,
                SentToBankAt = DateTime.Today.AddDays(-2),
                Remark = "Đề nghị giải ngân vốn vay mua lô 6 xe MPV Custin",
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
                    ModelName = "Hyundai Custin 2.0T Cao Cấp",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Bản MPV cửa trượt điện 7 chỗ",
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
                    Remark = "6 xe Custin 2.0T màu trắng tuyết"
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

            // 4. Hồ sơ DỰ THẢO MỚI (Draft / Pending) - Techcombank & Hyundai Sài Gòn
            var req4 = new BankingDisbursementRequest
            {
                OrgId = TenantContext.DefaultOrgId,
                TransNo = "DNTT-202505-004",
                TransType = BankingTransType.GNTTLC,
                DealerCode = "HYUNDAI-SG",
                DealerName = "Hyundai Sài Gòn 1S",
                BizResNumber = "0309118274-001",
                BankCode = "TECHCOMBANK",
                BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
                PaymentAccount = "19028819283011",
                PaymentBankName = "Techcombank - CN Sài Gòn",
                ReceivingUnit = "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM",
                ReceivingAccount = "113000088999",
                ReceivingBank = "VietinBank - CN Đống Đa",
                TotalCars = 4,
                TotalContractAmount = 2_796_000_000,
                TotalDisbursementAmount = 2_236_800_000,
                ActualDisbursedAmount = 0,
                Status = BankingTransStatus.Draft,
                BankStatus = BankingTransBankStatus.Pending,
                DisbursementTerm = "03 tháng",
                DisbursementInterestRate = 8.2m,
                LoanLimit = 18_000_000_000,
                Remark = "Dự thảo đề nghị tài trợ giải ngân theo L/C cho 4 xe Creta Cao Cấp",
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
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5L-PREM",
                    SpecDescription = "Bản SUV cỡ B SmartSense",
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
                    Remark = "4 xe Creta SUV B phân phối thị trường phía Nam"
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

        // Dữ liệu mẫu Hồ sơ Đề nghị Hủy Gán Ngân Hàng Bảo Lãnh Hợp Đồng Xe Ô Tô (DMS40_DlrCtr_CancelBankMD / BizHTC.Payment)
        if (!await db.CancelBankMDRequests.AnyAsync(r => r.OrgId == TenantContext.DefaultOrgId))
        {
            // 1. Hồ sơ 01: HTC ĐÃ DUYỆT HOÀN TẤT (Finished) - Hyundai Thanh Xuân đổi bảo lãnh VPBank sang VietinBank
            var can1 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-001",
                DlrCtrNo = "PLHD-2025-05/TX01",
                DealerCode = "HYUNDAI-TX",
                DealerName = "Hyundai Thanh Xuân",
                BankCodeMD = "VPB",
                BankNameMD = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                NewBankCodeMD = "CTG",
                NewBankNameMD = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 3_178_000_000,
                GuaranteeAmount = 2_700_000_000,
                ReasonType = CancelBankMDReasonType.ChangeBank,
                ReasonDescription = "Đại lý chuyển sang VietinBank tài trợ do được cấp hạn mức tín dụng và lãi suất ưu đãi tốt hơn",
                Status = CancelBankMDStatus.Finished,
                TotalVehicles = 2,
                RemarkDlr = "Kính đề nghị Ban Lãnh đạo HTC và Ngân hàng VPBank chấp thuận hủy cam kết bảo lãnh cũ để gán VietinBank",
                RemarkBank = "VPBank xác nhận chưa phát hành thư bảo lãnh chính thức và đồng ý giải phóng nghĩa vụ bảo lãnh cho hợp đồng PLHD-2025-05/TX01",
                RemarkHTC = "HTC phê duyệt hoàn tất hủy gán bảo lãnh VPBank. Đã cập nhật BankCodeMD = NULL và cho phép đại lý đăng ký bảo lãnh VietinBank",
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
                    SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                    ColorExtNameVN = "Trắng ngọc trai",
                    EngineNo = "ENG-SF-2025-01",
                    UnitPrice = 1_589_000_000,
                    GuaranteeAmount = 1_350_000_000,
                    Status = CancelBankMDDetailStatus.Finished,
                    Remark = "Đã hoàn tất hủy gán bảo lãnh VPBank"
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
                    SpecDescription = "Bản cao cấp 6 chỗ HTRAC",
                    ColorExtNameVN = "Đen Midnight",
                    EngineNo = "ENG-SF-2025-02",
                    UnitPrice = 1_589_000_000,
                    GuaranteeAmount = 1_350_000_000,
                    Status = CancelBankMDDetailStatus.Finished,
                    Remark = "Đã hoàn tất hủy gán bảo lãnh VPBank"
                }
            );

            // 2. Hồ sơ 02: NGÂN HÀNG ĐÃ DUYỆT (Approved) - Hyundai Hà Đông chuyển sang thanh toán vốn tự có
            var can2 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-002",
                DlrCtrNo = "PLHD-2025-05/HD02",
                DealerCode = "HYUNDAI-HD",
                DealerName = "Hyundai Hà Đông",
                BankCodeMD = "CTG",
                BankNameMD = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 2_447_000_000,
                GuaranteeAmount = 2_000_000_000,
                ReasonType = CancelBankMDReasonType.SwitchToOwnCapital,
                ReasonDescription = "Đại lý chuyển đổi sang thanh toán bằng vốn tự có qua UNC ngân hàng để tiết kiệm chi phí phí phát hành bảo lãnh",
                Status = CancelBankMDStatus.Approved,
                TotalVehicles = 3,
                RemarkDlr = "Đại lý đã chuẩn bị đủ nguồn tiền vốn tự có tại tài khoản BIDV, đề nghị hủy bảo lãnh VietinBank",
                RemarkBank = "VietinBank Chi nhánh Hà Tây chấp thuận hủy chỉ định bảo lãnh cho hợp đồng PLHD-2025-05/HD02 theo đề nghị của khách hàng",
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
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRT-1.5L-PRE",
                    SpecDescription = "Bản SUV đô thị SmartSense",
                    ColorExtNameVN = "Đỏ Mận",
                    EngineNo = "ENG-CR-2025-11",
                    UnitPrice = 699_000_000,
                    GuaranteeAmount = 570_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "Ngân hàng VietinBank đã duyệt chấp thuận hủy"
                },
                new ContractBankMDCancelDetail
                {
                    CancelBankMDId = can2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    CancelBankMDNo = can2.CancelBankMDNo,
                    VIN = "KMHCT81EPHU770202",
                    CarId = "CAR-CST-0202",
                    ModelCode = "CUSTIN-2.0T",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CUS-2.0T-PREM",
                    SpecDescription = "Bản MPV cửa lùa điện kép",
                    ColorExtNameVN = "Trắng",
                    EngineNo = "ENG-CS-2025-22",
                    UnitPrice = 974_000_000,
                    GuaranteeAmount = 800_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "Ngân hàng VietinBank đã duyệt chấp thuận hủy"
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
                    SpecDescription = "Bản máy xăng tăng áp thể thao",
                    ColorExtNameVN = "Xanh Dương",
                    EngineNo = "ENG-TC-2025-33",
                    UnitPrice = 774_000_000,
                    GuaranteeAmount = 630_000_000,
                    Status = CancelBankMDDetailStatus.Approved,
                    Remark = "Ngân hàng VietinBank đã duyệt chấp thuận hủy"
                }
            );

            // 3. Hồ sơ 03: MỚI LẬP CHỜ XỬ LÝ (Pending) - Hyundai Đông Anh đề nghị đổi sang Techcombank
            var can3 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-003",
                DlrCtrNo = "PLHD-2025-05/DA03",
                DealerCode = "HYUNDAI-DA",
                DealerName = "Hyundai Đông Anh",
                BankCodeMD = "MBB",
                BankNameMD = "Ngân hàng TMCP Quân Đội (MBBank)",
                NewBankCodeMD = "TCB",
                NewBankNameMD = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 3_118_000_000,
                GuaranteeAmount = 2_600_000_000,
                ReasonType = CancelBankMDReasonType.ChangeBank,
                ReasonDescription = "Đại lý chuyển bảo lãnh sang Techcombank để hưởng chính sách ưu đãi hạn mức và thời gian trả chậm dài hơn",
                Status = CancelBankMDStatus.Pending,
                TotalVehicles = 2,
                RemarkDlr = "Kính chuyển Hội sở MBBank xem xét chấp thuận hủy chỉ định bảo lãnh để đại lý gán Techcombank",
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
                    SpecDescription = "Bản SUV cỡ lớn 7 chỗ máy dầu",
                    ColorExtNameVN = "Xanh Bóng Đêm",
                    EngineNo = "ENG-PL-2025-41",
                    UnitPrice = 1_559_000_000,
                    GuaranteeAmount = 1_300_000_000,
                    Status = CancelBankMDDetailStatus.Pending,
                    Remark = "Chờ MBBank thẩm định chấp thuận"
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
                    SpecDescription = "Bản SUV cỡ lớn 7 chỗ máy dầu",
                    ColorExtNameVN = "Đen",
                    EngineNo = "ENG-PL-2025-42",
                    UnitPrice = 1_559_000_000,
                    GuaranteeAmount = 1_300_000_000,
                    Status = CancelBankMDDetailStatus.Pending,
                    Remark = "Chờ MBBank thẩm định chấp thuận"
                }
            );

            // 4. Hồ sơ 04: TỪ CHỐI (Rejected) - Hyundai Phạm Văn Đồng
            var can4 = new ContractBankMDCancel
            {
                OrgId = TenantContext.DefaultOrgId,
                CancelBankMDNo = "CANMD-202505-004",
                DlrCtrNo = "PLHD-2025-05/PVD04",
                DealerCode = "HYUNDAI-PVD",
                DealerName = "Hyundai Phạm Văn Đồng",
                BankCodeMD = "VCB",
                BankNameMD = "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank)",
                GuaranteeType = GuaranteeType.Payment,
                ContractAmount = 1_450_000_000,
                GuaranteeAmount = 1_450_000_000,
                ReasonType = CancelBankMDReasonType.ContractRestructuring,
                ReasonDescription = "Đại lý đề nghị hủy bảo lãnh để điều chỉnh cơ cấu phân bổ dòng xe",
                Status = CancelBankMDStatus.Rejected,
                TotalVehicles = 1,
                RemarkDlr = "Kính đề nghị HTC xem xét hủy bảo lãnh xe điện Ioniq 5 để cơ cấu lại hợp đồng",
                RejectBy = "NguyenDucThanh_GiamDocPhapChe_HTC",
                RejectDateTime = DateTime.Today.AddDays(-2),
                RejectReason = "Xe đã hoàn tất thủ tục xuất kho và đang trên đường vận chuyển giao đại lý theo biên bản BBBG, không thể hủy gán ngân hàng bảo lãnh.",
                RemarkHTC = "[HTC từ chối] Xe đã hoàn tất thủ tục xuất kho và đang trên đường vận chuyển giao đại lý theo biên bản BBBG, không thể hủy gán ngân hàng bảo lãnh.",
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
                    SpecDescription = "Xe điện công nghệ sạc siêu nhanh E-GMP",
                    ColorExtNameVN = "Bạc Nhám Gravity Gold",
                    EngineNo = "EM07-2025-99",
                    UnitPrice = 1_450_000_000,
                    GuaranteeAmount = 1_450_000_000,
                    Status = CancelBankMDDetailStatus.Cancelled,
                    Remark = "Từ chối do xe đã vận chuyển đi"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 19. Seed Dữ liệu mẫu Thu Tiền Thanh Toán & Quyết Toán Bồi Thường Bảo Hiểm Xe Ô Tô (Insurance Claim Payment) =====
        if (!await db.InsuranceClaimDebits.AnyAsync())
        {
            var d1 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-001",
                InsNo = "INS-PTI",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
                RONo = "RO-2025-0501",
                VIN = "KMHCT81EPHU990101",
                PlateNo = "30H-889.99",
                ModelCode = "SANTAFE",
                ModelName = "Hyundai Santa Fe 2.5 HTRAC",
                CustomerName = "Trần Đình Trọng",
                CustomerPhone = "0912345678",
                DebitDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(10),
                DebitAmount = 28_500_000,
                PaidAmount = 28_500_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Sơn sấy cản trước và thay cụm đèn pha LED bên lái sau va quẹt",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-20)
            };

            var d2 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-002",
                InsNo = "INS-PTI",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
                RONo = "RO-2025-0502",
                VIN = "KMHCT81EPHU990102",
                PlateNo = "30G-668.88",
                ModelCode = "TUCSON",
                ModelName = "Hyundai Tucson 2.0 AT",
                CustomerName = "Nguyễn Thị Mai",
                CustomerPhone = "0987654321",
                DebitDate = DateTime.Today.AddDays(-18),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 16_200_000,
                PaidAmount = 16_200_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Gò nắn tai xe bên phụ và sơn phủ bóng 2 lớp",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-18)
            };

            var d3 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-003",
                InsNo = "INS-PTI",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
                RONo = "RO-2025-0503",
                VIN = "KMHCT81EPHU990103",
                PlateNo = "29A-995.12",
                ModelCode = "CRETA",
                ModelName = "Hyundai Creta 1.5 Cao Cấp",
                CustomerName = "Lê Hoàng Long",
                CustomerPhone = "0903112233",
                DebitDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 19_800_000,
                PaidAmount = 10_300_000,
                RemainAmount = 9_500_000,
                Status = InsuranceDebitStatus.PartiallyPaid,
                Note = "Thay kính chắn gió trước chính hãng và sơn sườn xe",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-15)
            };

            var d4 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-004",
                InsNo = "INS-PTI",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
                RONo = "RO-2025-0504",
                VIN = "KMHCT81EPHU990104",
                PlateNo = "30F-334.56",
                ModelCode = "ACCENT",
                ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                CustomerName = "Đặng Quang Huy",
                CustomerPhone = "0945678901",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 7_500_000,
                PaidAmount = 0,
                RemainAmount = 7_500_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Sơn phục hồi trầy xước nắp capo và cản sau",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var d5 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-005",
                InsNo = "INS-BV",
                InsName = "Tổng Công ty Bảo hiểm Bảo Việt",
                RONo = "RO-2025-0505",
                VIN = "KMHCT81EPHU990201",
                PlateNo = "30E-778.90",
                ModelCode = "SANTAFE",
                ModelName = "Hyundai Santa Fe 2.5 HTRAC",
                CustomerName = "Phạm Hồng Quân",
                CustomerPhone = "0934567890",
                DebitDate = DateTime.Today.AddDays(-12),
                DueDate = DateTime.Today.AddDays(18),
                DebitAmount = 34_000_000,
                PaidAmount = 34_000_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Thay thế thước lái điện tử và bảo dưỡng phục hồi giảm xóc trước",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-12)
            };

            var d6 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-006",
                InsNo = "INS-BV",
                InsName = "Tổng Công ty Bảo hiểm Bảo Việt",
                RONo = "RO-2025-0506",
                VIN = "KMHCT81EPHU990202",
                PlateNo = "30K-112.33",
                ModelCode = "PALISADE",
                ModelName = "Hyundai Palisade 2.2D Prestige",
                CustomerName = "Hoàng Thu Trang",
                CustomerPhone = "0967890123",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(27),
                DebitAmount = 42_500_000,
                PaidAmount = 0,
                RemainAmount = 42_500_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Phục hồi cánh cửa sau bên phụ và sơn phủ bóng cốp điện",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var d7 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-007",
                InsNo = "INS-PJICO",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Petrolimex (PJICO)",
                RONo = "RO-2025-0507",
                VIN = "KMHCT81EPHU990301",
                PlateNo = "29B-445.67",
                ModelCode = "CRETA",
                ModelName = "Hyundai Creta 1.5 Cao Cấp",
                CustomerName = "Đỗ Đức Toàn",
                CustomerPhone = "0978901234",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 21_000_000,
                PaidAmount = 21_000_000,
                RemainAmount = 0,
                Status = InsuranceDebitStatus.Settled,
                Note = "Sửa chữa hệ thống giải nhiệt két nước và cản trước",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var d8 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-008",
                InsNo = "INS-PJICO",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Petrolimex (PJICO)",
                RONo = "RO-2025-0508",
                VIN = "KMHCT81EPHU990302",
                PlateNo = "30A-556.78",
                ModelCode = "ELANTRA",
                ModelName = "Hyundai Elantra N-Line 1.6 Turbo",
                CustomerName = "Bùi Anh Tuấn",
                CustomerPhone = "0923456789",
                DebitDate = DateTime.Today.AddDays(-7),
                DueDate = DateTime.Today.AddDays(23),
                DebitAmount = 12_800_000,
                PaidAmount = 6_000_000,
                RemainAmount = 6_800_000,
                Status = InsuranceDebitStatus.PartiallyPaid,
                Note = "Sơn sấy hông trái và cửa trước",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-7)
            };

            var d9 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-009",
                InsNo = "INS-PVI",
                InsName = "Tổng Công ty Bảo hiểm PVI (Dầu khí)",
                RONo = "RO-2025-0509",
                VIN = "KMHCT81EPHU990401",
                PlateNo = "30H-223.45",
                ModelCode = "TUCSON",
                ModelName = "Hyundai Tucson 2.0 AT",
                CustomerName = "Ngô Minh Khang",
                CustomerPhone = "0918765432",
                DebitDate = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(28),
                DebitAmount = 38_000_000,
                PaidAmount = 0,
                RemainAmount = 38_000_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Bảo dưỡng đại tu gầm và sơn ba-đờ-sốc sau",
                CreatedBy = "VuVanChien_CVDV",
                CreatedAt = DateTime.Today.AddDays(-2)
            };

            var d10 = new InsuranceClaimDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-INS-202505-010",
                InsNo = "INS-MIC",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Quân đội (MIC)",
                RONo = "RO-2025-0510",
                VIN = "KMHCT81EPHU990501",
                PlateNo = "29D-889.01",
                ModelCode = "ACCENT",
                ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                CustomerName = "Vũ Đình Nam",
                CustomerPhone = "0909876543",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(29),
                DebitAmount = 9_600_000,
                PaidAmount = 0,
                RemainAmount = 9_600_000,
                Status = InsuranceDebitStatus.Pending,
                Note = "Thay cụm gương chiếu hậu có camera 360 và sơn cánh cửa",
                CreatedBy = "NguyenVanHieu_CVDV",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            db.InsuranceClaimDebits.AddRange(d1, d2, d3, d4, d5, d6, d7, d8, d9, d10);
            await db.SaveChangesAsync();

            // Phiếu thu 1: PTI thanh toán gộp 55,000,000 VND (chuyển khoản VietinBank)
            var p1 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-001",
                InsNo = "INS-PTI",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Bưu điện (PTI)",
                PayDate = DateTime.Today.AddDays(-8),
                PayPersonName = "Nguyễn Văn Tuấn (Giám định viên PTI)",
                PayPersonIDCardNo = "001089012345",
                PayPersonPhone = "0911223344",
                PaymentAmount = 55_000_000,
                PaymentMethod = InsurancePaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - CN Đống Đa",
                BankAccountNo = "113000088999",
                BankTxnRef = "FT250508001882",
                TotalAllocated = 55_000_000,
                UnallocatedAmount = 0,
                Status = InsurancePaymentStatus.Settled,
                Note = "PTI thanh toán bồi thường đợt 1 tháng 05/2025 cho 3 xe",
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
                    Remark = "Tất toán toàn bộ lệnh RO"
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
                    Remark = "Tất toán toàn bộ lệnh RO"
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
                    Remark = "Thanh toán một phần nợ RO"
                }
            );

            // Phiếu thu 2: Bảo Việt thanh toán 34,000,000 VND qua Cổng VNPay QR
            var p2 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-002",
                InsNo = "INS-BV",
                InsName = "Tổng Công ty Bảo hiểm Bảo Việt",
                PayDate = DateTime.Today.AddDays(-4),
                PayPersonName = "Lê Thị Hồng (Kế toán Bồi thường Bảo Việt)",
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
                Note = "Bảo Việt chuyển khoản quét mã QR VNPay thanh toán lệnh RO-2025-0505",
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
                    Remark = "Tất toán toàn bộ lệnh RO qua cổng VNPay"
                }
            );

            // Phiếu thu 3: PJICO thanh toán 27,000,000 VND (chuyển khoản MBBank)
            var p3 = new InsurancePayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-INS-202505-003",
                InsNo = "INS-PJICO",
                InsName = "Tổng Công ty Cổ phần Bảo hiểm Petrolimex (PJICO)",
                PayDate = DateTime.Today.AddDays(-1),
                PayPersonName = "Hoàng Trọng Nghĩa (Giám định viên PJICO)",
                PayPersonIDCardNo = "001095067890",
                PayPersonPhone = "0933445566",
                PaymentAmount = 27_000_000,
                PaymentMethod = InsurancePaymentMethod.BankTransfer,
                BankCode = "MBB",
                BankName = "MBBank - CN Sở Giao Dịch",
                BankAccountNo = "0880112345678",
                BankTxnRef = "MB-TRANS-9908123",
                TotalAllocated = 27_000_000,
                UnallocatedAmount = 0,
                Status = InsurancePaymentStatus.Confirmed,
                Note = "PJICO chuyển tiền thanh toán theo thỏa thuận bồi thường xe Hyundai",
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
                    Remark = "Tất toán toàn bộ lệnh RO"
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
                    Remark = "Thanh toán một phần nợ RO"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 20. Seed Dữ liệu mẫu Công Nợ & Thanh Toán Quyết Toán Nhà Cung Cấp Phụ Tùng (Supplier Payment) =====
        if (!await db.SupplierDebits.AnyAsync())
        {
            var sd1 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-001",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "Lô E3, KCN Thăng Long, Huyện Đông Anh, Hà Nội",
                StockInNo = "PNK-2025-0501",
                StockInDate = DateTime.Today.AddDays(-25),
                OrderPartNo = "PO-2025-0480",
                Category = "Linh kiện gầm máy & Lọc dầu nhớt Santa Fe/Tucson",
                DebitDate = DateTime.Today.AddDays(-25),
                DueDate = DateTime.Today.AddDays(5),
                DebitAmount = 35_000_000,
                PaidAmount = 35_000_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "Nhập kho 100 bộ lọc dầu, 50 bộ lọc gió động cơ và cảm biến khí nạp",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-25)
            };

            var sd2 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-002",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "Lô E3, KCN Thăng Long, Huyện Đông Anh, Hà Nội",
                StockInNo = "PNK-2025-0502",
                StockInDate = DateTime.Today.AddDays(-22),
                OrderPartNo = "PO-2025-0485",
                Category = "Má phanh đĩa trước sau & Cụm moay-ơ Creta/Accent",
                DebitDate = DateTime.Today.AddDays(-22),
                DueDate = DateTime.Today.AddDays(8),
                DebitAmount = 22_500_000,
                PaidAmount = 22_500_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "Lô má phanh đĩa gốm chính hãng nhập khẩu Mobis Hàn Quốc",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-22)
            };

            var sd3 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-003",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "Lô E3, KCN Thăng Long, Huyện Đông Anh, Hà Nội",
                StockInNo = "PNK-2025-0503",
                StockInDate = DateTime.Today.AddDays(-15),
                OrderPartNo = "PO-2025-0492",
                Category = "Đèn pha Full LED & Gương chiếu hậu gập điện Palisade",
                DebitDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 48_000_000,
                PaidAmount = 7_500_000,
                RemainAmount = 40_500_000,
                Status = SupplierDebitStatus.PartiallyPaid,
                Note = "Cụm đèn pha thích ứng thông minh IFS và mặt gương sấy điện",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-15)
            };

            var sd4 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-004",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                SupplierPhone = "024-3768-9988",
                SupplierAddress = "Lô E3, KCN Thăng Long, Huyện Đông Anh, Hà Nội",
                StockInNo = "PNK-2025-0504",
                StockInDate = DateTime.Today.AddDays(-3),
                OrderPartNo = "PO-2025-0510",
                Category = "Càng chữ A & Giảm xóc hơi điện tử Custin",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(27),
                DebitAmount = 31_000_000,
                PaidAmount = 0,
                RemainAmount = 31_000_000,
                Status = SupplierDebitStatus.Pending,
                Note = "Lô giảm xóc trước sau cho xe Custin 2.0 Turbo",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var sd5 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-005",
                SupplierCode = "BOSCH-VN",
                SupplierName = "Công ty TNHH Robert Bosch Việt Nam",
                SupplierPhone = "028-6258-3690",
                SupplierAddress = "Tầng 14, Tòa nhà Deutsches Haus, 33 Lê Duẩn, Quận 1, TP. HCM",
                StockInNo = "PNK-2025-0505",
                StockInDate = DateTime.Today.AddDays(-18),
                OrderPartNo = "PO-2025-0488",
                Category = "Cần gạt mưa Aerotwin & Bugi Iridium cao cấp",
                DebitDate = DateTime.Today.AddDays(-18),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 18_500_000,
                PaidAmount = 18_500_000,
                RemainAmount = 0,
                Status = SupplierDebitStatus.Settled,
                Note = "Gạt mưa silicon đa năng và 120 chiếc bugi đánh lửa Bosch kép",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-18)
            };

            var sd6 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-006",
                SupplierCode = "BOSCH-VN",
                SupplierName = "Công ty TNHH Robert Bosch Việt Nam",
                SupplierPhone = "028-6258-3690",
                SupplierAddress = "Tầng 14, Tòa nhà Deutsches Haus, 33 Lê Duẩn, Quận 1, TP. HCM",
                StockInNo = "PNK-2025-0506",
                StockInDate = DateTime.Today.AddDays(-5),
                OrderPartNo = "PO-2025-0505",
                Category = "Cảm biến áp suất lốp TPMS & Còi sên ô tô 12V",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 14_200_000,
                PaidAmount = 0,
                RemainAmount = 14_200_000,
                Status = SupplierDebitStatus.Pending,
                Note = "Cảm biến van trong tích hợp Bluetooth và còi sên âm lượng cao",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var sd7 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-007",
                SupplierCode = "CASTROL-VN",
                SupplierName = "Công ty TNHH Castrol BP Petco Việt Nam",
                SupplierPhone = "028-3821-9153",
                SupplierAddress = "Lầu 9, Tòa nhà Times Square, 22-36 Nguyễn Huệ, Quận 1, TP. HCM",
                StockInNo = "PNK-2025-0507",
                StockInDate = DateTime.Today.AddDays(-10),
                OrderPartNo = "PO-2025-0498",
                Category = "Dầu nhớt tổng hợp Castrol Magnatec & Edge 5W-30",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 32_000_000,
                PaidAmount = 0,
                RemainAmount = 32_000_000,
                Status = SupplierDebitStatus.Pending,
                Note = "40 thùng dầu Castrol Magnatec Stop-Start 5W-30 và dầu cầu hộp số ATF",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var sd8 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-008",
                SupplierCode = "MICHELIN-VN",
                SupplierName = "Công ty TNHH Michelin Việt Nam",
                SupplierPhone = "028-3824-3456",
                SupplierAddress = "Tầng 10, Tòa nhà Empress Tower, 138-142 Hai Bà Trưng, Quận 1, TP. HCM",
                StockInNo = "PNK-2025-0508",
                StockInDate = DateTime.Today.AddDays(-7),
                OrderPartNo = "PO-2025-0502",
                Category = "Lốp xe ô tô Michelin Primacy 4 & Pilot Sport SUV",
                DebitDate = DateTime.Today.AddDays(-7),
                DueDate = DateTime.Today.AddDays(23),
                DebitAmount = 45_600_000,
                PaidAmount = 0,
                RemainAmount = 45_600_000,
                Status = SupplierDebitStatus.Pending,
                Note = "24 quả lốp kích thước 235/55R19 cho Santa Fe và 235/60R18 cho Tucson",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-7)
            };

            var sd9 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-009",
                SupplierCode = "3M-VN",
                SupplierName = "Công ty TNHH 3M Việt Nam",
                SupplierPhone = "028-5416-0429",
                SupplierAddress = "Tầng 20, Tòa nhà Mapletree Business Centre, 1060 Nguyễn Văn Linh, Quận 7, TP. HCM",
                StockInNo = "PNK-2025-0509",
                StockInDate = DateTime.Today.AddDays(-4),
                OrderPartNo = "PO-2025-0508",
                Category = "Phim cách nhiệt Crystalline & Hóa chất phủ gầm ceramic",
                DebitDate = DateTime.Today.AddDays(-4),
                DueDate = DateTime.Today.AddDays(26),
                DebitAmount = 26_800_000,
                PaidAmount = 0,
                RemainAmount = 26_800_000,
                Status = SupplierDebitStatus.Pending,
                Note = "5 cuộn phim cách nhiệt quang học 3M Crystalline và 30 chai xịt gầm chống rỉ",
                CreatedBy = "VuVanChien_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-4)
            };

            var sd10 = new SupplierDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-SUPP-202505-010",
                SupplierCode = "PPG-VN",
                SupplierName = "Công ty TNHH Sơn PPG Việt Nam",
                SupplierPhone = "0274-375-7260",
                SupplierAddress = "Đường số 6, KCN Việt Hương 1, Thị xã Thuận An, Bình Dương",
                StockInNo = "PNK-2025-0510",
                StockInDate = DateTime.Today.AddDays(-1),
                OrderPartNo = "PO-2025-0515",
                Category = "Sơn ô tô công nghiệp Deltron gốc nước & Dầu bóng 2K",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(29),
                DebitAmount = 17_500_000,
                PaidAmount = 0,
                RemainAmount = 17_500_000,
                Status = SupplierDebitStatus.Pending,
                Note = "Sơn pha màu vi tính chuẩn code màu Hyundai Trắng Tuyết, Đen Ngọc, Đỏ Mận",
                CreatedBy = "NguyenVanHieu_ThuKho",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            db.SupplierDebits.AddRange(sd1, sd2, sd3, sd4, sd5, sd6, sd7, sd8, sd9, sd10);
            await db.SaveChangesAsync();

            // Phiếu chi 1: Thanh toán cho Mobis 65,000,000 VND (chuyển khoản VietinBank CTG)
            // Phân bổ: 35M cho PNK 1 (tất toán), 22.5M cho PNK 2 (tất toán), 7.5M cho PNK 3 (trả một phần)
            var sp1 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-001",
                SupplierCode = "MOBIS-VN",
                SupplierName = "Công ty TNHH Mobis Auto Parts Việt Nam",
                PayDate = DateTime.Today.AddDays(-10),
                PayPersonName = "Nguyễn Văn Hùng (Kế toán công nợ Mobis)",
                PayPersonIDCardNo = "001088019283",
                PayPersonPhone = "024-3768-9988",
                PaymentAmount = 65_000_000,
                PaymentMethod = SupplierPaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - CN Đống Đa",
                BankAccountNo = "113000088999",
                BankTxnRef = "UNC-CTG-202505-00981",
                TotalAllocated = 65_000_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Settled,
                Note = "Thanh toán công nợ đợt 1 tháng 05/2025 cho 3 phiếu nhập kho phụ tùng chính hãng",
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
                    Remark = "Tất toán toàn bộ nợ phiếu nhập kho"
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
                    Remark = "Tất toán toàn bộ nợ phiếu nhập kho"
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
                    Remark = "Thanh toán một phần nợ phiếu nhập kho"
                }
            );

            // Phiếu chi 2: Thanh toán cho Bosch 18,500,000 VND (cổng VNPay QR B2B)
            var sp2 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-002",
                SupplierCode = "BOSCH-VN",
                SupplierName = "Công ty TNHH Robert Bosch Việt Nam",
                PayDate = DateTime.Today.AddDays(-6),
                PayPersonName = "Trần Minh Đức (Đại diện kinh doanh Bosch)",
                PayPersonIDCardNo = "079090012345",
                PayPersonPhone = "028-6258-3690",
                PaymentAmount = 18_500_000,
                PaymentMethod = SupplierPaymentMethod.VnPay,
                BankCode = "VCB",
                BankName = "Vietcombank - CN Tân Định",
                BankAccountNo = "0071008899221",
                BankTxnRef = "VNPAY-B2B-8839102",
                TotalAllocated = 18_500_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Settled,
                Note = "Thanh toán qua cổng VNPay QR hóa đơn gạt mưa và bugi Bosch",
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
                    Remark = "Tất toán toàn bộ nợ phiếu nhập kho"
                }
            );

            // Phiếu chi 3: Đã lập & Kế toán trưởng thẩm định duyệt chi cho Castrol 32,000,000 VND (chờ xuất tiền UNC Techcombank)
            var sp3 = new SupplierPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PM-SUPP-202505-003",
                SupplierCode = "CASTROL-VN",
                SupplierName = "Công ty TNHH Castrol BP Petco Việt Nam",
                PayDate = DateTime.Today,
                PayPersonName = "Lê Quốc Bảo (Thủ quỹ đối tác Castrol)",
                PayPersonIDCardNo = "079085023456",
                PayPersonPhone = "028-3821-9153",
                PaymentAmount = 32_000_000,
                PaymentMethod = SupplierPaymentMethod.BankTransfer,
                BankCode = "TCB",
                BankName = "Techcombank - Hội Sở Chính",
                BankAccountNo = "19028833990011",
                BankTxnRef = "UNC-TCB-202505-00512",
                TotalAllocated = 32_000_000,
                UnallocatedAmount = 0,
                Status = SupplierPaymentStatus.Confirmed,
                Note = "Kế toán trưởng đã duyệt lệnh chi thanh toán tiền dầu nhờn Castrol, chuẩn bị đẩy UNC",
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
                    Remark = "Tất toán toàn bộ nợ phiếu nhập kho"
                }
            );

            await db.SaveChangesAsync();
        }

        // ===== 21. Seed Dữ liệu mẫu Công Nợ & Thu Tiền Quyết Toán Khách Hàng Dịch Vụ Sửa Chữa (Customer Payment) =====
        if (!await db.CustomerDebits.AnyAsync())
        {
            var cd1 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-001",
                CusId = "CUS-001",
                CusName = "Nguyễn Văn Hùng",
                Phone = "0912-345-678",
                Address = "Số 15 Phố Duy Tân, Cầu Giấy, Hà Nội",
                RONo = "RO-202505-0101",
                RODate = DateTime.Today.AddDays(-14),
                PlateNo = "30H-889.92",
                VIN = "KMHSH81WPPU102911",
                ModelCode = "SANTAFE",
                ServiceType = "Bảo dưỡng định kỳ cấp lớn 40.000km",
                DebitDate = DateTime.Today.AddDays(-14),
                DueDate = DateTime.Today.AddDays(1),
                DebitAmount = 8_500_000,
                PaidAmount = 8_500_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "Bảo dưỡng cấp lớn 4 vạn: thay dầu động cơ, lọc dầu, lọc gió, dầu phanh và bugi",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-14)
            };

            var cd2 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-002",
                CusId = "CUS-001",
                CusName = "Nguyễn Văn Hùng",
                Phone = "0912-345-678",
                Address = "Số 15 Phố Duy Tân, Cầu Giấy, Hà Nội",
                RONo = "RO-202505-0205",
                RODate = DateTime.Today.AddDays(-10),
                PlateNo = "30H-889.92",
                VIN = "KMHSH81WPPU102911",
                ModelCode = "SANTAFE",
                ServiceType = "Sửa chữa hệ thống phanh",
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(5),
                DebitAmount = 4_200_000,
                PaidAmount = 4_200_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "Thay má phanh đĩa gốm trước sau và láng đĩa phanh vi tính",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            var cd3 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-003",
                CusId = "CUS-002",
                CusName = "Công ty CP Taxi Mai Linh Bắc Trung Bộ",
                Phone = "024-3833-3333",
                Address = "Khu đô thị Trung Hòa - Nhân Chính, Thanh Xuân, Hà Nội",
                RONo = "RO-202505-0112",
                RODate = DateTime.Today.AddDays(-8),
                PlateNo = "29E-023.45",
                VIN = "KMHDH41CPPU298102",
                ModelCode = "ACCENT",
                ServiceType = "Đại tu hộp số & Côn ly hợp",
                DebitDate = DateTime.Today.AddDays(-8),
                DueDate = DateTime.Today.AddDays(7),
                DebitAmount = 28_000_000,
                PaidAmount = 15_000_000,
                RemainAmount = 13_000_000,
                Status = CustomerDebitStatus.PartiallyPaid,
                Note = "Hạ hộp số thay bàn ép, lá côn, bi tê và bánh đà xe hợp đồng taxi",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-8)
            };

            var cd4 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-004",
                CusId = "CUS-002",
                CusName = "Công ty CP Taxi Mai Linh Bắc Trung Bộ",
                Phone = "024-3833-3333",
                Address = "Khu đô thị Trung Hòa - Nhân Chính, Thanh Xuân, Hà Nội",
                RONo = "RO-202505-0118",
                RODate = DateTime.Today.AddDays(-6),
                PlateNo = "29E-044.88",
                VIN = "KMHDH41CPPU301984",
                ModelCode = "ACCENT",
                ServiceType = "Bảo dưỡng 80.000km & Thay lốp",
                DebitDate = DateTime.Today.AddDays(-6),
                DueDate = DateTime.Today.AddDays(9),
                DebitAmount = 16_500_000,
                PaidAmount = 0,
                RemainAmount = 16_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Bảo dưỡng tổng thể và thay 4 quả lốp Michelin 185/65R15 xe taxi",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-6)
            };

            var cd5 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-005",
                CusId = "CUS-003",
                CusName = "Trần Thị Mai Phương",
                Phone = "0988-123-456",
                Address = "Biệt thự Vinhome Riverside, Long Biên, Hà Nội",
                RONo = "RO-202505-0125",
                RODate = DateTime.Today.AddDays(-5),
                PlateNo = "30K-128.66",
                VIN = "KMHTG81BAPU504123",
                ModelCode = "CRETA",
                ServiceType = "Sơn gò phục hồi thân vỏ",
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(10),
                DebitAmount = 6_800_000,
                PaidAmount = 6_800_000,
                RemainAmount = 0,
                Status = CustomerDebitStatus.Settled,
                Note = "Gò cản trước, sơn hấp buồng sấy ba đờ sốc và tai xe bên phụ",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            var cd6 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-006",
                CusId = "CUS-004",
                CusName = "Lê Hoàng Nam",
                Phone = "0904-567-890",
                Address = "Tòa nhà Keangnam Landmark 72, Phạm Hùng, Nam Từ Liêm, Hà Nội",
                RONo = "RO-202505-0130",
                RODate = DateTime.Today.AddDays(-4),
                PlateNo = "30F-998.12",
                VIN = "KMHT381CMPU612844",
                ModelCode = "TUCSON",
                ServiceType = "Sửa chữa điều hòa & Hệ thống làm mát",
                DebitDate = DateTime.Today.AddDays(-4),
                DueDate = DateTime.Today.AddDays(11),
                DebitAmount = 14_500_000,
                PaidAmount = 0,
                RemainAmount = 14_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Thay lốc điều hòa Hanon, vệ sinh dàn lạnh và nạp ga R134a chuẩn",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-4)
            };

            var cd7 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-007",
                CusId = "CUS-005",
                CusName = "Công ty Vận tải & Du lịch An Phú",
                Phone = "024-3999-8888",
                Address = "Số 48 Hoàng Quốc Việt, Cầu Giấy, Hà Nội",
                RONo = "RO-202505-0142",
                RODate = DateTime.Today.AddDays(-3),
                PlateNo = "29B-512.34",
                VIN = "KMHTS81DAPU718290",
                ModelCode = "STARGAZER",
                ServiceType = "Bảo dưỡng gầm & Hệ thống treo",
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(12),
                DebitAmount = 12_000_000,
                PaidAmount = 0,
                RemainAmount = 12_000_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Thay bộ giảm chấn trước sau, cao su cân bằng và rotuyn lái",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-3)
            };

            var cd8 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-008",
                CusId = "CUS-006",
                CusName = "Vũ Minh Trí",
                Phone = "0915-888-999",
                Address = "Chung cư Mandarin Garden, Hoàng Minh Giám, Cầu Giấy, Hà Nội",
                RONo = "RO-202505-0155",
                RODate = DateTime.Today.AddDays(-2),
                PlateNo = "30G-678.90",
                VIN = "KMHC881EAPU823901",
                ModelCode = "CUSTIN",
                ServiceType = "Chăm sóc & Phụ kiện cao cấp",
                DebitDate = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(13),
                DebitAmount = 18_200_000,
                PaidAmount = 0,
                RemainAmount = 18_200_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Dán phim cách nhiệt quang học 3M Crystalline và phủ gầm chống rỉ cao cấp",
                CreatedBy = "PhamQuangHai_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-2)
            };

            var cd9 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-009",
                CusId = "CUS-007",
                CusName = "Hoàng Đức Mạnh",
                Phone = "0936-777-222",
                Address = "Khu đô thị Ecopark, Văn Giang, Hưng Yên",
                RONo = "RO-202505-0160",
                RODate = DateTime.Today.AddDays(-1),
                PlateNo = "30H-456.78",
                VIN = "KMHEE81EAPU901844",
                ModelCode = "IONIQ5",
                ServiceType = "Bảo dưỡng chuyên biệt xe điện EV",
                DebitDate = DateTime.Today.AddDays(-1),
                DueDate = DateTime.Today.AddDays(14),
                DebitAmount = 5_500_000,
                PaidAmount = 0,
                RemainAmount = 5_500_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Kiểm tra hệ thống quản lý pin BMS, cập nhật ECU và kiểm tra phanh hồi năng lượng",
                CreatedBy = "NguyenTuanAnh_CoVanDichVu",
                CreatedAt = DateTime.Today.AddDays(-1)
            };

            var cd10 = new CustomerDebit
            {
                OrgId = TenantContext.DefaultOrgId,
                DebitNo = "DEB-CUS-202505-010",
                CusId = "CUS-008",
                CusName = "Phạm Thu Hương",
                Phone = "0979-333-555",
                Address = "Số 88 Phố Huế, Hai Bà Trưng, Hà Nội",
                RODate = DateTime.Today,
                PlateNo = "30A-789.01",
                VIN = "KMHDH41DPPU410294",
                ModelCode = "ELANTRA",
                RONo = "RO-202505-0175",
                ServiceType = "Căn chỉnh thước lái & Lốp xe",
                DebitDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 7_200_000,
                PaidAmount = 0,
                RemainAmount = 7_200_000,
                Status = CustomerDebitStatus.Pending,
                Note = "Cân chỉnh góc đặt bánh xe 3D và thay 2 quả lốp trước Bridgestone 205/55R16",
                CreatedBy = "VuMinhDuc_CoVanDichVu",
                CreatedAt = DateTime.Today
            };

            db.CustomerDebits.AddRange(cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10);
            await db.SaveChangesAsync();

            // Phiếu thu 1: Tất toán 2 lệnh sửa chữa cho anh Nguyễn Văn Hùng qua Chuyển khoản VietQR
            var cp1 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-001",
                CusId = "CUS-001",
                CusName = "Nguyễn Văn Hùng",
                CusPhone = "0912-345-678",
                PlateNo = "30H-889.92",
                PayDate = DateTime.Today.AddDays(-9),
                PayPersonName = "Nguyễn Văn Hùng",
                PayPersonIDCardNo = "001085002931",
                PayPersonPhone = "0912-345-678",
                PaymentAmount = 12_700_000,
                PaymentMethod = CustomerPaymentMethod.BankTransfer,
                BankCode = "CTG",
                BankName = "VietinBank - Chi nhánh Nam Thăng Long",
                BankAccountNo = "113000889988",
                BankTxnRef = "FT251299834211",
                TotalAllocated = 12_700_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Settled,
                Note = "Thu tiền thanh toán dịch vụ bảo dưỡng và má phanh qua quét mã VietQR",
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
                    Remark = "Tất toán toàn bộ nợ lệnh sửa chữa RO-202505-0101"
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
                    Remark = "Tất toán toàn bộ nợ lệnh sửa chữa RO-202505-0205"
                }
            );

            // Phiếu thu 2: Tất toán chi phí sơn gò cho chị Trần Thị Mai Phương qua VNPay QR
            var cp2 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-002",
                CusId = "CUS-003",
                CusName = "Trần Thị Mai Phương",
                CusPhone = "0988-123-456",
                PlateNo = "30K-128.66",
                PayDate = DateTime.Today.AddDays(-5),
                PayPersonName = "Trần Thị Mai Phương",
                PayPersonIDCardNo = "001192004821",
                PayPersonPhone = "0988-123-456",
                PaymentAmount = 6_800_000,
                PaymentMethod = CustomerPaymentMethod.VnPay,
                BankCode = "VNPAY",
                BankName = "Cổng thanh toán VNPay QR Pos",
                BankTxnRef = "VNPAY-POS-202505-8831",
                TotalAllocated = 6_800_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Settled,
                Note = "Khách hàng thanh toán qua cổng VNPay QR tại quầy thu ngân dịch vụ",
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
                    Remark = "Tất toán toàn bộ nợ sơn cản trước lệnh RO-202505-0125"
                }
            );

            // Phiếu thu 3: Thanh toán một phần cho Công ty Taxi Mai Linh Bắc Trung Bộ qua Techcombank UNC
            var cp3 = new CustomerPayment
            {
                OrgId = TenantContext.DefaultOrgId,
                PaymentNo = "PT-CUS-202505-003",
                CusId = "CUS-002",
                CusName = "Công ty CP Taxi Mai Linh Bắc Trung Bộ",
                CusPhone = "024-3833-3333",
                PlateNo = "29E-023.45",
                PayDate = DateTime.Today,
                PayPersonName = "Hoàng Kim Tuấn (Kế toán đội xe Mai Linh)",
                PayPersonIDCardNo = "036087001294",
                PayPersonPhone = "0903-222-111",
                PaymentAmount = 15_000_000,
                PaymentMethod = CustomerPaymentMethod.BankTransfer,
                BankCode = "TCB",
                BankName = "Techcombank - Hội Sở",
                BankAccountNo = "19028833990011",
                BankTxnRef = "UNC-TCB-CUS-202505-092",
                TotalAllocated = 15_000_000,
                UnallocatedAmount = 0,
                Status = CustomerPaymentStatus.Confirmed,
                Note = "Thu ngân xác nhận tiền nổi vào tài khoản Techcombank, trừ nợ đợt 1 lệnh đại tu hộp số RO-202505-0112",
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
                    Remark = "Trừ nợ đợt 1 (15 triệu / 28 triệu), còn nợ 13 triệu"
                }
            );

            await db.SaveChangesAsync();
        }

        // Seed dữ liệu mẫu cho Quản lý Biên Bản Thỏa Thuận Hủy Hợp Đồng & Quyết Toán Nghĩa Vụ Tài Chính (BizHTC.Contract / DMS40.Contract)
        if (!await db.ContractCancellations.AnyAsync())
        {
            // 1. Biên bản hoàn tất quyết toán hoàn cọc 100% qua UNC VietinBank (Settled)
            var cc1 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-001",
                DlrContractNo = "HDMB-2025-HN01",
                DealerCode = "VN001",
                DealerName = "Công ty CP Ô tô Hyundai Hà Nội",
                CancelDate = DateTime.Today.AddDays(-10),
                SettlementType = ContractCancelSettlementType.RefundDeposit,
                BankCode = "CTG",
                BankName = "Ngân hàng TMCP Công Thương Việt Nam (VietinBank)",
                BankGuaranteeNo = null,
                TransferContractNo = null,
                TotalVehicles = 2,
                TotalContractAmount = 2_190_000_000,
                TotalDepositPaid = 328_500_000,
                TotalRefundAmount = 328_500_000,
                TotalPenaltyAmount = 0,
                TotalGuaranteeRelease = 0,
                Status = ContractCancelStatus.Settled,
                CancelReason = "Hãng điều chỉnh kế hoạch sản xuất linh kiện toàn cầu, không kịp giao xe đúng tiến độ cam kết. Hai bên thống nhất thỏa thuận hủy phụ lục hợp đồng và HTC hoàn lại 100% tiền cọc cho đại lý.",
                BankTxnRef = "UNC-CTG-REF-202505-0019",
                SettledBy = "NguyenVanQuyet_KeToanThanhToan",
                SettledAt = DateTime.Today.AddDays(-8),
                ApprovedBy = "TranMinhDuc_PhoTongGiamDocKinhDoanh",
                ApprovedAt = DateTime.Today.AddDays(-9),
                Remark = "Đã xuất lệnh chi UNC chuyển khoản VietinBank tài khoản thụ hưởng của đại lý thành công.",
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
                    ModelName = "Hyundai Santa Fe 2.5 AWD Cao Cấp",
                    SpecCode = "SF-2.5-PRE",
                    ColorCode = "TRANG",
                    UnitPrice = 1_450_000_000,
                    DepositPaid = 217_500_000,
                    RefundAmount = 217_500_000,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Settled,
                    Remark = "Hoàn cọc 15% do đứt gãy chuỗi cung ứng linh kiện radar"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc1.ContractCancelNo,
                    VIN = "KMHCT81EPNU100882",
                    CarId = "CAR-CRETA-0882",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CR-1.5-PRE",
                    ColorCode = "DO",
                    UnitPrice = 740_000_000,
                    DepositPaid = 111_000_000,
                    RefundAmount = 111_000_000,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Settled,
                    Remark = "Hoàn cọc 15% thỏa thuận hủy đơn hàng đại lý"
                }
            );

            // 2. Biên bản phạt cọc do đại lý chậm nộp tiền thanh toán (Approved, chờ quyết toán)
            var cc2 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-002",
                DlrContractNo = "HDMB-2025-VH03",
                DealerCode = "VN040",
                DealerName = "Công ty CP Ô tô Hyundai Việt Hàn (Sài Gòn)",
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
                CancelReason = "Đại lý vi phạm nghiêm trọng cam kết tiến độ thanh toán xe (quá hạn 45 ngày kể từ ngày thông báo giao xe). Căn cứ Điều 8 Hợp đồng phân phối đại lý, HTC quyết định hủy hợp đồng và tịch thu toàn bộ tiền cọc nộp phạt.",
                BankTxnRef = null,
                ApprovedBy = "PhamQuangHai_GiamDocTaiChinh",
                ApprovedAt = DateTime.Today.AddDays(-2),
                Remark = "Lãnh đạo đã ký duyệt phạt cọc, chuyển phòng Kế toán hạch toán sung doanh thu phạt vi phạm hợp đồng.",
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
                    Remark = "Phạt cọc 15% vi phạm thời hạn nộp tiền thanh toán dứt điểm"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc2.ContractCancelNo,
                    VIN = "KMHAC81EPNU200116",
                    CarId = "CAR-ACCENT-0116",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5-DB",
                    ColorCode = "BAC",
                    UnitPrice = 560_000_000,
                    DepositPaid = 84_000_000,
                    RefundAmount = 0,
                    PenaltyAmount = 84_000_000,
                    GuaranteeAmount = 0,
                    Status = ContractCancelDetailStatus.Approved,
                    Remark = "Phạt cọc 15% vi phạm nhận xe theo thông báo số TB-GIAOXE-8839"
                }
            );

            // 3. Biên bản giải phóng bảo lãnh ngân hàng VPBank (Submitted, đang trình duyệt)
            var cc3 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-003",
                DlrContractNo = "HDMB-2025-DA08",
                DealerCode = "VN005",
                DealerName = "Công ty CP Thương mại Dịch vụ Hyundai Đông Anh",
                CancelDate = DateTime.Today.AddDays(-2),
                SettlementType = ContractCancelSettlementType.ReleaseGuarantee,
                BankCode = "VPB",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                BankGuaranteeNo = "BLTT-VPB-2025-9982",
                TransferContractNo = null,
                TotalVehicles = 2,
                TotalContractAmount = 3_150_000_000,
                TotalDepositPaid = 472_500_000,
                TotalRefundAmount = 0,
                TotalPenaltyAmount = 0,
                TotalGuaranteeRelease = 2_677_500_000,
                Status = ContractCancelStatus.Submitted,
                CancelReason = "Khách hàng mua lô doanh nghiệp của đại lý thay đổi nhu cầu sang xe thương mại tải nặng. Đại lý đề nghị hủy giao xe phụ lục HDMB-2025-DA08 và đề nghị HTC phát hành văn bản giải tỏa Thư bảo lãnh số BLTT-VPB-2025-9982 để hoàn trả hạn mức tín dụng cho đại lý.",
                BankTxnRef = null,
                Remark = "Hồ sơ đầy đủ công văn giải trình của đại lý và biên bản làm việc giữa 2 bên. Đã trình Ban Giám đốc thẩm định.",
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
                    Remark = "Giải phóng nghĩa vụ bảo lãnh 85% giá trị xe tại VPBank"
                },
                new ContractCancelDetail
                {
                    CancellationId = cc3.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    ContractCancelNo = cc3.ContractCancelNo,
                    VIN = "KMHCU81EPNU300552",
                    CarId = "CAR-CUSTIN-0552",
                    ModelCode = "CUSTIN",
                    ModelName = "Hyundai Custin 2.0 T-GDi Cao Cấp",
                    SpecCode = "CUS-2.0T-PRE",
                    ColorCode = "TRANG",
                    UnitPrice = 1_280_000_000,
                    DepositPaid = 192_000_000,
                    RefundAmount = 0,
                    PenaltyAmount = 0,
                    GuaranteeAmount = 1_088_000_000,
                    Status = ContractCancelDetailStatus.Pending,
                    Remark = "Giải phóng nghĩa vụ bảo lãnh 85% giá trị xe tại VPBank"
                }
            );

            // 4. Biên bản chuyển tiền cọc sang phụ lục hợp đồng xe khác (Draft)
            var cc4 = new ContractCancellation
            {
                OrgId = TenantContext.DefaultOrgId,
                ContractCancelNo = "DCC-202505-004",
                DlrContractNo = "HDMB-2025-HP05",
                DealerCode = "VN018",
                DealerName = "Công ty TNHH Hyundai Hải Phòng",
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
                CancelReason = "Đại lý và khách hàng thỏa thuận chuyển đổi sang đặt mua dòng xe thuần điện IONIQ 5 thế hệ mới theo phụ lục hợp đồng HDMB-2025-HP12, xin chuyển toàn bộ 217,500,000 VND tiền cọc đã nộp sang hợp đồng mới.",
                Remark = "Dự thảo biên bản thỏa thuận chuyển cọc, đang hoàn thiện hồ sơ đối chiếu phiếu thu tiền cọc ban đầu.",
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
                    Remark = "Chuyển tiền cọc sang hợp đồng mới HDMB-2025-HP12"
                }
            );

            await db.SaveChangesAsync();
        }

        // Seed dữ liệu mẫu cho Quản lý Hồ sơ Đề nghị Mượn / Bàn Giao Chứng Từ Gốc Xe Ô Tô & Xác Nhận Ngân Hàng (BizHTC.Car.Profile.cs / Car_DocReqList / FrmMngDocReq)
        if (!await db.CarDocRequests.AnyAsync())
        {
            // Hồ sơ 1: Bàn giao chứng từ xe hoàn tất thanh toán (TypeCRR = Normal, Status = HandedOver)
            var req1 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-001",
                DealerCode = "DLR-PVD",
                DealerName = "Công ty CP Ô tô Hyundai Phạm Văn Đồng",
                BankCode = null,
                BankName = null,
                TypeCRR = CarDocReqType.Normal,
                LetterRepresentationNo = "GGT-2025/05/PVD-01",
                LetterRepresentationDate = DateTime.Today.AddDays(-5),
                RepresentativeName = "Nguyễn Văn Hưng",
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
                HandoverRecipient = "Nguyễn Văn Hưng",
                Remark = "Đại lý đã hoàn tất thanh toán 100% tiền mua 02 xe Santa Fe qua UNC ngân hàng VietinBank. Hồ sơ gốc CO/CQ đã xuất giao đủ cho đại diện đại lý nhận làm thủ tục bàn giao khách.",
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
                    ColorNameVN = "Trắng Tinh Khôi",
                    ContractNo = "HDMB-2025-PVD01",
                    UnitPriceActual = 1_280_000_000,
                    PaymentPercent = 100.0m,
                    DepositPercent = 20.0m,
                    GuaranteePercent = 0m,
                    DutyCompletePercent = 100.0m,
                    CONo = "CO-2025-STA-8821",
                    CQNo = "CQ-2025-STA-8821",
                    HTCInvoiceNo = "HD-2025-001289",
                    DocumentsGiven = "Bản gốc CO, Bản sao CQ, Giấy kiểm định xuất xưởng, Hóa đơn VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req1.HandoverDate,
                    Remark = "Đã xuất giao hồ sơ gốc hoàn tất"
                },
                new CarDocReqDetail
                {
                    DocReqId = req1.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req1.DRListCode,
                    VIN = "KMHCT81DAPU029944",
                    CarId = "CAR-CRE-029944",
                    ModelCode = "CRETA",
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5-PRE",
                    EngineNo = "G4FL-PU029944",
                    ColorNameVN = "Đỏ Quyến Rũ",
                    ContractNo = "HDMB-2025-PVD01",
                    UnitPriceActual = 870_000_000,
                    PaymentPercent = 100.0m,
                    DepositPercent = 20.0m,
                    GuaranteePercent = 0m,
                    DutyCompletePercent = 100.0m,
                    CONo = "CO-2025-CRE-9944",
                    CQNo = "CQ-2025-CRE-9944",
                    HTCInvoiceNo = "HD-2025-001290",
                    DocumentsGiven = "Bản gốc CO, Bản sao CQ, Hóa đơn VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req1.HandoverDate,
                    Remark = "Đã xuất giao hồ sơ gốc hoàn tất"
                }
            );

            // Hồ sơ 2: Bàn giao theo Thư bảo lãnh ngân hàng tài trợ Techcombank (TypeCRR = Special, Status = Approved2)
            var req2 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-002",
                DealerCode = "DLR-DA",
                DealerName = "Công ty CP Ô tô Hyundai Đông Anh",
                BankCode = "TCB",
                BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam (Techcombank) - CN Thăng Long",
                TypeCRR = CarDocReqType.Special,
                LetterRepresentationNo = "GGT-2025/05/DA-08",
                LetterRepresentationDate = DateTime.Today.AddDays(-3),
                RepresentativeName = "Trần Hải Đăng",
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
                Remark = "Đại lý đã thanh toán 30% tiền cọc và có Thư bảo lãnh Techcombank số BL-2025-TCB-8821 bảo lãnh 70% giá trị hợp đồng. Ngân hàng đã thẩm định và xác nhận chấp thuận giải phóng hồ sơ xe. Đã có lệnh xuất két A2.",
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
                    ColorNameVN = "Đen Sang Trọng",
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
                    DocumentsGiven = "Bản gốc CO, Bản sao CQ, Tờ khai HQ, Hóa đơn VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "NguyenPhuongThao_CBTD_TCB",
                    BankApprDTime = DateTime.Today.AddDays(-1),
                    BankApprNote = "Techcombank Thăng Long xác nhận bảo lãnh hợp lệ cho số khung KMHJT81WAPU044120",
                    Status = CarDocReqDetailStatus.Approved2,
                    Remark = "Chờ thủ kho xuất giao chứng từ cho cán bộ ngân hàng ký nhận"
                },
                new CarDocReqDetail
                {
                    DocReqId = req2.Id,
                    OrgId = TenantContext.DefaultOrgId,
                    DRListCode = req2.DRListCode,
                    VIN = "KMHAC81CBPU077112",
                    CarId = "CAR-ACC-077112",
                    ModelCode = "ACCENT",
                    ModelName = "Hyundai All-New Accent 1.5 AT Đặc Biệt",
                    SpecCode = "ACC-1.5-AT-DB",
                    EngineNo = "G4FA-PU077112",
                    ColorNameVN = "Bạc Ánh Kim",
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
                    DocumentsGiven = "Bản gốc CO, Bản sao CQ, Hóa đơn VAT",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "NguyenPhuongThao_CBTD_TCB",
                    BankApprDTime = DateTime.Today.AddDays(-1),
                    BankApprNote = "Techcombank Thăng Long xác nhận bảo lãnh hợp lệ cho số khung KMHAC81CBPU077112",
                    Status = CarDocReqDetailStatus.Approved2,
                    Remark = "Chờ thủ kho xuất giao chứng từ cho cán bộ ngân hàng ký nhận"
                }
            );

            // Hồ sơ 3: Đại lý mượn hồ sơ gốc làm thủ tục đăng ký xe trước cho khách (TypeCRR = Dealer, Status = HandedOver kèm hạn trả két)
            var req3 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-003",
                DealerCode = "DLR-SG",
                DealerName = "Công ty CP Hyundai Sài Gòn Phân Phối",
                BankCode = "VCB",
                BankName = "Ngân hàng TMCP Ngoại Thương Việt Nam (Vietcombank) - CN Kỳ Đồng",
                TypeCRR = CarDocReqType.Dealer,
                LetterRepresentationNo = "GGT-2025/05/SG-14",
                LetterRepresentationDate = DateTime.Today.AddDays(-7),
                RepresentativeName = "Hoàng Minh Trí",
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
                HandoverRecipient = "Hoàng Minh Trí",
                ReturnDueDate = DateTime.Today.AddDays(7), // Hạn trả hồ sơ gốc trong vòng 10 ngày
                Remark = "Đại lý mượn bản gốc CO xe Palisade để hoàn tất thủ tục đăng ký xe và đăng kiểm biển số cho khách hàng VIP của Vietcombank. Cam kết hoàn trả bản gốc CO về két trước ngày quy định.",
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
                    ColorNameVN = "Trắng Ngọc Trai",
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
                    DocumentsGiven = "Bản gốc CO mượn tạm, Bản sao CQ công chứng",
                    BankApprStatus = BankDocApprStatus.Approved,
                    BankApprBy = "VoThiKimOanh_CBTD_VCB",
                    BankApprDTime = DateTime.Today.AddDays(-5),
                    BankApprNote = "Vietcombank Kỳ Đồng đồng ý cho đại lý mượn CO gốc đăng ký bấm biển xe cho KH",
                    Status = CarDocReqDetailStatus.HandedOver,
                    HandoverDate = req3.HandoverDate,
                    ReturnDueDate = req3.ReturnDueDate,
                    Remark = "Đang mượn hồ sơ gốc làm thủ tục đăng ký xe, hạn trả két ngày " + req3.ReturnDueDate?.ToString("dd/MM/yyyy")
                }
            );

            // Hồ sơ 4: Đề nghị mới tạo dự thảo (TypeCRR = Normal, Status = Draft)
            var req4 = new CarDocReqList
            {
                OrgId = TenantContext.DefaultOrgId,
                DRListCode = "DNGT-202505-004",
                DealerCode = "DLR-LB",
                DealerName = "Công ty CP Ô tô Hyundai Long Biên",
                BankCode = "VPB",
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank) - Hội Sở",
                TypeCRR = CarDocReqType.Special,
                LetterRepresentationNo = "GGT-2025/05/LB-03",
                LetterRepresentationDate = DateTime.Today,
                RepresentativeName = "Đoàn Văn Hùng",
                RepresentativeIdCard = "001094002871",
                RepresentativePhone = "0944556677",
                TotalVehicles = 1,
                TotalCarAmount = 1_250_000_000,
                TotalPaymentAmount = 250_000_000,
                TotalGuaranteeAmount = 1_000_000_000,
                AvgDutyCompletePercent = 100.0m,
                Status = CarDocReqStatus.Draft,
                Remark = "Dự thảo hồ sơ đề nghị giao hồ sơ gốc xe Hyundai Custin theo Thư bảo lãnh VPBank, đang chờ hoàn thiện sao y công chứng giấy giới thiệu.",
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
                    ModelName = "Hyundai Custin 2.0T Cao Cấp MPV",
                    SpecCode = "CUS-2.0T-PRE",
                    EngineNo = "G4NN-PU055209",
                    ColorNameVN = "Xanh Lục Bảo",
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
                    DocumentsGiven = "Bản gốc CO, Bản sao CQ, Hóa đơn VAT",
                    BankApprStatus = BankDocApprStatus.Pending,
                    Status = CarDocReqDetailStatus.Pending,
                    Remark = "Dự thảo đề nghị, chuẩn bị trình chuyên viên công nợ HTC thẩm định A1"
                }
            );

            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Hóa đơn GTGT (VAT E-Invoice) bán buôn xe ô tô (VAT_HTCInvoice / PrintVAT)
        if (!await db.HTCInvoices.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            // Hóa đơn 1: Đã phát hành, Đã thanh toán 1 phần (PartiallyPaid)
            var inv1 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-001",
                HTCInvoiceNo = "0012891",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-6),
                DealerCode = "DL-HYUNDAI-HADONG",
                DealerName = "Công ty Cổ phần Ô tô Hyundai Hà Đông",
                BuyerTaxCode = "0105882910",
                BuyerAddress = "Tổ 17 Phường Yên Nghĩa, Quận Hà Đông, TP. Hà Nội",
                BuyerLegalRepresentative = "Nguyễn Văn Tuấn - Tổng Giám Đốc",
                PaymentMethod = "CK",
                BankCode = "CTG",
                BankName = "VietinBank - CN Đô Thành",
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
                Remark = "TT 1,500,000,000 đ qua UNC MBB-8921 ngày 10/05/2025"
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
                    ModelName = "Hyundai Santa Fe 2.5 H-Trac Xăng Cao Cấp",
                    SpecCode = "SAN-2.5-GAS-PRE",
                    EngineNo = "G4KM-CU881023",
                    ColorVN = "Đen Sang Trọng",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-SAN-1023",
                    CQNo = "CQ-2025-SAN-1023",
                    SOCode = "SO-202505-HD01",
                    UnitPrice = 1_269_000_000,
                    VATRate = 10.0m,
                    VATAmount = 126_900_000,
                    TotalPrice = 1_395_900_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "Xe giao đợt 1 tháng 5/2025 theo HĐ đại lý"
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
                    ModelName = "Hyundai Tucson 2.0 Dầu Đặc Biệt",
                    SpecCode = "TUC-2.0D-SPE",
                    EngineNo = "D4HD-CU881024",
                    ColorVN = "Trắng Tinh Khôi",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-TUC-1024",
                    CQNo = "CQ-2025-TUC-1024",
                    SOCode = "SO-202505-HD02",
                    UnitPrice = 959_000_000,
                    VATRate = 10.0m,
                    VATAmount = 95_900_000,
                    TotalPrice = 1_054_900_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "Xe giao đợt 1 tháng 5/2025 theo HĐ đại lý"
                }
            );
            await db.SaveChangesAsync();

            // Hóa đơn 2: Đã phát hành, Đã quyết toán 100% (Settled)
            var inv2 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-002",
                HTCInvoiceNo = "0012892",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-5),
                DealerCode = "DL-HYUNDAI-SAIGON",
                DealerName = "Công ty Cổ phần Ô tô Hyundai Sài Gòn",
                BuyerTaxCode = "0304991823",
                BuyerAddress = "Số 70 Lương Định Của, Phường An Phú, TP. Thủ Đức, TP. Hồ Chí Minh",
                BuyerLegalRepresentative = "Lê Hoàng Nam - Giám Đốc",
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
                Remark = "TT 2,065,800,000 đ qua UNC VCB-3498 ngày 08/05/2025"
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
                    ModelName = "Hyundai Creta 1.5 Cao Cấp",
                    SpecCode = "CRE-1.5-PRE",
                    EngineNo = "G4FL-NU102941",
                    ColorVN = "Đỏ Mận",
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
                    ModelName = "Hyundai Creta 1.5 Đặc Biệt",
                    SpecCode = "CRE-1.5-SPE",
                    EngineNo = "G4FL-NU102942",
                    ColorVN = "Trắng Tuyết",
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
                    ModelName = "Hyundai Accent 1.5 AT Đặc Biệt All New",
                    SpecCode = "ACC-1.5-AT-SPE",
                    EngineNo = "G4LC-NU332190",
                    ColorVN = "Bạc Ánh Kim",
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

            // Hóa đơn 3: Đã phát hành, Chưa thanh toán (Issued)
            var inv3 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-003",
                HTCInvoiceNo = "0012893",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-3),
                DealerCode = "DL-HYUNDAI-DONGDO",
                DealerName = "Công ty Cổ phần Hyundai Đông Đô",
                BuyerTaxCode = "0106771890",
                BuyerAddress = "Số 98 Phố Phùng Hưng, Quận Hoàn Kiếm, TP. Hà Nội",
                BuyerLegalRepresentative = "Trần Mạnh Cường - Chủ Tịch HĐQT",
                PaymentMethod = "CK",
                BankCode = "MBB",
                BankName = "MBBank - CN Sở Giao Dịch",
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
                Remark = "Đã gửi hóa đơn điện tử cho đại lý, chờ kế toán đại lý ủy nhiệm chi"
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
                    ModelName = "Hyundai Palisade 2.2D Prestige 6 chỗ",
                    SpecCode = "PAL-2.2D-PRE-6S",
                    EngineNo = "D4HB-RU091822",
                    ColorVN = "Xanh Bóng Đêm",
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
                    ModelName = "Hyundai Palisade 2.2D Exclusive 7 chỗ",
                    SpecCode = "PAL-2.2D-EXC-7S",
                    EngineNo = "D4HB-RU091823",
                    ColorVN = "Trắng Ngọc Trai",
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

            // Hóa đơn 4: Đã duyệt, Chờ phát hành (Approved)
            var inv4 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-004",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-1),
                DealerCode = "DL-HYUNDAI-DANANG",
                DealerName = "Công ty Cổ phần Ô tô Hyundai Đà Nẵng",
                BuyerTaxCode = "0401882716",
                BuyerAddress = "Số 86 Duy Tân, Phường Hòa Thuận Tây, Quận Hải Châu, TP. Đà Nẵng",
                BuyerLegalRepresentative = "Võ Văn Hùng - Giám Đốc",
                PaymentMethod = "CK",
                BankCode = "VPB",
                BankName = "VPBank - CN Đà Nẵng",
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
                Remark = "Kế toán trưởng đã duyệt, sẵn sàng ký số CA phát hành"
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
                    ModelName = "Hyundai Custin 2.0T Cao Cấp MPV",
                    SpecCode = "CUS-2.0T-PRE",
                    EngineNo = "G4NN-PU055209",
                    ColorVN = "Xanh Lục Bảo",
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
                    ColorVN = "Xám Xi Măng",
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

            // Hóa đơn 5: Dự thảo (Draft)
            var inv5 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-005",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today,
                DealerCode = "DL-HYUNDAI-CANTHO",
                DealerName = "Công ty TNHH MTV Hyundai Cần Thơ",
                BuyerTaxCode = "1801293810",
                BuyerAddress = "Quốc Lộ 91B, Phường An Khánh, Quận Ninh Kiều, TP. Cần Thơ",
                BuyerLegalRepresentative = "Phạm Thị Mai - Giám Đốc",
                PaymentMethod = "CK",
                BankCode = "TCB",
                BankName = "Techcombank - CN Cần Thơ",
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
                Remark = "Dự thảo hóa đơn xe điện Ioniq 5 phân bổ về đại lý Cần Thơ"
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
                    ModelName = "Hyundai Ioniq 5 Prestige Điện AWD",
                    SpecCode = "IQ5-PRE-AWD",
                    EngineNo = "EM17-PU012899",
                    ColorVN = "Vàng Cát Gravity Gold",
                    ProductionYear = "2025",
                    CabinCONo = "CO-2025-IQ5-2899",
                    CQNo = "CQ-2025-IQ5-2899",
                    SOCode = "SO-202505-CT01",
                    UnitPrice = 1_450_000_000,
                    VATRate = 10.0m,
                    VATAmount = 145_000_000,
                    TotalPrice = 1_595_000_000,
                    Status = HTCInvoiceDetailStatus.Active,
                    Remark = "Dòng xe thuần điện EV cao cấp phân phối đại lý"
                }
            );
            await db.SaveChangesAsync();

            // Hóa đơn 6: Thu hồi / Hủy (Cancelled)
            var inv6 = new HTCInvoice
            {
                OrgId = TenantContext.DefaultOrgId,
                HTCInvoiceCode = "HD-HTC-202505-006",
                HTCInvoiceNo = "0012890",
                InvoiceSymbol = "1C25THC",
                InvoiceDate = DateTime.Today.AddDays(-10),
                DealerCode = "DL-HYUNDAI-LONGBIEN",
                DealerName = "Công ty Cổ phần Ô tô Hyundai Long Biên",
                BuyerTaxCode = "0107883921",
                BuyerAddress = "Số 3-5 Nguyễn Văn Linh, Phường Gia Thụy, Quận Long Biên, TP. Hà Nội",
                PaymentMethod = "CK",
                BankCode = "CTG",
                BankName = "VietinBank - CN Đông Hà Nội",
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
                Adj_DeleteReason = "BB số 14/BBTH-2025: Khách hàng đổi sang hợp đồng xe Santa Fe mới, thu hồi hóa đơn nhầm phiên bản.",
                ApprovedBy = "KeToanTruong_HTC",
                ApprovedDate = DateTime.Today.AddDays(-10).AddHours(2),
                IssuedBy = "GiamDocTaiChinh_HTC",
                IssuedDate = DateTime.Today.AddDays(-10).AddHours(4),
                CreatedBy = "KeToanBanHang_HTC",
                CreatedAt = DateTime.Today.AddDays(-10),
                Remark = "Đã lập biên bản thu hồi hóa đơn theo quy định của Cục Thuế"
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
                    ModelName = "Hyundai Stargazer X Cao Cấp 1.5 AT",
                    SpecCode = "STG-X-PRE",
                    EngineNo = "G4FL-PU011999",
                    ColorVN = "Trắng Mờ",
                    ProductionYear = "2025",
                    SOCode = "SO-202505-LB01",
                    UnitPrice = 599_000_000,
                    VATRate = 10.0m,
                    VATAmount = 59_900_000,
                    TotalPrice = 658_900_000,
                    Status = HTCInvoiceDetailStatus.Cancelled,
                    Remark = "Xe thu hồi do đổi sang hợp đồng xe Santa Fe"
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Danh mục Ngân hàng - Đại lý (Mst_BankDealer / FrmDealerBank)
        if (!await db.BankDealers.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.BankDealers.AddRange(
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Long Biên",
                    BankCode = "VCB",
                    BankName = "Vietcombank - CN Sở Giao Dịch",
                    CreditContractNo = "HĐTD-VCB-2025-001",
                    CreditContractDate = DateTime.Today.AddMonths(-6),
                    CreditAmount = 50_000_000_000,
                    BankBranchCode = "VCB-SGD",
                    BankBranchName = "Vietcombank - Chi nhánh Sở Giao Dịch",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Hạn mức tín dụng bảo lãnh + thanh toán xe nhập khẩu",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-6)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Long Biên",
                    BankCode = "TCB",
                    BankName = "Techcombank - CN Long Biên",
                    CreditContractNo = "HĐTD-TCB-2025-014",
                    CreditContractDate = DateTime.Today.AddMonths(-4),
                    CreditAmount = 30_000_000_000,
                    BankBranchCode = "TCB-LB",
                    BankBranchName = "Techcombank - Chi nhánh Long Biên",
                    FlagBankGrt = true,
                    FlagBankPmt = false,
                    Status = BankDealerStatus.Active,
                    Remark = "Chỉ dùng cho bảo lãnh mở L/C nhập khẩu",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-4)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-DANANG",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Đà Nẵng",
                    BankCode = "VPB",
                    BankName = "VPBank - CN Đà Nẵng",
                    CreditContractNo = "HĐTD-VPB-2025-007",
                    CreditContractDate = DateTime.Today.AddMonths(-3),
                    CreditAmount = 20_000_000_000,
                    BankBranchCode = "VPB-DN",
                    BankBranchName = "VPBank - Chi nhánh Đà Nẵng",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Hạn mức tín dụng bảo lãnh + thanh toán",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-3)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-CANTHO",
                    DealerName = "Công ty TNHH MTV Hyundai Cần Thơ",
                    BankCode = "CTG",
                    BankName = "VietinBank - CN Cần Thơ",
                    CreditContractNo = "HĐTD-CTG-2025-021",
                    CreditContractDate = DateTime.Today.AddMonths(-2),
                    CreditAmount = 15_000_000_000,
                    BankBranchCode = "CTG-CT",
                    BankBranchName = "VietinBank - Chi nhánh Cần Thơ",
                    FlagBankGrt = false,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Active,
                    Remark = "Chỉ dùng cho thanh toán UNC, không dùng bảo lãnh",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-2)
                },
                new BankDealer
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DealerCode = "DL-HYUNDAI-HAIPHONG",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Hải Phòng",
                    BankCode = "MBB",
                    BankName = "MBBank - CN Hải Phòng",
                    CreditContractNo = "HĐTD-MBB-2024-088",
                    CreditContractDate = DateTime.Today.AddMonths(-10),
                    CreditAmount = 10_000_000_000,
                    BankBranchCode = "MBB-HP",
                    BankBranchName = "MBBank - Chi nhánh Hải Phòng",
                    FlagBankGrt = true,
                    FlagBankPmt = true,
                    Status = BankDealerStatus.Inactive,
                    Remark = "Hợp đồng tín dụng đã tất toán, tạm ngưng sử dụng",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddMonths(-10),
                    UpdatedBy = "Admin_HTC",
                    UpdatedAt = DateTime.Today.AddMonths(-1)
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Cập nhật Chứng từ Kế toán (FrmUpdateChungTuKT / Pmt_Payment_UpdateFinancial)
        if (!await db.AccountingVoucherUpdates.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.AccountingVoucherUpdates.AddRange(
                new AccountingVoucherUpdate
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchNo = "CTKT-202505-001",
                    Description = "Cập nhật số chứng từ kế toán đợt 1 tháng 05/2025",
                    TotalItems = 2,
                    UpdatedItems = 1,
                    SkippedItems = 1,
                    Status = AccountingVoucherUpdateStatus.Applied,
                    CreatedBy = "KeToanTongHop",
                    CreatedAt = DateTime.Today.AddDays(-6),
                    AppliedBy = "KeToanTruong",
                    AppliedAt = DateTime.Today.AddDays(-5),
                    Remark = "Ghi sổ chứng từ cho các phiếu thanh toán đã hoàn tất",
                    Details =
                    {
                        new AccountingVoucherUpdateDetail
                        {
                            OrgId = TenantContext.DefaultOrgId,
                            PaymentNo = "PM-DEMO-20250501-001",
                            OldAccountingRecordNo = "PKT-202505-0145",
                            NewAccountingRecordNo = "PKT-202505-0301",
                            Status = AccountingVoucherUpdateDetailStatus.Updated,
                            Note = "Đã cập nhật số chứng từ kế toán."
                        },
                        new AccountingVoucherUpdateDetail
                        {
                            OrgId = TenantContext.DefaultOrgId,
                            PaymentNo = "PM-DEMO-20250510-002",
                            NewAccountingRecordNo = "PKT-202505-0302",
                            Status = AccountingVoucherUpdateDetailStatus.Skipped,
                            Note = "Phiếu chưa hoàn tất ghi sổ (trạng thái hiện tại: Approved)."
                        }
                    }
                },
                new AccountingVoucherUpdate
                {
                    OrgId = TenantContext.DefaultOrgId,
                    BatchNo = "CTKT-202505-002",
                    Description = "Cập nhật số chứng từ kế toán đợt 2 tháng 05/2025 (chờ áp dụng)",
                    TotalItems = 1,
                    UpdatedItems = 0,
                    SkippedItems = 0,
                    Status = AccountingVoucherUpdateStatus.Draft,
                    CreatedBy = "KeToanTongHop",
                    CreatedAt = DateTime.Today.AddDays(-1),
                    Remark = "Chờ kế toán trưởng áp dụng ghi sổ",
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

        // Dữ liệu mẫu Lịch Làm Việc & Hạn Nộp Cọc (Mst_Calendar / Mst_Calendar_ResetYear)
        // Sinh lịch làm việc cho năm hiện tại: Thứ 2..Thứ 6 làm việc, Thứ 7 & Chủ nhật nghỉ.
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

        // Dữ liệu mẫu Hợp đồng Mua bán Xe Đại lý (DMS40_CT_DealerContract / FrmMngDC)
        if (!await db.DealerContracts.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.DealerContracts.AddRange(
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-001",
                    DCPType = DlrCtrPaymentType.BankGuarantee,
                    DealerCode = "DL-HYUNDAI-LONGBIEN",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Long Biên",
                    BankCodeMD = "VCB",
                    BankNameMD = "Vietcombank - CN Sở Giao Dịch",
                    ContractDate = DateTime.Today.AddDays(-20),
                    TotalVehicles = 3,
                    TotalAmount = 2_400_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Approved,
                    HTCSignStatus = HTCSignStatus.Approved2,
                    DlrCtrStatus = DlrCtrStatus.Signed,
                    Remark = "Hợp đồng bán buôn xe Santa Fe & Tucson, bảo lãnh VCB",
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
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-SF-001", VIN = "KMHST81D8PU123001", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", SpecCode = "SF-2025-PRE", ColorCode = "WHT", ColorName = "Trắng", OriginNo = "CO-2025-001", ProductionYear = 2025, UnitPrice = 1_200_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-TU-002", VIN = "KMHJ3813DPU123002", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", SpecCode = "TU-2025-STD", ColorCode = "BLK", ColorName = "Đen", OriginNo = "CO-2025-002", ProductionYear = 2025, UnitPrice = 800_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-001", CarId = "CAR-TU-003", VIN = "KMHJ3813DPU123003", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", SpecCode = "TU-2025-STD", ColorCode = "SLV", ColorName = "Bạc", OriginNo = "CO-2025-003", ProductionYear = 2025, UnitPrice = 400_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed }
                    }
                },
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-002",
                    DCPType = DlrCtrPaymentType.LetterOfCredit,
                    DealerCode = "DL-HYUNDAI-DANANG",
                    DealerName = "Công ty Cổ phần Ô tô Hyundai Đà Nẵng",
                    BankCodeMD = "TCB",
                    BankNameMD = "Techcombank - CN Đà Nẵng",
                    ContractDate = DateTime.Today.AddDays(-10),
                    TotalVehicles = 2,
                    TotalAmount = 1_500_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Approved,
                    HTCSignStatus = HTCSignStatus.Approved1,
                    DlrCtrStatus = DlrCtrStatus.NotSign,
                    Remark = "Hợp đồng xe nhập khẩu mở L/C qua Techcombank, chờ HTC duyệt cấp 2",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-10),
                    DlrApprovedBy = "GiamDoc_DaiLy_DN",
                    DlrApprovedAt = DateTime.Today.AddDays(-8),
                    HTCApproved1By = "KeToanTruong_HTC",
                    HTCApproved1At = DateTime.Today.AddDays(-6),
                    Details =
                    {
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-002", CarId = "CAR-PA-004", VIN = "KMHST81D8PU123004", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", SpecCode = "PA-2025-PRE", ColorCode = "GRY", ColorName = "Xám", OriginNo = "CO-2025-004", ProductionYear = 2025, UnitPrice = 1_000_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-002", CarId = "CAR-PA-005", VIN = "KMHST81D8PU123005", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", SpecCode = "PA-2025-STD", ColorCode = "WHT", ColorName = "Trắng", OriginNo = "CO-2025-005", ProductionYear = 2025, UnitPrice = 500_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.Signed }
                    }
                },
                new DealerContract
                {
                    OrgId = TenantContext.DefaultOrgId,
                    DlrCtrNo = "PLHD-202505-003",
                    DCPType = DlrCtrPaymentType.OwnCapital,
                    DealerCode = "DL-HYUNDAI-CANTHO",
                    DealerName = "Công ty TNHH MTV Hyundai Cần Thơ",
                    BankCodeMD = null,
                    BankNameMD = null,
                    ContractDate = DateTime.Today.AddDays(-3),
                    TotalVehicles = 2,
                    TotalAmount = 900_000_000,
                    FlagDlrCtrAdjust = false,
                    DlrSignStatus = DlrSignStatus.Pending,
                    HTCSignStatus = HTCSignStatus.Pending,
                    DlrCtrStatus = DlrCtrStatus.NotSign,
                    Remark = "Hợp đồng thanh toán vốn tự có, chờ đại lý ký",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-3),
                    Details =
                    {
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-003", CarId = "CAR-AC-006", VIN = "KMHCT41D8PU123006", ModelCode = "ACCENT", ModelName = "Hyundai Accent", SpecCode = "AC-2025-STD", ColorCode = "RED", ColorName = "Đỏ", OriginNo = "CO-2025-006", ProductionYear = 2025, UnitPrice = 500_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.NotSign },
                        new DealerContractDetail { OrgId = TenantContext.DefaultOrgId, DlrCtrNo = "PLHD-202505-003", CarId = "CAR-CR-007", VIN = "KMHCT41D8PU123007", ModelCode = "CRETA", ModelName = "Hyundai Creta", SpecCode = "CR-2025-STD", ColorCode = "BLU", ColorName = "Xanh", OriginNo = "CO-2025-007", ProductionYear = 2025, UnitPrice = 400_000_000, DlrCtrStatusDtl = DlrCtrDetailStatus.NotSign }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Yêu cầu Điều chuyển Vận tải Kho (Sto_StorageRearrange / FrmMngSC)
        if (!await db.StorageRearranges.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.StorageRearranges.AddRange(
                new StorageRearrange
                {
                    OrgId = TenantContext.DefaultOrgId,
                    StorageRearrangeNo = "SC-202505-001",
                    RearrangeStatus = StorageRearrangeStatus.Approved2,
                    Remark = "Điều chuyển xe Santa Fe & Tucson từ kho Ninh Bình về kho Đông Anh",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-12),
                    ApprovedBy1 = "TruongKho_HTC",
                    ApprovedAt1 = DateTime.Today.AddDays(-11),
                    ApprovedBy2 = "BanGiamDoc_HTC",
                    ApprovedAt2 = DateTime.Today.AddDays(-10),
                    Details =
                    {
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-001", VIN = "KMHST81D8PU123001", StorageCodeFrom = "KHO-NBD", StorageCodeTo = "KHO-DA", ExpectedStartDate = DateTime.Today.AddDays(-9), ExpectedEndDate = DateTime.Today.AddDays(-7), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved2, Remark = "Xe ưu tiên giao đại lý Long Biên" },
                        new StorageRearrangeDetail { OrgId = TenantContext.DefaultOrgId, StorageRearrangeNo = "SC-202505-001", VIN = "KMHJ3813DPU123002", StorageCodeFrom = "KHO-NBD", StorageCodeTo = "KHO-DA", ExpectedStartDate = DateTime.Today.AddDays(-9), ExpectedEndDate = DateTime.Today.AddDays(-7), RearrangeDtlStatus = StorageRearrangeDetailStatus.Approved2 }
                    }
                },
                new StorageRearrange
                {
                    OrgId = TenantContext.DefaultOrgId,
                    StorageRearrangeNo = "SC-202505-002",
                    RearrangeStatus = StorageRearrangeStatus.Approved1,
                    Remark = "Điều chuyển xe Palisade từ kho Hưng Yên về kho Hiệp Phước SG, chờ duyệt cấp 2",
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
                    Remark = "Điều chuyển xe Accent & Creta từ kho Đông Anh về kho Ninh Bình, chờ duyệt cấp 1",
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

        // Dữ liệu mẫu Phiên bản Cước phí Vận tải (Mst_TranspFeeVer / Mst_TranspFee)
        if (!await db.TransportFeeVersions.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.TransportFeeVersions.AddRange(
                new TransportFeeVersion
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TFVCode = "TFV-2025-01",
                    Description = "Bảng cước phí vận tải áp dụng từ 01/2025",
                    Status = TransportFeeVersionStatus.Active,
                    CreatedDate = DateTime.Today.AddDays(-60),
                    AppliedDate = DateTime.Today.AddDays(-55),
                    Remark = "Cước vận tải tuyến Bắc - Nam, nhà vận tải NewWay & Đạt Đức",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-60),
                    Rates =
                    {
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Hồ Chí Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiệp Phước", TransporterCode = "NEWWAY", TransporterName = "Công ty Vận tải NewWay", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 18_000_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Hồ Chí Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiệp Phước", TransporterCode = "NEWWAY", TransporterName = "Công ty Vận tải NewWay", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", ValFee = 16_000_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "DN", ProvinceNameTo = "Đà Nẵng", DistrictCodeTo = "DN-HC", DistrictNameTo = "Hòa Cường", TransporterCode = "DATDUC", TransporterName = "Công ty Vận tải Đạt Đức", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 12_000_000, ExpectedDays = 3 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-01", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "DN", ProvinceNameTo = "Đà Nẵng", DistrictCodeTo = "DN-HC", DistrictNameTo = "Hòa Cường", TransporterCode = "DATDUC", TransporterName = "Công ty Vận tải Đạt Đức", ModelCode = "CRETA", ModelName = "Hyundai Creta", ValFee = 9_000_000, ExpectedDays = 3 }
                    }
                },
                new TransportFeeVersion
                {
                    OrgId = TenantContext.DefaultOrgId,
                    TFVCode = "TFV-2025-02",
                    Description = "Bảng cước phí vận tải điều chỉnh quý 2/2025 (chờ áp dụng)",
                    Status = TransportFeeVersionStatus.Draft,
                    CreatedDate = DateTime.Today.AddDays(-3),
                    Remark = "Điều chỉnh tăng cước tuyến Hà Nội - TP.HCM do giá nhiên liệu",
                    CreatedBy = "Admin_HTC",
                    CreatedAt = DateTime.Today.AddDays(-3),
                    Rates =
                    {
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-02", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Hồ Chí Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiệp Phước", TransporterCode = "NEWWAY", TransporterName = "Công ty Vận tải NewWay", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ValFee = 19_500_000, ExpectedDays = 5 },
                        new TransportFeeRate { OrgId = TenantContext.DefaultOrgId, TFVCode = "TFV-2025-02", ProvinceCodeFrom = "HN", ProvinceNameFrom = "Hà Nội", DistrictCodeFrom = "HN-DA", DistrictNameFrom = "Đông Anh", ProvinceCodeTo = "HCM", ProvinceNameTo = "TP. Hồ Chí Minh", DistrictCodeTo = "HCM-HP", DistrictNameTo = "Hiệp Phước", TransporterCode = "NEWWAY", TransporterName = "Công ty Vận tải NewWay", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ValFee = 21_000_000, ExpectedDays = 5 }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu Hóa đơn GTGT Nhà máy sản xuất TCG (VAT_TCGInvoice / FrmMngTCGInvoice)
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
                    Remark = "Hóa đơn GTGT nhà máy TCG xuất cho HTC lô xe Santa Fe & Tucson nhập kho",
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
                    Remark = "Hóa đơn GTGT nhà máy TCG lô xe Palisade nhập khẩu CBU, chờ duyệt",
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
                    Remark = "Hóa đơn GTGT nhà máy TCG lô xe Accent bị hủy do sai thông tin",
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

        // Dữ liệu mẫu File / Chứng từ Đính kèm Thư bảo lãnh (Pmt_GuaranteeAttachFile / Pmt_GuaranteeAttachFileHis)
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
                        GrtFileRemark = "Bản scan Thư bảo lãnh gốc do Vietcombank phát hành",
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
                        GrtFileRemark = "Công văn xác nhận hạn mức bảo lãnh của ngân hàng",
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
                        GrtFileRemark = "Bản scan Thư bảo lãnh Techcombank",
                        LogLUBy = "ChuyenVienKinhDoanh",
                        LogLUDateTime = DateTime.Now.AddDays(-10)
                    }
                );
            }

            await db.SaveChangesAsync();

            // Lịch sử lưu file (Pmt_GuaranteeAttachFileHis) — ghi lại trạng thái file hiện hành
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

        // Dữ liệu mẫu Đề nghị xuất hóa đơn / giao hồ sơ (RD_ReqInvoice / RD_ReqInvoiceDtl)
        if (!await db.ReqInvoices.AnyAsync(x => x.OrgId == TenantContext.DefaultOrgId))
        {
            db.ReqInvoices.AddRange(
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-001",
                    ReqIVStatus = ReqIVStatus.Approved,
                    Remark = "Đề nghị giao hồ sơ (hóa đơn GTGT + chứng từ gốc) cho đại lý Hyundai Thanh Xuân",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-15),
                    ApprovedBy = "KeToan_HTC",
                    ApprovedAt = DateTime.Today.AddDays(-14),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-001", VIN = "KMHST81D8PU123001", CarId = "CAR-SF-001", ModelCode = "SANTAFE", ModelName = "Hyundai Santa Fe", ColorCode = "WHT", ColorName = "Trắng", EngineNo = "D4HB-123001", TypeRDReqIv = RDInvoiceType.Dealer, DealerCode = "DLR-HYUNDAI-THANHXUAN", DealerName = "Công ty TNHH Hyundai Thanh Xuân", HTCInvoiceNo = "0000123", DlrCtrNo = "PLHD-202505-001", ProvinceName = "Hà Nội", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-15), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-14) },
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-001", VIN = "KMHJ3813DPU123002", CarId = "CAR-TU-002", ModelCode = "TUCSON", ModelName = "Hyundai Tucson", ColorCode = "BLK", ColorName = "Đen", EngineNo = "G4NA-123002", TypeRDReqIv = RDInvoiceType.Dealer, DealerCode = "DLR-HYUNDAI-THANHXUAN", DealerName = "Công ty TNHH Hyundai Thanh Xuân", HTCInvoiceNo = "0000123", DlrCtrNo = "PLHD-202505-001", ProvinceName = "Hà Nội", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-15), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-14) }
                    }
                },
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-002",
                    ReqIVStatus = ReqIVStatus.Pending,
                    Remark = "Đề nghị giao hồ sơ cho ngân hàng bảo lãnh Vietcombank (chờ duyệt chi tiết)",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-5),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-002", VIN = "KMHST81D8PU123004", CarId = "CAR-PA-004", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ColorCode = "GRY", ColorName = "Xám", EngineNo = "D4HB-123004", TypeRDReqIv = RDInvoiceType.BankBL, DealerCode = "DLR-HYUNDAI-HADONG", DealerName = "Công ty TNHH Hyundai Hà Đông", MortageBankCode = "VCB", GuaranteeNo = "GRT-20250501-001", PGBankCode = "VCB", PGBankCodeMonitor = "VCB", PGDateExpired = DateTime.Today.AddDays(60), DlrCtrNo = "PLHD-202505-002", ProvinceName = "Hà Nội", RDReqIvDtlStatus = RDReqIvDtlStatus.Approved, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-5), ApprovedBy = "KeToan_HTC", ApprovedAt = DateTime.Today.AddDays(-4) },
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-002", VIN = "KMHST81D8PU123005", CarId = "CAR-PA-005", ModelCode = "PALISADE", ModelName = "Hyundai Palisade", ColorCode = "WHT", ColorName = "Trắng", EngineNo = "D4HB-123005", TypeRDReqIv = RDInvoiceType.BankBL, DealerCode = "DLR-HYUNDAI-HADONG", DealerName = "Công ty TNHH Hyundai Hà Đông", MortageBankCode = "VCB", GuaranteeNo = "GRT-20250501-001", PGBankCode = "VCB", PGBankCodeMonitor = "VCB", PGDateExpired = DateTime.Today.AddDays(60), DlrCtrNo = "PLHD-202505-002", ProvinceName = "Hà Nội", RDReqIvDtlStatus = RDReqIvDtlStatus.Pending, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-5) }
                    }
                },
                new ReqInvoice
                {
                    OrgId = TenantContext.DefaultOrgId,
                    ReqIVNo = "RDIV-202505-003",
                    ReqIVStatus = ReqIVStatus.Pending,
                    Remark = "Đề nghị giao hồ sơ cho ngân hàng L/C Techcombank",
                    CreatedBy = "ChuyenVienKinhDoanh",
                    CreatedAt = DateTime.Today.AddDays(-2),
                    Details =
                    {
                        new ReqInvoiceDetail { OrgId = TenantContext.DefaultOrgId, ReqIVNo = "RDIV-202505-003", VIN = "KMHCT41D8PU123006", CarId = "CAR-AC-006", ModelCode = "ACCENT", ModelName = "Hyundai Accent", ColorCode = "RED", ColorName = "Đỏ", EngineNo = "G4LC-123006", TypeRDReqIv = RDInvoiceType.BankLC, DealerCode = "DLR-HYUNDAI-CANTHO", DealerName = "Công ty TNHH MTV Hyundai Cần Thơ", MortageBankCode = "TCB", GuaranteeNo = "GRT-20250510-002", PGBankCode = "TCB", PGBankCodeMonitor = "TCB", PGDateExpired = DateTime.Today.AddDays(45), DlrCtrNo = "PLHD-202505-003", ProvinceName = "Cần Thơ", RDReqIvDtlStatus = RDReqIvDtlStatus.Pending, CreatedBy = "ChuyenVienKinhDoanh", CreatedAt = DateTime.Today.AddDays(-2) }
                    }
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
