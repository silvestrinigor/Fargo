using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Articles.Dtos;

namespace Fargo.Application.Operations.Articles;

public sealed record ArticleByGuidQuery(Guid ArticleGuid) : IQuery<ArticleDto?>;

