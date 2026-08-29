using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Http.Client.Authentication;

public sealed class FargoAuthenticationProvider : BaseBearerTokenAuthenticationProvider
{
    public FargoAuthenticationProvider(
        Func<CancellationToken, Task<string>> accessTokenProvider)
        : base(new FargoAccessTokenProvider(accessTokenProvider))
    {
    }
}
