using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService
{
    private readonly GrpcPayment.PaymentService.PaymentServiceClient _Client;

    public PaymentsService(string baseAddress, ILogger<PaymentsService> logger)
        : base(baseAddress, logger)
    {
        _Client = new GrpcPayment.PaymentService.PaymentServiceClient(Channel);
    }

    public override async Task<TResponse> HandleInternalAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        if (request is not GrpcPayment.CreatePaymentRequest grpcRequest)
            throw new InvalidOperationException($"Unsupported request type: {request?.GetType().FullName}");

        var asyncCall = _Client.CreateAsync(grpcRequest);
        var grpcResponse = await asyncCall.ResponseAsync;

        return (TResponse)(object)grpcResponse!;
    }
}
