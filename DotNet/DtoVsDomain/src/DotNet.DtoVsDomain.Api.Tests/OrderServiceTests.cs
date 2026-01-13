using System.Threading;
using System.Threading.Tasks;
using DotNet.DtoVsDomain.Api.Contracts.Requests;
using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Services;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class OrderServiceTests
{
    private class TestTenantProvider : ITenantProvider
    {
        private readonly string _tenant;
        public TestTenantProvider(string tenant) => _tenant = tenant;
        public string CurrentTenant => _tenant;
        public string ResolveTenantOrThrow() => _tenant;
    }

    [Fact]
    public async Task CreateAsync_Creates_Order_WithCorrectTotalAndTenant()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new TestTenantProvider("acme");

        await using var context = new AppDbContext(options, tenantProvider);

        var repository = new DotNet.DtoVsDomain.Api.Persistence.Repositories.OrderRepository(context);
        var service = new OrderService(repository, tenantProvider);

        var customer = new DotNet.DtoVsDomain.Api.Domain.Entities.Customer { TenantId = "acme", Name = "Test", Email = "t@test" };
        var product = new DotNet.DtoVsDomain.Api.Domain.Entities.Product { TenantId = "acme", Name = "P", UnitPrice = 5m };
        context.Customers.Add(customer);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var req = new CreateOrderRequest { CustomerId = customer.Id, Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 2 } } };

        var order = await service.CreateAsync(req, CancellationToken.None);

        Assert.Equal(10m, order.Total);
        Assert.Equal("acme", order.TenantId);
    }
}