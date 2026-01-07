using Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Cache;
using Infrastructure.Common;

namespace WebApp.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var cache = app.Services.GetRequiredService<HybridCacheService>();
        var httpService = app.Services.GetRequiredKeyedService<BaseHttpService>(ServicesKeys.Orders);

        var ordersGroup = app.MapGroup("/orders")
            .WithTags("Orders");

        ordersGroup.MapGet("/{id}", async (
            [FromHeader] Guid correlationId,
            [FromRoute] int id,
            CancellationToken cancellationToken,
            [FromHeader] bool cacheEnabled = true
        ) => {
            var response = cacheEnabled switch
            {
                true => await cache.GetOrCreateAsync(
                    $"{nameof(OrderEndpoints)}-{id}",
                    async (cancellationToken) => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, new()
                    {
                        { "CorrelationId", correlationId.ToString() }
                    }),
                    cancellationToken
                ),
                false or _ => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, new()
                {
                    { "CorrelationId", correlationId.ToString() }
                }),
            };

            return response?.Success ? Results.Ok(response) : Results.NotFound(response);
        });

        ordersGroup.MapPost("/", async (
            [FromBody] dynamic request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await httpService.SendAsync("orders", request, cancellationToken);

            if (!response.Success || response.Data == null)
                return Results.BadRequest(response);

            return Results.Created($"/orders/{response.Data.Id}", response);
        });

        ordersGroup.MapPost("/paginated", async (
            [FromBody] dynamic request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await httpService.SendAsync("orders/paginated", request, cancellationToken);

            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        return app;
    }
}
