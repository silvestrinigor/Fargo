namespace Fargo.Application.Exceptions
{
    public class UserNotFoundFargoApplicationException(Guid userGuid)
        : FargoApplicationException()
    {
        public Guid UserGuid { get; } = userGuid;
    }
}