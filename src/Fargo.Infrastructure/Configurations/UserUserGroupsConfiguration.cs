using Fargo.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserUserGroupConfiguration : IEntityTypeConfiguration<UserUserGroup>
{
    public void Configure(EntityTypeBuilder<UserUserGroup> builder)
    {
        builder.ToTable("user_user_groups");

        builder.HasKey(x => new
        {
            x.UserGuid,
            x.UserGroupGuid
        });

        builder
        .HasOne(x => x.User)
        .WithMany(x => x.UserGroupMemberships)
        .HasForeignKey(x => x.UserGuid)
        .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasOne(x => x.UserGroup)
        .WithMany()
        .HasForeignKey(x => x.UserGroupGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
