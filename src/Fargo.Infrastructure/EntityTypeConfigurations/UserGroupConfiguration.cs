using Fargo.Core.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("user_groups");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.Nameid).IsUnique();

        builder.Property(x => x.Permissions).HasColumnType("jsonb");

        builder.HasMany(g => g.Partitions).WithMany().UsingEntity(p =>
        {
            p.ToTable("user_group_partitions");
        });

        builder.HasMany(g => g.PartitionAccesses).WithMany().UsingEntity(p =>
        {
            p.ToTable("user_group_partition_accesses");
        });
    }
}
