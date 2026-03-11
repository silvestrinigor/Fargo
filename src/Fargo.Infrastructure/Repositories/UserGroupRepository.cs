using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    /// <summary>
    /// Provides persistence operations for <see cref="UserGroup"/> entities.
    /// </summary>
    public sealed class UserGroupRepository(FargoWriteDbContext context) : IUserGroupRepository
    {
        private readonly DbSet<UserGroup> userGroups = context.UserGroups;

        /// <inheritdoc/>
        public Task<bool> Any(CancellationToken cancellationToken = default)
            => context.UserGroups.AnyAsync(cancellationToken);

        /// <inheritdoc/>
        public void Add(UserGroup userGroup)
        {
            context.UserGroups.Add(userGroup);
        }

        /// <inheritdoc/>
        public async Task<UserGroup?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                )
            => await userGroups
                .Include(g => g.UserGroupPermissions)
                .Where(g => g.Guid == entityGuid)
                .SingleOrDefaultAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<UserGroup?> GetByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                )
            => await userGroups
                .Where(g => g.Nameid == nameid)
                .SingleOrDefaultAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<bool> ExistsByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                )
            => await context.UserGroups
                .AnyAsync(x => x.Nameid == nameid, cancellationToken);

        /// <inheritdoc/>
        public void Remove(UserGroup userGroup)
        {
            context.UserGroups.Remove(userGroup);
        }
    }
}