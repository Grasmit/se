using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Persistence.Repositories;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class CustomerRepositoryTests
{
    private class DummyTenantProvider : ITenantProvider
    {
        public string CurrentTenant => "acme";
        public string ResolveTenantOrThrow() => "acme";
    }

    [Fact]
    public async Task GetAll_Returns_Aggregates()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options, new DummyTenantProvider());
        context.Customers.Add(new Customer { TenantId = "acme", Name = "Alice", Email = "a@acme" });
        context.Customers.Add(new Customer { TenantId = "acme", Name = "Bob", Email = "b@acme" });
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        var result = await repo.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.FullName == "Alice");
    }
}