using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserPartitionAccessConfiguration : IEntityTypeConfiguration<UserPartitionAccess>
{
    public void Configure(EntityTypeBuilder<UserPartitionAccess> builder)
    {
        builder.ToTable("UserPartitionAccesses", tableBuilder => tableBuilder.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasOne(a => a.Partition).WithMany().HasForeignKey(a => a.PartitionGuid);

        builder.HasOne(a => a.User).WithMany(g => g.PartitionAccesses).HasForeignKey(a => a.UserGuid);
    }
}
