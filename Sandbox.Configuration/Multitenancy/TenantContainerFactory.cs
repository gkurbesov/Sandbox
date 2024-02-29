using Sandbox.Configuration.JsonTenantedConfiguration;

namespace Sandbox.Configuration.Multitenancy;

public interface ITenantContainerFactory
{
    ITenantContainer GetTenantContainer(string tenantId);
}

public class TenantContainerFactory(
    IGlobalServiceResolver globalServiceResolver,
    ITenantServicesConfigurator servicesConfigurator,
    IConfiguration mainConfiguration)
    : ITenantContainerFactory
{
    private readonly Dictionary<string, ITenantContainer> _tenantContainers = new();

    public ITenantContainer GetTenantContainer(string tenantId)
    {
        if (!_tenantContainers.TryGetValue(tenantId, out var tenantContainer))
        {
            tenantContainer = new TenantContainer(tenantId);
            tenantContainer.SetGlobalServices(globalServiceResolver);

            var services = tenantContainer.GetTenantServices();
            var tenantConfiguration = CreateTenantConfiguration(services, tenantId);
            services.AddSingleton(tenantConfiguration);

            servicesConfigurator.ConfigureService(services, tenantConfiguration);

            _tenantContainers[tenantId] = tenantContainer;
        }

        return tenantContainer;
    }

    private IConfiguration CreateTenantConfiguration(IServiceCollection services, string tenantId)
    {
        var tenantConfiguration = new ConfigurationBuilder()
            .AddConfiguration(mainConfiguration)
            .AddTenantedJsonFile("tenants.appsettings.json", tenantId)
            .Build();

        return tenantConfiguration;
    }
}