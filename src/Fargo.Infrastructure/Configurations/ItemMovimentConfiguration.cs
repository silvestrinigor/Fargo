using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemMovimentConfiguration : IEntityTypeConfiguration<ItemMoviment>
{
    public void Configure(EntityTypeBuilder<ItemMoviment> builder)
    {
        builder.ToTable("item_moviments");

        builder
        .HasOne(m => m.ItemMoved)
        .WithMany()
        .HasForeignKey(m => m.ItemMovedGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(m => m.ItemContainerPosition)
        .WithMany()
        .HasForeignKey(m => m.ItemContainerPositionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
