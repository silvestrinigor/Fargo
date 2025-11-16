using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Persistence;
using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Enums;
using Fargo.Core.Services;

namespace Fargo.Application.Services;

public class AreaApplicationService(AreaService areaService, IEntityMainRepository entityMainRepository, IAreaRepository areaRepository, IUnitOfWork unitOfWork) : IAreaApplicationService
{
    private readonly AreaService areaService = areaService;
    private readonly IEntityMainRepository entityMainRepository = entityMainRepository;
    private readonly IAreaRepository areaRepository = areaRepository;
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task<EntityDto> CreateAreaAsync(EntityCreateDto areaCreateDto)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(areaCreateDto.Name);

        var area = new Area { Name = areaCreateDto.Name, EntityType = EEntityType.Area };

        areaRepository.Add(area);

        await unitOfWork.SaveChangesAsync();

        return area.ToEntityDto();
    }

    public async Task DeleteAreaAsync(Guid guid)
    {
        var area = await areaRepository.GetAsync(guid);

        ArgumentNullException.ThrowIfNull(area);

        areaRepository.Remove(area);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<EntityDto?> GetAreaAsync(Guid guid)
    {
        var area = await areaRepository.GetAsync(guid);

        return area?.ToEntityDto();
    }

    public async Task<IEnumerable<EntityDto>> GetAreaAsync()
    {
        var articles = await areaRepository.GetAsync();

        return articles.Select(article => article.ToEntityDto());
    }

    public async Task<IEnumerable<EntityDto>> GetAreaEntitiesAsync(Guid areaGuid)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Guid>> GetAreaGuidsAsync()
    {
        var articles = await areaRepository.GetGuidsAsync();

        return articles;
    }

    public async Task AddEntityIntoAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        var area = await areaRepository.GetAsync(areaGuid);
        ArgumentNullException.ThrowIfNull(area);

        var entity = await entityMainRepository.GetAsync(entityGuid);
        ArgumentNullException.ThrowIfNull(entity);

        await areaService.AddEntityIntoAreaAsync(area, entity);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveEntityFromAreaAsync(Guid areaGuid, Guid entityGuid)
    {
        var area = await areaRepository.GetAsync(areaGuid);
        ArgumentNullException.ThrowIfNull(area);

        var entity = await entityMainRepository.GetAsync(entityGuid);
        ArgumentNullException.ThrowIfNull(entity);

        await areaService.RemoveEntityFromAreaAsync(area, entity);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAreaAsync(Guid areaGuid, EntityUpdateDto areaUpdateDto)
    {
        var area = await areaRepository.GetAsync(areaGuid);

        ArgumentNullException.ThrowIfNull(area);

        area.UpdateEntityProperties(areaUpdateDto);

        await unitOfWork.SaveChangesAsync();
    }
}
