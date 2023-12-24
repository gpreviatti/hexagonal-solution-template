using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Domain.Orders;
public class Order : DomainEntity
{
    public Order()
    {
        
    }
    public Order(
        int id, string description, 
        DateTime createdAt, DateTime? updatedAt = null
    )
    {
        Id = id;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string Description { get; set; }
    public decimal Total { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();

    public void SetTotal()
    {
        UpdatedAt = DateTime.UtcNow;
        Total = Items.Sum(item => item.Value);
    }
}
