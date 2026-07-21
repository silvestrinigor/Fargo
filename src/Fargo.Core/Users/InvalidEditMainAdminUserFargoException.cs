namespace Fargo.Core.Users;

public sealed class InvalidEditMainAdminUserFargoException
    : FargoCoreException
{
    public static Guid AdminGuid => FargoDefaultGuids.AdminUserGuid;

    public InvalidEditMainAdminUserFargoException()
        : base($"The main administrator user {FargoDefaultGuids.AdminUserGuid} cannot edit this property.") { }

    public InvalidEditMainAdminUserFargoException(string message)
        : base(message) { }
}
