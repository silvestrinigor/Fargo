using Fargo.Core.ServiceAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ServiceAccountConfiguration : IEntityTypeConfiguration<ServiceAccount>
{
    public void Configure(EntityTypeBuilder<ServiceAccount> builder)
    {
        builder.ToTable("service_accounts");

        // The service account is an owned satellite of its actor, keyed and
        // joined 1:1 by the owning actor's identifier (mirroring
        // Article/ArticlePack). Its shared authorization state lives on the actor.
        builder.HasKey(x => x.ActorGuid);

        builder
        .HasOne(x => x.Actor)
        .WithOne(a => a.ServiceAccount)
        .HasForeignKey<ServiceAccount>(x => x.ActorGuid)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
