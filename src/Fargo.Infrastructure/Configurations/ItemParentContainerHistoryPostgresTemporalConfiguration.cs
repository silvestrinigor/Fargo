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
        // TODO: Configure postgres temporal table when efcore 11 available (without overlaps).

        builder
        .Property(x => x.ValidPeriod)
        .HasDefaultValueSql("tstzrange(now(), 'infinity', '[)')");

        builder
        .HasOne(x => x.Item)
        .WithMany()
        .HasForeignKey(x => x.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.ParentItemContainer)
        .WithMany()
        .HasForeignKey(x => x.ParentItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
