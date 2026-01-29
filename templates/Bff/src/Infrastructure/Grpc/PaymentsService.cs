using GrpcPayment;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService
{
    private readonly GrpcPayment.PaymentService.PaymentServiceClient _Client;

    public PaymentsService(string baseAddress, ILogger<PaymentsService> logger)
        : base(baseAddress, logger) => _Client = new GrpcPayment.PaymentService.PaymentServiceClient(Channel);

    public async Task<PaymentReply> CreatePaymentAsync(CreatePaymentRequest request) =>
        await ExecuteHandlerAsync<CreatePaymentRequest, PaymentReply>(() => _Client.CreateAsync(request));
}
