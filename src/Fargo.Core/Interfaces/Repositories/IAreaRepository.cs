using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories;

public interface IAreaRepository
{
    Task<Area?> GetAsync(Guid guid);
    Task<IEnumerable<Area>> GetAsync();
    Task<IEnumerable<Guid>> GetGuidsAsync();
    Task AddAsync(Area entity);
    Task RemoveAsync(Area entity);
    Task<bool> AnyAsync();
    Task<IEnumerable<NamedEntity>> GetAreaEntitiesAsync(Guid area);
    Task AddEntityIntoAreaAsync(Area areaGuid, NamedEntity entityGuid);
    Task RemoveEntityFromAreaAsync(Area areaGuid, NamedEntity entityGuid);
}
