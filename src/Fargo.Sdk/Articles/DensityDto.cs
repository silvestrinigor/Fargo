namespace Fargo.Sdk.Articles;

/// <summary>
/// Lightweight representation of a volumetric density value computed by the SDK.
/// </summary>
/// <param name="Value">The numeric magnitude (always in kg/m³).</param>
/// <param name="Unit">The unit abbreviation — always <c>"kg/m³"</c>.</param>
public sealed record DensityDto(double Value, string Unit);
