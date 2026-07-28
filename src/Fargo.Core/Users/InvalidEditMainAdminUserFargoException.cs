namespace Fargo.Core.Users;

public sealed class InvalidEditMainAdminUserFargoException
    : FargoCoreException
{
    public static Guid AdminGuid => FargoCoreGuids.AdminUserGuid;

    public InvalidEditMainAdminUserFargoException()
        : base($"The main administrator user {FargoCoreGuids.AdminUserGuid} cannot edit this property.") { }

    public InvalidEditMainAdminUserFargoException(string message)
        : base(message) { }
}
