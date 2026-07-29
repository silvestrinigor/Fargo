using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticlePackConfiguration : IEntityTypeConfiguration<ArticlePack>
{
    public void Configure(EntityTypeBuilder<ArticlePack> builder)
    {
        builder.ToTable("article_packs");

        builder.HasKey(p => p.Guid);

        builder.HasOne(a => a.FromArticle).WithMany().HasForeignKey(a => a.FromArticleGuid);
    }
}
