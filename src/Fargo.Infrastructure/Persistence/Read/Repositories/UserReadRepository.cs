using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public class UserReadRepository(FargoReadDbContext context) : EntityByGuidTemporalReadRepository<UserReadModel>(context.Users), IUserReadRepository
    {
        private readonly FargoReadDbContext context = context;

        public async Task<IEnumerable<PermissionReadModel>?> GetUserPermissions(Guid userGuid, DateTime? atDateTime = null, CancellationToken cancellationToken = default)
            => await GetUserPermissions(
                atDateTime is not null ? context.Permissions.TemporalAsOf(atDateTime.Value) : context.Permissions.AsQueryable(),
                userGuid,
                cancellationToken);

        protected static async Task<IEnumerable<PermissionReadModel>?> GetUserPermissions(IQueryable<PermissionReadModel> query, Guid userGuid, CancellationToken cancellationToken = default)
            => await query
                .Where(x => x.UserGuid == userGuid)
                .AsNoTracking()
                .ToListAsync(cancellationToken: cancellationToken);
    }
}
