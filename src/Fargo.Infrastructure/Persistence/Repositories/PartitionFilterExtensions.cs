using System.Linq.Expressions;

namespace Fargo.Infrastructure.Persistence.Repositories;

internal static class PartitionFilterExtensions
{
    /// <summary>
    /// A <see langword="null"/> <paramref name="partitionGuids"/> returns the query unfiltered.
    /// </summary>
    public static IQueryable<TEntity> WherePartitionAccessible<TEntity, TLink>(
        this IQueryable<TEntity> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        Expression<Func<TEntity, IEnumerable<TLink>>> partitionLinks,
        Expression<Func<TLink, Guid>> partitionGuid)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        var link = partitionGuid.Parameters[0];

        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [typeof(Guid)],
            Expression.Constant(partitionGuids, typeof(IEnumerable<Guid>)),
            partitionGuid.Body);

        var linkPredicate = Expression.Lambda<Func<TLink, bool>>(contains, link);

        var entity = partitionLinks.Parameters[0];

        var any = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Any),
            [typeof(TLink)],
            partitionLinks.Body,
            linkPredicate);

        var predicate = Expression.Lambda<Func<TEntity, bool>>(any, entity);

        return query.Where(predicate);
    }
}
