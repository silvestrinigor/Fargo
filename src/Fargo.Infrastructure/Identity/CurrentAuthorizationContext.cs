using Fargo.Application.Identity;

namespace Fargo.Infrastructure.Security;

public sealed class CurrentAuthorizationContext(
    ICurrentUser currentUser,
    IAuthorizationContextFactory authorizationContextFactory
) : ICurrentAuthorizationContext
{
    private Task<IAuthorizationContext>? cached;

    public Task<IAuthorizationContext> GetAsync(CancellationToken cancellationToken = default)
        => cached ??= authorizationContextFactory.CreateFromUserGuid(
            currentUser.UserGuid,
            cancellationToken);
}
