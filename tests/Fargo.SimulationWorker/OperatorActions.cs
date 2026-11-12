using Fargo.ClientHttp.Models;

namespace Fargo.SimulationWorker;

/// <summary>
/// The catalogue of discrete operations a simulated operator can perform. Each
/// method models one realistic warehouse action, mutates the operator's
/// in-memory world, and returns a short action name for metrics. All calls are
/// made through the operator's own authenticated client; the caller wraps them
/// in the rate-limit policy.
///
/// Three flows are covered:
///  • Entity-tracked containment flow (Receiver / PutAway / Picker): container
///    and product articles, concrete items, placement, relocation, removal and
///    deletion.
///  • Event-tracked bulk flow (BulkHandler): an event-tracked article plus
///    receipt / transfer / dispatch movement events and lifecycle events.
///  • Read-only flow (Auditor): inventory, movements, item-event and audit-log
///    queries.
/// </summary>
internal sealed class OperatorActions(SimulationOptions options, Names names, Check check)
{
    private readonly SimulationOptions _options = options;
    private readonly Names _names = names;
    private readonly Check _check = check;

    // ----- Entity-tracked containment flow -------------------------------------

    /// <summary>Creates a container article + its concrete container item.</summary>
    public async Task<string> CreateContainerAsync(OperatorContext ctx, CancellationToken ct)
    {
        var articleGuid = await ctx.Client.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = _names.Display("sim-container"),
                Description = "Simulation storage container.",
                ArticleType = FargoCodes.ArticleTypeContainer,
                PartitionsToAdd = [ctx.Account.PartitionGuid],
            },
            cancellationToken: ct)
            ?? throw new InvalidOperationException("container article create returned null");

        var itemGuid = await ctx.Client.V1.Items.PostAsync(
            new ItemCreateDto
            {
                ArticleGuid = articleGuid,
                PartitionsToAdd = [ctx.Account.PartitionGuid],
            },
            cancellationToken: ct)
            ?? throw new InvalidOperationException("container item create returned null");

        ctx.Containers.Add(itemGuid);

        // Occasionally nest this container inside an existing one for depth.
        if (ctx.Containers.Count > 1 && ctx.Random.Next(100) < 30)
        {
            var parent = ctx.Containers[ctx.Random.Next(ctx.Containers.Count - 1)];
            if (parent != itemGuid)
            {
                await ctx.Client.V1.Items[itemGuid].PatchAsync(
                    new ItemUpdateDto { ParentItemContainerGuid = parent },
                    cancellationToken: ct);
            }
        }

        return "create-container";
    }

    /// <summary>Creates an entity-tracked product article (optionally with a barcode).</summary>
    public async Task<string> CreateProductArticleAsync(OperatorContext ctx, CancellationToken ct)
    {
        var dto = new ArticleCreateDto
        {
            Name = _names.Display("sim-product"),
            Description = "Simulation product article.",
            ArticleType = FargoCodes.ArticleTypeDefault,
            TrackingMode = FargoCodes.TrackingEntity,
            PartitionsToAdd = [ctx.Account.PartitionGuid],
        };

        // Half the time attach a unique EAN-13 to exercise the barcode path.
        if (ctx.Random.Next(100) < 50)
        {
            dto.Barcode = new ArticleBarcodeDto { Ean13 = _names.Ean13() };
        }

        var articleGuid = await ctx.Client.V1.Articles.PostAsync(dto, cancellationToken: ct)
            ?? throw new InvalidOperationException("product article create returned null");

        ctx.ProductArticles.Add(articleGuid);
        return "create-product-article";
    }

    /// <summary>Creates a concrete item of a known product article, placed into a container if one exists.</summary>
    public async Task<string> CreateItemAsync(OperatorContext ctx, CancellationToken ct)
    {
        var article = ctx.PickRandomGuid(ctx.ProductArticles);
        if (article is null)
        {
            // No article yet — bootstrap one first.
            return await CreateProductArticleAsync(ctx, ct);
        }

        var container = ctx.PickRandomGuid(ctx.Containers);
        var dto = new ItemCreateDto
        {
            ArticleGuid = article.Value,
            PartitionsToAdd = [ctx.Account.PartitionGuid],
        };
        if (container is not null)
        {
            dto.ParentItemContainerGuid = container.Value;
        }

        var itemGuid = await ctx.Client.V1.Items.PostAsync(dto, cancellationToken: ct)
            ?? throw new InvalidOperationException("item create returned null");

        ctx.Items.Add(new TrackedItem(itemGuid, article.Value) { ContainerGuid = container });
        return "create-item";
    }

    /// <summary>Relocates a known item into a different container.</summary>
    public async Task<string> MoveItemAsync(OperatorContext ctx, CancellationToken ct)
    {
        var item = ctx.Items.Where(i => !i.Deleted).ToList() is { Count: > 0 } live
            ? live[ctx.Random.Next(live.Count)]
            : null;
        if (item is null || ctx.Containers.Count == 0)
        {
            return await CreateItemAsync(ctx, ct);
        }

        // Pick a destination different from the current container when possible.
        Guid destination;
        do
        {
            destination = ctx.Containers[ctx.Random.Next(ctx.Containers.Count)];
        }
        while (ctx.Containers.Count > 1 && destination == item.ContainerGuid);

        await ctx.Client.V1.Items[item.Guid].PatchAsync(
            new ItemUpdateDto { ParentItemContainerGuid = destination },
            cancellationToken: ct);

        item.ContainerGuid = destination;

        if (_options.EnableConsistencyChecks && ctx.Random.Next(100) < 20)
        {
            var dto = await ctx.Client.V1.Items[item.Guid].GetAsync(cancellationToken: ct);
            _check.Equal(ctx.Who, destination, dto?.ParentContainerGuid ?? Guid.Empty,
                "moved item parent container matches");
        }

        return "move-item";
    }

    /// <summary>Removes a known item from its container (picking / staging for dispatch).</summary>
    public async Task<string> RemoveItemAsync(OperatorContext ctx, CancellationToken ct)
    {
        var item = ctx.Items.FirstOrDefault(i => !i.Deleted && i.ContainerGuid is not null);
        if (item is null)
        {
            return await MoveItemAsync(ctx, ct);
        }

        await ctx.Client.V1.Items[item.Guid].PatchAsync(
            new ItemUpdateDto { RemoveFromParentItemContainer = true },
            cancellationToken: ct);

        item.ContainerGuid = null;
        return "remove-item";
    }

    /// <summary>Ships (deletes) a known item that has already been removed from its container.</summary>
    public async Task<string> ShipItemAsync(OperatorContext ctx, CancellationToken ct)
    {
        var item = ctx.Items.FirstOrDefault(i => !i.Deleted && i.ContainerGuid is null)
            ?? ctx.Items.FirstOrDefault(i => !i.Deleted);
        if (item is null)
        {
            return await CreateItemAsync(ctx, ct);
        }

        await ctx.Client.V1.Items[item.Guid].DeleteAsync(cancellationToken: ct);
        item.Deleted = true;
        return "ship-item";
    }

    // ----- Event-tracked bulk flow ---------------------------------------------

    /// <summary>Creates an event-tracked bulk article plus two bulk containers.</summary>
    public async Task<string> CreateBulkArticleAsync(OperatorContext ctx, CancellationToken ct)
    {
        var articleGuid = await ctx.Client.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = _names.Display("sim-bulk"),
                Description = "Simulation event-tracked bulk article.",
                ArticleType = FargoCodes.ArticleTypeDefault,
                TrackingMode = FargoCodes.TrackingEvent,
                PartitionsToAdd = [ctx.Account.PartitionGuid],
            },
            cancellationToken: ct)
            ?? throw new InvalidOperationException("bulk article create returned null");

        ctx.EventArticles.Add(articleGuid);

        // Ensure at least two containers exist for transfers.
        while (ctx.Containers.Count < 2)
        {
            await CreateContainerAsync(ctx, ct);
        }

        return "create-bulk-article";
    }

    /// <summary>Records a receipt of bulk stock from outside into a container.</summary>
    public async Task<string> BulkReceiptAsync(OperatorContext ctx, CancellationToken ct)
    {
        var article = ctx.PickRandomGuid(ctx.EventArticles);
        var container = ctx.PickRandomGuid(ctx.Containers);
        if (article is null || container is null)
        {
            return await CreateBulkArticleAsync(ctx, ct);
        }

        await ctx.Client.V1.Item.Events.PostAsync(
            new ItemEventCreateDto
            {
                ArticleGuid = article.Value,
                EventType = FargoCodes.EventMoviment,
                Quantity = new Scalar { Double = ctx.Random.Next(10, 200) },
                ToItemContainerGuid = container.Value,
            },
            cancellationToken: ct);

        return "bulk-receipt";
    }

    /// <summary>Transfers a bulk quantity between two containers (nets zero overall).</summary>
    public async Task<string> BulkTransferAsync(OperatorContext ctx, CancellationToken ct)
    {
        var article = ctx.PickRandomGuid(ctx.EventArticles);
        if (article is null || ctx.Containers.Count < 2)
        {
            return await BulkReceiptAsync(ctx, ct);
        }

        var from = ctx.Containers[ctx.Random.Next(ctx.Containers.Count)];
        Guid to;
        do
        {
            to = ctx.Containers[ctx.Random.Next(ctx.Containers.Count)];
        }
        while (to == from);

        await ctx.Client.V1.Item.Events.PostAsync(
            new ItemEventCreateDto
            {
                ArticleGuid = article.Value,
                EventType = FargoCodes.EventMoviment,
                Quantity = new Scalar { Double = ctx.Random.Next(1, 30) },
                FromItemContainerGuid = from,
                ToItemContainerGuid = to,
            },
            cancellationToken: ct);

        return "bulk-transfer";
    }

    /// <summary>Dispatches a bulk quantity out of a container to outside stock.</summary>
    public async Task<string> BulkDispatchAsync(OperatorContext ctx, CancellationToken ct)
    {
        var article = ctx.PickRandomGuid(ctx.EventArticles);
        var container = ctx.PickRandomGuid(ctx.Containers);
        if (article is null || container is null)
        {
            return await BulkReceiptAsync(ctx, ct);
        }

        await ctx.Client.V1.Item.Events.PostAsync(
            new ItemEventCreateDto
            {
                ArticleGuid = article.Value,
                EventType = FargoCodes.EventMoviment,
                Quantity = new Scalar { Double = ctx.Random.Next(1, 10) },
                FromItemContainerGuid = container.Value,
            },
            cancellationToken: ct);

        // Occasionally verify the computed inventory stays non-negative.
        if (_options.EnableConsistencyChecks && ctx.Random.Next(100) < 15)
        {
            var inv = await ctx.Client.V1.Articles[article.Value.ToString()].Inventory
                .GetAsync(cancellationToken: ct);
            _check.That(ctx.Who, (inv?.TotalCount ?? 0) >= 0, "bulk article inventory is non-negative");
        }

        return "bulk-dispatch";
    }

    // ----- Read-only auditor flow ----------------------------------------------

    /// <summary>Reads back inventory, movements, item events or audit logs.</summary>
    public async Task<string> AuditAsync(OperatorContext ctx, CancellationToken ct)
    {
        switch (ctx.Random.Next(4))
        {
            case 0:
                await ctx.Client.V1.Articles.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Page = 1;
                    cfg.QueryParameters.Limit = 20;
                }, ct);
                return "audit-list-articles";

            case 1:
                await ctx.Client.V1.Items.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Page = 1;
                    cfg.QueryParameters.Limit = 20;
                }, ct);
                return "audit-list-items";

            case 2:
                await ctx.Client.V1.Item.Events.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Page = 1;
                    cfg.QueryParameters.Limit = 20;
                }, ct);
                return "audit-list-item-events";

            default:
                await ctx.Client.V1.AuditLogs.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Page = 1;
                    cfg.QueryParameters.Limit = 20;
                    cfg.QueryParameters.PeriodStart = DateTimeOffset.UtcNow.AddHours(-1);
                }, ct);
                return "audit-list-audit-logs";
        }
    }
}
