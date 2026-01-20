# Azure for .NET — Overview

This folder contains focused documentation pages about how Microsoft Azure is commonly used with .NET / ASP.NET applications. Each page is a self-contained sheet and you can navigate between them using the links at the top of every page.

Pages

- [Identity and Authentication](identity.md)
- [Secrets & Credentials (Key Vault)](secrets.md)
- [Hosting & Databases](hosting.md)
- [Observability & Monitoring](observability.md)
- [Messaging & Events](messaging.md)
- [Architecture Diagrams & Flows](architecture.md)

Extras

- `bicep/appservice-keyvault.bicep`: sample Bicep to create App Service (system identity) and Key Vault with an access policy.
- `samples/deploy-webapp.yml`: sample GitHub Actions workflow to build & deploy the ASP.NET app.
- `diagrams/architecture.mmd` and `diagrams/render-diagrams.ps1`: Mermaid source and a Docker-based renderer script to export `architecture.png`.

Navigation: Start with Identity if you want auth-first, or with Hosting for deployment details.

---

If you'd like I can extend any page with CLI/ARM/Bicep snippets, or add a sample GitHub Actions workflow for CI/CD.
