using Fargo.Domain.Users;

namespace Fargo.Application.Authentication;

/// <summary>
/// Defines a service responsible for generating access tokens
/// for authenticated users.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user for whom the token will be generated.
    /// </param>
    /// <returns>
    /// A <see cref="TokenGenerateResult"/> containing the generated
    /// access token and its expiration information.
    /// </returns>
    TokenGenerateResult Generate(User user);
}
