namespace Fargo.Application.Common.Paging;

/// <param name="TotalCount">Matches across all pages, ignoring pagination.</param>
/// <param name="Page">1-based.</param>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int Limit)
{
    public int TotalPages =>
        Limit <= 0 || TotalCount <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)Limit);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1 && TotalPages > 0;
}
