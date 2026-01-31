namespace Fargo.Domain.Exceptions
{
    public class ItemNotFoundException(Guid itemGuid)
        : FargoException
    {
        public Guid ItemGuid 
        {
            get;
        } = itemGuid;
    }
}