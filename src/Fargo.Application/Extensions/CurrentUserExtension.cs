using Fargo.Application.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Extensions
{
    public static class CurrentUserExtension
    {
        extension(ICurrentUser currentUser)
        {
            public Actor ToActor()
                => new()
                {
                    UserGuid = currentUser.UserGuid,
                    UserPermissions = new HashSet<Permission>(),
                    PartitionGuids = currentUser.PartitionGuids
                };
        }
    }
}