using Fargo.Domain.ClientApplications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class ApiClientConfiguration : IEntityTypeConfiguration<ClientApplication>
{
    public void Configure(EntityTypeBuilder<ClientApplication> builder)
    {
        builder.ToTable("ApiClients", t => t.IsTemporal());
        builder.HasKey(x => x.Guid);
        builder.Property(x => x.Guid).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.KeyHash).IsUnique();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.EditedByGuid).IsRequired(false);
    }
}
