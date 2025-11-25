namespace Application.Orders;
public sealed record OrderDto(int Id, string Description, decimal Total, DateTime CreatedAt, IReadOnlyCollection<ItemDto>? Items = null)
{
    public static implicit operator OrderDto(Domain.Orders.Order order) => new(
        order.Id, order.Description, order.Total, order.CreatedAt,
        order.Items?.Select(item => (ItemDto) item).ToArray()
    );
};

public sealed record ItemDto(int Id, string Name, string Description, decimal Value)
{
    public static implicit operator ItemDto(Domain.Orders.Item item) =>
        new(item.Id, item.Name, item.Description, item.Value);
};
