using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder
            .HasKey(a => a.Guid);
        builder
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder
            .Property(a => a.CreatedAt)
            .IsRequired();
    }
}
