using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Sandbox.EfCore;

public class UnitOfWork
{
    private IConfiguration _configuration;
    private ConcurrentDictionary<string, ITestRepository> _repositories = new();
    private ConcurrentDictionary<string, ITenantService> _tenantServices = new();
    private Dictionary<string, (string tenant, int port)> _tenants = new();

    public UnitOfWork()
    {
        _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        FillTenants();
    }

    public string[] GetTenants() => _tenants.Keys.ToArray();


    public async Task DoWorkAsync(string tenant)
    {
        var tenantService = _tenantServices.GetOrAdd(tenant, _ => new TenantService(tenant, _tenants[tenant].port));
        var repository = _repositories.GetOrAdd(tenant, _ => new TestRepository());
        await repository.ExecuteAsync(tenantService).ConfigureAwait(false);
    }


    private void FillTenants()
    {
        var count = 100;
        var port = 1430;

        for (int i = 0; i < count; i++)
        {
            var tenat = $"Tenant{i}{i:X4}";
            _tenants.Add(tenat, (tenat, port++));
        }
    }
}