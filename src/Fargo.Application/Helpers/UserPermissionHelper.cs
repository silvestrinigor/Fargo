using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Services;

namespace Fargo.Application.Helpers;

public static class UserPermissionHelper
{
    public static void ValidatePermission(User user, ActionType action)
    {
        if (!UserService.HasPermission(user, action))
        {
            throw new UserNotAuthorizedFargoApplicationException(user.Guid, action);
        }
    }
}
