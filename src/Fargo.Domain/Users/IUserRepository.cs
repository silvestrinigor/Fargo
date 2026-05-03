namespace Fargo.Domain.Users;

/// <summary>
/// Defines the repository contract for managing <see cref="User"/> entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by its unique identifier.
    /// </summary>
    Task<User?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a user by their unique <see cref="Nameid"/>.
    /// </summary>
    Task<User?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified identifier exists.
    /// </summary>
    Task<bool> ExistsByGuid(
        Guid guid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new user to the persistence context.
    /// </summary>
    void Add(User user);

    /// <summary>
    /// Removes a user from the persistence context.
    /// </summary>
    void Remove(User user);

    /// <summary>
    /// Determines whether any users exist in the system.
    /// </summary>
    Task<bool> Any(CancellationToken cancellationToken = default);
}
