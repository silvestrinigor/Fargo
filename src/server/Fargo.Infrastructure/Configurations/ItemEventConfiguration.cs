using Fargo.Core.Items.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ItemEventConfiguration : IEntityTypeConfiguration<ItemEvent>
{
    public void Configure(EntityTypeBuilder<ItemEvent> builder)
    {
        builder.ToTable("item_events");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.ArticleGuid);

        builder.HasIndex(x => x.OccuredAt);

        builder.HasIndex(x => new { x.ArticleGuid, x.OccuredAt });

        builder
        .HasOne(x => x.Article)
        .WithMany()
        .HasForeignKey(x => x.ArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
