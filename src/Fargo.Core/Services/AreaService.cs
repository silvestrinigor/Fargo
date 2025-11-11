using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Services;

public class AreaService(IAreaRepository areaRepository)
{
    private readonly IAreaRepository areaRepository = areaRepository;

    public async Task InsertEntityIntoArea(Area area, Entity entity)
    {
        var fromArea = await areaRepository.GetEntityArea(entity.Guid);
        fromArea?.entities.Remove(entity);
        area.entities.Add(entity);
    }
    public static void RemoveEntityFromArea(Area area, Entity entity)
    {
        area.entities.Remove(entity);
    }
}
