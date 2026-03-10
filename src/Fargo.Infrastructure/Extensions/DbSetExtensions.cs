using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Extensions
{
    public static class DbSetExtensions
    {
        public static IQueryable<TEntity> TemporalAsOfIfDateTimeNotNull<TEntity>(
                this DbSet<TEntity> dbSet,
                DateTimeOffset? dateTime
                ) where TEntity : class
            => dateTime is not null
            ? dbSet.TemporalAsOf(dateTime.Value.UtcDateTime)
            : dbSet.AsQueryable();
    }
}