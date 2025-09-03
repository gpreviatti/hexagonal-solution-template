namespace Domain.Common;

public abstract class DomainEntity
{
    public DomainEntity() { }
    protected DomainEntity(DateTime? currentDate)
    {
        CreatedAt = currentDate;
        UpdatedAt = currentDate;
    }

    public int Id { get; set; } = 0;
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
