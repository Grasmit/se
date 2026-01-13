using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .ThenInclude(c => c)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order?.ToDomain();
    }

    public async Task<(IReadOnlyList<OrderAggregate> Orders, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderedAtUtc);

        var total = await query.CountAsync(cancellationToken);

        var items = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.Select(o => o.ToDomain()).ToList(), total);
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    public async Task<OrderAggregate> AddAsync(OrderAggregate aggregate, CancellationToken cancellationToken)
    {
        // Map domain aggregate -> persistence entity
        var persistenceOrder = new Order
        {
            TenantId = aggregate.TenantId,
            CustomerId = aggregate.CustomerId,
            OrderedAtUtc = aggregate.OrderedAtUtc,
            Status = aggregate.Status
        };

        foreach (var it in aggregate.Items)
        {
            persistenceOrder.AddItem(it.ProductId, it.Quantity, it.UnitPrice, aggregate.TenantId);
        }

        _dbContext.Orders.Add(persistenceOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Rehydrate to return canonical domain object (with ids)
        await _dbContext.Entry(persistenceOrder).Collection(o => o.Items).LoadAsync(cancellationToken);
        return persistenceOrder.ToDomain();
    }

    public async Task UpdateAsync(OrderAggregate aggregate, CancellationToken cancellationToken)
    {
        var persistenceOrder = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == aggregate.Id, cancellationToken);

        if (persistenceOrder is null) throw new KeyNotFoundException("Order not found");

        // For this demo, sync unit prices and quantities by product id; a real implementation should be more robust.
        foreach (var domainItem in aggregate.Items)
        {
            var match = persistenceOrder.Items.FirstOrDefault(i => i.ProductId == domainItem.ProductId && i.Quantity == domainItem.Quantity);
            if (match != null)
            {
                match.UnitPrice = domainItem.UnitPrice;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}