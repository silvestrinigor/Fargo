using Fargo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.Guid);

        builder.HasIndex(x => x.EntityGuid);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.ActorGuid);
        builder.HasIndex(x => x.ApiClientGuid);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.EntityGuid, x.OccurredAt });
    }
}
