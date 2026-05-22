---
name: endpoints-generator
description: 'Generate ASP.NET Minimal API endpoint classes in the hexagonal architecture template project following project patterns for caching, correlation ID, HTTP status codes, and DI registration'
---

# Endpoints Generator — Hexagonal Architecture Template

Generate ASP.NET Minimal API endpoint classes, register them in `EndpointExtensions.cs`, and follow all project conventions for caching, correlation IDs, request/response types, and HTTP status codes.

## When to Use

Activate this skill when:
- User requests a new API endpoint or endpoint group for a domain
- User wants to expose a use case via HTTP
- User asks for a GET, POST, PUT, or DELETE endpoint
- User mentions "endpoint", "route", "API", "controller", or "map"
- User needs pagination, caching, or CRUD endpoints for a new domain

---

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/endpoint-template.md` | Reusable Minimal API endpoint template |
| `./references/status-code-rules.md` | HTTP status mapping rules used in this project |

> Prefer these local assets so endpoint generation remains stable even if source examples move.

---

## Critical Project Convention: DI Is Automatic

**Do NOT manually register use cases in DI.**

`ApplicationDependencyInjection.cs` uses assembly scanning to auto-register every class ending in `UseCase` that extends `BaseInOutUseCase<,>`, `BaseInUseCase<>`, or `BaseOutUseCase<>`:

```csharp
// ApplicationDependencyInjection.cs — already handles registration automatically
var useCaseTypes = applicationAssembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase", StringComparison.Ordinal))
    .ToList();
// ... registers IBaseInOutUseCase<TRequest, TResponse> → concrete UseCase
```

Creating a new use case class is sufficient — no `services.AddScoped(...)` call is needed.

---

## Request and Response Types

### Base Types (from `Application.Common.Requests`)

```csharp
// Scalar request — extend this for single-entity operations
public record BaseRequest(Guid CorrelationId, string User = "", string TimezoneId = "");

// Paginated request — use directly for paginated list endpoints (POST /paginated)
public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null,
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);

// Single-item response
public record BaseResponse<TData>(bool Success, TData? Data = null, string? Message = null) : BaseResponse;

// Paginated response
public record BasePaginatedResponse<TData> : BaseResponse<IEnumerable<TData>>
{
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
}
```

### Domain Request Pattern

```csharp
// src/Application/Products/GetProductRequest.cs  ← defined inside the use case file
public sealed record GetProductRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed record CreateProductRequest(
    Guid CorrelationId,
    string Name,
    decimal Price,
    string CreatedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, CreatedBy, TimezoneId);

public sealed record UpdateProductRequest(
    Guid CorrelationId,
    int ProductId,
    string Name,
    decimal Price,
    string ModifiedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, ModifiedBy, TimezoneId);

public sealed record DeleteProductRequest(
    Guid CorrelationId,
    int ProductId,
    string DeletedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, DeletedBy, TimezoneId);
```

---

## HTTP Verb and Status Code Rules

| Verb | Route | Success | Failure |
|------|-------|---------|---------|
| `GET /{id}` | Single entity by ID | `200 OK` | `404 NotFound` |
| `POST /paginated` | Paginated list | `200 OK` | `400 BadRequest` |
| `POST /` | Create entity | `201 Created` (with Location header) | `400 BadRequest` |
| `PUT /{id}` | Update entity | `200 OK` | `400 BadRequest` |
| `DELETE /{id}` | Delete entity | `200 OK` | `400 BadRequest` |

---

## Step-by-Step Guide

### Step 1 — Create the Endpoint File

**File path**: `src/WebApp/Endpoints/{Domain}Endpoints.cs`
**Example**: `src/WebApp/Endpoints/ProductEndpoints.cs`

Checklist:
- [ ] `internal static class` (never `public`)
- [ ] Extension method `MapXxxEndpoints(this WebApplication app)`
- [ ] Retrieve `IHybridCacheService` from `app.Services` at group level (not per-handler)
- [ ] Use `app.MapGroup("/{route}").WithTags("{Domain}")` to group all endpoints
- [ ] Use `[FromServices]` for use case injection in handler delegates
- [ ] Use `[FromHeader] Guid correlationId` for all endpoints that need tracing
- [ ] Use `[FromHeader] bool cacheEnabled = true` for GET endpoints with cache
- [ ] Add/update matching local request examples in `src/WebApp/WebApp.http`

### Step 2 — Register in EndpointExtensions.cs

Add one line to `src/WebApp/Endpoints/EndpointExtensions.cs`:

```csharp
app.MapProductEndpoints();
```

### Step 3 — Add Local HTTP Test Samples

Whenever a new endpoint is created, add (or update) an example request in:

`src/WebApp/WebApp.http`

Rules:
- Add one sample per new route/verb combination (GET/POST/PUT/DELETE).
- Include required headers (e.g., `CorrelationId`, `CacheEnabled`) exactly as the endpoint contract expects.
- Include realistic body payloads for POST/PUT endpoints so local manual testing works out of the box.

### Step 4 — Verify Use Case Auto-Registration

Confirm each use case class:
- File ends in `UseCase.cs`
- Class name ends in `UseCase`
- Extends `BaseInOutUseCase<TReq, TResp>` (or `BaseInUseCase<T>` / `BaseOutUseCase<T>`)

No `AddScoped` call is needed — auto-registration handles it.

---

## Full Endpoint Class Template

```csharp
using Application.Common.Requests;
using Application.Common.Services;
using Application.Common.UseCases;
using Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

