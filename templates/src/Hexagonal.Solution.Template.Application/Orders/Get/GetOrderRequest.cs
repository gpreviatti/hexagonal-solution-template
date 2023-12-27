using Hexagonal.Solution.Template.Application.Common.Messages;
using MediatR;

namespace Hexagonal.Solution.Template.Application.Orders.Get;

public record GetOrderRequest(int Id) : IRequest<BaseResponse<OrderDto>>;