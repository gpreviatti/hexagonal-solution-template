using Grpc.Net.ClientFactory;
using GrpcPayment;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public sealed class PaymentsService(ILogger<PaymentsService> logger, GrpcClientFactory grpcClientFactory) : BaseGrpcService<PaymentService.PaymentServiceClient>(logger, grpcClientFactory)
{
    public async Task<PaymentReply> CreatePaymentAsync(CreatePaymentRequest request) =>
        await ExecuteHandlerAsync(() => Client.CreateAsync(request));
}
