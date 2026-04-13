namespace Fargo.Sdk.Articles;

/// <summary>
/// Lightweight representation of a mass value as returned by the API.
/// </summary>
/// <param name="Value">The numeric magnitude.</param>
/// <param name="Unit">The unit abbreviation (e.g. "g", "kg").</param>
public sealed record MassDto(double Value, string Unit);
