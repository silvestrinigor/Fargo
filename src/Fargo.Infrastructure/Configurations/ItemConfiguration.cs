using Fargo.Domain.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable(t => t.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder
            .HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleGuid)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ArticleGuid);

        builder.Property(x => x.EditedByGuid);

        builder.Property(x => x.ProductionDate).IsRequired(false);

        builder.Property(x => x.ParentContainerGuid).IsRequired(false);

        builder
            .HasOne<Item>()
            .WithMany()
            .HasForeignKey(x => x.ParentContainerGuid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentContainerGuid);

        builder.Ignore(x => x.ParentContainer);
        builder.Ignore(x => x.Container);

        builder.HasMany(i => i.Partitions).WithMany(p => p.ItemMembers);
    }
}
