using Fargo.Domain.Entities.Events.Abstracts;

namespace Fargo.Domain.Repositories
{
    public interface IEventReadRepository
    {
        Task<IEnumerable<Event>> GetAllEventsFromModel(Guid? modelGuid, CancellationToken cancellationToken = default);

        Task<Event?> GetEventByGuid(Guid eventGuid, CancellationToken cancellationToken = default);
    }
}
