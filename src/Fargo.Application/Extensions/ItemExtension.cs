using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Models;

namespace Fargo.Application.Extensions
{
    public static class ItemExtension
    {
        extension(Item item)
        {
            public ItemDto ToDto()
                => new(
                    item.Guid,
                    item.ArticleGuid
                    );
        }
    }
}
