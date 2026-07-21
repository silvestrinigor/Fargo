using Fargo.Core.Items;
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

        builder.Property(x => x.ProductionDate).IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.ParentContainerGuid).IsRequired(false);

        builder.HasOne(i => i.Container)
            .WithOne(c => c.Item)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ParentContainer)
            .WithMany()
            .HasForeignKey(i => i.ParentContainerGuid)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ParentContainerGuid);

        builder.Ignore(x => x.ParentContainer);

        builder.Ignore(x => x.Container);

        builder.HasMany(i => i.Partitions).WithMany(p => p.ItemMembers);
    }
}
