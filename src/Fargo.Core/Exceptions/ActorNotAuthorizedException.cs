using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Exceptions
{
    public class ActorNotAuthorizedException(
            Actor actor,
            ActionType action
            ) : FargoException
    {
        public Actor Actor
        {
            get;
        } = actor;

        public ActionType ActionType
        {
            get;
        } = action;
    }
}