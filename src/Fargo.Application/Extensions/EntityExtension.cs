using Fargo.Application.Dtos;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Application.Extensions;

public static class EntityExtension
{
    public static EntityDto ToEntityDto(this Entity entity)
    {
        return new EntityDto()
        {
            Name = entity.Name,
            Description = entity.Description,
            Guid = entity.Guid,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Entity UpdateEntityProperties(this Entity entity, EntityUpdateDto entityDto)
    {
        entity.Name = entityDto.Name ?? entity.Name;
        entity.Description = entityDto.Description ?? entity.Description;

        return entity;
    }
}
