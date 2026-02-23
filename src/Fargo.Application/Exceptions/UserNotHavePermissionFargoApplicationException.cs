using Fargo.Domain.Enums;

namespace Fargo.Application.Exceptions
{
    public class UserNotHavePermissionFargoApplicationException(Guid userGuid, ActionType action)
        : FargoApplicationException()
    {
        public Guid UserGuid { get; } = userGuid;

        public ActionType ActionType { get; } = action;
    }
}