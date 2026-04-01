using System.Diagnostics;

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

    public virtual void Update(string? user = null, string? timezoneId = null)
    {
        using var activity = ActivitySource.StartActivity($"{EntityName}.{nameof(Update)}");

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
        UpdatedByTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(timezoneId) ? TimeZoneInfo.Utc.Id : timezoneId).Id;

        activity?.SetTag(nameof(UpdatedBy), UpdatedBy);
        activity?.SetTag(nameof(UpdatedByTimezoneId), UpdatedByTimezoneId);
    }
}
