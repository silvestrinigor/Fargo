using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Contracts;

public interface IAreaRepository : IEntityRepository<Area>
{
    Task<Area?> GetEntityArea(Guid guid);
    Task<IEnumerable<Entity>> GetAreaEntities(Guid areaGuid);
}
