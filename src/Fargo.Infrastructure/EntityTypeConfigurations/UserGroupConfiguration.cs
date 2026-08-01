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

        builder.Property(x => x.Guid).ValueGeneratedNever();

        builder.Property(x => x.Nameid).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.Permissions).HasColumnType("jsonb");

        builder.HasMany(g => g.Partitions).WithMany();
    }
}
