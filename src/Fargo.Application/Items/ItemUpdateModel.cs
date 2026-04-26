namespace Fargo.Application.Items;

/// <summary>
/// Represents the data used to update an existing item.
/// </summary>
/// <param name="ProductionDate">
/// The new production date, or <see langword="null"/> to leave unchanged.
/// </param>
public record ItemUpdateModel(
    DateTimeOffset? ProductionDate = null
    );
