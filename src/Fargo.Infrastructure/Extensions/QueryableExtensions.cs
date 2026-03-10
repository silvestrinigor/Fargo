using Fargo.Application.Common;

namespace Fargo.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> WithPagination<TEntity>(
                this IQueryable<TEntity> query,
                Pagination pagination
                ) where TEntity : class
            => query
            .Skip(pagination.Skip)
            .Take(pagination.Take);
    }
}