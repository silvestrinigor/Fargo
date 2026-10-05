using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleVariationConfiguration : IEntityTypeConfiguration<ArticleVariation>
{
    public void Configure(EntityTypeBuilder<ArticleVariation> builder)
    {
        builder.ToTable("article_variations");

        builder.HasKey(v => v.VariationArticleGuid);

        builder
        .HasOne(v => v.VariationArticle)
        .WithOne(a => a.Variation)
        .HasForeignKey<ArticleVariation>(v => v.VariationArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(a => a.FromArticle)
        .WithMany()
        .HasForeignKey(a => a.FromArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
