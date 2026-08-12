using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemMovimentConfiguration : IEntityTypeConfiguration<ItemMoviment>
{
    public void Configure(EntityTypeBuilder<ItemMoviment> builder)
    {
        builder.ToTable("item_moviments");

        builder.HasKey(i => i.Guid);

        builder
        .HasOne<Item>()
        .WithMany()
        .HasForeignKey(m => m.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne<Item>()
        .WithMany()
        .HasForeignKey(m => m.MovedToContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
