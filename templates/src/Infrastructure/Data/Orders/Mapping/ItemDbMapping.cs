using Domain.Orders;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Orders.Mapping;
internal sealed class ItemDbMapping : BaseDbMapping<Item>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Item> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired(true);

        builder.Property(p => p.Value)
            .IsRequired(true)
            .HasPrecision(18, 2);

        builder.Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired(true);
    }
}
