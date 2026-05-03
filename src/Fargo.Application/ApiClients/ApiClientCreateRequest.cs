namespace Fargo.Application.ApiClients;

/// <summary>Wire shape for creating an API client.</summary>
public sealed record ApiClientCreateRequest(string Name, string? Description = null);
