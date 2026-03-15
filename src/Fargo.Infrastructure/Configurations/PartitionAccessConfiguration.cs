using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class PartitionAccessConfiguration : IEntityTypeConfiguration<UserPartitionAccess>
    {
        public void Configure(EntityTypeBuilder<UserPartitionAccess> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder
                .Property(x => x.UserGuid)
                .IsRequired();

            builder
                .Property(x => x.PartitionGuid)
                .IsRequired();

            builder
                .HasOne(x => x.User)
                .WithMany(x => x.PartitionAccesses)
                .HasForeignKey(x => x.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.Partition)
                .WithMany()
                .HasForeignKey(x => x.PartitionGuid)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}