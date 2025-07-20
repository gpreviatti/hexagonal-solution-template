using Domain.Common;

namespace Domain.Orders;

public sealed class Order : DomainEntity
{
    public Order() { }

    public Result Create(
        string description,
        ICollection<Item> items = null
    )
    {
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Items = items ?? [];

        if (Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        SetTotal();

        return Result.Ok();
    }

    public string Description { get; set; }
    public decimal Total { get; set; }
    public ICollection<Item> Items { get; set; } = [];

    private void SetTotal()
    {
        UpdatedAt = DateTime.UtcNow;
        if (Items == null || Items.Count == 0)
        {
            Total = 0;
            return;
        }

        Total = Items.Sum(item => item.Value);
    }
}
