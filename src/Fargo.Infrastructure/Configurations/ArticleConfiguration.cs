using Fargo.Domain.Articles;
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

        builder.Ignore(x => x.Barcodes);

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
