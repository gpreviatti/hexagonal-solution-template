using Hexagonal.Solution.Template.Application.Common.Messages;
using MediatR;

namespace Hexagonal.Solution.Template.Application.Orders.Create;
public sealed record CreateOrderRequest(string Description, CreateOrderItemRequest[] Items) : IRequest<BaseResponse<OrderDto>>;
