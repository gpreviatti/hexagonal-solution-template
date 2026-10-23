# Hexagonal Architecture Solution Template (BFF)

This repository provides a ready-to-use **Backend For Frontend (BFF)** template built with **Hexagonal Architecture** (Ports and Adapters).

The goal is to keep business contracts and API surfaces clean while making adapters (HTTP, gRPC, cache, observability, resilience) easy to evolve independently.

## Table of Contents

- [Architecture at a glance](#architecture-at-a-glance)
- [Project structure (tree view)](#project-structure-tree-view)
- [Detailed project structure](#detailed-project-structure)
  - [Solution root](#solution-root)
  - [`src/Contracts`](#srccontracts)
  - [`src/Infrastructure`](#srcinfrastructure)
  - [`src/WebApp`](#srcwebapp)
  - [`src/MockApi`](#srcmockapi)
  - [`tests/CommonTests`](#testscommontests)
  - [`tests/IntegrationTests`](#testsintegrationtests)
  - [`tests/LoadTests`](#testsloadtests)
- [Copilot skills included in this template](#copilot-skills-included-in-this-template)
  - [Using skills with `@runSubagent`](#using-skills-with-runsubagent)
- [Helper commands](#helper-commands)
- [Contribute](#contribute)

## Architecture at a glance

- **Inbound adapters:** `src/WebApp` minimal APIs and endpoint composition.
- **Contracts (port models):** `src/Contracts` request/response DTOs and gRPC `.proto` files.
- **Outbound adapters:** `src/Infrastructure` HTTP and gRPC service adapters, cache, retry policies, telemetry/logging setup.
- **Mock external systems:** `src/MockApi` local fake downstream HTTP + gRPC APIs.
- **Quality gates:** `tests/CommonTests`, `tests/IntegrationTests`, and `tests/LoadTests`.

## Project structure (tree view)

```text
.
├── src
│   ├── Contracts
│   │   ├── Common
│   │   ├── Orders
│   │   └── Protos
│   ├── Infrastructure
│   │   ├── Cache
│   │   ├── Common
│   │   ├── Grpc
│   │   └── Http
│   ├── MockApi
│   │   ├── Endpoints
│   │   └── GrpcServices
│   └── WebApp
│       ├── Endpoints
│       ├── Extensions
│       └── Middlewares
├── tests
│   ├── CommonTests
│   ├── IntegrationTests
│   │   ├── Common
│   │   ├── Fixtures
│   │   └── WebApp
│   │       ├── Grpc
│   │       └── Http
│   └── LoadTests
│       └── protos
└── scripts
  └── grafana
```

## Detailed project structure

### Solution root

- `Hexagonal.Solution.Template.Bff.slnx`: solution entry point.
- `Directory.Build.props` / `Directory.Packages.props`: centralized build and package management.
- `docker-compose.yml` / `docker-compose-local.yml` / `docker-compose-load-tests.yml`: local and load-test environments.
- `Dockerfile` / `Dockerfile.MockApi`: app and mock API container definitions.
- `scripts/grafana/`: local observability stack configuration (Grafana/Tempo/Loki/Prometheus).

### `src/Contracts`

Shared message contracts used by the BFF and tests.

- `Common/`
  - `BaseRequest.cs`, `BaseResponse.cs`: standard wrappers (correlation-aware base contracts).
- `Orders/`
  - `CreateOrderRequest.cs`, `OrderDto.cs`: order-specific request and response contracts.
- `Protos/`
  - `payment.proto`: gRPC contract for payment flows.

### `src/Infrastructure`

Adapters and cross-cutting technical concerns.

- `InfrastructureDependencyInjection.cs`: DI composition for cache, HTTP clients, gRPC clients, OpenTelemetry, retry policies.
- `Cache/HybridCacheService.cs`: distributed + local cache access strategy.
- `Http/BaseHttpService.cs`: reusable outbound HTTP adapter.
- `Grpc/PaymentsService.cs`: outbound gRPC adapter.
- `Common/`: constants, service keys, logging and default configuration helpers.

### `src/WebApp`

Inbound API layer (Minimal APIs).

- `Program.cs`: app bootstrap, OpenAPI, health checks, rate limiting, middleware pipeline.
- `Endpoints/`
  - `EndpointExtensions.cs`: endpoint registration root.
  - `OrderEndpoints.cs`: order endpoints (GET/POST).
  - `PaymentEndpoints.cs`: payment endpoint(s).
- `Extensions/`, `Middlewares/`: host and pipeline concerns.

### `src/MockApi`

Local fake dependencies used for development and tests.

- `Program.cs`: mock host startup.
- `Endpoints/OrderEndpoints.cs`: fake HTTP order endpoints.
- `GrpcServices/PaymentService.cs`: fake gRPC service.

### `tests/CommonTests`

Common unit-test support package and shared test utilities.

### `tests/IntegrationTests`

End-to-end API integration tests against `WebApp`.

- `Common/CustomWebApplicationFactory.cs`: host factory for test server.
- `WebApp/Http/`: HTTP integration fixtures/helpers/tests.
- `WebApp/Grpc/`: gRPC integration helpers/tests.

### `tests/LoadTests`

k6 scripts for performance validation.

- `scriptHttp.js`: HTTP load scenarios and thresholds.
- `scriptGrpc.js`: gRPC load scenarios and thresholds.
- `protos/`: protobuf contracts used by load tests.

## Copilot skills included in this template

This template now includes dedicated skills under `.github/skills/`:

- `create-contracts`: generate/update contract records and proto definitions.
- `create-endpoint-with-integration-tests`: create a new endpoint and its integration tests.
- `create-load-tests`: create or extend k6 load-test scripts and thresholds.

## Helper commands

### Start docker compose

```bash
docker-compose up -d
```

### Run load tests with full summary for HTTP script

```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

### Run load tests with full summary for gRPC script

```bash
k6 run tests/LoadTests/scriptGrpc.js --summary-mode=full
```

### Run tests (except load tests)

```bash
dotnet test
```

### Run common/unit tests

```bash
dotnet test tests/CommonTests
```

### Run integration tests

```bash
dotnet test tests/IntegrationTests
```

## Development Guide

The short rules for AI agents are in [.github/AGENTS.md](.github/AGENTS.md).

### Contracts

- HTTP request records inherit from `BaseRequest(Guid CorrelationId)`. Paginated requests inherit from `BasePaginatedRequest`.
- Use `sealed record` for immutable DTOs and `decimal` for money, never `float` or `double`.
- Keep gRPC field numbers in `src/Contracts/Protos/*.proto` stable.
- Add XML documentation to public contract types and notable fields.

```csharp
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId);
```

### Endpoints (Minimal APIs)

- **Location:** `src/WebApp/Endpoints/<Feature>Endpoints.cs`, a static internal class with a `MapXEndpoints(this WebApplication app)` extension method.
- **Grouping:** `app.MapGroup(serviceKey).WithTags(serviceKey).RequireRateLimiting(serviceKey)`, where `serviceKey = ServicesKey.<Feature>.ToString()`.
- **Dependencies:** `[FromKeyedServices(ServicesKey.<Feature>)]` or `[FromServices]`.
- **Correlation:** `[FromHeader] Guid? correlationId = null`, defaulting to `Guid.NewGuid()`.
- **Responses:** wrap in `BaseResponse<T>` and return `Results.Ok`, `Results.NotFound`, or `Results.BadRequest`.
- **OpenAPI:** add `.Produces<T>(statusCode)`, `.WithDescription(...)`, and `.WithName(...)`.
- **Tracing:** wrap logic in `DefaultConfigurations.ActivitySource.StartActivity(...)`.
- **Caching:** use `HybridCacheService.GetOrCreateAsync(...)` where it fits.

```csharp
ordersGroup.MapGet("/{id}", async (
    [FromRoute] int id,
    [FromKeyedServices(ServicesKey.Orders)] BaseHttpService httpService,
    [FromServices] HybridCacheService cache,
    [FromHeader] Guid? correlationId = null
) => {
    using var activity = DefaultConfigurations.ActivitySource.StartActivity($"{nameof(OrderEndpoints)}.GetById");
    var response = await cache.GetOrCreateAsync(
        $"order_{id}", async () => await httpService.GetAsync<BaseResponse<OrderDto>>($"orders/{id}"));
    return response != null ? Results.Ok(response) : Results.NotFound();
})
.Produces<BaseResponse<OrderDto>>(200)
.WithDescription("Gets an order by id")
.WithName("GetOrderById");
```

### Infrastructure adapters

- **HTTP:** extend `BaseHttpService` for outbound calls. It handles correlation tracking and resilience.
- **gRPC:** implement service adapters in `src/Infrastructure/Grpc/`, including channel creation and deadline handling.
- **Cache:** use `HybridCacheService` for distributed and local caching.
- **Resilience:** Polly retry and circuit-breaker policies through `Microsoft.Extensions.Http.Polly` and DI.
- **Telemetry:** OpenTelemetry traces, metrics, and logs, configured in `InfrastructureDependencyInjection.cs`.

### Integration tests

- Use `CustomWebApplicationFactory<Program>`. Annotate with `[Collection("WebApplicationFactoryCollectionDefinition")]`.
- Inherit `IClassFixture<BaseHttpFixture>` and use it to set `ResourceUrl`, headers, and `ApiHelper`.
- Name tests `GivenA<Context>Then<Outcome>`, for example `GivenAGetByIdValidRequestThenPass`.
- Cover at least the success case (status code and payload) and one not-found or invalid case.

```csharp
[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class OrderTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;

    public OrderTest(CustomWebApplicationFactory<Program> factory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(factory);
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact]
    public async Task GivenAGetByIdValidRequestThenPass()
    {
        // Arrange, Act, Assert
    }
}
```

### Load tests

k6 scripts (`scriptHttp.js`, `scriptGrpc.js`) live in `tests/LoadTests/`. See [Helper commands](#helper-commands) for how to run them.

### Code quality

- Cyclomatic complexity: at most 30 per method. Use guard clauses, extracted methods, or a strategy pattern.
- Zero compiler warnings (`TreatWarningsAsErrors`).
- New endpoints, services, and behaviors need tests, preferably integration tests for API behavior.
- No hardcoded secrets or SQL injection vectors. Track correlation IDs for auditing.

## Contribute

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