internal static class ProductEndpoints
{
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        var cache = app.Services.GetRequiredService<IHybridCacheService>();

        var group = app.MapGroup("/products")
            .WithTags("Products");

        // GET /{id} — single item with optional cache
        group.MapGet("/{id}", async (
            [FromServices] IBaseInOutUseCase<GetProductRequest, BaseResponse<ProductDto>> useCase,
            [FromHeader] Guid correlationId,
            [FromRoute] int id,
            CancellationToken cancellationToken,
            [FromHeader] bool cacheEnabled = true
        ) =>
        {
            var response = cacheEnabled switch
            {
                true => await cache.GetOrCreateAsync(
                    correlationId,
                    $"{nameof(ProductEndpoints)}-{id}",
                    async (cancellationToken) => await useCase.HandleAsync(new(correlationId, id), cancellationToken),
                    cancellationToken
                ),
                false or _ => await useCase.HandleAsync(new(correlationId, id), cancellationToken),
            };

            return response.Success ? Results.Ok(response) : Results.NotFound(response);
        });

        // POST /paginated — paginated list
        group.MapPost("/paginated", async (
            [FromServices] IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<ProductDto>> useCase,
            [FromBody] BasePaginatedRequest request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request, cancellationToken);

            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        // POST / — create
        group.MapPost("/", async (
            [FromServices] IBaseInOutUseCase<CreateProductRequest, BaseResponse<ProductDto>> useCase,
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request, cancellationToken);

            if (!response.Success || response.Data == null)
                return Results.BadRequest(response);

            return Results.Created($"/products/{response.Data.Id}", response);
        });

