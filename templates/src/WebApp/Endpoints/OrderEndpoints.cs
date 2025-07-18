using Application.Common.UseCases;
using Application.Orders;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/orders/{id}/{correlationId}", async (
            IBaseInOutUseCase<GetOrderRequest, OrderDto> useCase,
            int id,
            Guid correlationId
        ) => Results.Ok(await useCase.Handle(new(correlationId, id), CancellationToken.None)));

        app.MapPost("/orders", async (
            IBaseInOutUseCase<CreateOrderRequest, OrderDto> useCase,
            [FromBody] CreateOrderRequest request
        ) =>
        {
            var response = await useCase.Handle(request, CancellationToken.None);

            return Results.Created($"/orders/{response.Data.Id}", response);
        });

        return app;
    }
}