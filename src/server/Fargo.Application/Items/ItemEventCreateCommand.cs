using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Records an item event (movement, creation, or deletion) for an event-tracked
/// article and returns the identifier of the recorded event.
/// </summary>
/// <param name="Create">The event to record.</param>
public sealed record ItemEventCreateCommand(ItemEventCreateDto Create) : ICommand<Guid>;
