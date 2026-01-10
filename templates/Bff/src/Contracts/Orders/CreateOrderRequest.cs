using Contracts.Common;

namespace Contracts.Orders;

public sealed record CreateOrderRequest(Guid CorrelationId, string Description, CreateOrderItemRequest[] Items) : BaseRequest(CorrelationId);
public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);