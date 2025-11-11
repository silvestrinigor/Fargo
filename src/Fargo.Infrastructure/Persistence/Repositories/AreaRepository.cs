using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class AreaRepository(FargoContext fargoContext) : IAreaRepository
{
    private readonly FargoContext fargoContext = fargoContext;

    public async Task<Area?> GetAsync(Guid guid)
        => await fargoContext.Areas.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Area>> GetAsync()
        => await fargoContext.Areas.ToListAsync();

    public async Task<Area?> GetEntityArea(Guid entityGuid)
        => await fargoContext.Areas.Where(c => c.Entities.Any(e => e.Guid == entityGuid)).FirstOrDefaultAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.Areas.Select(x => x.Guid).ToListAsync();

    public void Remove(Area container)
        => fargoContext.Areas.Remove(container);

    public void Add(Area container)
        => fargoContext.Areas.Add(container);

    public async Task<IEnumerable<Entity>> GetAreaEntities(Guid areaGuid)
        => await fargoContext.Areas
            .Where(x => x.Guid == areaGuid)
            .SelectMany(x => x.Entities)
            .ToListAsync();
}