        // PUT /{id} — update
        group.MapPut("/{id}", async (
            [FromServices] IBaseInOutUseCase<UpdateProductRequest, BaseResponse<ProductDto>> useCase,
            [FromBody] UpdateProductRequest request,
            [FromRoute] int id,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request with { ProductId = id }, cancellationToken);

            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        // DELETE /{id}
        group.MapDelete("/{id}", async (
            [FromServices] IBaseInOutUseCase<DeleteProductRequest, BaseResponse> useCase,
            [FromHeader] Guid correlationId,
            [FromRoute] int id,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(new(correlationId, id), cancellationToken);

            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        return app;
    }
}
```

---

## Endpoint Patterns by Verb

### GET /{id} — Single Entity with Cache

Always use the `cacheEnabled` switch pattern. Cache key must include the class name to avoid collisions.

```csharp
group.MapGet("/{id}", async (
    [FromServices] IBaseInOutUseCase<GetProductRequest, BaseResponse<ProductDto>> useCase,
    [FromHeader] Guid correlationId,
    [FromRoute] int id,
    CancellationToken cancellationToken,
    [FromHeader] bool cacheEnabled = true
) =>
{
    var response = cacheEnabled switch
    {
        true => await cache.GetOrCreateAsync(
            correlationId,
            $"{nameof(ProductEndpoints)}-{id}",  // ← unique per endpoint + id
            async (cancellationToken) => await useCase.HandleAsync(new(correlationId, id), cancellationToken),
            cancellationToken
        ),
        false or _ => await useCase.HandleAsync(new(correlationId, id), cancellationToken),
    };

    return response.Success ? Results.Ok(response) : Results.NotFound(response);  // 200 or 404
});
```

**Rules:**
- `[FromHeader] bool cacheEnabled = true` — default to cached; client can opt out
- Cache key: `$"{nameof(XxxEndpoints)}-{id}"`
- `correlationId` is passed to `GetOrCreateAsync` for distributed cache scoping
- Returns `404 NotFound` on failure, not `400`

### POST /paginated — Paginated List

Use `BasePaginatedRequest` directly from the request body — no custom request record needed.

```csharp
group.MapPost("/paginated", async (
    [FromServices] IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<ProductDto>> useCase,
    [FromBody] BasePaginatedRequest request,
    CancellationToken cancellationToken
) =>
{
    var response = await useCase.HandleAsync(request, cancellationToken);

    return response.Success ? Results.Ok(response) : Results.BadRequest(response);  // 200 or 400
});
```

### POST / — Create

Returns `201 Created` with a `Location` header pointing to the new resource.

```csharp
group.MapPost("/", async (
    [FromServices] IBaseInOutUseCase<CreateProductRequest, BaseResponse<ProductDto>> useCase,
    [FromBody] CreateProductRequest request,
    CancellationToken cancellationToken
) =>
{
    var response = await useCase.HandleAsync(request, cancellationToken);

    if (!response.Success || response.Data == null)
        return Results.BadRequest(response);  // 400

    return Results.Created($"/products/{response.Data.Id}", response);  // 201 + Location
});
```

**Rules:**
- Check both `!response.Success` AND `response.Data == null` before returning `BadRequest`
- Location path must match the GET route: `$"/{route}/{response.Data.Id}"`

### PUT /{id} — Update

Bind `id` from route and inject into request using `with` expression.

```csharp
group.MapPut("/{id}", async (
    [FromServices] IBaseInOutUseCase<UpdateProductRequest, BaseResponse<ProductDto>> useCase,
    [FromBody] UpdateProductRequest request,
    [FromRoute] int id,
    CancellationToken cancellationToken
) =>
{
    var response = await useCase.HandleAsync(request with { ProductId = id }, cancellationToken);

    return response.Success ? Results.Ok(response) : Results.BadRequest(response);  // 200 or 400
});
```

### DELETE /{id}

Delete use cases typically return `BaseResponse` (no data payload).

```csharp
group.MapDelete("/{id}", async (
    [FromServices] IBaseInOutUseCase<DeleteProductRequest, BaseResponse> useCase,
    [FromHeader] Guid correlationId,
    [FromRoute] int id,
    CancellationToken cancellationToken
) =>
{
    var response = await useCase.HandleAsync(new(correlationId, id), cancellationToken);

    return response.Success ? Results.Ok(response) : Results.BadRequest(response);  // 200 or 400
});
```

---

## EndpointExtensions.cs Registration

**File**: `src/WebApp/Endpoints/EndpointExtensions.cs`

Add the new `Map...Endpoints()` call to the list:

```csharp
namespace WebApp.Endpoints;

internal static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapOrderEndpoints();
        app.MapProductEndpoints();   // ← add new domain here
        return app;
    }
}
```

---

## Correlation ID Pattern

`correlationId` is a required `Guid` header for endpoints that need distributed tracing or caching. It flows from the HTTP header through the request record to the use case and repository.

```csharp
// ✅ Required for GET (cache key scoping + tracing)
[FromHeader] Guid correlationId,

// ✅ For POST/PUT, include CorrelationId in the request body record
public sealed record CreateProductRequest(
    Guid CorrelationId,   // ← must be first parameter
    string Name,
    ...
) : BaseRequest(CorrelationId, ...);

// ❌ Do not add [FromHeader] correlationId when it's already in the request body
```

---

## Cache Pattern Rules

1. **Only cache GET /{id}** — paginated, create, update, and delete are never cached
2. **Cache key uniqueness**: always prefix with `nameof(XxxEndpoints)` to prevent cross-domain collisions
3. **Client opt-out**: always include `[FromHeader] bool cacheEnabled = true`
4. **Resolve cache once**: call `app.Services.GetRequiredService<IHybridCacheService>()` at group creation level, not inside handlers

```csharp
// ✅ Correct: resolve once per group
var cache = app.Services.GetRequiredService<IHybridCacheService>();

// ❌ Wrong: resolve inside handler lambda — creates a new resolution on every request
group.MapGet("/{id}", async (...) =>
{
    var cache = app.Services.GetRequiredService<IHybridCacheService>(); // ← don't do this
    ...
});
```

---

## Tips

### Only include endpoints that have a corresponding use case

Do not create endpoints speculatively. Each `MapGet`, `MapPost`, `MapPut`, `MapDelete` requires an existing use case. Check `src/Application/{Domain}/` before adding a handler.

### Use `[FromBody]` for POST/PUT payloads, `[FromRoute]` for path params

Never mix them — route `{id}` must always be `[FromRoute] int id`.

### `BaseResponse` vs `BaseResponse<T>`

- `BaseResponse` — no data payload (e.g., delete operations)
- `BaseResponse<T>` — single entity result
- `BasePaginatedResponse<T>` — paged list with `TotalPages` + `TotalRecords`
