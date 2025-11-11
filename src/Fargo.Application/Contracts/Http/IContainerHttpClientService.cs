using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts.Http;

public interface IContainerHttpClientService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<IEnumerable<Guid>> GetContainersGuidsAsync();
    Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid);
    Task<EntityDto> CreateContainerAsync(EntityCreateDto articleCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto);
    Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid);
    Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid);
}