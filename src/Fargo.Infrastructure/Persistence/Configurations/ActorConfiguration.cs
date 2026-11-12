using Fargo.Core.Entities.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.ToTable("actors");

        builder.Property(x => x.ActorType);

        builder.Property(x => x.IsActive);

        builder.HasIndex(x => x.Nameid).IsUnique();

        // Permissions is a read-only view of PermissionGrants (the actor_permissions rows).
        builder.Ignore(x => x.Permissions);
    }
}
