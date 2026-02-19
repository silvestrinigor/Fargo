namespace Fargo.Domain.Exceptions
{
    public class UserNotFoundException(Guid userGuid)
        : FargoException
    {
        public Guid UserGuid 
        {
            get;
        } = userGuid;
    }
}