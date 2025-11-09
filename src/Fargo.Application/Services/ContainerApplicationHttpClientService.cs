using Fargo.Application.Contracts;
using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Dtos;

namespace Fargo.Application.Services;

public class ContainerApplicationHttpClientService(IContainerHttpClientService containerHttpClientService) : IContainerApplicationService
{
    public async Task<EntityDto> CreateContainerAsync(EntityDto articleCreateDto)
    {
        return await containerHttpClientService.CreateContainerAsync(articleCreateDto);
    }

    public async Task DeleteContainerAsync(Guid guid)
    {
        await containerHttpClientService.DeleteContainerAsync(guid);
    }

    public async Task<EntityDto?> GetContainerAsync(Guid guid)
    {
        return await containerHttpClientService.GetContainerAsync(guid);
    }

    public async Task<IEnumerable<EntityDto>> GetContainerAsync()
    {
        return await containerHttpClientService.GetContainerAsync();
    }

    public async Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        await containerHttpClientService.InsertEntityIntoContainer(containerGuid, entityGuid);
    }

    public async Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid)
    {
        await containerHttpClientService.RemoveEntityFromContainer(containerGuid, entityGuid);
    }

    public async Task UpdateContainerAsync(EntityDto articleUpdateDto)
    {
        await containerHttpClientService.UpdateContainerAsync(articleUpdateDto);
    }
}
