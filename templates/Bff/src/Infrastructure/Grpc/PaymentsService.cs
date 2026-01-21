using Contracts.Common;
using Contracts.Payments;
using static GrpcPayment.PaymentService;

namespace Infrastructure.Grpc;

public sealed class PaymentsService : BaseGrpcService
{
    private readonly PaymentServiceClient _Client;

    public PaymentsService(string baseAddress) : base(baseAddress) => _Client = new(Channel);

    public async Task<BaseResponse> CreateAsync(CreatePaymentRequest request)
    {
        var grpcRequest = new GrpcPayment.CreatePaymentRequest
        {
            CorrelationId = request.CorrelationId.ToString(),
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethod
        };

        var result = await _Client.CreateAsync(grpcRequest);

        return new(result.Success, result.Message);
    }
}