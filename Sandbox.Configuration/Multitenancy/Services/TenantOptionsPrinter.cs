using Microsoft.Extensions.Options;

namespace Sandbox.Configuration.Multitenancy.Services;

public interface ITenantOptionsPrinter
{
    void PrintOptions();
}

public class TenantOptionsPrinter(IOptions<ApplicationOptions> options) : ITenantOptionsPrinter
{
    public void PrintOptions()
    {
        Console.WriteLine(new string('-', 30));
        Console.WriteLine($"ProjectId: {options.Value.ProjectId}");
        Console.WriteLine($"Project name: {options.Value.SystemName}");
        Console.WriteLine($"Deployment: {options.Value.Deployment}");
        Console.WriteLine(new string('-', 30));
        
    }
}