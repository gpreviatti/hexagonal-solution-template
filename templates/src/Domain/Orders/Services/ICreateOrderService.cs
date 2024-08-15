using Domain.Common;

namespace Domain.Orders.Services;
public interface ICreateOrderService
{
    Result<Order> Handle(string description, ICollection<Item> items);
}