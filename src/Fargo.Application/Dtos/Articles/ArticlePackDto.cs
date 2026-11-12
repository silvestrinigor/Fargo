using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

public sealed record ArticlePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);
