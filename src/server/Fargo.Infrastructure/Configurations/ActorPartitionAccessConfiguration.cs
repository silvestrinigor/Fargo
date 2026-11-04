using Fargo.Core.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ActorPartitionAccessConfiguration : IEntityTypeConfiguration<ActorPartitionAccess>
{
    public void Configure(EntityTypeBuilder<ActorPartitionAccess> builder)
    {
        builder.ToTable("actor_partition_accesses");

        builder.HasKey(x => new
        {
            x.ActorGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.Actor)
        .WithMany(x => x.PartitionAccesses)
        .HasForeignKey(x => x.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
