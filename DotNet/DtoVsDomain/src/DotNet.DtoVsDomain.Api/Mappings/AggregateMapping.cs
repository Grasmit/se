using DotNet.DtoVsDomain.Api.Contracts.Responses;
using DotNet.DtoVsDomain.Api.Domain.Aggregates;

namespace DotNet.DtoVsDomain.Api.Mappings;

public static class AggregateMapping
{
    public static OrderResponse ToResponse(this OrderAggregate order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = string.Empty, // Repository can include richer info if needed
            OrderedAtUtc = order.OrderedAtUtc,
            Status = order.Status.ToString(),
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}