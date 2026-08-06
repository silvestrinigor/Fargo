using Fargo.Core.Shared.Informations;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Defines the repository contract for managing <see cref="UserGroup"/> entities.
/// </summary>
public interface IUserGroupRepository
{
    /// <summary>
    /// Gets a user group by its unique identifier.
    /// </summary>
    Task<UserGroup?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a user group with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default);

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
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
        Guid userGroupGuid, bool includeRoot = true, CancellationToken cancellationToken = default);

    Task<bool> AnyChildUserGroupAsync(Guid parentUserGroupGuid, CancellationToken cancellationToken = default);
}
