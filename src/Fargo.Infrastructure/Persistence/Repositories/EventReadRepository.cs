using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EventReadRepository(FargoContext context) : IEventReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Event>> GetAllEventsFromEntity(Guid entityGuid, CancellationToken cancellationToken = default)
        {
            return await context.Events
                .AsNoTracking()
                .Where(e => e.RelatedEntityGuid == entityGuid)
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
