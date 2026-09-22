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
    }
}
