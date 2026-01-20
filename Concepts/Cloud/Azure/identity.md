<!-- Prev: README.md | Next: secrets.md -->

# Identity and Authentication (Azure AD / Entra)

Purpose

- Centralize authentication and authorization for APIs, services and users.
- Replace ad-hoc header or API-key schemes with OAuth2 / OpenID Connect tokens.

Core concepts

- Azure Active Directory (Entra ID): identity provider for users, groups and service principals.
- App registrations: represent applications (APIs and clients). Issue client IDs and secrets/certs.
- Service principals: runtime identity for an app in a tenant.
- Managed Identity: first‑class Azure identity attached to resources (App Service, VM, Functions, AKS).
- OAuth2 & OpenID Connect: access tokens (Bearer) for APIs and ID tokens for user sign-in.

Integration patterns for ASP.NET Core (typical)

- Web APIs (resource): validate incoming Bearer tokens using `Microsoft.Identity.Web` or `AddJwtBearer` with Microsoft identity metadata.
- Web apps: use `Microsoft.Identity.Web` + OpenID Connect middleware for user login.
- Service-to-service: prefer Managed Identity or client credentials flow for service principals.

Example (high level code guidance)

- In `Program.cs` use `builder.Services.AddMicrosoftIdentityWebApi(configuration)` or `AddJwtBearer` and require `[Authorize]` on controllers.
- Use token claims (e.g., `sub`, `oid`, `roles`, `scp`) for tenant scoping and RBAC.

Notes on multi-tenant and tenant scoping

- Single-tenant apps restrict auth to one Entra tenant.
- Multi-tenant apps allow users from many tenants but require consent and careful authorization checks.
- For tenancy inside your domain model (tenants per customer), combine validated Azure tokens with your existing tenancy provider (e.g., header-based `ITenantProvider`) and enforce mapping between token tenant claim and internal tenant.

Security recommendations

- Use Azure AD for auth; avoid custom token formats.
- Use managed identity where possible to avoid secrets.
- Regularly rotate any client secrets; prefer certs for long-lived credentials.
- Use Conditional Access & Privileged Identity Management for sensitive roles.

Links

- Index: [Overview](README.md)
- Next: [Secrets & Credentials (Key Vault)](secrets.md)

Databricks token acquisition (example)

If your app needs to call Azure Databricks REST APIs (recommended) use an AAD access token with scope `https://databricks.azure.net/.default`.

Example (C#) using `DefaultAzureCredential` — works locally (`az login` / VS sign-in) and in App Service (Managed Identity):

```csharp
var credential = new DefaultAzureCredential();
var token = await credential.GetTokenAsync(
	new TokenRequestContext(new[] { "https://databricks.azure.net/.default" }),
	CancellationToken.None);

httpClient.DefaultRequestHeaders.Authorization =
	new AuthenticationHeaderValue("Bearer", token.Token);
```

See sample: `samples/databricks-sql-client` for a full working example that submits SQL statements and polls results.

