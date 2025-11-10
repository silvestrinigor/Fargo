using Fargo.Application.Dtos;
using Fargo.Core.Entities;

namespace Fargo.Application.Extensions;

public static class ContainerExtension
{
    public static ContainerDto ToContainerDto(this Container entity)
    {
        return new ContainerDto()
        {
            Name = entity.Name,
            Description = entity.Description,
            Guid = entity.Guid,
            CreatedAt = entity.CreatedAt,
        };
    }
}
