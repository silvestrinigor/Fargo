using System.Text.Json.Serialization;
using Fargo.Sdk.Contracts;

namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents partial article metrics in API contracts.</summary>
public sealed record ArticleMetricsPatchInfo
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<MassInfo?> Mass { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<LengthInfo?> LengthX { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<LengthInfo?> LengthY { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<LengthInfo?> LengthZ { get; init; }
}
