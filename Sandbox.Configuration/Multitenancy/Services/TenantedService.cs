namespace Sandbox.Configuration.Multitenancy.Services;

public interface ITenantedService
{
    string GetStateValue();
}

public class TenantedService : ITenantedService
{
    private readonly ITenantProvider _tenantProvider;
    private readonly string _stateValue = Random.Shared.Next().ToString();

    public TenantedService(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"TenantedService for tenant {tenantProvider.TenantId} created");
        Console.ResetColor();
    }

    public string GetStateValue() => $"{_tenantProvider.TenantId}: State value for tenant is {_stateValue}";
}