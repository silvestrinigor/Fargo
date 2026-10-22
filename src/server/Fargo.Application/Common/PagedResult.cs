namespace Fargo.Application.Common;

/// <summary>
/// A page of query results together with the paging metadata needed by clients
/// to drive pagination (for example, to know whether more pages exist).
/// </summary>
/// <typeparam name="T">The type of the items in the page.</typeparam>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">
/// The total number of items across all pages that match the query, ignoring
/// pagination.
/// </param>
/// <param name="Page">The 1-based index of the current page.</param>
/// <param name="Limit">The maximum number of items per page.</param>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int Limit)
{
    /// <summary>
    /// Gets the total number of pages for the current <see cref="Limit"/>.
    /// A query that matches no items has zero pages.
    /// </summary>
    public int TotalPages =>
        Limit <= 0 || TotalCount <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)Limit);

    /// <summary>
    /// Gets a value indicating whether a page after the current one exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether a page before the current one exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1 && TotalPages > 0;
}
