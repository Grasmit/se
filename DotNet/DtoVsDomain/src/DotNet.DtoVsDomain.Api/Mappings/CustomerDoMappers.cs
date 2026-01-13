using DotNet.DtoVsDomain.Api.Domain.Dos;
using DotNet.DtoVsDomain.Api.Domain.Entities;

namespace DotNet.DtoVsDomain.Api.Mappings;

public static class CustomerDoMappers
{
    // Persistence -> DO
    public static CustomerDo ToDomainDo(this Customer e)
    {
        if (e == null) return null!; // callers may use null-conditional
        return new CustomerDo
        {
            CustomerId = e.Id,
            FullName = e.Name,
            ContactEmail = e.Email
        };
    }

    // DO -> persistence (create new entity)
    public static Customer ToPersistence(this CustomerDo d, string tenantId)
    {
        return new Customer
        {
            Id = d.CustomerId,
            Name = d.FullName,
            Email = d.ContactEmail,
            TenantId = tenantId
        };
    }

    // DO -> persistence (apply changes into existing tracked entity)
    public static void ApplyTo(this CustomerDo d, Customer target)
    {
        target.Name = d.FullName;
        target.Email = d.ContactEmail;
    }
}