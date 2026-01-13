using DotNet.DtoVsDomain.Api.Domain.Aggregates;

namespace DotNet.DtoVsDomain.Api.Persistence.Repositories;

public interface ICustomerRepository
{
    // Aggregate-oriented methods
    Task<IReadOnlyList<CustomerAggregate>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerAggregate> AddAsync(CustomerAggregate aggregate, string tenantId, CancellationToken cancellationToken);
    Task UpdateAsync(CustomerAggregate aggregate, CancellationToken cancellationToken);

    // DO-oriented methods (mapper-only approach returning simple domain objects)
    Task<IReadOnlyList<Domain.Dos.CustomerDo>> GetAllDoAsync(CancellationToken cancellationToken);
    Task<Domain.Dos.CustomerDo?> GetByIdDoAsync(Guid id, CancellationToken cancellationToken);
    Task<Domain.Dos.CustomerDo> AddDoAsync(Domain.Dos.CustomerDo dto, string tenantId, CancellationToken cancellationToken);
    Task UpdateDoAsync(Domain.Dos.CustomerDo dto, CancellationToken cancellationToken);
}