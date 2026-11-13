using Fargo.Core.Entities.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("user_groups");

        builder.HasIndex(x => x.Nameid).IsUnique();

        // Permissions is a read-only view of PermissionGrants (the user_group_permissions rows).
        builder.Ignore(x => x.Permissions);

        builder
        .HasOne(x => x.ParentUserGroup)
        .WithMany()
        .HasForeignKey(x => x.ParentUserGroupGuid)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
