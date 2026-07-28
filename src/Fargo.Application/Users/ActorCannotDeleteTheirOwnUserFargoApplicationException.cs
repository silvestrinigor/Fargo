namespace Fargo.Application.Users;

public sealed class ActorCannotDeleteTheirOwnUser : FargoApplicationException
{
    public Guid UserGuid { get; }

    public ActorCannotDeleteTheirOwnUser(
        Guid userGuid) : base(
            $"Actor cannot delete their own user '{userGuid}'",
            FargoApplicationErrorType.ActorCannotDeleteTheirOwnUser)
    {
        UserGuid = userGuid;
    }
}