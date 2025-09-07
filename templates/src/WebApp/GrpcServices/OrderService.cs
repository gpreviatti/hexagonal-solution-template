using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Orders;
using Grpc.Core;
using GrpcOrder;
using static GrpcOrder.OrderService;
using GetOrderRequest = Application.Orders.GetOrderRequest;

namespace WebApp.GrpcServices;

internal class OrderService(IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> useCase) : OrderServiceBase
{
    private readonly IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> _useCase = useCase;

    public override async Task<OrderReply> Get(
        GrpcOrder.GetOrderRequest request,
        ServerCallContext context
    )
    {
        var correlationId = Guid.TryParse(request.CorrelationId, out var guid) ? guid : Guid.Empty;

        var result = await _useCase.HandleAsync(
            new(correlationId, request.Id),
            context.CancellationToken,
            $"order-{request.Id}"
        );

        if (!result.Success)
            return new();

        return new()
        {
            Id = result.Data.Id,
            Description = result.Data.Description,
            Total = double.TryParse(result.Data.Total.ToString(), out var total) ? total : 0.0
        };
    }
}
