using System.Linq.Expressions;
using Fargo.Application.Common;
using Fargo.Infrastructure.Extensions;

namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Provides extension methods for applying pagination to LINQ queries.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to the specified query using the provided <see cref="Pagination"/> parameters.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of entity contained in the query.
    /// </typeparam>
    /// <param name="query">
    /// The query to which pagination will be applied.
    /// </param>
    /// <param name="pagination">
    /// The pagination parameters defining how many records to skip
    /// and how many records to retrieve.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> that applies the corresponding
    /// <see cref="Queryable.Skip{TSource}(IQueryable{TSource}, int)"/> and
    /// <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)"/> operations.
    /// </returns>
    /// <remarks>
    /// This helper simplifies applying pagination logic to database queries.
    /// The <see cref="Pagination"/> value object calculates the <c>Skip</c> and
    /// <c>Take</c> values internally based on the current page and limit.
    ///
    /// Because the method operates on <see cref="IQueryable{T}"/>, the pagination
    /// will be translated to the underlying query provider (such as SQL when using
    /// Entity Framework).
    /// </remarks>
    public static IQueryable<TEntity> WithPagination<TEntity>(
        this IQueryable<TEntity> query,
        Pagination pagination)
        => query
            .Skip(pagination.Skip)
            .Take(pagination.Take);

    /// <summary>
    /// Applies a whitelisted <see cref="Sort"/> to the query, always appending a
    /// stable <paramref name="tiebreaker"/> (typically the GUID key) so pages are
    /// deterministic even when the sort key has duplicate values. When
    /// <paramref name="sort"/> is <see langword="null"/> the query is ordered by
    /// <paramref name="defaultKey"/> then the tiebreaker, preserving the previous
    /// default ordering.
    /// </summary>
    /// <param name="query">The source query to order.</param>
    /// <param name="sort">The requested sort, or <see langword="null"/> for the default order.</param>
    /// <param name="allowed">
    /// Maps each permitted (lowercased) sort-field name to its key selector. A
    /// requested field outside this set throws
    /// <see cref="InvalidSortFieldFargoApplicationException"/> (surfaced as 400),
    /// so callers can never sort by an arbitrary or untranslatable column.
    /// </param>
    /// <param name="defaultKey">The key to order by when <paramref name="sort"/> is null.</param>
    /// <param name="tiebreaker">A stable key appended to every order for deterministic paging.</param>
    public static IOrderedQueryable<TEntity> ApplySort<TEntity, TDefaultKey, TTieKey>(
        this IQueryable<TEntity> query,
        Sort? sort,
        IReadOnlyDictionary<string, Expression<Func<TEntity, object?>>> allowed,
        Expression<Func<TEntity, TDefaultKey>> defaultKey,
        Expression<Func<TEntity, TTieKey>> tiebreaker)
    {
        if (sort is not { } requested)
        {
            return query.OrderBy(defaultKey).ThenBy(tiebreaker);
        }

        if (!allowed.TryGetValue(requested.Field, out var keySelector))
        {
            throw new InvalidSortFieldFargoApplicationException(requested.Field, allowed.Keys);
        }

        var ordered = requested.Direction == SortDirection.Descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);

        return ordered.ThenBy(tiebreaker);
    }
}
