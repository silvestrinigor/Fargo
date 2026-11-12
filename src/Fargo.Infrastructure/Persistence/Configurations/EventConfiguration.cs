using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // Root of the TPT hierarchy: the base lives in "events" and each derived
        // event type is mapped to its own table whose primary key is also a
        // foreign key back to events.guid.
        builder.UseTptMappingStrategy();

        builder.ToTable("events");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.EventType);

        builder.HasIndex(x => x.EventType);

        // OccuredAt lives on the base table, so point-in-time queries that order
        // or range by it are supported here.
        builder.HasIndex(x => x.OccuredAt);
    }
}
