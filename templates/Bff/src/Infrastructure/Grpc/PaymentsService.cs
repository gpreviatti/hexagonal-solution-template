using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService
{
    private readonly GrpcPayment.PaymentService.PaymentServiceClient _Client;

    public PaymentsService(string baseAddress, ILogger<PaymentsService> logger)
        : base(baseAddress, logger) => _Client = new GrpcPayment.PaymentService.PaymentServiceClient(Channel);

    // public async Task<GrpcPayment.PaymentReply> CreatePaymentAsync(GrpcPayment.CreatePaymentRequest request) =>
    //     await ExecuteHandlerAsync(request, _Client.CreateAsync);
}
