using Fargo.Application.Shared.Items;
using Fargo.Core.Items;
using System.Linq.Expressions;

namespace Fargo.Application.Items;

public static class ItemDtoMappings
{
    public static readonly Expression<Func<Item, ItemDto>> Projection = item => new ItemDto(
        item.Guid,
        item.ArticleGuid,
        item.ParentItemContainerGuid,
        item.IsFixed,
        item.Partitions.Select(partition => partition.PartitionGuid).ToArray());
}
