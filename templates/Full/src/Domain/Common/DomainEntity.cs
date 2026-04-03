using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Domain.Common;

public abstract class DomainEntity
{
    protected virtual string EntityName { get; }
    protected DomainEntity()
    {
        EntityName = GetType().Name;
    }
    protected DomainEntity(string user, string? timezoneId = null)
    {
        EntityName = GetType().Name;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = user;
        CreatedByTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(timezoneId) ? TimeZoneInfo.Utc.Id : timezoneId).Id;
        UpdatedAt = CreatedAt;
        UpdatedBy = CreatedBy;
        UpdatedByTimezoneId = CreatedByTimezoneId;
    }
    protected static ActivitySource ActivitySource { get; } = DefaultConfigurations.ActivitySource;
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string CreatedByTimezoneId { get; init; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public string? UpdatedByTimezoneId { get; private set; }

    protected static Result<TEntity> Handle<TEntity>(
        Func<Activity?, Result<TEntity>> factory,
        [CallerMemberName] string callerName = null!
    ) where TEntity : DomainEntity
    {
        using var activity = ActivitySource.StartActivity($"{typeof(TEntity).Name}.{callerName}");

        return factory(activity);
    }

    protected static Result Handle(
        Func<Activity?, Result> factory,
        [CallerMemberName] string callerName = null!
    )
    {
        using var activity = ActivitySource.StartActivity(callerName);

        return factory(activity);
    }

    public Result Update(string? user = null, string? timezoneId = null) => Handle(activity =>
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
        UpdatedByTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(timezoneId) ? TimeZoneInfo.Utc.Id : timezoneId).Id;

        activity?.SetTag(nameof(UpdatedBy), UpdatedBy);
        activity?.SetTag(nameof(UpdatedByTimezoneId), UpdatedByTimezoneId);

        return Result.Ok();
    });
}
