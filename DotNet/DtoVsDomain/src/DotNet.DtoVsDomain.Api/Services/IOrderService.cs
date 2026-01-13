using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Contracts.Requests;

namespace DotNet.DtoVsDomain.Api.Services;

public interface IOrderService
{
    Task<OrderAggregate> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<(IReadOnlyList<OrderAggregate> Orders, int Total)> GetAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task ApplyPercentageDiscountAsync(Guid orderId, decimal percent, CancellationToken cancellationToken);
}
