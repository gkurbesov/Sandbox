namespace Sandbox.Configuration.Multitenancy;

public interface IGlobalServiceResolver
{
    IEnumerable<ServiceDescriptor> GetGlobalServices();
    object GetRequiredService(Type serviceType);
}

public class GlobalServiceResolver(IServiceCollection serviceCollection, IServiceProvider globalServiceProvider)
    : IGlobalServiceResolver
{
    public IEnumerable<ServiceDescriptor> GetGlobalServices() => serviceCollection;

    public object GetRequiredService(Type serviceType) => globalServiceProvider.GetRequiredService(serviceType);
}