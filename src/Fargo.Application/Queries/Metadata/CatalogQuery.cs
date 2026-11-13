using Fargo.Application.Abstractions;
using Fargo.Application.Dtos.Metadata;
using Fargo.Core.Audits;
using Fargo.Core.Barcodes;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Events;

namespace Fargo.Application.Queries.Metadata;

/// <summary>
/// Reference catalogs the API exposes. Each one lists the numeric codes the API accepts and returns
/// for an enum, so callers do not have to hard-code them.
/// </summary>
public enum MetadataCatalog
{
    EntityTypes = 1,
    AuditLogTypes = 2,
    ActionTypes = 3,
    ActorTypes = 4,
    ArticleTypes = 5,
    ArticleTrackingModes = 6,
    BarcodeFormats = 7,
    ItemEventTypes = 8,
    SupplyOrderStatuses = 9,
    ErrorTypes = 10,
}

public sealed record CatalogQuery(MetadataCatalog Catalog) : IQuery<IReadOnlyList<CatalogEntryDto>>;

/// <remarks>
/// The values come from the enums compiled into the application, so no persistence is involved and
/// the result never changes while the process runs.
/// </remarks>
public sealed class CatalogQueryHandler : IQueryHandler<CatalogQuery, IReadOnlyList<CatalogEntryDto>>
{
    private static readonly IReadOnlyDictionary<MetadataCatalog, IReadOnlyList<CatalogEntryDto>> catalogs =
        new Dictionary<MetadataCatalog, IReadOnlyList<CatalogEntryDto>>
        {
            [MetadataCatalog.EntityTypes] = Describe<EntityType>(),
            [MetadataCatalog.AuditLogTypes] = Describe<AuditLogType>(),
            [MetadataCatalog.ActionTypes] = Describe<ActionType>(),
            [MetadataCatalog.ActorTypes] = Describe<ActorType>(),
            [MetadataCatalog.ArticleTypes] = Describe<ArticleType>(),
            [MetadataCatalog.ArticleTrackingModes] = Describe<ArticleTrackingMode>(),
            [MetadataCatalog.BarcodeFormats] = Describe<BarcodeFormat>(),
            [MetadataCatalog.ItemEventTypes] = Describe<ItemEventType>(),
            [MetadataCatalog.SupplyOrderStatuses] = Describe<SupplyOrderStatus>(),
            [MetadataCatalog.ErrorTypes] = Describe<FargoErrorType>(),
        };

    public Task<IReadOnlyList<CatalogEntryDto>> HandleAsync(CatalogQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(catalogs[query.Catalog]);
    }

    private static IReadOnlyList<CatalogEntryDto> Describe<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(value => new CatalogEntryDto(Convert.ToInt32(value), value.ToString()))
            .OrderBy(entry => entry.Id)
            .ToArray();
    }
}
