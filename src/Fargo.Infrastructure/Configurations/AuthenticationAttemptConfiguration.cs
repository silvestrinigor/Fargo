using Fargo.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class AuthenticationAttemptConfiguration : IEntityTypeConfiguration<AuthenticationAttempt>
{
    public void Configure(EntityTypeBuilder<AuthenticationAttempt> builder)
    {
        builder.ToTable("authentication_attempt");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => new { x.IpAddress, x.ActorIdentifier, x.OccuredAt, x.Successful });
    }
}
