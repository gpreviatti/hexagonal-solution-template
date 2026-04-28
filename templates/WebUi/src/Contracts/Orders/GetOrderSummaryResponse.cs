using Contracts.Common;

namespace Contracts.Orders;

public sealed record GetOrderSummaryResponse : BaseResponse<OrderSummaryDto>
{
    public GetOrderSummaryResponse() { }

    public GetOrderSummaryResponse(bool success, OrderSummaryDto? data = null, string? message = null) : base(success, data, message) {}
}