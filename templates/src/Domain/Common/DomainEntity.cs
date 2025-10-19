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

    public int Id { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    public virtual void Update(string? user = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user ?? "System";
    }
}
