using Fargo.Core.Articles;
using Fargo.Core.Shared.Barcodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Ean13)
            .HasConversion(new ValueConverter<Ean13?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Ean13.FromStorage(v) : null))
            .HasMaxLength(Ean13.CodeLength);

        builder.Property(x => x.Ean8)
            .HasConversion(new ValueConverter<Ean8?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Ean8.FromStorage(v) : null))
            .HasMaxLength(Ean8.CodeLength);

        builder.Property(x => x.UpcA)
            .HasConversion(new ValueConverter<UpcA?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? UpcA.FromStorage(v) : null))
            .HasMaxLength(UpcA.CodeLength);

        builder.Property(x => x.UpcE)
            .HasConversion(new ValueConverter<UpcE?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? UpcE.FromStorage(v) : null))
            .HasMaxLength(UpcE.CodeLength);

        builder.Property(x => x.Code128)
            .HasConversion(new ValueConverter<Code128?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Code128.FromStorage(v) : null))
            .HasMaxLength(Code128.MaxLength);

        builder.Property(x => x.Code39)
            .HasConversion(new ValueConverter<Code39?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Code39.FromStorage(v) : null))
            .HasMaxLength(Code39.MaxLength);

        builder.Property(x => x.Itf14)
            .HasConversion(new ValueConverter<Itf14?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Itf14.FromStorage(v) : null))
            .HasMaxLength(Itf14.CodeLength);

        builder.Property(x => x.Gs1128)
            .HasConversion(new ValueConverter<Gs1128?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? Gs1128.FromStorage(v) : null))
            .HasMaxLength(Gs1128.MaxLength);

        builder.Property(x => x.QrCode)
            .HasConversion(new ValueConverter<QrCode?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? QrCode.FromStorage(v) : null))
            .HasMaxLength(QrCode.MaxLength);

        builder.Property(x => x.DataMatrix)
            .HasConversion(new ValueConverter<DataMatrix?, string?>(
                v => v.HasValue ? v.Value.Code : null,
                v => v != null ? DataMatrix.FromStorage(v) : null))
            .HasMaxLength(DataMatrix.MaxLength);

        builder.HasIndex(x => x.Ean13).IsUnique().HasFilter("ean13 IS NOT NULL");

        builder.HasIndex(x => x.Ean8).IsUnique().HasFilter("ean8 IS NOT NULL");

        builder.HasIndex(x => x.UpcA).IsUnique().HasFilter("upc_a IS NOT NULL");

        builder.HasIndex(x => x.UpcE).IsUnique().HasFilter("upc_e IS NOT NULL");

        builder.HasIndex(x => x.Code128).IsUnique().HasFilter("code128 IS NOT NULL");

        builder.HasIndex(x => x.Code39).IsUnique().HasFilter("code39 IS NOT NULL");

        builder.HasIndex(x => x.Itf14).IsUnique().HasFilter("itf14 IS NOT NULL");

        builder.HasIndex(x => x.Gs1128).IsUnique().HasFilter("gs1128 IS NOT NULL");

        builder.HasIndex(x => x.QrCode).IsUnique().HasFilter("qr_code IS NOT NULL");

        builder.HasIndex(x => x.DataMatrix).IsUnique().HasFilter("data_matrix IS NOT NULL");

        builder.HasMany(a => a.Partitions).WithMany().UsingEntity(j =>
        {
            j.ToTable("article_partitions");
        });
    }
}
