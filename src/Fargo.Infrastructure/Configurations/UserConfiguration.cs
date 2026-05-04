using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", tableBuilder => tableBuilder.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.Nameid).IsUnique();

        builder.Property(x => x.Guid)
            .ValueGeneratedNever();

        builder.Property(x => x.Nameid)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .IsRequired(false);

        builder.Property(x => x.LastName)
            .IsRequired(false);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.DefaultPasswordExpirationPeriod)
            .HasConversion(
                x => x.HasValue ? x.Value.Ticks : (long?)null,
                x => x.HasValue ? TimeSpan.FromTicks(x.Value) : null)
            .IsRequired(false);

        builder.Property(x => x.RequirePasswordChangeAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.EditedByGuid)
            .IsRequired(false);

        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserGuid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserGroups).WithMany(x => x.Users);

        builder.HasMany(u => u.Partitions).WithMany(p => p.UserMembers);
    }
}
