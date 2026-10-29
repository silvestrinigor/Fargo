using Fargo.ClientHttp.Models;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Compact, serialization-friendly projections of the Kiota DTOs. The MCP SDK
/// serializes tool return values to JSON for the model; these records keep the
/// payloads small and stable (only the fields an agent needs), avoiding the
/// generated DTOs' additional-data bags and nested value objects.
/// </summary>
public static class ToolResults
{
    public static ArticleSummary ToSummary(this ArticleDto dto) => new(
        dto.Guid,
        dto.Name,
        dto.Description,
        dto.ArticleType,
        dto.TrackingMode,
        dto.Partitions?.Where(p => p.HasValue).Select(p => p!.Value).ToArray());

    public static PartitionSummary ToSummary(this PartitionDto dto) => new(
        dto.Guid,
        dto.Name,
        dto.Description,
        dto.IsGlobalPartition,
        dto.ParentPartitionGuid);

    public static ItemSummary ToSummary(this ItemDto dto) => new(
        dto.Guid,
        dto.ArticleGuid,
        dto.ParentContainerGuid,
        dto.IsFixed,
        dto.Partitions?.Where(p => p.HasValue).Select(p => p!.Value).ToArray());

    public static UserSummary ToSummary(this UserDto dto) => new(
        dto.Guid,
        dto.Nameid,
        dto.IsActive,
        dto.IsAdmin);

    public static UserGroupSummary ToSummary(this UserGroupDto dto) => new(
        dto.Guid,
        dto.Nameid,
        dto.Description,
        dto.IsActive,
        dto.IsAdminUserGroup,
        dto.ParentUserGroup);

    public static AuditLogSummary ToSummary(this AuditLogDto dto) => new(
        dto.Guid,
        dto.ActorGuid,
        dto.ActorType,
        dto.EntityGuid,
        dto.EntityType,
        dto.AuditLogType,
        dto.OccurredAt);

    public static ArticleInventorySummary ToSummary(this ArticleInventoryDto dto) => new(
        dto.TotalCount);

    public static ItemContainerInventorySummary ToSummary(this ItemContainerInventoryDto dto) => new(
        dto.ArticleGuid,
        dto.TotalCount);

    public static ItemMovimentSummary ToSummary(this ItemMovimentDto dto) => new(
        dto.MovedToItemContainerGuid,
        dto.OccurredAt);

    public static ItemEventSummary ToSummary(this ItemEventDto dto) => new(
        dto.Guid,
        dto.ArticleGuid,
        dto.EventType,
        dto.OccuredAt,
        dto.Moviment?.Quantity?.Double,
        dto.Moviment?.FromItemContainerGuid,
        dto.Moviment?.ToItemContainerGuid,
        dto.Lifecycle?.Quantity?.Double,
        dto.Lifecycle?.ItemContainerGuid);
}

public sealed record ArticleSummary(
    Guid? Guid,
    string? Name,
    string? Description,
    int? ArticleType,
    int? TrackingMode,
    Guid[]? Partitions);

public sealed record PartitionSummary(
    Guid? Guid,
    string? Name,
    string? Description,
    bool? IsGlobalPartition,
    Guid? ParentPartitionGuid);

public sealed record ItemSummary(
    Guid? Guid,
    Guid? ArticleGuid,
    Guid? ParentContainerGuid,
    bool? IsFixed,
    Guid[]? Partitions);

public sealed record UserSummary(
    Guid? Guid,
    string? Nameid,
    bool? IsActive,
    bool? IsAdmin);

public sealed record UserGroupSummary(
    Guid? Guid,
    string? Nameid,
    string? Description,
    bool? IsActive,
    bool? IsAdminUserGroup,
    Guid? ParentUserGroup);

public sealed record AuditLogSummary(
    Guid? Guid,
    Guid? ActorGuid,
    int? ActorType,
    Guid? EntityGuid,
    int? EntityType,
    int? AuditLogType,
    DateTimeOffset? OccurredAt);

public sealed record ArticleInventorySummary(
    double? TotalCount);

public sealed record ItemContainerInventorySummary(
    Guid? ArticleGuid,
    double? TotalCount);

public sealed record ItemMovimentSummary(
    Guid? MovedToItemContainerGuid,
    DateTimeOffset? OccurredAt);

public sealed record ItemEventSummary(
    Guid? Guid,
    Guid? ArticleGuid,
    int? EventType,
    DateTimeOffset? OccuredAt,
    double? MovimentQuantity,
    Guid? FromItemContainerGuid,
    Guid? ToItemContainerGuid,
    double? LifecycleQuantity,
    Guid? ItemContainerGuid);

/// <summary>
/// Numeric values of the server's <c>ItemEventType</c> enum, which the API
/// serializes as integers.
/// </summary>
public static class ItemEventTypes
{
    /// <summary>A movement of a quantity between containers.</summary>
    public const int Moviment = 1;

    /// <summary>The creation of a unit of stock.</summary>
    public const int Creation = 2;

    /// <summary>The deletion of a previously created unit of stock.</summary>
    public const int Deletion = 3;
}

/// <summary>
/// Numeric values of the server's <c>ArticleTrackingMode</c> enum, which the API
/// serializes as integers.
/// </summary>
public static class ArticleTrackingModes
{
    /// <summary>Each physical unit is tracked individually as its own item.</summary>
    public const int Entity = 1;

    /// <summary>Stock is tracked as aggregate quantities via movement events.</summary>
    public const int Event = 2;
}
