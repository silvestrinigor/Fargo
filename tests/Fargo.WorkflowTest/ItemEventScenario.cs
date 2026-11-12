using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;

namespace Fargo.WorkflowTest;

/// <summary>
/// Exercises and verifies the event-based (event-tracked) workflow end to end,
/// now that the API can create event-tracked articles:
///
///  1. Create an event-tracked article ("bulk-grain") and two container items.
///  2. Record a Creation event, a sequence of Moviment events (receipt into a
///     container, transfer between containers, dispatch out), and a Deletion.
///  3. Verify the computed inventory matches the net of the MOVEMENT quantities:
///       - article total  = received-from-outside − dispatched-to-outside
///       - per container   = quantity moved in − quantity moved out
///       - a transfer between two containers nets zero at the article level
///       - Creation/Deletion events do not change the computed inventory
///       - point-in-time ('at') reflects only events up to that instant
///  4. List item events and verify the recorded set and the filters.
///
/// Movement plan (quantities), article total tracked on the right:
///   M1 receipt   null -> A   +100     A=100           total=100
///   M2 transfer  A -> B       40      A=60,  B=40      total=100 (unchanged)
///   M3 dispatch  B -> null    10      A=60,  B=30      total=90
/// Creation (+500 at A) and Deletion (−20 at A) are recorded but MUST NOT change
/// the computed inventory (the aggregation only reads movement events).
/// </summary>
internal sealed class ItemEventScenario(
    FargoApiClient admin,
    string runTag,
    Check check)
{
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeContainer = 5;
    private const int TrackingEvent = 2;

    private const int EventMoviment = 1;
    private const int EventCreation = 2;
    private const int EventDeletion = 3;

    private readonly FargoApiClient _admin = admin;
    private readonly string _tag = runTag;
    private readonly Check _check = check;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Event-tracked article.
        var article = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N("bulk-grain"),
                ArticleType = ArticleTypeDefault,
                TrackingMode = TrackingEvent,
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to create event-tracked article.");

        Log.Step($"Item events: created event-tracked article {Short(article)}");

        var containerA = await CreateContainerAsync("event-ContainerA", cancellationToken);
        var containerB = await CreateContainerAsync("event-ContainerB", cancellationToken);

        await RecordEventsAndVerifyAsync(article, containerA, containerB, cancellationToken);
        await VerifyEventListAsync(article, cancellationToken);
    }

    private async Task RecordEventsAndVerifyAsync(
        Guid article, Guid containerA, Guid containerB, CancellationToken cancellationToken)
    {
        Log.Step("Item events: recording creation + movement + deletion events");

        // A lifecycle Creation of 500 at A — must NOT affect computed inventory.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = article,
            EventType = EventCreation,
            Quantity = new Scalar { Double = 500 },
            ItemContainerGuid = containerA,
        }, cancellationToken);

        // Snapshot BEFORE any movement: computed inventory should be 0 here.
        var beforeMoves = DateTimeOffset.UtcNow;
        await Task.Delay(1200, cancellationToken);

        // M1: receipt of 100 into A (from outside). total=100, A=100.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = article,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 100 },
            ToItemContainerGuid = containerA,
        }, cancellationToken);

        var afterReceipt = DateTimeOffset.UtcNow;
        await Task.Delay(1200, cancellationToken);

        // M2: transfer 40 from A to B. total unchanged (100), A=60, B=40.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = article,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 40 },
            FromItemContainerGuid = containerA,
            ToItemContainerGuid = containerB,
        }, cancellationToken);

        // M3: dispatch 10 out of B (to outside). total=90, B=30.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = article,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 10 },
            FromItemContainerGuid = containerB,
        }, cancellationToken);

        // A lifecycle Deletion of 20 at A — must NOT affect computed inventory.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = article,
            EventType = EventDeletion,
            Quantity = new Scalar { Double = 20 },
            ItemContainerGuid = containerA,
        }, cancellationToken);

        // ----- Assertions on the computed (live) inventory -----

        // Article total across the whole system = received (100) − dispatched (10) = 90.
        var total = await _admin.V1.Articles[article.ToString()].Inventory
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(90d, total?.TotalCount, "event article total inventory = 90 (100 in − 10 out)");

        // Container A net = +100 (receipt) − 40 (transfer out) = 60.
        var invA = await _admin.V1.Articles[article.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [containerA];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(60d, invA?.TotalCount, "Container A net = 60 (+100 −40)");

        // Container B net = +40 (transfer in) − 10 (dispatch) = 30.
        var invB = await _admin.V1.Articles[article.ToString()].Inventory.GetAsync(cfg =>
        {
            cfg.QueryParameters.InsideItemContainerGuids = [containerB];
            cfg.QueryParameters.IncludeDescendents = false;
        }, cancellationToken);
        _check.Equal(30d, invB?.TotalCount, "Container B net = 30 (+40 −10)");

        // Container inventory endpoint reports the same per-article net for A and B.
        var containerAInv = await _admin.V1.Items[containerA].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        _check.Equal(60d, containerAInv.FirstOrDefault(r => r.ArticleGuid == article)?.TotalCount,
            "Container A inventory endpoint net = 60");

        var containerBInv = await _admin.V1.Items[containerB].Inventory
            .GetAsync(cancellationToken: cancellationToken) ?? [];
        _check.Equal(30d, containerBInv.FirstOrDefault(r => r.ArticleGuid == article)?.TotalCount,
            "Container B inventory endpoint net = 30");

        // ----- Point-in-time assertions -----

        // Before any movement (but after the Creation): computed inventory = 0,
        // proving Creation events do not feed the computed stock.
        var atBeforeMoves = await _admin.V1.Articles[article.ToString()].Inventory.GetAsync(cfg =>
            cfg.QueryParameters.At = beforeMoves, cancellationToken);
        _check.Equal(0d, atBeforeMoves?.TotalCount,
            "point-in-time before movements = 0 (Creation does not count)");

        // Just after the receipt (before transfer/dispatch): total = 100.
        var atAfterReceipt = await _admin.V1.Articles[article.ToString()].Inventory.GetAsync(cfg =>
            cfg.QueryParameters.At = afterReceipt, cancellationToken);
        _check.Equal(100d, atAfterReceipt?.TotalCount,
            "point-in-time after receipt = 100 (before dispatch)");
    }

    private async Task VerifyEventListAsync(Guid article, CancellationToken cancellationToken)
    {
        Log.Step("Item events: listing and filtering the recorded events");

        // All events for the article: 1 creation + 3 moviments + 1 deletion = 5.
        await _check.AllowedAsync("admin lists item events for the article", async () =>
        {
            var page = await _admin.V1.Item.Events.GetAsync(cfg =>
            {
                cfg.QueryParameters.ArticleGuid = article;
                cfg.QueryParameters.Page = 1;
                cfg.QueryParameters.Limit = 50;
            }, cancellationToken);

            _check.Equal(5, page?.Items?.Count ?? 0, "article has 5 item events total");
        });

        // Filter to movement events only: 3.
        await _check.AllowedAsync("admin lists movement events only", async () =>
        {
            var page = await _admin.V1.Item.Events.GetAsync(cfg =>
            {
                cfg.QueryParameters.ArticleGuid = article;
                cfg.QueryParameters.EventType = EventMoviment;
                cfg.QueryParameters.Page = 1;
                cfg.QueryParameters.Limit = 50;
            }, cancellationToken);

            _check.Equal(3, page?.Items?.Count ?? 0, "article has 3 movement events");
        });
    }

    private async Task PostEventAsync(ItemEventCreateDto dto, CancellationToken cancellationToken)
    {
        var guid = await _admin.V1.Item.Events.PostAsync(dto, cancellationToken: cancellationToken);

        if (guid is null || guid == Guid.Empty)
        {
            throw new InvalidOperationException("Item event creation returned no identifier.");
        }
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

    private string N(string baseName) => $"{baseName}-{_tag}";

    private static string Short(Guid guid) => guid.ToString("N")[..8];
}
