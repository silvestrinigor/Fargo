namespace Fargo.Sdk.Contracts.ApiClients;

/// <summary>Represents an API client returned by the API.</summary>
public sealed record ApiClientDto(
    Guid Guid,
    string Name,
    string Description,
    bool IsActive,
    Guid? EditedByGuid = null);
