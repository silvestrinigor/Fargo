using Fargo.Core;
using Fargo.Core.Identity;

namespace Fargo.Application.Tests;

internal static class DomainTestActors
{
    public static Actor CreateDomainActor(params ActionType[] permissions)
        => new(
            Guid.NewGuid(),
            isAdmin: permissions.Length == 0,
            isActive: true,
            permissions,
            []);

    public static Actor CreateDomainActorWithoutPermissions()
        => new(
            Guid.NewGuid(),
            isAdmin: false,
            isActive: true,
            [],
            []);
}
