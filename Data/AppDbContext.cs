using Microsoft.EntityFrameworkCore;
using MiniPay.Models;

namespace MiniPay.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<PaymentIntent> Payments => Set<PaymentIntent>();
    public DbSet<ReconcileBatch> ReconcileBatches => Set<ReconcileBatch>();
    public DbSet<ReconcileDetail> ReconcileDetails => Set<ReconcileDetail>();
    public DbSet<BankingPayoutBatch> PayoutBatches => Set<BankingPayoutBatch>();
    public DbSet<BankingPayoutDetail> PayoutDetails => Set<BankingPayoutDetail>();
    public DbSet<PaymentDiscountRequest> DiscountRequests => Set<PaymentDiscountRequest>();
    public DbSet<PaymentDiscountDetail> DiscountDetails => Set<PaymentDiscountDetail>();
    public DbSet<PaymentGuarantee> Guarantees => Set<PaymentGuarantee>();
    public DbSet<PaymentGuaranteeDetail> GuaranteeDetails => Set<PaymentGuaranteeDetail>();
    public DbSet<MortgageRequest> MortgageRequests => Set<MortgageRequest>();
    public DbSet<MortgageDetail> MortgageDetails => Set<MortgageDetail>();
    public DbSet<RedeemRequest> RedeemRequests => Set<RedeemRequest>();
    public DbSet<RedeemDetail> RedeemDetails => Set<RedeemDetail>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<PaymentOrderDetail> PaymentOrderDetails => Set<PaymentOrderDetail>();
    public DbSet<BankBillMinutes> BankBillMinutes => Set<BankBillMinutes>();
    public DbSet<BankBillMinutesDetail> BankBillMinutesDetails => Set<BankBillMinutesDetail>();
    public DbSet<PaymentPDI> PaymentPDIs => Set<PaymentPDI>();
    public DbSet<PaymentPDIDetail> PaymentPDIDetails => Set<PaymentPDIDetail>();
    public DbSet<LatePaymentPenalty> LatePaymentPenalties => Set<LatePaymentPenalty>();
    public DbSet<LatePaymentPenaltyDetail> LatePaymentPenaltyDetails => Set<LatePaymentPenaltyDetail>();
    public DbSet<TransportInsPayment> TransportInsPayments => Set<TransportInsPayment>();
    public DbSet<TransportInsPaymentDetail> TransportInsPaymentDetails => Set<TransportInsPaymentDetail>();
    public DbSet<PaymentStorage> PaymentStorages => Set<PaymentStorage>();
    public DbSet<PaymentStorageDetail> PaymentStorageDetails => Set<PaymentStorageDetail>();
    public DbSet<GuaranteeExtensionDispatch> GuaranteeExtensionDispatches => Set<GuaranteeExtensionDispatch>();
    public DbSet<GuaranteeExtensionDetail> GuaranteeExtensionDetails => Set<GuaranteeExtensionDetail>();
    public DbSet<BankGuaranteeClaim> GuaranteeClaims => Set<BankGuaranteeClaim>();
    public DbSet<BankGuaranteeClaimDetail> GuaranteeClaimDetails => Set<BankGuaranteeClaimDetail>();
    public DbSet<PaymentAVN> PaymentAVNs => Set<PaymentAVN>();
    public DbSet<PaymentAVNDetail> PaymentAVNDetails => Set<PaymentAVNDetail>();
    public DbSet<PaymentGPS> PaymentGPSs => Set<PaymentGPS>();
    public DbSet<PaymentGPSDetail> PaymentGPSDetails => Set<PaymentGPSDetail>();
    public DbSet<UnitPriceGPS> UnitPriceGPSs => Set<UnitPriceGPS>();
    public DbSet<FinancialExpenseStatement> FnExpStatements => Set<FinancialExpenseStatement>();
    public DbSet<FinancialExpenseDetail> FnExpDetails => Set<FinancialExpenseDetail>();
    public DbSet<BankingDisbursementRequest> DisbursementRequests => Set<BankingDisbursementRequest>();
    public DbSet<BankingDisbursementDetail> DisbursementDetails => Set<BankingDisbursementDetail>();
    public DbSet<BankingDisbursementFile> DisbursementFiles => Set<BankingDisbursementFile>();
    public DbSet<ContractBankMDCancel> CancelBankMDRequests => Set<ContractBankMDCancel>();
    public DbSet<ContractBankMDCancelDetail> CancelBankMDDetails => Set<ContractBankMDCancelDetail>();
    public DbSet<InsuranceClaimDebit> InsuranceClaimDebits => Set<InsuranceClaimDebit>();
    public DbSet<InsurancePayment> InsurancePayments => Set<InsurancePayment>();
    public DbSet<InsurancePaymentDetail> InsurancePaymentDetails => Set<InsurancePaymentDetail>();
    public DbSet<SupplierDebit> SupplierDebits => Set<SupplierDebit>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<SupplierPaymentDetail> SupplierPaymentDetails => Set<SupplierPaymentDetail>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<PaymentIntent>().HasIndex(x => x.TxnRef).IsUnique();
        b.Entity<PaymentIntent>().Property(x => x.Status).HasConversion<int>();

        b.Entity<ReconcileBatch>().HasIndex(x => new { x.OrgId, x.BatchCode }).IsUnique();
        b.Entity<ReconcileBatch>().Property(x => x.Status).HasConversion<int>();

        b.Entity<ReconcileDetail>().Property(x => x.MatchStatus).HasConversion<int>();
        b.Entity<ReconcileDetail>().HasIndex(x => x.BatchId);
        b.Entity<ReconcileDetail>().HasIndex(x => new { x.OrgId, x.TxnRef });

        b.Entity<BankingPayoutBatch>().HasIndex(x => new { x.OrgId, x.BatchNo }).IsUnique();
        b.Entity<BankingPayoutBatch>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankingPayoutBatch>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<BankingPayoutDetail>().Property(x => x.TransType).HasConversion<int>();
        b.Entity<BankingPayoutDetail>().Property(x => x.DisbursementType).HasConversion<int>();
        b.Entity<BankingPayoutDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankingPayoutDetail>().HasIndex(x => x.BatchId);
        b.Entity<BankingPayoutDetail>().HasIndex(x => new { x.OrgId, x.TransNo });

        b.Entity<PaymentDiscountRequest>().HasIndex(x => new { x.OrgId, x.DiscountNo }).IsUnique();
        b.Entity<PaymentDiscountRequest>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentDiscountRequest>().Property(x => x.PartnerSignStatus).HasConversion<int>();
        b.Entity<PaymentDiscountRequest>().Property(x => x.ApproverSignStatus).HasConversion<int>();
        b.Entity<PaymentDiscountRequest>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentDiscountDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentDiscountDetail>().HasIndex(x => x.RequestId);
        b.Entity<PaymentDiscountDetail>().HasIndex(x => new { x.OrgId, x.ItemRefNo });

        b.Entity<PaymentGuarantee>().ToTable("PaymentGuarantees");
        b.Entity<PaymentGuarantee>().HasIndex(x => new { x.OrgId, x.GuaranteeNo }).IsUnique();
        b.Entity<PaymentGuarantee>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentGuarantee>().Property(x => x.GuaranteeType).HasConversion<int>();
        b.Entity<PaymentGuarantee>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.GuaranteeId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentGuaranteeDetail>().ToTable("PaymentGuaranteeDetails");
        b.Entity<PaymentGuaranteeDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentGuaranteeDetail>().HasIndex(x => x.GuaranteeId);
        b.Entity<PaymentGuaranteeDetail>().HasIndex(x => new { x.OrgId, x.ItemRefNo });

        b.Entity<MortgageRequest>().HasIndex(x => new { x.OrgId, x.ReqRMNo }).IsUnique();
        b.Entity<MortgageRequest>().Property(x => x.Status).HasConversion<int>();
        b.Entity<MortgageRequest>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.MortgageRequestId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<MortgageDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<MortgageDetail>().HasIndex(x => x.MortgageRequestId);
        b.Entity<MortgageDetail>().HasIndex(x => new { x.OrgId, x.ItemRefNo });

        b.Entity<RedeemRequest>().HasIndex(x => new { x.OrgId, x.ReqDMNo }).IsUnique();
        b.Entity<RedeemRequest>().Property(x => x.Status).HasConversion<int>();
        b.Entity<RedeemRequest>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.RedeemRequestId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<RedeemDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<RedeemDetail>().HasIndex(x => x.RedeemRequestId);
        b.Entity<RedeemDetail>().HasIndex(x => new { x.OrgId, x.ItemRefNo });

        b.Entity<PaymentOrder>().HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
        b.Entity<PaymentOrder>().Property(x => x.PaymentType).HasConversion<int>();
        b.Entity<PaymentOrder>().Property(x => x.Funds).HasConversion<int>();
        b.Entity<PaymentOrder>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentOrder>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentOrderId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentOrderDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentOrderDetail>().HasIndex(x => x.PaymentOrderId);
        b.Entity<PaymentOrderDetail>().HasIndex(x => new { x.OrgId, x.ItemRefNo });

        b.Entity<BankBillMinutes>().HasIndex(x => new { x.OrgId, x.BankBillMnNo }).IsUnique();
        b.Entity<BankBillMinutes>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankBillMinutes>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.MinutesId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<BankBillMinutesDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankBillMinutesDetail>().HasIndex(x => x.MinutesId);
        b.Entity<BankBillMinutesDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<PaymentPDI>().HasIndex(x => new { x.OrgId, x.PmtPDINo }).IsUnique();
        b.Entity<PaymentPDI>().HasIndex(x => new { x.OrgId, x.PmtMonth });
        b.Entity<PaymentPDI>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentPDI>().Property(x => x.TCMSSignStatus).HasConversion<int>();
        b.Entity<PaymentPDI>().Property(x => x.HTVSignStatus).HasConversion<int>();
        b.Entity<PaymentPDI>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentPDIId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentPDIDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentPDIDetail>().HasIndex(x => x.PaymentPDIId);
        b.Entity<PaymentPDIDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<LatePaymentPenalty>().HasIndex(x => new { x.OrgId, x.PenaltyRecordNo }).IsUnique();
        b.Entity<LatePaymentPenalty>().HasIndex(x => new { x.OrgId, x.SOCode });
        b.Entity<LatePaymentPenalty>().Property(x => x.Status).HasConversion<int>();
        b.Entity<LatePaymentPenalty>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PenaltyId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<LatePaymentPenaltyDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<LatePaymentPenaltyDetail>().HasIndex(x => x.PenaltyId);
        b.Entity<LatePaymentPenaltyDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<TransportInsPayment>().HasIndex(x => new { x.OrgId, x.TransportInsNo }).IsUnique();
        b.Entity<TransportInsPayment>().HasIndex(x => new { x.OrgId, x.PmtMonth });
        b.Entity<TransportInsPayment>().Property(x => x.Status).HasConversion<int>();
        b.Entity<TransportInsPayment>().Property(x => x.TCMSSignStatus).HasConversion<int>();
        b.Entity<TransportInsPayment>().Property(x => x.HTVSignStatus).HasConversion<int>();
        b.Entity<TransportInsPayment>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.TransportInsPaymentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<TransportInsPaymentDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<TransportInsPaymentDetail>().HasIndex(x => x.TransportInsPaymentId);
        b.Entity<TransportInsPaymentDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<PaymentStorage>().HasIndex(x => new { x.OrgId, x.PaymentStorageNo }).IsUnique();
        b.Entity<PaymentStorage>().HasIndex(x => new { x.OrgId, x.PmtMonth });
        b.Entity<PaymentStorage>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentStorage>().Property(x => x.TCMSSignStatus).HasConversion<int>();
        b.Entity<PaymentStorage>().Property(x => x.HTVSignStatus).HasConversion<int>();
        b.Entity<PaymentStorage>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentStorageId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentStorageDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentStorageDetail>().HasIndex(x => x.PaymentStorageId);
        b.Entity<PaymentStorageDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<GuaranteeExtensionDispatch>().HasIndex(x => new { x.OrgId, x.DispatchNo }).IsUnique();
        b.Entity<GuaranteeExtensionDispatch>().HasIndex(x => new { x.OrgId, x.BankCode });
        b.Entity<GuaranteeExtensionDispatch>().Property(x => x.Status).HasConversion<int>();
        b.Entity<GuaranteeExtensionDispatch>().Property(x => x.SignCAStatus).HasConversion<int>();
        b.Entity<GuaranteeExtensionDispatch>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.DispatchId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<GuaranteeExtensionDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<GuaranteeExtensionDetail>().HasIndex(x => x.DispatchId);
        b.Entity<GuaranteeExtensionDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<BankGuaranteeClaim>().HasIndex(x => new { x.OrgId, x.ClaimNo }).IsUnique();
        b.Entity<BankGuaranteeClaim>().HasIndex(x => new { x.OrgId, x.BankCode });
        b.Entity<BankGuaranteeClaim>().HasIndex(x => new { x.OrgId, x.DealerCode });
        b.Entity<BankGuaranteeClaim>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankGuaranteeClaim>().Property(x => x.SignCAStatus).HasConversion<int>();
        b.Entity<BankGuaranteeClaim>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<BankGuaranteeClaimDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankGuaranteeClaimDetail>().HasIndex(x => x.ClaimId);
        b.Entity<BankGuaranteeClaimDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<PaymentAVN>().HasIndex(x => new { x.OrgId, x.PaymentAVNNo }).IsUnique();
        b.Entity<PaymentAVN>().HasIndex(x => new { x.OrgId, x.PmtMonth });
        b.Entity<PaymentAVN>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentAVN>().Property(x => x.TCMSSignStatus).HasConversion<int>();
        b.Entity<PaymentAVN>().Property(x => x.HTVSignStatus).HasConversion<int>();
        b.Entity<PaymentAVN>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentAVNId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentAVNDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentAVNDetail>().HasIndex(x => x.PaymentAVNId);
        b.Entity<PaymentAVNDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<PaymentGPS>().HasIndex(x => new { x.OrgId, x.PaymentGPSNo }).IsUnique();
        b.Entity<PaymentGPS>().HasIndex(x => new { x.OrgId, x.PmtMonth });
        b.Entity<PaymentGPS>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentGPS>().Property(x => x.HTVSignStatus).HasConversion<int>();
        b.Entity<PaymentGPS>().Property(x => x.TCMSSignStatus).HasConversion<int>();
        b.Entity<PaymentGPS>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentGPSId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PaymentGPSDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<PaymentGPSDetail>().HasIndex(x => x.PaymentGPSId);
        b.Entity<PaymentGPSDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<UnitPriceGPS>().HasIndex(x => new { x.OrgId, x.ContractNo }).IsUnique();

        b.Entity<FinancialExpenseStatement>().HasIndex(x => new { x.OrgId, x.CaNo }).IsUnique();
        b.Entity<FinancialExpenseStatement>().HasIndex(x => new { x.OrgId, x.DealerCode });
        b.Entity<FinancialExpenseStatement>().Property(x => x.Status).HasConversion<int>();
        b.Entity<FinancialExpenseStatement>().Property(x => x.DlrSignStatus).HasConversion<int>();
        b.Entity<FinancialExpenseStatement>().Property(x => x.HTCSignStatus).HasConversion<int>();
        b.Entity<FinancialExpenseStatement>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.StatementId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<FinancialExpenseDetail>().Property(x => x.AssemblyType).HasConversion<int>();
        b.Entity<FinancialExpenseDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<FinancialExpenseDetail>().HasIndex(x => x.StatementId);
        b.Entity<FinancialExpenseDetail>().HasIndex(x => new { x.OrgId, x.VIN });

        b.Entity<BankingDisbursementRequest>().HasIndex(x => new { x.OrgId, x.TransNo }).IsUnique();
        b.Entity<BankingDisbursementRequest>().HasIndex(x => new { x.OrgId, x.DealerCode });
        b.Entity<BankingDisbursementRequest>().HasIndex(x => new { x.OrgId, x.BankCode });
        b.Entity<BankingDisbursementRequest>().Property(x => x.TransType).HasConversion<int>();
        b.Entity<BankingDisbursementRequest>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BankingDisbursementRequest>().Property(x => x.BankStatus).HasConversion<int>();
        b.Entity<BankingDisbursementRequest>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<BankingDisbursementRequest>().HasMany(x => x.BankFiles).WithOne().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<BankingDisbursementDetail>().Property(x => x.AssemblyType).HasConversion<int>();
        b.Entity<BankingDisbursementDetail>().HasIndex(x => x.RequestId);
        b.Entity<BankingDisbursementDetail>().HasIndex(x => new { x.OrgId, x.DlrCtrNo });

        b.Entity<BankingDisbursementFile>().Property(x => x.DocType).HasConversion<int>();
        b.Entity<BankingDisbursementFile>().Property(x => x.SignStatus).HasConversion<int>();
        b.Entity<BankingDisbursementFile>().HasIndex(x => x.RequestId);

        b.Entity<ContractBankMDCancel>().HasIndex(x => new { x.OrgId, x.CancelBankMDNo }).IsUnique();
        b.Entity<ContractBankMDCancel>().HasIndex(x => new { x.OrgId, x.DlrCtrNo });
        b.Entity<ContractBankMDCancel>().HasIndex(x => new { x.OrgId, x.DealerCode });
        b.Entity<ContractBankMDCancel>().HasIndex(x => new { x.OrgId, x.BankCodeMD });
        b.Entity<ContractBankMDCancel>().Property(x => x.GuaranteeType).HasConversion<int>();
        b.Entity<ContractBankMDCancel>().Property(x => x.ReasonType).HasConversion<int>();
        b.Entity<ContractBankMDCancel>().Property(x => x.Status).HasConversion<int>();
        b.Entity<ContractBankMDCancel>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.CancelBankMDId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<ContractBankMDCancelDetail>().Property(x => x.Status).HasConversion<int>();
        b.Entity<ContractBankMDCancelDetail>().HasIndex(x => x.CancelBankMDId);
        b.Entity<ContractBankMDCancelDetail>().HasIndex(x => new { x.OrgId, x.VIN });
        b.Entity<ContractBankMDCancelDetail>().HasIndex(x => new { x.OrgId, x.CancelBankMDNo });

        b.Entity<InsuranceClaimDebit>().HasIndex(x => new { x.OrgId, x.DebitNo }).IsUnique();
        b.Entity<InsuranceClaimDebit>().HasIndex(x => new { x.OrgId, x.InsNo });
        b.Entity<InsuranceClaimDebit>().HasIndex(x => new { x.OrgId, x.RONo });
        b.Entity<InsuranceClaimDebit>().HasIndex(x => new { x.OrgId, x.VIN });
        b.Entity<InsuranceClaimDebit>().Property(x => x.Status).HasConversion<int>();

        b.Entity<InsurancePayment>().HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
        b.Entity<InsurancePayment>().HasIndex(x => new { x.OrgId, x.InsNo });
        b.Entity<InsurancePayment>().Property(x => x.PaymentMethod).HasConversion<int>();
        b.Entity<InsurancePayment>().Property(x => x.Status).HasConversion<int>();
        b.Entity<InsurancePayment>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<InsurancePaymentDetail>().HasIndex(x => x.PaymentId);
        b.Entity<InsurancePaymentDetail>().HasIndex(x => x.DebitId);
        b.Entity<InsurancePaymentDetail>().HasIndex(x => new { x.OrgId, x.RONo });

        b.Entity<SupplierDebit>().HasIndex(x => new { x.OrgId, x.DebitNo }).IsUnique();
        b.Entity<SupplierDebit>().HasIndex(x => new { x.OrgId, x.SupplierCode });
        b.Entity<SupplierDebit>().HasIndex(x => new { x.OrgId, x.StockInNo });
        b.Entity<SupplierDebit>().Property(x => x.Status).HasConversion<int>();

        b.Entity<SupplierPayment>().HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
        b.Entity<SupplierPayment>().HasIndex(x => new { x.OrgId, x.SupplierCode });
        b.Entity<SupplierPayment>().Property(x => x.PaymentMethod).HasConversion<int>();
        b.Entity<SupplierPayment>().Property(x => x.Status).HasConversion<int>();
        b.Entity<SupplierPayment>().HasMany(x => x.Details).WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<SupplierPaymentDetail>().HasIndex(x => x.PaymentId);
        b.Entity<SupplierPaymentDetail>().HasIndex(x => x.DebitId);
        b.Entity<SupplierPaymentDetail>().HasIndex(x => new { x.OrgId, x.StockInNo });
    }
}
