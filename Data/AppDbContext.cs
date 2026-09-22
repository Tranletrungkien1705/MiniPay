using Microsoft.EntityFrameworkCore;
using MiniPay.Models;

namespace MiniPay.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<PaymentIntent> Payments => Set<PaymentIntent>();
    public DbSet<ReconcileBatch> ReconcileBatches => Set<ReconcileBatch>();
    public DbSet<ReconcileDetail> ReconcileDetails => Set<ReconcileDetail>();

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
    }
}
