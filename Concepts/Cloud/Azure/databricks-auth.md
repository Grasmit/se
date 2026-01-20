# Authenticating to Azure Databricks — Dev & Production

This document explains all practical ways to authenticate to Azure Databricks from .NET (local development and production), recommended approaches, code samples in C#, and a troubleshooting guide for common errors.

Contents

- Overview
- Authentication methods (PAT, Service Principal, Managed Identity, Certificate)
- C# code examples for each method
- Granting access in Databricks (UI & SCIM)
- Testing and diagnostics (az CLI, jwt checks)
- Troubleshooting (common errors and fixes)
- Best practices and checklist

---

Overview

- Databricks accepts OAuth2 access tokens issued by Azure AD for resource `https://databricks.azure.net` (use scope `https://databricks.azure.net/.default`) and also supports Databricks Personal Access Tokens (PAT) for quick tests.
- For production use prefer Azure AD tokens (Managed Identity or Service Principal). PATs are for development only.

Authentication methods

1) DefaultAzureCredential (recommended)
  - Local: uses Visual Studio / Azure CLI credentials.
  - Prod: uses Managed Identity (System or User assigned) on App Service, Function, VM.
  - Code: `DefaultAzureCredential` from `Azure.Identity`.

2) Managed Identity (production recommendation)
  - Enable on App Service / Azure Function / VM and grant that identity as a Databricks workspace principal (and warehouse CAN_USE permission).

3) Service Principal (Client Secret or Certificate)
  - Use when MI is not possible. Store secret/cert in Key Vault; use `ClientSecretCredential` or `ClientCertificateCredential`.

4) Personal Access Token (PAT) — development only
  - Generated in Databricks UI for quick testing, set as `Authorization: Bearer <PAT>`.

5) On‑Behalf‑Of (OBO) / Delegation
  - If your app receives a user token and needs to call Databricks on behalf of that user, exchange the incoming token with MSAL OBO (advanced scenario).

Code examples (C#)

Common imports

```csharp
using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text.Json;
```

Helper: set Authorization header from TokenCredential

```csharp
async Task SetBearerFromCredentialAsync(HttpClient http, TokenCredential credential, CancellationToken ct = default)
{
    var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://databricks.azure.net/.default" }), ct);
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
}
```

A — DefaultAzureCredential (local + prod via MI)

```csharp
var credential = new DefaultAzureCredential();
await SetBearerFromCredentialAsync(httpClient, credential);
// call Databricks SQL REST API endpoints
```

Notes: locally DefaultAzureCredential will attempt VisualStudioCredential, AzureCliCredential etc. In production it will use ManagedIdentityCredential when MI is enabled.

B — ClientSecretCredential (Service Principal)

```csharp
var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")!;
var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")!;
var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")!;
var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
await SetBearerFromCredentialAsync(httpClient, credential);
```

Common error: if the clientId belongs to a Managed Identity (MSI) you'll get an error — MSI cannot be used with ClientSecretCredential. Use DefaultAzureCredential instead.

C — ClientCertificateCredential (preferred over secrets)

```csharp
var credential = new ClientCertificateCredential(tenantId, clientId, new X509Certificate2("cert.pfx", "password"));
await SetBearerFromCredentialAsync(httpClient, credential);
```

D — Personal Access Token (dev only)

```csharp
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<PAT>");
```

E — Example: submit SQL statement (reuse from sample)

```csharp
var payload = new { statement = "SELECT 1", warehouse_id = "<warehouse-id>" };
var submit = await httpClient.PostAsJsonAsync("https://<workspace>/api/2.0/sql/statements", payload);
submit.EnsureSuccessStatusCode();
var doc = await submit.Content.ReadFromJsonAsync<JsonDocument>();
var id = doc.RootElement.GetProperty("statement_id").GetString();
```

Granting access in Databricks

- Add the identity (user / service principal / managed identity) in Workspace Admin → User Management or use SCIM to create ServicePrincipal entries.
- Grant SQL warehouse permission `CAN_USE` for running statements. For data access grant table permissions via Unity Catalog where available.
- SCIM example (requires workspace admin token):

```bash
curl -X POST https://<workspace>/api/2.0/preview/scim/v2/ServicePrincipals \
  -H "Authorization: Bearer <admin-token>" \
  -H "Content-Type: application/json" \
  -d '{"displayName":"my-sp","externalId":"<aad-object-id>"}'
```

Testing & diagnostics

- Request a token via CLI to validate AAD flow:

```powershell
az account get-access-token --resource https://databricks.azure.net --tenant <tenantId>
```

- Use `jwt.ms` or `jwt.io` to decode the token. Verify `aud` claim equals `https://databricks.azure.net` and that `exp` is in future.
- Use `az rest` or simple curl to call Databricks endpoints once token obtained.

Troubleshooting — common errors & fixes

- AuthenticationFailedException / MSAL errors
  - Cause: invalid client id/secret, using MSI id with ClientSecretCredential, expired secret, wrong tenant.
  - Fixes: verify env vars (TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET), ensure app registration exists (`az ad app show --id <clientId>`), or switch to DefaultAzureCredential for MI.

- AADSTS7000232 / "MSI identity should not use ClientSecretCredential"
  - Cause: code attempts client secret flow for a Managed Identity principal.
  - Fix: remove client secret envs and use DefaultAzureCredential, or create a proper app registration and secret.

- 401 Unauthorized / aud mismatch
  - Cause: you requested token for wrong resource (e.g., workspace URL) so `aud` wrong.
  - Fix: request token for `https://databricks.azure.net/.default` (or for your custom API App ID URI if applicable).

- 403 Forbidden
  - Cause: identity is not granted needed permissions in Databricks workspace/warehouse.
  - Fix: add identity to workspace and grant `CAN_USE` on the SQL warehouse or grant appropriate table permissions.

- Network / Private Link errors
  - Cause: workspace on Private Link or VNet and App Service cannot reach it.
  - Fix: configure VNet Integration for App Service or place client in same VNet, use Private Endpoint routing.

Best practices

- Prefer `DefaultAzureCredential` + Managed Identity for production.
- For app-only flows prefer certificate-based service principal over client secret.
- Do not use PAT in production.
- Use Key Vault for storing secrets and Key Vault references in App Service.
- Validate token `aud` and `scp` / `roles` claims on your service layer (if you implement middle tier).
- Automate adding service principals to Databricks using SCIM when possible.

Quick checklist (dev → prod)

1. Local dev: `az login` or Visual Studio sign-in → run sample using `DefaultAzureCredential`.
2. Create Databricks SQL warehouse and note `warehouse_id` and workspace URL.
3. Production: enable App Service Managed Identity and add MI to Databricks workspace; grant `CAN_USE` on warehouse.
4. Deploy app and confirm `DefaultAzureCredential` obtains token and API calls succeed.

References

- Databricks REST API docs: https://docs.databricks.com/dev-tools/api/latest/index.html
- Azure.Identity: https://learn.microsoft.com/dotnet/api/azure.identity
- Databricks SCIM API: https://docs.databricks.com/dev-tools/api/preview/scim/
