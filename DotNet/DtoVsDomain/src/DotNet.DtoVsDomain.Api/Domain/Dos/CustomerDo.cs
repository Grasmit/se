namespace DotNet.DtoVsDomain.Api.Domain.Dos;

// Simple Domain Object (DO) used inside application/service layers
public class CustomerDo
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public static CustomerDo CreateNew(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));

        return new CustomerDo { CustomerId = Guid.NewGuid(), FullName = name, ContactEmail = email };
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
        // Optionally add format validation here
        ContactEmail = email;
    }
}