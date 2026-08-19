# Catalog module completion plan

**Current status:** Phases 0-4 are complete. `dotnet build eShop.slnx` -> 0
errors, 0 warnings. `dotnet test` -> 142 passing (134 unit, 8 architecture).
The module is verified end to end against SQL Server 2022: migrations apply
into the `catalog` schema, seeding succeeds, full product CRUD plus price and
stock operations persist, and the three error paths return the expected
problem-details shapes.

Still open: Phase 5 (Contracts, Outbox, RabbitMQ), Phase 6 integration tests
with Testcontainers, and Phase 7 ops. No integration test exercises the
database yet; persistence has been verified manually, not automatically.

**Decision taken:** `Money` (amount + 3-letter ISO currency, non-negative, max
two decimal places) replaced `decimal Price`, per architecture.md. Done before
any migration existed, so it cost nothing; reversing it later would not be free.

## Known defects found in the scaffold

Ten of the twelve are resolved:

| # | Defect | Resolved by |
| --- | --- | --- |
| D1 | Host served the `WeatherForecast` template | Phase 0 — `Program.cs` composes the module |
| D2 | `eShop.API` missing from `eShop.slnx` | Phase 0 — added, so CI builds the host |
| D3 | `[HttpGet("/all")]` discarded the route prefix | Phase 0 — `api/catalog/products` |
| D4 | `PictureUri` ignored by EF, silently dropped on save | Phase 4 — persisted; confirmed round-tripping against SQL Server |
| D5 | `Description`, stock columns and `OnReorder` unconfigured | Phase 4 — configured, lengths taken from the aggregate's constants |
| D6 | Namespaces did not match the `Data` folders | Phase 4 |
| D7 | Seven build warnings and a possible NRE in `Entity` | Phase 0 — 0 warnings |
| D8 | 129 `obj`/`bin` files tracked in git | Phase 0 |
| D9 | `Catalog.Application` referenced EF Core | Phase 2 — read port returns DTOs, package reference dropped, now enforced by an architecture test |
| D12 | `ProductCreatedDomainEvent` could never carry an `Id` | Phase 1 — carries the aggregate |

Still open:

- **D10** — `Catalog.Tests.Integration` still has no project references and one
  skipped placeholder test. Phase 6.
- **D11** — the `Class1.cs` placeholders are gone, but `BuildingBlocks.Infrastructure`
  is now simply empty. It gains real content (`OutboxMessage`,
  `IIntegrationEventPublisher`) in Phase 5.

---

## Phase 0 — Make the module actually run (unblocks everything)

- [x] Add `eShop.API` to `eShop.slnx`; give it project references to `Catalog.API` and `Catalog.Infrastructure` **(D2)**.
- [x] Rewrite `Program.cs`: `AddControllers()`, `AddCatalogApplication()`, `AddCatalogInfrastructure(config)`, `AddCatalogApi()`, `MapControllers()`, OpenAPI, `AddProblemDetails()` **(D1)**.
- [x] `Catalog.API/Extensions/CatalogApiExtensions.cs` — `AddCatalogApi()` registering the module's application part, so the host never needs to know about individual controllers.
- [x] Fix the route and response attributes on `ProductsController` **(D3)**. Settle on `api/catalog/products`.
- [x] `git rm -r --cached` every tracked `obj/`/`bin/` file; one dedicated commit **(D8)**.
- [x] Fix `Entity<TId>` nullability and null-safe `Equals`/`GetHashCode`; consider `where TId : notnull` **(D7)**.
- [x] Delete both `Class1.cs` placeholders **(D11)**.
- [x] Add `Directory.Packages.props` (central package management) — MediatR, FluentValidation and EF versions are currently duplicated across nine csproj files.

**Exit criteria:** `GET /api/catalog/products` returns `200 []` from the running host.

## Phase 1 — Finish the domain

