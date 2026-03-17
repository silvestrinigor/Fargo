using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using System.Linq.Expressions;

namespace Fargo.Application.Mappings;

public static class UserMappings
{
    public static readonly Expression<Func<User, UserInformation>> InformationProjection =
        u => new UserInformation(
            u.Guid,
            u.Nameid,
            u.FirstName,
            u.LastName,
            u.Description,
            u.DefaultPasswordExpirationPeriod,
            u.RequirePasswordChangeAt,
            u.IsActive,
            u.Permissions.Select(p => new Permission(p.Guid, p.Action)).ToList(),
            u.PartitionAccesses.Select(p => p.Guid).ToList()
        );

    public static UserInformation ToInformation(this User u) =>
        new(
            u.Guid,
            u.Nameid,
            u.FirstName,
            u.LastName,
            u.Description,
            u.DefaultPasswordExpirationPeriod,
            u.RequirePasswordChangeAt,
            u.IsActive,
            [.. u.Permissions.Select(p => new Permission(p.Guid, p.Action))],
            [.. u.PartitionAccesses.Select(p => p.Guid)]
        );
}
