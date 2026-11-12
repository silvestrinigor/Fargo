using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

public sealed record ArticleKitComponentDto(
    Guid ArticleGuid,
    Scalar Quantity
);
