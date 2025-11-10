using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Infrastructure.Http.Fargo;

public class ContainerHttpClientService(HttpClient httpClient) : IContainerHttpClientService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<EntityDto> CreateContainerAsync(EntityCreateDto entityDto)
    {
        var response = await httpClient.PostAsJsonAsync("/containers", entityDto);

        var content = await response.Content.ReadAsStringAsync();

        var containerResponseDto = JsonSerializer.Deserialize<EntityDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        ArgumentNullException.ThrowIfNull(containerResponseDto);

        return containerResponseDto;
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
