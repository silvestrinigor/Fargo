namespace Fargo.Application.Articles;

/// <summary>
/// Physical measurements of an article on the wire (primitives only).
/// </summary>
public sealed record ArticleMetricsModel(
    MassModel? Mass = null,
    LengthModel? LengthX = null,
    LengthModel? LengthY = null,
    LengthModel? LengthZ = null,
    DensityModel? Density = null);

/// <summary>Mass value with unit abbreviation (e.g. "g", "kg").</summary>
public sealed record MassModel(double Value, string Unit);

/// <summary>Length value with unit abbreviation (e.g. "cm", "mm").</summary>
public sealed record LengthModel(double Value, string Unit);

/// <summary>Density value with unit abbreviation (e.g. "g/cm³").</summary>
public sealed record DensityModel(double Value, string Unit);
