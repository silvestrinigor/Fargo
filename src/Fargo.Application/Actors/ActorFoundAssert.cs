using Fargo.Core.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Actors;

public static class ActorAssertFound
{
    public static void ThrowNotAuthorizedIfNull([NotNull] Actor? actor)
    {
        if (actor is null)
        {
            throw new NotImplementedException();
        }
    }
}
