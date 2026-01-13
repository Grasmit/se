namespace DotNet.DtoVsDomain.Api.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public IReadOnlyCollection<OrderItem> Items => _items;

    public decimal Total => _items.Sum(i => i.LineTotal);

    public void AddItem(Guid productId, int quantity, decimal unitPrice, string tenantId)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be positive.");
        }

        _items.Add(new OrderItem
        {
            TenantId = tenantId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        });
    }

    public void Submit()
    {
        if (!_items.Any())
        {
            throw new InvalidOperationException("Order cannot be submitted without items.");
        }

        Status = OrderStatus.Submitted;
    }
}
