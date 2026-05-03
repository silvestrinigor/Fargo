namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents article metrics in API contracts.</summary>
public sealed record ArticleMetricsDto(
    MassDto? Mass = null,
    LengthDto? LengthX = null,
    LengthDto? LengthY = null,
    LengthDto? LengthZ = null,
    DensityDto? Density = null);
