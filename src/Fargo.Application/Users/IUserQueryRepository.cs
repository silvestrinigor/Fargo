using Fargo.Application.Common;

namespace Fargo.Application.Users;

/// <summary>
/// Defines read-only queries for user information.
/// </summary>
public interface IUserQueryRepository
{
    /// <summary>
    /// Gets user information by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// An optional collection of partition identifiers. When specified,
    /// only users associated with a partition that is a child of any of
    /// the specified partitions are considered.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The user information if a matching user is found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<UserDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of user information.
    /// </summary>
    /// <param name="pagination">
    /// The pagination parameters used to determine the requested page
    /// and the number of users to return.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// An optional collection of partition identifiers. When specified,
    /// only users associated with a partition that is a child of any of
    /// the specified partitions are considered.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the requested page of user information.
    /// </returns>
    Task<IReadOnlyCollection<UserDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
