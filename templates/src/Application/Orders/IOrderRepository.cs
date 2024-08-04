using Application.Common.Repositories;
using Domain.Orders;

namespace Application.Orders;
public interface IOrderRepository : IBaseRepository<Order>
{
}
