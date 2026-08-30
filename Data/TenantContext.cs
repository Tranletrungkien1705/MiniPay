namespace MiniPay.Data;

/// <summary>Ngữ cảnh tenant (merchant). Middleware set OrgId qua header X-Api-Key / cookie org_key.</summary>
public interface ITenantContext { Guid OrgId { get; set; } }

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DefaultOrgId = new("55555555-5555-5555-5555-555555555555");
    public const string CookieName = "org_key";
    public Guid OrgId { get; set; } = DefaultOrgId;
}
