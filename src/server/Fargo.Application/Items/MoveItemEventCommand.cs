using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Records a movement event for an event-tracked article.
/// </summary>
/// <param name="Create">The movement to record.</param>
public sealed record MoveItemEventCommand(ItemMovementCreateDto Create) : ICommand;
