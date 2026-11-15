using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class ItemEventConfiguration : IEntityTypeConfiguration<ItemEvent>
{
    public void Configure(EntityTypeBuilder<ItemEvent> builder)
    {
        builder.ToTable("item_events");

        builder.HasIndex(x => x.ArticleGuid);

        // Not a foreign key: the events of an item must outlive it.
        builder.HasIndex(x => x.ItemGuid);

        builder
        .HasOne(x => x.Article)
        .WithMany()
        .HasForeignKey(x => x.ArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);

        builder
        .HasOne(x => x.FromItemContainer)
        .WithMany()
        .HasForeignKey(x => x.FromItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);

        builder
        .HasOne(x => x.ToItemContainer)
        .WithMany()
        .HasForeignKey(x => x.ToItemContainerGuid)
        .OnDelete(DeleteBehavior.SetNull);
    }
}
