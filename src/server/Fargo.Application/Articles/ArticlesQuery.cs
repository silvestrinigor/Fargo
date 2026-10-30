using Fargo.Application.Common;

namespace Fargo.Application.Articles;

/// <summary>
/// Query used to retrieve multiple articles.
/// </summary>
/// <param name="WithPagination">
/// Pagination configuration.
/// </param>
/// <param name="ChildOfAnyOfThesePartitions">
/// Filters articles inside the provided partitions.
/// </param>
/// <param name="Sort">Optional sort specification (e.g. <c>name</c> or <c>-name</c>).</param>
/// <param name="Search">Optional case-insensitive text to match against the article name or description.</param>
public sealed record ArticlesQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<ArticleDto>>;
