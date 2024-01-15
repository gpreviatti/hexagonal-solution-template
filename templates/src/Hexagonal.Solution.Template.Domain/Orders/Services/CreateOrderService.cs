﻿using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders.Services;
public sealed class CreateOrderService : ICreateOrderService
{
    public Result<Order> Handle(string description, ICollection<Item> items)
    {
        var newOrder = new Order(default, description, DateTime.UtcNow, DateTime.UtcNow)
        {
            Items = items
        };

        newOrder.SetTotal();

        return Result.Ok(newOrder);
    }
}
