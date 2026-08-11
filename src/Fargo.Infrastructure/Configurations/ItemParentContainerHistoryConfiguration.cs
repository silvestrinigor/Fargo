using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemParentContainerHistoryConfiguration : IEntityTypeConfiguration<ItemParentContainerHistory>
{
    public void Configure(EntityTypeBuilder<ItemParentContainerHistory> builder)
    {
        builder.ToTable("item_moviments");

        builder.HasKey(i => i.Guid);

        builder.HasAlternateKey(i => new { i.ItemGuid, i.ValidAt });

        builder
        .HasOne(m => m.Item)
        .WithMany()
        .HasForeignKey(m => m.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(m => m.ParentItemContainer)
        .WithMany()
        .HasForeignKey(m => m.ParentItemContianerGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
