using Fargo.Domain.Entities.Events.Abstracts;

namespace Fargo.Domain.Repositories
{
    public interface IEventRepository
    {
        void Add(Event @event);

        void Remove(Event @event);
    }
}
