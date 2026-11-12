using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

public sealed record ArticleKitComponentDto(
    Guid ArticleGuid,
    Scalar Quantity
);
