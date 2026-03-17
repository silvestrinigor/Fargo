using Fargo.Application.Mappings;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupRepository(FargoDbContext context) : IUserGroupRepository
{
    private readonly DbSet<UserGroup> userGroups = context.UserGroups;

    public Task<bool> Any(CancellationToken cancellationToken = default)
    {
        return context.UserGroups.AnyAsync(cancellationToken);
    }

    public void Add(UserGroup userGroup)
    {
        context.UserGroups.Add(userGroup);
    }

    public async Task<UserGroup?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default)
    {
        return await userGroups
            .Include(userGroup => userGroup.Permissions)
            .Where(userGroup => userGroup.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserGroup?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await userGroups
            .Where(userGroup => userGroup.Nameid == nameid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await context.UserGroups
            .AnyAsync(userGroup => userGroup.Nameid == nameid, cancellationToken);
    }

    public void Remove(UserGroup userGroup)
    {
        context.UserGroups.Remove(userGroup);
    }

    public async Task<UserGroupInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(userGroup => userGroup.Guid == entityGuid)
            .Select(UserGroupMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfo(
        Pagination pagination,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserGroup> query = userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        if (userGuid.HasValue)
        {
            query = query.Where(userGroup =>
                userGroup.Users.Any(g => g.Guid == userGuid));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(UserGroupMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }
}
