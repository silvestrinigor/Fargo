using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleDimensionConfiguration : IEntityTypeConfiguration<ArticleDimension>
{
    public void Configure(EntityTypeBuilder<ArticleDimension> builder)
    {
        builder.ToTable("article_dimensions");

        builder.HasKey(x => x.ArticleGuid);

        builder.HasOne(d => d.Article).WithOne(a => a.Dimension).HasForeignKey<ArticleDimension>(d => d.ArticleGuid);
    }
}
