using Fargo.Domain.Abstracts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class EntityConfiguration : IEntityTypeConfiguration<Named>
    {
        public void Configure(EntityTypeBuilder<Named> builder)
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
                .HasOne<Named>()
                .WithMany()
                .HasForeignKey(a => a.ParentGuid)
                .OnDelete(DeleteBehavior.Restrict);
            builder
                .Property(a => a.CreatedAt)
                .IsRequired();
        }
    }
}