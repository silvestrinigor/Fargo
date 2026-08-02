using Fargo.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.EntityTypeConfigurations;

public sealed class UserAuthenticationConfiguration : IEntityTypeConfiguration<UserAuthentication>
{
    public void Configure(EntityTypeBuilder<UserAuthentication> builder)
    {
        builder.ToTable("user_authentications");

        builder.HasKey(x => x.UserGuid);

        builder
        .HasOne(x => x.User)
        .WithOne(x => x.Authentication)
        .HasForeignKey<UserAuthentication>(x => x.UserGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
