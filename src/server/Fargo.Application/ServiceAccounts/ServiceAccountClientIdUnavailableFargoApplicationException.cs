using Fargo.Application.Common;
using Fargo.Core.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Thrown when creating a service account with a client id already in use.</summary>
public sealed class ServiceAccountClientIdUnavailableFargoApplicationException(string clientId)
    : FargoApplicationException($"The client id '{clientId}' is already in use.", FargoErrorType.InvalidOperation);
