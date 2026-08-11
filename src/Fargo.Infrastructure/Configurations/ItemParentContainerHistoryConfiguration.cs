using Fargo.Core.Items;
using Fargo.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemParentContainerHistoryConfiguration : IEntityTypeConfiguration<ItemParentContainerHistory>
{
    public void Configure(EntityTypeBuilder<ItemParentContainerHistory> builder)
    {
        builder.ToTable("item_parent_containers_history");

        builder.HasKey(i => i.Guid);

        builder.HasAlternateKey(i => new { i.ItemGuid, i.ValidAt });

        // For some reason, I need configure the conversion here even with the configuration in the db context conversions.
        builder
        .Property(i => i.ValidAt)
        .HasConversion<DateTimeOffsetRangeNpgsqlRangeConverter>()
        .HasColumnType("tstzrange");

        builder
        .HasOne(m => m.Item)
        .WithMany(i => i.ParentItemContainerHistory)
        .HasForeignKey(m => m.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(m => m.ParentItemContainer)
        .WithMany()
        .HasForeignKey(m => m.ParentItemContianerGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
