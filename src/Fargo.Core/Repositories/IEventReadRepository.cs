using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IEventReadRepository
    {
        Task<IEnumerable<Event>> GetAllEventsFromEntity(Guid entityGuid, CancellationToken cancellationToken = default);
    }
}
