using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using System.Linq.Expressions;

namespace Fargo.Application.Mappings;

public static class UserGroupMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupInformation>> InformationProjection =
        u => new UserGroupInformation(
            u.Guid,
            u.Nameid,
            u.Description,
            u.IsActive,
            u.Permissions.Select(p => new Permission(p.Guid, p.Action)).ToList()
        );

    public static UserGroupInformation ToInformation(this UserGroup u) =>
        new(
            u.Guid,
            u.Nameid,
            u.Description,
            u.IsActive,
            [.. u.Permissions.Select(p => new Permission(p.Guid, p.Action))]
        );
}
