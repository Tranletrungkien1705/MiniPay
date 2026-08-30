using Microsoft.EntityFrameworkCore;
using MiniPay.Models;

namespace MiniPay.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Merchant", ApiKey = "demo-pay" });
            await db.SaveChangesAsync();
        }
    }
}
