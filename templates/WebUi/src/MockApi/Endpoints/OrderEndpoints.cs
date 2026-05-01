using AutoFixture;
using Contracts.Common;
using Contracts.Orders;

namespace MockApi.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("orders").WithTags("orders");
        var fixture = new Fixture();

        ordersGroup.MapGet("/summary", () => Results.Ok(new GetOrderSummaryResponse(
            success: true,
            data: fixture.Create<OrderSummaryDto>(),
            message: "Order summary retrieved."
        )));

        ordersGroup.MapGet("/", () => Results.Ok(new BaseResponse<IEnumerable<OrderDto>>(
            success: true,
            data: [.. fixture.CreateMany<OrderDto>(Random.Shared.Next(1, 5))],
            message: "Orders retrieved."
        )));

        return app;
    }
}
