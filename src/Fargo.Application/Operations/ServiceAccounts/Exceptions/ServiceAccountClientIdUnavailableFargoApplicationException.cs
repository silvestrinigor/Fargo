using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;

namespace Fargo.Application.Operations.ServiceAccounts;

public sealed class ServiceAccountClientIdUnavailableFargoApplicationException(string clientId)
    : FargoApplicationException($"The client id '{clientId}' is already in use.", FargoErrorType.Conflict);
