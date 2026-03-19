using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserQueries;

/// <summary>
/// Query used to retrieve a paginated collection of user information
/// accessible to the current user.
/// </summary>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the users
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is used.
/// </param>
public sealed record UserManyQuery(
    DateTimeOffset? AsOfDateTime = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<UserInformation>>;

/// <summary>
/// Handles the execution of <see cref="UserManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// only users that belong to at least one of those partitions.
///
/// If the current user has no accessible partitions, the repository returns
/// an empty result set.
/// </remarks>
public sealed class UserManyQueryHandler(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>>
{
    /// <summary>
    /// Executes the query to retrieve user information accessible
    /// to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the optional as-of date and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="UserInformation"/> objects
    /// accessible to the current user.
    /// </returns>
    public async Task<IReadOnlyCollection<UserInformation>> Handle(
        UserManyQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var partitionAccessGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken
        );

        return await userRepository.GetManyInfoInPartitions(
            query.Pagination ?? Pagination.FirstPage20Items,
            partitionAccessGuids,
            query.AsOfDateTime,
            cancellationToken
        );
    }
}
