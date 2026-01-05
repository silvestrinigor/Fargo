using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IEventRepository
    {
        void Add(Event @event);

        void Remove(Event @event);
    }
}
