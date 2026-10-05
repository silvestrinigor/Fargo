using UnitsNet;

namespace Fargo.Core.Articles;

public sealed record ArticleKitComponentInformation(
    Guid FromArticleGuid,
    Scalar Quantity
);
