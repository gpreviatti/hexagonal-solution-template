# Hexagonal Architecture Solution Template (Web UI)

This repository provides a ready-to-use **Blazor Web UI** template built with **Hexagonal Architecture** (Ports and Adapters) on **.NET 10**.

The template keeps UI composition in the web app, contracts in a dedicated project, and technical adapters in infrastructure while adding component-level test coverage with **bUnit**.
It also includes a **Mock API** project consumed via HTTP adapter services, comprehensive **OpenTelemetry** instrumentation (logs, traces, metrics, profiling), and a full observability stack with **Grafana**, **Loki**, **Prometheus**, **Tempo**, and **Pyroscope**.

## Architecture at a glance

- **Inbound UI adapter:** `src/WebApp` Blazor Web App with interactive server-side components and routing.
- **Contracts (port models):** `src/Contracts` request/response payload models and DTOs.
- **Outbound adapters:** `src/Infrastructure` HTTP service adapters, dependency injection, Polly resilience policies, and OpenTelemetry instrumentation (metrics, traces, structured logs).
- **Mock external service:** `src/MockApi` local HTTP API used by `WebApp` through `Infrastructure` adapters.
- **Quality gates:** `tests/UnitTests` for component and service unit tests using xUnit + bUnit.
- **Observability:** Docker Compose stack with Grafana, Loki (logs), Prometheus (metrics), Tempo (traces), Alloy (collector), and Pyroscope (continuous profiling).

## Project structure (tree view)

```text
.
├── src
│   ├── Contracts
│   │   └── Orders
│   ├── Infrastructure
│   │   ├── Common
│   │   ├── Http
│   ├── MockApi
│   │   └── Endpoints
│   └── WebApp
│       ├── Components
│       │   ├── Layout
│       │   └── Pages
│       └── wwwroot
└── tests
    └── UnitTests
        └── WebApp
```
Quick start

### Prerequisites

- **.NET 10 SDK** or later
- **Docker** and **Docker Compose** (for local observability stack)

### Build solution

```bash
dotnet build Hexagonal.Solution.Template.WebUi.slnx
```

### Run locally (development)

Start the observability stack in a separate terminal:

```bash
docker-compose -f docker-compose-local.yml up --build
```

Then run the **Mock API** in one terminal:

```bash
dotnet run --project src/MockApi/MockApi.csproj
```

And the **Web App** in another terminal:

```bash
dotnet run --project src/WebApp/WebApp.csproj
```

TheKey features & practices

### Code quality
- **.NET 10** with nullable reference types enabled and implicit usings
- **Strict analysis** enabled: `TreatWarningsAsErrors=true` and `CodeAnalysisTreatWarningsAsErrors=true`
- **Latest analysis level** for static code analysis recommendations
- **Package lock files** for reproducible builds

### Resilience & HTTP communication
- **Polly** for resilience policies (retries, circuit breaker, timeout patterns)
- **Generic HTTP service abstraction** (`IBaseHttpService`) with structured error handling
- **HTTP/2 support** with version negotiation and proper protocol handling
- **Typed service configuration** via dependency injection

### Observability & monitoring
- **OpenTelemetry** instrumentation for traces, metrics, and logs
- **Structured logging** with correlation IDs and context
- **Continuous profiling** with Pyroscope (CPU, memory, allocations, exceptions)
- **Metrics collection** from ASP.NET Core, HttpClient, runtime, and process
- **Distributed tracing** with correlation across services
- **Grafana Alloy** as unified data collector
- **Loki** for scalable log aggregation
- **Prometheus** for metrics scraping and querying
- **Tempo** for trace storage and querying

### UI architecture
- **Blazor Web App** with interactive server-side rendering
- **Component-based** architecture with layout abstraction
- **Interactive server render mode** for real-time interactivity
- **Status code pages** with graceful error handling

### Testing
- **bUnit** for component-level integration tests
- **xUnit** as test framework
- **Unit tests** for service adapters and business logic

## Development workflow

1. **Local development:** Run Mock API and Web App separately with hot-reload capability
2. **Observability:** Use Docker Compose to spin up the full observability stack
3. **Testing:** Run unit tests frequently and use bUnit for component testing
4. **Containerization:** Use provided Dockerfiles for production deployment

## Future considerations

- Browser E2E tests (Playwright or Cypress)
- API versioning strategies
- Advanced caching patterns
- Additional resilience patterns (bulkhead, rate limiting)
- Custom health checks and diagnostics endpoints
docker-compose up --build
```

### Run unit tests

```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

### View observability data

Once the observability stack is running:

