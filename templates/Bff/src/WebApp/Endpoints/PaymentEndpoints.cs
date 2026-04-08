using System.Diagnostics;
using Contracts.Common;
using GrpcPayment;
using Infrastructure.Common;
using Infrastructure.Grpc;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints;

public static partial class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var serviceKey = ServicesKey.Payments.ToString();

        var group = endpoints.MapGroup(serviceKey)
            .WithTags(serviceKey)
            .RequireRateLimiting(serviceKey);

        group.MapPost("/", async (
            [FromServices] PaymentsService paymentsService,
            [FromBody] CreatePaymentRequest request
        ) =>
        {
            using var activity = DefaultConfigurations.ActivitySource.StartActivity($"{nameof(PaymentEndpoints)}.CreatePayment");

            var result = await paymentsService.CreatePaymentAsync(request);

            activity?.SetTag("payment.amount", request.Amount);
            activity?.SetTag("payment.currency", request.Currency);
            activity?.SetStatus(result.Success ? ActivityStatusCode.Ok : ActivityStatusCode.Error, result.Message);

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
