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
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public virtual Result Update(string? user = null, string? timezoneId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
        UpdatedByTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(timezoneId) ? TimeZoneInfo.Utc.Id : timezoneId).Id;

        return Result.Ok();
    }

    public virtual Result Delete(string? user = null, string? timezoneId = null)
    {
        if (IsDeleted)
            return Result.Fail("Entity is already deleted.");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = user ?? "System";

        var updateResult = Update(user, timezoneId);
        if (updateResult.IsFailure)
            return updateResult;

        return Result.Ok();
    }
}
