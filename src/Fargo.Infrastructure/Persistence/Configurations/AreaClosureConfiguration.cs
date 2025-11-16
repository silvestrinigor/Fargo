using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class AreaClosureConfiguration : IEntityTypeConfiguration<AreaClosure>
    {
        public void Configure(EntityTypeBuilder<AreaClosure> builder)
        {
            builder.HasOne<Entity>()
                .WithMany()
                .HasForeignKey(x => x.AncestorGuid)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Entity>()
                .WithMany()
                .HasForeignKey(x => x.DescendantGuid)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasKey(x => new { x.AncestorGuid, x.DescendantGuid });

            builder.HasIndex(x => x.AncestorGuid);

            builder.HasIndex(x => x.DescendantGuid);
        }
    }
}
