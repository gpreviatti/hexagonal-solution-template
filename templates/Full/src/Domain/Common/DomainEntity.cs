namespace Domain.Common;

public abstract class DomainEntity
{
    protected DomainEntity() {}
    protected DomainEntity(DateTime currentDate, string? user = null)
    {
        CreatedAt = currentDate;
        CreatedBy = user ?? "System";
        UpdatedAt = currentDate;
        UpdatedBy = user ?? "System";
    }

    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    public virtual void Update(string? user = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }
}
