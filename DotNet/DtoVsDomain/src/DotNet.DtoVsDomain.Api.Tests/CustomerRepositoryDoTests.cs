using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class CustomerRepositoryDoTests
{
    [Fact]
    public async Task GetAllDoAsync_Returns_CustomerDo_List()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options, new DummyTenantProvider());

        context.Customers.Add(new DotNet.DtoVsDomain.Api.Domain.Entities.Customer { TenantId = "acme", Name = "Alice", Email = "a@acme" });
        context.Customers.Add(new DotNet.DtoVsDomain.Api.Domain.Entities.Customer { TenantId = "acme", Name = "Bob", Email = "b@acme" });
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        var list = await repo.GetAllDoAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Contains(list, c => c.FullName == "Alice");
    }

    [Fact]
    public async Task AddDoAsync_Persists_And_Returns_Do()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options, new DummyTenantProvider());

        var repo = new CustomerRepository(context);
        var dto = DotNet.DtoVsDomain.Api.Domain.Dos.CustomerDo.CreateNew("Charlie", "c@acme");

        var saved = await repo.AddDoAsync(dto, "acme", CancellationToken.None);

        Assert.Equal("Charlie", saved.FullName);
        Assert.Equal("c@acme", saved.ContactEmail);
    }

    private class DummyTenantProvider : DotNet.DtoVsDomain.Api.Services.Tenancy.ITenantProvider
    {
        public string CurrentTenant => "acme"; public string ResolveTenantOrThrow() => "acme";
    }
}