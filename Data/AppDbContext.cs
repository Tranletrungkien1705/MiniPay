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
    }
}
