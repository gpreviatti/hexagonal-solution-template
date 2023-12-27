namespace Hexagonal.Solution.Template.Application.Orders;
public record OrderDto(int Id, string Description, decimal Total, IList<ItemDto> Items = null);
