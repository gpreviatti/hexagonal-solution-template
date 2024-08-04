using Application.Common.Messages;
using Application.Orders;
using MediatR;

namespace Application.Orders.Create;
public sealed record CreateOrderRequest(string Description, CreateOrderItemRequest[] Items) : IRequest<BaseResponse<OrderDto>>;
