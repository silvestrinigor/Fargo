using Fargo.Core.Entities.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class ActorPartitionConfiguration : IEntityTypeConfiguration<ActorPartition>
{
    public void Configure(EntityTypeBuilder<ActorPartition> builder)
    {
        builder.ToTable("actor_partitions");

        builder.HasKey(x => new
        {
            x.ActorGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.Actor)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
