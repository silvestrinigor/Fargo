using Fargo.Domain.Barcodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class BarcodeConfiguration : IEntityTypeConfiguration<Barcode>
{
    public void Configure(EntityTypeBuilder<Barcode> builder)
    {
        builder.ToTable(t => t.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Code).HasMaxLength(3000).IsRequired();

        builder.Property(x => x.Format).IsRequired();

        builder.Ignore(x => x.Value);

        builder.Property(x => x.ArticleGuid).IsRequired();

        builder
            .HasOne(b => b.Article)
            .WithMany(a => a.BarcodeCollection)
            .HasForeignKey(b => b.ArticleGuid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ArticleGuid);

        builder.HasIndex(x => new { x.ArticleGuid, x.Format }).IsUnique();
    }
}
