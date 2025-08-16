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
            [FromServices] IBaseInOutUseCase<GetOrderRequest, OrderDto, GetOrderUseCase> useCase,
            int id,
            Guid correlationId
        ) => Results.Ok(await useCase.Handle(new(correlationId, id), CancellationToken.None)));

        ordersGroup.MapPost("/", async (
            [FromServices] IBaseInOutUseCase<CreateOrderRequest, OrderDto, CreateOrderUseCase> useCase,
            [FromBody] CreateOrderRequest request
        ) =>
        {
            var response = await useCase.Handle(request, CancellationToken.None);

            if (!response.Success)
                return Results.BadRequest(response);

            return Results.Created($"/orders/{response.Data.Id}", response);
        });

        return app;
    }
}