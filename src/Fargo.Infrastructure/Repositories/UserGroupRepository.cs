using Fargo.Application;
using Fargo.Application.Shared.UserGroups;
using Fargo.Application.UserGroups;
using Fargo.Core.Shared.Informations;
using Fargo.Core.UserGroups;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupRepository(FargoDbContext context) : IUserGroupRepository, IUserGroupQueryRepository
{
    private readonly DbSet<UserGroup> userGroups = context.UserGroups;

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => userGroups.AnyAsync(cancellationToken);

    public void Add(UserGroup userGroup) => userGroups.Add(userGroup);

    public void Remove(UserGroup userGroup) => userGroups.Remove(userGroup);

    public Task<UserGroup?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        return userGroups
        .Include(userGroup => userGroup.Partitions)
        .Include(userGroup => userGroup.PartitionAccesses)
        .SingleOrDefaultAsync(userGroup => userGroup.Guid == entityGuid, cancellationToken);
    }

    public Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default)
        => userGroups.AnyAsync(userGroup => userGroup.Nameid == nameid, cancellationToken);

    public async Task<UserGroupDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var userGroup = await ApplyPartitionFilter(
                userGroups
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
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                userGroups
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
                userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
        }

        return query.Where(userGroup =>
            userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }

    public async Task<IReadOnlyCollection<Guid>> GetDescendantUserGroupGuidsAsync(Guid userGroupGuid, bool includeRoot = true, CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE user_group_tree AS
        (
            SELECT guid, parent_user_group_guid
            FROM user_groups
            WHERE guid = {userGroupGuid}

            UNION ALL

            SELECT child.guid, child.parent_user_group_guid
            FROM user_groups AS child
            INNER JOIN user_group_tree AS parent
                ON child.parent_user_group_guid = parent.guid
        )
        SELECT guid
        FROM user_group_tree
        """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        if (!includeRoot)
        {
            guids.RemoveAll(guid => guid == userGroupGuid);
        }

        return guids;
    }

    public Task<bool> HasChildrenUserGroupAsync(Guid parentUserGroupGuid, CancellationToken cancellationToken = default)
    {
        return context.UserGroups.AnyAsync(u => u.ParentUserGroupGuid == parentUserGroupGuid, cancellationToken);
    }
}
