using Domain.Common;

namespace Domain.Orders;
public sealed class Item : DomainEntity
{
    public Item() { }

    public Item(
        string name,
        string description,
        decimal value,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        Name = name;
        Description = description;

        if (value <= 0)
            throw new DomainException("Item value cannot be zero or negative.");

        Value = value;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Value { get; private set; }
}
