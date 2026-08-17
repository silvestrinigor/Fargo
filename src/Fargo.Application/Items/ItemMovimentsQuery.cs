using Fargo.Application.Common;

namespace Fargo.Application.Items;

public sealed record ItemMovimentsQuery(
    Guid ItemGuid
) : IQuery<IReadOnlyCollection<ItemMovimentDto>?>;
