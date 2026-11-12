using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;
using Fargo.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Guid);

        builder.HasIndex(a => new { a.ActorGuid, a.OccurredAt });

        builder.HasIndex(a => a.OccurredAt);

        // The audit log relates directly to the acting actor. Deleting the actor
        // deletes its command audit logs (cascade). There is no inverse
        // navigation on Actor; the actor is referenced only by ActorGuid.
        builder
        .HasOne<Actor>()
        .WithMany()
        .HasForeignKey(a => a.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .Property(x => x.Metadata)
        .HasConversion(new AuditMetadataValueConverter())
        .HasColumnType("jsonb");
    }
}
