using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;

namespace Fargo.WorkflowTest;

/// <summary>
/// Exercises and verifies the Fargo inventory calculations through the public
/// HTTP client, asserting the computed numbers against known expected values:
///
///  • Article inventory (<c>GET /v1/articles/{guid}/inventory</c>) for an
///    entity-tracked article: total item count, direct-container count
///    (<c>includeDescendents=false</c>) and descendant-inclusive count
///    (<c>includeDescendents=true</c>, the default), plus a point-in-time count
///    reconstructed from containment history (<c>at</c>).
///  • Item-container inventory (<c>GET /v1/items/{guid}/inventory</c>): the
///    per-article contents of a container, computed recursively.
///  • The event-based tracking path (<c>POST/GET /v1/item/events</c>): because
///    the public API can only create entity-tracked articles, the scenario
///    asserts that recording an item event against such an article is rejected,
///    and that listing item events honours its filters and permission.
///
/// Containment layout built for the assertions (all items are of one article):
///
///   Shelf (container)                 ← 2 items directly
///     └── Tote (nested container)      ← 3 items directly
///   Pallet (container)                ← 1 item directly, then emptied
///
/// So: Shelf direct = 2, Shelf incl. descendants = 2 + 3 = 5, Pallet = 1,
/// article total = 2 + 3 + 1 = 6.
/// </summary>
internal sealed class InventoryScenario(
    FargoApiClient admin,
    WorkflowOptions options,
    string runTag,
    Check check)
{
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeContainer = 5;

    // ItemEventType codes (Fargo.Core Items/Events/ItemEventType).
    private const int EventTypeMoviment = 1;
    private const int EventTypeCreation = 2;

    private readonly FargoApiClient _admin = admin;
    private readonly WorkflowOptions _options = options;
    private readonly string _tag = runTag;
    private readonly Check _check = check;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var layout = await BuildContainmentLayoutAsync(cancellationToken);

        await VerifyArticleInventoryAsync(layout, cancellationToken);
        await VerifyContainerInventoryAsync(layout, cancellationToken);
        await VerifyPointInTimeInventoryAsync(layout, cancellationToken);
        await VerifyEventTrackingPathAsync(layout, cancellationToken);
    }

    private async Task<Layout> BuildContainmentLayoutAsync(CancellationToken cancellationToken)
    {
        Log.Step("Inventory: building a known containment layout");

        // Three container items.
        var shelf = await CreateContainerAsync("Shelf", cancellationToken);
        var tote = await CreateContainerAsync("Tote", cancellationToken);
        var pallet = await CreateContainerAsync("Pallet", cancellationToken);

        // Nest Tote inside Shelf so Shelf's descendant-inclusive inventory covers
        // Tote's contents.
        await _admin.V1.Items[tote].PatchAsync(
            new ItemUpdateDto { ParentItemContainerGuid = shelf },
            cancellationToken: cancellationToken);

        // One product article, tracked as entities (the only mode the API creates).
        var articleGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto { Name = N("inv-widget"), ArticleType = ArticleTypeDefault },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create inventory article.");

        // 2 items directly in Shelf, 3 in Tote, 1 in Pallet  => 6 total.
        var shelfItems = await CreateItemsInAsync(articleGuid, shelf, 2, cancellationToken);
        var toteItems = await CreateItemsInAsync(articleGuid, tote, 3, cancellationToken);
        var palletItems = await CreateItemsInAsync(articleGuid, pallet, 1, cancellationToken);

        Log.Success($"Article {Short(articleGuid)}: Shelf(2) > Tote(3), Pallet(1) = 6 items total");

        return new Layout(articleGuid, shelf, tote, pallet,
            [.. shelfItems], [.. toteItems], [.. palletItems]);
    }

    private async Task VerifyArticleInventoryAsync(Layout layout, CancellationToken cancellationToken)
    {
        Log.Step("Inventory: verifying article inventory counts");

        // Total across the whole system (no container filter).
        var total = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(6d, total?.TotalCount, "article total inventory = 6");

        // Pallet only (direct, no descendants): 1.
        var pallet = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Pallet];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(1d, pallet?.TotalCount, "article inventory in Pallet (direct) = 1");

        // Shelf direct only (descendants OFF): the 2 items placed directly in Shelf
        // (Tote itself is a container, not an item of this article).
        var shelfDirect = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Shelf];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(2d, shelfDirect?.TotalCount, "article inventory in Shelf (direct only) = 2");

        // Shelf including descendants (Tote nested inside): 2 + 3 = 5.
        var shelfDeep = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Shelf];
            cfg.QueryParameters.IncludeDescendents = true;
        }, cancellationToken);
        _check.Equal(5d, shelfDeep?.TotalCount, "article inventory in Shelf (incl. descendants) = 5");

        // Two containers at once (Shelf subtree + Pallet): 5 + 1 = 6.
        var union = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Shelf, layout.Pallet];
            cfg.QueryParameters.IncludeDescendents = true;
        }, cancellationToken);
        _check.Equal(6d, union?.TotalCount, "article inventory in Shelf+Pallet (incl. descendants) = 6");
    }

    private async Task VerifyContainerInventoryAsync(Layout layout, CancellationToken cancellationToken)
    {
        Log.Step("Inventory: verifying container (item) inventory contents");

        // The HTTP item-inventory endpoint always recurses into descendants, so
        // Shelf reports 2 (direct) + 3 (Tote) = 5 of the article.
        var shelf = await _admin.V1.Items[layout.Shelf].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        var shelfRow = shelf.FirstOrDefault(r => r.ArticleGuid == layout.ArticleGuid);
        _check.That(shelfRow is not null, "Shelf inventory lists the article");
        _check.Equal(5d, shelfRow?.TotalCount, "Shelf container inventory of article = 5 (recursive)");

        // Tote on its own reports exactly its 3 direct items.
        var tote = await _admin.V1.Items[layout.Tote].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        _check.Equal(3d, tote.FirstOrDefault(r => r.ArticleGuid == layout.ArticleGuid)?.TotalCount,
            "Tote container inventory of article = 3");

        // Pallet reports its 1 item.
        var pallet = await _admin.V1.Items[layout.Pallet].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        _check.Equal(1d, pallet.FirstOrDefault(r => r.ArticleGuid == layout.ArticleGuid)?.TotalCount,
            "Pallet container inventory of article = 1");

        // Empty the Pallet (remove its single item from all containers) and assert
        // the container now reports no rows for the article (zero-net is dropped).
        await _admin.V1.Items[layout.PalletItems[0]].PatchAsync(
            new ItemUpdateDto { RemoveFromParentItemContainer = true },
            cancellationToken: cancellationToken);

        var palletAfter = await _admin.V1.Items[layout.Pallet].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        _check.That(
            palletAfter.All(r => r.ArticleGuid != layout.ArticleGuid),
            "emptied Pallet reports no inventory for the article");

        // Put it back so later point-in-time checks have a stable 'current' state.
        await _admin.V1.Items[layout.PalletItems[0]].PatchAsync(
            new ItemUpdateDto { ParentItemContainerGuid = layout.Pallet },
            cancellationToken: cancellationToken);
    }

    private async Task VerifyPointInTimeInventoryAsync(Layout layout, CancellationToken cancellationToken)
    {
        Log.Step("Inventory: verifying point-in-time ('at') reconstruction");

        // Snapshot the current instant, then change the layout: move all three
        // Tote items out to the Pallet. 'now' should still reflect the old
        // placement; the live query should reflect the new one.
        var snapshot = DateTimeOffset.UtcNow;

        // Give the history a clear time gap so 'snapshot' is unambiguously before
        // the moves (containment history is stamped at UTC now).
        await Task.Delay(1500, cancellationToken);

        foreach (var item in layout.ToteItems)
        {
            await _admin.V1.Items[item].PatchAsync(
                new ItemUpdateDto { ParentItemContainerGuid = layout.Pallet },
                cancellationToken: cancellationToken);
        }

        // Live: Tote now holds 0, Pallet holds 1 + 3 = 4.
        var toteNow = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Tote];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(0d, toteNow?.TotalCount, "live: Tote now holds 0 of the article");

        var palletNow = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Pallet];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(4d, palletNow?.TotalCount, "live: Pallet now holds 4 of the article");

        // As-of the snapshot: Tote still held its original 3.
        var toteThen = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [layout.Tote];
            cfg.QueryParameters.IncludeDescendents = false;
            cfg.QueryParameters.At = snapshot;
        }, cancellationToken);
        _check.Equal(3d, toteThen?.TotalCount, "point-in-time: Tote held 3 at the snapshot");

        // The article total is unchanged by moves (6 then and now).
        var totalThen = await _admin.V1.Articles[layout.ArticleGuid.ToString()].Inventory.GetAsync(cfg =>
            cfg.QueryParameters.At = snapshot, cancellationToken);
        _check.Equal(6d, totalThen?.TotalCount, "point-in-time: article total was 6 at the snapshot");
    }

    private async Task VerifyEventTrackingPathAsync(Layout layout, CancellationToken cancellationToken)
    {
        Log.Step("Inventory: verifying the item-events (event-tracked) path");

        // The public API only creates entity-tracked articles. Recording an item
        // event against such an article must be rejected by the server (the
        // domain forbids events on non-event-tracked articles). Accept 400 (the
        // domain invariant surfaces as a bad request) or 403/404.
        await _check.DeniedAsync(
            "recording a movement event against an entity-tracked article is rejected",
            () => _admin.V1.Item.Events.PostAsync(
                new ItemEventCreateDto
                {
                    ArticleGuid = layout.ArticleGuid,
                    EventType = EventTypeMoviment,
                    Quantity = new Scalar { Double = 5 },
                    ToItemContainerGuid = layout.Shelf,
                },
                cancellationToken: cancellationToken),
            400, 403, 404);

        await _check.DeniedAsync(
            "recording a creation event against an entity-tracked article is rejected",
            () => _admin.V1.Item.Events.PostAsync(
                new ItemEventCreateDto
                {
                    ArticleGuid = layout.ArticleGuid,
                    EventType = EventTypeCreation,
                    Quantity = new Scalar { Double = 10 },
                    ItemContainerGuid = layout.Shelf,
                },
                cancellationToken: cancellationToken),
            400, 403, 404);

        // Listing item events (ReadItemEvents permission) works for the admin and,
        // filtered to this entity-tracked article, returns no events.
        await _check.AllowedAsync("admin lists item events (ReadItemEvents)", async () =>
        {
            var page = await _admin.V1.Item.Events.GetAsync(cfg =>
            {
                cfg.QueryParameters.ArticleGuid = layout.ArticleGuid;
                cfg.QueryParameters.Page = 1;
                cfg.QueryParameters.Limit = 10;
            }, cancellationToken);

            _check.Equal(0, page?.Items?.Count ?? 0,
                "no item events exist for the entity-tracked article");
        });
    }

    private async Task<Guid> CreateContainerAsync(string baseName, CancellationToken cancellationToken)
    {
        var articleGuid = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto { Name = N(baseName), ArticleType = ArticleTypeContainer },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create '{baseName}' container article.");

        var itemGuid = await _admin.V1.Items.PostAsync(
            new ItemCreateDto { ArticleGuid = articleGuid },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create '{baseName}' container item.");

        return itemGuid;
    }

    private async Task<List<Guid>> CreateItemsInAsync(
        Guid articleGuid, Guid containerGuid, int count, CancellationToken cancellationToken)
    {
        var created = new List<Guid>(count);

        for (var i = 0; i < count; i++)
        {
            var itemGuid = await _admin.V1.Items.PostAsync(
                new ItemCreateDto { ArticleGuid = articleGuid, ParentItemContainerGuid = containerGuid },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to create inventory item.");

            created.Add(itemGuid);
        }

        return created;
    }

    private string N(string baseName) => $"{baseName}-{_tag}";

    private static string Short(Guid guid) => guid.ToString("N")[..8];

    private sealed record Layout(
        Guid ArticleGuid,
        Guid Shelf,
        Guid Tote,
        Guid Pallet,
        IReadOnlyList<Guid> ShelfItems,
        IReadOnlyList<Guid> ToteItems,
        IReadOnlyList<Guid> PalletItems);
}
