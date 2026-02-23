using Fargo.Domain.Entities;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Services
{
    public class PermissionService
    {
        public static bool HasPermission(User user, ActionType action)
        {
            return false;
        }
    }
}