using Application.Common.Messages;

namespace Application.Orders.Get;

public interface IGetOrderUserCase
{
    Task<BaseResponse<OrderDto>> Handle(GetOrderRequest request, CancellationToken cancellationToken);
}
