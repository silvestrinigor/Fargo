using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts.Http;

public interface IContainerHttpClientService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<IEnumerable<Guid>> GetContainersGuidsAsync();
    Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid);
    Task<EntityDto> CreateContainerAsync(EntityCreateDto containerCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto containerUpdateDto);
    Task InsertEntityIntoContainerAsync(Guid containerGuid, Guid entityGuid);
    Task RemoveEntityFromContainerAsync(Guid containerGuid, Guid entityGuid);
}