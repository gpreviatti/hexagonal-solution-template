using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders.Services;
public interface ICreateOrderService
{
    Result<Order> Handle(string description, IEnumerable<Item> items);
}
