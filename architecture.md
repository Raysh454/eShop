# eShop architecture

## Goal

eShop is a modular monolith. Modules are independently owned business
capabilities which run in one deployable ASP.NET Core host today, while keeping
the technical boundaries needed to extract a module into a microservice later.

The initial modules are Catalog, Basket, Ordering, and Payments. Catalog is the
first module being implemented.

## Module layout

Each module is split into five projects:

```text
Catalog.API             HTTP endpoints and module registration
Catalog.Application     Vertical slices, CQRS handlers, validation and ports
Catalog.Domain          Aggregates, value objects, domain events and invariants
Catalog.Infrastructure EF Core, migrations, Outbox and external adapters
Catalog.Contracts       Versioned integration-event contracts
```

`eShop.API` is the composition host. It wires modules together and contains no
business features.

`BuildingBlocks.*` contains small shared technical primitives only. It must not
contain business concepts from any module.

### Standard module folder layout

Catalog follows the established Metamind module convention for API,
Application, Domain, and Infrastructure. The extra `Contracts` project is a
purposeful addition: Metamind does not have a dedicated cross-module contract
boundary, while eShop needs one to support eventual service extraction.

```text
src/Modules/Catalog/
├── Catalog.API/
│   ├── Controllers/                 # HTTP transport only
│   ├── Extensions/                  # MapCatalogEndpoints / API registration
│   └── Catalog.API.csproj
├── Catalog.Application/
│   ├── Extensions/                  # AddCatalogApplication (MediatR, behaviours)
│   ├── Features/
│   │   └── Products/
│   │       ├── CreateProduct/
│   │       │   ├── CreateProductCommand.cs
│   │       │   ├── CreateProductValidator.cs
│   │       │   └── CreateProductHandler.cs
│   │       ├── GetProduct/
│   │       │   ├── GetProductQuery.cs
│   │       │   ├── GetProductHandler.cs
│   │       │   └── ProductResponse.cs
│   │       └── GetProducts/
│   │           ├── GetProductsQuery.cs
│   │           ├── GetProductsHandler.cs
│   │           └── ProductResponse.cs
│   ├── Abstractions/                # module-owned ports, e.g. IProductRepository
│   └── Catalog.Application.csproj
├── Catalog.Domain/
│   ├── Events/                      # private domain events
│   ├── ValueObjects/                # e.g. Money
│   ├── Exceptions/                  # domain-specific rule failures
│   ├── CatalogItem.cs               # aggregate roots and entities follow Metamind's flat style
│   ├── CatalogBrand.cs
│   ├── CatalogType.cs
│   └── Catalog.Domain.csproj
├── Catalog.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   ├── Migrations/
│   │   └── CatalogContext.cs
│   ├── Repositories/                # implementations of Application ports
│   ├── Services/                    # external technical adapters
│   ├── EventHandlers/               # domain-event to outbox/integration-event mapping
│   ├── Messaging/                   # Outbox processor and RabbitMQ publisher
│   ├── Extensions/                  # AddCatalogInfrastructure
│   └── Catalog.Infrastructure.csproj
├── Catalog.Contracts/
│   ├── IntegrationEvents/
│   │   └── ProductCreatedV1.cs
│   └── Catalog.Contracts.csproj
├── Catalog.Test.Unit/
└── Catalog.Test.Integration/
```

Create a folder only when its first real member is needed; do not add empty
placeholder classes. A feature's command/query, handler, validator, response,
and mapper stay together in its feature folder. Controllers do not contain
business logic and never call `DbContext` directly.

### Contracts layout and ownership

`Catalog.Contracts` is not an API DTO project, a shared-kernel project, or a
place for interfaces. It contains only stable, serializable messages that
Catalog publishes to other modules:

```text
Catalog.Contracts/
└── IntegrationEvents/
    ├── ProductCreatedV1.cs
    ├── ProductPriceChangedV1.cs
    └── ProductStockChangedV1.cs
```

The owning module defines and versions its events. A consuming module takes a
reference only on the producer's Contracts project and keeps its RabbitMQ
consumer, handler, and local persistence inside its own Infrastructure project.
Command/query DTOs remain private to the producer's API/Application projects.

### Project-by-project placement rules

