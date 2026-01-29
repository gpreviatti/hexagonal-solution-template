using GrpcPayment;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService
{
    private readonly PaymentService.PaymentServiceClient _Client;

    public PaymentsService(string baseAddress, ILogger<PaymentsService> logger)
        : base(baseAddress, logger) => _Client = new PaymentService.PaymentServiceClient(Channel);

    public async Task<PaymentReply> CreatePaymentAsync(CreatePaymentRequest request) =>
        await ExecuteHandlerAsync(() => _Client.CreateAsync(request));
}
