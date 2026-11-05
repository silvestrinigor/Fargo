using Fargo.Core.Entities.Actors.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // The user is an owned satellite of its actor, keyed and joined 1:1 by
        // the owning actor's identifier (mirroring Article/ArticleVariation).
        builder.HasKey(x => x.ActorGuid);

        builder
        .HasOne(x => x.Actor)
        .WithOne(a => a.User)
        .HasForeignKey<User>(x => x.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);

        // Derived from the owning actor's identifier; not separately persisted.
        builder.Ignore(x => x.IsAdmin);
    }
}
