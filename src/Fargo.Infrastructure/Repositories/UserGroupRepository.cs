using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupRepository(FargoDbContext context) : IUserGroupRepository, IUserGroupQueryRepository
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

    public async Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!await userGroups.AnyAsync(g => g.Guid == entityGuid, cancellationToken))
        {
            return null;
        }

        IQueryable<Partition> query = userGroups
            .Where(g => g.Guid == entityGuid)
            .SelectMany(g => g.Partitions);

        if (partitionFilter is not null)
        {
            query = query.Where(p => partitionFilter.Contains(p.Guid));
        }

        return await query
            .AsNoTracking()
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
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
                userGroup.Users.Any(user => user.Guid == userGuid.Value));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(UserGroupMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuids(
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
                userGroup.Users.Any(user => user.Guid == userGuid.Value));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(userGroup => userGroup.Guid)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserGroupInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return null;
        }

        IQueryable<UserGroup> query = userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(userGroup => userGroup.Guid == entityGuid)
            .Where(userGroup => userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(UserGroupMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<UserGroup> query = userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(userGroup => userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (userGuid.HasValue)
        {
            query = query.Where(userGroup =>
                userGroup.Users.Any(user => user.Guid == userGuid.Value));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(UserGroupMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserGroupInformation?> GetInfoByGuidPublicOrInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(userGroup => userGroup.Guid == entityGuid)
            .Where(userGroup => !userGroup.Partitions.Any()
                || userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(UserGroupMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfoInPartitionsOrPublic(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserGroup> query = userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(userGroup => !userGroup.Partitions.Any()
                || userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (userGuid.HasValue)
        {
            query = query.Where(userGroup =>
                userGroup.Users.Any(user => user.Guid == userGuid.Value));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(UserGroupMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<UserGroup> query = userGroups
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(userGroup => userGroup.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (userGuid.HasValue)
        {
            query = query.Where(userGroup =>
                userGroup.Users.Any(user => user.Guid == userGuid.Value));
        }

        return await query
            .OrderBy(userGroup => userGroup.Guid)
            .WithPagination(pagination)
            .Select(userGroup => userGroup.Guid)
            .ToListAsync(cancellationToken);
    }
}
