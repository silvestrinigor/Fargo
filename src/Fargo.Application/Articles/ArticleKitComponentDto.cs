using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticleKitComponentDto(Guid ArticleGuid, Scalar Quantity);
