namespace Fargo.Application.Exceptions;

/// <summary>
/// Exception thrown when an item with the specified identifier cannot be found.
/// </summary>
public class ItemNotFoundFargoApplicationException(Guid itemGuid)
    : FargoApplicationException($"Item with guid '{itemGuid}' was not found.")
{
    /// <summary>
    /// Gets the identifier of the item that could not be found.
    /// </summary>
    public Guid ItemGuid { get; } = itemGuid;
}
