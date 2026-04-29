using Contracts.Orders;

namespace MockApi.Endpoints;

internal static class OrderSummaryEndpoints
{
    public static WebApplication MapOrderSummaryEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("orders").WithTags("orders");

        ordersGroup.MapGet("/summary", () =>
            Results.Ok(new GetOrderSummaryResponse(
                success: true,
                data: new OrderSummaryDto(42, 12345.67m, "USD"),
                message: "Order summary retrieved."))
        );

        return app;
    }
}
