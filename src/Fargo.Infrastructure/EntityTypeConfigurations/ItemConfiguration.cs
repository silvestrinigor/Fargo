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

        builder.HasIndex(x => x.ArticleGuid);

        builder.HasIndex(x => x.ParentItemContainerGuid);

        builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleGuid).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ParentItemContainer).WithMany().HasForeignKey(i => i.ParentItemContainerGuid).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Partitions).WithMany().UsingEntity(j =>
        {
            j.ToTable("item_partitions");
        });
    }
}
