using Application.Common.Messages;
using MediatR;

namespace Application.Orders.Create;
public interface ICreateOrderUseCase : IRequestHandler<CreateOrderRequest, BaseResponse<OrderDto>> { }
