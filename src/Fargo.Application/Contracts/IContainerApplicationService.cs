using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts;

public interface IContainerApplicationService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<IEnumerable<Guid>> GetContainersGuidsAsync();
    Task<EntityDto> CreateContainerAsync(EntityCreateDto containerCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto containerUpdateDto);
    Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid);
    Task InsertEntityIntoContainerAsync(Guid containerGuid, Guid entityGuid);
    Task RemoveEntityFromContainerAsync(Guid containerGuid, Guid entityGuid);
}
