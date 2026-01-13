namespace DotNet.DtoVsDomain.Api.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Product? Product { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
