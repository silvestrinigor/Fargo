using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class EntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
        {
            builder
                .HasKey(e => e.Guid);
            
            builder
                .Property(e => e.CreatedAt)
                .IsRequired();
            
            builder
                .Property(e => e.Name)
                .HasConversion(
                       v => v != null ? v.ToString() : null,
                       v => v != null ? Name.NewName(v) : null)
                .IsRequired(false);

            builder
                .Property(e => e.Description)
                .HasConversion(
                        v => v != null ? v.ToString() : null,
                        v => v != null ? Description.NewDescription(v) : null)
                .IsRequired(false);

            builder
                .Property(e => e.EntityType)
                .IsRequired();
        }
    }
}
