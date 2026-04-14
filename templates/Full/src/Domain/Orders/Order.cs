using Domain.Common;
using Domain.Common.Extensions;

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

        var setTotalResult = order.SetTotal();
        if (setTotalResult.IsFailure)
            return Result.Fail<Order>(setTotalResult.Message);

        activity?.SetTag(nameof(description), description);

        return Result.Ok(order);
    });

    private Result SetTotal() => Handle(activity =>
    {
        if (Items == null || Items.Count == 0)
            return Result.Fail("Order must have at least one item.");

        Total = Items.Sum(item => item.Value);

        activity?.SetTag(nameof(Total), Total);

        return Result.Ok();
    });

    public Result UpdateOrder(
        string description,
        ICollection<Item> items,
        string user = "System",
        string? timezoneId = null
    ) => Handle(activity =>
    {
        if (IsDeleted)
            return Result.Fail("Cannot update a deleted order.");

        Description = description;
        Items = items;

        var setTotalResult = SetTotal();
        if (setTotalResult.IsFailure)
            return setTotalResult;

        var updateResult = Update(user, timezoneId);
        if (updateResult.IsFailure)
            return updateResult;

        activity?.SetTag(nameof(description), description);

        return Result.Ok();
    });

    public string GetPeriodSinceWasCreated()
    {
        using var activity = ActivitySource.StartActivity($"{EntityName}.{nameof(GetPeriodSinceWasCreated)}");
        activity.SetDefaultTags();

        if (CreatedAt == default)
            return "CreatedAt was not set.";

        var timeSinceCreation = DateTime.UtcNow - CreatedAt;
        activity?.SetTag(nameof(timeSinceCreation), timeSinceCreation.ToString());

        if (timeSinceCreation.TotalSeconds < 60)
            return $"{(int)timeSinceCreation.TotalSeconds} seconds ago";
        if (timeSinceCreation.TotalMinutes < 60)
            return $"{(int)timeSinceCreation.TotalMinutes} minutes ago";
        if (timeSinceCreation.TotalHours < 24)
            return $"{(int) timeSinceCreation.TotalHours} hours ago";
        if (timeSinceCreation.TotalDays < 30)
            return $"{(int) timeSinceCreation.TotalDays} days ago";
        if (timeSinceCreation.TotalDays < 365)
            return $"{(int) (timeSinceCreation.TotalDays / 30)} months ago";

        return $"{(int)(timeSinceCreation.TotalDays / 365)} years ago";
    }
}
