namespace Fargo.Api.ApiClients;

/// <summary>Represents an API client returned by the API.</summary>
public sealed record ApiClientResult(
    Guid Guid,
    string Name,
    string Description,
    bool IsActive,
    Guid? EditedByGuid = null
);
