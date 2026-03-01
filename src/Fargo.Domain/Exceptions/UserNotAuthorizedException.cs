using Fargo.Domain.Enums;

namespace Fargo.Domain.Exceptions
{
    public class UserNotAuthorizedException(
            Guid userGuid,
            ActionType action
            ) : FargoException
    {
        public Guid UserGuid
        {
            get;
        } = userGuid;

        public ActionType ActionType
        {
            get;
        } = action;
    }
}