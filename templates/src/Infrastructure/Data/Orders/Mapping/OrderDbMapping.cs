using Domain.Orders;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Orders.Mapping;
internal sealed class OrderDbMapping : BaseDbMapping<Order>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Order> builder)
    {
        builder.Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Property(p => p.Total)
            .IsRequired(true)
            .HasPrecision(18, 2);

        builder.HasMany(p => p.Items);

        //builder.Navigation(p => p.Items).AutoInclude(false);
    }
}
