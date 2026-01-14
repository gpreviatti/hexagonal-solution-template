using Contracts.Common;

namespace Contracts.Orders;

/// <summary>
/// Request to create a new order
/// </summary>
/// <param name="CorrelationId">The unique identifier for the request</param>
/// <param name="Description">A description of the order</param>
/// <param name="Items">The items included in the order</param>
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId);

/// <summary>
/// An item included in the order
/// </summary>
/// <param name="Name">The name of the item</param>
/// <param name="Description">A description of the item</param>
/// <param name="Value">The value of the item</param>
public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);