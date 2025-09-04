using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Orders;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("/orders")
            .WithTags("Orders");

        ordersGroup.MapGet("/{id}/{correlationId}", async (
            [FromServices] IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> useCase,
            int id,
            Guid correlationId,
            CancellationToken cancellationToken
        ) => Results.Ok(await useCase.HandleAsync(new(correlationId, id), cancellationToken)));

        ordersGroup.MapPost("/", async (
            [FromServices] IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>, CreateOrderUseCase> useCase,
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken
        ) =>
        {
            var response = await useCase.HandleAsync(request, cancellationToken);

            if (!response.Success)
                return Results.BadRequest(response);

            return Results.Created($"/orders/{response.Data.Id}", response);
        });

        ordersGroup.MapPost("/paginated", async (
            [FromServices] IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>, GetAllOrdersUseCase> useCase,
            [FromBody] BasePaginatedRequest request,
            CancellationToken cancellationToken
        ) => Results.Ok(await useCase.HandleAsync(request, cancellationToken)));

        return app;
    }
}