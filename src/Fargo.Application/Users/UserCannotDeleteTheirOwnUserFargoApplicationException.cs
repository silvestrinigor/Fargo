namespace Fargo.Application.Users;

public sealed class UserCannotDeleteTheirOwnUserFargoApplicationException : FargoApplicationException
{
    public Guid UserGuid { get; }

    public UserCannotDeleteTheirOwnUserFargoApplicationException(
        Guid userGuid) : base(
            $"Actor cannot delete their own user '{userGuid}'",
            FargoApplicationErrorType.ActorCannotDeleteTheirOwnUser)
    {
        UserGuid = userGuid;
    }
}
