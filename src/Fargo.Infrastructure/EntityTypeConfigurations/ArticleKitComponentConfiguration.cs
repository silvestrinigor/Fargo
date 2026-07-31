using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleKitComponentConfiguration : IEntityTypeConfiguration<ArticleKitComponent>
{
    public void Configure(EntityTypeBuilder<ArticleKitComponent> builder)
    {
        builder.ToTable("article_kit_components");

        builder.HasKey(c => c.Guid);

        builder.HasOne(c => c.Article).WithMany().HasForeignKey(c => c.ArticleGuid);
    }
}
