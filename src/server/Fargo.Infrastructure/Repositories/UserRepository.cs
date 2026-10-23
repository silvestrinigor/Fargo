using Fargo.Application.Common;
using Fargo.Application.Users;
using Fargo.Core.Actors;
using Fargo.Core.Informations;
using Fargo.Core.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserRepository(FargoDbContext context) : IUserRepository, IUserQueryRepository
{
    private readonly DbSet<User> users = context.Users;

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => users.AnyAsync(cancellationToken);

    public void Add(User user) => users.Add(user);

    public void Remove(User user) => users.Remove(user);

    public Task<User?> GetByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default)
        => IncludeAggregate(users)
            .SingleOrDefaultAsync(user => user.Guid == userGuid, cancellationToken);

    public Task<User?> GetByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default)
        => IncludeAggregate(users)
            .SingleOrDefaultAsync(user => user.Nameid == nameid, cancellationToken);

    public Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default)
        => users.AnyAsync(user => user.Nameid == nameid, cancellationToken);

    public async Task<UserDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var user = await ApplyPartitionFilter(
                users
                    .AsNoTracking(),
                childOfAnyOfThesePartitions)
            .Where(user => user.Guid == entityGuid)
            .Select(UserDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return user;
    }

    public async Task<PagedResult<UserDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
                users
                    .AsNoTracking(),
                childOfAnyOfThesePartitions);

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var result = await queryFiltered
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(UserDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(
            result,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<User> IncludeAggregate(IQueryable<User> query)
        => query
        .Include(user => user.UserGroupMemberships)
        .Include(user => user.PartitionAccesses)
        .Include(user => user.Partitions)
        .Include(user => user.Authentication)
        .AsSplitQuery();

    private static IQueryable<User> ApplyPartitionFilter(
        IQueryable<User> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(user =>
            user.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }

    public async Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsFromUserAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE user_group_hierarchy AS
        (
            SELECT
                ug.guid,
                ug.parent_user_group_guid
            FROM user_groups ug
            INNER JOIN user_user_groups uug
                ON uug.user_group_guid = ug.guid
            INNER JOIN users u
                ON u.guid = uug.user_guid
            WHERE u.guid = {userGuid}
            AND u.is_active = true
            AND ug.is_active = true

            UNION ALL

            SELECT
                parent.guid,
                parent.parent_user_group_guid
            FROM user_groups parent
            INNER JOIN user_group_hierarchy child
                ON child.parent_user_group_guid = parent.guid
            WHERE parent.is_active = true
        ),
        granted_partitions AS
        (
            SELECT upa.partition_guid
            FROM user_partition_accesses upa
            INNER JOIN users u
                ON u.guid = upa.user_guid
            WHERE upa.user_guid = {userGuid}
            AND u.is_active = true

            UNION

            SELECT ugpa.partition_guid
            FROM user_group_partition_accesses ugpa
            INNER JOIN user_group_hierarchy ugh
                ON ugh.guid = ugpa.user_group_guid
        ),
        partition_tree AS
        (
            -- Seed with the partitions granted directly to the user/groups.
            SELECT p.guid, p.parent_partition_guid
            FROM partitions p
            INNER JOIN granted_partitions gp
                ON gp.partition_guid = p.guid

            UNION

            -- Expand downward: every child of an already-included partition is
            -- also accessible (access to a partition implies access to all of
            -- its descendant partitions).
            SELECT child.guid, child.parent_partition_guid
            FROM partitions AS child
            INNER JOIN partition_tree AS parent
                ON child.parent_partition_guid = parent.guid
        )
        SELECT DISTINCT guid AS partition_guid
        FROM partition_tree;
        """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        return guids;
    }

    public async Task<IReadOnlyCollection<ActionType>> GetAllActivePermissionsFromUserAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE user_group_hierarchy AS
        (
            -- Groups directly assigned to the user
            SELECT
                ug.guid,
                ug.permissions,
                ug.parent_user_group_guid
            FROM user_groups ug
            INNER JOIN user_user_groups uug
                ON uug.user_group_guid = ug.guid
            INNER JOIN users u
                ON u.guid = uug.user_guid
            WHERE u.guid = {userGuid}
              AND u.is_active = true
              AND ug.is_active = true

            UNION ALL

            -- Parent groups
            SELECT
                parent.guid,
                parent.permissions,
                parent.parent_user_group_guid
            FROM user_groups parent
            INNER JOIN user_group_hierarchy child
                ON child.parent_user_group_guid = parent.guid
            WHERE parent.is_active = true
        ),
        all_permissions AS
        (
            -- Direct user permissions
            SELECT jsonb_array_elements_text(u.permissions)::integer AS permission
            FROM users u
            WHERE u.guid = {userGuid}
              AND u.is_active = true

            UNION ALL

            -- Permissions inherited from groups
            SELECT jsonb_array_elements_text(ugh.permissions)::integer AS permission
            FROM user_group_hierarchy ugh
        )
        SELECT DISTINCT permission
        FROM all_permissions;
        """;

        var permissions = await context.Database
            .SqlQuery<ActionType>(query)
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
