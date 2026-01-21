using GrpcPayment;
using static GrpcPayment.PaymentService;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService<PaymentServiceClient>
{
    private readonly PaymentServiceClient _Client;
    public PaymentsService(string baseAddress) : base(baseAddress) => _Client = new(Channel);

    public async Task<PaymentReply> CreateAsync(CreatePaymentRequest request) => await _Client.CreateAsync(request);
}