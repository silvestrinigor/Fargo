using Fargo.Application.Common;
using Fargo.Application.Shared.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Query used to retrieve an article by identifier.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleByGuidQuery(Guid ArticleGuid) : IQuery<ArticleDto?>;
