namespace Fargo.Domain.Exceptions
{
    public class PartitionNotFoundException(Guid partitionGuid)
        : FargoException
    {
        public Guid PartitionGuid 
        {
            get;
        } = partitionGuid;
    }
}