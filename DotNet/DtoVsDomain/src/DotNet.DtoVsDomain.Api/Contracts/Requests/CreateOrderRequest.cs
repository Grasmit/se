// DTO: Represents an API contract for creating an order. DTOs are simple shapes used at the boundary and do not contain business behavior.
namespace DotNet.DtoVsDomain.Api.Contracts.Requests;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
