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
        var result = await _Client.CreateAsync(request);

        return new(result.Success, result.Message);
    }
}