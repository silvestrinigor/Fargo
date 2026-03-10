using Fargo.Application.Common;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class UserQueries(FargoReadDbContext context) : IUserQueries
    {
        private readonly DbSet<UserReadModel> users = context.Users;

        public async Task<UserReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .Where(a => a.Guid == entityGuid)
            .AsNoTracking()
            .OrderBy(x => x.Guid)
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<UserReadModel>> GetMany(
                Pagination pagination,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .Include(u => u.UserPermissions)
            .OrderBy(x => x.Guid)
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}