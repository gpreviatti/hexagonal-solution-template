namespace Application.Orders.Create;

using Application.Common.Messages;

public interface ICreateOrderUseCase
{
    Task<BaseResponse<OrderDto>> Handle(CreateOrderRequest request, CancellationToken cancellationToken);
}