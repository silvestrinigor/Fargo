using Fargo.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserPartitionConfiguration : IEntityTypeConfiguration<UserPartition>
{
    public void Configure(EntityTypeBuilder<UserPartition> builder)
    {
        builder.ToTable("user_partitions");

        builder.HasKey(x => new
        {
            x.UserGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.User)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.UserGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
