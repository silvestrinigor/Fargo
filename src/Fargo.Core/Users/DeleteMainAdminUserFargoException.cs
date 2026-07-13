namespace Fargo.Core.Users;

/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoException()
    : FargoException($"The main administrator user {FargoConstantGuids.AdminUserGuid} cannot be deleted.")
{
    public static Guid AdminGuid => FargoConstantGuids.AdminUserGuid;
}
