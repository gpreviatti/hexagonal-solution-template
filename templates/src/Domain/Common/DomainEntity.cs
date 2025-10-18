namespace Domain.Common;

public abstract class DomainEntity
{
    public DomainEntity() { }
    protected DomainEntity(DateTime? currentDate, string? user = null)
    {
        CreatedAt = currentDate ?? DateTime.UtcNow;
        CreatedBy = user ?? "System";
        UpdatedAt = currentDate ?? DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }

    public int Id { get; set; } = 0;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; set; }
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
