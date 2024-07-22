using Application.Common.Messages;
using Application.Orders;
using MediatR;

namespace Application.Orders.Get;

public sealed record GetOrderRequest(int Id) : IRequest<BaseResponse<OrderDto>>;