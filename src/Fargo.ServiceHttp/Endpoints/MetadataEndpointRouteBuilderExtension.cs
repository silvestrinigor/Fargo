using Fargo.Application.Abstractions;
using Fargo.Application.Dtos.Metadata;
using Fargo.Application.Queries.Metadata;
using Fargo.ServiceHttp.Versioning;

namespace Fargo.ServiceHttp.Endpoints;

public static class MetadataEndpointRouteBuilderExtension
{
    public static void MapFargoMetadata(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapMetadataGroup();

        group.MapCatalog("/entity-types", "GetEntityTypes", "Gets the entity types", MetadataCatalog.EntityTypes);
        group.MapCatalog("/audit-log-types", "GetAuditLogTypes", "Gets the audit log types", MetadataCatalog.AuditLogTypes);
        group.MapCatalog("/action-types", "GetActionTypes", "Gets the action (permission) types", MetadataCatalog.ActionTypes);
        group.MapCatalog("/actor-types", "GetActorTypes", "Gets the actor types", MetadataCatalog.ActorTypes);
        group.MapCatalog("/article-types", "GetArticleTypes", "Gets the article types", MetadataCatalog.ArticleTypes);
        group.MapCatalog("/article-tracking-modes", "GetArticleTrackingModes", "Gets the article tracking modes", MetadataCatalog.ArticleTrackingModes);
        group.MapCatalog("/barcode-formats", "GetBarcodeFormats", "Gets the barcode formats", MetadataCatalog.BarcodeFormats);
        group.MapCatalog("/item-event-types", "GetItemEventTypes", "Gets the item event types", MetadataCatalog.ItemEventTypes);
        group.MapCatalog("/supply-order-statuses", "GetSupplyOrderStatuses", "Gets the supply order statuses", MetadataCatalog.SupplyOrderStatuses);
        group.MapCatalog("/error-types", "GetErrorTypes", "Gets the error types", MetadataCatalog.ErrorTypes);
    }

    private static RouteGroupBuilder MapMetadataGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("Metadata", "/metadata")
        .RequireAuthorization()
        .WithTags("Metadata")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    private static void MapCatalog(
        this IEndpointRouteBuilder builder,
        string pattern,
        string name,
        string summary,
        MetadataCatalog catalog)
    {
        builder
        .MapGet(pattern, async (
            IQueryHandler<CatalogQuery, IReadOnlyList<CatalogEntryDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(new CatalogQuery(catalog), cancellationToken);

            return TypedResults.Ok(response);
        })
        .WithName(name)
        .WithSummary(summary)
        .WithDescription($"{summary} as id and name pairs. The id is the numeric code the API accepts and returns; the values are fixed by the application and are not read from the database.")
        .Produces<IReadOnlyList<CatalogEntryDto>>(StatusCodes.Status200OK);
    }
}
