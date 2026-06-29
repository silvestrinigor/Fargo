using Fargo.Core.Entities;

namespace Fargo.Core.Items;

public sealed class ItemBatch : Entity
{
    /// <summary>
    /// Gets the date on which this batch was produced.
    /// When <see langword="null"/>, the production date is unknown.
    /// </summary>
    public DateTimeOffset? ProductionDate { get; init; }
}
