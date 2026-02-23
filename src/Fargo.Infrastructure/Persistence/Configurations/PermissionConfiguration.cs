using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    internal class PermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder
                .HasKey(x => new { x.UserGuid, x.ActionType });

            builder
                .HasOne(x => x.User)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.UserGuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(x => x.ActionType)
                .IsRequired();
        }
    }
}