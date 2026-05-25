namespace Fargo.HttpClient;

public sealed record FargoListQuery(
    DateTimeOffset? TemporalAsOf = null,
    int? Page = null,
    int? Limit = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
);
