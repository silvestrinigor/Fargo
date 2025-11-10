using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;

namespace Fargo.Application.Services;

public class ContainerApplicationHttpClientService(IContainerHttpClientService containerHttpClientService) : IContainerApplicationService
{
    public async Task<EntityDto> CreateContainerAsync(EntityCreateDto articleCreateDto)
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

    public async Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid)
    {
        return await containerHttpClientService.GetContainerEntitiesAsync(containerGuid);
    }

    public async Task<IEnumerable<Guid>> GetContainersGuidsAsync()
    {
        return await containerHttpClientService.GetContainersGuidsAsync();
    }

    public async Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        await containerHttpClientService.InsertEntityIntoContainer(containerGuid, entityGuid);
    }

    public async Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid)
    {
        await containerHttpClientService.RemoveEntityFromContainer(containerGuid, entityGuid);
    }

    public async Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto)
    {
        await containerHttpClientService.UpdateContainerAsync(containerGuid, articleUpdateDto);
    }
}
