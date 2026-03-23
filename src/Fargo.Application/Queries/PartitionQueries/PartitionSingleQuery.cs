using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.PartitionQueries;

/// <summary>
/// Query used to retrieve a single partition information projection
/// accessible to the current user.
/// </summary>
/// <param name="PartitionGuid">
/// The unique identifier of the partition to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the partition
/// as it existed at the specified date and time.
/// </param>
public sealed record PartitionSingleQuery(
        Guid PartitionGuid,
        DateTimeOffset? AsOfDateTime = null
        ) : IQuery<PartitionInformation?>;

/// <summary>
/// Handles the execution of <see cref="PartitionSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// the requested partition only if it is accessible to the current user.
///
/// If the partition does not exist or is not accessible to the current user,
/// <see langword="null"/> is returned.
/// </remarks>
public sealed class PartitionSingleQueryHandler(
        ActorService actorService,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<PartitionSingleQuery, PartitionInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single partition information projection
    /// accessible to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the partition identifier and optional as-of date.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="PartitionInformation"/> accessible to the current user,
    /// or <see langword="null"/> if the partition does not exist or is not accessible.
    /// </returns>
    public async Task<PartitionInformation?> Handle(
            PartitionSingleQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (!actor.IsAdmin && !actor.IsSystem && !actor.PartitionAccesses.Contains(query.PartitionGuid))
        {
            return null;
        }

        return await partitionRepository.GetInfoByGuid(
                query.PartitionGuid,
                query.AsOfDateTime,
                cancellationToken
                );
    }
}
