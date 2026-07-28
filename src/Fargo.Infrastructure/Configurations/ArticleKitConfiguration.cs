using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleKitConfiguration : IEntityTypeConfiguration<ArticleKit>
{
    public void Configure(EntityTypeBuilder<ArticleKit> builder)
    {
        builder.ToTable("article_kits");

        builder.OwnsMany(x => x.Components, component =>
        {
            component.ToTable("article_kit_components");

            component.WithOwner().HasForeignKey();

            component.Property(x => x.ArticleGuid)
                .IsRequired();

            component.HasOne(x => x.Article)
                .WithMany()
                .HasForeignKey(x => x.ArticleGuid)
                .OnDelete(DeleteBehavior.Restrict);

            component.Property(x => x.Quantity)
                .IsRequired();
        });
    }
}
