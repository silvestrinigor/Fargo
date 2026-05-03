namespace Fargo.Sdk.Contracts.ApiClients;

/// <summary>Represents an API client update request.</summary>
public sealed record ApiClientUpdateDto(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);
