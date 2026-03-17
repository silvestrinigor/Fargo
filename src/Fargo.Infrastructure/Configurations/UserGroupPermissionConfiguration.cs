using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserGroupPermissionConfiguration : IEntityTypeConfiguration<UserGroupPermission>
{
    public void Configure(EntityTypeBuilder<UserGroupPermission> builder)
    {
        builder.ToTable("UserGroupPermissions", tableBuilder => tableBuilder.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasAlternateKey(x => new
        {
            x.UserGroupGuid,
            x.Action
        });

        builder.Property(x => x.Guid)
            .ValueGeneratedNever();

        builder.Property(x => x.UserGroupGuid)
            .IsRequired();

        builder.Property(x => x.Action)
            .IsRequired();

        builder.HasOne(x => x.UserGroup)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.UserGroupGuid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
