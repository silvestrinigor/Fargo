using Fargo.Application.Common.Exceptions;
using Fargo.Application.Common.Paging;
using System.Linq.Expressions;

namespace Fargo.Infrastructure.Persistence;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> WithPagination<TEntity>(
        this IQueryable<TEntity> query,
        Pagination pagination)
        => query
            .Skip(pagination.Skip)
            .Take(pagination.Take);

    /// <summary>
    /// Orders by a whitelisted <see cref="Sort"/>, always appending <paramref name="tiebreaker"/> so pages are
    /// deterministic. Orders by <paramref name="defaultKey"/> when <paramref name="sort"/> is
    /// <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="allowed"/> maps each permitted (lowercased) field name to its key selector. A field
    /// outside it throws <see cref="InvalidSortFieldFargoApplicationException"/> (surfaced as 400), so callers
    /// can never sort by an arbitrary or untranslatable column.
    /// </remarks>
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
