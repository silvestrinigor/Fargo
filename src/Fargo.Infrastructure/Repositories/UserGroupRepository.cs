using Fargo.Application;
using Fargo.Application.Shared.UserGroups;
using Fargo.Application.UserGroups;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
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

    public Task<UserGroup?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
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

    public async Task<UserGroupDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var userGroup = await ApplyPartitionFilter(
                userGroups
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Where(userGroup => userGroup.Guid == entityGuid)
            .Select(UserGroupDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return userGroup;
    }

    public async Task<IReadOnlyCollection<UserGroupDto>> GetManyInfoAsync(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                userGroups
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(UserGroupDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }

    private static IQueryable<UserGroup> ApplyPartitionFilter(
        IQueryable<UserGroup> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notChildOfAnyPartition)
    {
        if (partitionGuids is null)
        {
            if (notChildOfAnyPartition is true)
            {
                return query.Where(userGroup => !userGroup.Partitions.Any());
            }

            if (notChildOfAnyPartition is false)
            {
                return query.Where(userGroup => userGroup.Partitions.Any());
            }

            return query;
        }

        if (notChildOfAnyPartition is true)
        {
            return query.Where(userGroup =>
                !userGroup.Partitions.Any() ||
                userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query.Where(userGroup =>
            userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
    }

}
