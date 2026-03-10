using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for working with temporal queries in Entity Framework.
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Applies a temporal <c>AS OF</c> query when the provided date and time is not <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity contained in the <see cref="DbSet{TEntity}"/>.
        /// </typeparam>
        /// <param name="dbSet">
        /// The <see cref="DbSet{TEntity}"/> used to build the query.
        /// </param>
        /// <param name="dateTime">
        /// The point in time used to retrieve historical data.
        /// If <see langword="null"/>, the current table state is queried instead.
        /// </param>
        /// <returns>
        /// An <see cref="IQueryable{T}"/> representing either a temporal query
        /// at the specified point in time or the normal queryable set when
        /// no temporal reference is provided.
        /// </returns>
        /// <remarks>
        /// When <paramref name="dateTime"/> is provided, this method uses
        /// <see cref="RelationalQueryableExtensions.TemporalAsOf{TEntity}(IQueryable{TEntity}, DateTime)"/>
        /// to retrieve the entity state as it existed at the specified moment.
        ///
        /// When <paramref name="dateTime"/> is <see langword="null"/>,
        /// the method simply returns the current <see cref="DbSet{TEntity}"/> queryable.
        ///
        /// The provided <see cref="DateTimeOffset"/> value is converted to UTC
        /// before being passed to the temporal query.
        /// </remarks>
        public static IQueryable<TEntity> TemporalAsOfIfProvided<TEntity>(
                this DbSet<TEntity> dbSet,
                DateTimeOffset? dateTime
                ) where TEntity : class
            => dateTime is not null
            ? dbSet.TemporalAsOf(dateTime.Value.UtcDateTime)
            : dbSet.AsQueryable();
    }
}