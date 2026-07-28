using Fargo.Application.Shared.Items;

namespace Fargo.Application.Items;

public sealed record ItemSingleQuery(
    Guid ItemGuid) : IQuery<ItemDto?>;
