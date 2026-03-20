using System.Text.Json.Serialization;

namespace Fargo.Web.Features.Partitions;

public sealed record PartitionSummary(
    [property: JsonPropertyName("guid")] Guid Guid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("parentPartitionGuid")] Guid? ParentPartitionGuid,
    [property: JsonPropertyName("isActive")] bool IsActive = true);
