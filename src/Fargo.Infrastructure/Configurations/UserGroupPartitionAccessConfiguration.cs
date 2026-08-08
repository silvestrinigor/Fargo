using Fargo.Core.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserGroupPartitionAccessConfiguration : IEntityTypeConfiguration<UserGroupPartitionAccess>
{
    public void Configure(EntityTypeBuilder<UserGroupPartitionAccess> builder)
    {
        builder.ToTable("user_group_partition_accesses");

        builder.HasKey(x => new
        {
            x.UserGroupGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.UserGroup)
        .WithMany(x => x.PartitionAccesses)
        .HasForeignKey(x => x.UserGroupGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
