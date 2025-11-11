using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Core.Contracts;

public interface IAreaRepository : IEntityRepository<Area>
{
    Task<Area?> GetEntityArea(Guid guid);
    Task<IEnumerable<Entity>> GetAreaEntities(Guid areaGuid);
}
