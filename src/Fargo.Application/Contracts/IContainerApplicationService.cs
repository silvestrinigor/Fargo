using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts;

public interface IContainerApplicationService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<IEnumerable<Guid>> GetContainersGuidsAsync();
    Task<EntityDto> CreateContainerAsync(EntityCreateDto articleCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto);
    Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid);
    Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid);
    Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid);
}
