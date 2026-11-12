using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Persistence.Configurations;

internal static class EnumLookupBuilderExtensions
{
    /// <summary>
    /// Makes the enum column a foreign key to its reference table. Restrict, so a member cannot be removed while
    /// rows still use it.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasEnumForeignKey<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TEnum>> property)
        where TEntity : class
        where TEnum : struct, Enum
    {
        builder
        .HasOne<EnumLookup<TEnum>>()
        .WithMany()
        .HasForeignKey(((MemberExpression)property.Body).Member.Name)
        .OnDelete(DeleteBehavior.Restrict);

        return builder;
    }
}