# AGENTS.md

Guidance for AI coding agents working in this workspace.

## First read

- Project overview and local environment details are in `Readme.md`.

## Repository shape

- `src/WebApp`: Blazor Web App (UI/inbound adapter)
- `src/Contracts`: shared request/response contracts and DTOs (port models)
- `src/Infrastructure`: outbound adapters (HTTP, DI wiring, resiliency, telemetry)
- `src/MockApi`: local external API simulator
- `tests/UnitTests`: xUnit + bUnit tests

## Build, test, and run

Prefer VS Code tasks when available.

- Build all: `build [Solution]`
- Test all: `test [Solution]`
- Run Web app (watch): `watch [WebApp]`
- Run Mock API (watch): `watch [MockApi]`

If tasks are unavailable, use these examples from workspace root:

- Build solution: `dotnet build`
- Test solution: `dotnet test`
- Run app: `dotnet run --project src/WebApp/WebApp.csproj`
- Run mock API: `dotnet run --project src/MockApi/MockApi.csproj`

## Coding conventions that matter

- Target framework: `net10.0` (see `Directory.Build.props`).
- `Nullable` is enabled; avoid introducing nullable warnings.
- Warnings are treated as errors (`TreatWarningsAsErrors=true`, `CodeAnalysisTreatWarningsAsErrors=true`). Keep builds warning-free.
- Package versions are centrally managed in `Directory.Packages.props`.
- Keep architecture boundaries:
  - UI composition in `WebApp`
  - DTO/contracts in `Contracts`
  - external communication and technical concerns in `Infrastructure`

## Component patterns to follow (WebApp)

- Keep page markup in `.razor` and behavior/state in a `.razor.cs` partial class.
- Prefer constructor injection with `IServiceProvider` in component code-behind, then resolve keyed services explicitly.
- Use `OnInitializedAsync` for startup loading and run independent requests concurrently with `Task.WhenAll`.
- For failures, log with existing `Logs.FailedOperation(...)` helper and keep UI resilient (show loading/fallback state instead of throwing).
- Keep test-friendly selectors on interactive/validated UI elements using `data-testid`.
- For conditional UI, use null/state checks in markup (for example loading blocks before data renders).

Example page split pattern:

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

```csharp
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

## Testing conventions

- Frameworks: xUnit + bUnit + Moq.
- Prefer `data-testid` selectors for component assertions.
- Follow fixture-based component tests (shared mocks, explicit setup per scenario).
- Example assertion style:
  - Assert text in key metrics: `component.Find("[data-testid='total-orders']")`
  - Assert table row counts: `component.FindAll("[data-testid='orders-table'] tbody tr")`
  - Verify mocked HTTP calls exactly once with `Times.Once`.

Example test pattern:

```csharp
[Fact]
public void GivenHomeComponentWhenRenderedThenShouldDisplayOrderSummary()
{
    _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetValidOrdersResponse());
    _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetValidOrderSummaryResponse());

    var component = _fixture.Render<Home>();

    component.Find("[data-testid='total-orders']").TextContent.Contains("17");
    Assert.Equal(2, component.FindAll("[data-testid='orders-table'] tbody tr").Count);
    _fixture.HttpServiceMock.Verify(
        x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null),
        Times.Once);
}
```

## HTTP and observability patterns

- Reuse existing `IBaseHttpService` abstraction for outbound HTTP.
- Maintain resilience with existing Polly retry policies in `InfrastructureDependencyInjection`.
- Preserve OpenTelemetry wiring in infrastructure (`traces`, `metrics`, `logs`) unless task explicitly changes telemetry.
- Example DI pattern:
  - Register keyed HTTP service per integration (e.g., Orders).
  - Inject and call `SendAsync<TResponse>(route, HttpMethod.Get, cancellationToken)`.
  - On failed HTTP status, log and return `null`/fallback response rather than throwing by default.

Example service call pattern:

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

## Change hygiene

- Keep changes minimal and scoped to the request.
- Do not refactor unrelated files.
- Run relevant build/tests after changes.
