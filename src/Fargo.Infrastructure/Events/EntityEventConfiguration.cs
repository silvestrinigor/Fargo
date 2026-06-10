using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class EntityEventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("EntityEvents");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.EntityType).IsRequired();
        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.EntityGuid).IsRequired();
        builder.Property(x => x.ActorGuid).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.HasIndex(x => new { x.EntityGuid, x.OccurredAt });
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.ActorGuid);
    }
}
