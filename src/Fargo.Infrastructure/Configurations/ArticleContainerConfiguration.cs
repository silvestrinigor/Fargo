using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleContainerConfiguration : IEntityTypeConfiguration<ArticleContainer>
{
    public void Configure(EntityTypeBuilder<ArticleContainer> builder)
    {
        builder.ToTable("article_containers");

        builder.HasKey(c => c.ArticleGuid);

        builder
        .HasOne(c => c.Article)
        .WithOne(a => a.Container)
        .HasForeignKey<ArticleContainer>(c => c.ArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
