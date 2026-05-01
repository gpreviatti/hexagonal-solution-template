using Contracts.Common;

namespace Contracts.Orders;

/// <summary>
/// Represents the response for retrieving order summary information.
/// </summary>
public sealed record GetOrderSummaryResponse : BaseResponse<OrderSummaryDto>
{
    /// <summary>
    /// Parameterless constructor for deserialization purposes.
    /// </summary>
    public GetOrderSummaryResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrderSummaryResponse"/> class with the specified parameters.
    /// </summary>
    public GetOrderSummaryResponse(bool success, OrderSummaryDto? data = null, string? message = null) : base(success, data, message) { }
}
