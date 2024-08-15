using Application.Common.Messages;
using MediatR;

namespace Application.Orders.Create;
public sealed record CreateOrderRequest(string Description, CreateOrderItemRequest[] Items) : IRequest<BaseResponse<OrderDto>>;