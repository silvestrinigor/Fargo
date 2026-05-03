namespace Fargo.Sdk.Contracts.ApiClients;

/// <summary>Represents an API client create request.</summary>
public sealed record ApiClientCreateDto(
    string Name,
    string? Description = null);
