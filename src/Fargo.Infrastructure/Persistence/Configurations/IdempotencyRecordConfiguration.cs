using Fargo.Application.Idempotency;
using Fargo.Core.Entities.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    /// <summary>
    /// <c>IdempotencyRecordRepository</c> repeats the table name in its raw purge query, so keep the two in
    /// sync.
    /// </summary>
    public const string TableName = "idempotency_keys";

    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable(TableName);

        // The key is scoped to the actor. The unique key is also what lets only one of two racing requests with
        // the same key commit; the HTTP layer recognizes it by name.
        builder
        .HasKey(r => new { r.ActorGuid, r.Key })
        .HasName(IdempotencyRecord.PrimaryKeyName);

        builder
        .Property(r => r.CommandName)
        .HasMaxLength(IdempotencyRecord.CommandNameMaxLength);

        builder
        .Property(r => r.Response)
        .HasColumnType("text");

        // Retention purge of expired records.
        builder.HasIndex(r => r.ExpiresAt);

        // Deleting the actor deletes its records, like its audit logs.
        builder
        .HasOne<Actor>()
        .WithMany()
        .HasForeignKey(r => r.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
