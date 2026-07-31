using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleKitConfiguration : IEntityTypeConfiguration<ArticleKit>
{
    public void Configure(EntityTypeBuilder<ArticleKit> builder)
    {
        builder.ToTable("article_kits");

        builder.HasKey(k => k.Guid);

        builder.HasMany(k => k.Components).WithOne();
    }
}
