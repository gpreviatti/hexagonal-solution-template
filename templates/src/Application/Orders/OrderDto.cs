namespace Application.Orders;
public sealed record OrderDto(int Id, string Description, decimal Total, DateTime CreatedAt, IList<ItemDto> Items = null);

public sealed record ItemDto(int Id, string Name, string Description, decimal Value);
