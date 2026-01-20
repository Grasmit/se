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

Links

- Prev: [Secrets & Credentials (Key Vault)](secrets.md)
- Next: [Observability & Monitoring](observability.md)
