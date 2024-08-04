using Domain.Common;

namespace Domain.Orders;
public sealed class Item : DomainEntity
{
    public Item() { }

    public Item(
        int id, string name, string description, decimal value,
        DateTime? createdAt, DateTime? updatedAt = null
    )
    {
        Id = id;
        Name = name;
        Description = description;
        Value = value;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Item(
        int id, string name, string description, decimal value
    )
    {
        Id = id;
        Name = name;
        Description = description;
        Value = value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}
