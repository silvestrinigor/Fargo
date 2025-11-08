using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Dtos;

namespace Fargo.Infrastructure.ExternalServices.Fargo;

public class ContainerHttpClientService(HttpClient httpClient) : IContainerHttpClientService
{
    private readonly HttpClient httpClient = httpClient;

    public Task<EntityDto> CreateContainerAsync(EntityDto articleCreateDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteContainerAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<EntityDto?> GetContainerAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EntityDto>> GetContainerAsync()
    {
        throw new NotImplementedException();
    }

    public Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task UpdateContainerAsync(EntityDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
