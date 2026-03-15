using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroups", tableBuilder => tableBuilder.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder.HasAlternateKey(x => x.Nameid);

            builder.Property(x => x.Guid).ValueGeneratedNever();

            builder.Property(x => x.Nameid).IsRequired();

            builder.Property(x => x.Description).IsRequired();

            builder.Property(x => x.IsActive).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.CreatedByGuid).IsRequired();

            builder.Property(x => x.EditedAt).IsRequired(false);

            builder.Property(x => x.EditedByGuid).IsRequired(false);

            builder.HasMany(x => x.Permissions)
                .WithOne(x => x.UserGroup)
                .HasForeignKey(x => x.UserGroupGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(g => g.Partitions).WithMany(p => p.UserGroupMembers);
        }
    }
}