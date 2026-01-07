using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;
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
        }
    }
}
