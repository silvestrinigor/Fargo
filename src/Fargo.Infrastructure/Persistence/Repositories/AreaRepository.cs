using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class AreaRepository(FargoContext fargoContext) : IAreaRepository
{
    private readonly FargoContext fargoContext = fargoContext;
    
    public async Task<bool> AnyAsync()
        => await fargoContext.Areas.AnyAsync();

    public async Task<Area?> GetAsync(Guid guid)
        => await fargoContext.Areas.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Area>> GetAsync()
        => await fargoContext.Areas.ToListAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.Areas.Select(x => x.Guid).ToListAsync();

    public void Remove(Area container)
    {
        fargoContext.Areas.Remove(container);
    }

    public void Add(Area container)
    {
        fargoContext.Areas.Add(container);

        if (container.IsGlobalArea)
            return;

        var globalArea = fargoContext.Areas.First(x => x.IsGlobalArea);        

        fargoContext.AreaClosure.Add(new AreaClosure
        {
            AncestorGuid = container.Guid,
            DescendantGuid = container.Guid,
            Depth = 0
        });

        var ancestors = fargoContext.AreaClosure
            .Where(x => x.DescendantGuid == globalArea.Guid)
            .ToList();

        foreach (var ancestor in ancestors)
        {
            fargoContext.AreaClosure.Add(new AreaClosure
            {
                AncestorGuid = ancestor.AncestorGuid,
                DescendantGuid = container.Guid,
                Depth = ancestor.Depth + 1
            });
        }
    }

    public async Task<IEnumerable<Entity>> GetAreaEntities(Guid areaGuid)
        => await fargoContext.AreaClosure.Where(x => x.AncestorGuid != null && x.AncestorGuid == areaGuid)
            .Join(fargoContext.Entities,
                closure => closure.DescendantGuid,
                entity => entity.Guid,
                (closure, entity) => entity)
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

