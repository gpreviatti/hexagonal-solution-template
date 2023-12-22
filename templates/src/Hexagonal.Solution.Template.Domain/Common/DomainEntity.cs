namespace Hexagonal.Solution.Template.Domain.Common;
public abstract class DomainEntity
{
    protected DomainEntity(int id, DateTime? createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public int Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
