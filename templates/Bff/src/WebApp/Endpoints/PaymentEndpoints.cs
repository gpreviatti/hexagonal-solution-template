using Contracts.Common;
using GrpcPayment;
using Infrastructure.Grpc;
using Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

public static partial class PaymentEndpoints
{
    private const string BasePath = "api/payments";

    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var serviceKey = ServicesKeys.Payments.ToString();

        var group = endpoints.MapGroup(BasePath)
            .WithTags(serviceKey)
            .RequireRateLimiting(serviceKey);

        group.MapPost("/", async (
            [FromServices] PaymentsService paymentsService,
            [FromBody] CreatePaymentRequest request
        ) =>
        {
            var result = await paymentsService.CreatePaymentAsync(request);

            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .Produces<BaseResponse>(StatusCodes.Status200OK)
        .Produces<BaseResponse>(StatusCodes.Status400BadRequest)
        .Produces<BaseResponse>(StatusCodes.Status500InternalServerError)
        .WithDescription("Creates a new payment")
        .WithName("CreatePayment");

        return endpoints;
    }
}
