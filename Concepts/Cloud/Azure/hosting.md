<!-- Prev: secrets.md | Next: observability.md -->

# Hosting & Databases — App Service, AKS, Azure SQL

Common hosting choices for ASP.NET Core

- App Service (PaaS): easiest path for ASP.NET apps; supports Managed Identity, autoscale, deployment slots.
- Azure Kubernetes Service (AKS): for microservices and container orchestration.
- Azure Container Instances (ACI): lightweight container option for simple workloads.

Data storage

- Azure SQL Database / Managed Instance: primary relational store for EF Core and ADO.NET.
- Cosmos DB: multi-model, globally distributed NoSQL store.
- Blob Storage: files, media, backups.

Deployment patterns

- Direct App Service deployment (GitHub Actions or Azure DevOps) with staging slots.
- Containerize with Docker, push to Azure Container Registry (ACR), deploy to App Service or AKS.

Recommended steps to secure and operate

- Use Managed Identity for database access and Key Vault access.
- Put App Service behind VNet Integration for private resources when needed.
- Use health checks, readiness/liveness endpoints, and scale rules.

Notes about your current setup

- Your platform currently uses App Service + Azure SQL + Databricks. Keep Databricks for analytics; do not use it for OLTP.

Databricks from App Service — recommended production setup

- Enable System Assigned Managed Identity on the App Service and grant that identity access in the Databricks workspace (add as workspace user or service principal and grant SQL permissions).
- Store `WorkspaceUrl` and `WarehouseId` in App Settings or Key Vault references; do NOT store PATs in appsettings for production.

AppSettings example (App Service settings or Key Vault references):

```
DATABRICKS_WORKSPACE_URL = https://<your-workspace>.azuredatabricks.net
DATABRICKS_WAREHOUSE_ID = <warehouse-id>
```

Quick deploy checklist

1. `az webapp identity assign --name <app> --resource-group <rg>`
2. In Databricks workspace, add the App Service identity and grant SQL permissions (or workspace admin if needed).
3. Configure `DATABRICKS_WORKSPACE_URL` and `DATABRICKS_WAREHOUSE_ID` in App Service settings (or use Key Vault references).
4. Deploy the app; code can use `DefaultAzureCredential` to call Databricks (see `samples/databricks-sql-client`).


Links

- Prev: [Secrets & Credentials (Key Vault)](secrets.md)
- Next: [Observability & Monitoring](observability.md)
