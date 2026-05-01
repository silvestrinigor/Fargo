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

        builder.Ignore("ImageKey");

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
                    v => v != null ? new Ean13(v) : null))
                .HasMaxLength(13);

            barcodes.Property(x => x.Ean8)
                .HasConversion(new ValueConverter<Ean8?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new Ean8(v) : null))
                .HasMaxLength(8);

            barcodes.Property(x => x.UpcA)
                .HasConversion(new ValueConverter<UpcA?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new UpcA(v) : null))
                .HasMaxLength(12);

            barcodes.Property(x => x.UpcE)
                .HasConversion(new ValueConverter<UpcE?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new UpcE(v) : null))
                .HasMaxLength(8);

            barcodes.Property(x => x.Code128)
                .HasConversion(new ValueConverter<Code128?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new Code128(v) : null))
                .HasMaxLength(80);

            barcodes.Property(x => x.Code39)
                .HasConversion(new ValueConverter<Code39?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new Code39(v) : null))
                .HasMaxLength(80);

            barcodes.Property(x => x.Itf14)
                .HasConversion(new ValueConverter<Itf14?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new Itf14(v) : null))
                .HasMaxLength(14);

            barcodes.Property(x => x.Gs1128)
                .HasConversion(new ValueConverter<Gs1128?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new Gs1128(v) : null))
                .HasMaxLength(80);

            barcodes.Property(x => x.QrCode)
                .HasConversion(new ValueConverter<QrCode?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new QrCode(v) : null))
                .HasMaxLength(2953);

            barcodes.Property(x => x.DataMatrix)
                .HasConversion(new ValueConverter<DataMatrix?, string?>(
                    v => v.HasValue ? v.Value.Code : null,
                    v => v != null ? new DataMatrix(v) : null))
                .HasMaxLength(2335);
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

        builder.OwnsOne(x => x.Images, i =>
        {
            i.ToTable("Articles", t => t.IsTemporal(ttb =>
            {
                ttb.UseHistoryTable("ArticlesHistory");
                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
            }));

            i.Property(x => x.ImageKey)
                .HasMaxLength(500)
                .HasColumnName("ImageKey")
                .IsRequired(false);

            i.WithOwner();
        });

        builder.Navigation(x => x.Images).IsRequired();

        builder.HasMany(a => a.Partitions).WithMany(p => p.ArticleMembers);
    }
}
