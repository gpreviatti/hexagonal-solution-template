using Domain.Common;

namespace Domain.Orders;

public sealed class Order : DomainEntity
{
    public Order() {}

    public Order(string description, ICollection<Item> items) : base(DateTime.UtcNow)
    {
        Description = description;
        Items = items;
    }

    public string Description { get; private set; }
    public decimal Total { get; private set; }
    public ICollection<Item> Items { get; private set; } = [];

    public Result SetTotal()
    {
        if (Items == null || Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        Total = Items.Sum(item => item.Value);
        SetUpdatedAt();

        return Result.Ok();
    }
}
