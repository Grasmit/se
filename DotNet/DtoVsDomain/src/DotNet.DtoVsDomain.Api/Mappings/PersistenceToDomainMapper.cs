using DotNet.DtoVsDomain.Api.Domain.Aggregates;
using DotNet.DtoVsDomain.Api.Domain.Entities;

namespace DotNet.DtoVsDomain.Api.Mappings;

/// <summary>
/// Mapping helpers converting persistence entities to domain objects (aggregates or simple DOs).
/// These are extension methods — call them like: <c>customer.ToCustomerDo()</c> or <c>order.ToDomain()</c>.
/// <para>
/// Note: <c>ToDomain()</c> is a historical name used for orders and some other mappings. Prefer
/// explicit names like <c>ToCustomerAggregate()</c> or <c>ToCustomerDo()</c> to make intent clear.
/// </para>
/// </summary>
public static class PersistenceToDomainMapper
{
    // Convert EF-tracked persistence entity to a pure DO (aggregate) for business operations
    public static OrderAggregate ToDomain(this Order persistenceOrder)
    {
        return OrderAggregate.Rehydrate(persistenceOrder);
    }

    /// <summary>
    /// Convert persistence Customer entity to the domain aggregate (CustomerAggregate).
    /// Use when you need aggregate semantics.
    /// </summary>
    public static Domain.Aggregates.CustomerAggregate ToCustomerAggregate(this Customer customer)
    {
        return Domain.Aggregates.CustomerAggregate.Rehydrate(customer.Id, customer.Name, customer.Email);
    }

    /// <summary>
    /// Convert persistence Customer entity to a lightweight Domain Object (CustomerDo).
    /// Use when you only need a simple domain object for service operations.
    /// </summary>
    public static Domain.Dos.CustomerDo ToCustomerDo(this Customer customer)
    {
        return new Domain.Dos.CustomerDo
        {
            CustomerId = customer.Id,
            FullName = customer.Name,
            ContactEmail = customer.Email
        };
    }

    // Backwards-compatible alias for code that called ToDomain() on Customer; prefer explicit names above.
    public static Domain.Aggregates.CustomerAggregate ToDomain(this Customer customer)
        => ToCustomerAggregate(customer);

    // Customer domain -> persistence mapping helper (aggregate)
    public static Customer ToPersistence(this Domain.Aggregates.CustomerAggregate agg, string tenantId)
    {
        return new Customer
        {
            Id = agg.CustomerId,
            Name = agg.FullName,
            Email = agg.ContactEmail,
            TenantId = tenantId
        };
    }

    /// <summary>
    /// CustomerDo -> persistence mapping helper (for inserts). For updates prefer ApplyTo on an existing tracked entity.
    /// </summary>
    public static Customer ToPersistence(this Domain.Dos.CustomerDo agg, string tenantId)
    {
        return new Customer
        {
            Id = agg.CustomerId,
            Name = agg.FullName,
            Email = agg.ContactEmail,
            TenantId = tenantId
        };
    }
}
