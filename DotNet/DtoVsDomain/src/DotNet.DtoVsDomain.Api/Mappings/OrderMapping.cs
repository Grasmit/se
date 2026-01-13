using DotNet.DtoVsDomain.Api.Contracts.Responses;
using DotNet.DtoVsDomain.Api.Domain.Entities;

namespace DotNet.DtoVsDomain.Api.Mappings;

public static class OrderMapping
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? string.Empty,
            OrderedAtUtc = order.OrderedAtUtc,
            Status = order.Status.ToString(),
            Total = order.Total,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }
}
