using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleKitComponentDto(Guid ArticleGuid, Scalar Quantity);
