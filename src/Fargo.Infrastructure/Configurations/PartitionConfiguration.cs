using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations
{
    public class PartitionConfiguration : IEntityTypeConfiguration<Partition>
    {
        public void Configure(EntityTypeBuilder<Partition> builder)
        {
            builder.ToTable(t => t.IsTemporal());

            builder.HasKey(x => x.Guid);

            builder
                .Property(x => x.Name)
                .IsRequired();

            builder
                .Property(x => x.Description)
                .IsRequired();

            builder
                .Property(x => x.IsActive)
                .IsRequired();

            builder
                .Property(x => x.IsGlobal)
                .IsRequired();

            builder
                .Property(x => x.IsEditable)
                .IsRequired();

            builder
                .Property(x => x.ParentPartitionGuid);

            builder
                .HasOne(x => x.ParentPartition)
                .WithMany()
                .HasForeignKey(x => x.ParentPartitionGuid)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(x => x.CreatedAt)
                .IsRequired();

            builder
                .Property(x => x.CreatedByGuid);

            builder
                .Property(x => x.EditedAt);

            builder
                .Property(x => x.EditedByGuid);
        }
    }
}