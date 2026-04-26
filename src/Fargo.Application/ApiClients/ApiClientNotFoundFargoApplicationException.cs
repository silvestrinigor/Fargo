namespace Fargo.Application.ApiClients;

/// <summary>Exception thrown when a requested API client does not exist.</summary>
public sealed class ApiClientNotFoundFargoApplicationException(Guid guid)
    : FargoApplicationException($"API client '{guid}' was not found.");
