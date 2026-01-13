using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CustomerAggregate>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await _db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        return customers.Select(c => c.ToCustomerAggregate()).ToList();
    }

    public async Task<CustomerAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return c?.ToCustomerAggregate();
    }

    public async Task<CustomerAggregate> AddAsync(CustomerAggregate aggregate, string tenantId, CancellationToken cancellationToken)
    {
        var ent = aggregate.ToPersistence(tenantId);
        _db.Customers.Add(ent);
        await _db.SaveChangesAsync(cancellationToken);
        return ent.ToCustomerAggregate();
    }

    public async Task UpdateAsync(CustomerAggregate aggregate, CancellationToken cancellationToken)
    {
        var ent = await _db.Customers.FirstOrDefaultAsync(x => x.Id == aggregate.CustomerId, cancellationToken);
        if (ent == null) throw new KeyNotFoundException("Customer not found");

        ent.Name = aggregate.FullName;
        ent.Email = aggregate.ContactEmail;
        await _db.SaveChangesAsync(cancellationToken);
    }

    // --- DO-oriented implementations (mapper-only style) ---
    public async Task<IReadOnlyList<Domain.Dos.CustomerDo>> GetAllDoAsync(CancellationToken cancellationToken)
    {
        var customers = await _db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        return customers.Select(c => c.ToDomainDo()).ToList();
    }

    public async Task<Domain.Dos.CustomerDo?> GetByIdDoAsync(Guid id, CancellationToken cancellationToken)
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return c?.ToDomainDo();
    }

    public async Task<Domain.Dos.CustomerDo> AddDoAsync(Domain.Dos.CustomerDo dto, string tenantId, CancellationToken cancellationToken)
    {
        var ent = dto.ToPersistence(tenantId);
        _db.Customers.Add(ent);
        await _db.SaveChangesAsync(cancellationToken);
        return ent.ToDomainDo();
    }

    public async Task UpdateDoAsync(Domain.Dos.CustomerDo dto, CancellationToken cancellationToken)
    {
        var ent = await _db.Customers.FirstOrDefaultAsync(x => x.Id == dto.CustomerId, cancellationToken);
        if (ent == null) throw new KeyNotFoundException("Customer not found");
        dto.ApplyTo(ent);
        await _db.SaveChangesAsync(cancellationToken);
    }
}