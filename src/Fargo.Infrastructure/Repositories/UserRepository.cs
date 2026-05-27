using Fargo.Application;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Users;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserRepository(FargoDbContext context) : IUserRepository, IUserQueryRepository
{
    private readonly DbSet<User> users = context.Users;

    public Task<bool> Any(CancellationToken cancellationToken = default)
        => users.AnyAsync(cancellationToken);

    public void Add(User user) => users.Add(user);

    public void Remove(User user) => users.Remove(user);

    public Task<User?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => IncludeAggregate(users)
            .SingleOrDefaultAsync(user => user.Guid == entityGuid, cancellationToken);

    public Task<User?> GetByNameid(Nameid nameid, CancellationToken cancellationToken = default)
        => IncludeAggregate(users)
            .SingleOrDefaultAsync(user => user.Nameid == nameid, cancellationToken);

    public Task<bool> ExistsByGuid(Guid guid, CancellationToken cancellationToken = default)
        => users.AnyAsync(user => user.Guid == guid, cancellationToken);

    public Task<bool> ExistsByNameid(Nameid nameid, CancellationToken cancellationToken = default)
        => users.AnyAsync(user => user.Nameid == nameid, cancellationToken);

    public async Task<UserDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var user = await ApplyPartitionFilter(
                users
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Where(user => user.Guid == entityGuid)
            .Select(UserDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return user;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                users
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(UserDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }

    private static IQueryable<User> IncludeAggregate(IQueryable<User> query)
        => query
            .Include(user => user.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(group => group.Permissions)
            .Include(user => user.UserGroups)
                .ThenInclude(group => group.PartitionAccesses)
            .Include(user => user.UserGroups)
                .ThenInclude(group => group.Partitions)
            .Include(user => user.PartitionAccesses)
            .Include(user => user.Partitions);

    private static IQueryable<User> ApplyPartitionFilter(
        IQueryable<User> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notChildOfAnyPartition)
    {
        if (partitionGuids is null)
        {
            if (notChildOfAnyPartition is true)
            {
                return query.Where(user => !user.Partitions.Any());
            }

            if (notChildOfAnyPartition is false)
            {
                return query.Where(user => user.Partitions.Any());
            }

            return query;
        }

        if (notChildOfAnyPartition is true)
        {
            return query.Where(user =>
                !user.Partitions.Any() ||
                user.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query.Where(user =>
            user.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
    }

}
