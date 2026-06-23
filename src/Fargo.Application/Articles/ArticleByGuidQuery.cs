using Fargo.Application.Shared.Articles;

namespace Fargo.Application.Articles.Queries;

/// <summary>
/// Query used to retrieve an article by identifier.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="AsOfDateTime">
/// Temporal query date.
/// </param>
public sealed record ArticleByGuidQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;
