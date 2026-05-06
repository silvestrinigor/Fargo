namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents article metrics in API contracts.</summary>
public sealed record ArticleMetricsInfo(
    MassInfo? Mass = null,
    LengthInfo? LengthX = null,
    LengthInfo? LengthY = null,
    LengthInfo? LengthZ = null,
    DensityInfo? Density = null);
