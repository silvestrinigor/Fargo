using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class AreaRepository(FargoContext fargoContext) : IAreaRepository
{
    private readonly FargoContext fargoContext = fargoContext;
    
    public async Task<bool> AnyAsync() => 
        await fargoContext.Areas.AnyAsync();

    public async Task<Area?> GetAsync(Guid guid) =>
        await fargoContext.Areas.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Area>> GetAsync() =>
        await fargoContext.Areas.ToListAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync() => 
        await fargoContext.Areas.Select(x => x.Guid).ToListAsync();

    public void Remove(Area area)
    {
        fargoContext.Areas.Remove(area);
    }

    public void Add(Area area)
    {
        fargoContext.Areas.Add(area);

        fargoContext.AreaClosure.Add(new AreaClosure
        {
            AncestorGuid = area.Guid,
            DescendantGuid = area.Guid,
            Depth = 0
        });

        if (area.IsGlobalArea)
            return;

        var globalArea = fargoContext.Areas.First(x => x.IsGlobalArea);

        var ancestors = fargoContext.AreaClosure
            .Where(x => x.DescendantGuid == globalArea.Guid)
            .ToList();

        foreach (var ancestor in ancestors)
        {
            fargoContext.AreaClosure.Add(new AreaClosure
            {
                AncestorGuid = ancestor.AncestorGuid,
                DescendantGuid = area.Guid,
                Depth = ancestor.Depth + 1
            });
        }
    }

    public async Task<IEnumerable<Entity>> GetAreaEntitiesAsync(Guid areaGuid)
        => await fargoContext.AreaClosure
            .Where(x => x.AncestorGuid == areaGuid && x.Depth > 0)
            .Join(
                fargoContext.Entities,
                closure => closure.DescendantGuid,
                entity => entity.Guid,
                (closure, entity) => entity
            )
            .ToListAsync();

    public async Task AddEntityIntoAreaAsync(Area areaGuid, Entity entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task RemoveEntityFromAreaAsync(Area areaGuid, Entity entityGuid)
    {
        throw new NotImplementedException();
    }
}

