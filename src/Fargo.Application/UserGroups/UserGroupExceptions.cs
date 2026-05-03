namespace Fargo.Application.UserGroups;

public sealed class UserGroupNotFoundFargoApplicationException(Guid userGroupGuid)
    : FargoApplicationException($"User group '{userGroupGuid}' was not found.")
{
    public Guid UserGroupGuid { get; } = userGroupGuid;
}
