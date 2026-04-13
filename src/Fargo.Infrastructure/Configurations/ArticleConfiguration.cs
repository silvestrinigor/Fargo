using Fargo.Domain.Entities;
using Fargo.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable(t => t.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.EditedByGuid);

        builder.Property(x => x.Mass)
            .HasConversion<MassDoubleConverter>()
            .IsRequired(false);

        builder.Property(x => x.LengthX)
            .HasConversion<LengthDoubleConverter>()
            .IsRequired(false);

        builder.Property(x => x.LengthY)
            .HasConversion<LengthDoubleConverter>()
            .IsRequired(false);

        builder.Property(x => x.LengthZ)
            .HasConversion<LengthDoubleConverter>()
            .IsRequired(false);

        builder.Property(x => x.ImageKey)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasMany(a => a.Partitions).WithMany(p => p.ArticleMembers);
    }
}
