using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class EntityPartitionEventConfiguration : IEntityTypeConfiguration<EntityPartitionEvent>
{
    public void Configure(EntityTypeBuilder<EntityPartitionEvent> builder)
    {
        builder.ToTable("EntityPartitionEvents");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.Guid).ValueGeneratedNever();
        builder.Property(x => x.PartitionGuid).IsRequired();

        builder.Ignore(x => x.EntityType);
        builder.Ignore(x => x.EntityGuid);
        builder.Ignore(x => x.ActorGuid);
        builder.Ignore(x => x.OccurredAt);

        builder
            .HasOne(x => x.Event)
            .WithOne()
            .HasForeignKey<EntityPartitionEvent>(x => x.Guid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PartitionGuid);
    }
}
