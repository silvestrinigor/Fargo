using Fargo.Core.Entities.SupplyOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class SupplyOrderPartitionConfiguration : IEntityTypeConfiguration<SupplyOrderPartition>
{
    public void Configure(EntityTypeBuilder<SupplyOrderPartition> builder)
    {
        builder.ToTable("supply_order_partitions");

        builder.HasKey(x => new
        {
            x.SupplyOrderGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.SupplyOrder)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.SupplyOrderGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
