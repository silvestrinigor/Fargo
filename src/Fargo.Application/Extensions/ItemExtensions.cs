using Fargo.Application.Dtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class ItemExtensions
    {
        extension(Item item)
        {
            public ItemDto ToDto()
            {
                return new ItemDto(
                    item.Guid,
                    item.ArticleGuid,
                    item.CreatedAt
                    );
            }
        }
    }
}
