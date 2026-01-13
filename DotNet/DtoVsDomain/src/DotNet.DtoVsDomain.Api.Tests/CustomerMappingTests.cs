using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Mappings;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class CustomerMappingTests
{
    [Fact]
    public void Customer_ToCustomerAggregate_Rehydrate_Correctly()
    {
        var c = new Customer { Id = Guid.NewGuid(), Name = "Alice", Email = "a@test" };
        var agg = c.ToCustomerAggregate();
        Assert.Equal(c.Id, agg.CustomerId);
        Assert.Equal(c.Name, agg.FullName);
        Assert.Equal(c.Email, agg.ContactEmail);
    }

    [Fact]
    public void CustomerAggregate_ToPersistence_Works()
    {
        var agg = CustomerAggregate.CreateNew("Bob", "b@test");
        var ent = agg.ToPersistence("tenant1");
        Assert.Equal(agg.CustomerId, ent.Id);
        Assert.Equal("tenant1", ent.TenantId);
        Assert.Equal(agg.FullName, ent.Name);
    }
}