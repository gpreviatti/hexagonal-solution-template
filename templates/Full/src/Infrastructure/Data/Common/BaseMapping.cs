using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Common;

internal abstract class BaseDbMapping<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : DomainEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {

        builder.HasKey(p => p.Id);

        ConfigureDomainEntity(builder);

        builder.Property(p => p.CreatedAt)
            .IsRequired(true);

        builder.Property(p => p.UpdatedAt)
            .IsRequired(true);

        builder.Property(p => p.DeletedAt)
            .IsRequired(false);

        builder.Property(p => p.IsDeleted)
            .IsRequired(true)
            .HasDefaultValue(false);

        builder
            .HasQueryFilter(p => !p.IsDeleted);
    }

    public abstract void ConfigureDomainEntity(EntityTypeBuilder<TEntity> builder);
}
