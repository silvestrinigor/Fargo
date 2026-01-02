using Fargo.Application.Dtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class ItemDtoExtensions
    {
        extension(Item item)
        {
            public ItemDto ToDto()
            {
                return new ItemDto(
                    item.Guid,
                    item.Name,
                    item.Description
                    );
            }
        }
    }
}
