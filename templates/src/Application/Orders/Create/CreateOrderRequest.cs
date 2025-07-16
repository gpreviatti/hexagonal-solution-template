using Application.Common.Messages;

namespace Application.Orders.Create;
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId);
