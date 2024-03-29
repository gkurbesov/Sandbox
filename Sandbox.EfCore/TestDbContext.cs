using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Sandbox.EfCore;

public class TestDbContext(ITenantService tenantService) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(tenantService.GetConnectionString());
    }
}