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
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "Workflow";

    /// <summary>Base address of the running Fargo HTTP API.</summary>
    public required Uri Server { get; init; }

    /// <summary>Username used to authenticate against the API.</summary>
    public required string Username { get; init; }

    /// <summary>Password used to authenticate against the API.</summary>
    public required string Password { get; init; }

    /// <summary>Number of container items to create (e.g. shelves/boxes).</summary>
    public int Containers { get; init; }

    /// <summary>Number of distinct product articles to create.</summary>
    public int Articles { get; init; }

    /// <summary>Number of concrete items to create per product article.</summary>
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
}
