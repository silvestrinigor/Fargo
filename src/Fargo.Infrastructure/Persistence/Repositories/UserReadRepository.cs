using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class UserReadRepository(FargoReadDbContext context) : IUserReadRepository
    {
        private readonly DbSet<UserReadModel> users = context.Users;

        public async Task<UserReadModel?> GetByGuid(
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<UserReadModel>> GetMany(
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a => a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}