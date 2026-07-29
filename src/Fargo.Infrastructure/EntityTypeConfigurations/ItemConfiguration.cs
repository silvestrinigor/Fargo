using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(x => x.Guid);

        builder
            .HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ArticleGuid);

        builder.OwnsOne(i => i.Container, container =>
        {
        });

        builder.HasOne(i => i.ParentContainer)
            .WithMany()
            .HasForeignKey(i => i.ParentContainerGuid)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ParentContainerGuid);

        builder.HasMany(i => i.Partitions).WithMany();
    }
}
