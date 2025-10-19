using Domain.Common;

namespace Domain.Orders;
public sealed class Item : DomainEntity
{
    public Item() { }

    public Item(string name, string description, decimal value) : base(DateTime.UtcNow)
    {
        Name = name;
        Description = description;
        Value = value;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Value { get; private set; }
}
