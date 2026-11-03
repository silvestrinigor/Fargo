using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Command used to update an existing item from an API update payload.
/// </summary>
/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: the handler acquires a
/// containment advisory lock on the move path that must be held until commit, so
/// it runs inside a single explicit transaction owned by the pipeline.
/// </remarks>
public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Update
) : ITransactionalCommand;
