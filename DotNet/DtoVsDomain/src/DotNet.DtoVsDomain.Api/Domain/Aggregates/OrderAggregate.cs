using DotNet.DtoVsDomain.Api.Domain.Entities;

namespace DotNet.DtoVsDomain.Api.Domain.Aggregates;

// Pure domain aggregate (not EF-tracked). Use this when you want to operate on a DO
// that is detached from the persistence model and contains business behavior.
public class OrderAggregate
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public DateTime OrderedAtUtc { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItemAggregate> _items = new();
    public IReadOnlyList<OrderItemAggregate> Items => _items;

    public decimal Total => _items.Sum(i => i.LineTotal);

    private OrderAggregate() { }

    // Factory to create a new aggregate (not persisted yet)
    public static OrderAggregate CreateNew(Guid customerId, string tenantId)
    {
        return new OrderAggregate
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TenantId = tenantId,
            OrderedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Draft
        };
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        // reuse domain validation
        var item = new OrderItemAggregate(productId, quantity, unitPrice);
        _items.Add(item);
    }

    public static OrderAggregate Rehydrate(Order persistenceOrder)
    {
        if (persistenceOrder is null) throw new ArgumentNullException(nameof(persistenceOrder));

        var agg = new OrderAggregate
        {
            Id = persistenceOrder.Id,
            CustomerId = persistenceOrder.CustomerId,
            TenantId = persistenceOrder.TenantId,
            OrderedAtUtc = persistenceOrder.OrderedAtUtc,
            Status = persistenceOrder.Status
        };

        foreach (var item in persistenceOrder.Items)
        {
            agg._items.Add(new OrderItemAggregate(item.ProductId, item.Quantity, item.UnitPrice));
        }

        return agg;
    }

    public void ApplyPercentageDiscount(decimal percent)
    {
        if (percent <= 0 || percent >= 100) throw new InvalidOperationException("Discount percent must be between 0 and 100.");
        if (Status != OrderStatus.Submitted) throw new InvalidOperationException("Discounts may only be applied to submitted orders.");

        foreach (var item in _items)
        {
            item.ApplyPercentageDiscount(percent);
        }
    }

    public void Submit()
    {
        if (!_items.Any()) throw new InvalidOperationException("Order cannot be submitted without items.");
        Status = OrderStatus.Submitted;
    }
}

public class OrderItemAggregate
{
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public OrderItemAggregate(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new InvalidOperationException("Quantity must be positive.");
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void ApplyPercentageDiscount(decimal percent)
    {
        UnitPrice = Math.Round(UnitPrice * (1 - percent / 100m), 2);
    }
}
