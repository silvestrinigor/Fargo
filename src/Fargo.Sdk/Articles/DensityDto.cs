namespace Fargo.Sdk.Articles;

/// <summary>
/// Lightweight representation of a volumetric density value computed by the SDK.
/// The unit is chosen to match the natural unit for the article's mass and dimension units
/// (e.g. <c>"g/cm³"</c> when mass is in grams and dimensions are in centimetres).
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit abbreviation (e.g. "kg/m³", "g/cm³", "lb/ft³").</param>
public sealed record DensityDto(double Value, string Unit);
