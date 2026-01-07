using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EventReadRepository(FargoContext context) : IEventReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Event>> GetAllEventsFromModel(Guid? modelGuid, CancellationToken cancellationToken = default)
        {
            var query = context.Events
                .AsQueryable()
                .AsNoTracking();

            if (modelGuid is not null)
            {
                query = query.Where(e => e.ModelGuid == modelGuid);
            }

            return await context.Events
                .ToListAsync(cancellationToken);
        }

        public async Task<Event?> GetEventByGuid(Guid eventGuid, CancellationToken cancellationToken = default)
        {
            return await context.Events
                .AsNoTracking()
                .Where(x => x.Guid == eventGuid)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
