using Fargo.ClientHttp;

namespace Fargo.SimulationWorker;

/// <summary>
/// The shared setup produced by <see cref="WorldBootstrapper"/>: the partition
/// tree and the operator accounts the simulation will drive.
/// </summary>
internal sealed record SimulationWorld(
    IReadOnlyList<Guid> Partitions,
    IReadOnlyList<OperatorAccount> Operators);

internal sealed record OperatorAccount(
    int Index,
    string Nameid,
    OperatorRole Role,
    Guid PartitionGuid);

internal enum OperatorRole
{
    /// <summary>Creates product articles and concrete items (receiving goods).</summary>
    Receiver,

    /// <summary>Places and relocates items among containers (put-away / internal moves).</summary>
    PutAway,

    /// <summary>Moves, removes and ships/deletes items (order picking).</summary>
    Picker,

    /// <summary>Drives the event-tracked bulk-goods flow (receipt/transfer/dispatch).</summary>
    BulkHandler,

    /// <summary>Reads inventory, movements, item events and audit logs (read-only).</summary>
    Auditor,
}

/// <summary>
/// The mutable, per-operator view of the slice of the world it has created and
/// manipulated. Operators only act on their own known entities, which keeps each
/// loop self-contained and lets consistency checks compare intended vs server
/// state.
/// </summary>
internal sealed class OperatorContext(
    OperatorAccount account,
    FargoApiClient client,
    Random random)
{
    public OperatorAccount Account { get; } = account;

    public FargoApiClient Client { get; } = client;

    public Random Random { get; } = random;

    public string Who => $"{Account.Role}#{Account.Index:D2}";

    public List<Guid> Containers { get; } = [];

    public List<Guid> ProductArticles { get; } = [];

    /// <summary>Concrete items the operator has created, with their last-known container.</summary>
    public List<TrackedItem> Items { get; } = [];

    public List<Guid> EventArticles { get; } = [];

    public T? PickRandom<T>(IReadOnlyList<T> source) where T : class =>
        source.Count == 0 ? null : source[Random.Next(source.Count)];

    public Guid? PickRandomGuid(IReadOnlyList<Guid> source) =>
        source.Count == 0 ? null : source[Random.Next(source.Count)];
}

internal sealed class TrackedItem(Guid guid, Guid articleGuid)
{
    public Guid Guid { get; } = guid;

    public Guid ArticleGuid { get; } = articleGuid;

    /// <summary>The container the operator last placed this item in (null = loose/shipped).</summary>
    public Guid? ContainerGuid { get; set; }

    public bool Deleted { get; set; }
}
