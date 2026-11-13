using Fargo.ClientHttp;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public static class MetadataTools
{
    private const string Catalogs =
        "entity-types, audit-log-types, action-types, actor-types, article-types, article-tracking-modes, "
        + "barcode-formats, item-event-types, supply-order-statuses, error-types";

    [McpServerTool(Name = "fargo_metadata_get")]
    [Description("List the numeric codes (id) and names the API uses for an enumeration. Use it to translate codes such as entity types, audit log types or permissions.")]
    public static async Task<IReadOnlyList<CatalogEntrySummary>> GetCatalogAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The catalog to list. One of: " + Catalogs + ".")] string catalog,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var m = client.V1.Metadata;

        var entries = catalog.Trim().ToLowerInvariant() switch
        {
            "entity-types" => await m.EntityTypes.GetAsync(cancellationToken: cancellationToken),
            "audit-log-types" => await m.AuditLogTypes.GetAsync(cancellationToken: cancellationToken),
            "action-types" => await m.ActionTypes.GetAsync(cancellationToken: cancellationToken),
            "actor-types" => await m.ActorTypes.GetAsync(cancellationToken: cancellationToken),
            "article-types" => await m.ArticleTypes.GetAsync(cancellationToken: cancellationToken),
            "article-tracking-modes" => await m.ArticleTrackingModes.GetAsync(cancellationToken: cancellationToken),
            "barcode-formats" => await m.BarcodeFormats.GetAsync(cancellationToken: cancellationToken),
            "item-event-types" => await m.ItemEventTypes.GetAsync(cancellationToken: cancellationToken),
            "supply-order-statuses" => await m.SupplyOrderStatuses.GetAsync(cancellationToken: cancellationToken),
            "error-types" => await m.ErrorTypes.GetAsync(cancellationToken: cancellationToken),
            _ => throw new ArgumentException($"Unknown catalog '{catalog}'. Use one of: {Catalogs}.", nameof(catalog)),
        };

        return entries?.Select(e => new CatalogEntrySummary(e.Id, e.Name)).ToArray() ?? [];
    }
}

public sealed record CatalogEntrySummary(int? Id, string? Name);
