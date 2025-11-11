using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts.Http;

public interface IAreaHttpClientService
{
    Task<EntityDto?> GetAreaAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetAreaAsync();
    Task<IEnumerable<Guid>> GetAreaGuidsAsync();
    Task<EntityDto> CreateAreaAsync(EntityCreateDto areaCreateDto);
    Task DeleteAreaAsync(Guid guid);
    Task UpdateAreaAsync(Guid containerGuid, EntityUpdateDto areaUpdateDto);
    Task<IEnumerable<EntityDto>> GetAreaEntitiesAsync(Guid areaGuid);
    Task InsertEntityIntoAreaAsync(Guid areaGuid, Guid entityGuid);
    Task RemoveEntityFromAreaAsync(Guid areaGuid, Guid entityGuid);
}