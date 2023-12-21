using Hexagonal.Solution.Template.Application.Common.Messages;
using MediatR;

namespace Hexagonal.Solution.Template.Application.Orders.Create;
public interface ICreateOrderUseCase : IRequestHandler<CreateOrderRequest, BaseResponse<OrderDto>>
{}
