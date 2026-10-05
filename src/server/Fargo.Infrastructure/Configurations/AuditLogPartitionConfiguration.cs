using Fargo.Core.Audits;
using Fargo.Core.Partitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class AuditLogPartitionConfiguration : IEntityTypeConfiguration<AuditLogPartition>
{
    public void Configure(EntityTypeBuilder<AuditLogPartition> builder)
    {
        builder.ToTable("audit_log_partitions");

        builder.HasKey(x => new
        {
            x.AuditLogGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(a => a.AuditLog)
        .WithMany(a => a.Partitions)
        .HasForeignKey(a => a.AuditLogGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne<Partition>()
        .WithMany()
        .HasForeignKey(a => a.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
