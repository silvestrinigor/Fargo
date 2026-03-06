using Fargo.Domain.Enums;

namespace Fargo.Domain.Exceptions
{
    public class UserNotAuthorizedFargoDomainException(
            Guid userGuid,
            ActionType action
            ) : FargoDomainException
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