using Microsoft.AspNetCore.Mvc;

namespace MockApi.Endpoints;

internal static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("orders").WithTags("orders");

        ordersGroup.MapGet("/{id}", async (
            [FromHeader] Guid correlationId,
            [FromRoute] int id,
            CancellationToken cancellationToken,
            [FromHeader] bool cacheEnabled = true
        ) => {
            return Results.Ok(new
            {
                Success = true,
                Message = string.Empty,
                Data = new
                {
                    Id = id,
                    Total = 100.0,
                    Items = new[]
                    {
                        new { Id = 1, Name = "Item 1", Value = 50.0 },
                        new { Id = 2, Name = "Item 2", Value = 50.0 }
                    }
                }
            });
        });

        ordersGroup.MapPost("/", async (
            [FromBody] dynamic request,
            CancellationToken cancellationToken
        ) =>
        {
            return Results.Created($"/orders/1", new
            {
                Success = true,
                Message = string.Empty,
                Data = new
                {
                    Id = 1,
                    Total = 100.0,
                    Items = new[]
                    {
                        new { Id = 1, Name = "Item 1", Value = 50.0 },
                        new { Id = 2, Name = "Item 2", Value = 50.0 }
                    }
                }
            });
        });

        return app;
    }
}
