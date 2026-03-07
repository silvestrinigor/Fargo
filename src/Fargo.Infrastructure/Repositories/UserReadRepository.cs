using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class UserReadRepository(FargoReadDbContext context) : IUserReadRepository
    {
        private readonly DbSet<UserReadModel> users = context.Users;

        public async Task<UserReadModel?> GetByGuid(
                Guid entityGuid,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a => a.Guid == entityGuid)
            .AsNoTracking()
            .OrderBy(x => x.Guid)
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<UserReadModel>> GetMany(
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Include(u => u.UserPermissions)
            .WithPagination(pagination)
            .OrderBy(x => x.Guid)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}