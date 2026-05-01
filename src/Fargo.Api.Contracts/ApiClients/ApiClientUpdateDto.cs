namespace Fargo.Api.Contracts.ApiClients;

/// <summary>Represents an API client update request.</summary>
public sealed record ApiClientUpdateDto(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);