- [x] `Catalog.Domain/Exceptions/CatalogDomainException.cs`; replace the `ArgumentException`/`InvalidOperationException` throws in `CatalogItem` so the API can map rule failures distinctly from genuine infrastructure faults.
- [x] `Catalog.Domain/ValueObjects/Money.cs` (amount + currency, non-negative, arithmetic) and swap `decimal Price` for it. architecture.md explicitly lists `ValueObjects/Money`, and it is the cleanest place to demonstrate value-object equality. *Alternative: keep `decimal` and skip the owned-type mapping — but decide before the first migration, because it changes the schema.*
- [x] New behaviour on `CatalogItem`: `ChangeDetails(name, description, pictures)` and `SetStockThresholds(restock, max)`.
- [x] Domain events: rework `ProductCreatedDomainEvent` to carry the aggregate (or a full payload including `Id`) **(D12)**; add `ProductPriceChangedDomainEvent` and `ProductStockChangedDomainEvent`, raised from `ChangePrice`/`AddStock`/`RemoveStock`.
- [x] Expand `Catalog.Test.Unit`: every guard in `EnsureValidDetails`/`EnsureValidStock`, `RemoveStock` when out of stock, `AddStock` clamping at max, `Money` equality and negative amounts, and one event-raised assertion per mutating method.

## Phase 2 — Application slices

- [x] `Catalog.Application/Abstractions/`:
  - `ICatalogItemRepository` — `GetByIdAsync`, `Add`, `Remove`
  - `IUnitOfWork` — `SaveChangesAsync`
  - `ICatalogQueries` — read-side port returning DTOs, so Application keeps no EF dependency **(then drop the EF package reference, D9)**
- [x] Wire the real `CreateProductHandler` (it currently returns `0`): build the aggregate, `Add`, `SaveChangesAsync`, return the new `Id`.
- [x] Add the missing `CreateProductValidator`. `AddCatalogApplication` already registers validators and the pipeline behavior, so today nothing is actually validated.
- [x] Wire `GetProductsHandler` (it currently returns an empty list) with paging and optional `brandId`/`typeId` filters; add `PagedResult<T>` to `BuildingBlocks.Application`.
- [x] New slices under `Features/Products/`: `GetProduct` (by id), `UpdateProduct`, `ChangeProductPrice`, `AddStock`, `RemoveStock`, `DeleteProduct`.
- [x] New slices under `Features/Brands/` and `Features/Types/`: `GetBrands`, `GetTypes` — needed by any UI and by create-product validation.
- [x] `NotFoundException` plus a consistent result convention, so handlers never return `null` into a controller.
- [x] Handler unit tests against fake repository/query ports.

## Phase 3 — HTTP surface

- [x] Controller actions for every slice above — transport only, with correct `ProducesResponseType` per verb (`201` + `CreatedAtAction` for create, `204` for price/stock/delete, `404` for missing).
- [x] An `IExceptionHandler` in the host mapping `ValidationException` → `400 ValidationProblemDetails`, `CatalogDomainException` → `400`, `NotFoundException` → `404`.
- [x] Verify the generated OpenAPI document in Development, including the `Catalog-Products` tag grouping.

## Phase 4 — Persistence

- [x] Move `CatalogContext` to namespace `Catalog.Infrastructure.Data` and the configurations to `Catalog.Infrastructure.Data.Configurations` **(D6)**.
- [x] `builder.HasDefaultSchema("catalog")` — architecture.md requires a per-module schema and nothing sets one today.
- [x] Fix `CatalogItemConfiguration`: persist `PictureUri` **(D4)**; configure `Description`, the stock columns and `OnReorder`; map `Money` as an owned type; add `RowVersion` for optimistic concurrency on stock changes; index `(CatalogBrandId, CatalogTypeId)` **(D5)**.
- [x] Unique indexes on `CatalogBrand.Brand` and `CatalogType.Type`.
- [x] `Catalog.Infrastructure/Repositories/CatalogItemRepository.cs`, `CatalogQueries.cs` (`AsNoTracking` projections straight to DTOs), and `UnitOfWork`.
- [x] `Catalog.Infrastructure/Extensions/CatalogInfrastructureExtensions.cs` — `AddCatalogInfrastructure(IConfiguration)` registering the DbContext, SQL Server provider and ports.
- [x] First migration into `Data/Migrations/`, using the `catalog` schema and its own `__EFMigrationsHistory`; seed brands, types and a handful of items from `Data/Seeding/`.
- [x] Connection string plus a design-time factory so `dotnet ef` works directly against `Catalog.Infrastructure`.

