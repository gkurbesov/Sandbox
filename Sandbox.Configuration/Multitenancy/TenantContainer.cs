namespace Sandbox.Configuration.Multitenancy;

public interface ITenantContainer
{
    string TenantId { get; }

    void SetGlobalServices(IServiceCollection globalServices, IServiceProvider globalServiceProvider);

    IServiceCollection GetTenantServices();

    IServiceProvider GetTenantProvider();
}

public class TenantContainer(string tenantId) : ITenantContainer
{
    public string TenantId { get; } = tenantId;
    private IServiceCollection? _services;
    private IServiceProvider? _tenantProvider;

    public void SetGlobalServices(IServiceCollection globalServices, IServiceProvider globalServiceProvider)
    {
        if (_services != null)
        {
            throw new InvalidOperationException("Global services are already set");
        }

        _services = new ServiceCollection();
        _services.AddSingleton<ITenantProvider>(new TenantProvider(tenantId));
        foreach (var globalService in globalServices)
        {
            _services.Add(new ServiceDescriptor(
                globalService.ServiceType,
                _ => globalServiceProvider.GetRequiredService(globalService.ServiceType), globalService.Lifetime));
        }
    }

    public IServiceCollection GetTenantServices() =>
        _services ?? throw new InvalidOperationException("Set global services first");

    public IServiceProvider GetTenantProvider()
    {
        if (_services == null)
        {
            throw new InvalidOperationException("Global services are not set");
        }

        return _tenantProvider ??= _services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
}