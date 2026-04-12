namespace Fargo.Sdk.Articles;

/// <summary>
/// Lightweight representation of a length value as returned by the API.
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit abbreviation (e.g. "m", "cm", "mm", "in", "ft").</param>
public sealed record LengthDto(double Value, string Unit);
