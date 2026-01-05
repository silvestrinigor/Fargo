using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    internal class EntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
        {
            throw new NotImplementedException();
        }
    }
}
