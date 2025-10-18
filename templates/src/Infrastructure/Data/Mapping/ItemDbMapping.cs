using Domain.Orders;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Mapping;

internal sealed class ItemDbMapping : BaseDbMapping<Item>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Item> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Value)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired();
    }
}
