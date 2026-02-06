using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Extensions
{
    public static class DbSetExtension
    {
        public static IQueryable<TEntity> TemporalAsOfIfDateTimeNotNull<TEntity>(
                this DbSet<TEntity> dbSet,
                DateTime? dateTime
                ) where TEntity : class
            => dateTime is not null
            ? dbSet.TemporalAsOf(dateTime.Value)
            : dbSet.AsQueryable();
    }
}