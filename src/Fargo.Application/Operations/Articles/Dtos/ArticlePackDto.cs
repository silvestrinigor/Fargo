using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

public sealed record ArticlePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);
