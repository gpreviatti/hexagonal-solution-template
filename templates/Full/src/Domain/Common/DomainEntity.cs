namespace Domain.Common;

public abstract class DomainEntity
{
    protected DomainEntity() {}
    protected DomainEntity(string? user = null, string timezoneId = "")
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = user ?? "System";
        UpdatedAt = CreatedAt;
        UpdatedBy = CreatedBy;
        SetTimezoneId(timezoneId);
    }

    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public string TimezoneId { get; private set; }

    private void SetTimezoneId(string timezoneId)
    {
        if (string.IsNullOrWhiteSpace(timezoneId))
        {
            TimezoneId = TimeZoneInfo.Utc.Id;
            return;
        }

        TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        TimezoneId = timezoneId;
    }

    public virtual void Update(string? user = null, string timezoneId = "")
    {
        UpdatedAt = DateTime.UtcNow;
        SetTimezoneId(timezoneId);
        UpdatedBy = user ?? "System";
    }
}
