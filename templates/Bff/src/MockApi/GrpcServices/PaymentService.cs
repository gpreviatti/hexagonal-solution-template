using AutoFixture;
using Grpc.Core;
using GrpcPayment;
using static GrpcPayment.PaymentService;

namespace MockApi.GrpcServices;

public class PaymentService : PaymentServiceBase
{
    public override async Task<PaymentReply> Create(
        CreatePaymentRequest request,
        ServerCallContext context
    ) => new Fixture().Create<PaymentReply>();
}
