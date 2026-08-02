using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticlePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);