using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserGroupPartitionAccessConfiguration : IEntityTypeConfiguration<UserGroupPartitionAccess>
{
    public void Configure(EntityTypeBuilder<UserGroupPartitionAccess> builder)
    {
        builder.ToTable("UserGroupPartitionAccesses", tableBuilder => tableBuilder.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasOne(a => a.Partition).WithMany().HasForeignKey(a => a.PartitionGuid);

        builder.HasOne(a => a.UserGroup).WithMany(g => g.PartitionAccesses).HasForeignKey(a => a.UserGroupGuid);
    }
}
