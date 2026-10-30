using Fargo.Core.Identity;
using Fargo.Core.ServiceAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ServiceAccountConfiguration : IEntityTypeConfiguration<ServiceAccount>
{
    public void Configure(EntityTypeBuilder<ServiceAccount> builder)
    {
        builder.ToTable("service_accounts");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.ClientId).IsUnique();

        // Permissions and partition accesses are small primitive collections, so
        // they are stored as jsonb on the row (matching how User.Permissions is
        // persisted) rather than as separate join tables.
        builder.Property(x => x.Permissions).HasColumnType("jsonb");

        builder.Property(x => x.PartitionAccessGuids).HasColumnType("jsonb");
    }
}