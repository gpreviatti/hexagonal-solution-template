using Hexagonal.Solution.Template.Application.Common.Messages;
using MediatR;

namespace Hexagonal.Solution.Template.Application.Orders.Get;

public sealed record GetOrderRequest(int Id) : IRequest<BaseResponse<OrderDto>>;