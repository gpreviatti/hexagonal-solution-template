using AutoFixture;
using Contracts.Common;
using Contracts.Orders;
using Microsoft.AspNetCore.Mvc;

namespace MockApi.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("orders").WithTags("orders");
        var autoFixture = new Fixture();

        ordersGroup.MapGet("/{id}", async (
            [FromRoute] int id,
            CancellationToken cancellationToken
        ) => id switch
        {
            1 => Results.Ok(autoFixture.Create<BaseResponse<OrderDto>>()),
            2 => Results.Ok(autoFixture.Create<BaseResponse<OrderDto>>()),
            _ => Results.NotFound(autoFixture.Create<BaseResponse>())
        });

        ordersGroup.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken
        ) => Results.Created($"/orders/1", autoFixture.Create<BaseResponse<OrderDto>>()));

        return app;
    }
}
