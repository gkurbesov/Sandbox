using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;

namespace Sandbox.EfCore;

public class ContainerManagerBuilder
{
    private Builder _builder = new();

    public ContainerManagerBuilder WithTenant(string tenant, int port)
    {
        _builder.UseContainer()
            .WithName($"{tenant}-SqlServer")
            .ReuseIfExists()
            .UseImage("mcr.microsoft.com/mssql/server:latest")
            .ExposePort(port, 1433)
            .WaitForPort("1433/tcp", 30000, "127.0.0.1")
            .WithEnvironment("ACCEPT_EULA=Y", $"SA_PASSWORD=P@ssword!{tenant}");

        return this;
    }
    
    public ContainerManager Build()
    {
        return new(_builder.Build());
    }
    
}

public class ContainerManager(ICompositeService container) : IDisposable
{
    public void Start() => container.Start();

    public void Dispose()
    {
        container.Stop();
        container.Dispose();
    }
}