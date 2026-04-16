---
name: create-endpoint-with-integration-tests
description: "Create a new minimal API endpoint in src/WebApp and add matching integration tests in tests/IntegrationTests. Use when user asks for new GET/POST/PUT/PATCH/DELETE endpoints plus end-to-end validation."
---

# Create Endpoint with Integration Tests

> **Delegation**: Execute this workflow via `@runSubagent Woven Engineer Agent`.

Create or extend a minimal API endpoint and add its HTTP integration tests using the existing fixtures/helpers.

## When to use

Activate this skill when the user asks to:
- add a new endpoint to `src/WebApp/Endpoints`
- add a new route to existing endpoint groups (`orders`, `payments`, etc.)
- include integration tests validating status codes and response payloads

## Target files

- `src/WebApp/Endpoints/<Feature>Endpoints.cs` — static internal class with `MapXEndpoints(this WebApplication app)` extension
- `src/WebApp/Endpoints/EndpointExtensions.cs` — root `MapEndpoints()` that chains all feature mappers (currently: `MapOrderEndpoints()`, `MapPaymentEndpoints()`)
- `tests/IntegrationTests/WebApp/Http/<Feature>Test.cs` — sealed class decorated with `[Collection("WebApplicationFactoryCollectionDefinition")]` and `IClassFixture<BaseHttpFixture>`
- optionally `src/Contracts/<Feature>/*.cs` when new request/response models are required

## Endpoint conventions

- Group routes via `app.MapGroup(serviceKey).WithTags(serviceKey).RequireRateLimiting(serviceKey)` where `serviceKey = ServicesKey.<Feature>.ToString()`.
- Inject dependencies with `[FromKeyedServices(ServicesKey.<Feature>)] BaseHttpService httpService` and `[FromServices] HybridCacheService cache`.
- Correlate requests via `[FromHeader] Guid? correlationId = null` — default to `Guid.NewGuid()` if null.
- Wrap responses in `BaseResponse<T>` (from `Contracts.Common`); return `Results.Ok(response)` / `Results.NotFound` / `Results.BadRequest`.
- Keep OpenAPI metadata: `.Produces<BaseResponse<T>>(200)`, `.WithDescription(...)`, `.WithName(...)`.
- Wrap logic in `DefaultConfigurations.ActivitySource.StartActivity(...)` for distributed tracing.
- Use `HybridCacheService.GetOrCreateAsync` when `cacheEnabled == true` (see `OrderEndpoints.GetById` pattern).

## Integration test conventions

- Annotate test class with `[Collection("WebApplicationFactoryCollectionDefinition")]` to share `CustomWebApplicationFactory<Program>`.
- Inject both `CustomWebApplicationFactory<Program>` and `BaseHttpFixture` in constructor; call `_fixture.SetApiHelper(factory)`.
- Set `_fixture.ResourceUrl` using a format string, e.g. `"orders/{0}"`.
- Use `_fixture.ApiHelper.AddHeaders(new Dictionary<string, string> { { "CorrelationId", ... }, { "CacheEnabled", "false" } })`.
- Call `_fixture.ApiHelper.GetAsync`, `PostAsync`, etc.; deserialize with `ApiHelper.DeSerializeResponse<T>(result)`.
- Follow naming: `GivenA<Context>Then<Outcome>` (e.g., `GivenAGetByIdValidRequestThenPass`).
- Validate at minimum: success status + payload, not-found/invalid scenario.

## Workflow

1. Define route, method, and contract types.
2. Implement endpoint in `src/WebApp/Endpoints`.
3. Register endpoint in `EndpointExtensions.cs` (if new group).
4. Create/update integration test class under `tests/IntegrationTests/WebApp/Http`.
5. Run integration tests and fix failures.

## Practical examples

### Example 1 — Add PATCH route in existing group

**User asks:**
- "Add PATCH /orders/{id}/status and tests"

**Recommended delegation:**
- `@runSubagent Woven Engineer Agent Add PATCH /orders/{id}/status in OrderEndpoints and create integration tests for success + invalid id`

