using System.Text.Json.Serialization;

namespace Fargo.Api.Articles;

/// <summary>
/// Represents a physical mass value as returned by the API.
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit of measurement.</param>
[JsonConverter(typeof(MassJsonConverter))]
public sealed record Mass(double Value, MassUnit Unit)
{
    /// <summary>Returns the API abbreviation string for the unit (e.g. "kg", "g").</summary>
    public string ToAbbreviation() => Unit switch
    {
        MassUnit.Gram => "g",
        MassUnit.Kilogram => "kg",
        MassUnit.Milligram => "mg",
        MassUnit.Pound => "lb",
        MassUnit.Ounce => "oz",
        _ => "g"
    };

    /// <summary>Parses an API abbreviation string (e.g. "kg") into a <see cref="MassUnit"/>.</summary>
    public static MassUnit ParseUnit(string abbreviation) => abbreviation.ToLowerInvariant() switch
    {
        "g" => MassUnit.Gram,
        "kg" => MassUnit.Kilogram,
        "mg" => MassUnit.Milligram,
        "lb" => MassUnit.Pound,
        "oz" => MassUnit.Ounce,
        _ => throw new ArgumentException($"Unknown mass unit '{abbreviation}'.", nameof(abbreviation))
    };
}
