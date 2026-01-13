namespace DotNet.DtoVsDomain.Api.Domain.Aggregates;

// Pure domain object (DO) representing a Customer with domain-friendly names.
public class CustomerAggregate
{
    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;

    private CustomerAggregate() { }

    internal static CustomerAggregate Rehydrate(Guid id, string name, string email)
    {
        return new CustomerAggregate
        {
            CustomerId = id,
            FullName = name,
            ContactEmail = email
        };
    }

    public static CustomerAggregate CreateNew(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        return new CustomerAggregate
        {
            CustomerId = Guid.NewGuid(),
            FullName = name,
            ContactEmail = email
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        FullName = name;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        ContactEmail = email;
    }
}