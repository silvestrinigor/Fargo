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

        // Directly-granted permissions are a small primitive collection stored as
        // jsonb on the actor row (matching how user-group permissions are
        // persisted), rather than as a separate join table.
        builder.Property(x => x.Permissions).HasColumnType("jsonb");
    }
}
