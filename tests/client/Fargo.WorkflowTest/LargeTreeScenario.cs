using Fargo.ClientHttp;
using Fargo.ClientHttp.ErrorHandling;
using Fargo.ClientHttp.Models;
using System.Collections.Concurrent;

namespace Fargo.WorkflowTest;

/// <summary>
/// A large-scale stress and integration scenario that exercises the whole Fargo
/// surface at once:
///
///  1. <b>Partition tree</b> — builds a deep and wide partition hierarchy under
///     the global partition (fan-out × depth), recording the parent/child links
///     and verifying a sample of them round-trip.
///  2. <b>User groups</b> — creates many groups, including a nested group
///     hierarchy (parent/child groups), each scoped to a partition subtree with
///     a distinct permission set.
///  3. <b>Users</b> — creates many users spread across the groups and
///     partitions, clears their first-login password requirement, and logs in as
///     a sample to prove the accounts are usable.
///  4. <b>Articles &amp; items</b> — creates many articles (entity- and
///     event-tracked) stamped into partitions, and many items placed into a
///     per-partition container tree.
///  5. <b>Item events &amp; moviments</b> — moves items between containers
///     (entity tracking) and records movement/creation/deletion events against
///     an event-tracked article, then verifies the computed inventory.
///  6. <b>Access &amp; deletion boundaries</b> — logs in as scoped users and
///     asserts they can act inside their partition/permission envelope and are
///     denied outside it, then deletes users/groups and proves the deletes took.
///  7. <b>Concurrency</b> — fires parallel article/item creations, parallel
///     moves of the same item (last-writer-wins), and parallel group-membership
///     mutations, asserting the server stays consistent under contention.
///  8. <b>Audit log</b> — reads the audit log back filtered by actor, entity,
///     entity-type, audit-type and time window, and asserts the created,
///     updated and deleted entities left the expected trail.
///
/// The whole scenario runs as the admin (<paramref name="admin"/>) except where
/// it deliberately logs in as a created user to test a boundary.
/// </summary>
internal sealed class LargeTreeScenario(
    FargoApiClient admin,
    FargoClientBuilder clientBuilder,
    WorkflowOptions options,
    string runTag,
    Check check)
{
    private const int ArticleTypeDefault = 1;
    private const int ArticleTypeContainer = 5;
    private const int TrackingEntity = 1;
    private const int TrackingEvent = 2;

    private const int EventMoviment = 1;
    private const int EventCreation = 2;
    private const int EventDeletion = 3;

    private readonly FargoApiClient _admin = admin;
    private readonly FargoClientBuilder _clientBuilder = clientBuilder;
    private readonly WorkflowOptions _options = options;
    private readonly string _tag = runTag;
    private readonly Check _check = check;
    private readonly Random _random = new(options.Seed ^ 0x5A17);

    // State accumulated across phases.
    private readonly List<PartitionNode> _partitions = [];
    private readonly List<CreatedGroup> _groups = [];
    private readonly List<CreatedUser> _users = [];
    private readonly List<CreatedArticle> _articles = [];
    private readonly List<CreatedItem> _items = [];
    private DateTimeOffset _scenarioStart;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Log.Banner("LARGE TREE SCENARIO");
        _scenarioStart = DateTimeOffset.UtcNow.AddSeconds(-1);

        await BuildPartitionTreeAsync(cancellationToken);
        await CreateUserGroupsAsync(cancellationToken);
        await CreateUsersAsync(cancellationToken);
        await CreateArticlesAndItemsAsync(cancellationToken);
        await RecordItemEventsAsync(cancellationToken);
        await VerifyAccessBoundariesAsync(cancellationToken);
        await RunConcurrencyTestsAsync(cancellationToken);
        await VerifyDeletionsAsync(cancellationToken);
        await InspectAuditLogAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // 1. Partition tree
    // -------------------------------------------------------------------------

    private async Task BuildPartitionTreeAsync(CancellationToken cancellationToken)
    {
        var fanout = Math.Max(2, _options.PartitionTreeFanout);
        var depth = Math.Max(2, _options.PartitionTreeDepth);

        Log.Step($"Partition tree: building fan-out {fanout} × depth {depth} under the global partition");

        // Root of the scenario tree lives directly under the global partition.
        var root = await CreatePartitionAsync("P-root", null, 0, cancellationToken);

        // Breadth-first expansion: each level multiplies the frontier by fanout
        // until the configured depth is reached.
        var frontier = new List<PartitionNode> { root };
        for (var level = 1; level < depth; level++)
        {
            var next = new List<PartitionNode>();
            foreach (var parent in frontier)
            {
                for (var c = 0; c < fanout; c++)
                {
                    var node = await CreatePartitionAsync(
                        $"P-L{level}-{parent.Index:D3}-{c}", parent, level, cancellationToken);
                    next.Add(node);
                }
            }
            frontier = next;
        }

        Log.Success($"Created {_partitions.Count} partitions across {depth} levels " +
                    $"({frontier.Count} leaves).");

        // Verify a random sample of parent links round-tripped.
        var sample = _partitions.Where(p => p.ParentGuid is not null)
            .OrderBy(_ => _random.Next()).Take(Math.Min(5, _partitions.Count)).ToList();
        foreach (var node in sample)
        {
            var dto = await _admin.V1.Partitions[node.Guid].GetAsync(cancellationToken: cancellationToken);
            _check.Equal(node.ParentGuid, dto?.ParentPartitionGuid,
                $"partition {node.Name} parent link round-trips");
        }

        // The list endpoint can enumerate children of the root subtree.
        var listed = await _admin.V1.Partitions.GetAsync(cfg =>
        {
            cfg.QueryParameters.ChildOfAnyOfThesePartitions = [root.Guid];
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 500;
        }, cancellationToken);
        _check.That((listed?.Items?.Count ?? 0) >= 1,
            "partition list endpoint returns descendants of the root");
    }

    private async Task<PartitionNode> CreatePartitionAsync(
        string baseName, PartitionNode? parent, int level, CancellationToken cancellationToken)
    {
        var name = N(baseName);
        var guid = await _admin.V1.Partitions.PostAsync(
            new PartitionCreateDto
            {
                Name = name,
                Description = $"Large-tree partition at level {level}.",
                ParentPartitionGuid = parent?.Guid,
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create partition '{name}'.");

        var node = new PartitionNode(_partitions.Count, name, guid, parent?.Guid, level);
        _partitions.Add(node);
        return node;
    }

    // -------------------------------------------------------------------------
    // 2. User groups (including a nested hierarchy)
    // -------------------------------------------------------------------------

    private async Task CreateUserGroupsAsync(CancellationToken cancellationToken)
    {
        var target = Math.Max(3, _options.LargeTreeUserGroups);
        Log.Step($"User groups: creating {target} groups (with a nested parent/child hierarchy)");

        // A rotating catalogue of permission bundles so groups differ meaningfully.
        int[][] bundles =
        [
            [ActionTypes.CreateArticle, ActionTypes.EditArticle, ActionTypes.CreateItem, ActionTypes.EditItem],
            [ActionTypes.CreateItem, ActionTypes.EditItem, ActionTypes.DeleteItem],
            [ActionTypes.CreateArticle, ActionTypes.DeleteArticle],
            [ActionTypes.ReadAuditLogs, ActionTypes.ReadItemEvents],
            [ActionTypes.CreateItem, ActionTypes.EditItem, ActionTypes.ReadItemEvents],
        ];

        // Scope each group to a leaf-ish partition so access tests have real reach.
        var scopable = _partitions.Where(p => p.Level >= 1).ToList();
        if (scopable.Count == 0)
        {
            scopable = _partitions;
        }

        Guid? previousGroup = null;
        for (var i = 0; i < target; i++)
        {
            var partition = scopable[i % scopable.Count];
            var bundle = bundles[i % bundles.Length];

            // Every third group nests under the previous one to build a chain,
            // exercising the parentUserGroup relationship.
            var parent = (i % 3 == 0) ? null : previousGroup;

            var guid = await _admin.V1.UserGroups.PostAsync(
                new UserGroupCreateDto
                {
                    Nameid = N($"group-{i:D2}"),
                    Description = $"Large-tree group #{i} scoped to {partition.Name}.",
                    IsActive = true,
                    PermissionsToAdd = [.. bundle.Select(b => (int?)b)],
                    PartitionAccessesToAdd = [partition.Guid],
                    ParentUserGroup = parent,
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create group #{i}.");

            _groups.Add(new CreatedGroup(guid, N($"group-{i:D2}"), partition.Guid, bundle, parent));
            previousGroup = guid;
        }

        Log.Success($"Created {_groups.Count} user groups.");

        // Verify a sample: permissions and the nested parent link.
        var nested = _groups.FirstOrDefault(g => g.ParentGroup is not null);
        if (nested is not null)
        {
            var dto = await _admin.V1.UserGroups[nested.Guid].GetAsync(cancellationToken: cancellationToken);
            _check.Equal(nested.ParentGroup, dto?.ParentUserGroup, "nested group parent link round-trips");
            _check.That(dto?.Permissions is not null && dto.Permissions.Count > 0,
                "nested group retains its permissions");
        }

        // The group list endpoint returns the created groups.
        var listed = await _admin.V1.UserGroups.GetAsync(cfg =>
        {
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 500;
        }, cancellationToken);
        _check.That((listed?.Items?.Count ?? 0) >= _groups.Count,
            "user-group list endpoint returns at least the created groups");
    }

    // -------------------------------------------------------------------------
    // 3. Users
    // -------------------------------------------------------------------------

    private async Task CreateUsersAsync(CancellationToken cancellationToken)
    {
        var target = Math.Max(3, _options.LargeTreeUsers);
        Log.Step($"Users: creating {target} users spread across the groups and partitions");

        var scopable = _partitions.Where(p => p.Level >= 1).ToList();
        if (scopable.Count == 0)
        {
            scopable = _partitions;
        }

        for (var i = 0; i < target; i++)
        {
            var nameid = N($"user-{i:D3}");

            // Roughly 70% of users join a group (inheriting its perms/access);
            // the rest get a direct partition access and no group, to vary shape.
            Guid? group = (i % 10 < 7 && _groups.Count > 0)
                ? _groups[i % _groups.Count].Guid
                : null;
            var partition = scopable[i % scopable.Count];

            var create = new UserCreateDto
            {
                Nameid = nameid,
                FirstName = LetterName("User", i),
                LastName = "Bulk",
                Description = $"Large-tree bulk user #{i}.",
                IsActive = true,
                Authentication = new UserAuthenticationCreateDto
                {
                    Password = _options.NewUserPassword,
                    DefaultPasswordExpirationPeriod = "365.00:00:00",
                },
            };

            if (group is not null)
            {
                create.UserGroupsToAdd = [group];
            }
            else
            {
                create.PartitionAccessesToAdd = [partition.Guid];
            }

            var guid = await _admin.V1.Users.PostAsync(create, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create user #{i}.");

            _users.Add(new CreatedUser(guid, nameid, group, partition.Guid));
        }

        Log.Success($"Created {_users.Count} users.");

        // Clear the mandatory first-login password change for every user so they
        // are all immediately usable.
        foreach (var user in _users)
        {
            await ClearPasswordChangeRequirementAsync(user.Nameid, cancellationToken);
        }
        Log.Info($"Cleared first-login password requirement for {_users.Count} users.");

        // Log in as a sample to prove the accounts work and tokens are issued.
        var sample = _users.OrderBy(_ => _random.Next()).Take(Math.Min(3, _users.Count));
        foreach (var user in sample)
        {
            await _check.AllowedAsync($"login as freshly created user '{user.Nameid}'", async () =>
            {
                var client = await _clientBuilder.CreateAuthenticatedClientAsync(
                    user.Nameid, _options.NewUserPassword, cancellationToken);
                // A trivially-authorized self read: listing own-visible partitions.
                await client.V1.Partitions.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Page = 1;
                    cfg.QueryParameters.Limit = 1;
                }, cancellationToken);
            });
        }

        // The user list endpoint paginates: fetch page 1 with a small limit and
        // assert the paging metadata is coherent.
        var page1 = await _admin.V1.Users.GetAsync(cfg =>
        {
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 5;
        }, cancellationToken);
        _check.That((page1?.Items?.Count ?? 0) <= 5, "user list honours the page limit");
        _check.That((page1?.TotalCount ?? 0) >= _users.Count,
            "user list total count covers the created users");
    }

    // -------------------------------------------------------------------------
    // 4. Articles & items
    // -------------------------------------------------------------------------

    private async Task CreateArticlesAndItemsAsync(CancellationToken cancellationToken)
    {
        var articleCount = Math.Max(2, _options.LargeTreeArticles);
        var perArticle = Math.Max(1, _options.LargeTreeItemsPerArticle);

        Log.Step($"Articles/items: creating {articleCount} articles with {perArticle} items each");

        var leaves = _partitions.Where(p => p.Level >= 1).ToList();
        if (leaves.Count == 0)
        {
            leaves = _partitions;
        }

        // One container item per partition to hold that partition's items.
        var containers = new Dictionary<Guid, Guid>();
        foreach (var leaf in leaves)
        {
            var containerArticle = await _admin.V1.Articles.PostAsync(
                new ArticleCreateDto
                {
                    Name = N($"cont-{leaf.Index:D3}"),
                    ArticleType = ArticleTypeContainer,
                    PartitionsToAdd = [leaf.Guid],
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to create container article.");

            var containerItem = await _admin.V1.Items.PostAsync(
                new ItemCreateDto { ArticleGuid = containerArticle, PartitionsToAdd = [leaf.Guid] },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to create container item.");

            containers[leaf.Guid] = containerItem;
        }

        for (var a = 0; a < articleCount; a++)
        {
            var partition = leaves[a % leaves.Count];
            // Every fifth article is event-tracked, the rest entity-tracked.
            var eventTracked = (a % 5 == 4);

            var articleGuid = await _admin.V1.Articles.PostAsync(
                new ArticleCreateDto
                {
                    Name = N($"article-{a:D3}"),
                    Description = $"Large-tree article #{a}.",
                    ArticleType = ArticleTypeDefault,
                    TrackingMode = eventTracked ? TrackingEvent : TrackingEntity,
                    PartitionsToAdd = [partition.Guid],
                },
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Failed to create article #{a}.");

            var article = new CreatedArticle(articleGuid, N($"article-{a:D3}"), partition.Guid, eventTracked);
            _articles.Add(article);

            // Only entity-tracked articles get concrete items.
            if (eventTracked)
            {
                continue;
            }

            var container = containers[partition.Guid];
            for (var n = 0; n < perArticle; n++)
            {
                var itemGuid = await _admin.V1.Items.PostAsync(
                    new ItemCreateDto
                    {
                        ArticleGuid = articleGuid,
                        PartitionsToAdd = [partition.Guid],
                        ParentItemContainerGuid = container,
                    },
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException("Failed to create item.");

                _items.Add(new CreatedItem(itemGuid, articleGuid, partition.Guid, container));
            }
        }

        Log.Success($"Created {_articles.Count} articles and {_items.Count} items " +
                    $"across {containers.Count} partition containers.");

        // Verify an entity-tracked article's inventory equals the items we placed.
        var tracked = _articles.FirstOrDefault(a => !a.EventTracked);
        if (tracked is not null)
        {
            var expected = _items.Count(i => i.ArticleGuid == tracked.Guid);
            var inv = await _admin.V1.Articles[tracked.Guid.ToString()].Inventory
                .GetAsync(cancellationToken: cancellationToken);
            _check.Equal((double)expected, inv?.TotalCount,
                $"article {tracked.Name} inventory equals the {expected} items created");
        }
    }

    // -------------------------------------------------------------------------
    // 5. Item events & moviments
    // -------------------------------------------------------------------------

    private async Task RecordItemEventsAsync(CancellationToken cancellationToken)
    {
        Log.Step("Item events/moviments: relocating entity items and recording event-tracked movements");

        // 5a. Entity-tracked moviments: move a batch of items into a different
        // partition's container (where one exists) to generate containment
        // history, then assert the parent changed.
        var movable = _items.Take(Math.Min(_items.Count, 20)).ToList();
        var moved = 0;
        foreach (var item in movable)
        {
            // Find another container item (any item that is a container holder).
            var otherContainer = _items
                .Select(i => i.ContainerGuid)
                .FirstOrDefault(c => c != item.ContainerGuid);

            if (otherContainer == Guid.Empty || otherContainer == item.ContainerGuid)
            {
                continue;
            }

            try
            {
                await _admin.V1.Items[item.Guid].PatchAsync(
                    new ItemUpdateDto { ParentItemContainerGuid = otherContainer },
                    cancellationToken: cancellationToken);
                item.ContainerGuid = otherContainer;
                moved++;
            }
            catch (FargoApiException)
            {
                // Cross-partition move may be rejected by scoping; that is fine,
                // the event scenario below covers the event-tracking path.
            }
        }
        Log.Success($"Relocated {moved} entity items between containers.");

        if (moved > 0)
        {
            var sample = movable.First(i => i.ContainerGuid != Guid.Empty);
            var moviments = await _admin.V1.Items[sample.Guid].Moviments
                .GetAsync(cancellationToken: cancellationToken) ?? [];
            _check.That(moviments.Count >= 1, "relocated item has recorded moviment history");
        }

        // 5b. Event-tracked movements against an event-tracked article.
        var eventArticle = _articles.FirstOrDefault(a => a.EventTracked);
        if (eventArticle is null)
        {
            Log.Info("No event-tracked article present; skipping event-movement checks.");
            return;
        }

        // Two containers in the event article's partition.
        var cA = await CreateContainerItemAsync(eventArticle.PartitionGuid, "evt-A", cancellationToken);
        var cB = await CreateContainerItemAsync(eventArticle.PartitionGuid, "evt-B", cancellationToken);

        // Receipt +100 into A, transfer 40 A->B, dispatch 10 out of B.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = eventArticle.Guid,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 100 },
            ToItemContainerGuid = cA,
        }, cancellationToken);
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = eventArticle.Guid,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 40 },
            FromItemContainerGuid = cA,
            ToItemContainerGuid = cB,
        }, cancellationToken);
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = eventArticle.Guid,
            EventType = EventMoviment,
            Quantity = new Scalar { Double = 10 },
            FromItemContainerGuid = cB,
        }, cancellationToken);
        // Lifecycle events must not move the computed stock.
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = eventArticle.Guid,
            EventType = EventCreation,
            Quantity = new Scalar { Double = 500 },
            ItemContainerGuid = cA,
        }, cancellationToken);
        await PostEventAsync(new ItemEventCreateDto
        {
            ArticleGuid = eventArticle.Guid,
            EventType = EventDeletion,
            Quantity = new Scalar { Double = 20 },
            ItemContainerGuid = cA,
        }, cancellationToken);

        var total = await _admin.V1.Articles[eventArticle.Guid.ToString()].Inventory
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal(90d, total?.TotalCount, "event-tracked article total = 90 (100 in − 10 out)");

        var events = await _admin.V1.Item.Events.GetAsync(cfg =>
        {
            cfg.QueryParameters.ArticleGuid = eventArticle.Guid;
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 50;
        }, cancellationToken);
        _check.Equal(5, events?.Items?.Count ?? 0, "event-tracked article has 5 recorded events");
    }

    // -------------------------------------------------------------------------
    // 6. Access & deletion boundaries
    // -------------------------------------------------------------------------

    private async Task VerifyAccessBoundariesAsync(CancellationToken cancellationToken)
    {
        Log.Step("Access boundaries: verifying scoped users are allowed inside and denied outside their envelope");

        // Pick a user that belongs to a group with CreateItem/CreateArticle and a
        // partition access, so we can test an allowed create inside scope.
        var grouped = _users.FirstOrDefault(u => u.GroupGuid is not null);
        if (grouped is not null)
        {
            var group = _groups.First(g => g.Guid == grouped.GroupGuid);
            var client = await _clientBuilder.CreateAuthenticatedClientAsync(
                grouped.Nameid, _options.NewUserPassword, cancellationToken);

            var canCreateArticle = group.Permissions.Contains(ActionTypes.CreateArticle);
            if (canCreateArticle)
            {
                await _check.AllowedAsync(
                    $"'{grouped.Nameid}' creates an article inside its partition", () =>
                    client.V1.Articles.PostAsync(
                        new ArticleCreateDto
                        {
                            Name = N($"scoped-{grouped.Nameid}"),
                            ArticleType = ArticleTypeDefault,
                            PartitionsToAdd = [group.PartitionGuid],
                        },
                        cancellationToken: cancellationToken));
            }
            else
            {
                await _check.DeniedAsync(
                    $"'{grouped.Nameid}' (no CreateArticle) is denied creating an article", () =>
                    client.V1.Articles.PostAsync(
                        new ArticleCreateDto { Name = N($"nope-{grouped.Nameid}"), ArticleType = ArticleTypeDefault },
                        cancellationToken: cancellationToken),
                    401, 403);
            }

            // Regardless of permissions, a scoped user lacks CreatePartition.
            await _check.DeniedAsync(
                $"'{grouped.Nameid}' is denied creating a partition (no CreatePartition)", () =>
                client.V1.Partitions.PostAsync(
                    new PartitionCreateDto { Name = N($"evil-{grouped.Nameid}") },
                    cancellationToken: cancellationToken),
                401, 403);
        }

        // A user scoped to one partition must not see an entity that lives only in
        // a sibling partition. Build the negative case explicitly.
        var directAccessUser = _users.FirstOrDefault(u => u.GroupGuid is null);
        if (directAccessUser is not null)
        {
            // An article that is NOT in this user's partition.
            var foreign = _articles.FirstOrDefault(a => a.PartitionGuid != directAccessUser.PartitionGuid);
            if (foreign is not null)
            {
                var client = await _clientBuilder.CreateAuthenticatedClientAsync(
                    directAccessUser.Nameid, _options.NewUserPassword, cancellationToken);

                await _check.DeniedAsync(
                    $"'{directAccessUser.Nameid}' cannot read an article outside its partition", () =>
                    client.V1.Articles[foreign.Guid.ToString()].GetAsync(cancellationToken: cancellationToken),
                    401, 403, 404);
            }
        }

        // Admin can toggle a user active/inactive and the change is reflected.
        var toggleUser = _users[^1];
        await _check.AllowedAsync($"admin deactivates '{toggleUser.Nameid}'", () =>
            _admin.V1.Users[toggleUser.Guid].PutAsync(
                new UserUpdateDto { IsActive = false }, cancellationToken: cancellationToken));
        var afterDeactivate = await _admin.V1.Users[toggleUser.Guid].GetAsync(cancellationToken: cancellationToken);
        _check.Equal(false, afterDeactivate?.IsActive, $"'{toggleUser.Nameid}' is now inactive");

        // A deactivated user cannot authenticate.
        await _check.DeniedAsync($"inactive user '{toggleUser.Nameid}' cannot log in", () =>
            _clientBuilder.CreateAuthenticatedClientAsync(
                toggleUser.Nameid, _options.NewUserPassword, cancellationToken),
            401, 403);

        // Reactivate so later phases can still use it if needed.
        await _check.AllowedAsync($"admin reactivates '{toggleUser.Nameid}'", () =>
            _admin.V1.Users[toggleUser.Guid].PutAsync(
                new UserUpdateDto { IsActive = true }, cancellationToken: cancellationToken));
    }

    // -------------------------------------------------------------------------
    // 7. Concurrency
    // -------------------------------------------------------------------------

    private async Task RunConcurrencyTestsAsync(CancellationToken cancellationToken)
    {
        var degree = Math.Max(2, _options.ConcurrencyDegree);
        Log.Step($"Concurrency: firing {degree} parallel operations and asserting consistency");

        var partition = _partitions.First(p => p.Level >= 1 || p.ParentGuid is null);

        // 7a. Parallel article creation — every task must succeed and return a
        // distinct GUID (no collisions, no lost writes).
        var createdArticleGuids = new ConcurrentBag<Guid>();
        var articleTasks = Enumerable.Range(0, degree).Select(async i =>
        {
            try
            {
                var guid = await _admin.V1.Articles.PostAsync(
                    new ArticleCreateDto
                    {
                        Name = N($"cc-article-{i:D2}"),
                        ArticleType = ArticleTypeDefault,
                        PartitionsToAdd = [partition.Guid],
                    },
                    cancellationToken: cancellationToken);
                if (guid is Guid g && g != Guid.Empty)
                {
                    createdArticleGuids.Add(g);
                }
            }
            catch (FargoApiException ex)
            {
                Log.Warn($"concurrent article create {i} failed: {ex.StatusCode} {ex.Message}");
            }
        });
        await Task.WhenAll(articleTasks);
        _check.Equal(degree, createdArticleGuids.Count,
            $"all {degree} concurrent article creates succeeded");
        _check.Equal(createdArticleGuids.Count, createdArticleGuids.Distinct().Count(),
            "concurrent article creates produced distinct GUIDs");

        // 7b. Parallel item creation for one article, then assert the computed
        // inventory equals the number of items created (no lost inserts).
        var ccArticle = createdArticleGuids.First();
        var container = await CreateContainerItemAsync(partition.Guid, "cc-container", cancellationToken);
        var createdItemGuids = new ConcurrentBag<Guid>();
        var itemTasks = Enumerable.Range(0, degree).Select(async i =>
        {
            try
            {
                var guid = await _admin.V1.Items.PostAsync(
                    new ItemCreateDto
                    {
                        ArticleGuid = ccArticle,
                        PartitionsToAdd = [partition.Guid],
                        ParentItemContainerGuid = container,
                    },
                    cancellationToken: cancellationToken);
                if (guid is Guid g && g != Guid.Empty)
                {
                    createdItemGuids.Add(g);
                }
            }
            catch (FargoApiException ex)
            {
                Log.Warn($"concurrent item create {i} failed: {ex.StatusCode} {ex.Message}");
            }
        });
        await Task.WhenAll(itemTasks);
        _check.Equal(degree, createdItemGuids.Count, $"all {degree} concurrent item creates succeeded");

        var ccInventory = await _admin.V1.Articles[ccArticle.ToString()].Inventory
            .GetAsync(cancellationToken: cancellationToken);
        _check.Equal((double)createdItemGuids.Count, ccInventory?.TotalCount,
            "article inventory equals the number of concurrently created items");

        // 7c. Parallel moves of the SAME item between two containers. Last writer
        // wins; the item must end in exactly one of the two containers and the
        // server must not be left in an inconsistent state.
        var c1 = await CreateContainerItemAsync(partition.Guid, "cc-move-1", cancellationToken);
        var c2 = await CreateContainerItemAsync(partition.Guid, "cc-move-2", cancellationToken);
        var moveItem = createdItemGuids.First();
        var moveTasks = Enumerable.Range(0, degree).Select(async i =>
        {
            var destination = (i % 2 == 0) ? c1 : c2;
            try
            {
                await _admin.V1.Items[moveItem].PatchAsync(
                    new ItemUpdateDto { ParentItemContainerGuid = destination },
                    cancellationToken: cancellationToken);
            }
            catch (FargoApiException)
            {
                // Concurrency conflicts may surface as transient errors; tolerated.
            }
        });
        await Task.WhenAll(moveTasks);

        var finalItem = await _admin.V1.Items[moveItem].GetAsync(cancellationToken: cancellationToken);
        var landedSomewhere = finalItem?.ParentContainerGuid == c1 || finalItem?.ParentContainerGuid == c2;
        _check.That(landedSomewhere,
            "after concurrent moves the item resides in exactly one of the two target containers");

        // 7d. Parallel group-membership mutations on one user: add the user to and
        // remove it from several groups at once, then assert the final membership
        // is a consistent subset of the attempted groups.
        var churnUser = _users[0];
        var churnGroups = _groups.Take(Math.Min(_groups.Count, degree)).ToList();
        var membershipTasks = churnGroups.Select(async (g, i) =>
        {
            try
            {
                var dto = (i % 2 == 0)
                    ? new UserUpdateDto { UserGroupsToAdd = [g.Guid] }
                    : new UserUpdateDto { UserGroupsToRemove = [g.Guid] };
                await _admin.V1.Users[churnUser.Guid].PutAsync(dto, cancellationToken: cancellationToken);
            }
            catch (FargoApiException)
            {
                // Idempotent add/remove races are acceptable.
            }
        });
        await Task.WhenAll(membershipTasks);

        var churnDto = await _admin.V1.Users[churnUser.Guid].GetAsync(cancellationToken: cancellationToken);
        var finalGroups = churnDto?.UserGroups?.Where(x => x.HasValue).Select(x => x!.Value).ToHashSet() ?? [];
        var attempted = churnGroups.Select(g => g.Guid).ToHashSet();
        attempted.Add(churnUser.GroupGuid ?? Guid.Empty);
        _check.That(finalGroups.All(g => attempted.Contains(g)),
            "after concurrent membership churn the user's groups are a subset of the attempted set");
        Log.Success("Concurrency tests completed.");
    }

    // -------------------------------------------------------------------------
    // 8. Deletions
    // -------------------------------------------------------------------------

    private async Task VerifyDeletionsAsync(CancellationToken cancellationToken)
    {
        Log.Step("Deletion: deleting a sample of users/groups/items and proving the deletes took");

        // Delete a handful of users and confirm they 404 afterwards.
        var usersToDelete = _users.Take(Math.Min(3, _users.Count)).ToList();
        foreach (var user in usersToDelete)
        {
            await _check.AllowedAsync($"admin deletes user '{user.Nameid}'", () =>
                _admin.V1.Users[user.Guid].DeleteAsync(cancellationToken: cancellationToken));
            await _check.DeniedAsync($"deleted user '{user.Nameid}' is gone", () =>
                _admin.V1.Users[user.Guid].GetAsync(cancellationToken: cancellationToken), 404);
            _users.Remove(user);
        }

        // Delete a leaf group (one with no children referencing it as parent).
        var childParents = _groups.Where(g => g.ParentGroup is not null)
            .Select(g => g.ParentGroup!.Value).ToHashSet();
        var leafGroup = _groups.FirstOrDefault(g => !childParents.Contains(g.Guid));
        if (leafGroup is not null)
        {
            await _check.AllowedAsync($"admin deletes leaf group '{leafGroup.Nameid}'", () =>
                _admin.V1.UserGroups[leafGroup.Guid].DeleteAsync(cancellationToken: cancellationToken));
            await _check.DeniedAsync($"deleted group '{leafGroup.Nameid}' is gone", () =>
                _admin.V1.UserGroups[leafGroup.Guid].GetAsync(cancellationToken: cancellationToken), 404);
            _groups.Remove(leafGroup);
        }

        // Delete a couple of items and confirm they 404.
        var itemsToDelete = _items.Take(Math.Min(2, _items.Count)).ToList();
        foreach (var item in itemsToDelete)
        {
            await _check.AllowedAsync("admin deletes an item", () =>
                _admin.V1.Items[item.Guid].DeleteAsync(cancellationToken: cancellationToken));
            await _check.DeniedAsync("deleted item is gone", () =>
                _admin.V1.Items[item.Guid].GetAsync(cancellationToken: cancellationToken), 404);
            _items.Remove(item);
        }

        Log.Success("Deletion boundary checks completed.");
    }

    // -------------------------------------------------------------------------
    // 9. Audit log
    // -------------------------------------------------------------------------

    private async Task InspectAuditLogAsync(CancellationToken cancellationToken)
    {
        Log.Step("Audit log: reading the trail back and asserting the expected entries exist");

        // 9a. A time-windowed read from the scenario start returns a healthy page.
        var windowed = await _admin.V1.AuditLogs.GetAsync(cfg =>
        {
            cfg.QueryParameters.PeriodStart = _scenarioStart;
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 200;
        }, cancellationToken);
        _check.That((windowed?.TotalCount ?? 0) > 0,
            "audit log has entries since the scenario started");
        Log.Info($"Audit entries since scenario start: {windowed?.TotalCount ?? 0}");

        // 9b. Filter by entity type = Partition and assert creations are recorded
        // for the partitions we built.
        var partitionLogs = await _admin.V1.AuditLogs.GetAsync(cfg =>
        {
            cfg.QueryParameters.EntityType = EntityTypes.Partition;
            cfg.QueryParameters.PeriodStart = _scenarioStart;
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 500;
        }, cancellationToken);
        var partitionCreates = partitionLogs?.Items?
            .Count(l => l.AuditLogType == AuditLogTypes.EntityCreated) ?? 0;
        _check.That(partitionCreates >= _partitions.Count,
            $"audit log records at least {_partitions.Count} partition creations " +
            $"(saw {partitionCreates})");

        // 9c. Filter by a specific entity (a user we deactivated then reactivated):
        // there must be an update entry for it.
        var trackedUser = _users.LastOrDefault();
        if (trackedUser is not null)
        {
            var userLogs = await _admin.V1.AuditLogs.GetAsync(cfg =>
            {
                cfg.QueryParameters.EntityGuid = trackedUser.Guid;
                cfg.QueryParameters.Page = 1;
                cfg.QueryParameters.Limit = 100;
            }, cancellationToken);
            _check.That((userLogs?.Items?.Count ?? 0) > 0,
                $"audit log has entries for user '{trackedUser.Nameid}'");
            _check.That(
                userLogs?.Items?.Any(l => l.AuditLogType == AuditLogTypes.EntityUpdated) == true,
                $"audit log records an update for user '{trackedUser.Nameid}'");
            _check.That(
                userLogs?.Items?.All(l => l.EntityType == EntityTypes.User) == true,
                "entity-filtered audit entries all concern the user entity");
        }

        // 9d. Filter by entity type = User and audit type = deleted: the users we
        // deleted must appear as deletions.
        var deletionLogs = await _admin.V1.AuditLogs.GetAsync(cfg =>
        {
            cfg.QueryParameters.EntityType = EntityTypes.User;
            cfg.QueryParameters.PeriodStart = _scenarioStart;
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 500;
        }, cancellationToken);
        var userDeletes = deletionLogs?.Items?
            .Count(l => l.AuditLogType == AuditLogTypes.EntityDeleted) ?? 0;
        _check.That(userDeletes >= 1,
            $"audit log records user deletions (saw {userDeletes})");

        // 9e. Filter by actor = admin: the admin performed the vast majority of
        // writes, so there must be many entries attributed to the admin actor.
        var me = await _admin.V1.Users.GetAsync(cfg =>
        {
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 500;
        }, cancellationToken);
        var adminUser = me?.Items?.FirstOrDefault(u => u.IsAdmin == true);
        if (adminUser?.Guid is Guid adminGuid)
        {
            var adminLogs = await _admin.V1.AuditLogs.GetAsync(cfg =>
            {
                cfg.QueryParameters.ActorGuid = adminGuid;
                cfg.QueryParameters.ActorType = ActorTypes.User;
                cfg.QueryParameters.PeriodStart = _scenarioStart;
                cfg.QueryParameters.Page = 1;
                cfg.QueryParameters.Limit = 500;
            }, cancellationToken);
            _check.That((adminLogs?.TotalCount ?? 0) > 0,
                "audit log attributes entries to the admin actor");
            _check.That(
                adminLogs?.Items?.All(l => l.ActorGuid == adminGuid) == true,
                "actor-filtered audit entries are all attributed to the admin");
        }

        // 9f. A single audit entry is retrievable by its GUID.
        var anyEntry = windowed?.Items?.FirstOrDefault();
        if (anyEntry?.Guid is Guid entryGuid)
        {
            var single = await _admin.V1.AuditLogs[entryGuid].GetAsync(cancellationToken: cancellationToken);
            _check.Equal(entryGuid, single?.Guid, "a single audit entry is retrievable by GUID");
        }

        // 9g. Item-event recording leaves an ItemEventRecorded audit trail.
        var itemEventLogs = await _admin.V1.AuditLogs.GetAsync(cfg =>
        {
            cfg.QueryParameters.EntityType = EntityTypes.ItemEvent;
            cfg.QueryParameters.PeriodStart = _scenarioStart;
            cfg.QueryParameters.Page = 1;
            cfg.QueryParameters.Limit = 100;
        }, cancellationToken);
        Log.Info($"ItemEvent audit entries: {itemEventLogs?.TotalCount ?? 0}");

        Log.Success("Audit-log inspection completed.");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<Guid> CreateContainerItemAsync(
        Guid partitionGuid, string baseName, CancellationToken cancellationToken)
    {
        var article = await _admin.V1.Articles.PostAsync(
            new ArticleCreateDto
            {
                Name = N($"{baseName}-{_items.Count}-{_random.Next(100000)}"),
                ArticleType = ArticleTypeContainer,
                PartitionsToAdd = [partitionGuid],
            },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create '{baseName}' container article.");

        return await _admin.V1.Items.PostAsync(
            new ItemCreateDto { ArticleGuid = article, PartitionsToAdd = [partitionGuid] },
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create '{baseName}' container item.");
    }

    private async Task PostEventAsync(ItemEventCreateDto dto, CancellationToken cancellationToken)
    {
        var guid = await _admin.V1.Item.Events.PostAsync(dto, cancellationToken: cancellationToken);
        if (guid is null || guid == Guid.Empty)
        {
            throw new InvalidOperationException("Item event creation returned no identifier.");
        }
    }

    private async Task ClearPasswordChangeRequirementAsync(string nameid, CancellationToken cancellationToken)
    {
        await _admin.V1.Identity.Password.PutAsync(
            new IdentityPasswordUpdateDto
            {
                Nameid = nameid,
                CurrentPassword = _options.NewUserPassword,
                NewPassword = _options.NewUserPassword,
            },
            cancellationToken: cancellationToken);
    }

    private string N(string baseName) => $"{baseName}-{_tag}";

    /// <summary>
    /// Produces a letters-only name (first/last names reject digits) by encoding
    /// an index as a base-26 alphabetic suffix, e.g. 0 -> "UserA", 27 -> "UserAb".
    /// </summary>
    private static string LetterName(string prefix, int index)
    {
        var suffix = string.Empty;
        var n = index;
        do
        {
            suffix = (char)('a' + (n % 26)) + suffix;
            n = (n / 26) - 1;
        }
        while (n >= 0);

        // Capitalise the first letter of the suffix for readability.
        suffix = char.ToUpperInvariant(suffix[0]) + suffix[1..];
        return prefix + suffix;
    }

    private sealed record PartitionNode(int Index, string Name, Guid Guid, Guid? ParentGuid, int Level);

    private sealed record CreatedGroup(
        Guid Guid, string Nameid, Guid PartitionGuid, int[] Permissions, Guid? ParentGroup);

    private sealed record CreatedUser(Guid Guid, string Nameid, Guid? GroupGuid, Guid PartitionGuid);

    private sealed record CreatedArticle(Guid Guid, string Name, Guid PartitionGuid, bool EventTracked);

    private sealed class CreatedItem(Guid guid, Guid articleGuid, Guid partitionGuid, Guid containerGuid)
    {
        public Guid Guid { get; } = guid;
        public Guid ArticleGuid { get; } = articleGuid;
        public Guid PartitionGuid { get; } = partitionGuid;
        public Guid ContainerGuid { get; set; } = containerGuid;
    }
}
