namespace DotNet.DtoVsDomain.Api.Services.Tenancy;

public interface ITenantProvider
{
    string CurrentTenant { get; }
    string ResolveTenantOrThrow();
}
