using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleContainerConfiguration : IEntityTypeConfiguration<ArticleContainer>
{
    public void Configure(EntityTypeBuilder<ArticleContainer> builder)
    {
        builder.ToTable("article_containers");
    }
}
