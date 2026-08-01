using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleContainerConfiguration : IEntityTypeConfiguration<ArticleContainer>
{
    public void Configure(EntityTypeBuilder<ArticleContainer> builder)
    {
        builder.ToTable("article_containers");

        builder.HasOne<Article>().WithOne(a => a.Container).HasForeignKey<Article>("article_guid");

        builder.HasKey(c => c.Guid);
    }
}
