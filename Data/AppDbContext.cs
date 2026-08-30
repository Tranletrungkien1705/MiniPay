using Microsoft.EntityFrameworkCore;
using MiniPay.Models;

namespace MiniPay.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<PaymentIntent> Payments => Set<PaymentIntent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<PaymentIntent>().HasIndex(x => x.TxnRef).IsUnique();
        b.Entity<PaymentIntent>().Property(x => x.Status).HasConversion<int>();
    }
}
