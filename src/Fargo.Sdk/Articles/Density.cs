using System.Text.Json.Serialization;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a volumetric density value computed by the SDK.
/// The unit is chosen to match the natural unit for the article's mass and dimension units
/// (e.g. <see cref="DensityUnit.GramPerCubicCentimeter"/> when mass is in grams and dimensions
/// are in centimetres).
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit of measurement.</param>
[JsonConverter(typeof(DensityJsonConverter))]
public sealed record Density(double Value, DensityUnit Unit)
{
    /// <summary>Returns the API abbreviation string for the unit (e.g. "kg/m³").</summary>
    public string ToAbbreviation() => Unit switch
    {
        DensityUnit.KilogramPerCubicMeter => "kg/m³",
        DensityUnit.KilogramPerCubicCentimeter => "kg/cm³",
        DensityUnit.KilogramPerCubicMillimeter => "kg/mm³",
        DensityUnit.GramPerCubicMeter => "g/m³",
        DensityUnit.GramPerCubicCentimeter => "g/cm³",
        DensityUnit.GramPerCubicMillimeter => "g/mm³",
        DensityUnit.PoundPerCubicFoot => "lb/ft³",
        DensityUnit.PoundPerCubicInch => "lb/in³",
        _ => "kg/m³"
    };

    /// <summary>Parses an API abbreviation string into a <see cref="DensityUnit"/>.</summary>
    public static DensityUnit ParseUnit(string abbreviation) => abbreviation switch
    {
        "kg/m³" => DensityUnit.KilogramPerCubicMeter,
        "kg/cm³" => DensityUnit.KilogramPerCubicCentimeter,
        "kg/mm³" => DensityUnit.KilogramPerCubicMillimeter,
        "g/m³" => DensityUnit.GramPerCubicMeter,
        "g/cm³" => DensityUnit.GramPerCubicCentimeter,
        "g/mm³" => DensityUnit.GramPerCubicMillimeter,
        "lb/ft³" => DensityUnit.PoundPerCubicFoot,
        "lb/in³" => DensityUnit.PoundPerCubicInch,
        _ => throw new ArgumentException($"Unknown density unit '{abbreviation}'.", nameof(abbreviation))
    };
}
