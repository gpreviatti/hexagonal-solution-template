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
    )
    {
        Order order = new(description, items, user, timezoneId);

        var result = order.SetTotal();
        if (result.IsFailure)
            return Result.Fail<Order>(result.Message);

        return Result.Ok(order);
    }

    private Result SetTotal()
    {
        if (Items == null || Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        Total = Items.Sum(item => item.Value);

        return Result.Ok();
    }

    public Result Update(
        string description,
        ICollection<Item> items,
        string user = "System",
        string? timezoneId = null
    )
    {
        if (IsDeleted)
            return Result.Fail("Cannot update a deleted order.");

        Description = description;
        Items = items;

        var result = SetTotal();
        if (result.IsFailure)
            return result;

        result = Update(user, timezoneId);
        if (result.IsFailure)
            return result;

        return Result.Ok();
    }

    public override Result Delete(string? user = null, string? timezoneId = null)
    {
        var result = base.Delete(user, timezoneId);
        if (result.IsFailure)
            return result;

        foreach (var item in Items)
        {
            result = item.Delete(user, timezoneId);
            if (result.IsFailure)
                return Result.Fail($"Failed to delete item '{item.Name}': {result.Message}");
        }

        return Result.Ok();
    }

    public string GetPeriodSinceWasCreated()
    {
        if (CreatedAt == default)
            return "CreatedAt was not set.";

        var timeSinceCreation = DateTime.UtcNow - CreatedAt;

        if (timeSinceCreation.TotalSeconds < 60)
            return $"{(int) timeSinceCreation.TotalSeconds} seconds ago";
        if (timeSinceCreation.TotalMinutes < 60)
            return $"{(int) timeSinceCreation.TotalMinutes} minutes ago";
        if (timeSinceCreation.TotalHours < 24)
            return $"{(int) timeSinceCreation.TotalHours} hours ago";
        if (timeSinceCreation.TotalDays < 30)
            return $"{(int) timeSinceCreation.TotalDays} days ago";
        if (timeSinceCreation.TotalDays < 365)
            return $"{(int) (timeSinceCreation.TotalDays / 30)} months ago";

        return $"{(int) (timeSinceCreation.TotalDays / 365)} years ago";
    }
}
