namespace Fargo.Application.ApiClients;

/// <summary>Wire shape for updating an API client.</summary>
public sealed record ApiClientUpdateRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);
