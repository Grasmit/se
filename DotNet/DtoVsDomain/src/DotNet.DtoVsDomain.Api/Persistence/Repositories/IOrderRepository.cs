using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Domain.Entities;

namespace DotNet.DtoVsDomain.Api.Persistence.Repositories;

public interface IOrderRepository
{
    Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<OrderAggregate> Orders, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);

    // Lookup helpers useful for service-level validations
    Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task<OrderAggregate> AddAsync(OrderAggregate aggregate, CancellationToken cancellationToken);
    Task UpdateAsync(OrderAggregate aggregate, CancellationToken cancellationToken);
}