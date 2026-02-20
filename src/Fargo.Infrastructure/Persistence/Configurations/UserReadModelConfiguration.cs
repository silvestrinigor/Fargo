using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class UserReadModelConfiguration : IEntityTypeConfiguration<UserReadModel>
    {
        public void Configure(EntityTypeBuilder<UserReadModel> builder)
        {
            builder.ToTable(x => x.IsTemporal());

            builder.HasKey(x => x.Guid);
        }
    }
}