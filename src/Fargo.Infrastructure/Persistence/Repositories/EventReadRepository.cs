using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EventReadRepository(FargoContext context) : IEventReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Event>> GetAllEventsFromModel(Guid? modelGuid = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default)
        {
            var query = context.Events.AsQueryable();

            if (skip != null)
            {
                query = query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = query.Take(take.Value);
            }

            return await query
                .AsNoTracking()
                .Where(e => modelGuid == null || e.ModelGuid == modelGuid)
                .ToListAsync(cancellationToken);
        }

        public async Task<Event?> GetEventByGuid(Guid eventGuid, CancellationToken cancellationToken = default)
            => await context.Events
                .AsNoTracking()
                .Where(x => x.Guid == eventGuid)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
