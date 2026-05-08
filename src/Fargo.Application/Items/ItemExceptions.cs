namespace Fargo.Application.Items;

public class ItemNotFoundFargoApplicationException(Guid itemGuid)
    : FargoApplicationException($"Item with guid '{itemGuid}' was not found.")
{
    public Guid ItemGuid { get; } = itemGuid;
}
