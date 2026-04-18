# Minimal API Endpoint Template

```csharp
internal static class {Domain}Endpoints
{
    public static WebApplication Map{Domain}Endpoints(this WebApplication app)
    {
        var cache = app.Services.GetRequiredService<IHybridCacheService>();
        var group = app.MapGroup("/{route}").WithTags("{Domain}");

        group.MapGet("/{id}", async (
            [FromServices] IBaseInOutUseCase<Get{Entity}Request, BaseResponse<{Entity}Dto>> useCase,
            [FromHeader] Guid correlationId,
            [FromRoute] int id,
            CancellationToken cancellationToken,
            [FromHeader] bool cacheEnabled = true) =>
        {
            var response = cacheEnabled
                ? await cache.GetOrCreateAsync(correlationId, $"{nameof({Domain}Endpoints)}-{id}",
                    async ct => await useCase.HandleAsync(new(correlationId, id), ct), cancellationToken)
                : await useCase.HandleAsync(new(correlationId, id), cancellationToken);

            return response.Success ? Results.Ok(response) : Results.NotFound(response);
        });

        return app;
    }
}
```
