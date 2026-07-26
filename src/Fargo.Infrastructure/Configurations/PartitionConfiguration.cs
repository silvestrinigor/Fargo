using Fargo.Core.Partitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class PartitionConfiguration : IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Name).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.ParentPartitionGuid);

        builder
            .HasOne(x => x.ParentPartition)
            .WithMany(p => p.PartitionMembers)
            .HasForeignKey(x => x.ParentPartitionGuid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
