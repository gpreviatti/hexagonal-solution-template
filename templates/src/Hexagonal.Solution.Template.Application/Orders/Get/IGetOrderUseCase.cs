using Hexagonal.Solution.Template.Application.Common.Messages;
using MediatR;

namespace Hexagonal.Solution.Template.Application.Orders.Get;
public interface IGetOrderUseCase : IRequestHandler<GetOrderRequest, BaseResponse<OrderDto>>
{ }
