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
    }
}
