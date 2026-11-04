using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ItemEventConfiguration : IEntityTypeConfiguration<ItemEvent>
{
    public void Configure(EntityTypeBuilder<ItemEvent> builder)
    {
        builder.ToTable("item_events");

        builder.HasIndex(x => x.ArticleGuid);

        builder
        .HasOne(x => x.Article)
        .WithMany()
        .HasForeignKey(x => x.ArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
