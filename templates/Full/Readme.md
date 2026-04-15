# 🏗️ Hexagonal Architecture Solution Template

A production-ready .NET template for building applications following [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/) (also known as Ports and Adapters). It ships with Domain-Driven Design (DDD) patterns, CQRS-style use cases, full observability, and a complete multi-layer testing strategy out of the box.

---

## 📑 Table of Contents

1. [Project Overview](#-project-overview)
2. [Project Structure](#-project-structure)
3. [Getting Started](#-getting-started)
4. [Running the Application](#-running-the-application)
5. [Testing Strategy](#-testing-strategy)
6. [Development Workflow](#-development-workflow)
7. [AI Agent Skills](#-ai-agent-skills)
8. [Helper Commands](#-helper-commands)
9. [Docker Setup](#-docker-setup)
10. [Monitoring & Telemetry](#-monitoring--telemetry)
11. [Contributing](#-contributing)

---

## 🌐 Project Overview

### What is Hexagonal Architecture?

Hexagonal Architecture isolates the core business logic from external concerns (databases, HTTP, messaging, caches) by defining explicit **ports** (interfaces) and **adapters** (implementations).

```
            ┌─────────────────────────────────────┐
            │          External World              │
            │  HTTP  │  gRPC  │  Message Bus  │  DB│
            └────────┬───────────────┬─────────────┘
                     │   Adapters    │
            ┌────────▼───────────────▼─────────────┐
            │         Application Layer            │
            │  (Use Cases / Ports / Orchestration) │
            └────────────────┬─────────────────────┘
                             │
            ┌────────────────▼─────────────────────┐
            │            Domain Layer              │
            │  (Entities, Rules, Domain Events)    │
            └──────────────────────────────────────┘
```

### Key Design Decisions

| Concern | Approach |
|---|---|
| Business rules | Encapsulated in `Domain` aggregates and entities |
| Orchestration | Single-responsibility `UseCase` classes in `Application` |
| Persistence | Repository pattern with EF Core (PostgreSQL) |
| Caching | Hybrid cache (Redis + in-memory) via `IHybridCacheService` |
| Messaging | RabbitMQ producers/consumers via MassTransit |
| API surface | ASP.NET Minimal API (REST) + gRPC |
| Observability | OpenTelemetry → Grafana / Loki / Tempo / Prometheus |
| Validation | FluentValidation, validated at the application boundary |

---

## 📁 Project Structure

```
.
├── src/
│   ├── Domain/                 # Core business logic – no external dependencies
│   ├── Application/            # Use cases, ports (interfaces), DTOs
│   ├── Infrastructure/         # Adapters: EF Core, Redis, RabbitMQ, OpenTelemetry
│   └── WebApp/                 # Entry point: Minimal API + gRPC
├── tests/
│   ├── CommonTests/            # Shared test utilities and base fixtures
│   ├── UnitTests/              # Isolated unit + architecture tests
│   ├── IntegrationTests/       # End-to-end slice tests with real infra
│   └── LoadTests/              # k6 performance scripts
└── scripts/
    ├── sql/                    # Migrations & seed SQL run by Docker
    └── grafana/                # Alloy, Loki, Prometheus, Tempo configs
```

### `src/Domain/`

The innermost layer. Has **zero dependencies** on any other project or NuGet infrastructure package.

```
Domain/
├── Common/
│   ├── DomainEntity.cs         # Base class for all domain entities (Id, timestamps)
│   ├── Result.cs               # Railway-oriented Result<T> type for error handling
│   ├── DefaultConfigurations.cs
│   ├── Enums/                  # Domain-scoped enumerations
│   ├── Exceptions/             # Domain-specific exceptions
│   └── Extensions/             # Pure domain extension methods
├── Orders/                     # Orders aggregate (entity, value objects, domain events)
└── Notifications/              # Notifications aggregate
```

> **Design note:** All business invariants are enforced inside domain entities and aggregates — never in services or controllers.

### `src/Application/`

Orchestrates domain objects and coordinates infrastructure through **ports (interfaces)**. Each use case lives in its own file and does exactly one thing.

```
Application/
├── Common/
│   ├── UseCases/               # BaseUseCase<TRequest, TResponse> base class
│   ├── Repositories/           # IBaseRepository<T> port interface
│   ├── Services/               # IHybridCacheService, IProduceService ports
│   ├── Requests/               # Shared request base types
│   ├── Messages/               # Integration message contracts (RabbitMQ)
│   └── Helpers/                # Mapping helpers, extension methods
├── Orders/
│   ├── CreateOrderUseCase.cs
│   ├── GetOrderUseCase.cs
│   ├── GetAllOrdersUseCase.cs
│   └── OrderDto.cs
└── Notifications/
    ├── CreateNotificationUseCase.cs
    ├── GetNotificationUseCase.cs
    ├── GetAllNotificationsUseCase.cs
    └── NotificationDto.cs
```

> **Design note:** Use cases accept a `Request` record and return a `Result<Response>`. They validate input via FluentValidation, call the domain, persist through the repository port, and optionally publish messages.

### `src/Infrastructure/`

Contains all **adapter implementations**. Depends on Application (for port interfaces) and Domain.

```
Infrastructure/
├── Data/
│   ├── MyDbContext.cs          # EF Core DbContext
│   ├── Migrations/             # EF Core migrations
│   ├── Mapping/                # Fluent API entity configurations
│   └── Repositories/           # IBaseRepository<T> implementations
├── Cache/                      # IHybridCacheService implementation (Redis + IMemoryCache)
├── Messaging/
│   ├── Producers/              # RabbitMQ message publishers (MassTransit)
│   └── Consumers/              # RabbitMQ message consumers (MassTransit)
└── OpenTelemetry/              # Tracing, metrics, logging wiring (OTLP exporter)
```

### `src/WebApp/`

The outermost adapter layer. Composes everything together and exposes the API.

```
WebApp/
├── Program.cs                  # DI registration & middleware pipeline
├── Endpoints/
│   └── OrderEndpoints.cs       # Minimal API route definitions
├── GrpcServices/               # gRPC service implementations
├── Protos/                     # .proto definition files
├── Middlewares/                # Custom middleware (error handling, correlation IDs, etc.)
└── Extensions/                 # WebApplication builder helpers
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Minimum Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0 | `dotnet --version` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 4.x | For all backing services |
| [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | 9.0 | `dotnet tool install --global dotnet-ef` |
| [k6](https://k6.io/docs/get-started/installation/) | ≥ 0.50 | For load tests only |
| [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/getting-started/) | latest | `dotnet tool install --global dotnet-stryker` |

### Installation

```bash
# 1. Restore NuGet packages
dotnet restore

# 2. Start backing services (PostgreSQL, Redis, RabbitMQ)
docker compose -f docker-compose-local.yml up -d

# 3. Apply database migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Infrastructure

# 4. Run the application
dotnet run --project src/WebApp
```

> ⚠️ **Note:** `docker-compose-local.yml` also starts **pgAdmin** on port `5050` (credentials: `admin@admin.com` / `admin`). Use it to browse the database during development.

---

## ▶️ Running the Application

```bash
dotnet run --project src/WebApp
```

By default the app listens on:
- **REST API:** `https://localhost:7000` (check `Properties/launchSettings.json`)
- **gRPC:** configured in `WebApp/GrpcServices/`
- **Health check:** `/health`
- **Swagger / OpenAPI:** `/swagger`

You can also use the included `.http` file for quick manual testing:

```
src/WebApp/WebApp.http
```

---

## 🧪 Testing Strategy

This template enforces a multi-layer testing strategy. See [TEST_STRUCTURE_GUIDE.md](TEST_STRUCTURE_GUIDE.md) for detailed patterns and conventions.

### Test Projects

| Project | Purpose | Runner |
|---|---|---|
| `CommonTests` | Shared fixtures, `BaseFixture`, `BaseApplicationFixture<T>` | — (library) |
| `UnitTests` | Domain logic, use case orchestration, architecture rules | `dotnet test` |
| `IntegrationTests` | Full HTTP/gRPC slice tests against a real DB | `dotnet test` |
| `LoadTests` | Performance and throughput benchmarks | `k6` |

### Unit Tests (`tests/UnitTests/`)

```
UnitTests/
├── Domain/                     # Entity invariants, Result type, value objects
├── Application/
│   ├── Orders/                 # CreateOrderUseCaseTests, GetOrderUseCaseTests, …
│   └── Notifications/          # CreateNotificationUseCaseTests, …
└── Architecture/               # ArchUnit-style dependency direction rules
```

**Naming convention:** `GivenContext_WhenCondition_ThenExpectedResult`

```csharp
[Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
public async Task GivenAValidRequestThenPass() { ... }

[Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
public async Task GivenAValidRequestWhenOrderNotFoundThenFails() { ... }
```

**Run unit tests:**
```bash
dotnet test tests/UnitTests
```

### Integration Tests (`tests/IntegrationTests/`)

Spin up a real WebApp using `WebApplicationFactory<Program>` against a PostgreSQL test database.

```bash
dotnet test tests/IntegrationTests
```

### Load Tests (`tests/LoadTests/`)

Uses [k6](https://k6.io) to simulate concurrent traffic against the running application.

```bash
# REST API load test
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full

# gRPC load test
k6 run tests/LoadTests/scriptGrpc.js --summary-mode=full

# Start full monitoring stack first for metrics in Grafana
docker compose up -d
```

### Mutation Tests (Stryker.NET)

Mutation testing validates test suite quality by introducing code faults and checking that tests catch them. Thresholds: **high ≥ 90%, low ≥ 80%, break < 50%**.

```bash
cd tests/UnitTests

# Mutate Application layer
dotnet stryker --config-file stryker-config-application.json

# Mutate Domain layer
dotnet stryker --config-file stryker-config-domain.json
```

HTML reports are written to `tests/UnitTests/StrykerOutput/`.

### Run All Tests (except load tests)

```bash
dotnet test
```

---

## 🔧 Development Workflow

### Adding a New Feature (e.g., `Products`)

Follow the dependency direction: **Domain → Application → Infrastructure → WebApp**.

**1. Domain — define the entity**
```
src/Domain/Products/Product.cs          # Entity with invariants
```

**2. Application — add use case(s)**
```
src/Application/Products/CreateProductUseCase.cs
src/Application/Products/GetProductUseCase.cs
src/Application/Products/ProductDto.cs
```

**3. Infrastructure — add persistence**
```
src/Infrastructure/Data/Mapping/ProductConfiguration.cs   # EF Core config
```
Then create and apply a migration:
```bash
dotnet ef migrations add AddProduct --project src/Infrastructure --startup-project src/Infrastructure --output-dir Data/Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Infrastructure
```

**4. WebApp — expose the endpoint**
```
src/WebApp/Endpoints/ProductEndpoints.cs
```
Register routes in `EndpointExtensions.cs`.

**5. Tests — cover every layer**
```
tests/UnitTests/Domain/Products/
tests/UnitTests/Application/Products/
tests/IntegrationTests/WebApp/Products/
```

> ✅ **Tip:** Architecture tests in `tests/UnitTests/Architecture/` will automatically enforce that your new code respects the dependency rules.

---

## 🤖 AI Agent Skills

This template includes prebuilt Copilot skills to standardize generation and reduce implementation drift across contributors.

### Skills location

All skills are stored under:

```
.github/skills/
```

Each skill contains:
- `SKILL.md` — behavior, activation criteria, and conventions
- `references/` — local templates and snippets used by the skill (self-contained)

### Available skills

| Skill | Purpose |
|---|---|
| `entity-generator` | Generate domain entities with DDD patterns, tests, and persistence guidance |
| `use-case-generator` | Generate Application use cases (`BaseInOutUseCase`, `BaseInUseCase`, `BaseOutUseCase`) with validators, notification patterns, and xUnit unit tests with fixtures, mocks, and logging/repository verification |
| `endpoints-generator` | Generate ASP.NET Minimal API endpoints with cache, correlation ID, status code conventions, and mandatory `src/WebApp/WebApp.http` samples for local endpoint testing |
| `consumers-generator` | Generate RabbitMQ consumers and message contracts using `BaseConsumer<TMessage, TConsumer>` patterns |
| `ef-mapper` | Generate/update EF Core mapping classes with precision, enum conversion, relationships, and migration guidance |
| `integration-tests-generator` | Generate integration tests for HTTP, gRPC, and messaging flows using `WebApplicationFactory` conventions |
| `load-tests-generator` | Generate k6 load test scripts for HTTP/gRPC with thresholds, metrics, and env-driven profiles |

### How to use

Ask Copilot with intent that matches the skill. Examples:

- "Create a new domain entity with tests for Product"
- "Generate a use case and validator for updating product prices"
- "Add minimal API endpoints for Products"
- "Create integration tests for the new endpoint"
- "Generate a k6 script for the products GET endpoint"

> ✅ **Best practice:** Keep skill templates and references in `./references` inside each skill folder. This prevents breakage if source files are moved or renamed.

> 🧪 **Endpoint skill rule:** Whenever `endpoints-generator` creates a new endpoint route, add/update the corresponding request sample in `src/WebApp/WebApp.http` so local manual tests stay in sync with the API contract.

---

## 🛠️ Helper Commands

### Database Migrations

```bash
# Apply pending migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Infrastructure

# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/Infrastructure \
  --output-dir Data/Migrations

# Generate idempotent SQL script (for CI/CD deployments)
dotnet ef migrations script --idempotent \
  --project src/Infrastructure \
  --startup-project src/Infrastructure \
  --output scripts/sql/migrations.sql
```

### Running the App

```bash
dotnet run --project src/WebApp

# Hot reload during development
dotnet watch run --project src/WebApp
```

### Running Tests

```bash
# All tests (excludes load tests)
dotnet test

# Unit tests only
dotnet test tests/UnitTests

# Integration tests only
dotnet test tests/IntegrationTests

# With coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Mutation Tests

```bash
cd tests/UnitTests
dotnet stryker --config-file stryker-config-application.json
dotnet stryker --config-file stryker-config-domain.json
```

### Load Tests

```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
k6 run tests/LoadTests/scriptGrpc.js --summary-mode=full
```

---

## 🐳 Docker Setup

### `docker-compose-local.yml` — Development environment

Starts all backing services for local development:

| Service | Port(s) | Purpose |
|---|---|---|
| PostgreSQL 17 | `5432` | Primary database |
| pgAdmin | `5050` | Database GUI |
| Redis 8 | `6379` | Distributed cache |
| RabbitMQ | `5672` / `15672` | Message broker / Management UI |

```bash
docker compose -f docker-compose-local.yml up -d

# Tear down (keeps volumes)
docker compose -f docker-compose-local.yml down

# Tear down including volumes
docker compose -f docker-compose-local.yml down -v
```

> ⚠️ **Note:** On first start, `postgres-init` automatically runs `scripts/sql/migrations.sql` and `scripts/sql/seeds.sql` inside the database container.

### `docker-compose.yml` — Full stack (with observability)

Adds the complete monitoring stack on top of the local services:

| Service | Port | Purpose |
|---|---|---|
| Grafana | `3000` | Dashboards |
| Prometheus | `9090` | Metrics scraping |
| Loki | `3100` | Log aggregation |
| Tempo | `4317` | Distributed tracing (OTLP) |
| Grafana Alloy | — | Telemetry collector |

```bash
docker compose up -d
```

### `docker-compose-load-tests.yml` — Load test environment

Spins up a stable environment optimized for k6 load tests.

```bash
docker compose -f docker-compose-load-tests.yml up -d
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

### Build & run the application in Docker

```bash
docker build -t hexagonal-template .
docker run -p 8080:8080 hexagonal-template
```

---

## 📊 Monitoring & Telemetry

The template uses **OpenTelemetry** with OTLP exporters, collected and routed by **Grafana Alloy**.

### Stack

```
WebApp (OTLP)
    └─► Grafana Alloy (collector)
            ├─► Loki       (logs)
            ├─► Tempo      (traces)
            └─► Prometheus (metrics)
                    └─► Grafana (dashboards)
```

### Accessing the dashboards

Start the full stack first:

```bash
docker compose up -d
dotnet run --project src/WebApp
```

| Dashboard | URL | Credentials |
|---|---|---|
| Grafana | <http://localhost:3000> | `admin` / `admin` |
| Prometheus | <http://localhost:9090> | — |
| RabbitMQ | <http://localhost:15672> | `guest` / `guest` |
| pgAdmin | <http://localhost:5050> | `admin@admin.com` / `admin` |

### Configuration files

| File | Purpose |
|---|---|
| `scripts/grafana/config.alloy` | Grafana Alloy pipeline (scrape → export) |
| `scripts/grafana/datasources.yaml` | Pre-provisioned Grafana data sources |
| `scripts/grafana/prometheus.yml` | Prometheus scrape targets |
| `scripts/grafana/loki-config.yaml` | Loki storage configuration |
| `scripts/grafana/tempo.yaml` | Tempo trace storage configuration |

> ✅ **Tip:** Structured logs from the application include a `CorrelationId` that can be used to pivot from a log entry directly to the trace in Tempo's Explore view in Grafana.

---

## 🤝 Contributing

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)

### Guidelines

1. Follow the layer dependency rules enforced by the architecture tests.
2. Every new use case must have unit tests following the `GivenContext_WhenCondition_ThenExpectedResult` naming convention.
3. Run `dotnet test` and both Stryker configs before opening a pull request.
4. Keep domain entities free from infrastructure concerns — no EF Core attributes inside `Domain/`.
