namespace Application.Orders;
public sealed record OrderDto(int Id, string Description, decimal Total, IList<ItemDto> Items = null);