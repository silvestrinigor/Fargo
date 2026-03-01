using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Exceptions
{
    public class UserNotFoundFargoApplicationException
        : FargoApplicationException
    {
        public UserNotFoundFargoApplicationException(
                Guid? userGuid
                ) : base(
                    $"User with guid {userGuid} was not found."
                    )
        {
        }

        public UserNotFoundFargoApplicationException(
                Nameid? nameid
                ) : base(
                    $"User with nameid {nameid} was not found."
                    )
        {
        }

        public Guid? UserGuid { get; }

        public Nameid? Nameid { get; }
    }
}