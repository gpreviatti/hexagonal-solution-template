---
name: "Hexagonal BFF Template Agent Instructions"
description: "Agent instructions for the Hexagonal Architecture Backend for Frontend template."
---

# Hexagonal BFF Template — Agent Instructions

This file helps AI agents understand and work productively with the **Hexagonal Architecture Backend for Frontend (BFF) template**.

## Architecture Overview

See [Readme.md](../../Readme.md#architecture-at-a-glance) for detailed architecture. In brief:

- **Contracts** (`src/Contracts/`): Immutable request/response records and gRPC `.proto` definitions. Business domain contracts only—no infrastructure concerns.
- **Infrastructure** (`src/Infrastructure/`): Outbound adapters (HTTP/gRPC services, cache, retry policies), cross-cutting concerns (logging, telemetry, resilience), and DI composition.
- **WebApp** (`src/WebApp/`): Inbound adapters—minimal APIs with endpoint composition, middleware, and request orchestration.
- **MockApi** (`src/MockApi/`): Fake downstream HTTP and gRPC services for local development and integration testing.
- **Tests**: Unit tests (`tests/CommonTests`), integration tests (`tests/IntegrationTests`), and load tests (`tests/LoadTests`).

## Key Project Conventions

### C# & .NET

- **Target Framework**: `.NET 10.0`
- **Language**: C# 13 with latest idioms: file-scoped namespaces, records, init accessors, required members.
- **Nullable**: `<Nullable>enable</Nullable>` — null-safe reference types required; suppress warnings only when intentional.
- **Warnings as errors**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — zero-warning policy enforced.
- **Package Management**: Central Package Management (CPM) via `Directory.Packages.props`; never hardcode NuGet versions in project files.

### Contracts

- All HTTP request records inherit from `BaseRequest(Guid CorrelationId)` — correlation tracking is mandatory.
- Paginated requests inherit from `BasePaginatedRequest`.
- Use `sealed record` for immutable DTO types.
- Use `decimal` for monetary fields (not `float` or `double`).
- gRPC contracts in `src/Contracts/Protos/*.proto`; keep field numbers stable for backward compatibility.
- **XML documentation** required on public contract types and notable fields.

Example (from `src/Contracts/Orders/CreateOrderRequest.cs`):
```csharp
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId);
```

### Endpoints (Minimal APIs)

- **Location**: `src/WebApp/Endpoints/<Feature>Endpoints.cs` — static internal class with `MapXEndpoints(this WebApplication app)` extension method.
- **Grouping**: Use `app.MapGroup(serviceKey).WithTags(serviceKey).RequireRateLimiting(serviceKey)` where `serviceKey = ServicesKey.<Feature>.ToString()`.
- **Dependencies**: Inject via `[FromKeyedServices(ServicesKey.<Feature>)]` or `[FromServices]`.
- **Correlation**: Accept `[FromHeader] Guid? correlationId = null` — default to `Guid.NewGuid()` if null.
- **Responses**: Wrap in `BaseResponse<T>` (from `Contracts.Common`); return `Results.Ok(response)`, `Results.NotFound`, or `Results.BadRequest`.
- **OpenAPI**: Include `.Produces<T>(statusCode)`, `.WithDescription(...)`, `.WithName(...)`.
- **Tracing**: Wrap logic in `DefaultConfigurations.ActivitySource.StartActivity(...)` for distributed tracing.
- **Caching**: Use `HybridCacheService.GetOrCreateAsync(...)` when appropriate (see `OrderEndpoints.GetById` pattern).

Example (from `src/WebApp/Endpoints/OrderEndpoints.cs`):
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

### Infrastructure Adapters

- **HTTP**: Extend `BaseHttpService` for outbound HTTP calls with correlation tracking and resilience.
- **gRPC**: Implement service adapters in `src/Infrastructure/Grpc/` with channel creation and deadline handling.
- **Cache**: Use `HybridCacheService` for distributed + local cache orchestration.
- **Resilience**: Leverage `Microsoft.Extensions.Http.Polly` for retry + circuit-breaker policies via DI.
- **Telemetry**: OpenTelemetry for traces, metrics, logs. Configure in `InfrastructureDependencyInjection.cs`.

### Tests

- **Unit tests**: `tests/CommonTests/` — test utilities and lightweight domain logic tests.
- **Integration tests**: `tests/IntegrationTests/` — end-to-end API tests via `CustomWebApplicationFactory<Program>`.
  - Annotate with `[Collection("WebApplicationFactoryCollectionDefinition")]` for shared factory.
  - Use `BaseHttpFixture` to set up `ResourceUrl`, headers, and `ApiHelper`.
  - Follow naming: `GivenA<Context>Then<Outcome>` (e.g., `GivenAGetByIdValidRequestThenPass`).
  - Validate at minimum: success status + payload, not-found/invalid scenario.
- **Load tests**: `tests/LoadTests/` — k6 scripts (`scriptHttp.js`, `scriptGrpc.js`) for performance validation.

Example (from `tests/IntegrationTests/WebApp/Http/OrderTest.cs`):
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

## Build & Test Commands

**Build**: `dotnet build`
**Tests (all)**: `dotnet test`
**Tests (unit only)**: `dotnet test tests/CommonTests`
**Tests (integration)**: `dotnet test tests/IntegrationTests`
**Run WebApp**: `dotnet run -c Debug --project src/WebApp`
**Run MockApi**: `dotnet run -c Debug --project src/MockApi`
**Load tests (HTTP)**: `k6 run tests/LoadTests/scriptHttp.js --summary-mode=full`

## Docker & Local Development

- **Compose (with observability)**: `docker-compose -f docker-compose-local.yml up -d`
  - Starts: mock-api (5012), WebApp (5011), Redis (6379), Grafana (3000), Prometheus, Loki, Tempo, Pyroscope.
  - MockApi and WebApp are set to `ASPNETCORE_ENVIRONMENT=Development`.
  - Observability stack: Grafana dashboard at `http://localhost:3000`.

- **Compose (production-like)**: `docker-compose up -d`
  - Lightweight setup: mock-api, WebApp, Redis only (no observability).

- **Load tests compose**: `docker-compose -f docker-compose-load-tests.yml up`
  - Runs k6 load tests against WebApp with full observability stack.

## Skills Included in This Template

Use `@runSubagent Woven Engineer Agent` to delegate work to specialized skills. Each skill is code-aware and follows template conventions.

### create-contracts
**Use when**: Adding/changing request/response DTOs, gRPC proto definitions, or base contract wrappers.

**Location**: `.github/skills/create-contracts/SKILL.md`

**Example delegation**:
- "Add CancelOrderRequest and CancelOrderResponse contracts with correlation tracking"
- "Update OrderDto to include status and cancelledAt timestamp"
- "Create PaymentRequest and PaymentReply proto for gRPC service"

**Output**: New or updated contract files in `src/Contracts/` with proper inheritance, XML docs, and field naming consistency.

---

### create-endpoint-with-integration-tests
**Use when**: Building new endpoints (GET/POST/PUT/PATCH/DELETE) plus integration tests.

**Location**: `.github/skills/create-endpoint-with-integration-tests/SKILL.md`

**Example delegation**:
- "Add PATCH /orders/{id}/status endpoint with UpdateOrderStatusRequest and integration tests for success + invalid id"
- "Create GET /shipments/{id} endpoint with HybridCacheService integration and tests"
- "Extend OrderEndpoints with a new POST /orders/bulk route"

**Output**: New or updated endpoint in `src/WebApp/Endpoints/`, registration in `EndpointExtensions.cs`, and integration tests in `tests/IntegrationTests/WebApp/Http/`.

**Acceptance criteria** (enforced by skill):
- Endpoint has `Produces`, `WithDescription`, `WithName`.
- Tests assert status code and payload for happy path.
- Tests assert error behavior for edge cases.

---

### create-load-tests
**Use when**: Adding or extending k6 performance test scripts with scenarios and thresholds.

**Location**: `.github/skills/create-load-tests/SKILL.md`

**Example delegation**:
- "Add HTTP load scenarios for /orders/{id}/status endpoint with 1000 VUs, 180s duration, and P95 latency threshold of 500ms"
- "Create gRPC load test for PaymentService.Create with burst traffic pattern"
- "Extend scriptHttp.js with new scenario for POST /orders/bulk"

**Output**: New or updated k6 script (`tests/LoadTests/scriptHttp.js` or `scriptGrpc.js`) with thresholds, checks, and metrics.

---

### code-review
**Use when**: Reviewing pull requests for architecture compliance, test coverage, complexity, security, and modern C#.

**Location**: `.github/skills/code-review/SKILL.md`

**Example delegation**:
- "Review this endpoint and integration test changes against BFF standards"
- "Check this infrastructure adapter for proper error handling and resilience"
- "Audit this PR for complexity > 30, untested code, and C# 13 idiom violations"

**Mandatory gates**:
- All new behavior has tests (integration tests preferred for API behavior).
- Cyclomatic complexity ≤ 30 per method (hard fail if exceeded).
- Hexagonal architecture respected: contracts clean, adapters isolated, concerns separated.
- Modern C# used: file-scoped namespaces, records, required members, init accessors.
- No security vulnerabilities or secret exposure.

---

## Code Quality Standards

- **Complexity**: Cyclomatic complexity must stay ≤ 30 per method. Use guard clauses, extract methods, or strategy pattern to reduce nesting.
- **Warnings**: Zero compiler warnings. `TreatWarningsAsErrors=true` enforces this.
- **Tests**: New endpoints, services, and behaviors must have tests. Prefer integration tests for API behavior.
- **Security**: No hardcoded secrets, no SQL injection vectors, proper correlation tracking for auditing.
- **Observability**: Every endpoint wrapped in `ActivitySource.StartActivity(...)`. Use correlation IDs for tracing.

## When to Use Each Skill

| Task | Skill | Delegation Example |
|------|-------|-------------------|
| Add new endpoint with tests | `create-endpoint-with-integration-tests` | `@runSubagent Woven Engineer Agent Add GET /orders/search endpoint...` |
| Create/update contracts | `create-contracts` | `@runSubagent Woven Engineer Agent Create SearchOrderRequest and SearchOrderResponse...` |
| Add load test scenarios | `create-load-tests` | `@runSubagent Woven Engineer Agent Add gRPC load scenario for PaymentService...` |
| Review PR for compliance | `code-review` | `@runSubagent Woven Engineer Agent Review this PR against BFF standards...` |

## Quick Links

- [Template README](../../Readme.md) — Architecture, project structure, helper commands.
- [.NET Instructions](./instructions/dotnet.instructions.md) — C# and .NET architecture guidelines.
- [docker-compose.yml](../../docker-compose-local.yml) — Local development environment.
- [Directory.Build.props](../../Directory.Build.props) — Global .NET project settings.

## Tips for Agents

1. **Always build after changes**: `dotnet build` ensures zero-warning compliance.
2. **Run tests locally**: `dotnet test` before delegating to code-review skill.
3. **Use correlation IDs**: Every request flows with a `Guid CorrelationId` for tracing.
4. **Link, don't embed**: Reference [Readme.md](../../Readme.md) and `SKILL.md` files; avoid duplicating documentation.
5. **Respect hexagonal boundaries**: Contracts stay in `src/Contracts`, adapters in `src/Infrastructure`, inbound composition in `src/WebApp`.

