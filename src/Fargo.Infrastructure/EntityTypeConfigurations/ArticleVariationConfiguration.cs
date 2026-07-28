using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleVariationConfiguration : IEntityTypeConfiguration<ArticleVariation>
{
    public void Configure(EntityTypeBuilder<ArticleVariation> builder)
    {
        builder.ToTable("article_variations");

        builder.Property(x => x.FromArticleGuid).IsRequired();

        builder.HasOne(x => x.FromArticle)
            .WithMany()
            .HasForeignKey(x => x.FromArticleGuid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
