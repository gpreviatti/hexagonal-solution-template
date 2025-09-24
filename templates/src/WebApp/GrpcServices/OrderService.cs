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
    ) => await _cache.GetOrCreateAsync<OrderReply>(
        $"order-{request.Id}",
        async cancellationToken =>
        {
            var correlationId = Guid.TryParse(request.CorrelationId, out var guid) ? guid : Guid.Empty;

            var response = await _useCase.HandleAsync(
                new(correlationId, request.Id),
                cancellationToken,
                $"order-grpc-{request.Id}"
            );

            if (!response.Success)
                return new() { Success = false, Message = response.Message };

            return new()
            {
                Success = true,
                Message = string.Empty,
                Data = new()
                {
                    Id = response.Data.Id,
                    Description = response.Data.Description,
                    Total = double.TryParse(response.Data.Total.ToString(), out var total) ? total : 0.0
                }
            };
        },
        context.CancellationToken
    );
}
