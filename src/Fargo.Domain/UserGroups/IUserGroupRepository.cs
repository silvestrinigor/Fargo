namespace Fargo.Domain.Users;

/// <summary>
/// Defines the repository contract for managing <see cref="UserGroup"/> entities.
/// </summary>
public interface IUserGroupRepository
{
    /// <summary>
    /// Gets a user group by its unique identifier.
    /// </summary>
    Task<UserGroup?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a user group by its unique <see cref="Nameid"/>.
    /// </summary>
    Task<UserGroup?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user group with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new user group to the persistence context.
    /// </summary>
    void Add(UserGroup userGroup);

    /// <summary>
    /// Removes a user group from the persistence context.
    /// </summary>
    void Remove(UserGroup userGroup);

    /// <summary>
    /// Determines whether any user groups exist in the system.
    /// </summary>
    Task<bool> Any(CancellationToken cancellationToken = default);
}
