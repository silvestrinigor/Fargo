using Fargo.Core.Partitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class PartitionConfiguration : IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder.ToTable("partitions");

        builder.HasKey(x => x.Guid);

        builder
        .HasOne(x => x.ParentPartition)
        .WithMany()
        .HasForeignKey(x => x.ParentPartitionGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
