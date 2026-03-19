using Domain.Common;

namespace Domain.Orders;

public sealed class Order : DomainEntity
{
    public Order() { }

    public Order(
        string description,
        ICollection<Item> items,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        using var activity = ActivitySource.StartActivity($"{EntityName}.Constructor");

        Description = description;
        Items = items;

        activity?.SetTag(nameof(Description), Description);
        activity?.SetTag(nameof(Items), Items.Count);
    }

    public string Description { get; private set; }
    public decimal Total { get; private set; }
    public ICollection<Item> Items { get; private set; }

    public Result SetTotal(string user = "System", string? timezoneId = null)
    {
        using var activity = ActivitySource.StartActivity($"{EntityName}.{nameof(SetTotal)}");

        if (Items == null || Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        Total = Items.Sum(item => item.Value);
        Update(user, timezoneId);

        activity?.SetTag(nameof(Total), Total);

        return Result.Ok();
    }
}
