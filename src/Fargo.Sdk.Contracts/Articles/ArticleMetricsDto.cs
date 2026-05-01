namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents a mass measurement in API contracts.</summary>
public sealed record MassDto(double Value, string Unit);

/// <summary>Represents a length measurement in API contracts.</summary>
public sealed record LengthDto(double Value, string Unit);

/// <summary>Represents a density measurement in API contracts.</summary>
public sealed record DensityDto(double Value, string Unit);

/// <summary>Represents article metrics in API contracts.</summary>
public sealed record ArticleMetricsDto(
    MassDto? Mass = null,
    LengthDto? LengthX = null,
    LengthDto? LengthY = null,
    LengthDto? LengthZ = null,
    DensityDto? Density = null);
