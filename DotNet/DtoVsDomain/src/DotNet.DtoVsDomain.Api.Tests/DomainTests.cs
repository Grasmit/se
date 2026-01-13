using DotNet.DtoVsDomain.Api.Domain.Entities;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class DomainTests
{
    [Fact]
    public void Order_AddItem_Throws_OnNonPositiveQuantity()
    {
        var order = new Order();
        Assert.Throws<InvalidOperationException>(() => order.AddItem(Guid.NewGuid(), 0, 10m, "acme"));
    }
}