- **Grafana:** http://localhost:3000 (dashboards for metrics, logs, traces)
- **Prometheus:** http://localhost:9090 (metrics explorer)
- **Tempo:** http://localhost:3200 (trace visualization)
- **Pyroscope:** http://localhost:4040 (continuous profiling)ker-compose up --build
```

## Development Guide

The short rules for AI agents are in [AGENTS.md](AGENTS.md).

### Blazor component pattern

Keep markup in `.razor` and state and behavior in the `.razor.cs` partial class. Inject `IServiceProvider` through the primary constructor and resolve keyed services explicitly. Load startup data in `OnInitializedAsync` and run independent requests concurrently with `Task.WhenAll`.

```csharp
// Home.razor.cs
public partial class Home(IServiceProvider serviceProvider)
{
  private readonly IBaseHttpService _ordersHttpService = serviceProvider
    .GetRequiredKeyedService<IBaseHttpService>(ServicesKey.Orders.ToString());
  private readonly ILogger<Home> _logger = serviceProvider.GetRequiredService<ILogger<Home>>();
  private IEnumerable<OrderDto>? _orders;

  protected override async Task OnInitializedAsync()
  {
    await Task.WhenAll(GetOrders(), GetOrderSummary());
  }

  private async Task GetOrders()
  {
    var response = await _ordersHttpService.SendAsync<BaseResponse<IEnumerable<OrderDto>>>(
      nameof(ServicesKey.Orders), HttpMethod.Get, CancellationToken.None);

    if (response is null || !response.Success)
    {
      Logs.FailedOperation(_logger, "Failed to retrieve orders.");
      return;
    }

    _orders = response.Data;
  }
}
```

```razor
@* Home.razor *@
@if (_orders is null)
{
  <p data-testid="loading-orders">Loading orders...</p>
}
else
{
  <table data-testid="orders-table">
    ...
  </table>
}
```

On failure, log with `Logs.FailedOperation(...)` and show a loading or fallback state instead of throwing. Add `data-testid` to interactive and validated elements.

### HTTP and observability

- Use `IBaseHttpService` for outbound HTTP. Register one keyed service per integration (for example, Orders) and call it with `SendAsync<TResponse>(route, HttpMethod.Get, cancellationToken)`.
- Polly retry policies are registered in `InfrastructureDependencyInjection`.
- OpenTelemetry (traces, metrics, logs) is wired in Infrastructure. Keep it unless the change is about telemetry.
- On a failed HTTP status, log and return `null` or a fallback instead of throwing.

```csharp
var response = await _ordersHttpService.SendAsync<GetOrderSummaryResponse>(
  requestUri: "Orders/summary",
  httpMethod: HttpMethod.Get,
  cancellationToken: cancellationToken);

if (response is null || !response.Success)
{
  _logger.LogWarning("Failed to retrieve order summary.");
  return;
}

Summary = response.Data;
```

### Unit tests (bUnit)

Stack: xUnit, bUnit, and Moq. Use fixture-based component tests with shared mocks and explicit setup per scenario. Verify HTTP mocks with `Times.Once`.

```csharp
[Fact]
public void GivenHomeComponentWhenRenderedThenShouldDisplayOrderSummary()
{
    _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetValidOrdersResponse());
    _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetValidOrderSummaryResponse());

    var component = _fixture.Render<Home>();

    Assert.Contains("17", component.Find("[data-testid='total-orders']").TextContent);
    Assert.Equal(2, component.FindAll("[data-testid='orders-table'] tbody tr").Count);
    _fixture.HttpServiceMock.Verify(
        x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null),
        Times.Once);
}
```

### E2E tests (Playwright)

- Tests live in `tests/E2eTests` and use `Microsoft.Playwright.Xunit` with `BrowserFixture` (`tests/E2eTests/Common/BrowserFixture.cs`).
- `BrowserFixture` navigates to the app at startup with `WaitUntil = NetworkIdle`. Keep the startup path and initial page behavior compatible with this.
- Environment variables:
  - `WEB_APP_URL`: base URL, default `http://localhost:5013`.
  - `NAVIGATION_TIMEOUT_MS` and `WAIT_FOR_SELECTOR_TIMEOUT_MS`: timeouts.
- Start `WebApp` and `MockApi` before running E2E tests.
- Use selector-based waits (`WaitForSelectorAsync`), not sleeps.
- E2E data comes from MockApi and is non-deterministic. Assert presence, shape, and visibility, not exact totals.
- The `data-testid` contract used by `OrderPageTests` includes `loading-summary`, `total-orders`, `total-revenue`, `orders-table`, `order-row`, `view-items-*`, `order-items-table`, and `order-item-row`. If you change any of them, update the E2E tests in the same change.
- Not Found page contract: heading `h3` with text `Not Found`, and a message equivalent to `content you are looking for does not exist`.

## Notes

- This first version focuses on **Blazor + bUnit** only.
- Browser E2E tests can be added in a future iteration.
- `WebApp` calls `MockApi` at `/orders/summary` using HTTP adapter service(s) in `Infrastructure`.
