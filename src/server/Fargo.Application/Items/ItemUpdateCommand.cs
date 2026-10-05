using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Command used to update an existing item from an API update payload.
/// </summary>
public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Update
) : ICommand;
