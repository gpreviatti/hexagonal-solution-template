using Domain.Notifications;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Mapping;

internal sealed class NotificationDbMapping : BaseDbMapping<Notification>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(p => p.NotificationType)
            .IsRequired();

        builder.Property(p => p.NotificationStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Message)
            .HasMaxLength(4000)
            .IsRequired(false);
    }
}
