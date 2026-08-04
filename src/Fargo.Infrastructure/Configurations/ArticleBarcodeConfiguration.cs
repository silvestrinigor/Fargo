using Fargo.Core.Articles;
using Fargo.Core.Shared.Barcodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        builder.Property(x => x.Ean13)
            .HasConversion(new ValueConverter<Ean13?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? new Ean13(v) : null))
            .HasMaxLength(Ean13.CodeLength);

        builder.HasIndex(x => x.Ean13).IsUnique().HasFilter("ean13 IS NOT NULL");
    }
}
