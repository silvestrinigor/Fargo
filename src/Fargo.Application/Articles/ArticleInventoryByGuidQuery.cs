using Fargo.Application.Common;

namespace Fargo.Application.Articles;

public sealed record ArticleInventoryByGuidQuery(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid>? InsideItemContainerGuids = null,
    bool IncludeDescendents = true
) : IQuery<ArticleInventoryDto?>;
