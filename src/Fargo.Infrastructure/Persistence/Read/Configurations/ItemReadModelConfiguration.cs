using Fargo.Application.Models.ItemModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Read.Configurations
{
    public class ItemReadModelConfiguration : IEntityTypeConfiguration<ItemReadModel>
    {
        public void Configure(EntityTypeBuilder<ItemReadModel> builder)
        {
            builder.ToTable(x => x.IsTemporal());

            builder.HasKey(x => x.Guid);
        }
    }
}
