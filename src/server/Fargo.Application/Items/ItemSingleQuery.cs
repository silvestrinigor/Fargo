using Fargo.Application.Common;

namespace Fargo.Application.Items;

public sealed record ItemSingleQuery(Guid ItemGuid) : IQuery<ItemDto?>;
