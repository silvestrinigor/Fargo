using Fargo.Application.Dtos;
using Fargo.Application.Interfaces.Http;

namespace Fargo.Infrastructure.Http.Fargo;

public class AreaHttpClientService : IAreaHttpClientService
{
    public Task<EntityDto> CreateAreaAsync(EntityCreateDto areaCreateDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAreaAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<EntityDto?> GetAreaAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EntityDto>> GetAreaAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EntityDto>> GetAreaEntitiesAsync(Guid areaGuid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Guid>> GetAreaGuidsAsync()
    {
        throw new NotImplementedException();
    }

    public Task InsertEntityIntoAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task RemoveEntityFromAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAreaAsync(Guid containerGuid, EntityUpdateDto areaUpdateDto)
    {
        throw new NotImplementedException();
    }
}
