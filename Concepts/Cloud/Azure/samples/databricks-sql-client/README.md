# Databricks SQL client sample (C#)

This sample shows how to run a SQL statement against an Azure Databricks SQL warehouse using the Databricks SQL REST API and Azure AD authentication (`DefaultAzureCredential`). It supports local development (Azure CLI / Visual Studio credentials) and production (App Service Managed Identity).

Files

- `DatabricksSqlClient.cs` — lightweight client that submits SQL statements and polls for results.
- `Program.cs` — minimal console runner.
- `databricks-sql-client.csproj` — project file.

Prerequisites

- .NET SDK 8.0
- `az login` for local development or Visual Studio sign‑in
- Docker not required for this sample

Configuration

Provide the following settings via environment variables or `appsettings.json`:

- `DATABRICKS_WORKSPACE_URL` or `Databricks:WorkspaceUrl` — e.g. `https://adb-123456789012345.10.azuredatabricks.net`
- `DATABRICKS_WAREHOUSE_ID` or `Databricks:WarehouseId` — the SQL warehouse id
- Optional for quick local testing: `DATABRICKS_PAT` or `Databricks:Pat` (Personal Access Token). Use PAT only for dev/testing.

Local development

1. Login with the Azure CLI: `az login` (DefaultAzureCredential will use this account).
2. Set environment variables or create `appsettings.json` with the Databricks values.
3. Run:

```powershell
dotnet run --project databricks-sql-client.csproj
```

Production (App Service)

1. Enable System Assigned Managed Identity on the App Service:

```powershell
az webapp identity assign --name <app> --resource-group <rg>
```

2. In the Databricks workspace, add the App Service's managed identity as a workspace user or service principal and grant the required SQL permissions.

3. Configure `DATABRICKS_WORKSPACE_URL` and `DATABRICKS_WAREHOUSE_ID` as App Settings (or Key Vault references) in App Service.

4. Deploy the app; the sample uses `DefaultAzureCredential` which will acquire a token from the managed identity in production.

Security notes

- Do not store PATs in source control. Use PAT only for local testing.
- Prefer Managed Identity in production and grant least privilege in Databricks.

Next steps

- Integrate result parsing to your application domain (rows → DTOs).
- Add retries, exponential backoff and better error handling for production use.
