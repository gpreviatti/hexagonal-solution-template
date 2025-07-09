using Application.Common.Messages;
using MediatR;

namespace Application.Orders.Create;
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId), IRequest<BaseResponse<OrderDto>>;
