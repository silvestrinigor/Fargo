namespace Fargo.Application.Security
{
    public interface ICurrentUser
    {
        Guid UserGuid
        {
            get;
        }

        bool IsAuthenticated
        {
            get;
        }
    }
}