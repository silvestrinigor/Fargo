using Fargo.Domain.Entities.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.OwnsOne(x => x.Mass, mass =>
        {
            mass.Property("")
                .HasColumnName("Mass_Value")
                .HasColumnType("decimal(38,18)");

            mass.Property(m => m.Unit)
                .HasColumnName("Mass_Unit")
                .HasConversion(
                    u => u.ToString(),
                    u => Enum.Parse<MassUnit>(u)
                );
        });
    }
}
