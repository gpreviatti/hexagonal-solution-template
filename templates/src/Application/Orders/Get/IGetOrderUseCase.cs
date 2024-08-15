using Application.Common.Messages;
using MediatR;

namespace Application.Orders.Get;
public interface IGetOrderUseCase : IRequestHandler<GetOrderRequest, BaseResponse<OrderDto>>
{ }