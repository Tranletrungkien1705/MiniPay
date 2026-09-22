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
    }
}
