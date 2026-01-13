namespace DotNet.DtoVsDomain.Api.Contracts.Requests;

public class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
