using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public class UserReadRepository(FargoReadDbContext context) : EntityByGuidTemporalReadRepository<UserReadModel>(context.Users), IUserReadRepository
    {
        private readonly FargoReadDbContext context = context;

        public async Task<IEnumerable<PermissionReadModel>?> GetUserPermissions(
            Guid userGuid, 
            DateTime? temporalAsOf = null, 
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetUserPermissions(
                temporalAsOf is not null ? context.Permissions.TemporalAsOf(temporalAsOf.Value) : context.Permissions.AsQueryable(),
                userGuid,
                pagination,
                cancellationToken);

        protected static async Task<IEnumerable<PermissionReadModel>?> GetUserPermissions(
            IQueryable<PermissionReadModel> query,
            Guid userGuid, 
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await query
            .Where(x => x.UserGuid == userGuid)
            .Skip(pagination.Skip)
            .Take(pagination.Limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
