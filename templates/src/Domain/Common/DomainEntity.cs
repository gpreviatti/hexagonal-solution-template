namespace Domain.Common;

public abstract class DomainEntity(DateTime? currentDate = null, string? user = null)
{
    public int Id { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; } = currentDate ?? DateTime.UtcNow;
    public string? CreatedBy { get; private set; } = user ?? "System";
    public DateTime UpdatedAt { get; private set; } = currentDate ?? DateTime.UtcNow;
    public string? UpdatedBy { get; private set; } = user ?? "System";
    public void SetUpdated(string? user = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }
}
