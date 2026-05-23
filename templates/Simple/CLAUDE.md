# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build src/WebApp/WebApp.csproj

# Run
dotnet watch run --project src/WebApp/WebApp.csproj

# Unit tests (all)
dotnet test tests/UnitTests/UnitTests.csproj

# Unit tests (single test class)
dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~CreateOrderUseCaseTest"

# Integration tests (requires PostgreSQL on 127.0.0.1:5432 and Redis on localhost:6379)
dotnet test tests/IntegrationTests/IntegrationTests.csproj

# Mutation tests
dotnet stryker --config-file tests/UnitTests/stryker-config-core.json

# EF Core migrations (run from repo root)
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApp
dotnet ef database update --project src/Infrastructure --startup-project src/WebApp
```

## Architecture

This is a **hexagonal (ports & adapters)** template with three layers enforced by architecture tests:

- **`src/Core`** — Domain and use cases. No references to Infrastructure or WebApp allowed. Contains domain entities, use case base classes, repository/service interfaces, request/response records, and messages.
- **`src/Infrastructure`** — Adapters: EF Core (PostgreSQL via `IDbContextFactory<MyDbContext>`), Redis HybridCache, in-process messaging via `System.Threading.Channels`, and OpenTelemetry + Pyroscope.
- **`src/WebApp`** — Minimal API endpoints, health checks, exception middleware. References Infrastructure only.

### Use Cases

All use cases inherit from one of three generic base classes in `Core.Common.UseCases`:

| Base class | Interface | When to use |
|---|---|---|
| `BaseInOutUseCase<TRequest, TResponse>` | `IBaseInOutUseCase<TRequest, TResponse>` | Takes input, returns output |
| `BaseInUseCase<TRequest>` | `IBaseInUseCase<TRequest>` | Takes input, no output |
| `BaseOutUseCase<TResponse>` | `IBaseOutUseCase<TResponse>` | No input, returns output |

`CoreDependencyInjection.AddCore()` auto-registers all `*UseCase` classes by scanning the assembly — no manual registration needed.

The base classes handle: input validation (DataAnnotations on `BaseRequest`), OpenTelemetry activity tracing, metrics (`UseCaseExecutedMetric` / `UseCaseFailedMetric`), and structured logging. Implementations override `HandleInternalAsync` only.

### Request/Response contracts

- `BaseRequest` requires a non-default `CorrelationId` (Guid) enforced by `[NotDefault]`.
- `BaseResponse` / `BaseResponse<TData>` / `BasePaginatedResponse<TData>` are records used at all layer boundaries.
- `Result` / `Result<T>` are used inside domain entities for operation outcomes and are **not** the HTTP response type.

### Domain entities

All entities extend `DomainEntity`, which provides `Id`, audit fields (`CreatedAt/By`, `UpdatedAt/By`), and soft-delete (`IsDeleted`, `DeletedAt/By`). Entity creation returns `Result<T>` — check `IsFailure` before using `.Value`.

### Messaging (in-process)

`IProduceService` writes to a typed `Channel<TMessage>`. Consumers extend `BaseConsumer<TMessage, TConsumer>` which is a `BackgroundService` reading from the channel. Idempotency is enforced in `BaseConsumer` via HybridCache (key: `{ConsumerName}-{CorrelationId}`).

New message types require:
1. A record extending `BaseMessage` in `Core.Common.Messages`
2. A `Channel<TMessage>` singleton registered in `MessagingDependencyInjection`
3. A consumer class extending `BaseConsumer<TMessage, TConsumer>` registered as `IHostedService`

### Naming conventions (enforced by architecture tests)

- Core must not contain classes ending in: `Controller`, `Endpoint`, `DbContext`, `Mapping`, `Consumer`, `Producer`
- Core must not reference `Infrastructure` or `WebApp` assemblies
- All types in Core must be in the `Core.*` namespace

## Infrastructure dependencies

| Service | Config key | Default |
|---|---|---|
| PostgreSQL | `ConnectionStrings:OrderDb` | `Host=127.0.0.1;Port=5432;Database=OrderDb;Username=postgres;Password=cY5VvZkkh4AzES` |
| Redis | `ConnectionStrings:Redis` | `localhost:6379` |

Set `ENABLE_SENSITIVE_DATA_LOGGING=true` to enable EF Core query logging.

OpenTelemetry exports via OTLP; set `ASPNETCORE_ENVIRONMENT=IntegrationTests` to disable it during test runs.

## Testing patterns

**Unit tests** use `BaseCoreFixture<TRequest, TUseCase>` which pre-wires mocks for `IServiceProvider`, `IBaseRepository`, `IHybridCacheService`, and `IProduceService`. Each test class creates its own fixture class inheriting from it. Call `_fixture.ClearInvocations()` in the constructor, not `SetUp`.

**Integration tests** use `CustomWebApplicationFactory<Program>` which swaps the pooled DbContextFactory for a regular one targeting the local PostgreSQL instance. Tests are grouped under `[Collection(nameof(WebApplicationFactoryCollectionDefinition))]`.

Architecture tests in `tests/UnitTests/Architecture/` use `ArchitectureTestHelper` and run as part of unit tests — they validate layer isolation and naming conventions.
