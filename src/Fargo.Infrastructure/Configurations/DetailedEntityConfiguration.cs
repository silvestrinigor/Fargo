using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Infrastructure.Configurations;

public class DetailedEntityConfiguration : IEntityTypeConfiguration<DetailedEntity>
{
    public void Configure(EntityTypeBuilder<DetailedEntity> builder)
    {
        builder
            .HasKey(a => a.Guid);
        builder
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder
            .Property(a => a.CreatedAt)
            .IsRequired();
    }
}
