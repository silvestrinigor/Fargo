using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticlePackConfiguration : IEntityTypeConfiguration<ArticlePack>
{
    public void Configure(EntityTypeBuilder<ArticlePack> builder)
    {
        builder.ToTable("article_packs");

        builder.Property(x => x.FromArticleGuid).IsRequired();

        builder.HasOne(x => x.FromArticle)
            .WithMany()
            .HasForeignKey(x => x.FromArticleGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Quantity)
            .IsRequired();
    }
}
