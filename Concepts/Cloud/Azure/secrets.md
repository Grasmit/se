<!-- Prev: identity.md | Next: hosting.md -->

# Secrets & Credentials — Azure Key Vault and Managed Identity

Why

- Keep database connection strings, API keys, certificates and other secrets out of source code and configuration files.

Core components

- Azure Key Vault: secure store for secrets, keys and certificates.
- Managed Identity (System/User Assigned): gives an Azure resource an identity to request tokens without storing credentials.
- DefaultAzureCredential: SDK helper that tries MSI, environment vars, Visual Studio, CLI in order.

Typical integration for ASP.NET Core

1. Enable System Assigned Managed Identity on the App Service (or other resource).
2. Grant that identity access policies or RBAC permissions to Key Vault (Get, List for secrets).
3. In `Program.cs` add Key Vault as a configuration source using `DefaultAzureCredential`.

Example snippet (conceptual)

```csharp
if (!string.IsNullOrEmpty(builder.Configuration["KeyVault:VaultUri"]))
{
    builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["KeyVault:VaultUri"]!), new DefaultAzureCredential());
}
```

Connection strings

- Prefer Managed Identity with Azure SQL: acquire an access token and set it on `DbContext` instead of embedding username/password.

Migration checklist

- Move secrets out of `appsettings.json` into Key Vault references or App Service settings referencing Key Vault.
- Configure Managed Identity and grant least privilege.
- Update CI/CD pipelines to deploy secret references or set Key Vault policies for service principals.

Links

- Prev: [Identity and Authentication](identity.md)
- Next: [Hosting & Databases](hosting.md)
