using System.Text.Json.Serialization;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a physical length value as returned by the API.
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit of measurement.</param>
[JsonConverter(typeof(LengthJsonConverter))]
public sealed record Length(double Value, LengthUnit Unit)
{
    /// <summary>Returns the API abbreviation string for the unit (e.g. "m", "cm").</summary>
    public string ToAbbreviation() => Unit switch
    {
        LengthUnit.Millimeter => "mm",
        LengthUnit.Centimeter => "cm",
        LengthUnit.Meter => "m",
        LengthUnit.Kilometer => "km",
        LengthUnit.Inch => "in",
        LengthUnit.Foot => "ft",
        _ => "m"
    };

    /// <summary>Parses an API abbreviation string (e.g. "cm") into a <see cref="LengthUnit"/>.</summary>
    public static LengthUnit ParseUnit(string abbreviation) => abbreviation.ToLowerInvariant() switch
    {
        "mm" => LengthUnit.Millimeter,
        "cm" => LengthUnit.Centimeter,
        "m" => LengthUnit.Meter,
        "km" => LengthUnit.Kilometer,
        "in" => LengthUnit.Inch,
        "ft" => LengthUnit.Foot,
        _ => throw new ArgumentException($"Unknown length unit '{abbreviation}'.", nameof(abbreviation))
    };
}
