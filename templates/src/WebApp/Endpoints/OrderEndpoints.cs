using Application.Common.Requests;
using Application.Common.Services;
using Application.Common.UseCases;
using Application.Orders;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var cache = app.Services.GetRequiredService<IHybridCacheService>();

        var ordersGroup = app.MapGroup("/orders")
            .WithTags("Orders");

        ordersGroup.MapGet("/{id}", async (
            [FromServices] IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> useCase,
            [FromHeader] Guid correlationId,
            int id,
            CancellationToken cancellationToken
        ) => {
            var response = await cache.GetOrCreateAsync(
                $"{nameof(OrderEndpoints)}-{id}",
                async (cancellationToken) => await useCase.HandleAsync(
                        new(correlationId, id),
                        cancellationToken
                ),
                cancellationToken
            );

            return response.Success ? Results.Ok(response) : Results.NotFound(response);
        });

        ordersGroup.MapPost("/", async (
            [FromServices] IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>, CreateOrderUseCase> useCase,
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request, cancellationToken);

            return response.Success ? Results.Created($"/orders/{response.Data.Id}", response) : Results.BadRequest(response);
        });

        ordersGroup.MapPost("/paginated", async (
            [FromServices] IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>, GetAllOrdersUseCase> useCase,
            [FromBody] BasePaginatedRequest request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request, cancellationToken);

            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        return app;
    }
}
