using Fargo.Core.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public sealed class UserGroupPermissionConfiguration : IEntityTypeConfiguration<UserGroupPermission>
{
    public void Configure(EntityTypeBuilder<UserGroupPermission> builder)
    {
        builder.ToTable("user_group_permissions");

        builder.HasKey(x => x.Guid);

        builder.HasAlternateKey(x => new
        {
            x.UserGroupGuid,
            x.Action
        });

        builder.HasOne(x => x.UserGroup)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.UserGroupGuid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
