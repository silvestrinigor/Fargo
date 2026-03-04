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
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<UserReadModel>> GetMany(
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await users
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}