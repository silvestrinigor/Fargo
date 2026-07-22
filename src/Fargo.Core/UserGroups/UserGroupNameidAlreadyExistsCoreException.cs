using Fargo.Core.Shared;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Exception thrown when attempting to create a <see cref="Fargo.Core.UserGroups.UserGroup"/>
/// with a <see cref="Nameid"/> that already exists in the system.
/// </summary>
public sealed class UserGroupNameidAlreadyExistsCoreException(Nameid nameid)
    : FargoCoreException(
        $"A user group with nameid '{nameid}' already exists.",
        FargoCoreErrorType.UserGroupNameidAlrealdyInUse)
{
    /// <summary>
    /// Gets the conflicting <see cref="Nameid"/>.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
