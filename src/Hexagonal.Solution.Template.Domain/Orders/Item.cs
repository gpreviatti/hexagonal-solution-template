using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders;
public class Item : DomainEntity
{
    public Item(
        int id, string name, string description, decimal value, 
        DateTime? createdAt, DateTime? updatedAt = null
    ) : base(id, createdAt, updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Value = value;
    }

    public Item(
        int id, string name, string description, decimal value
    ) : base(id, DateTime.UtcNow, DateTime.UtcNow)
    {
        Id = id;
        Name = name;
        Description = description;
        Value = value;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}
