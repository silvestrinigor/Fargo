using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Articles.Dtos;

namespace Fargo.Application.Operations.Articles;

/// <summary>
/// Query used to retrieve an article by identifier.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleByGuidQuery(Guid ArticleGuid) : IQuery<ArticleDto?>;

