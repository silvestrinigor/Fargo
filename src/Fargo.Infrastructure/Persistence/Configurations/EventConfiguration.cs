using Fargo.Domain.Entities;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder
                .HasKey(e => e.Guid);

            builder
                .Property(x => x.EventData)
                .HasConversion(
                    x => x != null ? JsonSerializer.Serialize(x.ToEventDataEnvelope()) : null,
                    x => x != null ? JsonSerializer.Deserialize<EventDataEnvelope>(x)!.ToEventData() : null
                    );
        }
    }
}