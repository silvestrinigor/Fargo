using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Contracts;

public interface IAreaRepository : IEntityRepository<Area>
{
    Task<bool> AnyAsync();
    Task<IEnumerable<Entity>> GetAreaEntities(Guid area);
    Task AddEntityIntoAreaAsync(Area areaGuid, Entity entityGuid);
    Task RemoveEntityFromAreaAsync(Area areaGuid, Entity entityGuid);
}
