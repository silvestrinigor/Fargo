using Fargo.Domain.Entities.Models.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder
                .HasKey(x => x.Guid);

            builder
                .Property(x => x.ModelType);

            builder
                .HasIndex(x => x.ModelType);

            builder
                .UseTptMappingStrategy();
        }
    }
}
