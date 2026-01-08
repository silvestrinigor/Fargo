using Fargo.Domain.Entities.Events.Abstracts;

namespace Fargo.Domain.Repositories
{
    public interface IEventReadRepository
    {
        Task<IEnumerable<Event>> GetAllEventsFromModel(Guid? modelGuid = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default);

        Task<Event?> GetEventByGuid(Guid eventGuid, CancellationToken cancellationToken = default);
    }
}
