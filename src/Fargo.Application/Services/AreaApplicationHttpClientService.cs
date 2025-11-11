using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Http;
using Fargo.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Application.Services;

public class AreaApplicationHttpClientService(IAreaHttpClientService areaHttpClientService) : IAreaApplicationService
{
    private readonly IAreaHttpClientService areaHttpClientService = areaHttpClientService;

    public async Task<EntityDto> CreateAreaAsync(EntityCreateDto areaCreateDto)
    {
        return await areaHttpClientService.CreateAreaAsync(areaCreateDto);
    }

    public async Task DeleteAreaAsync(Guid guid)
    {
        await areaHttpClientService.DeleteAreaAsync(guid);
    }

    public async Task<EntityDto?> GetAreaAsync(Guid guid)
    {
        return await areaHttpClientService.GetAreaAsync(guid);
    }

    public async Task<IEnumerable<EntityDto>> GetAreaAsync()
    {
        return await areaHttpClientService.GetAreaAsync();
    }

    public async Task<IEnumerable<EntityDto>> GetAreaEntitiesAsync(Guid areaGuid)
    {
        return await areaHttpClientService.GetAreaEntitiesAsync(areaGuid);
    }

    public async Task<IEnumerable<Guid>> GetAreaGuidsAsync()
    {
        return await areaHttpClientService.GetAreaGuidsAsync();
    }

    public async Task InsertEntityIntoAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        await areaHttpClientService.InsertEntityIntoAreaAsync(areaGuid, entityGuid);
    }

    public async Task RemoveEntityFromAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        await areaHttpClientService.RemoveEntityFromAreaAsync(areaGuid, entityGuid);
    }

    public async Task UpdateAreaAsync(Guid containerGuid, EntityUpdateDto areaUpdateDto)
    {
        await areaHttpClientService.UpdateAreaAsync(containerGuid, areaUpdateDto);
    }
}
