using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Partitions;

/// <summary>
/// Query used to retrieve a single <see cref="PartitionInformation"/> projection
/// accessible to the current actor.
/// </summary>
/// <param name="PartitionGuid">
/// The unique identifier of the partition to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the partition
/// as it existed at the specified date and time.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only partitions visible to the current actor are returned.
/// </remarks>
public sealed record PartitionSingleQuery(
        Guid PartitionGuid,
        DateTimeOffset? AsOfDateTime = null
        ) : IQuery<PartitionInformation?>;

/// <summary>
/// Handles <see cref="PartitionSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Validating access to the requested partition.</description></item>
/// <item><description>Applying optional temporal (as-of) constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges can access any partition.
/// </para>
///
/// <para>
/// Regular actors can only access partitions that are explicitly available
/// in their partition access set.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if the partition existed
/// at the specified point in time but has since been deleted at the time of
/// the request, the following rules apply:
/// <list type="bullet">
/// <item>
/// <description>
/// Administrative and system actors can still access the historical data.
/// </description>
/// </item>
/// <item>
/// <description>
/// Regular actors will not have access to the partition, as it no longer
/// exists in the current context, and <see langword="null"/> is returned.
/// </description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// If the partition does not exist or is not accessible to the current actor,
/// <see langword="null"/> is returned.
/// </para>
/// </remarks>
public sealed class PartitionSingleQueryHandler(
        ActorService actorService,
        IPartitionQueryRepository partitionRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<PartitionSingleQuery, PartitionInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single partition information projection
    /// accessible to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing the partition identifier and optional temporal parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="PartitionInformation"/> visible to the current actor,
    /// or <see langword="null"/> if the partition does not exist or is not accessible.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// Administrative and system actors bypass access validation.
    ///
    /// Regular actors must have explicit access to the requested partition;
    /// otherwise, <see langword="null"/> is returned.
    ///
    /// This method does not throw when access is denied, following the
    /// query-side soft-authorization pattern.
    /// </remarks>
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
