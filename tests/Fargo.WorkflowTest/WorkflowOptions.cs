namespace Fargo.WorkflowTest;

/// <summary>
/// Runtime configuration for the workflow driver.
/// </summary>
/// <remarks>
/// The values are provided through application configuration — the
/// <c>Workflow</c> section of <c>appsettings.json</c> (and its
/// environment-specific overrides) or matching environment variables — and are
/// bound and validated at startup. They are intentionally not hard-coded.
/// </remarks>
public sealed class WorkflowOptions
{
    public const string SectionName = "Workflow";

    public required Uri Server { get; init; }

    /// <summary>
    /// Base address of the running real-time service. When empty, the
    /// real-time scenario is skipped.
    /// </summary>
    public Uri? RealtimeServer { get; init; }

    /// <summary>
    /// When true and <see cref="RealtimeServer"/> is set, the workflow runs the
    /// real-time scenario: it connects users to the hub and verifies that
    /// article changes are pushed only to users with access to the article's
    /// partitions.
    /// </summary>
    public bool RunRealtimeScenario { get; init; } = true;

    public required string Username { get; init; }

    public required string Password { get; init; }

    /// <summary>Number of container items to create (e.g. shelves/boxes).</summary>
    public int Containers { get; init; }

    public int Articles { get; init; }

    public int ItemsPerArticle { get; init; }

    /// <summary>Number of random relocations to perform after the initial placement.</summary>
    public int Moves { get; init; }

    /// <summary>Seed for the random move generator, so runs are reproducible.</summary>
    public int Seed { get; init; }

    /// <summary>
    /// Password assigned to every user the workflow creates. Must satisfy the
    /// server's password policy. The workflow logs in as these users after
    /// clearing their initial password-change requirement.
    /// </summary>
    public string NewUserPassword { get; init; } = "Workflow123@";

    /// <summary>
    /// A short unique suffix appended to generated names (users, groups,
    /// partitions) so repeated runs against the same database do not collide on
    /// unique identifiers. When empty, a timestamp-based suffix is generated.
    /// </summary>
    public string RunTag { get; init; } = string.Empty;

    /// <summary>
    /// When true, the workflow runs the identity and authorization scenarios
    /// (partitions, user groups, users, permission and partition-access checks)
    /// in addition to the inventory scenario.
    /// </summary>
    public bool RunIdentityScenarios { get; init; } = true;

    /// <summary>
    /// When true, the workflow runs the inventory calculation scenario (article
    /// inventory, container inventory, point-in-time reconstruction, and the
    /// item-events path) and verifies the computed numbers.
    /// </summary>
    public bool RunInventoryScenarios { get; init; } = true;

    /// <summary>
    /// When true, the workflow runs the article-property coverage scenario,
    /// creating articles with every optional property set and across every
    /// article type, and verifying each value round-trips.
    /// </summary>
    public bool RunArticlePropertyScenarios { get; init; } = true;

    /// <summary>
    /// When true, the workflow runs the event-tracked item-events scenario:
    /// creating an event-tracked article, recording creation/movement/deletion
    /// events, and verifying the computed event-based inventory.
    /// </summary>
    public bool RunItemEventScenarios { get; init; } = true;

    /// <summary>
    /// When true, the workflow runs the large-tree stress scenario: it builds a
    /// deep and wide partition hierarchy, creates many user groups (including a
    /// nested group hierarchy), many users and many articles/items spread across
    /// the tree, records item events, exercises partition-scoped access and
    /// deletion boundaries, runs concurrency tests, and inspects the audit log.
    /// </summary>
    public bool RunLargeTreeScenario { get; init; } = true;

    /// <summary>
    /// Fan-out (number of child partitions created per non-leaf partition) for
    /// the large-tree scenario. Combined with <see cref="PartitionTreeDepth"/>
    /// this drives the total number of partitions created.
    /// </summary>
    public int PartitionTreeFanout { get; init; } = 3;

    public int PartitionTreeDepth { get; init; } = 3;

    public int LargeTreeUserGroups { get; init; } = 12;

    public int LargeTreeUsers { get; init; } = 20;

    public int LargeTreeArticles { get; init; } = 15;

    public int LargeTreeItemsPerArticle { get; init; } = 6;

    /// <summary>
    /// Degree of parallelism used by the concurrency tests in the large-tree
    /// scenario (e.g. simultaneous article/item creations and item moves).
    /// </summary>
    public int ConcurrencyDegree { get; init; } = 16;
}
