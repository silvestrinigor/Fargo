using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EventReadRepository(FargoContext context) : IEventReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Event>> GetAllEventsFromModel(Guid? modelGuid, CancellationToken cancellationToken = default)
            => await context.Events
                .AsNoTracking()
                .Where(e => modelGuid == null || e.ModelGuid == modelGuid)
                .ToListAsync(cancellationToken);

        public async Task<Event?> GetEventByGuid(Guid eventGuid, CancellationToken cancellationToken = default)
            => await context.Events
                .AsNoTracking()
                .Where(x => x.Guid == eventGuid)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
