using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders.Services;
public class CreateOrderService : ICreateOrderService
{
    public Result<Order> Handle(string description, IEnumerable<Item> items)
    {
        var newOrder = new Order(default, description, DateTime.UtcNow, items, DateTime.UtcNow);

        newOrder.SetTotal();

        return Result.Ok(newOrder);
    }
}
