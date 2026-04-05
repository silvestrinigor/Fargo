using Fargo.Sdk.Authentication;

namespace Fargo.Sdk;

public interface IEngine
{
    IAuthenticationManager AuthenticationManager { get; }
}
