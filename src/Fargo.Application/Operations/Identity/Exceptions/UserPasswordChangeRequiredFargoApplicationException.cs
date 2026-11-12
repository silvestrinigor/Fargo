using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;

namespace Fargo.Application.Operations.Identity;

public sealed class UserPasswordChangeRequiredFargoApplicationException(Guid userGuid)
    : FargoApplicationException($"User '{userGuid}' must change their password before continuing.", FargoErrorType.NotAuthorized)
{
    public Guid UserGuid { get; } = userGuid;
}
