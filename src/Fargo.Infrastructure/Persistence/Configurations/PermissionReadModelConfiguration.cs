using Fargo.Application.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations
{
    public class PermissionReadModelConfiguration : IEntityTypeConfiguration<PermissionReadModel>
    {
        public void Configure(EntityTypeBuilder<PermissionReadModel> builder)
        {
            builder
                .ToTable(t => t.IsTemporal());

            builder
                .HasKey(x => new { x.UserGuid, x.ActionType });
        }
    }
}