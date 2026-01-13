using DotNet.DtoVsDomain.Api.Contracts.Requests;
using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Services;

public class OrderService : IOrderService
{
    private readonly ITenantProvider _tenantProvider;
    private readonly Persistence.Repositories.IOrderRepository _repository;

    public OrderService(Persistence.Repositories.IOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderAggregate> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var tenant = _tenantProvider.ResolveTenantOrThrow();

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("At least one order item is required.");
        }

        var customer = await _repository.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new KeyNotFoundException("Customer not found for current tenant.");
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _repository.GetProductsByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Distinct().Count())
        {
            throw new KeyNotFoundException("One or more products do not exist for current tenant.");
        }

        // Build a pure domain aggregate and apply business rules before persisting
        var aggregate = Domain.Aggregates.OrderAggregate.CreateNew(request.CustomerId, tenant);

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            aggregate.AddItem(product.Id, item.Quantity, product.UnitPrice);
        }

        aggregate.Submit();

        var saved = await _repository.AddAsync(aggregate, cancellationToken);
        return saved;
    }

    public async Task<(IReadOnlyList<OrderAggregate> Orders, int Total)> GetAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _repository.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    // Example of converting persistence entity -> pure DO, applying business logic, and persisting changes via repository.
    public async Task ApplyPercentageDiscountAsync(Guid orderId, decimal percent, CancellationToken cancellationToken)
    {
        var domainOrder = await _repository.GetByIdAsync(orderId, cancellationToken);

        if (domainOrder == null)
        {
            throw new KeyNotFoundException("Order not found for current tenant.");
        }

        // Domain enforces business rules
        domainOrder.ApplyPercentageDiscount(percent);

        // Persist domain changes via repository
        await _repository.UpdateAsync(domainOrder, cancellationToken);
    }
}

