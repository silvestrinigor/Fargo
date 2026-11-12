using UnitsNet;

namespace Fargo.Core.Entities.Articles;

public sealed record ArticleKitComponentInformation(
    Guid FromArticleGuid,
    Scalar Quantity
);
