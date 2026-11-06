using Fargo.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// The table name. <see cref="OutboxRelay"/> repeats it in its raw claim
    /// query, so keep the two in sync.
    /// </summary>
    public const string TableName = "outbox_messages";

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(m => m.Sequence);

        builder
        .Property(m => m.Sequence)
        .UseIdentityByDefaultColumn();

        builder.HasIndex(m => m.EventId).IsUnique();

        builder
        .Property(m => m.EventType)
        .HasMaxLength(OutboxMessage.EventTypeMaxLength);

        // Stored as text, not jsonb, so the body is published byte-for-byte as
        // serialized (jsonb would reorder keys and normalize whitespace).
        builder
        .Property(m => m.Body)
        .HasColumnType("text");

        builder
        .Property(m => m.LastError)
        .HasMaxLength(OutboxMessage.LastErrorMaxLength);

        // The relay only ever scans unpublished rows in sequence order.
        builder
        .HasIndex(m => m.Sequence)
        .HasDatabaseName("ix_outbox_messages_unprocessed_sequence")
        .HasFilter("processed_at IS NULL");

        // Retention purge of published rows.
        builder.HasIndex(m => m.ProcessedAt);
    }
}
