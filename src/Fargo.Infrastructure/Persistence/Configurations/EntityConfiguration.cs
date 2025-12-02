using Fargo.Domain.Abstracts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class EntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
        {
            builder
                .HasKey(a => a.Guid);
            builder
                .Property(a => a.Name)
                .HasMaxLength(200);
            builder
                .Property(a => a.Description)
                .HasMaxLength(1000);
            builder
                .HasOne<Entity>()
                .WithMany()
                .HasForeignKey(a => a.ParentGuid)
                .OnDelete(DeleteBehavior.Restrict);
            builder
                .Property(a => a.CreatedAt)
                .IsRequired();
        }
    }
}