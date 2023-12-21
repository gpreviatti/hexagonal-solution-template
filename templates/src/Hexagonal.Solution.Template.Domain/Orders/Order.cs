using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders;
public class Order : DomainEntity
{
    public Order(
        int id, string description, DateTime createdAt, DateTime? updatedAt = null
    ) : base(id, createdAt, updatedAt)
    {
        Id = id;
        Description = description;
    }

    public Order(
        int id, string description, DateTime createdAt, IEnumerable<Item> items, DateTime? updatedAt = null
    ) : base(id, createdAt, updatedAt)
    {
        Id = id;
        Description = description;
        CreatedAt = createdAt;
        Items = items;
        UpdatedAt = updatedAt;
    }

    public string Description { get; set; }
    public decimal Total { get; set; }
    public IEnumerable<Item> Items { get; set; } = new List<Item>();

    public void SetTotal()
    {
        UpdatedAt = DateTime.UtcNow;
        Total = Items.Sum(item => item.Value);
    }
}
