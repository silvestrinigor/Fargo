using Fargo.Core.Entities.Items;
using System.Linq.Expressions;

namespace Fargo.Application.Dtos.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    Guid? ParentContainerGuid,
    bool IsFixed,
    IReadOnlyCollection<Guid> Partitions
);

public static class ItemDtoMappings
{
    public static readonly Expression<Func<Item, ItemDto>> Projection = item => new ItemDto(
        item.Guid,
        item.ArticleGuid,
        item.ParentItemContainerGuid,
        item.IsFixed,
        item.Partitions.Select(partition => partition.PartitionGuid).ToArray()
    );
}
