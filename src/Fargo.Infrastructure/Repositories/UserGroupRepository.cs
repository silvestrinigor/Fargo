using Fargo.Application;
using Fargo.Application.UserGroups;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupRepository(FargoDbContext context) : IUserGroupRepository, IUserGroupQueryRepository
{
    private readonly DbSet<UserGroup> userGroups = context.UserGroups;

    public Task<bool> Any(CancellationToken cancellationToken = default)
        => userGroups.AnyAsync(cancellationToken);

    public void Add(UserGroup userGroup) => userGroups.Add(userGroup);

    public void Remove(UserGroup userGroup) => userGroups.Remove(userGroup);

    public Task<UserGroup?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => userGroups
            .Include(userGroup => userGroup.Permissions)
            .Include(userGroup => userGroup.Partitions)
            .Include(userGroup => userGroup.Users)
            .SingleOrDefaultAsync(userGroup => userGroup.Guid == entityGuid, cancellationToken);

    public Task<UserGroup?> GetByNameid(Nameid nameid, CancellationToken cancellationToken = default)
        => userGroups
            .Include(userGroup => userGroup.Permissions)
            .Include(userGroup => userGroup.Partitions)
            .Include(userGroup => userGroup.Users)
            .SingleOrDefaultAsync(userGroup => userGroup.Nameid == nameid, cancellationToken);

    public Task<bool> ExistsByNameid(Nameid nameid, CancellationToken cancellationToken = default)
        => userGroups.AnyAsync(userGroup => userGroup.Nameid == nameid, cancellationToken);

    public async Task<UserGroupDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var userGroup = await ApplyPartitionFilter(
                userGroups
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(userGroup => userGroup.Permissions)
                    .Include(userGroup => userGroup.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .SingleOrDefaultAsync(userGroup => userGroup.Guid == entityGuid, cancellationToken);

        return userGroup is null ? null : Map(userGroup);
    }

    public async Task<IReadOnlyCollection<UserGroupDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                userGroups
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(userGroup => userGroup.Permissions)
                    .Include(userGroup => userGroup.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(Map)];
    }

    private static IQueryable<UserGroup> ApplyPartitionFilter(
        IQueryable<UserGroup> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notInsideAnyPartition)
    {
        if (notInsideAnyPartition is true)
        {
            return query.Where(userGroup => !userGroup.Partitions.Any());
        }

        if (notInsideAnyPartition is false)
        {
            query = query.Where(userGroup => userGroup.Partitions.Any());
        }

        if (partitionGuids is { Count: > 0 })
        {
            query = query.Where(userGroup =>
                !userGroup.Partitions.Any() ||
                userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query;
    }

    private static UserGroupDto Map(UserGroup userGroup)
        => new(
            userGroup.Guid,
            userGroup.Nameid,
            userGroup.Description,
            [.. userGroup.Permissions.Select(permission => new Permission(permission.Guid, permission.Action))],
            [.. userGroup.Partitions.Select(partition => partition.Guid)],
            userGroup.IsActive,
            userGroup.EditedByGuid);
}
