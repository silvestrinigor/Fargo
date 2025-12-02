using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.Property(x => x.Mass)
            .HasColumnType("decimal(38,18)")
            .HasConversion(
                v => new { v.Value, Unit = v.uni.ToString() },
                v => new Mass(v.Value, Enum.Parse<MassUnit>(v.Unit))

        builder.Property(x => x.Length)
            .HasColumnType("decimal(38,18)")
            .HasConversion(
                v => v.HasValue ? v.Value.Meters : (double?)null,
                v => v.HasValue ? Length.FromMeters(v.Value) : (Length?)null);

        builder.Property(x => x.Width)
            .HasColumnType("decimal(38,18)")
            .HasConversion(
                v => v.HasValue ? v.Value.Meters : (double?)null,
                v => v.HasValue ? Length.FromMeters(v.Value) : (Length?)null);

        builder.Property(x => x.Height)
            .HasColumnType("decimal(38,18)")
            .HasConversion(
                v => v.HasValue ? v.Value.Meters : (double?)null,
                v => v.HasValue ? Length.FromMeters(v.Value) : (Length?)null);

        builder.Property(x => x.ShelfLife);

        builder.Ignore(x => x.Volume);
        builder.Ignore(x => x.Density);
    }
}