## Phase 5 — Contracts, Outbox, RabbitMQ

- [ ] `Catalog.Contracts/IntegrationEvents/`: `ProductCreatedV1`, `ProductPriceChangedV1`, `ProductStockChangedV1` — plain serializable records, zero dependencies.
- [ ] `BuildingBlocks.Infrastructure`: `OutboxMessage` and `IIntegrationEventPublisher` (this is what fills the currently empty project, D11).
- [ ] `Catalog.Infrastructure/EventHandlers/` — MediatR notification handlers mapping each domain event to its `*V1` contract.
- [ ] Dispatch domain events **inside** `SaveChangesAsync` via a `SaveChangesInterceptor`, so outbox rows are written in the same transaction; clear events afterwards. This also resolves **D12**, since Ids exist by that point.
- [ ] `Catalog.Infrastructure/Messaging/`: RabbitMQ connection/publisher and `OutboxProcessor : BackgroundService` — poll unprocessed rows, publish, stamp `ProcessedOn`, retry with backoff, dead-letter after N attempts.
- [ ] Outbox table configuration and migration.

## Phase 6 — Tests

- [ ] `Catalog.Tests.Integration`: add the missing project references **(D10)**, plus `Testcontainers.MsSql` and `Testcontainers.RabbitMq`; a `CatalogDatabaseFixture` that applies migrations, shared through a collection fixture.
- [ ] Integration tests: repository round trip (this is the regression guard for **D4**), migrations apply cleanly, unique-index violations, outbox row written in the same transaction, outbox processor publishes and marks processed.
- [ ] `WebApplicationFactory<Program>` end-to-end tests over the real endpoints — create → get → change price → remove stock — including the `400`/`404` mappings.
- [x] Extend `tests/Architecture.Tests/ArchTest.cs`; it currently checks only two of the documented rules. Add: Domain has no EF Core/ASP.NET/RabbitMQ dependency; Application has no EF Core or ASP.NET types; API has no `DbContext` dependency; Contracts references nothing; every `ICommand`/`IQuery` has a handler; every command has a validator.

## Phase 7 — Ops

- [ ] Multi-stage `Dockerfile` for `eShop.API` and a `docker-compose.yml` (host + `mssql/server` + `rabbitmq:management`) with healthchecks and a `.env`.
- [ ] Pick one migration strategy — `dotnet ef migrations bundle` or a startup migrator gated to Development. Do not auto-migrate in production.
- [ ] `.github/workflows/ci.yml`: restore → build (`-warnaserror`) → unit and architecture tests → integration tests (Testcontainers needs a Docker-enabled runner) → later, image build and push.
- [ ] `/health` and `/health/ready` endpoints covering SQL Server and RabbitMQ.

---

## Suggested execution order

Phase 0 comes first — until the host composes the module, nothing else is
observable. Then 1 → 2 → 4 → 3 produces a genuinely working vertical slice end
to end, with persistence landing before HTTP so the controller is wired against
real data on the first try. Phases 5–7 are additive and can land independently.

## Decisions to make before Phase 1

1. **`Money` value object vs plain `decimal`** — architecture.md calls for it, and it changes the EF mapping, so decide before the first migration.
2. **Controllers vs minimal-API endpoints** — architecture.md shows a `Controllers/` folder in the layout but a `CreateProductEndpoint.cs` in the slice example. The existing code uses controllers; recommend staying with controllers and correcting the doc.
3. **MediatR 14 is commercially licensed** — fine for a personal or portfolio project, but worth a conscious decision given the modular-monolith framing.
