using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class UserReadRepository(FargoContext context) : IUserReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<User>> GetAllAsync(
            DateTime? atDateTime = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default)
        {
            var query = atDateTime is not null
                ? context.Users.TemporalAsOf(atDateTime.Value)
                : context.Users.AsQueryable();

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query
                .AsNoTracking()
                .Include(x => x.Permissions)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetByGuidAsync(
            Guid userGuid,
            DateTime? atDateTime = null,
            CancellationToken cancellationToken = default)
        {
            var query = atDateTime is not null
                ? context.Users.TemporalAsOf(atDateTime.Value)
                : context.Users;

            return await query
                .AsNoTracking()
                .Include(x => x.Permissions)
                .Where(x => x.Guid == userGuid)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
