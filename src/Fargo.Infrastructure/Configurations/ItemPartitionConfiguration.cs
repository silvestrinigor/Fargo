using Fargo.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class ItemPartitionConfiguration : IEntityTypeConfiguration<ItemPartition>
{
    public void Configure(EntityTypeBuilder<ItemPartition> builder)
    {
        builder.ToTable("item_partitions");

        builder.HasKey(x => new
        {
            x.ItemGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.Item)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.ItemGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
