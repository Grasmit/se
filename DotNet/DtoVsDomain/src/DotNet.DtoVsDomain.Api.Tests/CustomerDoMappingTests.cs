using DotNet.DtoVsDomain.Api.Domain.Dos;
using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Mappings;
using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class CustomerDoMappingTests
{
    [Fact]
    public void Customer_ToDomainDo_Rehydrate_Correctly()
    {
        var c = new Customer { Id = Guid.NewGuid(), Name = "Alice", Email = "a@test" };
        var d = c.ToDomainDo();
        Assert.Equal(c.Id, d.CustomerId);
        Assert.Equal(c.Name, d.FullName);
        Assert.Equal(c.Email, d.ContactEmail);
    }

    [Fact]
    public void CustomerDo_ApplyTo_Updates_PersistenceEntity()
    {
        var ent = new Customer { Id = Guid.NewGuid(), Name = "Old", Email = "old@test", TenantId = "acme" };
        var d = CustomerDo.CreateNew("New Name", "new@test");
        d.ChangeEmail("updated@test");

        d.ApplyTo(ent);

        Assert.Equal("New Name", ent.Name);
        Assert.Equal("updated@test", ent.Email);
    }

    [Fact]
    public async Task Roundtrip_ReadModifyPersist_Works()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options, new DummyTenantProvider());

        var ent = new Customer { Id = Guid.NewGuid(), Name = "Alice", Email = "a@acme", TenantId = "acme" };
        context.Customers.Add(ent);
        await context.SaveChangesAsync();

        // Read and map to DO
        var loaded = await context.Customers.FirstAsync();
        var d = loaded.ToDomainDo();

        // Apply domain change
        d.ChangeEmail("alice@new");

        // Map back to tracked entity then persist
        var tracked = await context.Customers.FirstAsync(c => c.Id == d.CustomerId);
        d.ApplyTo(tracked);
        await context.SaveChangesAsync();

        // Reload and assert
        var reloaded = await context.Customers.FirstAsync(c => c.Id == d.CustomerId);
        Assert.Equal("alice@new", reloaded.Email);
    }

    private class DummyTenantProvider : ITenantProvider
    {
        public string CurrentTenant => "acme";
        public string ResolveTenantOrThrow() => "acme";
    }
}