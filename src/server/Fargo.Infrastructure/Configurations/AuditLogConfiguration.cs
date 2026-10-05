using Fargo.Core.Audits;
using Fargo.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Guid);

        builder.HasIndex(a => new { a.EntityGuid, a.OccurredAt });

        builder.HasIndex(a => new { a.ActorGuid, a.OccurredAt });

        builder.HasIndex(a => a.OccurredAt);

        builder
        .Property(x => x.Metadata)
        .HasConversion(new AuditMetadataValueConverter())
        .HasColumnType("jsonb");
    }
}
