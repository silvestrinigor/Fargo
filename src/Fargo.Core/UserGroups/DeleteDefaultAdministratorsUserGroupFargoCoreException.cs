namespace Fargo.Core.UserGroups;

/// <summary>
/// Exception thrown when an attempt is made to delete the default administrators user group.
/// </summary>
/// <remarks>
/// The default administrators user group is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteDefaultAdministratorsUserGroupFargoCoreException()
    : FargoCoreException(
        "The default administrators user group cannot be deleted.",
        FargoCoreErrorType.CannotDeleteMainAdministratorsUserGroup)
{
}
