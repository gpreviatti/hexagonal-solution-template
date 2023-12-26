using Hexagonal.Solution.Template.Application.Orders;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data.Common;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;
public class OrderRepository(MyDbContext dbContext) : BaseRepository<Order>(dbContext), IOrderRepository
{
}
