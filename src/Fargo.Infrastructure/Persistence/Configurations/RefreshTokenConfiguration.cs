using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder
        .HasOne<User>()
        .WithMany()
        .HasForeignKey(x => x.UserGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
