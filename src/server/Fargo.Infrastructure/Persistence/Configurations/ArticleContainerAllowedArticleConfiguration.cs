using Fargo.Core.Entities.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class ArticleContainerAllowedArticleConfiguration : IEntityTypeConfiguration<ArticleContainerAllowedArticle>
{
    public void Configure(EntityTypeBuilder<ArticleContainerAllowedArticle> builder)
    {
        builder.ToTable("article_container_allowed_articles");

        builder.HasKey(a => new { a.ContainerArticleGuid, a.AllowedArticleGuid });

        builder
        .HasOne(a => a.Container)
        .WithMany(c => c.AllowedArticles)
        .HasForeignKey(a => a.ContainerArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(a => a.AllowedArticle)
        .WithMany()
        .HasForeignKey(a => a.AllowedArticleGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
