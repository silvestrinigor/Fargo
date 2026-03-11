using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    /// <summary>
    /// Provides query operations for <see cref="UserGroupReadModel"/>.
    /// </summary>
    public class UserGroupQueries(FargoReadDbContext context) : IUserGroupQueries
    {
        private readonly DbSet<UserGroupReadModel> userGroups = context.UserGroups;

        /// <summary>
        /// Retrieves a single user group by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the user group.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve temporal data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The matching <see cref="UserGroupReadModel"/>, or <c>null</c> if not found.
        /// </returns>
        public async Task<UserGroupReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .Include(x => x.UserGroupPermissions)
            .Where(x => x.Guid == entityGuid)
            .AsNoTracking()
            .OrderBy(x => x.Guid)
            .SingleOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Retrieves a paginated collection of user groups.
        /// </summary>
        /// <param name="pagination">
        /// The pagination parameters used to limit the result set.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve temporal data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserGroupReadModel"/>.
        /// </returns>
        public async Task<IReadOnlyCollection<UserGroupReadModel>> GetMany(
                Pagination pagination,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .Include(x => x.UserGroupPermissions)
            .OrderBy(x => x.Guid)
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}