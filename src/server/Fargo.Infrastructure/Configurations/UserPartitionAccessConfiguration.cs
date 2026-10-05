using Fargo.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserPartitionAccessConfiguration : IEntityTypeConfiguration<UserPartitionAccess>
{
    public void Configure(EntityTypeBuilder<UserPartitionAccess> builder)
    {
        builder.ToTable("user_partition_accesses");

        builder.HasKey(x => new
        {
            x.UserGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.User)
        .WithMany(x => x.PartitionAccesses)
        .HasForeignKey(x => x.UserGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
