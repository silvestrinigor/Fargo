using Fargo.Core.Informations;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// Defines the repository contract for querying and persisting
/// <see cref="UserGroup"/> entities.
/// </summary>
/// <remarks>
/// Implementations are responsible for retrieving user groups from the
/// persistence layer and tracking changes for creation and deletion.
/// Changes are typically committed through a unit of work.
/// </remarks>
public interface IUserGroupRepository
{
    /// <summary>
    /// Gets a user group by its unique identifier.
    /// </summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="UserGroup"/> if found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<UserGroup?> GetByGuidAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a user group with the specified
    /// <see cref="Nameid"/> already exists.
    /// </summary>
    /// <param name="nameid">The name identifier to search for.</param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching user group exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any user groups exist in the system.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if at least one user group exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified user group has any child user groups.
    /// </summary>
    /// <param name="parentUserGroupGuid">
    /// The identifier of the parent user group.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the user group has one or more child user
    /// groups; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> HasChildrenUserGroupAsync(Guid parentUserGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the identifiers of all descendant user groups of the specified
    /// user group.
    /// </summary>
    /// <param name="userGroupGuid">
    /// The identifier of the root user group.
    /// </param>
    /// <param name="includeRoot">
    /// <see langword="true"/> to include the specified user group in the
    /// result; otherwise, only descendant user groups are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the identifiers of the matching
    /// descendant user groups.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetDescendantUserGroupGuidsAsync(Guid userGroupGuid, bool includeRoot = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a single, global advisory lock that serializes user-group
    /// re-parenting across the whole user-group tree.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <remarks>
    /// The lock protects the check-then-act in the re-parent flow: validating
    /// that assigning a parent does not create a circular hierarchy reads other
    /// user groups, so two concurrent re-parents could each pass validation and
    /// together create a cycle. A circular hierarchy can span several groups
    /// whose paths only share an interior node, so a per-group key would not
    /// reliably serialize the conflicting moves; one global lock serializes every
    /// such re-parent and is therefore correct for cycles of any size. It is only
    /// meant to be taken when actually assigning a parent (the sole change that
    /// can create a cycle). The lock releases automatically when the surrounding
    /// transaction ends, so it must be called inside a transaction.
    /// </remarks>
    Task AcquireHierarchyLockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user group to the persistence context.
    /// </summary>
    /// <param name="userGroup">The user group to add.</param>
    /// <remarks>
    /// The user group is tracked by the persistence context. The operation is
    /// not committed until the associated unit of work is completed.
    /// </remarks>
    void Add(UserGroup userGroup);

    /// <summary>
    /// Removes a user group from the persistence context.
    /// </summary>
    /// <param name="userGroup">The user group to remove.</param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(UserGroup userGroup);
}
