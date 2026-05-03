using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

public class UserNotFoundFargoApplicationException
    : FargoApplicationException
{
    public UserNotFoundFargoApplicationException(Guid userGuid)
        : base($"User with guid '{userGuid}' was not found.")
    {
        UserGuid = userGuid;
    }

    public UserNotFoundFargoApplicationException(Nameid nameid)
        : base($"User with nameid '{nameid}' was not found.")
    {
        Nameid = nameid;
    }

    public Guid? UserGuid { get; }

    public Nameid? Nameid { get; }
}

public sealed class UserNotAuthorizedFargoApplicationException(
    Guid userGuid,
    ActionType actionType
) : FargoApplicationException(
    $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
{
    public Guid UserGuid { get; } = userGuid;

    public ActionType ActionType { get; } = actionType;
}
