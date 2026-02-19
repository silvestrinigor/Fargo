namespace Fargo.Application.Exceptions
{
    public class ItemNotFoundFargoApplicationException(Guid itemGuid)
        : FargoApplicationException()
    {
        public Guid ItemGuid { get; } = itemGuid;
    }
}