| Project | Contains | Must not contain |
| --- | --- | --- |
| `Module.API` | Controllers/endpoints, request binding, HTTP response mapping, authorization attributes, API registration extensions | Business rules, EF Core queries, repositories, RabbitMQ code, cross-module contracts |
| `Module.Application` | `Features/<Area>/<UseCase>` slices, commands, queries, handlers, validators, feature responses/mappers, and interfaces/ports required by a use case | ASP.NET types, EF Core types, controller code, RabbitMQ client types |
| `Module.Domain` | Aggregate roots, entities, value objects, domain events, domain exceptions, and invariant-enforcing methods | Persistence configuration, MediatR handlers, HTTP concerns, message-broker concerns |
| `Module.Infrastructure` | `Data`, EF configurations/migrations, repository and service implementations, messaging/Outbox code, event handlers, DI extensions | HTTP controllers and business decisions that belong on an aggregate |
| `Module.Contracts` | Versioned, serializable integration-event records published by the module | Commands, queries, API request/response types, domain entities, repositories, service interfaces |

Use the following layout for every new module, replacing `<Module>` and
`<Area>` with the module and business-area names:

```text
src/Modules/<Module>/
├── <Module>.API/
│   ├── Controllers/
│   └── Extensions/
├── <Module>.Application/
│   ├── Abstractions/
│   ├── Extensions/
│   └── Features/<Area>/<UseCase>/
│       ├── <UseCase>Command.cs or <UseCase>Query.cs
│       ├── <UseCase>Handler.cs
│       ├── <UseCase>Validator.cs                 # commands that accept input
│       └── <UseCase>Response.cs                  # only when this slice returns data
├── <Module>.Domain/
│   ├── Events/
│   ├── Exceptions/
│   └── ValueObjects/
├── <Module>.Infrastructure/
│   ├── Data/Configurations/
│   ├── Data/Migrations/
│   ├── EventHandlers/
│   ├── Extensions/
│   ├── Messaging/
│   ├── Repositories/
│   └── Services/
├── <Module>.Contracts/
│   └── IntegrationEvents/
├── <Module>.Test.Unit/
└── <Module>.Test.Integration/
```

## Dependency rules

```text
eShop.API --> module API --> module Application --> module Domain
                              |                  ^
                              v                  |
                       module Infrastructure -----+

other modules <-------- module Contracts
```

- A module may reference its own projects and BuildingBlocks projects only.
- `Contracts` has no references to Domain, Application, Infrastructure, or
  another module.
- A module must never query or write another module's database tables.
- Domain code has no dependency on EF Core, MediatR, RabbitMQ, ASP.NET Core, or
  infrastructure concerns.
- API endpoints depend on commands/queries and DTOs, never EF entities.

Architecture tests enforce these rules as modules are added.

## Catalog design

`CatalogItem` is the Catalog aggregate root. It owns product details, price,
availability, and stock thresholds. Changes occur only through intent-revealing
methods such as `Create`, `ChangePrice`, `AddStock`, and `RemoveStock`.

Business rules live in the aggregate and value objects. Commands coordinate a
use case; they do not duplicate domain rules. A command handler loads an
aggregate through a Catalog-owned port, invokes domain behavior, and saves the
transaction.

Application code is organised by vertical slice:

```text
Features/Products/CreateProduct/
  CreateProductCommand.cs
  CreateProductValidator.cs
  CreateProductHandler.cs
  CreateProductEndpoint.cs
```

Queries may use an efficient read projection; commands operate on aggregates.

## Communication

Within a module, requests are dispatched through MediatR using command/query
handlers and pipeline behaviours.

Across module boundaries, communication is asynchronous and event-driven:

1. A domain operation raises a domain event.
2. In the same database transaction, infrastructure maps relevant domain events
   to versioned integration events and writes them to the module Outbox table.
3. A background worker publishes pending Outbox messages to RabbitMQ.
4. Consumers use only the publishing module's `*.Contracts` package and save
   their own local state idempotently.

Domain events are private implementation details. Integration events are the
public, versioned boundary (for example, `ProductCreatedV1`).

## Data ownership and extraction path

Initially modules may share one SQL Server instance, but every module owns a
separate schema, DbContext, migrations, and Outbox table. This prevents
cross-module joins and makes eventual extraction a move of that module's
database, worker, API, and message configuration rather than a rewrite.

## Quality gates

- Unit tests cover aggregate invariants and application handlers.
- Integration tests use Testcontainers for SQL Server and RabbitMQ.
- Architecture tests forbid invalid project/assembly dependencies.
- Docker Compose runs the host, SQL Server, and RabbitMQ locally.
- GitHub Actions restores, builds, runs all tests, and later builds the
  container image.

## Delivery order

1. Stabilise compilation and module composition.
2. Complete the Catalog domain model and unit tests.
3. Implement Catalog vertical slices and HTTP endpoints.
4. Add SQL Server persistence, migrations, and integration tests.
5. Add the Outbox, RabbitMQ publishing, and event contracts.
6. Add Basket, Ordering, and Payments using the same module template.
