using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using Microsoft.Extensions.Options;

namespace Fargo.WorkflowTest;

/// <summary>
/// Drives an end-to-end exercise of the Fargo workflow against a running API.
///
/// The inventory scenario logs in, creates container and product articles,
/// creates many items, places them into containers, relocates them between
/// containers, removes some, and verifies the resulting state through the read
/// endpoints (location chain, movement history, container inventory).
///
/// The identity/authorization scenario (see <see cref="IdentityScenario"/>) then
/// creates partitions, user groups and users, grants and revokes permissions and
/// partition access, and logs in as the created users to prove that permitted
/// operations succeed and forbidden ones are rejected.
/// </summary>
public sealed class WorkflowRunner
{
    // ArticleType enum values as exposed by the API (see Fargo.Core.Articles.ArticleType).
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeContainer = 5;

    private readonly FargoApiClient _client;
    private readonly ITokenStore _tokenStore;
    private readonly WorkflowOptions _options;
    private readonly Random _random;
    private readonly string _runTag;

    public WorkflowRunner(FargoApiClient client, ITokenStore tokenStore, IOptions<WorkflowOptions> options)
    {
        _client = client;
        _tokenStore = tokenStore;
        _options = options.Value;
        _random = new Random(_options.Seed);
        _runTag = string.IsNullOrWhiteSpace(_options.RunTag)
            ? DateTime.UtcNow.ToString("yyMMddHHmmss")
            : _options.RunTag;
    }

    /// <summary>
    /// Runs the full workflow. Returns a process exit code: 0 on success,
    /// non-zero when a verification assertion fails or an unexpected error
    /// occurs.
    /// </summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        Log.Banner("Fargo workflow test");
        Log.Info($"Server           : {_options.Server}");
        Log.Info($"User             : {_options.Username}");
        Log.Info($"Containers       : {_options.Containers}");
        Log.Info($"Product articles : {_options.Articles}");
        Log.Info($"Items / article  : {_options.ItemsPerArticle}");
        Log.Info($"Random moves     : {_options.Moves} (seed {_options.Seed})");
        Log.Info($"Identity scenario: {(_options.RunIdentityScenarios ? "enabled" : "disabled")}");
        Log.Info($"Inventory scenario: {(_options.RunInventoryScenarios ? "enabled" : "disabled")}");
        Log.Info($"Article props scenario: {(_options.RunArticlePropertyScenarios ? "enabled" : "disabled")}");
        Log.Info($"Item events scenario: {(_options.RunItemEventScenarios ? "enabled" : "disabled")}");
        Log.Info($"Large-tree scenario: {(_options.RunLargeTreeScenario ? "enabled" : "disabled")}");
        Log.Info($"Run tag          : {_runTag}");

        var check = new Check();

        await LoginAsync(cancellationToken);

        // --- Inventory scenario ---
        var containers = await CreateContainersAsync(cancellationToken);

        var items = await CreateProductItemsAsync(cancellationToken);

        await PlaceItemsInitiallyAsync(items, containers, cancellationToken);

        await PerformRandomMovesAsync(items, containers, cancellationToken);

        await RemoveSomeItemsAsync(items, cancellationToken);

        var inventoryFailures = await VerifyAsync(items, containers, cancellationToken);

        // --- Identity / authorization scenario ---
        if (_options.RunIdentityScenarios)
        {
            var clientBuilder = new FargoClientBuilder(_options.Server);
            var identity = new IdentityScenario(_client, clientBuilder, _options, _runTag, check);
            await identity.RunAsync(cancellationToken);
        }

        // --- Inventory calculation scenario ---
        if (_options.RunInventoryScenarios)
        {
            var inventory = new InventoryScenario(_client, _options, _runTag, check);
            await inventory.RunAsync(cancellationToken);
        }

        // --- Article property coverage scenario ---
        if (_options.RunArticlePropertyScenarios)
        {
            var articleProps = new ArticlePropertiesScenario(_client, _runTag, check);
            await articleProps.RunAsync(cancellationToken);
        }

        // --- Event-tracked item-events scenario ---
        if (_options.RunItemEventScenarios)
        {
            var itemEvents = new ItemEventScenario(_client, _runTag, check);
            await itemEvents.RunAsync(cancellationToken);
        }

        // --- Large-tree stress / integration scenario ---
        if (_options.RunLargeTreeScenario)
        {
            var clientBuilder = new FargoClientBuilder(_options.Server);
            var largeTree = new LargeTreeScenario(_client, clientBuilder, _options, _runTag, check);
            await largeTree.RunAsync(cancellationToken);
        }

        var totalFailures = inventoryFailures + check.Failures;

