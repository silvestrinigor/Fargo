namespace Fargo.Sdk.Contracts.ApiClients;

/// <summary>Represents an API client create request.</summary>
public sealed record ApiClientCreateRequest(
    string Name,
    string? Description = null);

/// <summary>Represents an API client update request.</summary>
public sealed record ApiClientUpdateRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);
