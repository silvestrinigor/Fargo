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

        IReadOnlySet<Guid> PartitionGuids
        {
            get;
        }
    }
}