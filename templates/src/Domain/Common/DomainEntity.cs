namespace Domain.Common;
public abstract class DomainEntity
{
    public int Id { get; set; } = 0;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
