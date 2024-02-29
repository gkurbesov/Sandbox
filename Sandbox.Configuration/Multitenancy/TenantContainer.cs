namespace Sandbox.Configuration.Multitenancy;

public interface ITenantContainer
{
    string TenantId { get; }

    void SetGlobalServices(IGlobalServiceResolver globalServiceResolver);

    IServiceCollection GetTenantServices();

    IServiceProvider GetTenantProvider();
}

public class TenantContainer(string tenantId) : ITenantContainer
{
    public string TenantId { get; } = tenantId;
    private IServiceCollection? _services;
    private IServiceProvider? _tenantProvider;

    public void SetGlobalServices(IGlobalServiceResolver globalServiceResolver)
    {
        if (_services != null)
        {
            throw new InvalidOperationException("Global services are already set");
        }

        _services = new ServiceCollection();
        _services.AddSingleton<ITenantProvider>(new TenantProvider(tenantId));
        foreach (var globalService in globalServiceResolver.GetGlobalServices())
        {
            _services.Add(new ServiceDescriptor(
                globalService.ServiceType,
                _ => globalServiceResolver.GetRequiredService(globalService.ServiceType), globalService.Lifetime));
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