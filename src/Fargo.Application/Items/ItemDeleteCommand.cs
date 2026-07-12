namespace Fargo.Application.Items;

/// <summary>
/// Command used to delete an item.
/// </summary>
public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;
