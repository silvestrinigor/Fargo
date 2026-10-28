using System.Linq.Expressions;

namespace Fargo.Infrastructure.Repositories;

/// <summary>
/// Shared, EF-translatable partition visibility filter used by the resource
/// repositories. Centralizes the previously-duplicated
/// <c>ApplyPartitionFilter</c> logic: an entity is visible when any of its
/// partition links references a partition the actor can access.
/// </summary>
internal static class PartitionFilterExtensions
{
    /// <summary>
    /// Restricts <paramref name="query"/> to entities that have at least one
    /// partition link (selected by <paramref name="partitionLinks"/>) whose
    /// <see cref="Guid"/> (read by <paramref name="partitionGuid"/>) is in
    /// <paramref name="partitionGuids"/>. When <paramref name="partitionGuids"/>
    /// is <see langword="null"/> the query is returned unfiltered, matching the
    /// prior per-repository behavior.
    /// </summary>
    /// <remarks>
    /// The produced predicate is structurally identical to the hand-written form
    /// each repository used previously —
    /// <c>e =&gt; links(e).Any(p =&gt; partitionGuids.Contains(guid(p)))</c> —
    /// so EF Core translates it to the same SQL.
    /// </remarks>
    /// <typeparam name="TEntity">The queried entity type.</typeparam>
    /// <typeparam name="TLink">The partition-link element type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="partitionGuids">
    /// The partition GUIDs the actor can access, or <see langword="null"/> to
    /// apply no filter.
    /// </param>
    /// <param name="partitionLinks">
    /// Selects an entity's partition-link collection (for example
    /// <c>article =&gt; article.Partitions</c>).
    /// </param>
    /// <param name="partitionGuid">
    /// Reads the partition GUID from a link (for example
    /// <c>partition =&gt; partition.PartitionGuid</c>).
    /// </param>
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

        // Build: p => partitionGuids.Contains(partitionGuid(p))
        var link = partitionGuid.Parameters[0];

        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [typeof(Guid)],
            Expression.Constant(partitionGuids, typeof(IEnumerable<Guid>)),
            partitionGuid.Body);

        var linkPredicate = Expression.Lambda<Func<TLink, bool>>(contains, link);

        // Build: e => partitionLinks(e).Any(linkPredicate)
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
