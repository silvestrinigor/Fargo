using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IContainerRepository
    {
        Task<Container?> GetByGuidAsync(Guid guid);
        void Add(Container container);
        void Remove(Container container);
        Task<IEnumerable<Guid>> GetChildEntitiesGuidsAsync(Guid containerGuid);
        Task<bool> HasChildEntitiesAsync(Guid containerGuid);
    }
}
