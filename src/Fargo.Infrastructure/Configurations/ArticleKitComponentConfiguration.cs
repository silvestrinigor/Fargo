using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleKitComponentConfiguration : IEntityTypeConfiguration<ArticleKitComponent>
{
    public void Configure(EntityTypeBuilder<ArticleKitComponent> builder)
    {
        builder.ToTable("article_kit_components");

        builder.HasKey(c => c.KitArticleGuid);

        builder
        .HasOne(c => c.KitArticle)
        .WithMany(k => k.KitComponents)
        .HasForeignKey(c => c.KitArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(c => c.FromArticle)
        .WithMany()
        .HasForeignKey(c => c.FromArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
