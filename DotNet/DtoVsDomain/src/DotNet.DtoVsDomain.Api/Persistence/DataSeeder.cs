using DotNet.DtoVsDomain.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await dbContext.Tenants.AnyAsync(cancellationToken))
        {
            return;
        }

        var tenants = new[]
        {
            new Tenant { TenantId = "acme", Slug = "acme", Name = "Acme Corp" },
            new Tenant { TenantId = "globex", Slug = "globex", Name = "Globex" }
        };

        dbContext.Tenants.AddRange(tenants);

        dbContext.Customers.AddRange(
            new Customer { TenantId = "acme", Name = "Alice Adams", Email = "alice@acme.test" },
            new Customer { TenantId = "acme", Name = "Bob Brown", Email = "bob@acme.test" },
            new Customer { TenantId = "globex", Name = "Gina Gray", Email = "gina@globex.test" }
        );

        dbContext.Products.AddRange(
            new Product { TenantId = "acme", Name = "API Subscription", UnitPrice = 19.99m },
            new Product { TenantId = "acme", Name = "Dashboard Add-on", UnitPrice = 9.99m },
            new Product { TenantId = "globex", Name = "Data Export", UnitPrice = 29.00m }
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
