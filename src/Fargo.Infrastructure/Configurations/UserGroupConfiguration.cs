using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroups", tableBuilder => tableBuilder.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.Nameid).IsUnique();

        builder.Property(x => x.Guid).ValueGeneratedNever();

        builder.Property(x => x.Nameid).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.EditedByGuid).IsRequired(false);

        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.UserGroup)
            .HasForeignKey(x => x.UserGroupGuid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Partitions).WithMany(p => p.UserGroupMembers);
    }
}
