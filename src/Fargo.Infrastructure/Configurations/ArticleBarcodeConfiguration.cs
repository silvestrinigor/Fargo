using Fargo.Core.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleBarcodeConfiguration : IEntityTypeConfiguration<ArticleBarcode>
{
    public void Configure(EntityTypeBuilder<ArticleBarcode> builder)
    {
        builder.ToTable("article_barcodes");

        builder.HasKey(x => x.ArticleGuid);

        builder
        .HasOne(b => b.Article)
        .WithOne(a => a.Barcode)
        .HasForeignKey<ArticleBarcode>(b => b.ArticleGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Ean13).IsUnique().HasFilter("ean13 IS NOT NULL");
    }
}
