using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Contracts;

public interface IContainerRepository : IEntityRepository<Container>
{
    Task<Container?> GetEntityContainer(Guid guid);
    Task<IEnumerable<Entity>> GetContainerEntities(Guid containerGuid);
}
