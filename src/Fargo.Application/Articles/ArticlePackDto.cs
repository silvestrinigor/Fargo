using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticlePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);
