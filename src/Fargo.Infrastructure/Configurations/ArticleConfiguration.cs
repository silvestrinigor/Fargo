using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            barcodes.Ignore(x => x.Ean13);
            barcodes.Ignore(x => x.Ean8);
            barcodes.Ignore(x => x.UpcA);
            barcodes.Ignore(x => x.UpcE);
            barcodes.Ignore(x => x.Code128);
            barcodes.Ignore(x => x.Code39);
            barcodes.Ignore(x => x.Itf14);
            barcodes.Ignore(x => x.Gs1128);
            barcodes.Ignore(x => x.QrCode);
            barcodes.Ignore(x => x.DataMatrix);
            barcodes.Ignore(x => x.IsEmpty);

            barcodes.OwnsOne<Ean13Data>("Ean13Data", e =>
            {
                e.ToTable("ArticleEan13", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(13).IsRequired();
            });
            barcodes.OwnsOne<Ean8Data>("Ean8Data", e =>
            {
                e.ToTable("ArticleEan8", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(8).IsRequired();
            });
            barcodes.OwnsOne<UpcAData>("UpcAData", e =>
            {
                e.ToTable("ArticleUpcA", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(12).IsRequired();
            });
            barcodes.OwnsOne<UpcEData>("UpcEData", e =>
            {
                e.ToTable("ArticleUpcE", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(8).IsRequired();
            });
            barcodes.OwnsOne<Code128Data>("Code128Data", e =>
            {
                e.ToTable("ArticleCode128", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(80).IsRequired();
            });
            barcodes.OwnsOne<Code39Data>("Code39Data", e =>
            {
                e.ToTable("ArticleCode39", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(80).IsRequired();
            });
            barcodes.OwnsOne<Itf14Data>("Itf14Data", e =>
            {
                e.ToTable("ArticleItf14", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(14).IsRequired();
            });
            barcodes.OwnsOne<Gs1128Data>("Gs1128Data", e =>
            {
                e.ToTable("ArticleGs1128", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(80).IsRequired();
            });
            barcodes.OwnsOne<QrCodeData>("QrCodeData", e =>
            {
                e.ToTable("ArticleQrCode", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(2953).IsRequired();
            });
            barcodes.OwnsOne<DataMatrixData>("DataMatrixData", e =>
            {
                e.ToTable("ArticleDataMatrix", t => t.IsTemporal());
                e.HasKey(x => x.ArticleGuid);
                e.WithOwner().HasForeignKey(x => x.ArticleGuid);
                e.Property(x => x.Code).HasMaxLength(2335).IsRequired();
            });
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
