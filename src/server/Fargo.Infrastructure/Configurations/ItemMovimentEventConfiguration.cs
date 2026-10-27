using Fargo.Core.Items.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemMovimentEventConfiguration : IEntityTypeConfiguration<ItemMovimentEvent>
{
    public void Configure(EntityTypeBuilder<ItemMovimentEvent> builder)
    {
        builder.ToTable("item_moviment_events");

        builder.HasKey(i => new { i.ItemEventGuid });

        builder.Property(x => x.Quantity);

        builder
        .HasOne<ItemEvent>()
        .WithOne(x => x.Moviment)
        .HasForeignKey<ItemMovimentEvent>(x => x.ItemEventGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.FromItemContainer)
        .WithMany()
        .HasForeignKey(m => m.FromItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);

        builder
        .HasOne(x => x.ToItemContainer)
        .WithMany()
        .HasForeignKey(m => m.ToItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