        Log.Banner("SUMMARY");
        Log.Info($"Inventory verification failures : {inventoryFailures}");
        Log.Info($"Identity/authz checks passed    : {check.Passed}");
        Log.Info($"Identity/authz checks failed    : {check.Failures}");

        if (totalFailures == 0)
        {
            Log.Banner("WORKFLOW COMPLETED SUCCESSFULLY");
            return 0;
        }

        Log.Banner($"WORKFLOW COMPLETED WITH {totalFailures} FAILURE(S)");
        return 1;
    }

    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        Log.Step($"Authenticating as '{_options.Username}'");

        var auth = await _client.V1.Identity.Login.PostAsync(
            new IdentityLoginDto
            {
                Nameid = _options.Username,
                Password = _options.Password,
            },
            cancellationToken: cancellationToken);

        if (auth?.AccessToken is null || auth.RefreshToken is null || auth.ExpiresAt is null)
        {
            throw new InvalidOperationException(
                "Login failed: the server did not return a complete token set.");
        }

        // Persist the token so the authentication provider attaches it to every
        // subsequent request automatically.
        await _tokenStore.SetAsync(
            new AuthTokens(auth.AccessToken, auth.RefreshToken, auth.ExpiresAt.Value),
            cancellationToken);

        Log.Success($"Logged in. Token valid until {auth.ExpiresAt.Value:u}.");
    }

    private async Task<List<ContainerRef>> CreateContainersAsync(CancellationToken cancellationToken)
    {
        Log.Step($"Creating {_options.Containers} container articles + items");

        var containers = new List<ContainerRef>(_options.Containers);

        for (var i = 1; i <= _options.Containers; i++)
        {
            var name = $"Container-{i:D2}";

            var articleGuid = await _client.V1.Articles.PostAsync(
                new ArticleCreateDto
                {
                    Name = name,
                    Description = $"Storage container #{i} created by the workflow test.",
                    ArticleType = ArticleTypeContainer,
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create container article '{name}'.");

            var itemGuid = await _client.V1.Items.PostAsync(
                new ItemCreateDto { ArticleGuid = articleGuid },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create container item for '{name}'.");

            containers.Add(new ContainerRef(name, articleGuid, itemGuid));
            Log.Success($"{name}: article {Short(articleGuid)}, item {Short(itemGuid)}");
        }

        return containers;
    }

    private async Task<List<ItemRef>> CreateProductItemsAsync(CancellationToken cancellationToken)
    {
        Log.Step($"Creating {_options.Articles} product articles with {_options.ItemsPerArticle} items each");

        var items = new List<ItemRef>(_options.Articles * _options.ItemsPerArticle);

        for (var a = 1; a <= _options.Articles; a++)
        {
            var articleName = $"Product-{a:D2}";

            var articleGuid = await _client.V1.Articles.PostAsync(
                new ArticleCreateDto
                {
                    Name = articleName,
                    Description = $"Product article #{a} created by the workflow test.",
                    ArticleType = ArticleTypeDefault,
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create product article '{articleName}'.");

            for (var n = 1; n <= _options.ItemsPerArticle; n++)
            {
                var itemGuid = await _client.V1.Items.PostAsync(
                    new ItemCreateDto { ArticleGuid = articleGuid },
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"Failed to create item #{n} of article '{articleName}'.");

                items.Add(new ItemRef($"{articleName}#{n:D2}", articleGuid, itemGuid));
            }

            Log.Success($"{articleName}: article {Short(articleGuid)}, {_options.ItemsPerArticle} items");
        }

        Log.Info($"Total product items created: {items.Count}");
        return items;
    }

    private async Task PlaceItemsInitiallyAsync(
        List<ItemRef> items,
        List<ContainerRef> containers,
        CancellationToken cancellationToken)
    {
        Log.Step("Placing every item into a starting container (round-robin)");

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var container = containers[i % containers.Count];

            await MoveIntoAsync(item, container, cancellationToken);
        }

        Log.Success($"Placed {items.Count} items across {containers.Count} containers.");
    }

    private async Task PerformRandomMovesAsync(
        List<ItemRef> items,
        List<ContainerRef> containers,
        CancellationToken cancellationToken)
    {
        if (_options.Moves == 0 || items.Count == 0)
        {
            return;
        }

        Log.Step($"Relocating items between containers ({_options.Moves} random moves)");

        for (var m = 0; m < _options.Moves; m++)
        {
            var item = items[_random.Next(items.Count)];

            // Pick a destination container different from the item's current one.
            ContainerRef destination;
            do
            {
                destination = containers[_random.Next(containers.Count)];
            }
            while (containers.Count > 1 && destination.ItemGuid == item.CurrentContainerGuid);

            await MoveIntoAsync(item, destination, cancellationToken);
        }

        Log.Success($"Performed {_options.Moves} relocations.");
    }

    private async Task RemoveSomeItemsAsync(List<ItemRef> items, CancellationToken cancellationToken)
    {
        // Remove roughly a quarter of the items from all containers to exercise
        // the "remove from parent container" path as well.
        var toRemove = Math.Max(1, items.Count / 4);

        Log.Step($"Removing {toRemove} items from their containers");

        for (var i = 0; i < toRemove; i++)
        {
            var item = items[i];

            await _client.V1.Items[item.ItemGuid].PatchAsync(
                new ItemUpdateDto { RemoveFromParentItemContainer = true },
                cancellationToken: cancellationToken);

            item.CurrentContainerGuid = null;
            item.MoveCount++;
        }

        Log.Success($"Removed {toRemove} items from containers (now loose).");
    }

    private async Task MoveIntoAsync(ItemRef item, ContainerRef container, CancellationToken cancellationToken)
    {
        await _client.V1.Items[item.ItemGuid].PatchAsync(
            new ItemUpdateDto { ParentItemContainerGuid = container.ItemGuid },
            cancellationToken: cancellationToken);

        item.CurrentContainerGuid = container.ItemGuid;
        item.MoveCount++;
    }

    private async Task<int> VerifyAsync(
        List<ItemRef> items,
        List<ContainerRef> containers,
        CancellationToken cancellationToken)
    {
        Log.Step("Verifying final state against the API");

        var failures = 0;

        // 1. Every item's server-side parent container matches what we expect.
        foreach (var item in items)
        {
            var dto = await _client.V1.Items[item.ItemGuid].GetAsync(cancellationToken: cancellationToken);

            if (dto is null)
            {
                Log.Error($"{item.Name}: item not found on server.");
                failures++;
                continue;
            }

            if (dto.ParentContainerGuid != item.CurrentContainerGuid)
            {
                Log.Error(
                    $"{item.Name}: expected parent {Short(item.CurrentContainerGuid)}, " +
                    $"server reports {Short(dto.ParentContainerGuid)}.");
                failures++;
            }
        }

        // 2. The movement history length matches the number of moves we issued.
        var sampled = items.Take(Math.Min(items.Count, 5)).ToList();
        foreach (var item in sampled)
        {
            var moviments = await _client.V1.Items[item.ItemGuid].Moviments
                .GetAsync(cancellationToken: cancellationToken) ?? [];

            if (moviments.Count != item.MoveCount)
            {
                Log.Warn(
                    $"{item.Name}: expected {item.MoveCount} moviments, server has {moviments.Count}.");
            }
            else
            {
                Log.Success($"{item.Name}: {moviments.Count} moviment(s) recorded.");
            }
        }

        // 3. The location chain of a placed item resolves to a container root.
        var placed = items.FirstOrDefault(i => i.CurrentContainerGuid is not null);
        if (placed is not null)
        {
            var chain = await _client.V1.Items[placed.ItemGuid].Location
                .GetAsync(cancellationToken: cancellationToken) ?? [];

            Log.Info(
                $"Location chain for {placed.Name}: " +
                string.Join(" -> ", chain.Select(c => Short(c.Guid))));

            if (chain.Count < 2)
            {
                Log.Error($"{placed.Name}: expected a chain of at least 2 (item + container).");
                failures++;
            }
        }

        // 4. Report how many items each container holds (server-side truth).
        Log.Info("Container occupancy (server-side count of directly contained items):");
        foreach (var container in containers)
        {
            var count = items.Count(i => i.CurrentContainerGuid == container.ItemGuid);
            Log.Info($"  {container.Name}: {count} item(s)");
        }

        if (failures == 0)
        {
            Log.Success("All structural assertions passed.");
        }

        return failures;
    }

    private static string Short(Guid? guid) =>
        guid is null ? "(none)" : guid.Value.ToString("N")[..8];

    /// <summary>A created container article + its concrete container item.</summary>
    private sealed record ContainerRef(string Name, Guid ArticleGuid, Guid ItemGuid);

    /// <summary>A created product item plus the client-side view of its state.</summary>
    private sealed class ItemRef(string name, Guid articleGuid, Guid itemGuid)
    {
        public string Name { get; } = name;
        public Guid ArticleGuid { get; } = articleGuid;
        public Guid ItemGuid { get; } = itemGuid;

        /// <summary>Where we last asked the server to put this item (null = loose).</summary>
        public Guid? CurrentContainerGuid { get; set; }

        /// <summary>How many move/remove operations we issued for this item.</summary>
        public int MoveCount { get; set; }
    }
}
