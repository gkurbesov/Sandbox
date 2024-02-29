using Sandbox.Configuration.JsonTenantedConfiguration;

namespace Sandbox.Configuration.Multitenancy;

public interface ITenantContainerFactory
{
    ITenantContainer GetOrCreateTenantContainer(string tenantId);
}

public class TenantContainerFactory(
    IGlobalServiceResolver globalServiceResolver,
    ITenantServicesConfigurator servicesConfigurator,
    IConfiguration mainConfiguration)
    : ITenantContainerFactory
{
    private readonly Dictionary<string, ITenantContainer> _tenantContainers = new();

    public ITenantContainer GetOrCreateTenantContainer(string tenantId)
    {
        if (!_tenantContainers.TryGetValue(tenantId, out var tenantContainer))
        {
            tenantContainer = new TenantContainer(tenantId);
            tenantContainer.SetGlobalServices(globalServiceResolver);

            var services = tenantContainer.GetTenantServices();
            var tenantConfiguration = CreateTenantConfiguration(tenantId);
            services.AddSingleton(tenantConfiguration);

            servicesConfigurator.ConfigureService(services, tenantConfiguration);

            _tenantContainers[tenantId] = tenantContainer;
        }

        return tenantContainer;
    }

    private IConfiguration CreateTenantConfiguration(string tenantId)
    {
        var tenantConfiguration = new ConfigurationBuilder()
            .AddConfiguration(mainConfiguration)
            .AddTenantedJsonFile("tenants.appsettings.json", tenantId)
            .Build();

        return tenantConfiguration;
    }
}