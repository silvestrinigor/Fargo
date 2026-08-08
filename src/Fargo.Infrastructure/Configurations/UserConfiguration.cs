using Fargo.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.Nameid).IsUnique();

        builder.Property(x => x.Permissions).HasColumnType("jsonb");

        builder.HasMany(u => u.Partitions).WithMany().UsingEntity(j =>
        {
            j.ToTable("user_partitions");
        });

        builder.HasMany(u => u.PartitionAccesses).WithMany().UsingEntity(j =>
        {
            j.ToTable("user_partition_accesses");
        });
    }
}
