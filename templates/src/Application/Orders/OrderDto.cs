namespace Application.Orders;
public sealed record OrderDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyCollection<ItemDto>? Items { get; set; }
};

public sealed record ItemDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
};
