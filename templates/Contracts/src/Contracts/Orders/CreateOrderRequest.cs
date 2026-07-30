using Contracts.Common;

namespace Contracts.Orders;

/// <summary>
/// Request to create a new order
/// </summary>
/// <param name="CorrelationId">The unique identifier for the request</param>
/// <param name="Description">A description of the order</param>
/// <param name="Items">The items included in the order</param>
/// <param name="CreatedBy">The user creating the order</param>
/// <param name="TimezoneId">The timezone identifier for the request</param>
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items,
    string CreatedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, CreatedBy, TimezoneId);

/// <summary>
/// An item included in the order
/// </summary>
/// <param name="Name">The name of the item</param>
/// <param name="Description">A description of the item</param>
/// <param name="Value">The value of the item</param>
public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);
