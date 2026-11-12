using Fargo.Core.Informations;

namespace Fargo.Core.Entities.UserGroups;

public interface IUserGroupRepository
{
    Task<UserGroup?> GetByGuidAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameidAsync(Nameid nameid, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<bool> HasChildrenUserGroupAsync(Guid parentUserGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descendants at any depth. The root itself is included only when
    /// <paramref name="includeRoot"/> is set.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantUserGroupGuidsAsync(Guid userGroupGuid, bool includeRoot = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a single global advisory lock that serializes user-group re-parenting across the
    /// whole tree.
    /// </summary>
    /// <remarks>
    /// Validating that a parent assignment creates no cycle reads other groups, so two concurrent
    /// re-parents could each pass and together create one. A cycle can span groups that only share
    /// an interior node, so a per-group key would not serialize them; one global lock does. Take it
    /// only when assigning a parent, the sole change that can create a cycle. It is released when the
    /// surrounding transaction ends, so call it inside a transaction.
    /// </remarks>
    Task AcquireHierarchyLockAsync(CancellationToken cancellationToken = default);

    void Add(UserGroup userGroup);

    void Remove(UserGroup userGroup);
}
