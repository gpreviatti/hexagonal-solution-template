using Hexagonal.Solution.Template.Application.Common.Repositories;
using Hexagonal.Solution.Template.Domain.Orders;

namespace Hexagonal.Solution.Template.Application.Orders;
public interface IOrderRepository : IBaseRepository<Order>
{
}
