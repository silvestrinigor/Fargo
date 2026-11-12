using Fargo.Core.Entities.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class UserGroupPermissionConfiguration : IEntityTypeConfiguration<UserGroupPermission>
{
    public void Configure(EntityTypeBuilder<UserGroupPermission> builder)
    {
        builder.ToTable("user_group_permissions");

        builder.HasKey(x => new
        {
            x.UserGroupGuid,
            x.ActionType
        });

        builder
        .HasOne(x => x.UserGroup)
        .WithMany(x => x.PermissionGrants)
        .HasForeignKey(x => x.UserGroupGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}