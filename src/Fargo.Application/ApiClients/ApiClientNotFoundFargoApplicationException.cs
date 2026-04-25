namespace Fargo.Application.ApiClients;

public sealed class ApiClientNotFoundFargoApplicationException(Guid guid)
    : FargoApplicationException($"API client '{guid}' was not found.");
