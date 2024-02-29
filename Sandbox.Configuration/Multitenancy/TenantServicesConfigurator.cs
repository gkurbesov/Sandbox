using Sandbox.Configuration.Multitenancy.Services;

namespace Sandbox.Configuration.Multitenancy;

public interface ITenantServicesConfigurator
{
    void ConfigureService(IServiceCollection services, IConfiguration configuration);
}

public class TenantServicesConfigurator : ITenantServicesConfigurator
{
    public void ConfigureService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITenantedService, TenantedService>();
        services.AddTransient<ITenantOptionsPrinter, TenantOptionsPrinter>();
        services.AddOptions<ApplicationOptions>()
            .Bind(configuration.GetSection(ApplicationOptions.SectionName));
    }
}