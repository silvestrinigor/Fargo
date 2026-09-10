using Fargo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemParentContainerHistoryPostgresTemporalConfiguration : IEntityTypeConfiguration<ItemParentContainerHistoryPostgresTemporal>
{
    public void Configure(EntityTypeBuilder<ItemParentContainerHistoryPostgresTemporal> builder)
    {
        builder.ToTable("item_parent_container_history");

        builder.HasKey(i => new { i.ItemGuid, i.ValidPeriod });

        builder
        .Property(x => x.ValidPeriod)
        .HasDefaultValueSql("tstzrange(now(), 'infinity', '[)')");
    }
}
