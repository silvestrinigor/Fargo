using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EventRepository(FargoContext context) : IEventRepository
    {
        private readonly FargoContext context = context;

        public void Add(Event @event)
        {
            context.Events.Add(@event);
        }

        public void Remove(Event @event)
        {
            context.Events.Remove(@event);
        }
    }
}
