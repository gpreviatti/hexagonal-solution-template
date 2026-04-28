using Contracts.Common;

namespace Contracts.Orders;

public sealed record GetOrderSummaryRequest(Guid CorrelationId) : BaseRequest(CorrelationId);