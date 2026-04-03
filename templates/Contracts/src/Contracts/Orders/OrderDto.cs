namespace Contracts.Orders;

/// <summary>
/// Data transfer object representing an order
/// </summary>
public sealed record OrderDto
{
    /// <summary>
    /// The unique identifier of the order
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A description of the order
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The total value of the order
    /// </summary>
    /// <example>99.99</example>
    public decimal Total { get; set; }

    /// <summary>
    /// The items included in the order
    /// </summary>
    public IReadOnlyCollection<ItemDto>? Items { get; set; }
};

/// <summary>
/// Data transfer object representing an item in an order
/// </summary>
public sealed record ItemDto
{
    /// <summary>
    /// The unique identifier of the item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the item
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the item
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The value of the item
    /// </summary>
    /// <example>99.99</example>
    public decimal Value { get; set; }
};
