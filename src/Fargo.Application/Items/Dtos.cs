using Fargo.Core.Items;
using System.Linq.Expressions;

namespace Fargo.Application.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate,
    Guid? ParentContainerGuid,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid
);

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null
);

public static class ItemDtoMappings
{
    public static readonly Expression<Func<Item, ItemDto>> Projection = item => new ItemDto(
        item.Guid,
        item.ArticleGuid,
        item.ProductionDate,
        item.ParentContainerGuid,
        item.Partitions.Select(partition => partition.Guid).ToArray(),
        item.EditedByGuid);
}
