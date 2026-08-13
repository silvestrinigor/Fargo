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
public sealed record ArticlesQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;
