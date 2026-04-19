using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Query used to retrieve a paginated collection of <see cref="UserInformation"/>
/// accessible to the current actor.
/// </summary>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the users
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is applied.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only users visible to the current actor are returned.
/// </remarks>
public sealed record UserManyQuery(
    DateTimeOffset? AsOfDateTime = null,
    Pagination? Pagination = null,
    Guid? PartitionGuid = null
) : IQuery<IReadOnlyCollection<UserInformation>>;

/// <summary>
/// Handles <see cref="UserManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering users based on partition access.</description></item>
/// <item><description>Applying optional temporal (as-of) and pagination constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access all users.
/// </para>
///
/// <para>
/// Regular actors can only access users that belong to at least one
/// partition they have access to.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if a user belonged
/// to a partition that has since been deleted at the time of the request,
/// the following rules apply:
/// <list type="bullet">
/// <item>
/// <description>
/// Administrative and system actors can still access the historical data.
/// </description>
/// </item>
/// <item>
/// <description>
/// Regular actors will not have access to such users, as the partition
/// no longer exists in the current context, and the result will be excluded
/// from the returned collection.
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class UserManyQueryHandler(
        ActorService actorService,
    IUserRepository userRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>>
{
    /// <summary>
    /// Executes the query to retrieve user information accessible
    /// to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing the optional temporal parameter and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="UserInformation"/> objects
    /// visible to the current actor.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, users are retrieved
    /// without partition filtering.
    ///
    /// Otherwise, only users belonging to partitions accessible to the actor
    /// are returned.
    ///
    /// Pagination defaults to <see cref="Pagination.FirstPage20Items"/> when not specified.
    /// </remarks>
    public async Task<IReadOnlyCollection<UserInformation>> Handle(
        UserManyQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            if (query.PartitionGuid.HasValue)
            {
                return await userRepository.GetManyInfoInPartitions(
                        query.Pagination ?? Pagination.FirstPage20Items,
                        [query.PartitionGuid.Value],
                        query.AsOfDateTime,
                        cancellationToken
                        );
            }

            return await userRepository.GetManyInfo(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.AsOfDateTime,
                    cancellationToken
                    );
        }
        else
        {
            if (query.PartitionGuid.HasValue && !actor.PartitionAccesses.Contains(query.PartitionGuid.Value))
                return [];

            var partitions = query.PartitionGuid.HasValue
                ? (IReadOnlyCollection<Guid>)[query.PartitionGuid.Value]
                : actor.PartitionAccesses;

            return await userRepository.GetManyInfoInPartitions(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    partitions,
                    query.AsOfDateTime,
                    cancellationToken
                    );
        }
    }
}
