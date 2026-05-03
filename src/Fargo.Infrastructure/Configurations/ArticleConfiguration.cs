using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="Article"/>.
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles", t => t.IsTemporal(ttb =>
        {
            ttb.UseHistoryTable("ArticlesHistory");
            ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
            ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
        }));

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.EditedByGuid);

        builder.OwnsOne(a => a.Barcodes, barcodes =>
        {
            barcodes.ToTable("Articles", t => t.IsTemporal(ttb =>
            {
                ttb.UseHistoryTable("ArticlesHistory");
                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
            }));

            barcodes.Ignore(x => x.IsEmpty);

            barcodes.Property(x => x.Ean13)
                .HasConversion(new ValueConverter<Ean13?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Ean13.FromStorage(v) : null))
                .HasMaxLength(Ean13.CodeLength);

            barcodes.Property(x => x.Ean8)
                .HasConversion(new ValueConverter<Ean8?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Ean8.FromStorage(v) : null))
                .HasMaxLength(Ean8.CodeLength);

            barcodes.Property(x => x.UpcA)
                .HasConversion(new ValueConverter<UpcA?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? UpcA.FromStorage(v) : null))
                .HasMaxLength(UpcA.CodeLength);

            barcodes.Property(x => x.UpcE)
                .HasConversion(new ValueConverter<UpcE?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? UpcE.FromStorage(v) : null))
                .HasMaxLength(UpcE.CodeLength);

            barcodes.Property(x => x.Code128)
                .HasConversion(new ValueConverter<Code128?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Code128.FromStorage(v) : null))
                .HasMaxLength(Code128.MaxLength);

            barcodes.Property(x => x.Code39)
                .HasConversion(new ValueConverter<Code39?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Code39.FromStorage(v) : null))
                .HasMaxLength(Code39.MaxLength);

            barcodes.Property(x => x.Itf14)
                .HasConversion(new ValueConverter<Itf14?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Itf14.FromStorage(v) : null))
                .HasMaxLength(Itf14.CodeLength);

            barcodes.Property(x => x.Gs1128)
                .HasConversion(new ValueConverter<Gs1128?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? Gs1128.FromStorage(v) : null))
                .HasMaxLength(Gs1128.MaxLength);

            barcodes.Property(x => x.QrCode)
                .HasConversion(new ValueConverter<QrCode?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? QrCode.FromStorage(v) : null))
                .HasMaxLength(QrCode.MaxLength);

            barcodes.Property(x => x.DataMatrix)
                .HasConversion(new ValueConverter<DataMatrix?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? DataMatrix.FromStorage(v) : null))
                .HasMaxLength(DataMatrix.MaxLength);

            barcodes.HasIndex(x => x.Ean13).IsUnique().HasFilter("[Barcodes_Ean13] IS NOT NULL");
            barcodes.HasIndex(x => x.Ean8).IsUnique().HasFilter("[Barcodes_Ean8] IS NOT NULL");
            barcodes.HasIndex(x => x.UpcA).IsUnique().HasFilter("[Barcodes_UpcA] IS NOT NULL");
            barcodes.HasIndex(x => x.UpcE).IsUnique().HasFilter("[Barcodes_UpcE] IS NOT NULL");
            barcodes.HasIndex(x => x.Code128).IsUnique().HasFilter("[Barcodes_Code128] IS NOT NULL");
            barcodes.HasIndex(x => x.Code39).IsUnique().HasFilter("[Barcodes_Code39] IS NOT NULL");
            barcodes.HasIndex(x => x.Itf14).IsUnique().HasFilter("[Barcodes_Itf14] IS NOT NULL");
            barcodes.HasIndex(x => x.Gs1128).IsUnique().HasFilter("[Barcodes_Gs1128] IS NOT NULL");
            barcodes.HasIndex(x => x.QrCode).IsUnique().HasFilter("[Barcodes_QrCode] IS NOT NULL");
            barcodes.HasIndex(x => x.DataMatrix).IsUnique().HasFilter("[Barcodes_DataMatrix] IS NOT NULL");
        });

        builder.OwnsOne(x => x.Metrics, m =>
        {
            m.ToTable("Articles", t => t.IsTemporal(ttb =>
            {
                ttb.UseHistoryTable("ArticlesHistory");
                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
            }));

            m.Property(x => x.Mass)
                .HasConversion<MassStringConverter>()
                .HasMaxLength(50)
                .HasColumnName("Mass")
                .IsRequired(false);

            m.Property(x => x.LengthX)
                .HasConversion<LengthStringConverter>()
                .HasMaxLength(50)
                .HasColumnName("LengthX")
                .IsRequired(false);

            m.Property(x => x.LengthY)
                .HasConversion<LengthStringConverter>()
                .HasMaxLength(50)
                .HasColumnName("LengthY")
                .IsRequired(false);

            m.Property(x => x.LengthZ)
                .HasConversion<LengthStringConverter>()
                .HasMaxLength(50)
                .HasColumnName("LengthZ")
                .IsRequired(false);

            m.WithOwner();
        });

        builder.Property(x => x.ShelfLife)
            .HasConversion(
                x => x.HasValue ? (long?)x.Value.Ticks : null,
                x => x.HasValue ? (TimeSpan?)TimeSpan.FromTicks(x.Value) : null)
            .IsRequired(false);

        builder.HasMany(a => a.Partitions).WithMany(p => p.ArticleMembers);
    }
}
