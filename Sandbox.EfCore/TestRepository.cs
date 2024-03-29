using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sandbox.EfCore;

public interface ITestRepository
{
    Task<object> ExecuteAsync(ITenantService tenantService);
}

public class TestRepository() : ITestRepository
{
    public async Task<object> ExecuteAsync(ITenantService tenantService)
    {
        var context = new TestDbContext(tenantService);
        await using var _ = context.ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        await context.Database.ExecuteSqlRawAsync("").ConfigureAwait(false);
        return new object();
    }
}