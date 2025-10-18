namespace Domain.Common;

public abstract class DomainEntity(DateTime? currentDate = null, string? user = null)
{
    public int Id { get; set; } = 0;
    public DateTime CreatedAt { get; private set; } = currentDate ?? DateTime.UtcNow;
    public string? CreatedBy { get; set; } = user ?? "System";
    public DateTime UpdatedAt { get; private set; } = currentDate ?? DateTime.UtcNow;
    public string? UpdatedBy { get; set; } = user ?? "System";
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
