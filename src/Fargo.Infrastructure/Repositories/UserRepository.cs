using Fargo.Application.Mappings;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserRepository(FargoDbContext context) : IUserRepository
{
    private readonly DbSet<User> users = context.Users;

    public Task<bool> Any(CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public async Task<User?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default)
    {
        return await users
            .Include(user => user.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(g => g.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(g => g.PartitionAccesses)
            .Include(user => user.PartitionAccesses)
            .Include(user => user.Partitions)
            .Where(user => user.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await users
            .Include(user => user.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(g => g.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(g => g.PartitionAccesses)
            .Include(user => user.PartitionAccesses)
            .Include(user => user.Partitions)
            .Where(user => user.Nameid == nameid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!await users.AnyAsync(u => u.Guid == entityGuid, cancellationToken))
        {
            return null;
        }

        IQueryable<Partition> query = users
            .Where(u => u.Guid == entityGuid)
            .SelectMany(u => u.Partitions);

        if (partitionFilter is not null)
        {
            query = query.Where(p => partitionFilter.Contains(p.Guid));
        }

        return await query
            .AsNoTracking()
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }

    public async Task<bool> ExistsByGuid(
        Guid guid,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(user => user.Guid == guid, cancellationToken);
    }

    public async Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(user => user.Nameid == nameid, cancellationToken);
    }

    public async Task<UserInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(user => user.Guid == entityGuid)
            .Select(UserMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(UserMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return null;
        }

        IQueryable<User> query = users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(user => user.Guid == entityGuid)
            .Where(user => user.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(UserMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<User> query = users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(user => user.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        return await query
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(UserMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(user => user.Guid)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<User> query = users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(user => user.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        return await query
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(user => user.Guid)
            .ToListAsync(cancellationToken);
    }
}
