namespace DotNet.DtoVsDomain.Api.Contracts.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
