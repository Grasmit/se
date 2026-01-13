using Microsoft.AspNetCore.Http;

namespace DotNet.DtoVsDomain.Api.Services.Tenancy;

public class HttpTenantProvider : ITenantProvider
{
    private const string HeaderName = "X-Tenant";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentTenant => _httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString() ?? string.Empty;

    public string ResolveTenantOrThrow()
    {
        if (string.IsNullOrWhiteSpace(CurrentTenant))
        {
            throw new InvalidOperationException("Tenant header 'X-Tenant' is required.");
        }

        return CurrentTenant;
    }
}
