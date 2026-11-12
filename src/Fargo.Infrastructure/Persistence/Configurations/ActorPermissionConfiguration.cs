using Fargo.Core.Entities.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public sealed class ActorPermissionConfiguration : IEntityTypeConfiguration<ActorPermission>
{
    public void Configure(EntityTypeBuilder<ActorPermission> builder)
    {
        builder.ToTable("actor_permissions");

        builder.HasKey(x => new
        {
            x.ActorGuid,
            x.ActionType
        });

        builder
        .HasOne(x => x.Actor)
        .WithMany(x => x.PermissionGrants)
        .HasForeignKey(x => x.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}