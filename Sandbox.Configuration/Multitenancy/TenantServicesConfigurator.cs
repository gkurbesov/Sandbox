using Sandbox.Configuration.Multitenancy.Services;

namespace Sandbox.Configuration.Multitenancy;

public interface ITenantServicesConfigurator
{
    void ConfigureService(IServiceCollection services);
}

public class TenantServicesConfigurator : ITenantServicesConfigurator
{
    public void ConfigureService(IServiceCollection services)
    {
        services.AddSingleton<ITenantedService, TenantedService>();
    }
}