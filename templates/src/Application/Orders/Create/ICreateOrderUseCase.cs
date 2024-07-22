using Application.Common.Messages;
using Application.Orders;
using MediatR;

namespace Application.Orders.Create;
public interface ICreateOrderUseCase : IRequestHandler<CreateOrderRequest, BaseResponse<OrderDto>>
{ }
