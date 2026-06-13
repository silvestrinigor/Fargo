using Fargo.Application.Shared.Items;
using Fargo.Core.Items;
using System.Linq.Expressions;

namespace Fargo.Application.Items;

public static class ItemDtoMappings
{
    public static readonly Expression<Func<Item, ItemDto>> Projection = item => new ItemDto(
        item.Guid,
        item.ArticleGuid,
        item.ProductionDate,
        item.ParentContainerGuid,
        item.Partitions.Select(partition => partition.Guid).ToArray(),
        item.IsActive,
        item.EditedByActorid);
}
