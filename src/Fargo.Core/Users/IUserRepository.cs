using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Informations;

namespace Fargo.Core.Users;

/// <summary>
/// Defines the repository contract for managing <see cref="User"/> entities.
/// </summary>
/// <remarks>
/// Implementations are responsible for retrieving users from the persistence layer
/// and tracking changes for creation and deletion. Changes are typically committed
/// through a unit of work.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by its unique identifier.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<User?> GetByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their unique <see cref="Nameid"/>.
    /// </summary>
    /// <param name="userNameid">The unique name identifier of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<User?> GetByNameidAsync(Nameid userNameid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all partition access identifiers available to an active user,
    /// including accesses granted directly and through active user groups
    /// and their parent groups.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A collection containing the unique identifiers of all partitions
    /// the user has access to. Returns an empty collection if the user is
    /// inactive. Accesses granted through inactive user groups are excluded.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsFromUserAsync(
        Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all action permissions available to an active user, including
    /// permissions granted directly to the user and through active user groups
    /// and their parent groups.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A collection containing all action permissions available to the user.
    /// Returns an empty collection if the user is inactive. Permissions granted
    /// through inactive user groups are excluded.
    /// </returns>
    Task<IReadOnlyCollection<ActionType>> GetAllActivePermissionsFromUserAsync(
    Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a user with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    /// <param name="nameid">The name identifier to search for.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if a matching user exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any users exist in the system.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if at least one user exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the persistence context.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <remarks>
    /// The user is tracked by the persistence context. The operation is not
    /// committed until the associated unit of work is completed.
    /// </remarks>
    void Add(User user);

    /// <summary>
    /// Removes a user from the persistence context.
    /// </summary>
    /// <param name="user">The user to remove.</param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(User user);
}
