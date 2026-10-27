using Fargo.Core.Items.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ItemLifecycleEventConfiguration : IEntityTypeConfiguration<ItemLifecycleEvent>
{
    public void Configure(EntityTypeBuilder<ItemLifecycleEvent> builder)
    {
        builder.ToTable("item_lifecycle_events");

        builder.HasKey(i => new { i.ItemEventGuid });

        builder.Property(x => x.Quantity);

        builder
        .HasOne<ItemEvent>()
        .WithOne(x => x.Lifecycle)
        .HasForeignKey<ItemLifecycleEvent>(x => x.ItemEventGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.ItemContainer)
        .WithMany()
        .HasForeignKey(m => m.ItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
