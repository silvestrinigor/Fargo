using Fargo.Client.Models;
using Microsoft.Kiota.Abstractions.Serialization;

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

    public static ServiceAccountSummary ToSummary(this ServiceAccountDto dto) => new(
        dto.Guid,
        dto.ClientId,
        dto.Description,
        dto.IsActive,
        dto.Permissions?.Where(p => p.HasValue).Select(p => p!.Value).ToArray(),
        dto.PartitionAccessGuids?.Where(g => g.HasValue).Select(g => g!.Value).ToArray());

    public static ServiceAccountCreatedSummary ToCreatedSummary(this ServiceAccountCreatedDto dto) => new(
        dto.Guid,
        dto.ClientId,
        dto.ClientSecret);

    public static AuditLogSummary ToSummary(this AuditLogDto dto) => new(
        dto.Guid,
        dto.ActorGuid,
        dto.ActorType,
        dto.AuditLogType,
        dto.CommandName,
        dto.OccurredAt,
        dto.Metadata?.AdditionalData is { Count: > 0 } data
            ? data.ToDictionary(kv => kv.Key, kv => UntypedToPlain(kv.Value))
            : null);

    /// <summary>
    /// Converts a Kiota untyped JSON node (how audit metadata values are parsed)
    /// to plain CLR values the MCP serializer renders as ordinary JSON.
    /// </summary>
    private static object? UntypedToPlain(object? value) => value switch
    {
        null or UntypedNull => null,
        UntypedString s => s.GetValue(),
        UntypedBoolean b => b.GetValue(),
        UntypedInteger i => i.GetValue(),
        UntypedLong l => l.GetValue(),
        UntypedDecimal d => d.GetValue(),
        UntypedDouble d => d.GetValue(),
        UntypedFloat f => f.GetValue(),
        UntypedArray array => array.GetValue().Select(UntypedToPlain).ToArray(),
        UntypedObject obj => obj.GetValue().ToDictionary(kv => kv.Key, kv => UntypedToPlain(kv.Value)),
        _ => value.ToString()
    };

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

public sealed record ServiceAccountSummary(
    Guid? Guid,
    string? ClientId,
    string? Description,
    bool? IsActive,
    int[]? Permissions,
    Guid[]? PartitionAccessGuids);

/// <summary>
/// Result of creating a service account or rotating its secret. Carries the
/// plain client secret, which the server reveals only this once.
/// </summary>
public sealed record ServiceAccountCreatedSummary(
    Guid? Guid,
    string? ClientId,
    string? ClientSecret);

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
    int? AuditLogType,
    string? CommandName,
    DateTimeOffset? OccurredAt,
    IReadOnlyDictionary<string, object?>? Metadata);

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
