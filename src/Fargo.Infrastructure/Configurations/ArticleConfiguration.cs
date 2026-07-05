using Fargo.Core.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Drawing;
using UnitsNet;

namespace Fargo.Infrastructure.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles", t => t.IsTemporal(ttb =>
        {
            ttb.UseHistoryTable("ArticlesHistory");
            ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
            ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
        }));

        builder.HasKey(x => x.Guid);

        builder.Ignore(x => x.ArticleType);

        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.Color)
            .HasConversion(
                x => x.HasValue ? x.Value.ToArgb() : (int?)null,
                x => x.HasValue ? Color.FromArgb(x.Value) : null)
            .IsRequired(false);

        builder.OwnsOne(x => x.Container, container =>
        {
            container.ToTable("ArticleContainers");

            container.WithOwner().HasForeignKey("ArticleGuid");

            container.Property(x => x.MaxMass)
                .HasConversion<MassStringConverter>()
                .HasMaxLength(50)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.Variation, variation =>
        {
            variation.ToTable("ArticleVariations");

            variation.WithOwner().HasForeignKey("ArticleGuid");

            variation.Property(x => x.FromArticleGuid).IsRequired();

            variation.HasOne(x => x.FromArticle)
                .WithMany()
                .HasForeignKey(x => x.FromArticleGuid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.OwnsOne(x => x.Pack, pack =>
        {
            pack.ToTable("ArticlePacks");

            pack.WithOwner().HasForeignKey("ArticleGuid");

            pack.Property(x => x.FromArticleGuid).IsRequired();

            pack.HasOne(x => x.FromArticle)
                .WithMany()
                .HasForeignKey(x => x.FromArticleGuid)
                .OnDelete(DeleteBehavior.Restrict);

            pack.Property(x => x.Quantity)
                .HasConversion(
                    x => x.Amount,
                    x => Scalar.FromAmount(x))
                .IsRequired();
        });

        builder.OwnsOne(x => x.Kit, kit =>
        {
            kit.ToTable("ArticleKits");

            kit.WithOwner().HasForeignKey("ArticleGuid");

            kit.OwnsMany(x => x.Components, component =>
            {
                component.ToTable("ArticleKitPacks");

                component.WithOwner().HasForeignKey("KitArticleGuid");

                component.Property<Guid>("Guid");
                component.HasKey("Guid");

                component.Property(x => x.ArticleGuid)
                    .HasColumnName("FromArticleGuid")
                    .IsRequired();

                component.HasOne(x => x.Article)
                    .WithMany()
                    .HasForeignKey(x => x.ArticleGuid)
                    .OnDelete(DeleteBehavior.Restrict);

                component.Property(x => x.Quantity)
                    .HasConversion(
                        x => x.Amount,
                        x => Scalar.FromAmount(x))
                    .IsRequired();

                component.HasIndex("KitArticleGuid", nameof(ArticleKitComponent.ArticleGuid))
                    .IsUnique();
            });

            kit.WithOwner();
        });

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

        builder.HasIndex(x => x.Ean13).IsUnique().HasFilter("[Ean13] IS NOT NULL");

        builder.HasIndex(x => x.Ean8).IsUnique().HasFilter("[Ean8] IS NOT NULL");

        builder.HasIndex(x => x.UpcA).IsUnique().HasFilter("[UpcA] IS NOT NULL");

        builder.HasIndex(x => x.UpcE).IsUnique().HasFilter("[UpcE] IS NOT NULL");

        builder.HasIndex(x => x.Code128).IsUnique().HasFilter("[Code128] IS NOT NULL");

        builder.HasIndex(x => x.Code39).IsUnique().HasFilter("[Code39] IS NOT NULL");

        builder.HasIndex(x => x.Itf14).IsUnique().HasFilter("[Itf14] IS NOT NULL");

        builder.HasIndex(x => x.Gs1128).IsUnique().HasFilter("[Gs1128] IS NOT NULL");

        builder.HasIndex(x => x.QrCode).IsUnique().HasFilter("[QrCode] IS NOT NULL");

        builder.HasIndex(x => x.DataMatrix).IsUnique().HasFilter("[DataMatrix] IS NOT NULL");

        builder.Property(x => x.Mass)
            .HasConversion<MassStringConverter>()
            .HasMaxLength(50)
            .HasColumnName("Mass")
            .IsRequired(false);

        builder.Property(x => x.LengthX)
            .HasConversion<LengthStringConverter>()
            .HasMaxLength(50)
            .HasColumnName("LengthX")
            .IsRequired(false);

        builder.Property(x => x.LengthY)
            .HasConversion<LengthStringConverter>()
            .HasMaxLength(50)
            .HasColumnName("LengthY")
            .IsRequired(false);

        builder.Property(x => x.LengthZ)
            .HasConversion<LengthStringConverter>()
            .HasMaxLength(50)
            .HasColumnName("LengthZ")
            .IsRequired(false);

        builder.Property(x => x.ShelfLife)
            .HasConversion(
                x => x.HasValue ? (long?)x.Value.Ticks : null,
                x => x.HasValue ? (TimeSpan?)TimeSpan.FromTicks(x.Value) : null)
            .IsRequired(false);

        builder.HasMany(a => a.Partitions).WithMany(p => p.ArticleMembers);
    }
}
