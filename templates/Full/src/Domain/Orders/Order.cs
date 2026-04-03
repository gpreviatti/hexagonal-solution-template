using Domain.Common;

namespace Domain.Orders;

public sealed class Order : DomainEntity
{
    public Order() { }

    private Order(
        string description,
        ICollection<Item> items,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        Description = description;
        Items = items;
    }

    public string Description { get; private set; }
    public decimal Total { get; private set; }
    public ICollection<Item> Items { get; private set; }

    public static Result<Order> Create(
        string description,
        ICollection<Item> items,
        string user = "System",
        string? timezoneId = null
    ) => Handle(activity =>
    {
        Order order = new(description, items, user, timezoneId);

        var setTotalResult = order.SetTotal(user, timezoneId);
        if (setTotalResult.IsFailure)
            return Result.Fail<Order>(setTotalResult.Message);

        activity?.SetTag(nameof(description), description);

        return Result.Ok(order);
    });

    public Result SetTotal(string user = "System", string? timezoneId = null) => Handle(activity =>
    {
        if (Items == null || Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        Total = Items.Sum(item => item.Value);

        var result = Update(user, timezoneId);
        if (result.IsFailure)
            return Result.Fail(result.Message);

        activity?.SetTag(nameof(Total), Total);

        return Result.Ok();
    });
}
