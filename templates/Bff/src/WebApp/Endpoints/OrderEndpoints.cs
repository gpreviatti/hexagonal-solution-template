using Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Cache;

namespace WebApp.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var serviceKey = ServicesKeys.Orders.ToString();

        var ordersGroup = app.MapGroup(serviceKey).WithTags(serviceKey);

        ordersGroup.MapGet("/{id}", async (
            [FromKeyedServices(ServicesKeys.Orders)] BaseHttpService httpService,
            [FromRoute] int id,
            [FromServices] HybridCacheService cache,
            CancellationToken cancellationToken,
            [FromHeader] Guid? correlationId = null,
            [FromHeader] bool cacheEnabled = true
        ) => {
            var response = cacheEnabled switch
            {
                true => await cache.GetOrCreateAsync(
                    $"{nameof(OrderEndpoints)}-{id}",
                    async (cancellationToken) => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
                    {
                        { "CorrelationId", (correlationId ?? Guid.NewGuid()).ToString() }
                    }),
                    cancellationToken
                ),
                false or _ => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
                {
                    { "CorrelationId", (correlationId ?? Guid.NewGuid()).ToString() }
                }),
            };

            return response?.Success ? Results.Ok(response) : Results.NotFound(response);
        });

        ordersGroup.MapPost("/", async (
            [FromBody] object request,
            [FromKeyedServices(ServicesKeys.Orders)] BaseHttpService httpService,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await httpService.SendAsync("orders", HttpMethod.Post, cancellationToken, request);

            if (response == null)
                return Results.BadRequest(new { Success = false, Message = "No response from service" });

            bool success = response.Success ?? false;
            if (!success || response.Data == null)
                return Results.BadRequest(response);

            var data = response.Data;
            
            var id = data?.Id?.ToString() ?? "unknown";

            return Results.Created($"/orders/{id}", response);
        });

        return app;
    }
}
