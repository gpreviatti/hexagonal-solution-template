using Grpc.Core;
using GrpcPayment;
using static GrpcPayment.PaymentService;

namespace MockApi.GrpcServices;

public class PaymentService : PaymentServiceBase
{
    public override async Task<PaymentReply> Create(CreatePaymentRequest request, ServerCallContext context) => new()
    {
        Success = true,
        Message = "Payment created successfully"
    };
}
