using Application.Common.Messages;

namespace Application.Orders.Get;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);
