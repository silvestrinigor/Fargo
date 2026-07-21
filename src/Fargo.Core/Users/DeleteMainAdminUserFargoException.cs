namespace Fargo.Core.Users;

/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoCoreException()
    : FargoCoreException(
        $"The main administrator user {FargoDefaultGuids.AdminUserGuid} cannot be deleted.",
        FargoCoreErrorType.CannotDeleteMainAdminUser)
{
    public static Guid AdminGuid => FargoDefaultGuids.AdminUserGuid;
}
