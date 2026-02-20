using Fargo.Application.Commom;

namespace Fargo.Infrastructure.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<TEntity> WithPagination<TEntity>(
                this IQueryable<TEntity> query,
                Pagination pagination
                ) where TEntity : class
            => query
            .Skip(pagination.Skip)
            .Take(pagination.Limit);
    }
}