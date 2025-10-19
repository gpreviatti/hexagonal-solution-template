namespace Domain.Common;

public abstract class DomainEntity
{
    public DomainEntity() {}
    public DomainEntity(string? user = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = user ?? "System";
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }

    public int Id { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public void Update(string? user = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }
}
