using Fargo.Core.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserGroupPartitionConfiguration : IEntityTypeConfiguration<UserGroupPartition>
{
    public void Configure(EntityTypeBuilder<UserGroupPartition> builder)
    {
        builder.ToTable("user_group_partitions");

        builder.HasKey(x => new
        {
            x.UserGroupGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.UserGroup)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.UserGroupGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
