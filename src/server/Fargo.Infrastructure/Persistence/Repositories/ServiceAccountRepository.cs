using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Queries.ServiceAccounts.Dtos;

namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class ServiceAccountRepository(FargoDbContext context)
    : IServiceAccountRepository, IServiceAccountQueryRepository
{
    private readonly DbSet<ServiceAccount> serviceAccounts = context.Set<ServiceAccount>();
    private readonly DbSet<Actor> actors = context.Actors;

    public Task<ServiceAccount?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
        => IncludeAggregate(serviceAccounts)
            .SingleOrDefaultAsync(account => account.ActorGuid == guid, cancellationToken);

    public Task<ServiceAccount?> GetByClientIdAsync(Nameid clientId, CancellationToken cancellationToken = default)
        => IncludeAggregate(serviceAccounts)
            .SingleOrDefaultAsync(account => account.Actor.Nameid == clientId, cancellationToken);

    private static IQueryable<ServiceAccount> IncludeAggregate(IQueryable<ServiceAccount> query)
        => query
            .Include(account => account.Actor).ThenInclude(actor => actor.Partitions)
            .Include(account => account.Actor).ThenInclude(actor => actor.PartitionAccesses)
            .AsSplitQuery();

    public async Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsAsync(
        Guid serviceGuid,
        CancellationToken cancellationToken = default)
    {
        // Load the account's directly-granted partition accesses (inactive
        // accounts grant nothing), then expand downward through the partition
        // hierarchy so access to a partition implies access to all of its
        // descendants — mirroring the user partition-access resolution.
        var account = await IncludeAggregate(serviceAccounts)
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.ActorGuid == serviceGuid, cancellationToken);

        if (account is null || !account.Actor.IsActive || account.Actor.PartitionAccesses.Count == 0)
        {
            return [];
        }

        var grantedPartitionGuids = account.Actor.PartitionAccesses.Select(a => a.PartitionGuid).ToArray();

        FormattableString query = $"""
        WITH RECURSIVE partition_tree AS
        (
            -- Seed with the partitions granted directly to the service account.
            SELECT p.guid, p.parent_partition_guid
            FROM partitions p
            WHERE p.guid = ANY({grantedPartitionGuids})

            UNION

            -- Expand downward: every child of an already-included partition is
            -- also accessible.
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

    public void Add(ServiceAccount serviceAccount) => actors.Add(serviceAccount.Actor);

    public void Remove(ServiceAccount serviceAccount) => actors.Remove(serviceAccount.Actor);

    public async Task<ServiceAccountDto?> GetInfoByGuidAsync(
        Guid guid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var account = await ApplyPartitionFilter(
                serviceAccounts.AsNoTracking(),
                childOfAnyOfThesePartitions)
            .Where(a => a.ActorGuid == guid)
            .Select(ServiceAccountDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return account;
    }

    public async Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPartitionFilter(
            serviceAccounts.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .OrderBy(a => a.ActorGuid)
            .WithPagination(pagination)
            .Select(ServiceAccountDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceAccountDto>(
            accounts,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    // Restricts the query to service accounts whose partition membership
    // intersects the actor's accessible partitions, mirroring how users are
    // filtered. A null partition set means no filter (unscoped read).
    private static IQueryable<ServiceAccount> ApplyPartitionFilter(
        IQueryable<ServiceAccount> query,
        IReadOnlyCollection<Guid>? partitionGuids)
        => query.WherePartitionAccessible(
            partitionGuids,
            account => account.Actor.Partitions,
            partition => partition.PartitionGuid);
}
