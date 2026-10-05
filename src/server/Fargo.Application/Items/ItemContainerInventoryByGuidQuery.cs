using Fargo.Application.Common;

namespace Fargo.Application.Items;

public sealed record ItemContainerInventoryByGuidQuery(
    Guid ItemContainerGuid,
    IReadOnlyCollection<Guid>? ArticleGuids = null,
    bool IncludeDescendents = true
) : IQuery<IReadOnlyCollection<ItemContainerInventoryDto>?>;
