using Fargo.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public static class EntityAssertFound
{
    public static void ThrowNotFoundIfNull([NotNull] Entity? entity)
    {
        if (entity is null)
        {
            throw new NotImplementedException();
        }
    }
}
