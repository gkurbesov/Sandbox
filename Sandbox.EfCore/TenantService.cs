namespace Sandbox.EfCore;

public interface ITenantService
{
    string Tenant { get; }
    string GetConnectionString();
}

public class TenantService(string tenant, int sqlPort) : ITenantService
{
    public string Tenant { get; } = tenant;

    public string GetConnectionString()
    {
        return $"Server=localhost,{sqlPort};Database={tenant};User Id=sa;Password=P@ssword!{Tenant};TrustServerCertificate=true";
    }
}