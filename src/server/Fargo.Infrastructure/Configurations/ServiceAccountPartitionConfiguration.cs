using Fargo.Core.ServiceAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ServiceAccountPartitionConfiguration : IEntityTypeConfiguration<ServiceAccountPartition>
{
    public void Configure(EntityTypeBuilder<ServiceAccountPartition> builder)
    {
        builder.ToTable("service_account_partitions");

        builder.HasKey(x => new
        {
            x.ServiceAccountGuid,
            x.PartitionGuid
        });

        builder
        .HasOne(x => x.ServiceAccount)
        .WithMany(x => x.Partitions)
        .HasForeignKey(x => x.ServiceAccountGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.Partition)
        .WithMany()
        .HasForeignKey(x => x.PartitionGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
