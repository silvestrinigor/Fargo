using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Exceptions
{
    public class UserActorSetDiferentUserPasswordException(
            Actor actor,
            User user
            ) : FargoException
    {
        public Actor Actor
        {
            get;
        } = actor;

        public User User
        {
            get;
        } = user;
    }
}