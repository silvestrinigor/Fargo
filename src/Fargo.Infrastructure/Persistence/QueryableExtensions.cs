using Fargo.Application;

namespace Fargo.Infrastructure.Extensions;

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
            Pagination pagination
            )
        => query
            .Skip(pagination.Skip)
            .Take(pagination.Take);
}
