# DTO vs Domain (DO) Learning Project

Production-style sample showing how domain objects stay pure while DTOs shape API contracts. Scenario: multi-tenant SaaS order pipeline (tenants "acme" and "globex") with C#/.NET 8 Web API, EF Core, SQL Server, and Docker.

## Where to look
- Domain objects (business rules): [src/DotNet.DtoVsDomain.Api/Domain/Entities/Order.cs](src/DotNet.DtoVsDomain.Api/Domain/Entities/Order.cs), [src/DotNet.DtoVsDomain.Api/Domain/Entities/OrderItem.cs](src/DotNet.DtoVsDomain.Api/Domain/Entities/OrderItem.cs)
- DTOs (API contracts): [src/DotNet.DtoVsDomain.Api/Contracts/Requests/CreateOrderRequest.cs](src/DotNet.DtoVsDomain.Api/Contracts/Requests/CreateOrderRequest.cs), [src/DotNet.DtoVsDomain.Api/Contracts/Responses/OrderResponse.cs](src/DotNet.DtoVsDomain.Api/Contracts/Responses/OrderResponse.cs)
- Mapping: [src/DotNet.DtoVsDomain.Api/Mappings/OrderMapping.cs](src/DotNet.DtoVsDomain.Api/Mappings/OrderMapping.cs)
- Application service: [src/DotNet.DtoVsDomain.Api/Services/OrderService.cs](src/DotNet.DtoVsDomain.Api/Services/OrderService.cs)
- API surface: [src/DotNet.DtoVsDomain.Api/Controllers/OrdersController.cs](src/DotNet.DtoVsDomain.Api/Controllers/OrdersController.cs)
- Persistence/tenancy: [src/DotNet.DtoVsDomain.Api/Persistence/AppDbContext.cs](src/DotNet.DtoVsDomain.Api/Persistence/AppDbContext.cs), [src/DotNet.DtoVsDomain.Api/Services/Tenancy/HttpTenantProvider.cs](src/DotNet.DtoVsDomain.Api/Services/Tenancy/HttpTenantProvider.cs)

## Run it
1) From repo root: `docker-compose up --build`
2) Wait for SQL health-check, then hit Swagger at http://localhost:8080/swagger
3) Always send `X-Tenant` header with either `acme` or `globex`

Seed data (by tenant):
- acme → customers: Alice, Bob; products: API Subscription ($19.99), Dashboard Add-on ($9.99)
- globex → customers: Gina; products: Data Export ($29.00)

Get tenant-specific IDs to build orders:
- List customers: `GET http://localhost:8080/api/customers` with `X-Tenant`
- List products: `GET http://localhost:8080/api/products` with `X-Tenant`

## DTO vs DO quick notes
- Domain objects express rules and invariants (e.g., `Order.AddItem()` enforces positive quantity, `Order.Submit()` requires items). They carry behavior and are tenant-aware via the `TenantId` field.
- DTOs are transport-only shapes optimized for HTTP contracts. They omit behavior and internal fields (e.g., no tenant keys, no domain timestamps) and can aggregate view data such as product names in `OrderResponse`.
- Mapping keeps layers decoupled: `Order` (DO) → `OrderResponse` (DTO) in [OrderMapping](src/DotNet.DtoVsDomain.Api/Mappings/OrderMapping.cs). Input DTOs are validated and converted to domain commands inside [OrderService](src/DotNet.DtoVsDomain.Api/Services/OrderService.cs).

## Sample requests
Create an order (Acme):
```http
POST http://localhost:8080/api/orders
X-Tenant: acme
Content-Type: application/json

{
  "customerId": "<customer-guid-here>",
  "items": [
    { "productId": "<product-guid-here>", "quantity": 2 }
  ]
}
```

Tip: Try sending `"quantity": 0` — the request will be rejected with 400 due to DTO validation (FluentValidation). Domain invariants (e.g., `Order.AddItem()`) are enforced in the domain layer and covered by unit tests.

List orders with paging:
```http
GET http://localhost:8080/api/orders?page=1&pageSize=10
X-Tenant: acme
```

## Talking points for KT
- Keep domain models persistence-ignorant; EF Core configuration lives in [AppDbContext](src/DotNet.DtoVsDomain.Api/Persistence/AppDbContext.cs).
- DTOs should be versionable and stable; avoid leaking domain enums or internal IDs unless required.
- Use services to translate and validate (e.g., product existence, tenant isolation) before mutating domain objects.
- Global query filters enforce tenant isolation; `HttpTenantProvider` pulls `X-Tenant` from the request so both queries and saves are scoped.
- Seeding sets up two tenants and products, demonstrating that the same API surface serves multiple tenants without code changes.

**Further reading:** See `docs/DTO_vs_DO.md` for a deep dive into DTO vs Domain Object and concrete examples in this project.
