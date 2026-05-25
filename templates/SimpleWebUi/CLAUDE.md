# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build src/WebUi/WebUi.csproj

# Run
dotnet watch run --project src/WebUi/WebUi.csproj

# Unit tests (all)
dotnet test tests/UnitTests/UnitTests.csproj

# Unit tests (single test class)
dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~CreateOrderUseCaseTest"

# Integration tests (requires PostgreSQL on 127.0.0.1:5432 and Redis on localhost:6379)
dotnet test tests/IntegrationTests/IntegrationTests.csproj

# E2e tests (requires the web app running on http://localhost:5015)
dotnet test tests/E2eTests/E2eTests.csproj

# E2e tests with custom app URL
WEB_APP_URL=http://localhost:5015 dotnet test tests/E2eTests/E2eTests.csproj

# Mutation tests
dotnet stryker --config-file tests/UnitTests/stryker-config-core.json

# EF Core migrations (run from repo root)
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebUi
dotnet ef database update --project src/Infrastructure --startup-project src/WebUi
```

## Architecture

This is a **hexagonal (ports & adapters)** template with three layers enforced by architecture tests:

- **`src/Core`** — Domain and use cases. No references to Infrastructure or WebUi allowed. Contains domain entities, use case base classes, repository/service interfaces, request/response records, and messages.
- **`src/Infrastructure`** — Adapters: EF Core (PostgreSQL via `IDbContextFactory<MyDbContext>`), Redis HybridCache, in-process messaging via `System.Threading.Channels`, and OpenTelemetry + Pyroscope.
- **`src/WebUi`** — Blazor Server pages, components, health checks, exception middleware. References Infrastructure only.

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
- Core must not reference `Infrastructure` or `WebUi` assemblies
- All types in Core must be in the `Core.*` namespace

## Infrastructure dependencies

| Service | Config key | Default |
|---|---|---|
| PostgreSQL | `ConnectionStrings:OrderDb` | `Host=127.0.0.1;Port=5432;Database=OrderDb;Username=postgres;Password=cY5VvZkkh4AzES` |
| Redis | `ConnectionStrings:Redis` | `localhost:6379` |

Set `ENABLE_SENSITIVE_DATA_LOGGING=true` to enable EF Core query logging.

OpenTelemetry exports via OTLP; set `ASPNETCORE_ENVIRONMENT=IntegrationTests` to disable it during test runs.

## Blazor page patterns

### Code-behind structure

Pages use partial classes in `.razor.cs` files. Implement `IDisposable` with a `CancellationTokenSource` for every page that calls async use cases:

```csharp
public partial class MyPage : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await MyUseCase.HandleAsync(new(...), _cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

Never use `CancellationToken.None`. Always pass `_cancellationTokenSource.Token` to `HandleAsync`.

## Testing patterns

### Shared CSS selectors

`tests/CommonTests/WebUi/Selectors.cs` defines all CSS selector constants used by both unit tests and E2e tests. Both test projects reference `CommonTests` and import it via `global using CommonTests.WebUi` in their `GlobalUsings.cs`. Add new selectors there — never hardcode selector strings in test files.

### Unit tests (use case)

`BaseCoreFixture<TRequest, TUseCase>` pre-wires mocks for `IServiceProvider`, `IBaseRepository`, `IHybridCacheService`, and `IProduceService`. Each test class creates its own fixture class inheriting from it. Call `_fixture.ClearInvocations()` in the constructor, not in `SetUp`.

### Unit tests (component)

`BaseComponentFixture` (in `tests/UnitTests/WebUi/`) extends bUnit's `BunitContext`. Use `Render<T>()`, `Find()`, `FindAll()`, and `FindComponent<T>()`. Reference selectors via `Selectors.*` constants.

```csharp
public class MyComponentFixture : BaseComponentFixture
{
    public IRenderedComponent<MyComponent> RenderComponent() =>
        Render<MyComponent>(p => p.Add(c => c.SomeProp, value));
}

public sealed class MyComponentTests(MyComponentFixture fixture) : IClassFixture<MyComponentFixture>
{
    [Fact(DisplayName = nameof(GivenWhenThen))]
    public void GivenWhenThen()
    {
        var cut = fixture.RenderComponent();
        var button = cut.Find(Selectors.ButtonSubmit);
        Assert.NotNull(button);
    }
}
```

### Integration tests

Use `CustomWebApplicationFactory<Program>` which swaps the pooled DbContextFactory for a regular one targeting the local PostgreSQL instance. Tests are grouped under `[Collection(nameof(WebApplicationFactoryCollectionDefinition))]`.

### E2e tests

E2e tests use Playwright via `Microsoft.Playwright.Xunit`. Each page has a fixture class extending `BrowserFixture` and a test class using `IClassFixture<TFixture>`.

**`BrowserFixture`** (in `tests/E2eTests/Common/`) extends `PageTest` and exposes:
- `Page` — the Playwright `IPage`
- `WebAppUrl` — defaults to `http://localhost:5015`, overridable via `WEB_APP_URL` env var
- `NavigationTimeoutMs` — defaults to 30000, overridable via `NAVIGATION_TIMEOUT_MS` env var
- `WaitForSelectorTimeoutMs` — defaults to 10000, overridable via `WAIT_FOR_SELECTOR_TIMEOUT_MS` env var

**Pattern:**

```csharp
public class MyPageFixture : BrowserFixture
{
    public async Task NavigateAsync() => await Page.NavigateAsync($"{WebAppUrl}/my-page");
}

public sealed class MyPageTests(MyPageFixture fixture) : IClassFixture<MyPageFixture>
{
    [Fact(DisplayName = nameof(GivenWhenThen))]
    public async Task GivenWhenThen()
    {
        await fixture.NavigateAsync();
        await fixture.Page.WaitForPageLoadAsync();

        var heading = await fixture.Page.GetHeadingTextAsync();

        Assert.Equal("Expected Heading", heading);
    }
}
```

**`PlaywrightExtensions`** methods available on `IPage`:
- `NavigateAsync(url)` — GotoAsync with `WaitUntil = NetworkIdle`
- `WaitForPageLoadAsync()` — waits for `h1` to appear
- `WaitForFormAsync()` — waits for the submit button to appear
- `GetHeadingTextAsync()` — returns trimmed text of the first `h1`
- `ClickSubmitAsync()` — clicks `button[type='submit']`
- `GetTableRowsAsync()` — returns all `tbody tr` elements
- `ClickAndWaitForLoadAsync(selector)` — click + wait for `NetworkIdle`

**Blazor Server timing:**

Blazor Server uses SignalR (WebSocket), so `WaitForLoadStateAsync(NetworkIdle)` does not detect Blazor circuit processing. Follow these rules:

1. After filling a bound input, trigger the binding with `Tab` and wait for Blazor to process:
   ```csharp
   var locator = Page.Locator(Selectors.InputOrderDescription);
   await locator.FillAsync(value);
   await locator.PressAsync("Tab");
   await Page.WaitForTimeoutAsync(300);
   ```

2. After clicking a button that causes Blazor to re-render the DOM (e.g., add/remove item rows), wait for the DOM change with `WaitForFunctionAsync`:
   ```csharp
   await Page.ClickAsync(Selectors.ButtonOutlineSuccess);
   await Page.WaitForFunctionAsync($"() => document.querySelectorAll('{Selectors.ItemRow}').length === {expectedCount}");
   ```

3. After submitting a form that navigates away, use `WaitForURLAsync` (not `WaitForLoadStateAsync`):
   ```csharp
   await Page.ClickSubmitAsync();
   await Page.WaitForURLAsync("**/orders", new() { Timeout = 10000 });
   ```

4. To read the current value of a bound input, use `InputValueAsync` (reads the DOM property, not the HTML attribute):
   ```csharp
   var value = await Page.InputValueAsync(Selectors.InputOrderDescription);
   ```

### Test naming

All test methods follow the `GivenWhenThen` naming convention with `[Fact(DisplayName = nameof(GivenWhenThen))]`. No underscores in method names (CA1707).

Architecture tests in `tests/UnitTests/Architecture/` use `ArchitectureTestHelper` and run as part of unit tests — they validate layer isolation and naming conventions.
