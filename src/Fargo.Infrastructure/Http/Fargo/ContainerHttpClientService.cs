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
        var response = await httpClient.PostAsJsonAsync("/containers", entityDto, jsonSerializerOptions);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EntityDto>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task DeleteContainerAsync(Guid guid)
    {
        var response = await httpClient.DeleteAsync($"/containers/{guid}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<EntityDto?> GetContainerAsync(Guid guid)
    {
        var response = await httpClient.GetAsync($"/containers/{guid}");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EntityDto?>(jsonSerializerOptions);
    }

    public async Task<IEnumerable<EntityDto>> GetContainerAsync()
    {
        var response = await httpClient.GetAsync($"/containers");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<EntityDto>>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid)
    {
        var response = await httpClient.GetAsync($"/containers/{containerGuid}/entities");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<EntityDto>>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task<IEnumerable<Guid>> GetContainersGuidsAsync()
    {
        var response = await httpClient.GetAsync($"/containers/guids");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<Guid>>(jsonSerializerOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");
    }

    public async Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        var response = await httpClient.PutAsync($"/containers/{containerGuid}/entities/{entityGuid}", null);

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid)
    {
        var response = await httpClient.DeleteAsync($"/containers/{containerGuid}/entities/{entityGuid}");

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto)
    {
        var response = await httpClient.PatchAsJsonAsync($"/containers/{containerGuid}", articleUpdateDto, jsonSerializerOptions);

        response.EnsureSuccessStatusCode();
    }
}
