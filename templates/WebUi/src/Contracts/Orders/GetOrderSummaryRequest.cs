using Contracts.Common;

namespace Contracts.Orders;

/// <summary>
/// Represents the request for retrieving order summary information.
/// </summary>
/// <param name="CorrelationId"></param>
public sealed record GetOrderSummaryRequest(Guid CorrelationId) : BaseRequest(CorrelationId);
