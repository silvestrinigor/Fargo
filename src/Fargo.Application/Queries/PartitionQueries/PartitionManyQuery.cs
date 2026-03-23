using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.PartitionQueries;

/// <summary>
/// Query used to retrieve a paginated collection of partition information
/// accessible to the current user.
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
/// When <see langword="null"/>, a default pagination is used.
/// </param>
public sealed record PartitionManyQuery(
        Guid? ParentPartitionGuid = null,
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<PartitionInformation>>;

/// <summary>
/// Handles the execution of <see cref="PartitionManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// only those partitions accessible to the current user.
///
/// If <see cref="PartitionManyQuery.ParentPartitionGuid"/> is provided,
/// only partitions whose parent matches that identifier are returned.
/// </remarks>
public sealed class PartitionManyQueryHandler(
        ActorService actorService,
        IPartitionRepository partitionRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>>
{
    /// <summary>
    /// Executes the query to retrieve partition information accessible
    /// to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the optional parent partition filter, as-of date,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="PartitionInformation"/> objects
    /// accessible to the current user.
    /// </returns>
    public async Task<IReadOnlyCollection<PartitionInformation>> Handle(
            PartitionManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

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
