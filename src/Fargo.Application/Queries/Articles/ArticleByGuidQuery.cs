using Fargo.Application.Abstractions;
using Fargo.Application.Dtos.Articles;

namespace Fargo.Application.Queries.Articles;

public sealed record ArticleByGuidQuery(Guid ArticleGuid) : IQuery<ArticleDto?>;

