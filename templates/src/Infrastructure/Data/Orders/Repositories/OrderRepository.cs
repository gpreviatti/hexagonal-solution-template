using Application.Orders;
using Domain.Orders;
using Infrastructure.Data.Common;

namespace Infrastructure.Data.Orders.Repositories;
public class OrderRepository(MyDbContext dbContext) : BaseRepository<Order>(dbContext), IOrderRepository
{
}
