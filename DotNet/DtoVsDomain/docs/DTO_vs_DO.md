# DTO vs Domain Object (DO) — Notes & Examples 🔧💡

## Summary
- **Domain Object (DO)**: Represents a business concept and *contains behavior and invariants* (methods, rules). Lives in the domain layer and is used by services/repositories. Example: `Order` with `AddItem()` and `Submit()` methods.
- **Data Transfer Object (DTO)**: A simple, serializable shape used for transporting data across boundaries (HTTP requests/responses, queues). DTOs have no business behavior. Example: `CreateOrderRequest` and `OrderResponse`.

## Why separate them?
- Clear separation prevents leaking domain concerns to clients and makes API contracts stable and versionable.
- DTO validation (format/sanity) is distinct from domain validation (business rules/invariants). Validate input DTOs at the edge; enforce invariants in the domain.

## How this project demonstrates the pattern
- DOs are under `src/DotNet.DtoVsDomain.Api/Domain/Entities` (e.g., `Order.cs`). They implement business behavior (quantity checks, submission invariants).
- DTOs are under `src/DotNet.DtoVsDomain.Api/Contracts` (requests/responses), and are intentionally behavior-free.
- Mapping logic is separated into `src/DotNet.DtoVsDomain.Api/Mappings/OrderMapping.cs`.
- DTO validation lives at the boundary using FluentValidation (`Validators/CreateOrderRequestValidator.cs`) — ensures well-formed input values.
- Domain tests show rules enforced by the DO (e.g., `Order.AddItem()` throws on invalid quantity).

## Concrete guidance & examples
### Input flow (best practice)
1. Controller receives JSON and model binder constructs a DTO (e.g., `CreateOrderRequest`).
2. DTO is validated (FluentValidation) for shape/format (not business rules). If invalid → return 400.
3. Controller calls a service which translates DTO → Domain actions (e.g., read products, call `new Order(); order.AddItem()`).
4. Service enforces business rules using domain methods; domain methods throw or return errors when invariants are violated.
5. On success, map domain objects → response DTOs (`OrderResponse`) and return 201/200.

### Example: DTO-level validations (in `Validators`)
- `CreateOrderRequestValidator` ensures `CustomerId` is present and `Items` contains at least one element.
- `CreateOrderItemRequestValidator` ensures `ProductId` is present and `Quantity > 0`.

### Example: Domain-level validations (in `Domain/Entities`)
- `Order.AddItem()` throws if `quantity <= 0`.
- `Order.Submit()` throws if no items present.

## Tests
- See `src/DotNet.DtoVsDomain.Api.Tests` for tests demonstrating:
  - DTO validation failures are caught early (unit tests for validators).
  - Domain invariants are enforced (unit tests for `Order`).
  - Integration-like test for `OrderService` shows DTO → DO flow and tenant awareness using an in-memory database.
  - Persistence ↔ Domain mapping tests show how to rehydrate a pure DO from EF entities, apply domain rules, and map changes back to persistence (`PersistenceMappingTests`).

## Rehydrating a DO from persistence
- Sometimes you want a pure, non-tracked DO to apply business rules safely. Steps:
  1. Load persistence entities (EF models) with required navigations in a repository/service.
  2. Map them to a domain aggregate (`OrderAggregate.Rehydrate` / `persistenceOrder.ToDomain()`), or map to a plain DO (mapper-only approach where you use a `CustomerDo` or `OrderDo`).
  3. Apply business logic on the DO (e.g., `ApplyPercentageDiscount` or `CustomerDo.ChangeEmail()`).
  4. Map the changed state back to persistence entities and save.

### Mapper-only (DO) approach (no aggregate)
- If your domain needs are small, you can use a plain DO (`CustomerDo`) plus small, explicit mappers:
  - `Customer` (persistence) -> `CustomerDo` (domain) via `ToDomainDo()`
  - `CustomerDo` -> `Customer` for new inserts via `ToPersistence(tenantId)`
  - `CustomerDo.ApplyTo(Customer)` to copy changes into an existing tracked EF entity
- Benefits: explicit, easy to test, no third-party mapping libraries required. See `src/DotNet.DtoVsDomain.Api/Domain/Dos/CustomerDo.cs` and `src/DotNet.DtoVsDomain.Api/Mappings/CustomerDoMappers.cs` for examples.

Why do this?
- It avoids accidental side effects from EF change tracking when evaluating/validating rules.
- Enables easier unit testing of domain logic because the DO is pure and does not need DB.
- Keeps a clear separation between persistence concerns and business behavior.

Example: see `OrderService.ApplyPercentageDiscountAsync` and the `IOrderRepository` / `OrderRepository` implementation for an example of this flow (repository returns a pure DO which the service operates on).

## Practical tips for production code
- Don't return EF entities directly from controllers—map to response DTOs.
- Keep domain classes persistence-ignorant; EF Core configuration belongs in `AppDbContext`.
- Use validators for DTOs (FluentValidation) and keep business rules as unit-tested domain methods.
- Version DTOs when changing contracts; keep domain refactorings independent of API contracts when possible.

---
If you'd like, I can also:
- Add a sample HTTP integration test (using `WebApplicationFactory`) to show an end-to-end request with tenant header.
- Add a short screencast or a walk-through document that steps you through the DTO → DO mapping in a debugger.

Happy to continue—tell me which follow-up you'd prefer next! ✅
