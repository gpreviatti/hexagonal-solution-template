using Application.Common.Requests;
using Application.Common.Services;
using Application.Common.UseCases;
using Application.Orders;
using Grpc.Core;
using GrpcOrder;
using static GrpcOrder.OrderService;
using GetOrderRequest = Application.Orders.GetOrderRequest;
using OrderDto = Application.Orders.OrderDto;

namespace WebApp.GrpcServices;

public class OrderService(
    IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> useCase,
    IHybridCacheService cache
) : OrderServiceBase
{
    private readonly IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase> _useCase = useCase;
    private readonly IHybridCacheService _cache = cache;

    public override async Task<OrderReply> Get(
        GrpcOrder.GetOrderRequest request,
        ServerCallContext context
    )
    {
        var correlationId = Guid.TryParse(request.CorrelationId, out var guid) ? guid : Guid.Empty;

        var result = await _cache.GetOrCreateAsync($"order-{request.Id}", async cancellationToken =>
            await _useCase.HandleAsync(
                new(correlationId, request.Id),
                cancellationToken,
                $"order-{request.Id}"
            ),
            context.CancellationToken
        );

        if (!result.Success)
            return new() { Success = false, Message = result.Message };

        return new()
        {
            Success = true,
            Message = string.Empty,
            Data = new()
            {
                Id = result.Data.Id,
                Description = result.Data.Description,
                Total = double.TryParse(result.Data.Total.ToString(), out var total) ? total : 0.0
            }
        };
    }
}
