using Microsoft.EntityFrameworkCore;

namespace ColdChain.Gateway;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public void Seed()
    {
        if (Tenants.Any())
            return;

        Tenants.AddRange(
            new Tenant { Id = "pharma", Name = "Pharma", MaxTemperatureC = 4 },
            new Tenant { Id = "frozen", Name = "Frozen Food", MaxTemperatureC = 2 });
        SaveChanges();
    }
}