**Expected file changes:**
- `src/WebApp/Endpoints/OrderEndpoints.cs` (updated)
- `tests/IntegrationTests/WebApp/Http/OrderTest.cs` (updated)
- `src/Contracts/Orders/*.cs` (optional, if new request/response types are needed)

**Real project reference:**
```csharp
// Pattern from src/WebApp/Endpoints/OrderEndpoints.cs
ordersGroup.MapPatch("/{id}/status", async (
    [FromRoute] int id,
    [FromBody] UpdateOrderStatusRequest request,
    [FromKeyedServices(ServicesKey.Orders)] BaseHttpService httpService,
    CancellationToken cancellationToken,
    [FromHeader] Guid? correlationId = null
) => {
    using var activity = DefaultConfigurations.ActivitySource.StartActivity($"{nameof(OrderEndpoints)}.UpdateStatus");
    var response = await httpService.SendAsync<UpdateOrderStatusRequest, BaseResponse<OrderDto>>(
        $"orders/{id}/status", HttpMethod.Patch, request, cancellationToken: cancellationToken);
    return response != null ? Results.Ok(response) : Results.NotFound();
})
.Produces<BaseResponse<OrderDto>>(StatusCodes.Status200OK)
.Produces<BaseResponse>(StatusCodes.Status404NotFound)
.WithDescription("Updates the status of an order")
.WithName("UpdateOrderStatus");
```

**Acceptance checks:**
- Endpoint has `Produces`, `WithDescription`, `WithName`
- Tests assert status code and payload for happy path
- Tests assert error behavior for invalid id/request

### Example 2 — Add endpoint group + tests

**User asks:**
- "Create GET /shipments/{id} endpoint with integration tests"

**Recommended delegation:**
- `@runSubagent Woven Engineer Agent Create ShipmentEndpoints with GET by id, register in EndpointExtensions, and add IntegrationTests/WebApp/Http/ShipmentTest.cs`

**Expected file changes:**
- `src/WebApp/Endpoints/ShipmentEndpoints.cs` (new)
- `src/WebApp/Endpoints/EndpointExtensions.cs` (updated — add `.MapShipmentEndpoints()` to the chain)
- `tests/IntegrationTests/WebApp/Http/ShipmentTest.cs` (new)
- `src/Contracts/Shipments/*.cs` (optional)

**Real project reference:**
```csharp
// EndpointExtensions.cs pattern:
public static WebApplication MapEndpoints(this WebApplication app)
{
    app
        .MapOrderEndpoints()
        .MapPaymentEndpoints()
        .MapShipmentEndpoints(); // add here
    return app;
}
```

**Integration test scaffold:**
```csharp
[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class ShipmentTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;
    public ShipmentTest(CustomWebApplicationFactory<Program> factory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(factory);
        _fixture.ResourceUrl = "shipments/{0}";
    }

    [Fact(DisplayName = nameof(GivenAGetByIdValidRequestThenPass))]
    public async Task GivenAGetByIdValidRequestThenPass()
    {
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, 1);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() },
            { "CacheEnabled", "false" }
        });
        var result = await _fixture.ApiHelper.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
```

**Acceptance checks:**
- New endpoint group is registered and reachable
- Integration tests use `CustomWebApplicationFactory<Program>` and `BaseHttpFixture`
- Correlation/cache headers are handled consistently

### Example 3 — Create POST route with validation tests

**User asks:**
- "Create POST /payments/refund and test invalid payload"

**Expected file changes:**
- `src/WebApp/Endpoints/PaymentEndpoints.cs` (updated)
- `tests/IntegrationTests/WebApp/Http/PaymentTest.cs` (new or updated)
- `src/Contracts/Payments/*.cs` (optional)

**Acceptance checks:**
- Success path returns expected status/body
- Validation failure returns 400 (or project-standard error)
- Tests remain deterministic and fixture-based

## Done checklist

- [ ] Endpoint mapped and discoverable in OpenAPI
- [ ] Success + failure test cases added
- [ ] Tests use shared fixture/helpers
- [ ] Integration tests pass
