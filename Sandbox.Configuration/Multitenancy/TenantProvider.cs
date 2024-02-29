namespace Sandbox.Configuration.Multitenancy;

public interface ITenantProvider
{
    string TenantId { get; }
}

public class TenantProvider(string tenantId) : ITenantProvider
{
    public string TenantId { get; } = tenantId;
}