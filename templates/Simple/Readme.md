# 🏗️ Hexagonal Architecture Solution Template

A production-ready .NET template for building services following [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/) (also known as Ports and Adapters). It ships with Domain-Driven Design (DDD) patterns, CQRS-style use cases, full observability, and a complete multi-layer testing strategy out of the box.

---

## 📑 Table of Contents

1. [Project Overview](#-project-overview)
2. [Project Structure](#-project-structure)
3. [Getting Started](#-getting-started)
4. [Running the App](#-running-the-app)
5. [Testing Strategy](#-testing-strategy)
6. [Development Workflow](#-development-workflow)
7. [Helper Commands](#-helper-commands)
9. [Docker Setup](#-docker-setup)
10. [Monitoring & Telemetry](#-monitoring--telemetry)
11. [Contributing](#-contributing)

---

## 🌐 Project Overview

### What is Hexagonal Architecture?

Hexagonal Architecture isolates the core business logic from external concerns (databases, HTTP, messaging, caches) by defining explicit **ports** (interfaces) and **adapters** (implementations).

```mermaid
flowchart TD
    subgraph ExternalWorld["External World"]
        HTTP["HTTP"]
        DB["Database"]
        Cache["Cache"]
        InProcMessaging["In-Process Messaging"]
    end

    subgraph AppLayer["Core Layer"]
        UseCases["Use Cases / Ports / Orchestration / Entities / Rules "]
    end

    Adapters["Adapters"]

    ExternalWorld --> Adapters
    Adapters --> AppLayer

    style ExternalWorld fill:#e1f5ff
    style AppLayer fill:#f3e5f5
    style Adapters fill:#f0f4c3
```

### Key Design Decisions

| Concern | Approach |
|---|---|
| Business rules & domain | Encapsulated in aggregates and entities inside `Core` |
| Orchestration | Single-responsibility `UseCase` classes in `Core` |
| Persistence | Repository pattern with EF Core (PostgreSQL) |
| Caching | Hybrid cache (Redis + in-memory) via `IHybridCacheService` |
| Messaging | In-process `System.Threading.Channels` producers/consumers |
| API surface | ASP.NET Minimal API (REST) |
| Observability | OpenTelemetry → Grafana / Loki / Tempo / Prometheus / Pyroscope |
| Validation | DataAnnotations, validated automatically at the use case boundary |

---

## 📁 Project Structure

```
.
├── src/
│   ├── Core/                   # Domain entities, use cases, ports (interfaces), DTOs
│   ├── Infrastructure/         # Adapters: EF Core, Redis, in-process messaging, OpenTelemetry
│   └── WebApp/                 # Entry point: Minimal API endpoints
├── tests/
│   ├── CommonTests/            # Shared test utilities and base fixtures
│   ├── UnitTests/              # Isolated unit + architecture tests
│   ├── IntegrationTests/       # End-to-end slice tests with real infra
│   └── LoadTests/              # k6 performance scripts
└── scripts/
    ├── sql/                    # Migrations & seed SQL run by Docker
    └── grafana/                # Alloy, Loki, Prometheus, Tempo configs
```

### `src/Core/`

The innermost layer. Contains both domain logic and use case orchestration. Has **zero dependencies** on Infrastructure or WebApp.

```
Core/
├── Common/
│   ├── DomainEntity.cs         # Base class for all entities (Id, audit fields, soft-delete)
│   ├── Result.cs               # Railway-oriented Result<T> type for domain outcomes
│   ├── DefaultConfigurations.cs# App name, ActivitySource, Meter
│   ├── Attributes/             # Custom validation attributes (e.g. [NotDefault])
│   ├── Enums/                  # Domain-scoped enumerations
│   ├── Exceptions/             # Domain-specific exceptions
│   ├── Extensions/             # Pure domain extension methods
│   ├── Helpers/                # Structured logging helpers
│   ├── Messages/               # In-process message contracts (BaseMessage)
│   ├── Repositories/           # IBaseRepository port interface
│   ├── Requests/               # BaseRequest, BaseResponse, BasePaginatedRequest/Response
│   ├── Services/               # IHybridCacheService, IProduceService port interfaces
│   └── UseCases/               # BaseUseCase, BaseInOutUseCase, BaseInUseCase, BaseOutUseCase
├── Orders/                     # Orders aggregate + use cases + DTO
└── Notifications/              # Notifications aggregate + use cases + DTO
```

> **Design note:** All business invariants are enforced inside domain entities. Entity creation/mutations return `Result<T>` — always check `IsFailure` before using `.Value`.

### `src/Infrastructure/`

Contains all **adapter implementations**. Depends only on Core.

```
Infrastructure/
├── Data/
│   ├── MyDbContext.cs          # EF Core DbContext
│   ├── MyDbContextFactory.cs   # Design-time factory for EF CLI
│   ├── Migrations/             # EF Core migrations
│   ├── Mapping/                # Fluent API entity configurations
│   └── Common/
│       └── BaseRepository.cs   # IBaseRepository implementation
├── Cache/
│   └── Services/
│       └── HybridCacheService.cs   # IHybridCacheService implementation (Redis + IMemoryCache)
├── Messaging/
│   ├── Producers/
│   │   └── ProducerService.cs  # IProduceService — writes to Channel<TMessage>
│   └── Consumers/
│       ├── BaseConsumer.cs     # BackgroundService reading from Channel<TMessage>, idempotent
│       └── CreateNotificationConsumer.cs
├── Common/
│   ├── BaseBackgroundService.cs
│   └── BaseBackgroundChannelService.cs
└── OpenTelemetry/              # Tracing, metrics, logging wiring (OTLP + Pyroscope)
```

### `src/WebApp/`

The outermost adapter layer. Composes everything together and exposes the API.

```
WebApp/
├── Program.cs                  # DI registration & middleware pipeline
├── Endpoints/
│   ├── EndpointExtensions.cs   # Route registration entry point
│   └── OrderEndpoints.cs       # Minimal API route definitions
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
└── Extensions/
    └── HealthCheckExtensions.cs
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Minimum Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | `dotnet --version` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 4.x | For all backing services |
| [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | latest | `dotnet tool install --global dotnet-ef` |
| [k6](https://k6.io/docs/get-started/installation/) | ≥ 0.50 | For load tests only |
| [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/getting-started/) | latest | `dotnet tool install --global dotnet-stryker` |

### Installation

```bash
# 1. Restore NuGet packages
dotnet restore

# 2. Start backing services (PostgreSQL, Redis, full observability stack)
docker compose -f docker-compose-local.yml up -d

# 3. Apply database migrations
dotnet ef database update --project src/Infrastructure --startup-project src/WebApp

# 4. Run the app
dotnet run --project src/WebApp
```

> ⚠️ **Note:** `docker-compose-local.yml` also starts **pgAdmin** on port `5050` (credentials: `admin@admin.com` / `admin`) and the full observability stack (Grafana, Prometheus, Loki, Tempo, Pyroscope). The `postgres-init` container automatically runs `scripts/sql/migrations.sql` and `scripts/sql/seeds.sql` on first start.

---

## ▶️ Running the App

```bash
dotnet run --project src/WebApp
```

By default the app listens on:
- **REST API:** `https://localhost:7175` / `http://localhost:5010`
- **Health check:** `/health`

---

## 🧪 Testing Strategy

### Test Projects

| Project | Purpose | Runner |
|---|---|---|
| `CommonTests` | Shared fixtures and `BaseFixture` | — (library) |
| `UnitTests` | Domain logic, use case orchestration, architecture rules | `dotnet test` |
| `IntegrationTests` | Full HTTP slice tests against a real DB | `dotnet test` |
| `LoadTests` | HTTP performance benchmarks | `k6` |

### Unit Tests (`tests/UnitTests/`)

```
UnitTests/
├── Core/
│   ├── Common/                 # DomainEntity tests, mock extensions
│   ├── Orders/                 # CreateOrderUseCaseTests, GetOrderUseCaseTests, …
│   └── Notifications/          # CreateNotificationUseCaseTests, …
└── Architecture/               # Layer dependency and naming convention enforcement
```

**Naming convention:** `GivenContext_WhenCondition_ThenExpectedResult`

```csharp
[Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
public async Task GivenAValidRequestThenPass() { ... }
```

```bash
# All unit tests
dotnet test tests/UnitTests

# Single test class
dotnet test tests/UnitTests --filter "FullyQualifiedName~CreateOrderUseCaseTest"
```

### Integration Tests (`tests/IntegrationTests/`)

Spin up a real WebApp using `CustomWebApplicationFactory<Program>` against a local PostgreSQL instance (`Host=127.0.0.1;Port=5432;Database=OrderDb`).

```bash
dotnet test tests/IntegrationTests
```

### Load Tests (`tests/LoadTests/`)

Uses [k6](https://k6.io) to simulate concurrent HTTP traffic.

```bash
# Run locally against the running app
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full

# Or use the dedicated load test compose environment
docker compose -f docker-compose-load-tests.yml up -d
```

### Mutation Tests (Stryker.NET)

Thresholds: **high ≥ 90%, low ≥ 80%, break < 50%**.

```bash
cd tests/UnitTests
dotnet stryker --config-file stryker-config-core.json
```

HTML reports are written to `tests/UnitTests/StrykerOutput/`.

### Run All Tests (except load tests)

```bash
dotnet test
```

---

## 🔧 Development Workflow

### Adding a New Feature (e.g., `Products`)

Follow the dependency direction: **Core → Infrastructure → WebApp**.

**1. Core — define the entity and use cases**
```
src/Core/Products/Product.cs                  # Entity with invariants
src/Core/Products/CreateProductUseCase.cs
src/Core/Products/GetProductUseCase.cs
src/Core/Products/ProductDto.cs
```

Use cases extending `BaseInOutUseCase` are **auto-registered** by `CoreDependencyInjection.AddCore()` — no manual DI wiring needed.

**2. Infrastructure — add persistence mapping**
```
src/Infrastructure/Data/Mapping/ProductDbMapping.cs   # EF Core fluent config
```

Create and apply the migration:
```bash
dotnet ef migrations add AddProduct \
  --project src/Infrastructure \
  --startup-project src/WebApp \
  --output-dir Data/Migrations

dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/WebApp
```

**3. WebApp — expose the endpoint**
```
src/WebApp/Endpoints/ProductEndpoints.cs
```
Register routes in `EndpointExtensions.cs`.

**4. Tests — cover every layer**
```
tests/UnitTests/Core/Products/
tests/IntegrationTests/WebApp/Http/Products/
```

> ✅ Architecture tests in `tests/UnitTests/Architecture/` automatically enforce that new code respects the dependency rules and naming conventions.

---

## 🛠️ Helper Commands

### Database Migrations

```bash
# Apply pending migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/WebApp

# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/WebApp \
  --output-dir Data/Migrations

# Generate idempotent SQL script (for CI/CD deployments)
dotnet ef migrations script --idempotent \
  --project src/Infrastructure \
  --startup-project src/WebApp \
  --output scripts/sql/migrations.sql
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
dotnet stryker --config-file stryker-config-core.json
```

### Load Tests

```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

---

## 🐳 Docker Setup

### `docker-compose-local.yml` — Development environment

Starts all backing services for local development, including the full observability stack:

| Service | Port(s) | Purpose |
|---|---|---|
| PostgreSQL 17 | `5432` | Primary database |
| pgAdmin | `5050` | Database GUI (`admin@admin.com` / `admin`) |
| Redis 8 | `6379` | Distributed cache |
| Grafana Alloy | `4317`, `4318` | Telemetry collector (OTLP) |
| Prometheus | `9090` | Metrics |
| Loki | `3100` | Log aggregation |
| Grafana | `3000` | Dashboards |
| Tempo | `3200` | Distributed tracing |
| Pyroscope | `4040` | Continuous profiling |

```bash
docker compose -f docker-compose-local.yml up -d

# Tear down (keeps volumes)
docker compose -f docker-compose-local.yml down

# Tear down including volumes
docker compose -f docker-compose-local.yml down -v
```

### `docker-compose.yml` — Minimal environment (no observability)

Starts only PostgreSQL, Redis, and pgAdmin — useful when you don't need the monitoring stack.

```bash
docker compose up -d
```

### `docker-compose-load-tests.yml` — Load test environment

Spins up the full stack plus the app container and runs k6 automatically.

```bash
docker compose -f docker-compose-load-tests.yml up -d
```

### Build & run the app in Docker

```bash
docker build -t hexagonal-template .
docker run -p 5000:5000 hexagonal-template
```

---

## 📊 Monitoring & Telemetry

The template uses **OpenTelemetry** with OTLP exporters, collected and routed by **Grafana Alloy**.

```mermaid
graph TB
    WebApp["WebApp<br/>(OTLP)"]
    Alloy["Grafana Alloy<br/>(Collector)"]
    Loki["Loki<br/>(Logs)"]
    Tempo["Tempo<br/>(Traces)"]
    Prometheus["Prometheus<br/>(Metrics)"]
    Pyroscope["Pyroscope<br/>(Profiles)"]
    Grafana["Grafana<br/>(Dashboards)"]

    WebApp -->|OTLP| Alloy
    Alloy -->|logs| Loki
    Alloy -->|traces| Tempo
    Alloy -->|metrics| Prometheus
    Alloy -->|profiles| Pyroscope
    Prometheus --> Grafana
    Loki --> Grafana
    Tempo --> Grafana

    style WebApp fill:#bbdefb
    style Alloy fill:#c8e6c9
    style Loki fill:#fff9c4
    style Tempo fill:#ffe0b2
    style Prometheus fill:#f8bbd0
    style Pyroscope fill:#e1bee7
    style Grafana fill:#d1c4e9
```

### Accessing the dashboards

```bash
docker compose -f docker-compose-local.yml up -d
dotnet run --project src/WebApp
```

| Dashboard | URL | Credentials |
|---|---|---|
| Grafana | <http://localhost:3000> | anonymous (admin role) |
| Prometheus | <http://localhost:9090> | — |
| pgAdmin | <http://localhost:5050> | `admin@admin.com` / `admin` |
| Pyroscope | <http://localhost:4040> | — |

### Configuration files

| File | Purpose |
|---|---|
| `scripts/grafana/config.alloy` | Grafana Alloy pipeline (scrape → export) |
| `scripts/grafana/datasources.yaml` | Pre-provisioned Grafana data sources |
| `scripts/grafana/prometheus.yml` | Prometheus scrape targets |
| `scripts/grafana/loki-config.yaml` | Loki storage configuration |
| `scripts/grafana/tempo.yaml` | Tempo trace storage configuration |

> ✅ **Tip:** Structured logs include a `CorrelationId` that can be used to pivot from a log entry directly to the trace in Tempo's Explore view in Grafana.

---

## 🤝 Contributing

### Guidelines

1. Follow the layer dependency rules enforced by the architecture tests.
2. Every new use case must have unit tests following the `GivenContext_WhenCondition_ThenExpectedResult` naming convention.
3. Run `dotnet test` and the Stryker config before opening a pull request.
4. Keep domain entities free from infrastructure concerns — no EF Core attributes on entities in `Core`.

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
