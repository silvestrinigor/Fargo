using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(t => t.IsTemporal());

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserGuid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ReplacedByTokenHash);

        builder.Property(x => x.ExpiresAt);
    }
}
