using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Mappings;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class PersistenceMappingTests
{
    [Fact]
    public void ToDomain_Rehydrates_OrderAggregate()
    {
        var order = new Order { CustomerId = Guid.NewGuid(), TenantId = "acme" };
        order.AddItem(Guid.NewGuid(), 2, 10m, "acme");
        order.Submit();

        var agg = order.ToDomain();

        Assert.Equal(order.CustomerId, agg.CustomerId);
        Assert.Equal(order.Items.Count, agg.Items.Count);
        Assert.Equal(order.Total, agg.Total);
    }

    [Fact]
    public void ApplyPercentageDiscount_Modifies_DomainAndMappingBackWorks()
    {
        var order = new Order { CustomerId = Guid.NewGuid(), TenantId = "acme" };
        order.AddItem(Guid.NewGuid(), 2, 10m, "acme");
        order.Submit();

        var agg = order.ToDomain();
        agg.ApplyPercentageDiscount(10m); // 10% discount

        // Simulate mapping back to persistence
        foreach (var item in order.Items)
        {
            var updated = agg.Items.First(i => i.ProductId == item.ProductId);
            item.UnitPrice = updated.UnitPrice;
        }

        Assert.Equal(9.00m, order.Items.First().UnitPrice);
    }
}