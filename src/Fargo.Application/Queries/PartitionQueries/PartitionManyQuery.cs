using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.PartitionQueries;

/// <summary>
/// Query used to retrieve a paginated collection of <see cref="PartitionInformation"/>
/// accessible to the current actor.
/// </summary>
/// <param name="ParentPartitionGuid">
/// Optional filter used to retrieve only partitions that belong to a specific
/// parent partition. When provided, only child partitions of the specified
/// parent are returned.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the partitions
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is applied.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only partitions visible to the current actor are returned.
/// </remarks>
public sealed record PartitionManyQuery(
        Guid? ParentPartitionGuid = null,
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<PartitionInformation>>;

/// <summary>
/// Handles <see cref="PartitionManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering partitions based on access permissions.</description></item>
/// <item><description>Applying optional parent filter, temporal (as-of), and pagination constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass access filtering
/// and can retrieve all partitions.
/// </para>
///
/// <para>
/// Regular actors can only access partitions that are explicitly available
/// through their partition access set.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if a partition existed
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
/// Regular actors will not have access to such partitions, as they no longer
/// exist in the current context, and the result will be excluded from the
/// returned collection.
/// </description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// If <see cref="PartitionManyQuery.ParentPartitionGuid"/> is provided,
/// only partitions whose parent matches that identifier are returned.
/// </para>
/// </remarks>
public sealed class PartitionManyQueryHandler(
        ActorService actorService,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>>
{
    /// <summary>
    /// Executes the query to retrieve partition information accessible
    /// to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing optional parent filter, temporal parameter,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="PartitionInformation"/> objects
    /// visible to the current actor.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, partitions are retrieved
    /// without access filtering.
    ///
    /// Otherwise, only partitions explicitly accessible to the actor are returned.
    ///
    /// Pagination defaults to <see cref="Pagination.FirstPage20Items"/> when not specified.
    /// </remarks>
    public async Task<IReadOnlyCollection<PartitionInformation>> Handle(
            PartitionManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var partitions = await partitionRepository.GetManyInfo(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.ParentPartitionGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return partitions;
        }
        else
        {
            var partitions = await partitionRepository.GetManyInfoByGuids(
                    actor.PartitionAccesses,
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.ParentPartitionGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return partitions;
        }
    }
}
