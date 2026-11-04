using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

/// <summary>
/// Maps the shared <c>events</c> table that is the root of the Table-Per-Type
/// (TPT) hierarchy. Every concrete event deriving from <see cref="Event"/>
/// contributes exactly one row here, keyed by its <see cref="Event.Guid"/> and
/// carrying the properties common to all events: its <see cref="Event.EventType"/>
/// family and the instant it occurred.
/// </summary>
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
