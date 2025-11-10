using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.Http.Fargo;

public class ContainerHttpClientService(HttpClient httpClient) : IContainerHttpClientService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EntityDto> CreateContainerAsync(EntityCreateDto entityDto)
    {
        var response = await httpClient.PostAsJsonAsync("/containers", entityDto);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EntityDto>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
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

    public Task<IEnumerable<Guid>> GetContainersGuidsAsync()
    {
        throw new NotImplementedException();
    }

    public Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
