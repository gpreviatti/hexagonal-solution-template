using Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Cache;
using Contracts.Orders;
using Contracts.Common;

namespace WebApp.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var serviceKey = ServicesKeys.Orders.ToString();

        var ordersGroup = app.MapGroup(serviceKey)
            .WithTags(serviceKey)
            .RequireRateLimiting(serviceKey);

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
                    async (cancellationToken) => await httpService.SendAsync<BaseResponse<OrderDto>>($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
                    {
                        { "CorrelationId", (correlationId ?? Guid.NewGuid()).ToString() }
                    }),
                    cancellationToken
                ),
                false or _ => await httpService.SendAsync<BaseResponse<OrderDto>>($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
                {
                    { "CorrelationId", (correlationId ?? Guid.NewGuid()).ToString() }
                }),
            };

            return response != null ? Results.Ok(response) : Results.NotFound(response);
        })
        .Produces<BaseResponse<OrderDto>>(StatusCodes.Status200OK)
        .Produces<BaseResponse>(StatusCodes.Status404NotFound)
        .Produces<BaseResponse>(StatusCodes.Status400BadRequest)
        .Produces<BaseResponse>(StatusCodes.Status500InternalServerError)
        .WithDescription("Gets an order by its identifier")
        .WithName("GetById");

        ordersGroup.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            [FromKeyedServices(ServicesKeys.Orders)] BaseHttpService httpService,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await httpService.SendAsync<CreateOrderRequest, BaseResponse<OrderDto>>("orders", HttpMethod.Post, cancellationToken, request);

            if (response == null)
                return Results.BadRequest();

            var id = response.Data?.Id.ToString() ?? "unknown";

            return Results.Created($"/orders/{id}", response);
        })
        .Produces<BaseResponse<OrderDto>>(StatusCodes.Status201Created)
        .Produces<BaseResponse>(StatusCodes.Status400BadRequest)
        .Produces<BaseResponse>(StatusCodes.Status500InternalServerError)
        .WithDescription("Creates a new order")
        .WithName("Create");

        return app;
    }
}
