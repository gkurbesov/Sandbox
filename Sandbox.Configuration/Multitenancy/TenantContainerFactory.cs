namespace Sandbox.Configuration.Multitenancy;

public interface ITenantContainerFactory
{
    ITenantContainer GetTenantContainer(string tenantId);
}

public class TenantContainerFactory(
    ServiceCollection globalServices,
    IServiceProvider globalServiceProvider,
    ITenantServicesConfigurator servicesConfigurator)
    : ITenantContainerFactory
{
    private readonly Dictionary<string, ITenantContainer> _tenantContainers = new();

    public ITenantContainer GetTenantContainer(string tenantId)
    {
        if (!_tenantContainers.TryGetValue(tenantId, out var tenantContainer))
        {
            tenantContainer = new TenantContainer(tenantId);
            tenantContainer.SetGlobalServices(globalServices, globalServiceProvider);
            servicesConfigurator.ConfigureService(tenantContainer.GetTenantServices());
            _tenantContainers[tenantId] = tenantContainer;
        }

        return tenantContainer;
    }
}