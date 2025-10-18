using Domain.Common;

namespace Domain.Orders;
public sealed class Item(string name, string description, decimal value) : DomainEntity
{
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public decimal Value { get; private set; } = value;
}
