using Application.Common.Messages;
using Application.Orders;
using MediatR;

namespace Application.Orders.Get;
public interface IGetOrderUseCase : IRequestHandler<GetOrderRequest, BaseResponse<OrderDto>>
{ }
