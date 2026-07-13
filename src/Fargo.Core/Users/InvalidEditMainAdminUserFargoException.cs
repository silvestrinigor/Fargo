namespace Fargo.Core.Users;

public sealed class InvalidEditMainAdminUserFargoException
    : FargoException
{
    public static Guid AdminGuid => FargoConstantGuids.AdminUserGuid;

    public InvalidEditMainAdminUserFargoException()
        : base($"The main administrator user {FargoConstantGuids.AdminUserGuid} cannot edit this property.") { }

    public InvalidEditMainAdminUserFargoException(string message)
        : base(message) { }
}
