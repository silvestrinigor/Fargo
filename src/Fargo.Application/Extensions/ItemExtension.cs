using Fargo.Application.Dtos.ItemDtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class ItemExtension
    {
        extension(Item item)
        {
            public ItemDto ToDto()
                => new(
                    item.Guid,
                    item.ArticleGuid,
                    item.ParentItemGuid);
        }
    }
}
